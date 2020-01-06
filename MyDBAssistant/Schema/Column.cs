using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyDBAssistant.Schema
{
    public class Column
    {
        /// <summary>
        /// 表ID
        /// </summary>
        public int TableId { get; set; }
        /// <summary>
        /// 列顺序
        /// </summary>
        public int ColumnOrder { get; set; }
        /// <summary>
        /// 列名
        /// </summary>
        public string ColumnName { get; set; }
        /// <summary>
        /// 是否标识
        /// </summary>
        public bool IsIdentity { get; set; }
        /// <summary>
        /// 是否主键
        /// </summary>
        public bool IsPrimaryKey { get; set; }
        /// <summary>
        /// SQL数据类型
        /// </summary>
        public string SqlType { get; set; }
        /// <summary>
        /// C#数据类型
        /// </summary>
        public string CSharpType { get { string typeName = GetCSharpType(SqlType, "String", IsNullable); return typeName; /*Type t = Type.GetType("System." + typeName); if (t.IsValueType && IsNullable) { typeName = string.Concat(typeName, "?"); } return typeName;*/ } }
        /// <summary>
        /// 大小
        /// </summary>
        public int Size { get; set; }
        //长度
        public int Precision { get; set; }
        /// <summary>
        /// 精度
        /// </summary>
        public int Scale { get; set; }
        /// <summary>
        /// 是否可空
        /// </summary>
        public bool IsNullable { get; set; }
        /// <summary>
        /// 默认值
        /// </summary>
        public string DefaultValue { get; set; }
        /// <summary>
        /// 列描述
        /// </summary>
        public string ColumnDescription { get; set; }
        /// <summary>
        /// SQL用户自定义类型名称
        /// </summary>
        public string UserTypeName { get; set; }
        /// <summary>
        /// 是否用户自定义类型
        /// </summary>
        public bool IsUserType { get; set; }
        /// <summary>
        /// 分隔符
        /// </summary>
        public string Splitter { get; set; }
        /// <summary>
        /// C#属性名称
        /// </summary>
        public string PropertyName
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Splitter))
                {
                    return string.Concat(ColumnName.Substring(0, 1).ToUpper(), ColumnName.Substring(1).ToLower());
                }
                else
                {
                    string[] parts = ColumnName.Split(new string[] { Splitter }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length > 1)
                    {
                        string propName = parts.Aggregate((s1, s2) => { return string.Concat(s1.Substring(0, 1).ToUpper(), s1.Substring(1).ToLower(), s2.Substring(0, 1).ToUpper(), s2.Substring(1).ToLower()); });
                        //string propName = "";
                        //parts.ToList().ForEach(s => propName = string.Concat(propName, s.Substring(0, 1).ToUpper(), s.Substring(1).ToLower()));
                        return propName;
                    }
                    else
                    {
                        return string.Concat(ColumnName.Substring(0, 1).ToUpper(), ColumnName.Substring(1).ToLower());
                    }
                }
            }
        }
        public string CreateColumnSql()
        {
            StringBuilder sb = new StringBuilder();
            string type = MakeSqlType();
            string valueType = "int,bigint,smallint,float,double,decimal,numeric,money,smallmoney,bit,tinyint";
            string defaultValue = string.IsNullOrEmpty(DefaultValue) ? "" : (valueType.IndexOf(SqlType, StringComparison.CurrentCultureIgnoreCase) >= 0 ? DefaultValue :       //valueType
                (DefaultValue.Contains("()") ? DefaultValue : string.Concat("'", DefaultValue, "'")));                               //mssql internal functions...
            sb.AppendFormat("[{0}] {1} {2} {3} {4}", ColumnName, type,
                IsIdentity && "smallint,int,bigint".IndexOf(SqlType, StringComparison.CurrentCultureIgnoreCase) >= 0 ? "IDENTITY(1, 1)" : "",         //int型自增列
                IsNullable && !IsPrimaryKey ? "NULL" : "NOT NULL",
                string.IsNullOrWhiteSpace(DefaultValue) ? "" : string.Format("DEFAULT({0})", defaultValue));
            return sb.ToString();
        }

        private string MakeSqlType()
        {
            string type = "";
            string withLenghTypes = "char,nchar,nvarchar,varchar,binary,varbinary";
            string withScaleTypes = "numeric,decimal";
            if (withLenghTypes.IndexOf(SqlType, StringComparison.CurrentCultureIgnoreCase) >= 0)
            {
                type = string.Format("[{0}]({1})", SqlType.ToLower(), Precision <= 0 ? "1" : (Precision > 4000 ? "4000" : Precision.ToString()));
            }
            else if (withScaleTypes.IndexOf(SqlType, StringComparison.CurrentCultureIgnoreCase) >= 0)
            {
                type = string.Format("[{0}]({1},{2})", SqlType.ToLower(), Precision <= 0 ? "18" : Precision.ToString(), Scale < 0 ? "0" : Scale.ToString());
            }
            else
            {
                type = string.Format("[{0}]", SqlType.ToLower());
            }
            return type;
        }

        public static IList<string> GetSqlTypes()
        {
            IList<string> sqlTypes = new List<string>();
            sqlTypes.Add("char");
            sqlTypes.Add("nchar");
            sqlTypes.Add("nvarchar");
            sqlTypes.Add("text");
            sqlTypes.Add("ntext");
            sqlTypes.Add("varchar");
            sqlTypes.Add("datetime");
            sqlTypes.Add("binary");
            sqlTypes.Add("varbinary");
            sqlTypes.Add("timestamp");
            sqlTypes.Add("image");
            sqlTypes.Add("tinyint");
            sqlTypes.Add("smallint");
            sqlTypes.Add("int");
            sqlTypes.Add("bigint");
            sqlTypes.Add("float");
            sqlTypes.Add("bit");
            sqlTypes.Add("money");
            sqlTypes.Add("smallmoney");
            sqlTypes.Add("numeric");
            sqlTypes.Add("decimal");
            sqlTypes.Add("uniqueidentifier");
            return sqlTypes;
        }

        public static string GetCSharpType(string dbType, string defaultType, bool nullable)
        {
            switch (dbType.ToLower())
            {
                case "char":
                case "nchar":
                case "nvarchar":
                case "text":
                case "ntext":
                case "varchar": return "string";
                case "date":
                case "datetime": return nullable ? "DateTime?" : "DateTime";
                case "binary":
                case "varbinary":
                case "timestamp":
                case "image": return "Byte[]";
                case "tinyint":
                case "smallint":
                case "int": return nullable ? "Int32?" : "Int32";
                case "bigint": return nullable ? "Int64?" : "Int64";
                case "float": return nullable ? "double?" : "double";
                case "bit": return nullable ? "bool?" : "bool";
                case "money":
                case "smallmoney":
                case "numeric":
                case "decimal": return nullable ? "decimal?" : "decimal";
                case "uniqueidentifier": return nullable ? "Guid?" : "Guid";
                default: if (string.IsNullOrWhiteSpace(defaultType)) { return "unknown"; } else { return defaultType; };
            }
        }
    }
}
