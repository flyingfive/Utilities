using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlyingFive.Data.Drivers.SqlServer
{
    /// <summary>
    /// 分页模式
    /// </summary>
    public enum PagingMode : int
    {
        /// <summary>
        /// ROW_NUMBER函数分页(2005及以上支持)
        /// </summary>
        ROW_NUMBER = 1,
        /// <summary>
        /// OFFSET_FETCH关键字方式(2012及以上支持)
        /// </summary>
        OFFSET_FETCH = 2
    }
}
