using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using System.Management;

namespace SerialPortTest
{
    /// <summary>
    /// Serial Device Property structure Definition
    /// </summary>
    public struct PropertySerialDevice
    {
        public string PortName;
        public int BaudRate;
        public Parity Parity;
        public int DataBits;
        public StopBits StopBits;
        public Handshake Handshake;
    }

    /// <summary>
    /// Form1 as Main Form
    /// </summary>
    /// <seealso cref="System.Windows.Forms.Form" />
    public partial class Form1 : Form
    {
        public SerialPortProcessor xSerialDevice = null;
        private List<string> stringListPort;
        private bool xConnected;
        private CancellationTokenSource ReadCancellationTokenSource;
        private string RxText = "";
        private bool IsWriting = false;

        /// <summary>
        /// Initializes the port setting.
        /// </summary>
        private void Init_PortSetting()
        {
            xSerialDevice.BaudRate = 9600;
            xSerialDevice.DataBits = 8;
            xSerialDevice.Parity = Parity.None;
            xSerialDevice.StopBits = StopBits.One;
            xSerialDevice.Handshake = Handshake.None;
        }

        /// <summary>
        /// Gets the port setting.
        /// </summary>
        private void Get_PortSetting()
        {
            Form2 _Form2 = new Form2();
            _Form2.xPropertySerialDevice.BaudRate = xSerialDevice.BaudRate;
            _Form2.xPropertySerialDevice.DataBits = xSerialDevice.DataBits;
            _Form2.xPropertySerialDevice.Parity = xSerialDevice.Parity;
            _Form2.xPropertySerialDevice.StopBits = xSerialDevice.StopBits;
            _Form2.xPropertySerialDevice.Handshake = xSerialDevice.Handshake;
            _Form2.ShowDialog();
            xSerialDevice.BaudRate = _Form2.xPropertySerialDevice.BaudRate;
            xSerialDevice.DataBits = _Form2.xPropertySerialDevice.DataBits;
            xSerialDevice.Parity = _Form2.xPropertySerialDevice.Parity;
            xSerialDevice.StopBits = _Form2.xPropertySerialDevice.StopBits;
            xSerialDevice.Handshake = _Form2.xPropertySerialDevice.Handshake;
            _Form2.Dispose();
        }

        /// <summary>
        /// Connects the serial device.
        /// </summary>
        public void Connect_SerialDevice()
        {
            if (xSerialDevice.PortName != "")
            {
                if (xSerialDevice.Start() == 0)
                {
                    ReadCancellationTokenSource = new CancellationTokenSource();    // Create cancellation token object to close I/O operations when closing the device
                    xConnected = true;
                    button1.Text = "DISCONNECT";
                    Task.Run(() => ListenSerialPort(ReadCancellationTokenSource.Token));
                }
            }
        }

        /// <summary>
        /// Disconnects the serial device.
        /// </summary>
        public void Disconnect_SerialDevice()
        {
            if (xSerialDevice != null)
            {
                CancelReadTextTask();
                xSerialDevice.Close();
                xConnected = false;
                button1.Text = "CONNECT";
            }
        }

        /// <summary>
        /// Puts the transmit text asynchronous.
        /// </summary>
        /// <param name="TxText">The tx text.</param>
        public async void PutTransmitText_Async(string TxText)
        {
            byte[] TxData = System.Text.Encoding.ASCII.GetBytes(TxText);    //ASCII
//            byte[] TxData = System.Text.Encoding.GetEncoding("iso-2022-jp").GetBytes(TxText); //JIS
//            byte[] TxData = System.Text.Encoding.GetEncoding("shift_jis").GetBytes(TxText);   //Shift-JIS
//            byte[] TxData = System.Text.Encoding.GetEncoding("euc-jp").GetBytes(TxText);  //EUC
//            byte[] TxData = System.Text.Encoding.UTF8.GetBytes(TxText);   //utf-8
//            byte[] TxData = System.Text.Encoding.Unicode.GetBytes(TxText);    //unicode
            xSerialDevice.WriteData(TxData);
            int xSize = TxData.Length;
            textBox1.Text = "Write " + Convert.ToString(xSize) + " byte.";
        }

        /// <summary>
        /// Gets the received text asynchronous.
        /// </summary>
        /// <returns></returns>
        public async Task GetReceivedText_Async()
        {
            byte[] RxDataRaw, RxData;
            int xLength;

            RxDataRaw = xSerialDevice.ReceiveData();
            if (RxDataRaw[0] != 0)
            {
                xLength = RxDataRaw.Length - 1;
                RxData = new byte[RxDataRaw.Length - 1];
                for (int i = 0; i < xLength; i++)
                {
                    RxData[i] = RxDataRaw[i + 1];
                }
                RxText += System.Text.Encoding.ASCII.GetString(RxData);
            }
        }

