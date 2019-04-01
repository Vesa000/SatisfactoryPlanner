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
    /// Interaction logic for RecipeNode.xaml
    /// </summary>
    public partial class RecipeNode : UserControl
    {
        public Recipe recipe;
        Canvas canvas;
        Double zoom;
        public Boolean dragEnabled = true;
        public double machinecount = 0;
        public double requestedRate = 0;
        public double currentRate = 0;

        public RecipeNode(Recipe _recipe, Canvas _canvas, Double _zoom)
        {
            InitializeComponent();

            recipe = _recipe;
            canvas = _canvas;
            zoom = _zoom;
            foreach (RecipeItem item in recipe.inputs)
            {
                ItemNode inode = new ItemNode(item, true);
                inPanel.Children.Add(inode);
            }
            foreach (RecipeItem item in recipe.Outputs)
            {
                ItemNode inode = new ItemNode(item, false);
                outPanel.Children.Add(inode);
            }
            recipeText.Text = recipe.name;
            ((MainWindow)Application.Current.MainWindow).UpdateLines();
        }

        public void Reset()
        {
            currentRate = 0;
            deviceText.Text = recipe.device.name;
            rectangle.Stroke = new SolidColorBrush(Colors.White);
            for (int i = 0; i < inPanel.Children.Count; i++)
            {
                ((ItemNode)inPanel.Children[i]).speedText.Text = recipe.inputs[i].count.ToString();
            }
            for (int i = 0; i < outPanel.Children.Count; i++)
            {
                ((ItemNode)outPanel.Children[i]).speedText.Text = recipe.Outputs[i].count.ToString();
            }
        }
        public void SetRates()
        {
            if (requestedRate != 0)
            {
                rectangle.Stroke = new SolidColorBrush(Colors.Green);
                SetRate(requestedRate, true);
            }
        }

        private void RecipeNode_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.MouseDevice.LeftButton == MouseButtonState.Released || !dragEnabled || ((MainWindow)Application.Current.MainWindow).drawLine)
                return;
            Point p = e.GetPosition(canvas);
            p.X = p.X - (grid.Width / 2);
            p.Y = p.Y - (grid.Height / 2);
            Canvas.SetLeft(this, p.X);
            Canvas.SetTop(this, p.Y);
            ((MainWindow)Application.Current.MainWindow).UpdateLines();
        }

        public void SetRate(double rate, Boolean first)
        {
            if (requestedRate != 0)
            {
                if(!first)
                {
                    return;
                }
            }
            else
            {
                rectangle.Stroke = new SolidColorBrush(Colors.Black);
            }

            currentRate += rate / recipe.Outputs[0].count;
            SetMachineTexts();

            foreach (ItemNode node in inPanel.Children)
            {
                node.SetConnectedRate(currentRate * node.recipeItem.count);
            }
            foreach (ItemNode node in outPanel.Children)
            {
                node.speedText.Text = $"{(currentRate * node.recipeItem.count * 60).ToString("0.##")}/m";
            }
        }

        public void SetRequestedRate()
        {
            if (requestedRate != 0)
            {
                currentRate = requestedRate;
                SetRateUp(requestedRate);
            }
        }

        public double SetRateUp(double rate)
        {
            if (currentRate > rate)
            {
                currentRate = rate;
                SetMachineTexts();
                SetRate(currentRate, false);
                rectangle.Stroke = new SolidColorBrush(Colors.Red);
            }
            double rateOut = currentRate;
            List<ItemNode> nodes = Helper.GetConnected((ItemNode)outPanel.Children[0]);
            foreach (ItemNode node in nodes)
            {
                RecipeNode rnode = (RecipeNode)Helper.FindParentOfType(node, typeof(RecipeNode));
                rateOut = rnode.SetRateUp(rateOut * (recipe.Outputs[0].count / node.recipeItem.count));
            }
            return rate - currentRate;
        }

        private void SetMachineTexts()
        {
            machinecount = (currentRate * recipe.CraftTime);
            deviceText.Text = $"{machinecount.ToString("0.##")} {recipe.device.name}s";
        }

        private void DeleteNode_Click(object sender, RoutedEventArgs e)
        {
            foreach (ItemNode node in inPanel.Children)
            {
                ((MainWindow)Application.Current.MainWindow).DeleteConnections(node);
            }
            foreach (ItemNode node in outPanel.Children)
            {
                ((MainWindow)Application.Current.MainWindow).DeleteConnections(node);
            }
            ((Canvas)this.Parent).Children.Remove(this);
        }

        private void ItemRateText_TextChanged(object sender, TextChangedEventArgs e)
        {
            requestedRate = 0;
            if (int.TryParse(ItemRateText.Text, out int rate))
            {
                requestedRate = (double)rate / (double)60;
            }
            ((MainWindow)Application.Current.MainWindow).UpdateRates();
        }

        public double RateToPerMinute(double pieces, double time)
        {
            return (60 * pieces) / time;
        }
    }
}
