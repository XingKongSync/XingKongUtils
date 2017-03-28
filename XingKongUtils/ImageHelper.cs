using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace XingKongUtils
{
    /// <summary>
    /// 快速获取指定图片文件的长和宽
    /// 支持jpg和png
    /// </summary>
    public class ImageHelper
    {
        public static Size getPictureSize(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(filePath);
            }
            string ext = Path.GetExtension(filePath).ToLower();
            switch (ext)
            {
                case ".jpg":
                    return getJpgSize(filePath);
                case ".png":
                    return getPngSize(filePath);
                default:
                    throw new NotSupportedException("不支持的图片类型");
            }
        }

        /// <summary>
        /// 快速获取JPG图片大小及英寸分辨率
        /// </summary>
        /// <param name="fileName">文件路径</param>
        /// <returns></returns>
        public static Size getJpgSize(string fileName)
        {
            Size jpgSize;
            float wpx;
            float hpx;
            getJpgSize(fileName, out jpgSize, out wpx, out hpx);
            return jpgSize;
        }

        /// <summary>
        /// 快速获取JPG图片大小及英寸分辨率
        /// </summary>
        /// <param name="FileName">文件路径</param>
        /// <param name="JpgSize">图像尺寸</param>
        /// <param name="Wpx">水平方向(像素/英寸)分辨率</param>
        /// <param name="Hpx">垂直方向(像素/英寸)分辨率</param>
        /// <returns></returns>
        public static int getJpgSize(string FileName, out Size JpgSize, out float Wpx, out float Hpx)
        {
            JpgSize = new Size(0, 0);
            Wpx = 0; Hpx = 0;
            int rx = 0;
            if (!File.Exists(FileName)) return rx;
            FileStream F_Stream = File.OpenRead(FileName);
            int ff = F_Stream.ReadByte();
            int type = F_Stream.ReadByte();
            if (ff != 0xff || type != 0xd8)
            {//非JPG文件
                F_Stream.Close();
                return rx;
            }
            long ps = 0;
            do
            {
                do
                {
                    ff = F_Stream.ReadByte();
                    if (ff < 0) //文件结束
                    {
                        F_Stream.Close();
                        return rx;
                    }
                } while (ff != 0xff);

                do
                {
                    type = F_Stream.ReadByte();
                } while (type == 0xff);

                //MessageBox.Show(ff.ToString() + "," + type.ToString(), F_Stream.Position.ToString());
                ps = F_Stream.Position;
                switch (type)
                {
                    case 0x00:
                    case 0x01:
                    case 0xD0:
                    case 0xD1:
                    case 0xD2:
                    case 0xD3:
                    case 0xD4:
                    case 0xD5:
                    case 0xD6:
                    case 0xD7:
                        break;
                    case 0xc0: //SOF0段
                        ps = F_Stream.ReadByte() * 256;
                        ps = F_Stream.Position + ps + F_Stream.ReadByte() - 2; //加段长度

                        F_Stream.ReadByte(); //丢弃精度数据
                        //高度
                        JpgSize.Height = F_Stream.ReadByte() * 256;
                        JpgSize.Height = JpgSize.Height + F_Stream.ReadByte();
                        //宽度
                        JpgSize.Width = F_Stream.ReadByte() * 256;
                        JpgSize.Width = JpgSize.Width + F_Stream.ReadByte();
                        //后面信息忽略
                        if (rx != 1 && rx < 3) rx = rx + 1;
                        break;
                    case 0xe0: //APP0段
                        ps = F_Stream.ReadByte() * 256;
                        ps = F_Stream.Position + ps + F_Stream.ReadByte() - 2; //加段长度

                        F_Stream.Seek(5, SeekOrigin.Current); //丢弃APP0标记(5bytes)
                        F_Stream.Seek(2, SeekOrigin.Current); //丢弃主版本号(1bytes)及次版本号(1bytes)
                        int units = F_Stream.ReadByte(); //X和Y的密度单位,units=0：无单位,units=1：点数/英寸,units=2：点数/厘米

                        //水平方向(像素/英寸)分辨率
                        Wpx = F_Stream.ReadByte() * 256;
                        Wpx = Wpx + F_Stream.ReadByte();
                        if (units == 2) Wpx = (float)(Wpx * 2.54); //厘米变为英寸
                        //垂直方向(像素/英寸)分辨率
                        Hpx = F_Stream.ReadByte() * 256;
                        Hpx = Hpx + F_Stream.ReadByte();
                        if (units == 2) Hpx = (float)(Hpx * 2.54); //厘米变为英寸
                        //后面信息忽略
                        if (rx != 2 && rx < 3) rx = rx + 2;
                        break;

                    default: //别的段都跳过////////////////
                        ps = F_Stream.ReadByte() * 256;
                        ps = F_Stream.Position + ps + F_Stream.ReadByte() - 2; //加段长度
                        break;
                }
                if (ps + 1 >= F_Stream.Length) //文件结束
                {
                    F_Stream.Close();
                    return rx;
                }
                F_Stream.Position = ps; //移动指针
            } while (type != 0xda); // 扫描行开始
            F_Stream.Close();
            return rx;
        }

        /// <summary>
        /// 读取png格式图片的长宽
        /// </summary>
        /// <param name="fileName">文件路径</param>
        /// <returns></returns>
        public static Size getPngSize(string fileName)
        {
            if (!File.Exists(fileName))
            {
                throw new FileNotFoundException(fileName);
            }
            FileStream fstream = File.OpenRead(fileName);
            fstream.Position = 0x10;

            byte[] bytes = new byte[4];
            fstream.Read(bytes, 0, 4);
            Array.Reverse(bytes);
            int width = BitConverter.ToInt32(bytes, 0);
            //Console.WriteLine("widthBytes:" + HexHelper.ByteToHex(widthBytes));

            fstream.Read(bytes, 0, 4);
            Array.Reverse(bytes);
            int height = BitConverter.ToInt32(bytes, 0);
            //Console.WriteLine("widthBytes:" + HexHelper.ByteToHex(widthBytes));

            Size pngSize = new Size(width, height);
            return pngSize;
        }

        public class Size
        {
            public int Height;
            public int Width;

            public Size()
            {

            }

            public Size(int width, int height)
            {
                this.Height = height;
                this.Width = width;
            }
        }

        /// <summary>
        /// 以逆时针为方向对图像进行旋转
        /// </summary>
        /// <param name="b">位图流</param>
        /// <param name="angle">旋转角度[0,360](前台给的)</param>
        /// <returns></returns>
        public static Bitmap Rotate(Bitmap b, int angle)
        {
            angle = angle % 360;
            //弧度转换
            double radian = angle * Math.PI / 180.0;
            double cos = Math.Cos(radian);
            double sin = Math.Sin(radian);
            //原图的宽和高
            int w = b.Width;
            int h = b.Height;
            int W = (int)(Math.Max(Math.Abs(w * cos - h * sin), Math.Abs(w * cos + h * sin)));
            int H = (int)(Math.Max(Math.Abs(w * sin - h * cos), Math.Abs(w * sin + h * cos)));
            //目标位图
            Bitmap dsImage = new Bitmap(W, H);
            System.Drawing.Graphics g = Graphics.FromImage(dsImage);
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Bilinear;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            //计算偏移量
            Point Offset = new Point((W - w) / 2, (H - h) / 2);
            //构造图像显示区域：让图像的中心与窗口的中心点一致
            Rectangle rect = new Rectangle(Offset.X, Offset.Y, w, h);
            Point center = new Point(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);
            g.TranslateTransform(center.X, center.Y);
            g.RotateTransform(360 - angle);
            //恢复图像在水平和垂直方向的平移
            g.TranslateTransform(-center.X, -center.Y);
            g.DrawImage(b, rect);
            //重至绘图的所有变换
            g.ResetTransform();
            g.Save();
            g.Dispose();
            //dsImage.Save("yuancd.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
            return dsImage;
        }

        /// <summary>
        /// 图像按比例缩放
        /// </summary>
        /// <param name="b"></param>
        /// <param name="destHeight"></param>
        /// <param name="destWidth"></param>
        /// <returns></returns>
        public static Bitmap Scale(Bitmap b, int destHeight, int destWidth)
        {
            System.Drawing.Image imgSource = b;
            System.Drawing.Imaging.ImageFormat thisFormat = imgSource.RawFormat;
            int sW = 0, sH = 0;
            // 按比例缩放           
            int sWidth = imgSource.Width;
            int sHeight = imgSource.Height;
            if (sHeight > destHeight || sWidth > destWidth)
            {
                if ((sWidth * destHeight) > (sHeight * destWidth))
                {
                    sW = destWidth;
                    sH = (destWidth * sHeight) / sWidth;
                }
                else
                {
                    sH = destHeight;
                    sW = (sWidth * destHeight) / sHeight;
                }
            }
            else
            {
                sW = sWidth;
                sH = sHeight;
            }
            Bitmap outBmp = new Bitmap(sW, sH);
            Graphics g = Graphics.FromImage(outBmp);
            g.Clear(Color.Transparent);
            // 设置画布的描绘质量         
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.DrawImage(imgSource, new Rectangle(0, 0, sW, sH), 0, 0, imgSource.Width, imgSource.Height, GraphicsUnit.Pixel);
            g.Dispose();
            // 以下代码为保存图片时，设置压缩质量     
            EncoderParameters encoderParams = new EncoderParameters();
            long[] quality = new long[1];
            quality[0] = 100;
            EncoderParameter encoderParam = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);
            encoderParams.Param[0] = encoderParam;
            imgSource.Dispose();
            return outBmp;
        }
    }
}
