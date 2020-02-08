using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RecipesApi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace RecipesApi
{
    public static class RecipesFunctions
    {
        private static readonly string RepoPath = @"d:\home\data\recipes";
        private static readonly string CategoriesFilename = Path.Combine(RepoPath, "_data", "categories.yml");
        private static readonly Deserializer YamlDeserializer = new Deserializer();
        private static readonly Serializer YamlSerializer = new Serializer();

        [FunctionName("GetCategories")]
        public static Task<IActionResult> GetCategories(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "categories")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Getting categories...");
            Git.CloneRepo(log, RepoPath);

            log.LogInformation("Reading yml file...");
            var categoriesYml = File.ReadAllText(CategoriesFilename);

            return Task.FromResult<IActionResult>(new OkObjectResult(YamlDeserializer.Deserialize<List<Category>>(categoriesYml)));
        }

        [FunctionName("SaveCategories")]
        public static async Task<IActionResult> SaveCategories(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "categories")] HttpRequest req,
            ClaimsPrincipal principal,
            ILogger log)
        {
            var (name, email) = principal.GetNameAndEmail();
            Git.CloneRepo(log, RepoPath);

            var categories = JsonConvert.DeserializeObject<List<Category>>(await req.ReadAsStringAsync());
            foreach (var category in categories)
            {
                var expectedDir = Path.Combine(RepoPath, category.name);
                //add new categories
                if (!Directory.Exists(expectedDir))
                {
                    Directory.CreateDirectory(expectedDir);
                    File.WriteAllText(Path.Combine(expectedDir, "index.html"), "---\nlayout: category\n---");
                }

                //cleanup old categories (this is all pure hack)
                foreach (var dir in Directory.EnumerateDirectories(RepoPath).Where(d => categories.All(c => !d.EndsWith(c.name, StringComparison.InvariantCultureIgnoreCase))))
                {
                    var indexFile = Path.Combine(dir, "index.html");
                    if (!File.Exists(indexFile))
                        continue;

                    var data = File.ReadAllLines(indexFile);
                    if (data != null && data.Length == 3 && data[1].Equals("layout: category"))
                        Directory.Delete(dir, true);
                }
            }

            //save yml
            var updatedYaml = YamlSerializer.Serialize(categories);
            File.WriteAllText(CategoriesFilename, updatedYaml);
            Git.CommitAllChanges(RepoPath, "Update categories.yml", name, email);

            return new OkResult();
        }

        [FunctionName("GetRecipes")]
        public static Task<IActionResult> GetRecipes(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "categories/{categorySlug}/recipes")] HttpRequest req,
            string categorySlug,
            ILogger log)
        {
            Git.CloneRepo(log, RepoPath);

            var recipeFilesForCategory = Directory.EnumerateFiles(Path.Combine(RepoPath, categorySlug)).Where(fn => !fn.Contains("index.html"));

            var responseJson = new List<object>();
            foreach (var recipeFile in recipeFilesForCategory)
            {
                var yaml = string.Join(Environment.NewLine, File.ReadAllLines(recipeFile).Skip(1).TakeWhile(l => !l.Equals("---")));
                var frontMatter = YamlDeserializer.Deserialize<Dictionary<string, object>>(yaml);
                responseJson.Add(frontMatter["recipe"]);
            }
            return Task.FromResult<IActionResult>(new OkObjectResult(responseJson));
        }

        [FunctionName("SaveRecipes")]
        public static async Task<IActionResult> SaveRecipes(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "categories/{categorySlug}/recipes")] HttpRequest req,
            ClaimsPrincipal principal,
            string categorySlug,
            ILogger log)
        {
            if (string.IsNullOrWhiteSpace(categorySlug))
                return new BadRequestResult();

            var (name, email) = principal.GetNameAndEmail();
            Git.CloneRepo(log, RepoPath);

            var categoryDirectory = Path.Combine(RepoPath, categorySlug);
            foreach (var file in Directory.EnumerateFiles(categoryDirectory).Where(f => !f.EndsWith("index.html")))
                File.Delete(file);

            var recipes = JsonConvert.DeserializeObject<List<Recipe>>(await req.ReadAsStringAsync());
            foreach (var recipe in recipes)
            {
                var result = YamlSerializer.Serialize(new { layout = "recipe", recipe });
                File.WriteAllText(Path.Combine(categoryDirectory, GenerateSlug(recipe.name) + ".html"), $"---\n{result}\n---");
            }
            Git.CommitAllChanges(RepoPath, $"Updating {categorySlug} recipes", name, email);
            return new OkResult();
        }

        private static (string, string) GetNameAndEmail(this ClaimsPrincipal principal) =>
        (
            principal.Claims?.FirstOrDefault(c => c.Type.Equals("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress"))?.Value,
            principal.Claims?.FirstOrDefault(c => c.Type.Equals("name"))?.Value
        );

        private static string GenerateSlug(this string phrase)
        {
            phrase = (phrase ?? "").ToLower();
            string str = Regex.Replace(phrase, @"[^a-z0-9\s-]", "");
            str = Regex.Replace(str, @"\s+", " ").Trim();
            str = str.Substring(0, str.Length <= 45 ? str.Length : 45).Trim();
            str = Regex.Replace(str, @"\s", "-");
            return str;
        }
    }
}
