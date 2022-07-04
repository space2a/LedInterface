using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Image = System.Drawing.Image;
using MessageBox = System.Windows.Forms.MessageBox;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using Point = System.Windows.Point;

namespace LedInterface
{
    public partial class MainWindow : Window
    {
        private List<PanelIdentity> panelIdentities = new List<PanelIdentity>();

        public static LedColor currentColor = new LedColor() { R = 255, G = 255, B = 255};
        private bool ini = false;

        public MainWindow()
        {
            InitializeComponent();

            createPanel(32,16, PanelDirection.Origin);

            SizeBox.SelectionChanged += SizeBox_SelectionChanged;
            PlayingGif.Visibility = Visibility.Hidden;
            ini = true;
        }

        private void SizeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var el = panelIdentities[0].PanelElement.getLeds();

            for (int y = 0; y < el.Count; y++)
            {
                var ledstack = el[y] as StackPanel;
                for (int x = 0; x < ledstack.Children.Count; x++)
                {
                    var l = ledstack.Children[x] as Led;
                    Console.WriteLine(SizeBox.SelectedItem.ToString());
                    l.changedSize(int.Parse((SizeBox.SelectedItem as ComboBoxItem).Content.ToString()), int.Parse((SizeBox.SelectedItem as ComboBoxItem).Content.ToString()));
                }
            }
        }

        private void createPanel(int xSize, int ySize, PanelDirection panelDirection)
        {
            int[] coords = new int[2] { 0, 0 };
            if (panelDirection != PanelDirection.Origin)
                coords = getNewCoords(panelDirection);

            PanelIdentity panelIdentity = new PanelIdentity(coords) { xSize = xSize, ySize = ySize, Direction = panelDirection };
            panelIdentities.Add(panelIdentity);

            Panel panelElement = new Panel(panelIdentity) { Margin = new Thickness(20) };
            Workspace.Children.Add(panelElement);

            panelIdentity.PanelElement = panelElement;

            for (int y = 0; y < ySize; y++)
            {
                for (int x = 0; x < xSize; x++)
                {
                    LedIdentity ledIdentity = new LedIdentity() { CoordsX = x, CoordsY = y };
                    panelElement.addLed(ledIdentity);
                    //Console.WriteLine("new led " + x + ";" + y);
                }
            }
        }

        private int[] getNewCoords(PanelDirection Direction)
        {
            int[] coords = panelIdentities[panelIdentities.Count - 1].getCoords();

            coords[(int)Direction]++;

            return coords;
        }

        public string createLedsInfo(int f, int t, int pl = 0)
        {
            string info = "";

            int i = 0;
            foreach (var p in panelIdentities)
            {
                info += "#" + p.getCoords()[0] + ";" + p.getCoords()[1] + "\n" + p.PanelElement.createPanelInfo(f, t, pl);
            }

            return info + "|";
        }

        private void ButtonNewPanelBottom(object sender, MouseButtonEventArgs e) { createPanel(32, 16, PanelDirection.Bottom); }

        private void ButtonNewPanelRight(object sender, MouseButtonEventArgs e) { createPanel(32, 16, PanelDirection.Right); }

        private void senddata(string data)
        {
            DateTime start = DateTime.Now;
            ServerCom.SERVER_IP = IP.Text;
            string d = ServerCom.SendData(data, out Exception exception);
            if (exception != null)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("An error occured during the communication process" + exception.StackTrace);
                Console.ResetColor();
            }

