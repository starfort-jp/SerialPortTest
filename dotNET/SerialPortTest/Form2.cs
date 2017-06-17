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

namespace SerialPortTest
{
    public partial class Form2 : Form
    {
        public PropertySerialDevice xPropertySerialDevice;
      
        public Form2()
        {
            InitializeComponent();
            //ComboBox BaudRate
            comboBox1.Items.Add("2400");
            comboBox1.Items.Add("4800");
            comboBox1.Items.Add("9600");
            comboBox1.Items.Add("14400");
            comboBox1.Items.Add("19200");
            comboBox1.Items.Add("38400");
            comboBox1.Items.Add("57600");
            comboBox1.Items.Add("115200");
            comboBox1.Items.Add("128000");
            comboBox1.Items.Add("256000");
            //ComboBox DataBit
            comboBox2.Items.Add("4");
            comboBox2.Items.Add("5");
            comboBox2.Items.Add("6");
            comboBox2.Items.Add("7");
            comboBox2.Items.Add("8");
            comboBox2.SelectedIndex = 4;
            //ComboBox Parity
            comboBox3.Items.Add("NONE");
            comboBox3.Items.Add("EVEN");
            comboBox3.Items.Add("ODD");
            comboBox3.Items.Add("Mark");
            comboBox3.Items.Add("Space");
            comboBox3.SelectedIndex = 0;
            //ComboBox StopBits
            comboBox4.Items.Add("NONE");
            comboBox4.Items.Add("1");
            comboBox4.Items.Add("1.5");
            comboBox4.Items.Add("2");
            comboBox4.SelectedIndex = 1;
            //ComboBox Handshake
            comboBox5.Items.Add("NONE");
            comboBox5.Items.Add("RTS");
            comboBox5.Items.Add("XOn/XOff");
            comboBox5.Items.Add("BOTH");
            comboBox5.SelectedIndex = 0;
        }

        private void Form2_Shown(object sender, EventArgs e)
        {
            switch (xPropertySerialDevice.BaudRate)
            {
                case 2400:
                    comboBox1.SelectedIndex = 0;
                    break;
                case 4800:
                    comboBox1.SelectedIndex = 1;
                    break;
                case 9600:
                    comboBox1.SelectedIndex = 2;
                    break;
                case 14400:
                    comboBox1.SelectedIndex = 3;
                    break;
                case 19200:
                    comboBox1.SelectedIndex = 4;
                    break;
                case 38400:
                    comboBox1.SelectedIndex = 5;
                    break;
                case 57600:
                    comboBox1.SelectedIndex = 6;
                    break;
                case 115200:
                    comboBox1.SelectedIndex = 7;
                    break;
                case 128000:
                    comboBox1.SelectedIndex = 8;
                    break;
                case 256000:
                    comboBox1.SelectedIndex = 9;
                    break;
            }
            switch (xPropertySerialDevice.DataBits)
            {
                case 4:
                    comboBox2.SelectedIndex = 0;
                    break;
                case 5:
                    comboBox2.SelectedIndex = 1;
                    break;
                case 6:
                    comboBox2.SelectedIndex = 2;
                    break;
                case 7:
                    comboBox2.SelectedIndex = 3;
                    break;
                case 8:
                    comboBox2.SelectedIndex = 4;
                    break;
            }
            switch (xPropertySerialDevice.Parity)
            {
                case Parity.None:
                    comboBox3.SelectedIndex = 0;
                    break;
                case Parity.Even:
                    comboBox3.SelectedIndex = 1;
                    break;
                case Parity.Odd:
                    comboBox3.SelectedIndex = 2;
                    break;
                case Parity.Mark:
                    comboBox3.SelectedIndex = 3;
                    break;
                case Parity.Space:
                    comboBox3.SelectedIndex = 4;
                    break;
            }
            switch (xPropertySerialDevice.StopBits)
            {
                case StopBits.None:
                    comboBox4.SelectedIndex = 0;
                    break;
                case StopBits.One:
                    comboBox4.SelectedIndex = 1;
                    break;
                case StopBits.OnePointFive:
                    comboBox4.SelectedIndex = 2;
                    break;
                case StopBits.Two:
                    comboBox4.SelectedIndex = 3;
                    break;
            }
            switch (xPropertySerialDevice.Handshake)
            {
                case Handshake.None:
                    comboBox5.SelectedIndex = 0;
                    break;
                case Handshake.RequestToSend:
                    comboBox5.SelectedIndex = 1;
                    break;
                case Handshake.XOnXOff:
                    comboBox5.SelectedIndex = 2;
                    break;
                case Handshake.RequestToSendXOnXOff:
                    comboBox5.SelectedIndex = 3;
                    break;
            }
        }

        private void comboBox1_DropDownClosed(object sender, EventArgs e)
        {
            switch (comboBox1.SelectedIndex)
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

        private void comboBox2_DropDownClosed(object sender, EventArgs e)
        {
            switch (comboBox2.SelectedIndex)
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

        private void comboBox3_DropDownClosed(object sender, EventArgs e)
        {
            switch (comboBox3.SelectedIndex)
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

        private void comboBox4_DropDownClosed(object sender, EventArgs e)
        {
            switch (comboBox4.SelectedIndex)
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

        private void comboBox5_DropDownClosed(object sender, EventArgs e)
        {
            switch (comboBox5.SelectedIndex)
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
    }
}
