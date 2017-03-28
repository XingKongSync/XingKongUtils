using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace XingKongUtils
{
    /// <summary>
    /// 支持流量控制、丢失重发的串口通信类
    /// </summary>
    public class XKserialPort
    {
        private bool isopen;
        public bool IsOpen { get { return isopen; } set { } }

        private bool iserror;
        public bool IsError { get { return iserror; } set { } }

        #region WindowsAPI声明
        [StructLayout(LayoutKind.Sequential)]
        private struct COMMTIMEOUTS
        {
            public UInt32 ReadIntervalTimeout;
            public UInt32 ReadTotalTimeoutMultiplier;
            public UInt32 ReadTotalTimeoutConstant;
            public UInt32 WriteTotalTimeoutMultiplier;
            public UInt32 WriteTotalTimeoutConstant;
        }

        public const byte NOPARITY = 0;
        public const byte ONESTOPBIT = 0;
        public const uint PURGE_TXCLEAR = 0x0004;

        public struct DCB
        {
            //taken from c struct in platform sdk 
            public int DCBlength; // sizeof(DCB) 
            public int BaudRate; // current baud rate 
            /* these are the c struct bit fields, bit twiddle flag to set 
            public int fBinary; // binary mode, no EOF check 
            public int fParity; // enable parity checking 
            public int fOutxCtsFlow; // CTS output flow control 
            public int fOutxDsrFlow; // DSR output flow control 
            public int fDtrControl; // DTR flow control type 
            public int fDsrSensitivity; // DSR sensitivity 
            public int fTXContinueOnXoff; // XOFF continues Tx 
            public int fOutX; // XON/XOFF out flow control 
            public int fInX; // XON/XOFF in flow control 
            public int fErrorChar; // enable error replacement 
            public int fNull; // enable null stripping 
            public int fRtsControl; // RTS flow control 
            public int fAbortOnError; // abort on error 
            public int fDummy2; // reserved 
            */
            public uint flags;
            public ushort wReserved; // not currently used 
            public ushort XonLim; // transmit XON threshold 
            public ushort XoffLim; // transmit XOFF threshold 
            public byte ByteSize; // number of bits/byte, 4-8 
            public byte Parity; // 0-4=no,odd,even,mark,space 
            public byte StopBits; // 0,1,2 = 1, 1.5, 2 
            public char XonChar; // Tx and Rx XON character 
            public char XoffChar; // Tx and Rx XOFF character 
            public char ErrorChar; // error replacement character 
            public char EofChar; // end of input character 
            public char EvtChar; // received event character 
            public ushort wReserved1; // reserved; do not use 
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct OVERLAPPED
        {
            public int Internal;
            public int InternalHigh;
            public int Offset;
            public int OffsetHigh;
            public int hEvent;
        }

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern SafeFileHandle CreateFile(
            string lpFileName,
            [MarshalAs(UnmanagedType.U4)] FileAccess dwDesiredAccess,
            [MarshalAs(UnmanagedType.U4)] FileShare dwShareMode,
            IntPtr lpSecurityAttributes,
            [MarshalAs(UnmanagedType.U4)] FileMode dwCreationDisposition,
            [MarshalAs(UnmanagedType.U4)] FileAttributes dwFlagsAndAttributes,
            IntPtr hTemplateFile
         );

        [DllImport("kernel32.dll")]
        private static extern bool WriteFile(int hFile, byte[] lpBuffer, int nNumberOfBytesToWrite, ref int lpNumberOfBytesWritten, ref OVERLAPPED lpOverlapped);

        private IntPtr NULL = new IntPtr(0);

        [DllImport("kernel32")]
        private static extern bool SetupComm(
            int hFile,
            int dwInQueue,
            int dwOutQueue
        );

        [DllImport("kernel32")]
        private static extern bool SetCommTimeouts(
            int hFile,                  // handle to comm device 
            ref COMMTIMEOUTS lpCommTimeouts  // time-out values 
         );

        [DllImport("kernel32")]
        private static extern bool SetCommState(
            int hFile,  // handle to communications device 
            ref DCB lpDCB    // device-control block 
         );

        [DllImport("kernel32")]
        private static extern bool GetCommState(
            int hFile,  // handle to communications device 
            ref DCB lpDCB    // device-control block 
         );

        [DllImport("kernel32")]
        private static extern bool PurgeComm(
            int hFile,     // handle to file
            uint dwFlags
         );
        #endregion

        /// <summary>
        /// 初始化DCB
        /// </summary>
        /// <param name="hComm">串口的句柄</param>
        /// <param name="BaudRate">波特率</param>
        /// <param name="Parity">奇偶校验</param>
        /// <param name="StopBits">停止位</param>
        /// <returns>初始化后的DCB</returns>
        private DCB initDCB(int hComm, int BaudRate, byte Parity, byte StopBits, FlowControlType flowControl)
        {
            DCB dcbCommPort = new DCB();
            // SET BAUD RATE, PARITY, WORD SIZE, AND STOP BITS. 
            GetCommState(hComm, ref dcbCommPort);
            dcbCommPort.BaudRate = BaudRate;

            if (Parity > 0)
            {
                dcbCommPort.flags |= 2;
            }

            switch (flowControl)
            {
                case FlowControlType.Hardware:
                    //开启DTR、RTS、CTS和DSR
                    dcbCommPort.flags = 8237;
                    break;
                case FlowControlType.Software:
                    //开启XON/XOFF
                    dcbCommPort.flags &= 4294963200;
                    dcbCommPort.flags |= 897;
                    break;
                case FlowControlType.None:
                    //关闭DTR、RTS、CTS和DSR
                    dcbCommPort.flags = 1;
                    break;
                default:
                    break;
            }

            dcbCommPort.Parity = Parity;
            dcbCommPort.ByteSize = 8;
            dcbCommPort.StopBits = StopBits;
            return dcbCommPort;
        }

        private int BaudRate;//波特率
        private byte Parity;//校验
        private byte StopBits;//停止位
        private string PortName;//端口名
        private SafeFileHandle hPort;//串口句柄
        private OVERLAPPED overlapped;
        private FlowControlType FlowControl;//流量控制
        private uint WriteTotalTimeoutMultiplier;//平均每字节等待时间
        private uint WriteTotalTimeoutConstant;//全部发送完毕后的等待时间

        /// <summary>
        /// 流量控制的方式
        /// 共三种：硬件控制（DTR、RTS、CTS和DSR）、软件（XON/XOFF）和无流量控制
        /// </summary>
        public enum FlowControlType
        {
            Hardware,
            Software,
            None
        };

        /// <summary>
        /// 创建一个串口通信实例
        /// </summary>
        /// <param name="portName">串口名</param>
        /// <param name="baudRate">波特率</param>
        /// <param name="parity">校验，偶校验(2) 无校验(0) 奇校验(1)</param>
        /// <param name="stopBits">停止位，1停止位(0) 1.5停止位(1) 2停止位(2)</param>
        /// <param name="flowControl">流量控制类型</param>
        public XKserialPort(string portName, int baudRate = 9600, byte parity = 0, byte stopBits = 0, FlowControlType flowControl = FlowControlType.None)
        {
            this.BaudRate = baudRate;
            this.Parity = parity;
            this.StopBits = stopBits;
            this.PortName = portName;
            this.FlowControl = flowControl;
            overlapped = new OVERLAPPED();

            Open();//打开端口

            SetupComm((int)(hPort.DangerousGetHandle()), 1024, 1024);//设置端口缓冲区大小
            if (portName.ToUpper().IndexOf("COM") != -1)
            {
                //如果是串口
                SetTimeOuts();//设置默认超时

                //配置串口参数，波特率，无校验，1位停止位
                DCB dcb = initDCB((int)(hPort.DangerousGetHandle()), BaudRate, Parity, StopBits, flowControl);
                SetCommState((int)(hPort.DangerousGetHandle()), ref dcb);
            }
            //清空缓冲区
            PurgeComm((int)(hPort.DangerousGetHandle()), PURGE_TXCLEAR);

            //关闭端口
            hPort.Close();
        }

        /// <summary>
        /// 超时设置
        /// </summary>
        /// <param name="readIntervalTimeout">间隔超时</param>
        /// <param name="readTotalTimeoutMultiplier">总超时</param>
        /// <param name="readTotalTimeoutConstant">读数据总时间常量</param>
        /// <param name="writeTotalTimeoutMultiplier">平均写一个字节的时间上限</param>
        /// <param name="writeTotalTimeoutConstant">写数据总超时常量</param>
        /// <returns></returns>
        public bool SetTimeOuts(uint readIntervalTimeout = 500, uint readTotalTimeoutMultiplier = 0, uint readTotalTimeoutConstant = 0, uint writeTotalTimeoutMultiplier = 2, uint writeTotalTimeoutConstant = 500)
        {
            //在设置超时的时候，端口必须打开，如果程序没有先Open，则在此处自动Open，设置完超时后再关闭
            bool isClosed = hPort.IsClosed;
            if (isClosed)
            {
                Open();
            }
            WriteTotalTimeoutMultiplier = writeTotalTimeoutMultiplier;
            WriteTotalTimeoutConstant = writeTotalTimeoutConstant;

            //超时控制
            COMMTIMEOUTS TimeOuts;
            TimeOuts.ReadIntervalTimeout = readIntervalTimeout;
            TimeOuts.ReadTotalTimeoutMultiplier = readTotalTimeoutMultiplier;
            TimeOuts.ReadTotalTimeoutConstant = readTotalTimeoutConstant;
            TimeOuts.WriteTotalTimeoutMultiplier = writeTotalTimeoutMultiplier;
            TimeOuts.WriteTotalTimeoutConstant = writeTotalTimeoutConstant;
            bool result = SetCommTimeouts((int)(hPort.DangerousGetHandle()), ref TimeOuts);

            //如果原来端口是关闭的，现在处理完成之后关闭
            if (isClosed)
            {
                Close();
            }
            return result;
        }

        /// <summary>
        /// 打开串口
        /// </summary>
        /// <returns>True:打开成功 False:打开失败</returns>
        public bool Open()
        {
            hPort = CreateFile(PortName, FileAccess.ReadWrite, FileShare.None, NULL, FileMode.Open, FileAttributes.Normal, NULL);
            if (hPort.IsInvalid)
            {
                throw new IOException(PortName + "端口打开失败，系统返回的句柄无效");
            }
            else
            {
                isopen = true;//打开标志位设为True
                iserror = false;//错误标志位设为False
                return true;
            }
        }

        /// <summary>
        /// 写入字符串数据
        /// </summary>
        /// <param name="InputString">待写入的字符串</param>
        /// <returns>是否写入成功</returns>
        public bool Write(string InputString)
        {

            if (isopen == false)
            {
                return false;
            }

            int i = 0;
            int j = 0;
            int k = 0;
            byte[] inputbyte = new byte[1024 * 10];
            i = 0;
            j = 0;
            // TODO 判断中文的方式需要改进，不知是否因为有其他ESC指令的原因
            while (i < InputString.Length && j < (1024 * 10 - 3))
            {
                if ((InputString[i] >= '一') && (InputString[i] < 0x9fff))
                {
                    char c = InputString[i];
                    byte[] gb2312 = Encoding.GetEncoding("gb2312").GetBytes(c.ToString());
                    inputbyte[j] = gb2312[0];
                    inputbyte[j + 1] = gb2312[1];
                    j++;
                }
                else
                {
                    inputbyte[j] = (byte)InputString[i];
                }
                i++;
                j++;
            }
            bool flag = Write(inputbyte.Take(j).ToArray(), ref k);

            return flag;
        }

        /// <summary>
        /// 写入字节数据
        /// </summary>
        /// <param name="inputBytes">待写入的数据</param>
        /// <param name="numbersOfBytesWritten">剩余待写入长度</param>
        /// <param name="retryCount">重试计数，当计数达到5时立即返回</param>
        /// <returns>是否写入成功</returns>
        public bool Write(byte[] inputBytes, ref int numbersOfBytesWritten, int retryCount = 0)
        {
            if (hPort.IsClosed)
            {
                throw new InvalidOperationException("无法向串口写入数据，请确保端口未被占用且已打开");
            }
            if (iserror)
            {
                return false;
            }
            bool result = WriteFile((int)(hPort.DangerousGetHandle()), inputBytes, inputBytes.Length, ref numbersOfBytesWritten, ref overlapped);

            if (retryCount > 4)
            {
                //如果重发次数超过5次，就放弃重发
                //置错误标志位为True
                iserror = true;
                return false;
            }
            if (numbersOfBytesWritten != inputBytes.Length)
            {
                //如果有部分数据没写入
                //延长每字节超时时间，为了防止卡死时间过长，上限是10
                if (WriteTotalTimeoutMultiplier < 10)
                {
                    WriteTotalTimeoutMultiplier += 2;
                }
                SetTimeOuts(500, 0, 0, WriteTotalTimeoutMultiplier, WriteTotalTimeoutConstant);

                //补发数据
                int temp = 0;
                result = Write(inputBytes.Skip(numbersOfBytesWritten).ToArray(), ref temp, retryCount + 1);
            }

            //为在无流量控制的情况下自动延时，确保数据不会因设备来不及处理而丢失
            if (FlowControl == FlowControlType.None)
            {
                Thread.Sleep(inputBytes.Length * (int)WriteTotalTimeoutMultiplier);
            }
            return result;
        }

        /// <summary>
        /// 关闭串口
        /// </summary>
        public void Close()
        {
            hPort.Close();
            isopen = false;
        }

    }
}
