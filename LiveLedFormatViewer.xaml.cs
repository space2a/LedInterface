using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Timers;
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
    public partial class LiveLedFormatViewer : Window
    {
        private MainWindow _mainWindow;
        private bool run = true;
        public LiveLedFormatViewer(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            this.Closing += delegate (object sender, System.ComponentModel.CancelEventArgs e) { run = false; };

           new Thread(() =>
           {

               while (run)
               {
                   Application.Current.Dispatcher.Invoke(new Action(() =>
                   {
                       //DataBox.Text = _mainWindow.createLedsInfo();
                   }));

                   Thread.Sleep(1000);
               }

           }).Start();
        }
    }
}
