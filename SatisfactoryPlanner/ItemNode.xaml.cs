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
using SatisfactoryPlanner;

namespace SatisfactoryPlanner
{
    public struct Connection
    {
        public ItemNode from;
        public ItemNode to;
        public Line line;
        public Connection(ItemNode _from, ItemNode _to, Line _line,Canvas canvas)
        {
            from = _from;
            to = _to;
            line = _line;

            line.Stroke = Brushes.Black;
            Canvas.SetZIndex(line, -1);
            Point p = from.TranslatePoint(new Point(0, 0), canvas);
            line.X1 = p.X + 25;
            line.Y1 = p.Y + 25;
            Point pp = to.TranslatePoint(new Point(0, 0), canvas);
            line.X2 = pp.X+25;
            line.Y2 = pp.Y+25;
            canvas.Children.Add(line);
        }
    }
    public partial class ItemNode : UserControl
    {
        public RecipeItem recipeItem;
        public Boolean input;
        public ItemNode(RecipeItem _recipeItem, Boolean _input)
        {
            InitializeComponent();
            recipeItem = _recipeItem;
            input = _input;
            image.Source = new BitmapImage(new Uri(recipeItem.item.ImageSource, UriKind.Relative));
            speedText.Text = recipeItem.count.ToString();
            ResourceNameTooltipText.Text = recipeItem.item.Name;
        }
        public void SetRate(double rate)
        {
            ((RecipeNode)Helper.FindParentOfType(this, typeof(RecipeNode))).SetRate(rate, false);
        }
        public void SetConnectedRate(double rate)
        {
            speedText.Text = $"{(rate * 60).ToString("0.##")}/m";
            foreach(ItemNode node in Helper.GetConnected(this))
            {
                node.SetRate(rate);
            }
        }

        

        private void itemNode_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow).StartLineDraw(this);
        }
    }
}
