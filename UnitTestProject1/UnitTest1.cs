using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;
using System.IO;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestHttpPost()
        {
            var args = XingKongUtils.HttpUtils.ConstructArgs();
            args.Add("inspectTeamId", "9");
            string url = "http://iems.jingnengyun.com/rt/ap/v1/head/get_prlist_by_id";

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
            response = XingKongUtils.HttpUtils.Post(url, "{\"inspectTeamId\" : \"9\"}", XingKongUtils.HttpUtils.RequestType.Raw);
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
            var logManager = XingKongUtils.LogManager.GetInstance();
            XingKongUtils.LogManager.Log("testing log.");
        }

        [TestMethod]
        public void TestAutoRun()
        {
            string filePath = @"C:\Users\liuqi\Desktop\wiringPi用户手册V001.pdf";
            Console.WriteLine(Path.GetDirectoryName(filePath));
        }

        [TestMethod]
        public void TestHttpGet()
        {
            string url = "https://sp0.baidu.com/9_Q4sjW91Qh3otqbppnN2DJv/pae/channel/data/asyncqury?cb=jQuery110204408993111524364_1489902564206&appid=4001&com=yuantong&nu=200423030012&vcode=&token=&_=1489902564208";
            string response = XingKongUtils.HttpUtils.Get(url);
            Console.WriteLine(response);
        }
    }
}
