using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;

namespace SatisfactoryPlanner
{
    class RecipeHandler
    {
        public List<Recipe> recipes = new List<Recipe>();
        public RecipeHandler()
        {
            using (StreamReader sr = new StreamReader("Recipes.json"))
            {
                string json = sr.ReadToEnd();
                List<RecipeJson> recipeJsons = JsonConvert.DeserializeObject<List<RecipeJson>>(json);
                foreach (RecipeJson r in recipeJsons)
                {
                    Recipe re = new Recipe();
                    foreach (In i in r.In)
                    {
                        RecipeItem ri = new RecipeItem();
                        Item item = new Item();
                        item.Name = i.Item;
                        item.ImageSource = $"Content/Images/{i.Item}.png";
                        ri.item = item;
                        ri.count = i.Pieces;
                        re.inputs.Add(ri);
                    }
                    foreach (Out o in r.Out)
                    {
                        RecipeItem ri = new RecipeItem();
                        Item item = new Item();
                        item.Name = o.Item;
                        item.ImageSource = $"Content/Images/{o.Item}.png";
                        ri.item = item;
                        ri.count = o.Pieces;
                        re.Outputs.Add(ri);
                    }
                    re.CraftTime = r.CraftTime;
                    re.name = r.Name;
                    Device d = new Device();
                    d.name = r.Device;
                    re.device = d;
                    recipes.Add(re);
                }
            }
        }

        public List<Recipe> GetRecipesProducing(String item)
        {
            List<Recipe> list = new List<Recipe>();
            foreach (Recipe r in recipes)
            {
                foreach (RecipeItem ri in r.Outputs)
                {
                    if (ri.item.Name == item)
                    {
                        list.Add(r);
                    }
                }
            }
            return list;
        }

        public List<Recipe> GetRecipesUsing(String item)
        {
            List<Recipe> list = new List<Recipe>();
            foreach (Recipe r in recipes)
            {
                foreach (RecipeItem ri in r.inputs)
                {
                    if (ri.item.Name == item)
                    {
                        list.Add(r);
                    }
                }
            }
            return list;
        }

        public List<Recipe> GetItems(String str)
        {
            List<Recipe> list = new List<Recipe>();
            foreach (Recipe r in recipes)
            {
                foreach (RecipeItem ri in r.Outputs)
                {
                    if (ri.item.Name.ToLower().Contains(str.ToLower()))
                    {
                        list.Add(r);
                    }
                }
            }
            return list;
        }
    }
}
public class In
{
    public string Item { get; set; }
    public int Pieces { get; set; }
}

public class Out
{
    public string Item { get; set; }
    public int Pieces { get; set; }
}

public class RecipeJson
{
    public string Name { get; set; }
    public List<In> In { get; set; }
    public List<Out> Out { get; set; }
    public string Device { get; set; }
    public int CraftTime { get; set; }
}