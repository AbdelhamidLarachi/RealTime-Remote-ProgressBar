using System.Diagnostics;
using System;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Windows;
using System.Collections.Generic;
using System.Linq;

namespace WpfApplication3
{
    public partial class MainWindow : Window
    {
        public static MainWindow AppWindow;

        public MainWindow()
        {
            InitializeComponent();
        }

         public void myMain()
        {
            TcpListener serverSocket = new TcpListener(4523);
            serverSocket.Start();
            Debug.WriteLine("connected");
           
            while (true)
            {
                TcpClient clientSocket = serverSocket.AcceptTcpClient();
                handleClient clientx = new handleClient();
                clientx.startClient(clientSocket);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
                Thread thread = new Thread(delegate ()
            {
                WorkMethod();
            });
            thread.IsBackground = true;
            thread.Start();

            Thread t = new Thread(myMain);
            t.SetApartmentState(ApartmentState.STA);
            t.Start();

        }


        private void WorkMethod()
        {
            int t = 100;
            int i = 0;
            while (i < t)
            {
                Thread.Sleep(1000);// to get first value!

                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    pbMyProgressBar.Value = i;
                    pbMyProgressBar.Maximum = t;
                    prog.Text = i + "/" + t;

                }));
                i = handleClient.progression;
                t = handleClient.TotalFiles;
            }
            
        }
    }
 

    public class handleClient
    {
        MainWindow mw = new MainWindow();
        public static int TotalFiles;
        public static int progression=0;


        TcpClient clientSocket;
        public void startClient(TcpClient inClientSocket)
        {
            this.clientSocket = inClientSocket;
            Thread ctThread = new Thread(GetStream);
            ctThread.Start();
        }

        private void GetStream()
        {
            byte[] buffer = new byte[100];
            while (true)
            {
                BinaryReader reader = new BinaryReader(clientSocket.GetStream());
                string i = reader.ReadString();
                TotalFiles = Convert.ToInt32(i);
                progression++;
                
            }

        }
    }
}