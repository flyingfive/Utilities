using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyDBAssistant.Schema
{
    /// <summary>
    /// 数据库实例版本
    /// </summary>
    public enum MsSqlVersion : int
    {
        MSSQL2000 = 2000,
        MSSQL2005 = 2005,
        MSSQL2008 = 2008,
        MSSQL2012 = 2012
    }
}
