﻿using FlyingFive;
using FlyingFive.Caching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace MyDBAssistant
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Singleton<ICacheManager>.Instance = new FlyingFive.Caching.MemoryCacheManager();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FrmMain());
        }
    }
}