            Console.WriteLine("From ESP32:'" + d + "' " + (DateTime.Now - start).TotalMilliseconds.ToString(".00") + "ms");
        }

        public bool Bits1 = false;
        public bool Bits4 = false;

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                isPlayingGif = false;
                Bitmap bitmap = new Bitmap(openFileDialog.FileName);
                Bitmap f  = new Bitmap(bitmap as System.Drawing.Image, new System.Drawing.Size(32,16));

                if(Bits1)
                    f = BitmapTo1Bpp(f);
                else if(Bits4)
                {
                    RectangleF cloneRect = new RectangleF(0, 0, 32, 16);
                    Bitmap cloneBitmap = f.Clone(cloneRect, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
                    f = cloneBitmap;
                }

                Console.WriteLine("new bitmap size:" + f.Width + "," + f.Height);

                var el = panelIdentities[0].PanelElement.getLeds();
                for (int y = 0; y < el.Count; y++)
                {
                    var ledstack = el[y] as StackPanel;
                    for (int x = 0; x < ledstack.Children.Count; x++)
                    {
                        var l = ledstack.Children[x] as Led;
                        var c = f.GetPixel(x, y);
                        l.updateColor(new LedColor() { R = c.R, G = c.G, B = c.B });
                    }
                }
            }
        }


        public static bool isPlayingGif = false;
        private void playGif(Image[] images)
        {
            Console.WriteLine("playGif(): " + images + " images");
            List<Bitmap> bitmaps = new List<Bitmap>();

            for (int i = 0; i < images.Length; i++)
            {
                Bitmap f = new Bitmap(images[i] as System.Drawing.Image, new System.Drawing.Size(32, 16));

                RectangleF cloneRect = new RectangleF(0, 0, 32, 16);
                if (Bits4)
                {
                    Bitmap cloneBitmap = f.Clone(cloneRect, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
                    bitmaps.Add(cloneBitmap);
                }
                else bitmaps.Add(f);
                f = null;
            }

            removeLeftovers();

            isPlayingGif = true;
            PlayingGif.Visibility = Visibility.Visible;

            new Thread(() =>
            {

                while (isPlayingGif)
                {
                    //ui thread invoke needed here

                    int i = 0;
                    bool g = false;
                    double w = 0;
                    foreach (var img in bitmaps)
                    {
                        g = false;
                        System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            var el = panelIdentities[0].PanelElement.getLeds();
                            for (int y = 0; y < el.Count; y++)
                            {
                                var ledstack = el[y] as StackPanel;
                                for (int x = 0; x < ledstack.Children.Count; x++)
                                {
                                    var l = ledstack.Children[x] as Led;
                                    var c = img.GetPixel(x, y);
                                    l.updateColor(new LedColor() { R = c.R, G = c.G, B = c.B });
                                }
                            }

                            PlayingState.Content = "Lecture gif... (" + i++ + "/" + bitmaps.Count + ")";
                            PlayingGif.Visibility = Visibility.Visible;


                            DateTime start = DateTime.Now;
                            senddata(createLedsInfo(0, 8));
                            senddata(createLedsInfo(8, 16, 16));

                            w = 100 - ((DateTime.Now - start).TotalMilliseconds);

                            Console.WriteLine("gif latency : " + ((DateTime.Now - start).TotalMilliseconds));
                            g = true;
                        }));

                        while (!g) { }
                        Console.WriteLine(w);
                        if(w > 0)
                            Thread.Sleep(int.Parse(w.ToString().Substring(0, w.ToString().IndexOf(","))));

                        if (!isPlayingGif) break;
                    }
                }

                System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    PlayingGif.Visibility = Visibility.Hidden;
                }));

            }).Start();
        }


        public static Bitmap BitmapTo1Bpp(Bitmap img)
        {
            int w = img.Width;
            int h = img.Height;
            Bitmap bmp = new Bitmap(w, h, System.Drawing.Imaging.PixelFormat.Format1bppIndexed);
            BitmapData data = bmp.LockBits(new System.Drawing.Rectangle(0, 0, w, h), ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format1bppIndexed);
            byte[] scan = new byte[(w + 7) / 8];
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    if (x % 8 == 0) scan[x / 8] = 0;
                    System.Drawing.Color c = img.GetPixel(x, y);
                    if (c.GetBrightness() >= 0.5) scan[x / 8] |= (byte)(0x80 >> (x % 8));
                }
                Marshal.Copy(scan, 0, (IntPtr)((long)data.Scan0 + data.Stride * y), scan.Length);
            }
            bmp.UnlockBits(data);
            return bmp;
        }

        private void SendData_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (IP.Text == "") { MessageBox.Show("Merci de renseigner une adresse IP pour l'ESP32.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }

            if (isPlayingGif) return;
            senddata(createLedsInfo(0, 8));
            senddata(createLedsInfo(8, 16, 16));
        }

        private void ColorPicker_Closed(object sender, RoutedEventArgs e)
        {
        }

        private void CP_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<System.Windows.Media.Color?> e)
        {
            currentColor = new LedColor() { R = e.NewValue.Value.R, B = e.NewValue.Value.B, G = e.NewValue.Value.G };
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "fichiers gif (*.gif)|*.gif";
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                new Thread(() =>
                {
                    if (isPlayingGif)
                    {
                        isPlayingGif = false;
                        Thread.Sleep(250);
                    }

                    Image[] frames = getFrames(Image.FromFile(openFileDialog.FileName));
                    
                    System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        System.Windows.MessageBox.Show("Récupération des images terminée");
                        playGif(frames);
                    }));

                }).Start();
            }
        }


        Image[] getFrames(Image originalImg)
        {
            int numberOfFrames = originalImg.GetFrameCount(FrameDimension.Time);
            Image[] frames = new Image[numberOfFrames];

            for (int i = 0; i < numberOfFrames; i++)
            {
                originalImg.SelectActiveFrame(FrameDimension.Time, i);
                frames[i] = ((Image)originalImg.Clone());
            }

            return frames;
        }

        private void StopPlayingGif_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isPlayingGif = false;
        }

        private void removeLeftovers() //plus dutiliter
        {
            return;
            string[] cs = System.IO.Directory.GetFiles(".", "*.png");
            for (int i = 0; i < cs.Length; i++)
            {
                System.IO.File.Delete(cs[i]);
                Console.WriteLine("leftover:" + cs[i] + " removed");
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e) { coordsLed(true); }

        private void coordsLed(bool s)
        {
            if (!ini) return;
            var el = panelIdentities[0].PanelElement.getLeds();
            for (int y = 0; y < el.Count; y++)
            {
                var ledstack = el[y] as StackPanel;
                for (int x = 0; x < ledstack.Children.Count; x++)
                {
                    var l = ledstack.Children[x] as Led;
                    l.ledCoords(s);
                }
            }
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e) { coordsLed(false); }

        private void TextBlock_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            SearchESP32 searchESP32 = new SearchESP32(this);
            searchESP32.ShowDialog();
            if(searchESP32.ESP32IP != "")
            {
                IP.Text = searchESP32.ESP32IP;
                ServerCom.SERVER_IP = IP.Text;
            }    
        }

        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            foreach (var p in panelIdentities)
            {
                foreach (StackPanel item in p.PanelElement.Workspace.Children)
                {
                    foreach (var l in item.Children)
                    {
                        (l as Led).updateColor(new LedColor() { R = 0, G = 0, B = 0 });
                    }
                }
            }
        }

        private void Bpp4_Checked(object sender, RoutedEventArgs e)
        {
            Bpp1.IsChecked = false;
            Rienbpp.IsChecked = false;
            Bits1 = false;
            Bits4 = true;
        }

        private void Bpp1_Checked(object sender, RoutedEventArgs e)
        {
            Bpp4.IsChecked = false;
            Rienbpp.IsChecked = false;
            Bits1 = true;
            Bits4 = false;
        }

        private void Rienbpp_Checked(object sender, RoutedEventArgs e)
        {
            Bpp4.IsChecked = false;
            Bpp1.IsChecked = false;
            Rienbpp.IsChecked = true;
            Bits1 = false;
            Bits4 = false;
        }
    }
}
