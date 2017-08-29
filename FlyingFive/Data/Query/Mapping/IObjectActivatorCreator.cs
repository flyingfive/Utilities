using FlyingFive.Data.Mapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlyingFive.Data.Query.Mapping
{
    /// <summary>
    /// 查询中的对象激活工具生成器
    /// </summary>
    public interface IObjectActivatorCreator
    {
        /// <summary>
        /// 创建一个对象激活工具
        /// </summary>
        /// <returns></returns>
        IObjectActivator CreateObjectActivator();
        /// <summary>
        /// 从数据库上下文创建一个对象激活工具
        /// </summary>
        /// <param name="dbContext"></param>
        /// <returns></returns>
        IObjectActivator CreateObjectActivator(IDbContext dbContext);
    }
}
