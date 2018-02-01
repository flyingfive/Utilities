using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace FlyingFive.Data.Fakes
{
    /// <summary>
    /// 操作数据库的参数伪装
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

        public FakeParameter() { }

        public FakeParameter(string name, object value)
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
        public FakeParameter(string name, object value, Type type)
        {
            this.Name = name;
            this.Value = value;
            this.DataType = type;
        }
    }
}
