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
    public partial class MainWindow : Window
    {
        Double zoomMax = 500;
        Double zoomMin = 0.05;
        Double zoom = 1;
        Point lastpos;
        private ScaleTransform scaleTransform;
        private TranslateTransform translateTransform;
        private TransformGroup transformGroup;
        RecipeHandler rh = new RecipeHandler();
        public bool drawLine = false;
        ItemNode ItemBeingConnected;
        Line ConnectionLine;
        public List<Connection> connections = new List<Connection>();

        public MainWindow()
        {
            InitializeComponent();

            translateTransform = new TranslateTransform();
            scaleTransform = new ScaleTransform();
            transformGroup = new TransformGroup();
            transformGroup.Children.Add(scaleTransform);
            transformGroup.Children.Add(translateTransform);
            canvas.RenderTransform = transformGroup;
            ItemSearchBox_TextChanged(null, null);
        }

        private void addItemButton_Click(object sender, RoutedEventArgs e)
        {
            Recipe recipe = ((ListItem)ItemsListView.SelectedItem).recipe;
            RecipeNode rn = new RecipeNode(recipe, canvas, zoom);
            canvas.Children.Add(rn);
            rn.SetValue(Canvas.TopProperty, canvas.Height / 2);
            rn.SetValue(Canvas.LeftProperty, canvas.Width / 2);
        }

        private void ItemSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ItemsListView.Items.Clear();
            List<Recipe> items = rh.GetItems(ItemSearchBox.Text);
            foreach (Recipe r in items)
            {
                ListItem li = new ListItem(r);
                ItemsListView.Items.Add(li);
            }
        }

        public void StartLineDraw(ItemNode from)
        {
            drawLine = true;
            ItemBeingConnected = from;
            ConnectionLine = new Line();
            ConnectionLine.Stroke = Brushes.Black;
            Canvas.SetZIndex(ConnectionLine, -1);
            canvas.Children.Add(ConnectionLine);
        }

        public void DeleteConnections(ItemNode inode)
        {
            for (int i = 0; i < connections.Count(); i++)
            {
                if (connections[i].from == inode || connections[i].to == inode)
                {
                    canvas.Children.Remove(connections[i].line);
                    connections.Remove(connections[i]);
                }
            }
        }

        public void UpdateRates()
        {
            for(int i=0;i<canvas.Children.Count;i++)
            {
                if (canvas.Children[i].GetType()==typeof(RecipeNode))
                {
                    ((RecipeNode)canvas.Children[i]).Reset();
                }
            }
            for (int i = 0; i < canvas.Children.Count; i++)
            {
                if (canvas.Children[i].GetType() == typeof(RecipeNode))
                {
                    ((RecipeNode)canvas.Children[i]).SetRates();
                }
            }
            for (int i = 0; i < canvas.Children.Count; i++)
            {
                if (canvas.Children[i].GetType() == typeof(RecipeNode))
                {
                    ((RecipeNode)canvas.Children[i]).SetRequestedRate();
                }
            }
        }

        private void canvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            double delta = e.Delta > 0 ? 1.1 : 0.9;
            zoom *= delta;
            if (zoom < zoomMin) { zoom = zoomMin; }
            if (zoom > zoomMax) { zoom = zoomMax; }
            Point mousePos = e.GetPosition(canvas);

            double oldCenterX = scaleTransform.CenterX;
            double oldCenterY = scaleTransform.CenterY;

            scaleTransform.CenterX = mousePos.X;
            scaleTransform.CenterY = mousePos.Y;

            translateTransform.X += (mousePos.X - oldCenterX) * (scaleTransform.ScaleX - 1);
            translateTransform.Y += (mousePos.Y - oldCenterY) * (scaleTransform.ScaleY - 1);

            scaleTransform.ScaleX = zoom;
            scaleTransform.ScaleY = zoom;
            e.Handled = true;
        }

        private void canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (canvas.IsMouseCaptured)
            {
                Point t = e.GetPosition(canvas);
                Vector delta = (e.GetPosition(canvas) - lastpos) * zoom;
                translateTransform.X += delta.X;
                translateTransform.Y += delta.Y;
                lastpos = e.GetPosition(canvas);
                e.Handled = true;
            }
            else if (e.MouseDevice.LeftButton == MouseButtonState.Pressed && drawLine)
            {
                Point p = ItemBeingConnected.TranslatePoint(new Point(0, 0), canvas);
                ConnectionLine.X1 = p.X + 25;
                ConnectionLine.Y1 = p.Y + 25;
                Point mousePos = e.GetPosition(canvas);
                ConnectionLine.X2 = mousePos.X;
                ConnectionLine.Y2 = mousePos.Y;
            }
        }

        private void canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            canvas.ReleaseMouseCapture();
            if (drawLine)
            {
                drawLine = false;
                canvas.Children.Remove(ConnectionLine);
                RecipeNode rn = (RecipeNode)Helper.FindParentOfType((FrameworkElement)e.OriginalSource, typeof(RecipeNode));
                if (rn != null)
                {
                    foreach (ItemNode node in ItemBeingConnected.input ? rn.outPanel.Children : rn.inPanel.Children)
                    {
                        if(node.recipeItem.item.Name== ItemBeingConnected.recipeItem.item.Name)
                        {
                            connections.Add(new Connection(ItemBeingConnected, node,new Line(),canvas));
                            UpdateRates();
                        }
                    }
                }
                else
                {
                    ContextMenu cm = ConnectRecipeMenu(ItemBeingConnected);
                    cm.IsOpen = true;
                }
            }
            e.Handled = true;
        }

        private void canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource == canvas)
            {
                lastpos = e.GetPosition(canvas);
                canvas.CaptureMouse();
                e.Handled = true;
            }
        }

        private ContextMenu ConnectRecipeMenu(ItemNode from)
        {
            ContextMenu cm = new ContextMenu();
            List<Recipe> recipes;
            if (from.input)
            {
                recipes = rh.GetRecipesProducing(from.recipeItem.item.Name);
            }
            else
            {
                recipes = rh.GetRecipesUsing(from.recipeItem.item.Name);
            }
            foreach (Recipe r in recipes)
            {
                MenuItem mi = new MenuItem();
                mi.Icon = new Image { Source = new BitmapImage(new Uri(r.Outputs[0].item.ImageSource, UriKind.Relative)) };
                TextBlock tb = new TextBlock();
                string str = string.Join(" + ", r.inputs.Select(ri => $"{ri.item.Name}({ri.count})"));
                str += " => ";
                str += string.Join(" + ", r.Outputs.Select(ri => $"{ri.item.Name}({ri.count})"));
                tb.Text = str;
                mi.Header = tb;
                mi.Click += delegate (object sender, RoutedEventArgs e) { RecipeContextMenu_Click(sender, e, r, new Point(ConnectionLine.X2, ConnectionLine.Y2), from); };
                cm.Items.Add(mi);
            }
            return cm;
        }

        private void RecipeContextMenu_Click(object sender, RoutedEventArgs e, Recipe recipe, Point pos, ItemNode from)
        {
            RecipeNode rn = new RecipeNode(recipe, canvas, zoom);
            canvas.Children.Add(rn);
            Canvas.SetLeft(rn, pos.X - 125);
            Canvas.SetTop(rn, pos.Y - 75);
            foreach (ItemNode to in rn.inPanel.Children)
            {
                if (to.recipeItem.item.Name == from.recipeItem.item.Name)
                {
                    connections.Add(new Connection(from, to, new Line(),canvas));
                }
            }
            foreach (ItemNode to in rn.outPanel.Children)
            {
                if (to.recipeItem.item.Name == from.recipeItem.item.Name)
                {
                    connections.Add(new Connection(from, to, new Line(),canvas));
                }
            }
            UpdateLines();
            UpdateRates();
        }
        public void UpdateLines()
        {
            foreach (Connection c in connections)
            {
                Point p = c.from.TranslatePoint(new Point(0, 0), canvas);
                Point p2 = c.to.TranslatePoint(new Point(0, 0), canvas);
                c.line.X1 = p.X + 25;
                c.line.Y1 = p.Y + 25;
                c.line.X2 = p2.X + 25;
                c.line.Y2 = p2.Y + 25;
            }
        }
    }
}