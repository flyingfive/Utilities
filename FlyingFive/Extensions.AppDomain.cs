using FlyingFive.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace FlyingFive
{
    public static partial class Extensions
    {
        /// <summary>
        /// 获取应用程序域的入口起始程序集
        /// </summary>
        /// <param name="appDomain"></param>
        /// <returns></returns>
        public static Assembly GetEntryAssembly(this AppDomain appDomain)
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
            return entryAssembly;
        }
    }
}
