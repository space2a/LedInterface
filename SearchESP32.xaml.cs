using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LedInterface
{
    public partial class SearchESP32 : Window
    {

        public string ESP32IP = "";

        string[] data = new string[2] { "", "" };

        public SearchESP32(MainWindow mainWindow)
        {
            InitializeComponent();

            data[0] = mainWindow.createLedsInfo(0, 8);
            data[1] = mainWindow.createLedsInfo(8, 16, 16);

            this.Loaded += SearchESP32_Loaded;
        }

        private void SearchESP32_Loaded(object sender, RoutedEventArgs e)
        {
            

            new Thread(() =>
            {
                string IP = "";

                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                        IP = ip.ToString();

                IP = IP.Substring(0, IP.LastIndexOf(".") + 1);

                for (int i = 0; i < 255; i++)
                {
                    new Thread(() =>
                    {
                        string thIp = IP + i;
                        Console.WriteLine("NEW THREAD : " + thIp);
                        if (ServerCom.SendData(data[0], out Exception err, thIp).IndexOf("ESP32OK") != -1)
                        {
                            ServerCom.SendData(data[1], out Exception err2, thIp);
                            ESP32IP = thIp;
                            System.Windows.Application.Current.Dispatcher.Invoke(new Action(() => { MessageBox.Show("ESP32 découvert."); this.Close(); }));
                        }

                    }).Start();
                    Thread.Sleep(10);
                }

            }).Start();
        }
    }
}
