using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LedInterface
{
    public partial class Panel : UserControl
    {
        private PanelIdentity _identity;
        public Panel(PanelIdentity identity)
        {
            InitializeComponent();
            _identity = identity;
        }

        public void addLed(LedIdentity identity)
        {
            Led led = new Led(identity) { Margin = new Thickness(1) };


            if (led._identity.CoordsX == 0)
            {
                StackPanel stackPanel = new StackPanel() { Margin = new Thickness(1), Background = new SolidColorBrush(Colors.Black), Orientation = Orientation.Horizontal };
                Workspace.Children.Add(stackPanel);
            }

            (Workspace.Children[Workspace.Children.Count - 1] as StackPanel).Children.Add(led);
        }

        public string createPanelInfo(int from, int to, int p = 0)
        {
            string info = "";

            //var c = new LedColor();
            //Thread.Sleep(1);
            //c.R = new Random().Next(0, 255);
            //Thread.Sleep(1);
            //c.G = new Random().Next(0, 255);
            //Thread.Sleep(2);
            //c.B = new Random().Next(0, 255);
            //
            //
            //int thr = 0;

            for (int i = from; i < to; i++)
            {
                foreach (var l in (Workspace.Children[i] as StackPanel).Children)
                {
                    var ii = (l as Led)._identity;
                    //info += "-" + ii.CoordsX + "." + (ii.CoordsY+p) + ";" + ii.Color.getColor() + "_\n";
                    info += "-" + ii.Color.getColor() + "_\n";
                    //(l as Led).updateColor(c);
                    //if (thr++ > 256) return info;
                }
            }

            return info;
        }

        public UIElementCollection getLeds()
        {
            return Workspace.Children;
        }


    }
}
