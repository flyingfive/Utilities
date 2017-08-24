using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace FlyingFive.Data
{
    public class DataAccessException : DataException
    {
        public DataAccessException()
            : this("数据访问异常.")
        {
        }

        public DataAccessException(string message)
            : base(message)
        {
        }
        public DataAccessException(Exception innerException)
            : base("数据访问异常.", innerException)
        {
        }

        public DataAccessException(string msg, Exception ex) : base(msg, ex) { }
    }
}
