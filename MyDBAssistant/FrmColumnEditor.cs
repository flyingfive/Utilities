using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MyDBAssistant.Schema;
using MyDBAssistant.Data;

namespace MyDBAssistant
{
    public partial class FrmColumnEditor : Form
    {
        private Column _column = null;
        private Table _table = null;
        private MsSqlHelper _mssqlHelper = null;
        private ConnectionString _currentConnection = null;
        private int _tableId = 0;
        private string _columnName = "";
        public FrmColumnEditor(int tableId, string columnName)
        {
            InitializeComponent();
            _tableId = tableId;
            _columnName = columnName;
            this.KeyPreview = true;
            this.btnClose.Click += (s, e) => { this.DialogResult = System.Windows.Forms.DialogResult.No; this.Close(); };
            this.KeyDown += (s, e) => { if (e.KeyCode == Keys.Escape) { this.DialogResult = System.Windows.Forms.DialogResult.No; this.Close(); } };
            this.Load += FrmColumnEditor_Load;
        }

        void FrmColumnEditor_Load(object sender, EventArgs e)
        {
            FrmMain frmMain = this.Owner as FrmMain;
            if (frmMain == null) { Application.Exit(); return; }
            _currentConnection = frmMain.CurrentConnecttion;
            _mssqlHelper = new MsSqlHelper(_currentConnection.ToString());
            _table = _mssqlHelper.ExecuteQueryAsList<Table>("SELECT TOP 1 TableId, TableName, TableDescription FROM dbo.v_sys_DataDict WHERE TableId = @TableId", CommandType.Text, new DbParameter[] { new SqlParameter("@TableId", _tableId) }).FirstOrDefault();
            var sqlTypes = Column.GetSqlTypes();
            sqlTypes.Insert(0, "--请选择--");
            cboSqlType.Items.AddRange(sqlTypes.ToArray());
            if (_table == null) { throw new DataException(string.Format("数据库中不存在Id为:{0}的对象", _tableId.ToString())); }
            if (!string.IsNullOrEmpty(_columnName))             //编辑列
            {
                _column = _mssqlHelper.ExecuteQueryAsList<Column>("SELECT * FROM dbo.v_sys_DataDict WHERE TableId = @TableId AND ColumnName = @ColumnName", CommandType.Text, new DbParameter[] { new SqlParameter("@TableId", _tableId), new SqlParameter("@ColumnName", _columnName) }).FirstOrDefault();
                if (_column == null) { throw new DataException(string.Format("对象Id为:{0}不包含列:{1}", _tableId.ToString(), _columnName)); }
                txtName.Text = _column.ColumnName;
                txtDescription.Text = _column.ColumnDescription;
                cboSqlType.SelectedIndex = sqlTypes.IndexOf(_column.SqlType.ToLower());//.SelectedText = _column.SqlType;
                txtPrecision.Text = _column.Precision.ToString();
                txtScale.Text = _column.Scale.ToString();
                chkNullable.Checked = _column.IsNullable;
                chkPK.Checked = _column.IsPrimaryKey;
                chkIdentity.Checked = _column.IsIdentity;
                txtDefault.Text = _column.DefaultValue;
                chkPK.Enabled = false;
                txtName.ReadOnly = true;
                txtDescription.ReadOnly = true;
                txtDefault.ReadOnly = true;                 //不修改默认值
                chkIdentity.Enabled = false;                //不修改标识
            }
            else
            {
                btnName.Visible = false;
                btnDescription.Visible = false;
            }
            txtName.Focus();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(string.Format("ALTER TABLE [{0}]", _table.TableName));
            if (!string.IsNullOrEmpty(_columnName))          //修改列
            {
                Column newColumn = new Column() { ColumnName = _column.ColumnName, SqlType = cboSqlType.Text, IsNullable = chkNullable.Checked, Precision = Convert.ToInt32(txtPrecision.Text.Trim()), Scale = Convert.ToInt32(txtScale.Text.Trim()) };
                sb.AppendLine(string.Format("ALTER COLUMN {0}", newColumn.CreateColumnSql()));
                _mssqlHelper.ExecuteNonQuery(sb.ToString(), CommandType.Text);
                DatabaseSchemaHistory log = new DatabaseSchemaHistory() { Description = string.Format("修改表:{0}.{1}", _table.TableName, _column.ColumnName), SqlScript = sb.ToString() };
                log.Insert(_mssqlHelper);
                if (!txtName.ReadOnly)
                {
                    var lst = _mssqlHelper.ExecuteQueryAsList<Column>("SELECT a.* FROM dbo.v_sys_DataDict a WHERE a.TableId = @TableId ORDER BY a.ColumnOrder",
                        CommandType.Text, new DbParameter[] { new SqlParameter("@TableId", _tableId) });
                    var column = lst.Where(c => c.ColumnName.Equals(txtName.Text.Trim())).SingleOrDefault();
                    if (column != null) { MessageBox.Show(string.Format("表:{0}中已存在列:{1},请重新指定表名!", _table.TableName, txtName.Text.Trim()), "提示", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
                    newColumn.ColumnName = txtName.Text.Trim();
                    string renameSql = string.Format("sp_rename '{0}.{1}', '{2}', 'column'", _table.TableName, _column.ColumnName, txtName.Text.Trim());
                    _mssqlHelper.ExecuteNonQuery(renameSql, CommandType.Text);
                    log = new DatabaseSchemaHistory() { Description = string.Format("列名:{0},修改为:{1}", _column.ColumnName, txtName.Text.Trim()), SqlScript = renameSql };
                    log.Insert(_mssqlHelper);
                }
                if (!txtDescription.ReadOnly)
                {
                    bool existsDescriptionSql = Convert.ToInt32(_mssqlHelper.ExecuteQueryAsSingle(string.Format("SELECT ISNULL(COUNT(0), 0) FROM ::fn_listextendedproperty ('MS_Description', 'user', 'dbo', 'table', '{0}', 'column', '{1}')", _table.TableName, txtName.Text.Trim()), CommandType.Text)) > 0;
                    string sql = string.Format("EXEC sp_addextendedproperty 'MS_Description', '{0}', 'user', dbo, 'table', [{1}], 'column', [{2}]", txtDescription.Text.Trim(), _table.TableName, txtName.Text.Trim());
                    if (existsDescriptionSql)
                    {
                        sql = string.Format("EXEC sp_updateextendedproperty 'MS_Description', '{0}', 'user', dbo, 'table', [{1}], 'column', [{2}]", txtDescription.Text.Trim(), _table.TableName, txtName.Text.Trim());
                    }
                    _mssqlHelper.ExecuteNonQuery(sql, CommandType.Text);
                    log = new DatabaseSchemaHistory() { Description = string.Format("修改列描述信息", _table.TableName, txtName.Text.Trim()), SqlScript = sql };
                    log.Insert(_mssqlHelper);
                }
                _column = _mssqlHelper.ExecuteQueryAsList<Column>("SELECT * FROM dbo.v_sys_DataDict WHERE TableId = @TableId AND ColumnName = @ColumnName", CommandType.Text, new DbParameter[] { new SqlParameter("@TableId", _tableId), new SqlParameter("@ColumnName", txtName.Text.Trim()) }).FirstOrDefault();
                if (_column == null) { throw new DataException(string.Format("对象Id为:{0}不包含列:{1}", _tableId.ToString(), _columnName)); }
                _columnName = _column.ColumnName;
                MessageBox.Show(string.Format("列信息更新成功!{0}", txtName.ReadOnly ? "" : "\r\n请注意同步修改数据库中的存储过程,视图...等引用此列的对象~"), "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else                                             //新增列
            {
                if (string.IsNullOrWhiteSpace(txtName.Text)) { MessageBox.Show("列名不能为空!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
                if (string.IsNullOrWhiteSpace(txtDescription.Text)) { MessageBox.Show("描述必填!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
                if (cboSqlType.SelectedIndex <= 0) { MessageBox.Show("请选择数据类型!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
                //string sql = "SELECT a.* FROM dbo.v_sys_DataDict a WHERE a.TableId = @TableId ORDER BY a.ColumnOrder";
                var lst = _mssqlHelper.ExecuteQueryAsList<Column>("SELECT a.* FROM dbo.v_sys_DataDict a WHERE a.TableId = @TableId ORDER BY a.ColumnOrder",
                    CommandType.Text, new DbParameter[] { new SqlParameter("@TableId", _tableId) });
                var column = lst.Where(c => c.ColumnName.Equals(txtName.Text.Trim())).SingleOrDefault();
                if (column != null) { MessageBox.Show(string.Format("表:{0}中已存在列:{1},请重新指定表名!", _table.TableName, txtName.Text.Trim()), "提示", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
                Column newColumn = new Column() { ColumnName = txtName.Text.Trim(), SqlType = cboSqlType.Text, IsNullable = chkNullable.Checked, Precision = string.IsNullOrWhiteSpace(txtPrecision.Text) ? 0 : Convert.ToInt32(txtPrecision.Text.Trim()), Scale = string.IsNullOrWhiteSpace(txtScale.Text) ? 0 : Convert.ToInt32(txtScale.Text.Trim()), DefaultValue = txtDefault.Text.Trim(), IsIdentity = chkIdentity.Checked, ColumnDescription = txtDescription.Text.Trim() };
                sb.AppendLine(string.Format("ADD {0}", newColumn.CreateColumnSql()));
                _mssqlHelper.ExecuteNonQuery(sb.ToString(), CommandType.Text);
                string sql = string.Format("EXEC sp_addextendedproperty 'MS_Description', '{0}', 'user', dbo, 'table', [{1}], 'column', [{2}]", newColumn.ColumnDescription, _table.TableName, newColumn.ColumnName);
                _mssqlHelper.ExecuteNonQuery(sql, CommandType.Text);
                DatabaseSchemaHistory log = new DatabaseSchemaHistory() { Description = string.Format("修改表:{0},添加字段:{1}", _table.TableName, newColumn.ColumnName), SqlScript = string.Concat(sb.ToString(), Environment.NewLine, sql) };
                log.Insert(_mssqlHelper);
                MessageBox.Show(string.Format("添加列成功!"), "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
            txtName.ReadOnly = true;
            txtDescription.ReadOnly = true;
        }

        private void Button_ModifyColumnClick(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            if (btn == null) { return; }
            string name = btn.Name.Replace("btn", "");
            switch (name.ToLower())
            {
                case "name": if (MessageBox.Show("修改列名会造成引用到此列对象的存储过程,函数,视图等无法正常执行\r\n是否继续?", "警告", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == System.Windows.Forms.DialogResult.Yes) { txtName.ReadOnly = false; } break;
                case "description": txtDescription.ReadOnly = false; break;
                default: break;
            }
        }
    }
}
