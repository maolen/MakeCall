using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows;

namespace MakeCall
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static UdpClient Sender { get; set; }
        public static UdpClient Listener { get; set; }
        public static WaveIn Input { get; set; }
        public static WaveOut Output { get; set; }
        public static BufferedWaveProvider BufferStream { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            Input = new WaveIn();
            Input.WaveFormat = new WaveFormat(8000, 16, 1);
            Input.DataAvailable += VoiceInput;
            Output = new WaveOut();
            BufferStream = new BufferedWaveProvider(new WaveFormat(8000, 16, 1));
            Output.Init(BufferStream);

            Task.Run(() => Listening());
        }


        private static void Listening()
        {
            using (Listener = new UdpClient(int.Parse(listenPortTB.Text)))
            {
                IPEndPoint remoteIp = null;
                Output.Play();
                try
                {
                    while (true)
                    {
                        byte[] data = Listener.Receive(ref remoteIp);
                        BufferStream.AddSamples(data, 0, data.Length);
                    }
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception.Message);
                }
            }

        }

        private static void CallClick(object sender, RoutedEventArgs e)
        {
            Input.StartRecording();
        }
        private static void VoiceInput(object sender, WaveInEventArgs e)
        {

            try
            {
                EndPoint endPoint = new IPEndPoint(IPAddress.Parse(ipAdressTB.Text), int.Parse(listenPortTB.Text));
                Sender = new UdpClient();
                Sender.Send(e.Buffer, e.Buffer.Length, endPoint);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
}
