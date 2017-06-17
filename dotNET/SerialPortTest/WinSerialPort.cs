﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices; // DLL Import
using System.Collections.Specialized; // BitVector32
using System.IO.Ports; // SerialhComPort Object
using System.Windows.Forms;
using System.Windows;

namespace SerialPortTest
{
    /// <summary>
    /// Serial Port class of Win32 API
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    class WinSerialPort : IDisposable
    {
        #region Win32 API 

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr CreateFile(
        string lpFileName, // File Name 
        DesiredAccess dwDesiredAccess, // Access Mode 
        ShareMode dwShareMode, // Shared Mode 
        int lpSecurityAttributes, // Security Descripter 
        CreationDisposition dwCreationDisposition, // Creation
        FlagsAndAttributes dwFlagsAndAttributes, // Attribute 
        IntPtr hTemplateFile // Handle 
        );

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll")]
        static extern bool GetCommState(IntPtr hFile, ref DCB lpDCB);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool SetCommState(IntPtr hFile, ref DCB lpDCB);

        [DllImport("kernel32.dll", SetLastError = true)]
        //static extern bool ReadFile(IntPtr hFile, [Out] byte[] lpBuffer, uint nNumberOfBytesToRead, out uint lpNumberOfBytesRead, IntPtr lpOverlapped);
        static extern bool ReadFile(IntPtr hFile, [Out] byte[] lpBuffer, uint nNumberOfBytesToRead, out uint lpNumberOfBytesRead, int lpOverlapped);

        [DllImport("kernel32.dll")]
        //static extern bool WriteFile(IntPtr hFile, byte[] lpBuffer, uint nNumberOfBytesToWrite, out uint lpNumberOfBytesWritten, [In] ref NativeOverlapped lpOverlapped);
        static extern bool WriteFile(IntPtr hFile, byte[] lpBuffer, uint nNumberOfBytesToWrite, out uint lpNumberOfBytesWritten, int lpOverlapped);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool GetCommTimeouts(IntPtr hFile, ref COMMTIMEOUTS lpCommTimeouts);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool SetCommTimeouts(IntPtr hFile, [In] ref COMMTIMEOUTS lpCommTimeouts);

        [DllImport("kernel32.dll")]
        static extern bool ClearCommError(IntPtr hFile, out uint lpErrors, out LPCOMSTAT lpStat);

        [DllImport("kernel32.dll")]
        static extern bool SetupComm(IntPtr hFile, uint dwInQueue, uint dwOutQueue);

        [DllImport("kernel32.dll")]
        static extern bool PurgeComm(IntPtr hFile, Purge dwFlags);
        #endregion
        #region record type 

        private enum DesiredAccess : uint
        {
            GENERIC_READ = 0x80000000,
            GENERIC_WRITE = 0x40000000,
            GENERIC_EXECUTE = 0x20000000
        }

        private enum ShareMode : uint
        {
            FILE_SHARE_READ = 0x00000001,
            FILE_SHARE_WRITE = 0x00000002,
            FILE_SHARE_DELETE = 0x00000004
        }

        private enum CreationDisposition : uint
        {
            CREATE_NEW = 1,
            CREATE_ALWAYS = 2,
            OPEN_EXISTING = 3,
            OPEN_ALWAYS = 4,
            TRUNCATE_EXISTING = 5
        }

        private enum FlagsAndAttributes : uint
        {
            FILE_ATTRIBUTE_ARCHIVE = 0x00000020,
            FILE_ATTRIBUTE_ENCRYPTED = 0x00004000,
            FILE_ATTRIBUTE_HIDDEN = 0x00000002,
            FILE_ATTRIBUTE_NORMAL = 0x00000080,
            FILE_ATTRIBUTE_NOT_CONTENT_INDEXED = 0x00002000,
            FILE_ATTRIBUTE_OFFLINE = 0x00001000,
            FILE_ATTRIBUTE_READONLY = 0x00000001,
            FILE_ATTRIBUTE_SYSTEM = 0x00000004,
            FILE_ATTRIBUTE_TEMPORARY = 0x00000100
        }

        public enum DtrControl : int
        {
            /// <summary>
            /// Disables the DTR line when the device is opened and leaves it disabled.
            /// </summary>
            Disable = 0,

            /// <summary>
            /// Enables the DTR line when the device is opened and leaves it on.
            /// </summary>
            Enable = 1,

            /// <summary>
            /// Enables DTR handshaking. If handshaking is enabled, it is an error for the application to adjust the line by 
            /// using the EscapeCommFunction function.
            /// </summary>
            Handshake = 2
        }

        public enum RtsControl : int
        {
            /// <summary>
            /// Disables the RTS line when the device is opened and leaves it disabled.
            /// </summary>
            Disable = 0,

            /// <summary>
            /// Enables the RTS line when the device is opened and leaves it on.
            /// </summary>
            Enable = 1,

            /// <summary>
            /// Enables RTS handshaking. The driver raises the RTS line when the "type-ahead" (input) buffer 
            /// is less than one-half full and lowers the RTS line when the buffer is more than 
            /// three-quarters full. If handshaking is enabled, it is an error for the application to 
            /// adjust the line by using the EscapeCommFunction function.
            /// </summary>
            Handshake = 2,

            /// <summary>
            /// Specifies that the RTS line will be high if bytes are available for transmission. After 
            /// all buffered bytes have been sent, the RTS line will be low.
            /// </summary>
            Toggle = 3
        }

        public enum Purge : uint
        {
            TxAbort = 1,
            ExAbort = 2,
            TxClear = 4,
            RxClear = 8,
        }
        #endregion
        #region "record type"
        struct LPCOMSTAT
        {
            public UInt32 fCtsHold;
            public UInt32 fDsrHold;
            public UInt32 fRlsdHold;
            public UInt32 fXoffHold;
            public UInt32 fXoffSent;
            public UInt32 fEof;
            public UInt32 fTxim;
            public UInt32 fReserved;
            public UInt32 cbInQue;
            public UInt32 cbOutQue;
        }
        #endregion
        #region "record type"
        struct COMMTIMEOUTS
        {
            public UInt32 ReadIntervalTimeout;
            public UInt32 ReadTotalTimeoutMultiplier;
            public UInt32 ReadTotalTimeoutConstant;
            public UInt32 WriteTotalTimeoutMultiplier;
            public UInt32 WriteTotalTimeoutConstant;
        }
        #endregion
        #region "DCB record type"
        [StructLayout(LayoutKind.Sequential)]
        internal struct DCB
        {
            internal uint DCBLength;
            internal uint BaudRate;
            private BitVector32 Flags;

            private ushort wReserved; // not currently used 
            internal ushort XonLim; // transmit XON threshold 
            internal ushort XoffLim; // transmit XOFF threshold 

            internal byte ByteSize;
            internal Parity Parity;
            internal StopBits StopBits;

            internal sbyte XonChar; // Tx and Rx XON character 
            internal sbyte XoffChar; // Tx and Rx XOFF character 
            internal sbyte ErrorChar; // error replacement character 
            internal sbyte EofChar; // end of input character 
            internal sbyte EvtChar; // received event character 
            private ushort wReserved1; // reserved; do not use 

            private static readonly int fBinary;
            private static readonly int fParity;
            private static readonly int fOutxCtsFlow;
            private static readonly int fOutxDsrFlow;
            private static readonly BitVector32.Section fDtrControl;
            private static readonly int fDsrSensitivity;
            private static readonly int fTXContinueOnXoff;
            private static readonly int fOutX;
            private static readonly int fInX;
            private static readonly int fErrorChar;
            private static readonly int fNull;
            private static readonly BitVector32.Section fRtsControl;
            private static readonly int fAbortOnError;

            static DCB()
            {
                // Create Boolean Mask
                int previousMask;
                fBinary = BitVector32.CreateMask();
                fParity = BitVector32.CreateMask(fBinary);
                fOutxCtsFlow = BitVector32.CreateMask(fParity);
                fOutxDsrFlow = BitVector32.CreateMask(fOutxCtsFlow);
                previousMask = BitVector32.CreateMask(fOutxDsrFlow);
                previousMask = BitVector32.CreateMask(previousMask);
                fDsrSensitivity = BitVector32.CreateMask(previousMask);
                fTXContinueOnXoff = BitVector32.CreateMask(fDsrSensitivity);
                fOutX = BitVector32.CreateMask(fTXContinueOnXoff);
                fInX = BitVector32.CreateMask(fOutX);
                fErrorChar = BitVector32.CreateMask(fInX);
                fNull = BitVector32.CreateMask(fErrorChar);
                previousMask = BitVector32.CreateMask(fNull);
                previousMask = BitVector32.CreateMask(previousMask);
                fAbortOnError = BitVector32.CreateMask(previousMask);

                // Create section Mask
                BitVector32.Section previousSection;
                previousSection = BitVector32.CreateSection(1);
                previousSection = BitVector32.CreateSection(1, previousSection);
                previousSection = BitVector32.CreateSection(1, previousSection);
                previousSection = BitVector32.CreateSection(1, previousSection);
                fDtrControl = BitVector32.CreateSection(2, previousSection);
                previousSection = BitVector32.CreateSection(1, fDtrControl);
                previousSection = BitVector32.CreateSection(1, previousSection);
                previousSection = BitVector32.CreateSection(1, previousSection);
                previousSection = BitVector32.CreateSection(1, previousSection);
                previousSection = BitVector32.CreateSection(1, previousSection);
                previousSection = BitVector32.CreateSection(1, previousSection);
                fRtsControl = BitVector32.CreateSection(3, previousSection);
                previousSection = BitVector32.CreateSection(1, fRtsControl);
            }

            public bool Binary
            {
                get { return Flags[fBinary]; }
                set { Flags[fBinary] = value; }
            }

            public bool CheckParity
            {
                get { return Flags[fParity]; }
                set { Flags[fParity] = value; }
            }

            public bool OutxCtsFlow
            {
                get { return Flags[fOutxCtsFlow]; }
                set { Flags[fOutxCtsFlow] = value; }
            }

            public bool OutxDsrFlow
            {
                get { return Flags[fOutxDsrFlow]; }
                set { Flags[fOutxDsrFlow] = value; }
            }

            public DtrControl DtrControl
            {
                get { return (DtrControl)Flags[fDtrControl]; }
                set { Flags[fDtrControl] = (int)value; }
            }

            public bool DsrSensitivity
            {
                get { return Flags[fDsrSensitivity]; }
                set { Flags[fDsrSensitivity] = value; }
            }

            public bool TxContinueOnXoff
            {
                get { return Flags[fTXContinueOnXoff]; }
                set { Flags[fTXContinueOnXoff] = value; }
            }

            public bool OutX
            {
                get { return Flags[fOutX]; }
                set { Flags[fOutX] = value; }
            }

            public bool InX
            {
                get { return Flags[fInX]; }
                set { Flags[fInX] = value; }
            }

            public bool ReplaceErrorChar
            {
                get { return Flags[fErrorChar]; }
                set { Flags[fErrorChar] = value; }
            }

            public bool Null
            {
                get { return Flags[fNull]; }
                set { Flags[fNull] = value; }
            }

            public RtsControl RtsControl
            {
                get { return (RtsControl)Flags[fRtsControl]; }
                set { Flags[fRtsControl] = (int)value; }
            }

            public bool AbortOnError
            {
                get { return Flags[fAbortOnError]; }
                set { Flags[fAbortOnError] = value; }
            }
        }
        #endregion

        IntPtr hComPort { get; set; }
        public string PortName { get; set; }
        public uint BaudRate { get; set; }
        public Parity Parity { get; set; }
        public byte DataBits { get; set; }
        public StopBits StopBits { get; set; }
        public Handshake Handshake { get; set; }
        public byte[] RxBuffer { get; } = new byte[1];
        public bool IsOpen { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="WinSerialPort"/> class.
        /// </summary>
        /// <param name="portName">Name of the port.</param>
        /// <param name="baudRate">The baud rate.</param>
        /// <param name="parity">The parity.</param>
        /// <param name="dataBits">The data bits.</param>
        /// <param name="stopBits">The stop bits.</param>
        public WinSerialPort(string portName, uint baudRate, Parity parity, byte dataBits, StopBits stopBits)
        {
            this.PortName = portName;
            this.BaudRate = baudRate;
            this.Parity = parity;
            this.DataBits = dataBits;
            this.StopBits = stopBits;
            this.Handshake = Handshake.None;
            this.IsOpen = false;
        }

        /// <summary>
        /// Opens this instance.
        /// </summary>
        /// <returns></returns>
        public bool Open()
        {
            hComPort = CreateFile(PortName, DesiredAccess.GENERIC_READ | DesiredAccess.GENERIC_WRITE, 0, 0, CreationDisposition.OPEN_EXISTING, FlagsAndAttributes.FILE_ATTRIBUTE_NORMAL, IntPtr.Zero);

            //Open port
            if (0 >= hComPort.ToInt32())
                return false;

            //Setting Communication
            var dcb = new DCB();
            dcb.DCBLength = (uint)Marshal.SizeOf(dcb);
            if (!GetCommState(hComPort, ref dcb))
                return false;
            dcb.BaudRate = BaudRate;
            dcb.ByteSize = DataBits;
            dcb.Parity = Parity;
            dcb.StopBits = StopBits;
            dcb.XonChar = 0x11;
            dcb.XoffChar = 0x13;
            dcb.Binary = true;
            dcb.CheckParity = true;
            dcb.RtsControl = RtsControl.Disable;
            dcb.DtrControl = DtrControl.Disable;
            SetCommState(hComPort, ref dcb);
/*
            if (!SetCommState(port, ref dcb))
            {
                int errCode = Marshal.GetLastWin32Error();
                MessageBox.Show("SetCommStateに失敗しました。" + errCode.ToString(), "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            On Windows10, SetCommState returns - 1, GetLastError = 87
*/

            //Initializes the communications parameters
            if (!SetupComm(hComPort, 1024, 1024))
            {
                return false;
            }

            //Sets the time-out parameters
            var Commtimeouts = new COMMTIMEOUTS();
            if (!GetCommTimeouts(hComPort, ref Commtimeouts))
            {
                return false;
            }

            Commtimeouts.ReadIntervalTimeout = 100;

            if (!SetCommTimeouts(hComPort, ref Commtimeouts))
            {
                return false;
            }

            this.IsOpen = true;
            return true;
        }

        /// <summary>
        /// Closes this instance.
        /// </summary>
        public void Close()
        {
            if (hComPort != IntPtr.Zero)
            {
                DiscardInBuffer();
                DiscardOutBuffer();
                Dispose();
            }
        }

        /// <summary>
        /// Writes the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="size">The size.</param>
        public void Write(byte[] buffer, int offset, int size)
        {
            uint writen = 0;
            WriteFile(hComPort, buffer, (uint)size, out writen, 0);
        }

        /// <summary>
        /// Reads the byte.
        /// </summary>
        /// <returns></returns>
        public int ReadByte()   //return read byte, RxD -> RxBuffer
        {
            uint RxBufferSize = 0;
            uint pError;
            LPCOMSTAT pStat;

            if (ClearCommError(hComPort, out pError, out pStat))
            {
                if (pStat.fDsrHold > 0)
                {
                    if (ReadFile(hComPort, RxBuffer, 1, out RxBufferSize, 0))
                    {
                        if (RxBufferSize > 0)
                        {
                            return (int)RxBufferSize;
                        }
                    }
                }
            }
            return -1;
        }

        /// <summary>
        /// Discards the in buffer.
        /// </summary>
        public void DiscardInBuffer()
        {
            PurgeComm(hComPort, Purge.RxClear);
        }

        /// <summary>
        /// Discards the out buffer.
        /// </summary>
        public void DiscardOutBuffer()
        {
            PurgeComm(hComPort, Purge.TxClear);
        }

        /// <summary>
        /// アンマネージ リソースの解放またはリセットに関連付けられているアプリケーション定義のタスクを実行します。
        /// </summary>
        public void Dispose()
        {
            CloseHandle(hComPort);
            hComPort = IntPtr.Zero;
            this.IsOpen = false;
        }

    }
}
