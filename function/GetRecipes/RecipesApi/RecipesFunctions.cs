using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RecipesApi
{
    public static class RecipesFunctions
    {
        private static readonly string RepoPath = @"d:\home\data\recipes";
        private static readonly string CategoriesFilename = Path.Combine(RepoPath, "_data", "categories.yml");

        [FunctionName("GetCategories")]
        public static Task<IActionResult> GetCategories(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ClaimsPrincipal principal,
            ILogger log)
        {
            log.LogInformation("Getting categories...");
            CloneRepo(log);

            log.LogInformation("Reading yml file...");
            var categoriesYml = File.ReadAllText(CategoriesFilename);

            var ymlDeserializer = new YamlDotNet.Serialization.Deserializer();
            return Task.FromResult<IActionResult>(new OkObjectResult(ymlDeserializer.Deserialize<List<Category>>(categoriesYml)));
        }

        [FunctionName("SaveCategories")]
        public static async Task<IActionResult> SaveCategories(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ClaimsPrincipal principal,
            ILogger log)
        {
            CloneRepo(log);

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
            var serializer = new YamlDotNet.Serialization.Serializer();
            var result = serializer.Serialize(categories);
            File.WriteAllText(CategoriesFilename, result);
            CommitAllChanges();

            return new OkResult();
        }

        [FunctionName("GetRecipes")]
        public static Task<IActionResult> GetRecipes(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "{categorySlug}/recipes")] HttpRequest req,
            ClaimsPrincipal principal,
            string categorySlug,
            ILogger log)
        {
            CloneRepo(log);

            var recipeFilesForCategory = Directory.EnumerateFiles(Path.Combine(RepoPath, categorySlug)).Where(fn => !fn.Contains("index.html"));

            var ymlDeserializer = new YamlDotNet.Serialization.Deserializer();
            var responseJson = new List<object>();
            foreach (var recipeFile in recipeFilesForCategory)
            {
                var yaml = string.Join(Environment.NewLine, File.ReadAllLines(recipeFile).Skip(1).TakeWhile(l => !l.Equals("---")));
                var frontMatter = ymlDeserializer.Deserialize<Dictionary<string, object>>(yaml);
                responseJson.Add(frontMatter["recipe"]);
            }
            return Task.FromResult<IActionResult>(new OkObjectResult(responseJson));
        }

        [FunctionName("SaveRecipes")]
        public static async Task<IActionResult> SaveRecipes(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "{categorySlug}/recipes")] HttpRequest req,
            ClaimsPrincipal principal,
            string categorySlug,
            ILogger log)
        {
            CloneRepo(log);

            var categoryDirectory = Path.Combine(RepoPath, categorySlug);
            foreach (var file in Directory.EnumerateFiles(categoryDirectory).Where(f => !f.EndsWith("index.html")))
                File.Delete(file);

            var recipes = JsonConvert.DeserializeObject<List<Recipe>>(await req.ReadAsStringAsync());
            foreach (var recipe in recipes)
            {
                var serializer = new YamlDotNet.Serialization.Serializer();
                var result = serializer.Serialize(new { layout = "recipe", recipe });
                File.WriteAllText(Path.Combine(categoryDirectory, GenerateSlug(recipe.name) + ".html"), $"---\n{result}\n---");
            }
            return new OkResult();
        }

        public static string GenerateSlug(this string phrase)
        {
            phrase = (phrase ?? "").ToLower();
            string str = Regex.Replace(phrase, @"[^a-z0-9\s-]", "");
            str = Regex.Replace(str, @"\s+", " ").Trim();
            str = str.Substring(0, str.Length <= 45 ? str.Length : 45).Trim();
            str = Regex.Replace(str, @"\s", "-");
            return str;
        }

        static void CommitAllChanges()
        {
            using var gitRepo = new Repository(RepoPath);
            Commands.Stage(gitRepo, "*");
            Signature author = new Signature("Test", "@testsig", DateTime.Now);
            Signature committer = author;

            // Commit to the repository
            Commit commit = gitRepo.Commit("Here's a commit i made!", author, committer);
            PushOptions options = new PushOptions
            {
                CredentialsProvider = new CredentialsHandler(
                (_, __, ___) =>
                    new UsernamePasswordCredentials()
                    {
                        Username = Environment.GetEnvironmentVariable("git_user"),
                        Password = Environment.GetEnvironmentVariable("git_token")
                    })
            };

            gitRepo.Network.Push(gitRepo.Branches["master"], options);

        }

        static void CloneRepo(ILogger logger)
        {
            if (Directory.Exists(RepoPath))
                return;

            logger.LogInformation("Cloning repository...");
            Repository.Clone("https://github.com/thenickfish/recipes.git", RepoPath);
        }
    }
}
