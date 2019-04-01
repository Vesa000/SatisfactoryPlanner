using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SatisfactoryPlanner
{
    /// <summary>
    /// Interaction logic for ListItem.xaml
    /// </summary>
    public partial class ListItem : UserControl
    {
        public Recipe recipe;
        public ListItem(Recipe _recipe)
        {
            InitializeComponent();

            recipe = _recipe;
            image.Source = new BitmapImage(new Uri(recipe.Outputs[0].item.ImageSource, UriKind.Relative));
            textblock.Text = recipe.name;
        }
    }
}
