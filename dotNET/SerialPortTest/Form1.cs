using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using System.Threading;
using System.Management;

namespace SerialPortTest
{
    /// <summary>
    /// Serial Device Property record
    /// </summary>
    struct PropertySerialDevice
    {
        public string PortName;
        public int BaudRate;
        public Parity Parity;
        public int DataBits;
        public StopBits StopBits;
        public Handshake Handshake;
    }

    /// <summary>
    /// SerialPortTest Form Class
    /// </summary>
    /// <seealso cref="System.Windows.Forms.Form" />
    public partial class Form1 : Form
    {
        private SerialPortProcessor xSerialDevice = null;
        private PropertySerialDevice xPropertySerialDevice;
        private List<string> stringListPort;
        private bool xConnected;

        /// <summary>
        /// Connects the serial device.
        /// </summary>
        public void Connect_SerialDevice()
        {
            xSerialDevice.BaudRate = xPropertySerialDevice.BaudRate;
            xSerialDevice.DataBits = xPropertySerialDevice.DataBits;
//            xSerialDevice.BaudRate = (uint)xPropertySerialDevice.BaudRate;
//            xSerialDevice.DataBits = (byte)xPropertySerialDevice.DataBits;
            xSerialDevice.Parity = xPropertySerialDevice.Parity;
            xSerialDevice.StopBits = xPropertySerialDevice.StopBits;
            xSerialDevice.Handshake = xPropertySerialDevice.Handshake;
            if (xSerialDevice.PortName != "")
            {
                if (xSerialDevice.Start() == 0)
                {
                    xConnected = true;
                    button1.Text = "DISCONNECT";
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
                xSerialDevice.Close();
                xConnected = false;
                button1.Text = "CONNECT";
            }
        }

        /// <summary>
        /// Gets the recieved text.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        public void Get_RecievedText(byte[] buffer)
        {
            string RxText = System.Text.Encoding.ASCII.GetString(buffer);
            richTextBox2.AppendText(RxText);
        }

        /// <summary>
        /// Puts the transmitting text.
        /// </summary>
        public void Put_TransmittingText()
        {
            richTextBox1.SelectAll();
            string TxText = richTextBox1.SelectedText;
            //ASCII
            byte[] TxData = System.Text.Encoding.ASCII.GetBytes(TxText);
            //JIS
            //byte[] TxData = System.Text.Encoding.GetEncoding("iso-2022-jp").GetBytes(TxText);
            //Shift-JIS
            //byte[] TxData = System.Text.Encoding.GetEncoding("shift_jis").GetBytes(TxText);
            //EUC
            //byte[] TxData = System.Text.Encoding.GetEncoding("euc-jp").GetBytes(TxText);
            //utf-8
            //byte[] TxData = System.Text.Encoding.UTF8.GetBytes(TxText);
            //unicode
            //byte[] TxData = System.Text.Encoding.Unicode.GetBytes(TxText);
            xSerialDevice.WriteData(TxData);
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
            xSerialDevice.DataReceived += Get_RecievedText;
            stringListPort = new List<string>();
            xConnected = false;
            xSerialDevice.PortName = "";
            //ComboBox BaudRate
            comboBox2.Items.Add("2400");
            comboBox2.Items.Add("4800");
            comboBox2.Items.Add("9600");
            comboBox2.Items.Add("14400");
            comboBox2.Items.Add("19200");
            comboBox2.Items.Add("38400");
            comboBox2.Items.Add("57600");
            comboBox2.Items.Add("115200");
            comboBox2.Items.Add("128000");
            comboBox2.Items.Add("256000");
            comboBox2.SelectedIndex = 2;
            xPropertySerialDevice.BaudRate = 9600;
            //ComboBox DataBit
            comboBox3.Items.Add("4");
            comboBox3.Items.Add("5");
            comboBox3.Items.Add("6");
            comboBox3.Items.Add("7");
            comboBox3.Items.Add("8");
            comboBox3.SelectedIndex = 4;
            xPropertySerialDevice.DataBits = 8;
            //ComboBox Parity
            comboBox4.Items.Add("NONE");
            comboBox4.Items.Add("EVEN");
            comboBox4.Items.Add("ODD");
            comboBox4.Items.Add("Mark");
            comboBox4.Items.Add("Space");
            comboBox4.SelectedIndex = 0;
            xPropertySerialDevice.Parity = Parity.None;
            //ComboBox StopBit
            comboBox5.Items.Add("NONE");
            comboBox5.Items.Add("1");
            comboBox5.Items.Add("1.5");
            comboBox5.Items.Add("2");
            comboBox5.SelectedIndex = 1;
            xPropertySerialDevice.StopBits = StopBits.One;
            //ComboBox Handshake
            comboBox6.Items.Add("NONE");
            comboBox6.Items.Add("RTS");
            comboBox6.Items.Add("XOn/XOff");
            comboBox6.Items.Add("BOTH");
            comboBox6.SelectedIndex = 0;
            xPropertySerialDevice.Handshake = Handshake.None;
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
            string xPortName;
            xPortName = "";
            foreach (var p in SerialPort.GetPortNames())
            {
                xPortName = p;
                comboBox1.Items.Add(xPortName.ToString());
                stringListPort.Add(xPortName);
            }

            //WMI version - Caption + Port Number
/* 
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
*/
        }

        /// <summary>
        /// Handles the DropDownClosed event of the comboBox1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void comboBox1_DropDownClosed(object sender, EventArgs e)
        {
            xSerialDevice.PortName = stringListPort[comboBox1.SelectedIndex];
        }

        /// <summary>
        /// Handles the DropDownClosed event of the comboBox2 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void comboBox2_DropDownClosed(object sender, EventArgs e)
        {
            switch (comboBox2.SelectedIndex)
            {
                case 0:
                    xPropertySerialDevice.BaudRate = 2400;
                    break;
                case 1:
                    xPropertySerialDevice.BaudRate = 4800;
                    break;
                case 2:
                    xPropertySerialDevice.BaudRate = 9600;
                    break;
                case 3:
                    xPropertySerialDevice.BaudRate = 14400;
                    break;
                case 4:
                    xPropertySerialDevice.BaudRate = 19200;
                    break;
                case 5:
                    xPropertySerialDevice.BaudRate = 38400;
                    break;
                case 6:
                    xPropertySerialDevice.BaudRate = 57600;
                    break;
                case 7:
                    xPropertySerialDevice.BaudRate = 115200;
                    break;
                case 8:
                    xPropertySerialDevice.BaudRate = 128000;
                    break;
                case 9:
                    xPropertySerialDevice.BaudRate = 256000;
                    break;
            }
        }

        /// <summary>
        /// Handles the DropDownClosed event of the comboBox3 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void comboBox3_DropDownClosed(object sender, EventArgs e)
        {
            switch (comboBox3.SelectedIndex)
            {
                case 0:
                    xPropertySerialDevice.DataBits = 4;
                    break;
                case 1:
                    xPropertySerialDevice.DataBits = 5;
                    break;
                case 2:
                    xPropertySerialDevice.DataBits = 6;
                    break;
                case 3:
                    xPropertySerialDevice.DataBits = 7;
                    break;
                case 4:
                    xPropertySerialDevice.DataBits = 8;
                    break;
            }
        }

        /// <summary>
        /// Handles the DropDownClosed event of the comboBox4 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
        private void comboBox4_DropDownClosed(object sender, EventArgs e)
        {
            switch (comboBox4.SelectedIndex)
            {
                case 0:
                    xPropertySerialDevice.Parity = Parity.None;
                    break;
                case 1:
                    xPropertySerialDevice.Parity = Parity.Even;
                    break;
                case 2:
                    xPropertySerialDevice.Parity = Parity.Odd;
                    break;
                case 3:
                    xPropertySerialDevice.Parity = Parity.Mark;
                    break;
                case 4:
                    xPropertySerialDevice.Parity = Parity.Space;
                    break;
            }

        }

        /// <summary>
        /// Handles the DropDownClosed event of the comboBox5 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
        private void comboBox5_DropDownClosed(object sender, EventArgs e)
        {
            switch (comboBox5.SelectedIndex)
            {
                case 0:
                    xPropertySerialDevice.StopBits = StopBits.None;
                    break;
                case 1:
                    xPropertySerialDevice.StopBits = StopBits.One;
                    break;
                case 2:
                    xPropertySerialDevice.StopBits = StopBits.OnePointFive;
                    break;
                case 3:
                    xPropertySerialDevice.StopBits = StopBits.Two;
                    break;
            }

        }

        /// <summary>
        /// Handles the DropDownClosed event of the comboBox6 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
        private void comboBox6_DropDownClosed(object sender, EventArgs e)
        {
            switch (comboBox6.SelectedIndex)
            {
                case 0:
                    xPropertySerialDevice.Handshake = Handshake.None;
                    break;
                case 1:
                    xPropertySerialDevice.Handshake = Handshake.RequestToSend;
                    break;
                case 2:
                    xPropertySerialDevice.Handshake = Handshake.XOnXOff;
                    break;
                case 3:
                    xPropertySerialDevice.Handshake = Handshake.RequestToSendXOnXOff;
                    break;
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
            Put_TransmittingText();
        }

        /// <summary>
        /// Handles the Click event of the button3 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
        private void button3_Click(object sender, EventArgs e)
        {
            richTextBox2.Clear();
        }

        /// <summary>
        /// Handles the Click event of the button4 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
        private void button4_Click(object sender, EventArgs e)
        {
            /*
                        byte[] buffer = new byte[1];
                        int RxData = xSerialDevice.ByteRead();
                        if (RxData != -1)
                        {
                            buffer[0] = (byte)RxData;
                            string RxText = System.Text.Encoding.ASCII.GetString(buffer);
                            richTextBox2.AppendText(RxText);
                        }
            */
        }
    }
}
