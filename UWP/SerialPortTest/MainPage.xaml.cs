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
using System.Text;

// 空白ページの項目テンプレートについては、https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x411 を参照してください

namespace SerialPortTest
{
    /// <summary>
    /// Serial Port Property Record
    /// </summary>
    public struct VirtualSerialPortProperty
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    /// <summary>
    /// Main Page class (Partial)
    /// </summary>
    /// <seealso cref="Windows.UI.Xaml.Controls.Page" />
    /// <seealso cref="Windows.UI.Xaml.Markup.IComponentConnector" />
    /// <seealso cref="Windows.UI.Xaml.Markup.IComponentConnector2" />
    public sealed partial class MainPage : Page
    {
        private ObservableCollection<VirtualSerialPortProperty> xPortList;
        private VirtualSerialPortProperty[] xPortPropertyList;
        private SerialDevice SelectedSerialDevice;
        DataWriter dataWriteObject = null;
        DataReader dataReaderObject = null;

        private ObservableCollection<DeviceInformation> listOfDevices;
        private CancellationTokenSource ReadCancellationTokenSource;

//-----Internal Routines
        /// <summary>
        /// Creates the user component.
        /// </summary>
        private void CreateUserComponent()
        {
            this.xPortList = new ObservableCollection<VirtualSerialPortProperty>();
            this.xPortPropertyList = new VirtualSerialPortProperty[16];
        }

        /// <summary>
        /// Initializes the user component.
        /// </summary>
        private void InitializeUserComponent()
        {
            this.xPortList.Clear();
            for (int xLP = 0; xLP < 16; xLP++)
            {
                this.xPortPropertyList[xLP].Id = "";
                this.xPortPropertyList[xLP].Name = "";
            }
        }

        /// <summary>
        /// Gets the name of the serial port.
        /// </summary>
        private async void GetSerialPortName()
        {
            string selector;

            this.RefreshSerialPortProperty.IsEnabled = false;
            this.SerialPortNameCB.IsEnabled = false;
            if (this.TxText.Text != "")
            {
                this.TxText.Text = "";
            }
            if (this.RxText.Text != "")
            {
                this.RxText.Text = "";
            }
            CloseDevice();
            this.InitializeUserComponent();
            for (int xLP = 0; xLP < 16; xLP++)
            {
                selector = SerialDevice.GetDeviceSelector("COM" + xLP.ToString());
                DeviceInformationCollection xDeviceCollection = await DeviceInformation.FindAllAsync(selector);
                if (xDeviceCollection.Count > 0)
                {
                    this.xPortPropertyList[xLP].Id = xDeviceCollection[0].Id;
                    this.xPortPropertyList[xLP].Name = xDeviceCollection[0].Name;
                    this.xPortList.Add(this.xPortPropertyList[xLP]);
                }
            }
            SerialPortNameCB.DataContext = this.xPortList;
            this.RefreshSerialPortProperty.IsEnabled = true;
            this.SerialPortNameCB.IsEnabled = true;
        }

        /// <summary>
        /// Closes the device.
        /// </summary>
        private void CloseDevice()
        {
            if (SelectedSerialDevice != null)
            {
                SelectedSerialDevice.Dispose();
                SelectedSerialDevice = null;
                this.TxText.IsEnabled = false;
                this.RxText.IsEnabled = false;
            }
        }

        /// <summary>
        /// Writes the Text to serial port.
        /// </summary>
        /// <param name="TxText">The Tx text.</param>
        /// <returns></returns>
        private async Task WriteTextAsync(string TxTextString)
        {
            Task<UInt32> storeAsyncTask;

            if (TxTextString.Length != 0)
            {
                dataWriteObject.WriteString(TxTextString);  // Load the text from the sendText input text box to the dataWriter object
                storeAsyncTask = dataWriteObject.StoreAsync().AsTask(); // Launch an async task to complete the write operation
                 UInt32 bytesWritten = await storeAsyncTask;
                if (bytesWritten > 0)
                {
                    Encoding xEnc = Encoding.GetEncoding("us-ascii");
                    int xSize = xEnc.GetByteCount(TxTextString);
                    status.Text = xSize.ToString();
                    status.Text += " bytes written successfully!";
                }
            }
            else
            {
                status.Text = "Enter the text you want to write and then click on 'WRITE'";
            }
        }

