using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XingKongAutoStartup
{
    /// <summary>
    /// 设置开机启动的类
    /// </summary>
    public class AutoRunHelper
    {
        private string lnkPath;

        private string excutablePath;

        private string description;

        /// <summary>
        /// 初始化开机自启类实例
        /// </summary>
        /// <param name="excutablePath">可执行文件的路径</param>
        /// <param name="name">启动项名称</param>
        /// <param name="description">启动项描述</param>
        public AutoRunHelper(string excutablePath, string name, string description)
        {
            this.excutablePath = excutablePath;
            this.description = description;

            lnkPath = System.Environment.GetFolderPath(Environment.SpecialFolder.Startup) + "\\" + name +".lnk";
        }

        /// <summary>
        /// 判断是否本系统是否已经设置了开机启动
        /// </summary>
        /// <returns>true:已经设置开机启动，false：未设置开机启动</returns>
        public bool isAutoRun()
        {
            if (File.Exists(lnkPath))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary> 
        /// 设置是否开机启动
        /// </summary> 
        /// <param name="isAutoRun">是否启动</param> 
        public void RunWhenStart(bool isAutoRun)
        {
            if (isAutoRun == true)
            {
                IWshRuntimeLibrary.WshShell shell = new IWshRuntimeLibrary.WshShell();
                IWshRuntimeLibrary.IWshShortcut shortCut = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(lnkPath);
                shortCut.TargetPath = excutablePath;
                shortCut.WindowStyle = 1;
                shortCut.Description = description;
                shortCut.IconLocation = excutablePath;
                shortCut.WorkingDirectory = Path.GetDirectoryName(excutablePath);
                shortCut.Save();
            }
            else
            {
                if (File.Exists(lnkPath))
                {
                    File.Delete(lnkPath);
                }
            }
        }
    }
}
