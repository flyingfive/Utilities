using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyDBAssistant.Data
{
    [Serializable]
    public class DbAccessException : System.Exception
    {
        public DbAccessException()
            : this("数据库操作异常.")
        {
        }

        public DbAccessException(string message)
            : base(message)
        {
        }

        public DbAccessException(Exception innerException)
            : base(innerException.Message, innerException)
        {
        }

        public DbAccessException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
