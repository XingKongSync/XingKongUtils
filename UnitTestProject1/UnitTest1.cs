using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;
using System.IO;
using XingKongUtils;
using XingKongAutoStartup;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestHttpPost()
        {
            var args = XingKongUtils.HttpUtils.ConstructArgs();
            args.Add("name", "XingKong");
            args.Add("city", "Beijing");
            string url = "http://www.w3school.com.cn/example/jquery/demo_test_post.asp";

            Console.WriteLine("开始测试static版的HttpPost");
            var response = XingKongUtils.HttpUtils.Post(url, args, XingKongUtils.HttpUtils.RequestType.Json);
            Console.WriteLine(response);

            Console.WriteLine();

            Console.WriteLine("开始测试实例版的HttpPost");
            var httpUtils = new XingKongUtils.HttpUtils();
            response = httpUtils.Post_KeepAlive(url, args, XingKongUtils.HttpUtils.RequestType.Json);
            Console.WriteLine(response);

            Console.WriteLine();

            Console.WriteLine("开始测试Static发送Raw数据的的HttpPost");
            response = XingKongUtils.HttpUtils.Post(url, "name=XingKong&city=Beijing", XingKongUtils.HttpUtils.RequestType.Raw);
            Console.WriteLine(response);
        }

        [TestMethod]
        public void TestHexHelper()
        {
            string helloworld = "Hello World!";
            string hexStr = XingKongUtils.HexHelper.ByteToHex(Encoding.ASCII.GetBytes(helloworld));
            Console.WriteLine(string.Format("{0}的16进制表示为：{1}", helloworld, hexStr));

            byte[] data = XingKongUtils.HexHelper.HexToBytes(hexStr);
            string hexStr2 = XingKongUtils.HexHelper.ByteToHex(data);
            Console.WriteLine("\r\n将刚刚的16进制字符串转换回byte数组，然后再转换成16进制表示");
            Console.WriteLine(hexStr2);

            string str = XingKongUtils.HexHelper.BytesToString(data, Encoding.ASCII);
            Console.WriteLine("\r\n将byte数组转换回字符串");
            Console.WriteLine(str);
        }

        [TestMethod]
        public void TestImageHelper()
        {
            string pngPath = @"C:\Users\liuqi\Desktop\51.png";
            var pngSize = XingKongUtils.ImageHelper.getPngSize(pngPath);
            Console.WriteLine(string.Format("Width:{0}, Height{1}", pngSize.Width, pngSize.Height));

            string jpgPath = @"C:\Users\liuqi\Desktop\51.jpg";
            var jpgSize = XingKongUtils.ImageHelper.getJpgSize(jpgPath);
            Console.WriteLine(string.Format("Width:{0}, Height{1}", jpgSize.Width, jpgSize.Height));

            string picPath = @"Z:\My Media\My Pictures\我们为我们\001.jpg";
            var picSize = XingKongUtils.ImageHelper.getPictureSize(picPath);
            Console.WriteLine(string.Format("Width:{0}, Height{1}", picSize.Width, picSize.Height));
        }

        [TestMethod]
        public void TestLogManager()
        {
            LogManager.ShowConsole();
            LogManager.Log("testing log.");
            LogManager.Log("warning", LogManager.MessageType.Warning);
            LogManager.Log("error", LogManager.MessageType.Error, true);
            LogManager.HideConsole();
        }

        [TestMethod]
        public void TestHttpGet()
        {
            string url = "http://www.baidu.com";
            string response = XingKongUtils.HttpUtils.Get(url);
            Console.WriteLine(response);
        }

        [TestMethod]
        public void TestXKSerialPort()
        {
            XKserialPort xkSerialPort = new XKserialPort("COM1", 9600, 0, 0, XKserialPort.FlowControlType.Hardware);
            xkSerialPort.SetTimeOuts(500, 0, 0, 2, 500);
            xkSerialPort.Open();
            xkSerialPort.Write("hello.");//GB2312
            byte[] data = new byte[] { 0xff, 0x01, 0x02 };
            int datalength = data.Length;
            xkSerialPort.Write(data, ref datalength);
            xkSerialPort.Close();
        }

        [TestMethod]
        public void TestAutoRun()
        {
            AutoRunHelper autoRunHelper = new AutoRunHelper(@"D:\test.exe", "MyApp", "My Application will auto run after windows stated");
            if (!autoRunHelper.isAutoRun())
            {
                autoRunHelper.RunWhenStart(true);
            }
        }
    }
}
