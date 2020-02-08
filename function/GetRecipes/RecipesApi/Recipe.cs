using System.Collections.Generic;

namespace RecipesApi
{
    class Recipe
    {
        public string name { get; set; }
        public string blurb { get; set; }
        public string prep_time { get; set; }
        public string cook_time { get; set; }
        public string total_time { get; set; }
        public string makes { get; set; }
        public List<string> ingredients { get; set; }
        public List<string> steps { get; set; }
    }
}
