using FlyingFive.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Hosting;

namespace FlyingFive
{
    public static partial class Extensions
    {
        /// <summary>
        /// 获取应用程序域的入口起始程序集
        /// </summary>
        /// <param name="appDomain">应用程序域</param>
        /// <param name="useExecuting">找不到时是否使用当前执行程序集</param>
        /// <returns></returns>
        public static Assembly GetEntryAssembly(this AppDomain appDomain, bool useExecuting = false)
        {
            var entryAssembly = Assembly.GetEntryAssembly();
            if (entryAssembly == null && System.Web.Hosting.HostingEnvironment.IsHosted)
            {
                var asaxType = System.Web.Compilation.BuildManager.GetGlobalAsaxType();
                while (asaxType.BaseType != null && asaxType.BaseType != typeof(System.Web.HttpApplication))
                {
                    asaxType = asaxType.BaseType;
                }
                return asaxType.Assembly;
            }
            if (entryAssembly == null)
            {
                if (useExecuting)
                {
                    entryAssembly = Assembly.GetCallingAssembly();
                }
            }
            if (entryAssembly == null)
            {
                throw new NotSupportedException("应用程序域入口查找失败。");
            }
            return entryAssembly;
        }
    }
}
