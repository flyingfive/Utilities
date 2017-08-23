using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace FlyingFive.Win
{
    /// <summary>
    /// 提供winform控件扩展行为
    /// </summary>
    public static partial class Extensions
    {
        /// <summary>
        /// 判断当前是否运行在visual studio设计器模式
        /// </summary>
        /// <returns></returns>
        public static bool IsInDesignMode(this Control ctl)
        {
            bool returnFlag = false;
            if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
            {
                returnFlag = true;
            }
            else if (System.Diagnostics.Process.GetCurrentProcess().ProcessName.ToUpper().Equals("DEVENV"))
            {
                returnFlag = true;
            }
            return returnFlag;
        }

    }
}
