using NAudio.Wave;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MakeCall
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //public string ReceiverIp { get; set; } = "127.0.0.1";
        public int Port { get; set; } = 5555;
        public bool IsConnected { get; set; }
        public Socket Client { get; set; }
        public WaveIn Input { get; set; }
        public WaveOut Output { get; set; }
        public BufferedWaveProvider BufferStream { get; set; }
        public Thread IncomeThread { get; set; }
        public Socket Listener { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            //var sender = new DarrenLee.LiveStream.Audio.Sender();
            //sender.Send(ReceiverIp, Port);

            //var receiver = new DarrenLee.LiveStream.Audio.Receiver();
            //receiver.Receive(ReceiverIp, Port);
            //создаем поток для записи нашей речи
            using (Input = new WaveIn())
            using(Output = new WaveOut())
            {
                //определяем его формат - частота дискретизации 8000 Гц, ширина сэмпла - 16 бит, 1 канал - моно
                Input.WaveFormat = new WaveFormat(8000, 16, 1);
                //добавляем код обработки нашего голоса, поступающего на микрофон
                Input.DataAvailable += Voice_Input;
                //создаем поток для прослушивания входящего звука

                //создаем поток для буферного потока и определяем у него такой же формат как и потока с микрофона
                BufferStream = new BufferedWaveProvider(new WaveFormat(8000, 16, 1));
                //привязываем поток входящего звука к буферному потоку

                Output.Init(BufferStream);
                //сокет для отправки звука
                Client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                IsConnected = true;
                Listener = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                //создаем поток для прослушивания
                IncomeThread = new Thread(new ThreadStart(Listening));
                //запускаем его
                IncomeThread.Start();
            }           
        }
        private void Listening()
        {
            //Прослушиваем по адресу
            IPEndPoint localIP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), Port);
            Listener.Bind(localIP);
            //начинаем воспроизводить входящий звук
            Output.Play();
            //адрес, с которого пришли данные
            EndPoint remoteIp = new IPEndPoint(IPAddress.Any, 0);
            //бесконечный цикл
            while (IsConnected == true)
            {
                try
                {
                    //промежуточный буфер
                    byte[] data = new byte[65535];
                    //получено данных
                    int received = Listener.ReceiveFrom(data, ref remoteIp);
                    //добавляем данные в буфер, откуда output будет воспроизводить звук
                    BufferStream.AddSamples(data, 0, received);
                }
                catch (SocketException exception)
                {
                    MessageBox.Show(exception.Message);
                }
            }
        }
        private void Voice_Input(object sender, WaveInEventArgs e)
        {
            try
            {
                //Подключаемся к удаленному адресу
                IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Parse(addressTextBox.Text), Port);
                //посылаем байты, полученные с микрофона на удаленный адрес
                Client.SendTo(e.Buffer, remoteEndPoint);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void MakeCall(object sender, RoutedEventArgs e)
        {
            Input.StartRecording();
        }
    }
}
