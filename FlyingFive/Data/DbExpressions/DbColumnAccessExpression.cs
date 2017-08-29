using FlyingFive.Data.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlyingFive.Data.DbExpressions
{
    /// <summary>
    /// 数据库列访问操作的表现
    /// </summary>
    public class DbColumnAccessExpression : DbExpression
    {
        public DbTable Table { get; private set; }
        public DbColumn Column { get; private set; }

        public DbColumnAccessExpression(DbTable table, DbColumn column)
            : base(DbExpressionType.ColumnAccess, column.Type)
        {
            this.Table = table;
            this.Column = column;
        }

        /// <summary>
        /// 接受一个DB列访问的操作表达式
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="visitor"></param>
        /// <returns></returns>
        public override T Accept<T>(DbExpressionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
