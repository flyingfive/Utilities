using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlyingFive.Data
{
    /// <summary>
    /// 异常辅助工具
    /// </summary>
    public static class UtilExceptions
    {
        /// <summary>
        /// NULL检查
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="paramName"></param>
        public static void CheckNull(object obj, string paramName = null)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(string.Format("参数{0}不能为null", paramName));
            }
        }
    }
}
