using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Serialization;

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
   
        class MyImage
        {
            public string Name { get; set; }
            public ImageSource ImageSource { get; set; }
        }
    public partial class MainWindow : Window
    {
        ObservableCollection<MyImage> myImages = new ObservableCollection<MyImage>();
        public MainWindow()
        {
            InitializeComponent();
            Images.ItemsSource = myImages;

        }
        public T Deserializer<T>(byte[] _byteArray)
        {
            T ReturnValue;
            using (var _MemoryStream = new MemoryStream(_byteArray))
            {
                IFormatter _BinaryFormatter = new BinaryFormatter();
                ReturnValue = (T)_BinaryFormatter.Deserialize(_MemoryStream);
            }
            return ReturnValue;
        }
        public BitmapImage Convert(Bitmap src)
        {
            MemoryStream ms = new MemoryStream();
            ((System.Drawing.Bitmap)src).Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            ms.Seek(0, SeekOrigin.Begin);
            image.StreamSource = ms;
            image.EndInit();
            return image;
        }
        byte[] bufferData = new byte[1920 * 1080 * 4];
        bool stop = false;
        void GetPrintScreen(object obj)
        {
            int sleep = (int)obj;
            if (token.IsCancellationRequested)
            {
                MessageBox.Show("Operation ended");
                return;
            }
            while (stop == false)
            {
                Thread.Sleep(sleep);
                TcpClient client = new TcpClient();
                try
                {
                    IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 7777);
                    client.Connect(iPEndPoint);

                    client.GetStream().Read(bufferData, 0, bufferData.Length);
                    Bitmap bitmap = Deserializer<Bitmap>(bufferData);
                    Dispatcher.Invoke(() =>
                    {
                        myImages.Add(new MyImage() { Name = DateTime.Now.ToString(), ImageSource = Convert(bitmap) });
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
        Task task;
        static CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
        CancellationToken token = cancelTokenSource.Token;
        private void btStop_Click(object sender, RoutedEventArgs e)
        {

            stop = true;

        }

        private void btStart_Click(object sender, RoutedEventArgs e)
        {

            if (stop == true)
            {
                stop = false;
            }
            task = new Task((obj) => GetPrintScreen(obj), int.Parse(tbTimeSpan.Text) * 1000);
            task.Start();
        }


    }
}
