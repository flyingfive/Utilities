using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace FlyingFive.Windows
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


    }


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
}
