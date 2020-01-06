using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace FlyingFive.Win
{
    /// <summary>
    /// win32 api接口工具
    /// </summary>
    public static class Win32Api
    {

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern Int32 SendMessage(IntPtr hWnd, int msg, int wParam, [MarshalAs(UnmanagedType.LPWStr)] string lParam);

        [DllImport("user32.dll")]
        public static extern bool SendMessage(IntPtr hwnd, int msg, int wParam, StringBuilder lParam);

        [DllImport("user32.dll")]
        internal static extern bool GetComboBoxInfo(IntPtr hwnd, ref COMBOBOXINFO pcbi);

        [StructLayout(LayoutKind.Sequential)]
        internal struct COMBOBOXINFO
        {
            public int cbSize;
            public RECT rcItem;
            public RECT rcButton;
            public IntPtr stateButton;
            public IntPtr hwndCombo;
            public IntPtr hwndItem;
            public IntPtr hwndList;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }


        public const int EM_SETCUEBANNER = 0x1501;
        public const int EM_GETCUEBANNER = 0x1502;

        /// <summary>
        /// 设置控件的水印提示文字
        /// </summary>
        /// <param name="control"></param>
        /// <param name="text"></param>
        internal static void SetCueText(Control control, string text)
        {
            if (control is ComboBox)
            {
                COMBOBOXINFO info = GetComboBoxInfo(control);
                SendMessage(info.hwndItem, EM_SETCUEBANNER, 0, text);
            }
            else
            {
                SendMessage(control.Handle, EM_SETCUEBANNER, 0, text);
            }
        }
        ///// <summary>
        ///// 获取控件的水印提示文字
        ///// </summary>
        ///// <param name="control"></param>
        ///// <returns></returns>
        //public static string GetCueText(Control control)
        //{
        //    StringBuilder builder = new StringBuilder();
        //    if (control is ComboBox)
        //    {
        //        COMBOBOXINFO info = new COMBOBOXINFO();
        //        //a combobox is made up of two controls, a list and textbox;
        //        //we want the textbox  
        //        info.cbSize = Marshal.SizeOf(info);
        //        GetComboBoxInfo(control.Handle, ref info);
        //        SendMessage(info.hwndItem, EM_GETCUEBANNER, 0, builder);
        //    }
        //    else
        //    {
        //        SendMessage(control.Handle, EM_GETCUEBANNER, 0, builder);
        //    }
        //    return builder.ToString();
        //}

        internal static COMBOBOXINFO GetComboBoxInfo(Control control)
        {
            COMBOBOXINFO info = new COMBOBOXINFO();
            //a combobox is made up of three controls, a button, a list and textbox;  
            //we want the textbox  
            info.cbSize = Marshal.SizeOf(info);
            GetComboBoxInfo(control.Handle, ref info);
            return info;
        }
    }
}
