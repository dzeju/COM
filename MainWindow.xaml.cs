using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using System.IO.Ports;
using System.Threading;

namespace COM
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static bool _continue = false;
        SerialPort serialPort = new SerialPort();
        Thread readThread;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = new ComboBoxItems();
            {
                PortsBox.SelectedIndex = 3;
                BaudRateBox.SelectedIndex = 3;
                ParityBox.SelectedIndex = 0;
                DataBitsBox.SelectedIndex = 1;
                StopBitsBox.SelectedIndex = 1;
            }
            readThread = new Thread(ReadData);

        }

        private void OpenBtn_Click(object sender, RoutedEventArgs e)
        {
            if (serialPort.IsOpen)
            {
                try
                {
                    _continue = false;
                    readThread.Join();
                    serialPort.Close();

                    OpenBtn.Content = "Otwórz Port";
                    {
                        PortsBox.IsEnabled = true;
                        BaudRateBox.IsEnabled = true;
                        ParityBox.IsEnabled = true;
                        DataBitsBox.IsEnabled = true;
                        StopBitsBox.IsEnabled = true;
                    }
                }
                catch (System.IO.IOException ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else if (!serialPort.IsOpen)
            {
                try
                {
                    serialPort = new SerialPort(PortsBox.SelectedItem.ToString(),
                                                Convert.ToInt32(BaudRateBox.SelectedItem),
                                                (Parity)Enum.Parse(typeof(Parity), ParityBox.SelectedItem.ToString()),
                                                Convert.ToInt32(DataBitsBox.SelectedItem),
                                                (StopBits)Enum.Parse(typeof(StopBits), StopBitsBox.SelectedItem.ToString()));
                    serialPort.Open();

                    _continue = true;
                    readThread = new Thread(ReadData);
                    readThread.Start();
                    OpenBtn.Content = "Zamknij Port";
                    {
                        PortsBox.IsEnabled = false;
                        BaudRateBox.IsEnabled = false;
                        ParityBox.IsEnabled = false;
                        DataBitsBox.IsEnabled = false;
                        StopBitsBox.IsEnabled = false;
                    }
                }
                catch (System.IO.IOException ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void ReadData()
        {
            while (_continue)
            {
                if(serialPort.BytesToRead != 0)
                    try
                    {
                        int bytes = serialPort.BytesToRead;
                        byte[] buffer = new byte[bytes];
                        serialPort.Read(buffer, 0, bytes);
                        /*string buff = serialPort.ReadLine();
                        byte[] buffer = Encoding.ASCII.GetBytes(buff);*/
                        Handler(buffer);
                    }
                catch (TimeoutException) { }
                Thread.Sleep(1);
            }
        }

        private void Handler(byte[] buffer)
        {
            byte[] up = { 0x04, 0x01 };
            byte[] down = { 0x04, 0x10 };
            if (buffer == up)
                MessageBox.Show("UP");
            else if (buffer == down)
                MessageBox.Show("DOWN");
            else
                MessageBox.Show("Nie rozpoznaję \n" + 
                                BitConverter.ToString(buffer) + "\n" +
                                BitConverter.ToString(up));
        }
    }
}
