using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace FlyingFive
{
    public static partial class Extensions
    {
        /// <summary>
        /// 获取Url参数
        /// </summary>
        /// <param name="url">url地址</param>
        /// <returns></returns>
        public static NameValueCollection GetUrlArguments(this Uri url)
        {
            var collection = System.Web.HttpUtility.ParseQueryString(url.Query);
            return collection;
        }
    }
}
