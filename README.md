# XingKongUtils
.Net Framework Utils Collection

这个解决方案包含了一些常用的C#工具类。<br/>
This solution contains some C# utils.<br/>

## Example

### XingKongUtils.HexHelper
可以将16进制的文本转换为byte数组，也可以将byte数组转换为16进制的文本<br/>
You can use HexHelper to convert byte array to hex string and also can convert hex string to byte array.
```C#
string helloworld = "Hello World!";
string hexStr = HexHelper.ByteToHex(Encoding.ASCII.GetBytes(helloworld));
Console.WriteLine(string.Format("{0}的16进制表示为：{1}", helloworld, hexStr));

byte[] data = HexHelper.HexToBytes(hexStr);
string hexStr2 = HexHelper.ByteToHex(data);
Console.WriteLine("\r\n将刚刚的16进制字符串转换回byte数组，然后再转换成16进制表示");
Console.WriteLine(hexStr2);

string str = HexHelper.BytesToString(data, Encoding.ASCII);
Console.WriteLine("\r\n将byte数组转换回字符串");
Console.WriteLine(str);
```

### XingKongUtils.ImageHelper
可以获取jpg和png图片的长和宽，并且可以缩放和旋转图片。<br/>
You can get the jpg and png picture file's size. By this class you also can rotate and scale a picture.
```C#
string pngPath = @"D:\51.png";
var pngSize = XingKongUtils.ImageHelper.getPngSize(pngPath);
Console.WriteLine(string.Format("Width:{0}, Height{1}", pngSize.Width, pngSize.Height));

string jpgPath = @"D:\51.jpg";
var jpgSize = XingKongUtils.ImageHelper.getJpgSize(jpgPath);
Console.WriteLine(string.Format("Width:{0}, Height{1}", jpgSize.Width, jpgSize.Height));

string picPath = @"D:\001.jpg";
var picSize = XingKongUtils.ImageHelper.getPictureSize(picPath);
Console.WriteLine(string.Format("Width:{0}, Height{1}", picSize.Width, picSize.Height));
```

### XingKongUtils.HttpUtils
可以方便地使用Post和Get方法，并且支持Json、Form和Raw格式的参数。<br/>
You can easily do HTTP-POST and HTTP-GET with this class, it also support three type parameters like Josn, Form and Raw.
```C#
var args = XingKongUtils.HttpUtils.ConstructArgs();
args.Add("name", "XingKong");
args.Add("city", "Beijing");
string url = "http://www.w3school.com.cn/example/jquery/demo_test_post.asp";

Console.WriteLine("开始测试static版的HttpPost");
var response = XingKongUtils.HttpUtils.Post(url, args, XingKongUtils.HttpUtils.RequestType.Json);
Console.WriteLine(response);

Console.WriteLine("开始测试实例版的HttpPost");
var httpUtils = new XingKongUtils.HttpUtils();
response = httpUtils.Post_KeepAlive(url, args, XingKongUtils.HttpUtils.RequestType.Json);
Console.WriteLine(response);

Console.WriteLine("开始测试Static发送Raw数据的的HttpPost");
response = XingKongUtils.HttpUtils.Post(url, "name=XingKong&city=Beijing", XingKongUtils.HttpUtils.RequestType.Raw);
Console.WriteLine(response);

Console.WriteLine("开始测试GET方法");
response = XingKongUtils.HttpUtils.Get("http://www.baidu.com");
Console.WriteLine(response);
```

### XingKongUtils.UdpUtils
通过如下方法创建一个Udp Client，并开始监听端口。当收到数据时会触发DataReceived或者DataReceived2事件。DataReceived事件不关心数据的发送者，如果期望获得到发送方的相关信息的话，请绑定DataReceived2事件。<br/>
You can create a udp client and then start listening a port like this. When data received you can get the data in event handler DataReceived or DataReceived2. If you want get the infomation about data sender, please bind the DataReceived2 event instead of DataReceived.
```C#
UdpUtils.UdpListen udpClient = new UdpUtils.UdpListen(9849);
udpClient.DataReceived += UdpClient_DataReceived;
//udpClient.DataReceived2 += UdpClient_DataReceived2;
udpClient.StartListen();
```
### XingKongUtils.XKserialPort
直接调用Windows API实现的SerialPort类，与.Net Framework自带的SerialPort类似，但是支持更底层的超时控制，更好地支持硬件握手和流量控制，即使当硬件不支持流量控制时，也可以通过手动指定延时时间来尽量减少丢失数据的可能性。此外，XKserialPort还支持丢失重发，当数据不能全部写入时，会记录当前指针位置，然后稍后重试写入。当检测到数据没有全部写入成功时，XKserialPort还会适当延长平均每字节的等待时间。<br/>
XKserialPort is familiar with the SerialPort in .Net Framework, but it uses Windows API directly. XKserialPort support low level timeout controls, and works better with hardware handshake. Even when the hardware doesn't support handshake, it also can decrease the posibility of losing data by giving timeout arguments manually. XKserialPort also support resend the remaining data when write data failed. When writing data failed hanppend, XKserialPort will record the position and try to resend after a moment of waiting. It also increase the value of WriteTotalTimeoutMultiplier properly.
    
```C#
XKserialPort xkSerialPort = new XKserialPort("COM1", 9600, 0, 0, XKserialPort.FlowControlType.Hardware);
xkSerialPort.SetTimeOuts(500, 0, 0, 2, 500);
xkSerialPort.Open();
xkSerialPort.Write("hello.");//GB2312
byte[] data = new byte[] { 0xff, 0x01, 0x02 };
int datalength = data.Length;
xkSerialPort.Write(data, ref datalength);
xkSerialPort.Close();
```
### XingKongUtils.LogManager
可以为WinForm或者Wpf程序添加一个控制台的输出窗口，并且支持将日志输出到文件。<br/>
You can use LogManager to print some infomation on Console in your WinForm or Wpf application, it also support write your log into a file.
```C#
LogManager.ShowConsole();
LogManager.Log("testing log.");
LogManager.Log("warning", LogManager.MessageType.Warning);
LogManager.Log("error", LogManager.MessageType.Error, true);
LogManager.HideConsole();
```
### XingKongUtils.AutoRunHelper
可以将你的应用程序设置或者取消开机自启。<br/>
You can easily make or cancel your application auto run after Windows started.
```C#
AutoRunHelper autoRunHelper = new AutoRunHelper(@"D:\test.exe", "MyApp", "My Application will auto run after windows stated");
if (!autoRunHelper.isAutoRun())
{
    autoRunHelper.RunWhenStart(true);
}
```
