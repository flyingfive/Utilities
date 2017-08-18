using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlyingFive
{
    /// <summary>
    /// 表示程序任务
    /// </summary>
    public interface ITask
    {
        /// <summary>
        /// 任务描述
        /// </summary>
        string Description { get; }
        /// <summary>
        /// 执行任务
        /// </summary>
        void Execute();
    }
}