        /// <summary>
        /// Reads the text asynchronous.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        private async Task ReadTextAsync(CancellationToken cancellationToken)
        {
            Task<UInt32> loadAsyncTask;

            uint ReadBufferLength = 1024;

            cancellationToken.ThrowIfCancellationRequested();   // If task cancellation was requested, comply
            dataReaderObject.InputStreamOptions = InputStreamOptions.Partial;   // Set InputStreamOptions to complete the asynchronous read operation when one or more bytes is available
            using (var childCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
            {
                loadAsyncTask = dataReaderObject.LoadAsync(ReadBufferLength).AsTask(childCancellationTokenSource.Token);    // Create a task object to wait for data on the serialPort.InputStream
                 UInt32 bytesRead = await loadAsyncTask; // Launch the task and wait
                if (bytesRead > 0)
                {
                    RxText.Text += dataReaderObject.ReadString(bytesRead);
                    status.Text = bytesRead.ToString() + " bytes read successfully!";
                }
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
        /// Listens the serial port.
        /// </summary>
        private async void ListenSerialPort()
        {
            try
            {
                if (SelectedSerialDevice != null)
                {
                    dataReaderObject = new DataReader(SelectedSerialDevice.InputStream);
                    while (true)    // keep reading the serial input
                    {
                        await ReadTextAsync(ReadCancellationTokenSource.Token);
                    }
                }
            }
            catch (TaskCanceledException tce)
            {
                status.Text = "Reading task was cancelled, closing device and cleaning up";
                CloseDevice();
            }
            catch (Exception ex)
            {
                status.Text = ex.Message;
            }
            finally
            {
                if (dataReaderObject != null)   // Cleanup once complete
                {
                    dataReaderObject.DetachStream();
                    dataReaderObject = null;
                }
            }
        }
//-----

        //-----Class Method
        /// <summary>
        /// Initializes a new instance of the <see cref="MainPage"/> class.
        /// </summary>
        public MainPage()
        {
            this.CreateUserComponent();
            this.InitializeUserComponent();
            this.InitializeComponent();
            this.SerialPortNameCB.IsEnabled = false;
            this.TxText.IsEnabled = false;
            this.RxText.IsEnabled = false;
        }

        /// <summary>
        /// Serials the port name combo box drop down closed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private async void SerialPortNameCB_DropDownClosed(object sender, object e)
        {
            string CurrentPortID;

            CloseDevice();
            CurrentPortID = SelectedPortName.Text.ToString();
            SelectedSerialDevice = await SerialDevice.FromIdAsync(CurrentPortID);
            if (SelectedSerialDevice != null)
            {
//                await new MessageDialog("Success, created serial device.").ShowAsync();   //Debug Message
                RxText.Text = "";    // Set the RxText to invoke the TextChanged callback
                ReadCancellationTokenSource = new CancellationTokenSource();    // Create cancellation token object to close I/O operations when closing the device
                ListenSerialPort();
                this.TxText.IsEnabled = true;
                this.RxText.IsEnabled = true;
            }
            else
            {
                await new MessageDialog("Oops, can't create serial device.").ShowAsync();
            }
        }

        /// <summary>
        /// Handles the Click event of the refresh Button control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            GetSerialPortName();
        }

        /// <summary>
        /// Handles the TextChanged event of the TxText control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="TextChangedEventArgs"/> instance containing the event data.</param>
        private async void TxText_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (this.TxText.Text.Length >= 1)
            {
                try
                {
                    if (SelectedSerialDevice != null)
                    {
                        string TxTextString = this.TxText.Text.Substring((this.TxText.Text.Length - 1), 1);
                        dataWriteObject = new DataWriter(SelectedSerialDevice.OutputStream);    // Create the DataWriter object and attach to OutputStream
                        await WriteTextAsync(TxTextString);   //Launch the WriteAsync task to perform the write
                    }
                    else
                    {
                        await new MessageDialog("Oops, can't find serial device.").ShowAsync();
                    }
                }
                catch (Exception ex)
                {
                    await new MessageDialog("Oops, on trying to write to  serial device. " + ex.Message).ShowAsync();
                }
                finally
                {
                    if (dataWriteObject != null)
                    {
                        dataWriteObject.DetachStream();
                        dataWriteObject = null;     // Cleanup once complete
                    }
                }

            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            TxText.Text = "";
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            RxText.Text = "";
        }
    }
//-----
}
