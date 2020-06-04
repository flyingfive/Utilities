using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace FlyingFive.Windows.Forms
{
    /// <summary>
    /// Winform消息提示框封装
    /// </summary>
    public class MsgBox
    {
        /// <summary>
        /// 弹出消息提示
        /// </summary>
        /// <param name="msg">提示消息</param>
        /// <param name="title">标题</param>
        public static void Information(string msg, string title = "")
        {
            if (!System.Windows.Forms.SystemInformation.UserInteractive) { return; }
            title = string.IsNullOrEmpty(title) ? "消息" : title;
            var f = Application.OpenForms.OfType<Form>().FirstOrDefault();
            if (f != null)
            {
                f.Invoke(new Action(() =>
                {
                    MessageBox.Show(msg, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }));
            }
            else
            {
                MessageBox.Show(msg, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// 弹出错误提示
        /// </summary>
        /// <param name="msg">错误消息</param>
        /// <param name="title">标题</param>
        public static void Error(string msg, string title = "")
        {
            if (!System.Windows.Forms.SystemInformation.UserInteractive) { return; }
            title = string.IsNullOrEmpty(title) ? "错误" : title;
            title = string.Concat(title, "[Ver: ", AppVersion.CurrentVersion.ToString(), "]");
            var f = Application.OpenForms.OfType<Form>().FirstOrDefault();
            if (f != null)
            {
                f.Invoke(new Action(() =>
                {
                    MessageBox.Show(msg, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }));
            }
            else
            {
                MessageBox.Show(msg, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 弹出确认消息
        /// </summary>
        /// <param name="msg">确认消息</param>
        /// <param name="title">标题</param>
        /// <returns></returns>
        public static bool Confirm(string msg, string title = "")
        {
            if (!System.Windows.Forms.SystemInformation.UserInteractive) { return false; }
            if (string.IsNullOrEmpty(title)) { title = "确认"; }
            var f = Application.OpenForms.OfType<Form>().FirstOrDefault();
            if (f != null)
            {
                var flag = f.Invoke(new Func<bool>(() =>
                {
                    return MessageBox.Show(msg, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
                }));
                return Convert.ToBoolean(flag);
            }
            else
            {
                return MessageBox.Show(msg, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
            }
        }

        /// <summary>
        /// 是/否/取消选择框
        /// </summary>
        /// <param name="msg">消息</param>
        /// <param name="title">标题</param>
        /// <returns></returns>
        public static DialogResult ShowYesNoCancel(string msg, string title = "")
        {
            if (!System.Windows.Forms.SystemInformation.UserInteractive) { return DialogResult.None; }
            if (string.IsNullOrEmpty(title)) { title = "提示"; }
            return MessageBox.Show(msg, title, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
        }
    }
}
