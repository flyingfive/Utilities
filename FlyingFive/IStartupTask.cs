using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlyingFive
{
    /// <summary>
    /// 表示程序启动时要执行的任务
    /// </summary>
    public interface IStartupTask : ITask
    {
        /// <summary>
        /// 执行顺序
        /// </summary>
        int Order { get; }
    }
}
