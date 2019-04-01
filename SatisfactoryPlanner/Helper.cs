using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SatisfactoryPlanner
{
    public static class Helper
    {
        public static FrameworkElement FindParentOfType(FrameworkElement child, Type type)
        {
            FrameworkElement parent = (FrameworkElement)child.Parent;
            if(parent== null)
            {
                return null;
            }
            else if (parent.GetType() == type)
            {
                return parent;
            }
            else
            {
                return FindParentOfType(parent, type);
            }
        }
        public static List<ItemNode> GetConnected(ItemNode from)
        {
            List<ItemNode> connected = new List<ItemNode>();
            foreach (Connection c in ((MainWindow)Application.Current.MainWindow).connections)
            {
                if (c.from == from)
                {
                    connected.Add(c.to);
                }
                if (c.to == from)
                {
                    connected.Add(c.from);
                }
            }
            return connected;
        }
    }
}
