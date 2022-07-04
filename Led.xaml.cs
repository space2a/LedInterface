using System;
using System.Collections.Generic;
using System.Text;
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
    public partial class Led : UserControl
    {
        public LedIdentity _identity;

        public Led(LedIdentity ledIdentity)
        {
            InitializeComponent();
            _identity = ledIdentity;
            Debug.Content = _identity.CoordsX + ";" + _identity.CoordsY;
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            updateColor(MainWindow.currentColor, true);
        }

        public void changedSize(int w, int h)
        {
            this.Width = w;
            this.Height = h;
        }

        public void updateColor(LedColor ledColor, bool manual = false)
        {
            if (MainWindow.isPlayingGif && manual) { MessageBox.Show("Impossible de dessiner lors de la lecture d'un fichier gif."); return; }

            Background.Fill = new SolidColorBrush(Color.FromArgb(255, (byte)ledColor.R, (byte)ledColor.G, (byte)ledColor.B));
            _identity.Color = ledColor;
        }

        public void ledCoords(bool s)
        {
            if (s)
                Debug.Visibility = Visibility.Visible;
            else
                Debug.Visibility = Visibility.Hidden;
        }

        private void Grid_MouseEnter(object sender, MouseEventArgs e)
        {
            if(Mouse.LeftButton == MouseButtonState.Pressed)
                updateColor(MainWindow.currentColor, true);
            else if(Mouse.RightButton == MouseButtonState.Pressed)
                updateColor(new LedColor() { R = 0, B = 0, G = 0 }, true);
        }

        private void Grid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            updateColor(new LedColor() { R = 0, B = 0, G = 0 } );
        }
    }
}
