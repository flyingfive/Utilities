using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using MyDBAssistant.Data;

namespace MyDBAssistant.Schema
{
    public class Table
    {
        public string TableName { get; set; }
        public int TableId { get; set; }
        public bool IsView { get; set; }
        public string TableDescription { get; set; }

        public IList<Column> Columns { get; set; }

        public Table() { Columns = new List<Column>(); }

        public string ExportSheetName
        {
            get
            {
                if (string.IsNullOrWhiteSpace(TableName)) { return "Sheet1"; }
                return TableName.Length > 30 ? TableName.Substring(0, 30) : TableName;
            }
        }
        /// <summary>
        /// 创建表
        /// </summary>
        /// <param name="helper"></param>
        public void Create(MsSqlHelper helper)
        {
            string createTableSql = MakeCreateTableSql();
            helper.ExecuteNonQuery(createTableSql, System.Data.CommandType.Text);
            string appendExtendPropertiesSql = MakeExtendPropertiesSql();
            helper.ExecuteNonQuery(appendExtendPropertiesSql, System.Data.CommandType.Text);
            DatabaseSchemaHistory log = new DatabaseSchemaHistory() { Description = string.Format("创建表:{0}", TableName), SqlScript = string.Format("{0}{1}{2}", createTableSql, Environment.NewLine, appendExtendPropertiesSql) };
            log.Insert(helper);
        }

        public string MakeExtendPropertiesSql()
        {
            StringBuilder sb = new StringBuilder();
            //sb.AppendFormat("EXEC sp_addextendedproperty 'MS_Description', '{0}', 'user', dbo, 'table', [{1}]", TableDescription, TableName);
            //sb.AppendLine();


            //bool existsDescriptionSql = _mssqlHelper.ExecuteQueryAsSingle<int>(string.Format("SELECT ISNULL(COUNT(0), 0) FROM ::fn_listextendedproperty ('MS_Description', 'user', 'dbo', 'table', '{0}', 'column', '{1}')", _table.TableName, txtName.Text.Trim()), CommandType.Text) > 0;
            //string sql = string.Format("EXEC sp_addextendedproperty 'MS_Description', '{0}', 'user', dbo, 'table', [{1}], 'column', [{2}]", txtDescription.Text.Trim(), _table.TableName, txtName.Text.Trim());
            //if (existsDescriptionSql)
            //{
            //    sql = string.Format("EXEC sp_updateextendedproperty 'MS_Description', '{0}', 'user', dbo, 'table', [{1}], 'column', [{2}]", txtDescription.Text.Trim(), _table.TableName, txtName.Text.Trim());
            //}

            sb.AppendLine(string.Format("IF ((SELECT COUNT(0) FROM ::fn_listextendedproperty ('MS_Description', 'user', 'dbo', 'table', '{0}', NULL, NULL)) = 0)", TableName));
            sb.AppendLine("BEGIN");
            sb.AppendLine(string.Format("EXEC sp_addextendedproperty 'MS_Description', '{0}', 'user', dbo, 'table', [{1}]", TableDescription, TableName));
            sb.AppendLine("END");
            sb.AppendLine("ELSE");
            sb.AppendLine("BEGIN");
            sb.AppendLine(string.Format("EXEC sp_updateextendedproperty 'MS_Description', '{0}', 'user', dbo, 'table', [{1}]", TableDescription, TableName));
            sb.AppendLine("END");
            foreach (Column column in Columns)
            {
                if (string.IsNullOrWhiteSpace(column.ColumnName) || string.IsNullOrWhiteSpace(column.ColumnDescription)) { continue; }
                //sb.AppendLine(string.Format("EXEC sp_addextendedproperty 'MS_Description', '{0}', 'user', dbo, 'table', [{1}], 'column', [{2}]", column.ColumnDescription, TableName, column.ColumnName));

                sb.AppendLine(string.Format("IF ((SELECT COUNT(0) FROM ::fn_listextendedproperty ('MS_Description', 'user', 'dbo', 'table', '{0}', 'column', '{1}')) = 0)", TableName, column.ColumnName));
                sb.AppendLine("BEGIN");
                sb.AppendLine(string.Format("EXEC sp_addextendedproperty 'MS_Description', '{0}', 'user', dbo, 'table', [{1}], 'column', '{2}'", column.ColumnDescription, TableName, column.ColumnName));
                sb.AppendLine("END");
                sb.AppendLine("ELSE");
                sb.AppendLine("BEGIN");
                sb.AppendLine(string.Format("EXEC sp_updateextendedproperty 'MS_Description', '{0}', 'user', dbo, 'table', [{1}], 'column', '{2}'", column.ColumnDescription, TableName, column.ColumnName));
                sb.AppendLine("END");

            }
            return sb.ToString();
        }

        private string MakeCreateTableSql()
        {
            if (Columns == null || Columns.Count == 0) { return ""; }
            if (string.IsNullOrWhiteSpace(TableName)) { return ""; }
            StringBuilder sb = new StringBuilder();
            string pkNames = "";
            //sb.AppendFormat("CREATE TABLE [{0}]", TableName);
            //sb.AppendLine();
            sb.AppendLine(string.Format("CREATE TABLE [{0}]", TableName));
            sb.AppendLine("(");
            foreach (Column column in Columns)
            {
                //sb.AppendFormat("{0} ,", column.ToString());
                //sb.AppendLine();
                sb.AppendLine(string.Format("{0} ,", column.CreateColumnSql()));
                if (column.IsPrimaryKey)
                {
                    pkNames = string.Concat(pkNames, "[", column.ColumnName, "]", ",");
                }
            }
            //sb = sb.Remove(sb.ToString().LastIndexOf(","), 1);
            pkNames = pkNames.Substring(0, pkNames.Length - 1);
            pkNames = string.Format("CONSTRAINT pk_{0} PRIMARY KEY CLUSTERED ({1}) ON [PRIMARY]", TableName, pkNames);//"PRIMARY KEY CLUSTERED ({0}) ON [PRIMARY]", pkNames);
            sb.AppendLine(pkNames);
            sb.AppendLine(")");
            return sb.ToString();
        }

        public string GetClassName(string prefix)
        {
            if (!string.IsNullOrWhiteSpace(prefix))
            {
                var name = this.TableName
                    .Replace(prefix, string.Empty)
                    .Split(new char[] { '_' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(i => i.Substring(0, 1).ToUpper() + i.Substring(1)).Aggregate((s1, s2) => { return string.Concat(s1, s2); });
                return name;
            }
            else
            {
                var name = this.TableName
                    //.Replace(prefix, string.Empty)
                    .Split(new char[] { '_' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(i => i.Substring(0, 1).ToUpper() + i.Substring(1)).Aggregate((s1, s2) => { return string.Concat(s1, s2); });
                return name;
            }
        }

    }
}
