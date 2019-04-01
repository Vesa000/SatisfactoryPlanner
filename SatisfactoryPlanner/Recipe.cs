using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;



namespace SatisfactoryPlanner
{
    public struct RecipeItem
    {
        public Item item;
        public float count;

        public RecipeItem(Item _item, int _count)
        {
            item = _item;
            count = _count;
        }
    }

    public class Recipe
    {
        public String name;
        public List<RecipeItem> inputs = new List<RecipeItem>();
        public List<RecipeItem> Outputs = new List<RecipeItem>();
        public float CraftTime;
        public Device device;
    }
}
