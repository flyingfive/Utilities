﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace FlyingFive.Data.Fakes
{
    /// <summary>
    /// 表示数据库的伪装参数
    /// </summary>
    public class FakeParameter
    {
        /// <summary>
        /// 参数名称
        /// </summary>
        public string Name { get; set; }
        private object _value = null;
        /// <summary>
        /// 参数值
        /// </summary>
        public virtual object Value { get { return _value; } set { _value = value; if (_value != null) { DataType = _value.GetType(); } } }
        /// <summary>
        /// 参数类型
        /// </summary>
        public DbType? DbType { get; set; }
        /// <summary>
        /// 数字总精度
        /// </summary>
        public byte? Precision { get; set; }
        /// <summary>
        /// 小数位数
        /// </summary>
        public byte? Scale { get; set; }
        /// <summary>
        /// 字节数
        /// </summary>
        public int? Size { get; set; }
        /// <summary>
        /// 参数数据类型
        /// </summary>
        public Type DataType { get; set; }
        /// <summary>
        /// 参数方向
        /// </summary>
        public ParameterDirection ParameterDirection { get; set; }
        /// <summary>
        /// 确切的实际DB参数(如果该属性存在值，则忽略其它属性创建DB参数而直接使用该属性值)
        /// </summary>
        public IDbDataParameter ExplicitParameter { get; set; }

        public FakeParameter() { ParameterDirection = ParameterDirection.Input; }

        public FakeParameter(string name, object value) : this()
        {
            this.Name = name;
            this.Value = value;
        }

        /// <summary>
        /// 创建参数
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <param name="value">参数值</param>
        /// <param name="type">参数类型</param>
        public FakeParameter(string name, object value, Type type) : this(name, value)
        {
            this.DataType = type;
        }

        /// <summary>
        /// 创建参数
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <param name="value">参数值</param>
        /// <returns></returns>
        public static FakeParameter Create(string name, object value)
        {
            return new FakeParameter(name, value);
        }

        /// <summary>
        /// 创建参数
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <param name="value">参数值</param>
        /// <param name="type">参数类型</param>
        public static FakeParameter Create(string name, object value, Type type)
        {
            return new FakeParameter(name, value, type);
        }
    }

    /// <summary>
    /// 表示输出参数
    /// </summary>
    public class OutputParameter
    {
        private FakeParameter _fakeParameter = null;
        private IDbDataParameter _dbParameter = null;

        public OutputParameter(FakeParameter param, IDbDataParameter parameter)
        {
            this._fakeParameter = param;
            this._dbParameter = parameter;
        }

        /// <summary>
        /// 执行完DB操作后从实际的DB参数中将值映射到伪装参数
        /// </summary>
        public void MapValue()
        {
            object val = this._dbParameter.Value;
            if (val == DBNull.Value)
                this._fakeParameter.Value = null;
            else
                this._fakeParameter.Value = val;
        }

        /// <summary>
        /// DB操作完成后批量映射输出参数的实际值到伪装参数上
        /// </summary>
        /// <param name="outputParameters">输出参数列表</param>
        public static void CallMapValue(IList<OutputParameter> outputParameters)
        {
            if (outputParameters != null)
            {
                for (int i = 0; i < outputParameters.Count; i++)
                {
                    outputParameters[i].MapValue();
                }
            }
        }
    }

    /// <summary>
    /// MsSql数据库的伪装参数
    /// </summary>
    public class FakeMsSqlParameter : FakeParameter
    {
        public SqlDbType SqlDbType { get; set; }

        public override object Value
        {
            get
            {
                return base.Value;
            }
            set
            {
                base.Value = value;
                if (base.Value != null) { this.DataType.ToSqlDbType(); }
            }
        }

        public FakeMsSqlParameter(string name, object value) : base(name, value) { }
        public FakeMsSqlParameter(string name, object value, Type type)
            : base(name, value, type)
        {
            this.SqlDbType = type.ToSqlDbType();
        }

        /// <summary>
        /// 列出MsSql语法中的参数变量名称
        /// </summary>
        /// <param name="sql">SQL文本</param>
        /// <returns></returns>
        public static List<string> FindQueryParmeters(string sql)
        {
            var lst = new List<string>();
            //修正正则表达式匹配参数时，Sql中包括@@rowcount之类的变量的情况，不应该算作参数
            Regex paramReg = new Regex(@"[^@@](?<p>@\w+)");
            MatchCollection matches = paramReg.Matches(String.Concat(sql, " "));
            foreach (Match m in matches)
            {
                lst.Add(m.Groups["p"].Value.Trim());
            }
            return lst;
        }
    }
}
