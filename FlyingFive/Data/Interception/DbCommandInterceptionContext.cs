using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlyingFive.Data.Interception
{
    /// <summary>
    /// System.Data.IDbCommand对象拦截器当前拦截的上下文信息
    /// </summary>
    /// <typeparam name="TResult">当前上下文所操作的结果数据类型</typeparam>
    public class DbCommandInterceptionContext<TResult>
    {
        /// <summary>
        /// 结果
        /// </summary>
        public TResult Result { get; set; }
        /// <summary>
        /// 当前会话处理的DB操作中发生的异常
        /// </summary>
        public Exception Exception { get; set; }
        public Dictionary<string, object> DataBag { get; private set; }

        public DbCommandInterceptionContext() { this.DataBag = new Dictionary<string, object>(); }
    }
}