        /// <summary>
        /// Cancels the read text task.
        /// </summary>
        private void CancelReadTextTask()
        {
            if (ReadCancellationTokenSource != null)
            {
                if (!ReadCancellationTokenSource.IsCancellationRequested)
                {
                    ReadCancellationTokenSource.Cancel();
                }
            }
        }

        /// <summary>
        /// Listen the serial port.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        private async void ListenSerialPort(CancellationToken cancellationToken)
        {
            try
            {
                if (xConnected == true)
                {
                    while (!(cancellationToken.IsCancellationRequested))    // keep reading the serial input
                    {
                        if (!this.IsWriting) 
                        {
                            await GetReceivedText_Async();
                            if (RxText != "")
                            {
                                int xSize = RxText.Length;
                                this.Invoke((MethodInvoker)(() => { richTextBox2.Text += RxText; }));
                                this.Invoke((MethodInvoker)(() => { textBox1.Text = "Read " + Convert.ToString(xSize) + " byte."; }));
                                RxText = "";
                            }
                        }
                        await Task.Delay(100);
                    }
                }
            }
            catch (TaskCanceledException tce)
            {
                MessageBox.Show("Reading task was cancelled.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Form1"/> class.
        /// </summary>
        public Form1()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Handles the Shown event of the Form1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void Form1_Shown(object sender, EventArgs e)
        {
            xSerialDevice = new SerialPortProcessor();
            stringListPort = new List<string>();
            xConnected = false;
            xSerialDevice.PortName = "";
            Init_PortSetting();
        }

        /// <summary>
        /// Handles the FormClosing event of the Form1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="FormClosingEventArgs"/> instance containing the event data.</param>
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (xSerialDevice != null)
            {
                xSerialDevice.Close();
            }
        }

        /// <summary>
        /// Handles the Click event of the portSettingsToolStripMenuItem control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void portSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Get_PortSetting();
        }

        /// <summary>
        /// Handles the DropDown event of the comboBox1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void comboBox1_DropDown(object sender, EventArgs e)
        {
            string xCaption;
            string xDeviceID;

            comboBox1.Items.Clear();
            stringListPort.Clear();

//.Net version - Port Number Only
/*
            string xPortName;
            xPortName = "";
            foreach (var p in SerialPort.GetPortNames())
            {
                xPortName = p;
                comboBox1.Items.Add(xPortName.ToString());
                stringListPort.Add(xPortName);
            }
*/

//WMI version - Caption + Port Number
 
            xCaption = "";
            xDeviceID = "";
            ManagementClass mcW32SerPort = new ManagementClass("Win32_SerialPort");
            foreach (ManagementObject port in mcW32SerPort.GetInstances())
            {
                xCaption = port.GetPropertyValue("Caption").ToString();
                xDeviceID = port.GetPropertyValue("DeviceID").ToString();
                comboBox1.Items.Add(xCaption);
                stringListPort.Add(xDeviceID);
            }

        }

        /// <summary>
        /// Handles the DropDownClosed event of the comboBox1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
        private void comboBox1_DropDownClosed(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex >= 0)
            {
                xSerialDevice.PortName = stringListPort[comboBox1.SelectedIndex];
            }
        }

        /// <summary>
        /// Handles the Click event of the button1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
        private void button1_Click(object sender, EventArgs e)
        {
            if (xConnected == false)
            {
                Connect_SerialDevice();
            }
            else
            {
                Disconnect_SerialDevice();
            }
        }

        /// <summary>
        /// Handles the Click event of the button2 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
        private void button2_Click(object sender, EventArgs e)
        {
            IsWriting = true;
            richTextBox1.Clear();
            textBox1.Text = "";
            IsWriting = false;
        }

        /// <summary>
        /// Handles the Click event of the button3 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
        private void button3_Click(object sender, EventArgs e)
        {
            IsWriting = true;
            richTextBox2.Clear();
            textBox1.Text = "";
            IsWriting = false;
        }

        /// <summary>
        /// Handles the KeyPress event of the richTextBox1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.KeyPressEventArgs" /> instance containing the event data.</param>
        private void richTextBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            string TxText;

            IsWriting = true;
            if (xConnected == true)
            {
                TxText = e.KeyChar.ToString();
                PutTransmitText_Async(TxText);
            }
            IsWriting = false;
        }
    }
}
