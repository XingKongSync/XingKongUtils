using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace XingKongUtils
{
    public class LogManager
    {
        private static LogManager m_Instance;

        /// <summary>
        /// 日志文件的输出文件夹
        /// </summary>
        private string outPutDirectory;

        /// <summary>
        /// 获得一个LogManager实例
        /// </summary>
        /// <returns></returns>
        public static LogManager GetInstance()
        {
            if (m_Instance == null)
            {
                m_Instance = new LogManager();
            }
            return m_Instance;
        }

        private const string Kernel32_DllName = "kernel32.dll";

        [DllImport(Kernel32_DllName)]
        private static extern bool AllocConsole();

        [DllImport(Kernel32_DllName)]
        private static extern bool FreeConsole();

        [DllImport(Kernel32_DllName)]
        private static extern IntPtr GetConsoleWindow();


        /// <summary>
        /// 判断本程序是否已经显示控制台
        /// </summary>
        public static bool HasConsole
        {
            get { return GetConsoleWindow() != IntPtr.Zero; }
        }

        /// <summary>
        /// 显示控制台
        /// </summary>
        public static void ShowConsole()
        {
            if (!HasConsole)
            {
                AllocConsole();
                InvalidateOutAndError();
            }
        }

        /// <summary>
        /// 关闭控制台
        /// </summary>
        public static void HideConsole()
        {
            if (HasConsole)
            {
                SetOutAndErrorNull();
                FreeConsole();
            }
        }

        private static void InvalidateOutAndError()
        {
            Type type = typeof(System.Console);
            System.Reflection.FieldInfo _out = type.GetField("_out",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            System.Reflection.FieldInfo _error = type.GetField("_error",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            System.Reflection.MethodInfo _InitializeStdOutError = type.GetMethod("InitializeStdOutError",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);

            _out.SetValue(null, null);
            _error.SetValue(null, null);
            _InitializeStdOutError.Invoke(null, new object[] { true });
        }

        private static void SetOutAndErrorNull()
        {
            Console.SetOut(TextWriter.Null);
            Console.SetError(TextWriter.Null);
        }

        private LogManager()
        {
            //初始化输出目录
            outPutDirectory = Path.Combine(System.IO.Directory.GetCurrentDirectory(), @"LogData");

            //如果目录不存在则创建
            if (!Directory.Exists(outPutDirectory))
            {
                Directory.CreateDirectory(outPutDirectory);
            }
        }

        public void LogInfo(string logdata, bool writeToFile)
        {
            string filename = string.Format("Info_{0}.txt", DateTime.Now.ToString("yyyy-MM-dd"));
            string path = Path.Combine(outPutDirectory, filename);
            string log = string.Format("{0} : {1}", DateTime.Now.ToString("HH:mm:ss"), logdata);

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(log);

            if (writeToFile)
            {
                File.AppendAllText(path, log + "\r\n");
            }
        }

        public void LogWarning(string logdata, bool writeToFile)
        {
            string filename = string.Format("Warning_{0}.txt", DateTime.Now.ToString("yyyy-MM-dd"));
            string path = Path.Combine(outPutDirectory, filename);
            string log = string.Format("{0} : {1}", DateTime.Now.ToString("HH:mm:ss"), logdata);

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(log);

            if (writeToFile)
            {
                File.AppendAllText(path, log + "\r\n");
            }
        }

        public void LogError(string logdata, bool writeToFile)
        {
            string filename = string.Format("Error_{0}.txt", DateTime.Now.ToString("yyyy-MM-dd"));
            string path = Path.Combine(outPutDirectory, filename);
            string log = string.Format("{0} : {1}", DateTime.Now.ToString("HH:mm:ss"), logdata);

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(log);

            if (writeToFile)
            {
                File.AppendAllText(path, log + "\r\n");
            }
        }

        public static void Log(string logdata, MessageType msgType = MessageType.Info, bool writeToFile = true)
        {
            switch (msgType)
            {
                case MessageType.Info:
                    GetInstance().LogInfo(logdata, writeToFile);
                    break;
                case MessageType.Warning:
                    GetInstance().LogWarning(logdata, writeToFile);
                    break;
                case MessageType.Error:
                    GetInstance().LogError(logdata, writeToFile);
                    break;
                default:
                    break;
            }

        }

        public enum MessageType
        {
            Info,
            Warning,
            Error
        }
    }
}
