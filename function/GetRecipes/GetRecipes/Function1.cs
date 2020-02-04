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
using System.Threading.Tasks;

namespace GetRecipes
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            var path = @"C:\Development\recipes";
            var c = Path.Combine(path, "_data", "categories.yml");
            var categories = File.ReadAllText(c);

            var deserializer = new YamlDotNet.Serialization.Deserializer();
            var dict = deserializer.Deserialize<List<Category>>(categories);
            //Console.WriteLine(dict["hello"]);

            foreach (var file in Directory.EnumerateDirectories(path))
            {

            }

            log.LogInformation("C# HTTP trigger function processed a request.");

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

            return name != null
                ? (ActionResult)new OkObjectResult($"Hello, {name}")
                : new BadRequestObjectResult("Please pass a name on the query string or in the request body");
        }

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
                if (!Directory.Exists(expectedDir))
                {
                    Directory.CreateDirectory(expectedDir);
                    File.WriteAllText(Path.Combine(expectedDir, "index.html"), "---\nlayout: category\n---");
                }
                foreach (var dir in Directory.EnumerateDirectories(rootDirectory).Where(d => categories.All(c => !c.name.EndsWith(d, StringComparison.InvariantCultureIgnoreCase))))
                {
                    var indexFile = Path.Combine(dir, "index.html");
                    if (File.Exists(indexFile))
                        Directory.Delete(dir, true);
                }
            }

            //save yml

            var serializer = new YamlDotNet.Serialization.Serializer();
            var result = serializer.Serialize(categories);
            File.WriteAllText(categoriesFilename, result);

            // git add -A
            // git commit -m "whatever"
            // git push

            return new OkResult();
        }

        private class Category
        {
            public string name { get; set; }
            public string blurb { get; set; }
        }
    }
}
