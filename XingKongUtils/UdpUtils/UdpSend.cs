using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace XingKongUtils.UdpUtils
{
    public class UdpSend
    {
        private UdpClient uc;
        private string Ip;
        private int Port;

        public UdpSend(string ip, int port)
        {
            uc = new UdpClient(); //初始化
            Ip = ip;
            Port = port;
        }


        public void message(byte[] data)
        {
            uc.Send(data, data.Length, Ip, Port); //发送数据
        }
    }
}
