using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XingKongUtils
{
    public class HexHelper
    {
        /// <summary>
        /// 将16进制表示的字符串转换成对应的byte数组
        /// 注意，数与数之间要用空格分隔
        /// </summary>
        /// <param name="hexStr">待转换的16进制表示的字符串</param>
        /// <returns>转换后的byte数组</returns>
        public static byte[] HexToBytes(string hexStr)
        {
            List<byte> bytes = new List<byte>();
            string[] parts = hexStr.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string part in parts)
            {
                bytes.Add(Convert.ToByte(part, 16));
            }
            return bytes.ToArray();
        }


        /// <summary>
        /// 将字节数组转换为指定编码的字符串，同时删除不可见字符
        /// </summary>
        /// <param name="bytes">要转换的字节数组</param>
        /// <returns>转换后的字符串</returns>
        public static string BytesToString(byte[] bytes, Encoding encoding)
        {
            if (encoding == null)
            {
                throw new ArgumentNullException("encoding can't be null.");
            }
            string str = encoding.GetString(bytes);
            str = DeleteUnVisibleChar(str);
            return str;
        }

        /// <summary>
        /// 删除不可见字符
        /// </summary>
        /// <param name="sourceString">原始字符</param>
        /// <returns>删除后的结果</returns>
        public static string DeleteUnVisibleChar(string sourceString)
        {
            StringBuilder sBuilder = new System.Text.StringBuilder(131);
            for (int i = 0; i < sourceString.Length; i++)
            {
                int Unicode = sourceString[i];
                if (Unicode >= 16)
                {
                    sBuilder.Append(sourceString[i].ToString());
                }
            }
            return sBuilder.ToString();
        }

        /// <summary>
        /// 将byte数组转换成对应的16进制字符串表示
        /// </summary>
        /// <param name="Hex">待转换的byte数组</param>
        /// <returns>转换后的结果</returns>
        public static string ByteToHex(byte[] Hex)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte hex in Hex)
            {
                sb.Append(hex.ToString("X2") + " ");
            }
            return sb.ToString();
        }
    }
}
