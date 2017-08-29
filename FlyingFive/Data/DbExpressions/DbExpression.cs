using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlyingFive.Data.DbExpressions
{
    /// <summary>
    /// 各种数据库的表现形式基类
    /// </summary>
    public abstract class DbExpression
    {
        /// <summary>
        /// 当前节点所操作的数据类型
        /// </summary>
        public virtual Type Type { get; private set; }
        /// <summary>
        /// 当前节点的DB表现形式类型
        /// </summary>
        public virtual DbExpressionType NodeType { get; private set; }

        protected DbExpression(DbExpressionType nodeType) : this(nodeType, typeof(void)) { }

        protected DbExpression(DbExpressionType nodeType, Type type)
        {
            this.NodeType = nodeType;
            this.Type = type;
        }

        /// <summary>
        /// 接受一个DB操作表现的实例，并转换成对应的SQL编码
        /// </summary>
        /// <typeparam name="T">DB操作的数据类型</typeparam>
        /// <param name="visitor">DB操作表现翻译器</param>
        /// <returns></returns>
        public abstract T Accept<T>(DbExpressionVisitor<T> visitor);
    }
}
