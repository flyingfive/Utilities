﻿using System;
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

        /// <summary>
        /// 在创建UI控件的线程上调用委托
        /// </summary>
        /// <param name="control">UI控件</param>
        /// <param name="action">调用的方法</param>
        /// <param name="args">方法参数</param>
        /// <returns></returns>
        public static object UIThreadInvoke(this Control control, Delegate action, params object[] args)
        {
            object ret = null;
            if (control == null) { throw new ArgumentNullException("参数：control不能为null."); }
            if (action == null) { throw new ArgumentNullException("参数：action不能为null."); }
            if (control.IsDisposed)
            {
                throw new ObjectDisposedException("无法在已释放的控件上执行调用！");
            }
            if (!control.IsHandleCreated)
            {
                throw new ObjectDisposedException("不能在未创建句柄的控件上调用！");
            }
            if (control.InvokeRequired)
            {
                ret = control.Invoke(action, args);
            }
            else
            {
                ret = action.DynamicInvoke(args);
            }
            return ret;
        }

        /// <summary>
        /// 设置控件的水印提示文字
        /// </summary>
        /// <param name="control"></param>
        /// <param name="text"></param>
        public static void SetCueText(this Control control, string text)
        {
            if (string.IsNullOrWhiteSpace(text)) { return; }
            if (control == null) { throw new ArgumentNullException("参数：control不能为null."); }
            if (control.IsDisposed)
            {
                throw new ObjectDisposedException("无法在已释放的控件上设置水印提示！");
            }
            if (control.InvokeRequired)
            {
                control.Invoke(new Action<Control, string>((ctl, txt) =>
                {
                    Win32Utility.SetCueText(ctl, text);
                }), new object[] { control, text });
            }
            else
            {
                Win32Utility.SetCueText(control, text);
            }
        }

        ///// <summary>
        ///// 获取控件的水印提示文字
        ///// </summary>
        ///// <param name="control"></param>
        ///// <returns></returns>
        //public static string GetCueText(this Control control)
        //{
        //    if (control == null) { throw new ArgumentNullException("参数：control不能为null."); }
        //    if (control.IsDisposed)
        //    {
        //        throw new ObjectDisposedException("无法在已释放的控件上获取控件的水印提示！");
        //    }
        //    if (control.InvokeRequired)
        //    {
        //        var obj = control.Invoke(new Func<Control, string>((ctl) => { var text = Win32Utility.GetCueText(ctl); return text; }), new object[] { control });
        //        return obj == null ? string.Empty : obj.ToString();
        //    }
        //    else
        //    {
        //        var text = Win32Utility.GetCueText(control);
        //        return text;
        //    }
        //}
    }
}
