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
                var typeFinder = new AppDomainTypeFinder();
                var entryType = typeFinder.FindClassesOfType<System.Web.HttpApplication>().Where(t => t.BaseType == typeof(System.Web.HttpApplication)).FirstOrDefault();
                if (entryType != null) { return entryType.Assembly; }
            }
            return entryAssembly;
        }
    }
}
