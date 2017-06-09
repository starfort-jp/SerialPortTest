using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Windows.Forms;

namespace SerialPortTest
{
    /// <summary>
    /// 
    /// </summary>
    class SerialPortProcessor
    {
//        private SerialPort xSerialPort = null;
        private WinSerialPort xSerialPort = null;
        private Thread receiveThread = null;

        public String PortName { get; set; }
        public uint BaudRate { get; set; }
        public Parity Parity { get; set; }
        public byte DataBits { get; set; }
        public StopBits StopBits { get; set; }
        public Handshake Handshake { get; set; }

        public SerialPortProcessor()
        {
        }

        public int Start()
        {
            if (xSerialPort != null && xSerialPort.IsOpen == true)
            {
                xSerialPort.Close();
            }
            if (xSerialPort == null)
            {
//                xSerialPort = new SerialPort(PortName, BaudRate, Parity, DataBits, StopBits);
                xSerialPort = new WinSerialPort(PortName, BaudRate, Parity, DataBits, StopBits);
            }
            try
            {
                /*  //typical settings
                xSerialPort.BaudRate = 9600;
                xSerialPort.DataBits = 8;
                xSerialPort.DiscardNull = false;
                xSerialPort.DtrEnable = false;
                xSerialPort.Encoding = Encoding.UTF8;
                xSerialPort.Handshake = Handshake.None;
                xSerialPort.NewLine = Environment.NewLine;
                xSerialPort.Parity = Parity.None;
                xSerialPort.ParityReplace = 63;
                xSerialPort.PortName = "COM3";
                xSerialPort.ReadBufferSize = 4096;
                xSerialPort.ReadTimeout = -1;
                xSerialPort.ReceivedBytesThreshold = 1;
                xSerialPort.RtsEnable = false;
                xSerialPort.StopBits = StopBits.One;
                xSerialPort.WriteBufferSize = 2048;
                xSerialPort.WriteTimeout = -1;
                */
                xSerialPort.portName = PortName;
                xSerialPort.Open();
                receiveThread = new Thread(SerialPortProcessor.ReceiveWork);
                receiveThread.Start(this);
                return (0);
            }
            catch (IOException ex)
            {
                MessageBox.Show("ポートのオープン中、I/O 例外が発生しました。" + ex.ToString(), "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return (1);
            }
        }

        public static void ReceiveWork(object target)
        {
            SerialPortProcessor my = target as SerialPortProcessor;
            my.ReceiveData();
        }

        public void WriteData(byte[] buffer)
        {
            try
            {
                xSerialPort.Write(buffer, 0, buffer.Length);
            }
            catch (IOException ex)
            {
                MessageBox.Show("ポートへの書込み中、I/O 例外が発生しました。" + ex.ToString(), "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public delegate void DataReceivedHandler(byte[] data);
        public event DataReceivedHandler DataReceived;

        public void ReceiveData()
        {
            if (xSerialPort == null)
            {
                return;
            }
            do
            {
                try
                {
/*
                    int rbyte = xSerialPort.BytesToRead;
                    byte[] buffer = new byte[rbyte];
                    int read = 0;
                    while (read < rbyte)
                    {
                        int length = xSerialPort.Read(buffer, read, rbyte - read);
                        read += length;
                    }
                    if (rbyte > 0)
                    {
                        DataReceived(buffer);
                    }
*/

//                    int xReadByte = xSerialPort.ReadByte();
                    int xReadByte = 0;  //nothing to do, not implemented thread read
                    if (xReadByte > 0)
                    {
                        byte[] buffer = new byte[1];
                        buffer[0] = xSerialPort.recvBuffer[0];
                        DataReceived(buffer);
                    }
                }
                catch (IOException ex)
                {
                    MessageBox.Show("ポートからの読み込み中、I/O 例外が発生しました。" + ex.ToString(), "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (InvalidOperationException ex)
                {
                    MessageBox.Show("ポートからの読み込み中、不正処理例外が発生しました。" + ex.ToString(), "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            } while (xSerialPort.IsOpen);
        }

        public int ByteRead()
        {
            if (xSerialPort == null)
            {
                return (-1);
            }
            try
            {
                int xReadByte = xSerialPort.ReadByte();
                if (xReadByte > 0)
                {
                    int RxData = (int)xSerialPort.recvBuffer[0];
                    return (RxData);
                }
                return (-1);
            }
            catch (IOException ex)
            {
                MessageBox.Show("ポートからの読み込み中、I/O 例外が発生しました。" + ex.ToString(), "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return (-1);
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show("ポートからの読み込み中、不正処理例外が発生しました。" + ex.ToString(), "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return (-1);
            }
        }

        public void Close()
        {
            if (receiveThread != null && xSerialPort != null)
            {
//                xSerialPort.Close();
//                xSerialPort.DiscardInBuffer();
//                xSerialPort.DiscardOutBuffer();
                xSerialPort.Dispose();
                receiveThread.Join();
            }
        }
    }
}
