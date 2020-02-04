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
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GetRecipes
{
    public static class Recipes
    {
        [FunctionName("GetCategories")]
        public static Task<IActionResult> GetCategories(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            var path = @"C:\Development\recipes";
            var c = Path.Combine(path, "_data", "categories.yml");
            var categories = File.ReadAllText(c);

            var deserializer = new YamlDotNet.Serialization.Deserializer();
            return Task.FromResult<IActionResult>(new OkObjectResult(deserializer.Deserialize<List<Category>>(categories)));
        }

        [FunctionName("SaveCategories")]
        public static async Task<IActionResult> SaveCategories(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            var rootDirectory = @"C:\Development\recipes";
            var categoriesFilename = Path.Combine(rootDirectory, "_data", "categories.yml");

            var categories = JsonConvert.DeserializeObject<List<Category>>(await req.ReadAsStringAsync());
            foreach (var category in categories)
            {
                var expectedDir = Path.Combine(rootDirectory, category.name);
                //add new categories
                if (!Directory.Exists(expectedDir))
                {
                    Directory.CreateDirectory(expectedDir);
                    File.WriteAllText(Path.Combine(expectedDir, "index.html"), "---\nlayout: category\n---");
                }

                //cleanup old categories (this is all pure hack)
                foreach (var dir in Directory.EnumerateDirectories(rootDirectory).Where(d => categories.All(c => !d.EndsWith(c.name, StringComparison.InvariantCultureIgnoreCase))))
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
            File.WriteAllText(categoriesFilename, result);

            commitallchanges();

            return new OkResult();
        }

        [FunctionName("GetRecipes")]
        public static Task<IActionResult> GetRecipes(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "{category}/recipes")] HttpRequest req,
            string category,
            ILogger log)
        {
            var rootDirectory = @"C:\Development\recipes";

            var files = Directory.EnumerateFiles(Path.Combine(rootDirectory, category)).Where(fn => !fn.Contains("index.html"));

            var deserializer = new YamlDotNet.Serialization.Deserializer();
            var data = new List<object>();
            foreach (var file in files)
            {
                var yaml = string.Join(Environment.NewLine, File.ReadAllLines(file).Skip(1).TakeWhile(l => !l.Equals("---")));
                var frontMatterKeys = deserializer.Deserialize<Dictionary<string, object>>(yaml);
                data.Add(frontMatterKeys["recipe"]);
            }
            return Task.FromResult<IActionResult>(new OkObjectResult(data));
        }

        [FunctionName("SaveRecipes")]
        public static async Task<IActionResult> SaveRecipes(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "{category}/recipes")] HttpRequest req,
            string category,
            ILogger log)
        {
            var rootDirectory = @"C:\Development\recipes";

            var dir = Path.Combine(rootDirectory, category);
            foreach (var file in Directory.EnumerateFiles(dir).Where(f => !f.EndsWith("index.html")))
                File.Delete(file);

            var recipes = JsonConvert.DeserializeObject<List<Recipe>>(await req.ReadAsStringAsync());
            foreach (var recipe in recipes)
            {
                var serializer = new YamlDotNet.Serialization.Serializer();
                var result = serializer.Serialize(new { layout = "recipe", recipe = recipe });
                File.WriteAllText(Path.Combine(dir, GenerateSlug(recipe.name) + ".html"), $"---\n{result}\n---");
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

        private static readonly string RepoPath = @"C:\Development\recipes";

        static void commitallchanges()
        {
            using (var repo = new Repository(RepoPath))
            {
                Commands.Stage(repo, "*");
                Signature author = new Signature("Test", "@testsig", DateTime.Now);
                Signature committer = author;

                // Commit to the repository
                Commit commit = repo.Commit("Here's a commit i made!", author, committer);
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
                
                repo.Network.Push(repo.Branches["master"], options);

            }

        }

        static void Clone()
        {
            if (!Directory.Exists(RepoPath))
                Repository.Clone("https://github.com/thenickfish/recipes.git", RepoPath);
        }
    }
}
