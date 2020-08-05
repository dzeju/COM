using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Ports;

namespace COM
{
    class ComboBoxItems
    {
        public List<string> ports {get; set;}
        public List<int> baudRate { get; set; }
        public List<Parity> parity { get; set; }
        public List<int> dataBits { get; set; }
        public List<StopBits> stopBits { get; set; }

        public ComboBoxItems()
        {
            ports = new List<string>();
            ports.Clear();
            foreach (string name in SerialPort.GetPortNames())
            {
                ports.Add(name);
            }
            
            baudRate = new List<int>()
            {
                1200,
                2400,
                4800,
                9600,
                19200,
                38400,
                57600,
                115200
            };

            parity = new List<Parity>()
            {
                Parity.None,
                Parity.Odd,
                Parity.Even,
                Parity.Mark,
                Parity.Space
            };

            dataBits = new List<int>()
            {
                7,8,9
            };

            stopBits = new List<StopBits>()
            {
                StopBits.None,
                StopBits.One,
                StopBits.OnePointFive,
                StopBits.Two
            };
        }
    }
}
