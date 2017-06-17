using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Ports;
using System.Windows.Forms;

namespace SerialPortTest
{
    /// <summary>
    /// Serial Port virtual class
    /// (Win32 API, includes .NET version as comment.)
    /// </summary>
    public class SerialPortProcessor
    {
//        private SerialPort xSerialPort = null;
        private WinSerialPort xSerialPort = null;

        public String PortName { get; set; }
        public int BaudRate { get; set; }
        public Parity Parity { get; set; }
        public int DataBits { get; set; }
        public StopBits StopBits { get; set; }
        public Handshake Handshake { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SerialPortProcessor"/> class.
        /// </summary>
        public SerialPortProcessor()
        {
        }

        /// <summary>
        /// Starts xSerialPort instance.
        /// </summary>
        /// <returns >Success = 0, Fail = 1</returns>
        public int Start()
        {
            if (xSerialPort != null)
            {
                xSerialPort.Close();
            }
            if (xSerialPort == null)
            {
//                xSerialPort = new SerialPort(PortName, BaudRate, Parity, DataBits, StopBits);
                uint _baudRate = (uint)BaudRate;
                byte _dataBits = (byte)DataBits;
                xSerialPort = new WinSerialPort(PortName, _baudRate, Parity, _dataBits, StopBits);
            }
            try
            {
                /*  //typical settings for override
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
                xSerialPort.PortName = PortName;
                xSerialPort.Open();
                return (0);
            }
            catch (IOException ex)
            {
                MessageBox.Show("ポートのオープン中、I/O 例外が発生しました。" + ex.ToString(), "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return (1);
            }
        }

        /// <summary>
        /// Closes xSerialPort instance.
        /// </summary>
        public void Close()
        {
            if (xSerialPort != null)
            {
                xSerialPort.Close();
            }
        }

        /// <summary>
        /// Writes the data.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
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
            catch (InvalidOperationException ex)
            {
                MessageBox.Show("ポートへの書込み中、不正命令例外が発生しました。" + ex.ToString(), "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Receives the data.
        /// </summary>
        public byte[] ReceiveData()
        {
            byte[] RxText;

            if (xSerialPort == null)
            {
                RxText = new byte[1];
                RxText[0] = 0;
                return RxText;
            }
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
                int xReadByte = xSerialPort.ReadByte();
                if (xReadByte > 0)
                {
                    RxText = new byte[xReadByte + 1];
                    RxText[0] = 1;
                    for (int i= 1; i < (xReadByte + 1); i++)
                    {
                        RxText[i] = xSerialPort.RxBuffer[i-1];
                    }
                    return RxText;
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
            RxText = new byte[1];
            RxText[0] = 0;
            return RxText;
        }
    }
}
