using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlyingFive.Data.Infrastructure
{
    /// <summary>
    /// SQL代码构造器
    /// </summary>
    public interface ISqlBuilder
    {
        /// <summary>
        /// 转成SQL语句
        /// </summary>
        /// <returns></returns>
        string ToSql();
        /// <summary>
        /// 追加SQL代码
        /// </summary>
        /// <param name="obj">SQL代码片段1</param>
        /// <returns></returns>
        ISqlBuilder Append(object obj);
        /// <summary>
        /// 追加SQL代码
        /// </summary>
        /// <param name="obj1">SQL代码片段1</param>
        /// <param name="obj2">SQL代码片段2</param>
        /// <returns></returns>
        ISqlBuilder Append(object obj1, object obj2);
        /// <summary>
        /// 追加SQL代码
        /// </summary>
        /// <param name="obj1">SQL代码片段1</param>
        /// <param name="obj2">SQL代码片段2</param>
        /// <param name="obj3">SQL代码片段3</param>
        /// <returns></returns>
        ISqlBuilder Append(object obj1, object obj2, object obj3);
        /// <summary>
        /// 追加SQL代码
        /// </summary>
        /// <param name="objs">SQL代码片段</param>
        /// <returns></returns>
        ISqlBuilder Append(params object[] objs);
    }

    /// <summary>
    /// SQL代码构造器
    /// </summary>
    public class SqlBuilder : ISqlBuilder
    {
        private StringBuilder _sb = null;

        /// <summary>
        /// 初始化SQL代码构造器实例
        /// </summary>
        public SqlBuilder()
        {
            this._sb = new StringBuilder();
        }

        public string ToSql()
        {
            return this._sb.ToString();
        }

        public ISqlBuilder Append(object obj)
        {
            this._sb.Append(obj);
            return this;
        }

        public ISqlBuilder Append(object obj1, object obj2)
        {
            return this.Append(obj1).Append(obj2);
        }

        public ISqlBuilder Append(object obj1, object obj2, object obj3)
        {
            return this.Append(obj1).Append(obj2).Append(obj3);
        }

        public ISqlBuilder Append(params object[] objs)
        {
            if (objs == null || objs.Length <= 0) { return this; }
            foreach (var obj in objs)
            {
                if (obj == null) { continue; }
                this.Append(obj);
            }
            return this;
        }
    }
}
