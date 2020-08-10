using System;
using System.Linq;
using System.Windows;
using System.IO.Ports;
using System.Threading;
using System.ComponentModel;
using System.Collections.Generic;
using System.Windows.Controls;

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
        int counter = 0;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = new DataCont();
            {
                PortsBox.SelectedIndex = 1;
                BaudRateBox.SelectedIndex = 3;
                ParityBox.SelectedIndex = 0;
                DataBitsBox.SelectedIndex = 1;
                StopBitsBox.SelectedIndex = 1;
            }
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
                        Handler(buffer);
                    }
                catch (TimeoutException) { }
                //Thread.Sleep(1);
            }
        }

        private void Handler(byte[] buffer)
        {
            byte[] up = { 0x04, 0x01 };
            byte[] down = { 0x04, 0x10 };

            if (buffer.Length >= 2)
                if (buffer[buffer.Length-2] == 0x04)
                {
                    if (buffer[buffer.Length-1] == 0x01)
                    {
                        this.Dispatcher.Invoke((Action)(() =>
                        {
                            counter++;
                            count.Text = counter.ToString() + ", cm: " + ((float)((float)counter / Convert.ToInt32(PrzelicznikTxt.Text))).ToString();
                            StatusLbl.Content = "odebrano pakiet: " + BitConverter.ToString(buffer);
                        }));
                    }
                    else if (buffer[buffer.Length - 1] == 0x10)
                    {
                        this.Dispatcher.Invoke((Action)(() =>
                        {
                            counter--;
                            count.Text = counter.ToString() + ", cm: " + ((float)((float)counter / Convert.ToInt32(PrzelicznikTxt.Text))).ToString();
                            StatusLbl.Content = "odebrano pakiet: " + BitConverter.ToString(buffer);
                        }));
                    }
                    else
                    {
                        this.Dispatcher.Invoke((Action)(() =>
                        {
                            StatusLbl.Content = "nie rozpoznano pakietu: " + BitConverter.ToString(buffer);
                        }));
                    }
                }

            /*if (buffer.SequenceEqual(up))
            {
                this.Dispatcher.Invoke((Action)(() =>
                {
                    counter++;
                    count.Text = counter.ToString();
                    StatusLbl.Content = "odebrano pakiet" + BitConverter.ToString(buffer);
                }));
            }
            else if (buffer.SequenceEqual(down))
            {
                this.Dispatcher.Invoke((Action)(() =>
                {
                    counter--;
                    count.Text = counter.ToString();
                    StatusLbl.Content = "odebrano pakiet" + BitConverter.ToString(buffer);
                }));
            }
            else
            {
                this.Dispatcher.Invoke((Action)(() =>
                {
                    StatusLbl.Content = "nie rozpoznano pakietu, " + buffer.Length.ToString() + BitConverter.ToString(buffer);
                }));
            }*/
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (serialPort.IsOpen)
            {
                _continue = false;
                readThread.Join();
                serialPort.Close();
            }
        }

        private void ResetBtn_Click(object sender, RoutedEventArgs e)
        {
            counter = 0;
            count.Text = counter.ToString();
        }

        private void RefreshBtn_Click(object sender, RoutedEventArgs e)
        {
            DataContext = new DataCont();
            PortsBox.GetBindingExpression(ComboBox.ItemsSourceProperty)
                      .UpdateTarget();
        }
    }
}
