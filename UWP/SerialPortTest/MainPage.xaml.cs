using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Collections.ObjectModel;
using System.Collections;
using System.Threading.Tasks;
using System.Threading;
using Windows.Devices.SerialCommunication;
using Windows.Devices.Enumeration;
using Windows.Storage.Streams;
using Windows.UI.Popups;


// 空白ページの項目テンプレートについては、https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x411 を参照してください

namespace SerialPortTest
{
    /// <summary>
    /// それ自体で使用できる空白ページまたはフレーム内に移動できる空白ページ。
    /// </summary>

    public struct VirtualSerialPortProperty
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public sealed partial class MainPage : Page
    {
        private ObservableCollection<VirtualSerialPortProperty> xPortList;
        private VirtualSerialPortProperty xPortItem;
        private List<string> xStringListID;
        private List<string> xStringListName;

        public MainPage()
        {
            this.InitializeComponent();

            this.xPortList = new ObservableCollection<VirtualSerialPortProperty>();
            this.xPortItem = new VirtualSerialPortProperty();
            this.xStringListID = new List<string>();
            this.xStringListName = new List<string>();
            this.xPortList.Clear();
            for (int xLP = 0; xLP <= 15; xLP++)
            {
                this.xStringListID.Add("");
                this.xStringListName.Add("");
            }
        }

        private async void SerialPortNameCB_DropDownOpened(object sender, object e)
        {
            string selector;
            int xCount;

            this.xPortList.Clear();
            for (int xLP = 0; xLP <= 15; xLP++)
            {
                this.xStringListID[xLP] = "";
                this.xStringListName[xLP] = "";
                this.xPortItem.Id = "";
                this.xPortItem.Name = "";
               selector = SerialDevice.GetDeviceSelector("COM" + xLP.ToString());
                DeviceInformationCollection xDeviceCollection = await DeviceInformation.FindAllAsync(selector);
                if (xDeviceCollection.Count > 0)
                {
                    this.xStringListID[xLP] = xDeviceCollection[0].Id;
                    this.xStringListName[xLP] = xDeviceCollection[0].Name;
                }
            }
            xCount = 0;
            for (int xLP = 0; xLP <= 15; xLP++)
            {
                if (xStringListID[xLP] != "")
                {
                    this.xPortItem.Id = xStringListID[xLP];
                    this.xPortItem.Name = xStringListName[xLP];
                    this.xPortList.Add(this.xPortItem);
                    xCount++;
                }
            }
            SerialPortNameCB.DataContext = this.xPortList;
        }

        private async void SerialPortNameCB_DropDownClosed(object sender, object e)
        {
            string CurrentPortID;
            string xID, xName, xPortName;
            uint xBaudRate;
            ushort xDataBit;

            xID = "";
            xName = "";
            xPortName = "";
            xBaudRate = 0;
            xDataBit = 0;

            CurrentPortID = SelectedPortName.Text.ToString();
            SerialDevice SelectedSerialDevice = await SerialDevice.FromIdAsync(CurrentPortID);
            if (SelectedSerialDevice != null)
            {
                xBaudRate = SelectedSerialDevice.BaudRate;
                xDataBit = SelectedSerialDevice.DataBits;
                xPortName = SelectedSerialDevice.PortName;
                SelectedSerialDevice.Dispose();
                SelectedSerialDevice = null;

            }
            else
            {
                xBaudRate = 0;
                xDataBit = 0;
                await new MessageDialog("Oops, can't create serial device.").ShowAsync();
            }
            SelectedPortProperty.Text = "ID: " + CurrentPortID  + "\r\n"
                                      + "port name: " + xPortName.ToString() + "\r\n"
                                      + "baud rate: " + xBaudRate.ToString() + "\r\n"
                                      + "data bit: " + xDataBit.ToString();
        }
    }
}
