using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlyingFive.DynamicProxy
{
    /// <summary>
    /// 代码调用的安全管理
    /// </summary>
    public class SecurityManager
    {

		public SecurityManager()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userRole"></param>
        /// <param name="methodName"></param>
        /// <returns></returns>
        public static bool IsMethodInRole(string userRole, string methodName)
        {
            return true;
        }
    }
}
