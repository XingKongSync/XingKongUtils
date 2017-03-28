using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace XingKongUtils.UdpUtils
{
    public class UdpListen
    {
        private UdpClient uc = null;//声明UDPClient

        public delegate void DataReceivedHandler(byte[] data);

        public event DataReceivedHandler DataReceived;

        public delegate void DataReceivedHandler2(byte[] data, IPEndPoint addrSource);

        public event DataReceivedHandler2 DataReceived2;

        private int Port;

        private Thread th;

        public UdpListen(int port)
        {
            Port = port;
        }

        public void StopListen()
        {
            if (th != null && th.IsAlive)
            {
                th.Abort();
                th = null;
            }
        }

        public void StartListen()
        {
            //此处注意端口号要与发送方相同
            uc = new UdpClient(Port);
            //开一线程
            th = new Thread(new ThreadStart(listen));
            //设置为后台
            th.IsBackground = true;
            th.Start();

        }

        private void listen()
        {
            //声明终结点
            IPEndPoint iep = null;
            while (true)
            {
                //获得发过来的数据包
                byte[] data = uc.Receive(ref iep);
                DataReceived?.Invoke(data);
                DataReceived2?.Invoke(data, iep);
            }
        }
    }
}
