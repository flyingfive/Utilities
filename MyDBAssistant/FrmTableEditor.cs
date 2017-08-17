using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MyDBAssistant.Schema;
using MyDBAssistant.Data;

namespace MyDBAssistant
{
    public partial class FrmTableEditor : FrmBase
    {
        private int _tableId = 0;
        private ConnectionString _currentConnection = null;
        private MsSqlHelper _mssqlHelper = null;
        private Table _table = null;
        public FrmTableEditor(int tableId, ConnectionString currentConnection)
        {
            InitializeComponent();
            _tableId = tableId;
            _currentConnection = currentConnection;
            SetupColumns();
            this.KeyPreview = true;
            this.Load += FrmTableEditor_Load;
            this.KeyDown += (s, e) => { if (e.KeyCode == Keys.Escape) { btnClose_Click(btnClose, EventArgs.Empty); } if (e.KeyCode == Keys.Enter) { btnSave_Click(btnSave, EventArgs.Empty); } };
            _mssqlHelper = new MsSqlHelper(_currentConnection.ToString());
        }

        void FrmTableEditor_Load(object sender, EventArgs e)
        {
            IList<Column> columns = new List<Column>();
            if (_tableId > 0)
            {
                string sql = "SELECT a.* FROM dbo.v_sys_DataDict a WHERE a.TableId = @TableId ORDER BY a.ColumnOrder";
                columns = _mssqlHelper.ExecuteQueryAsList<Column>(sql, CommandType.Text, new DbParameter[] { new SqlParameter("@TableId", _tableId) });
            }
            BindingList<Column> lst = new BindingList<Column>(columns);
            dgvColumns.DataSource = lst;
            if (_tableId == 0)
            {
                _table = new Table();
                btnTableName.Visible = false;
                btnDescription.Visible = false;
            }
            else
            {
                _table = _mssqlHelper.ExecuteQueryAsList<Table>("SELECT TOP 1 TableId, TableName, TableDescription FROM dbo.v_sys_DataDict WHERE TableId = @TableId", CommandType.Text, new DbParameter[] { new SqlParameter("@TableId", _tableId) }).FirstOrDefault();
                if (_table == null) { throw new DataException(string.Format("数据库中不存在Id为:{0}的对象", _tableId.ToString())); }
                txtName.Text = _table.TableName;
                txtDescription.Text = _table.TableDescription;
                txtName.ReadOnly = true;
                txtDescription.ReadOnly = true;
                btnSave.Enabled = false;
            }
            txtName.Focus();
        }

        private void SetupColumns()
        {
            dgvColumns.EditMode = DataGridViewEditMode.EditOnKeystrokeOrF2;
            var sqlTypes = Column.GetSqlTypes();
            sqlTypes.Insert(0, "--请选择--");
            dgvColumns.Columns.Clear();
            dgvColumns.ReadOnly = _tableId != 0;
            dgvColumns.AutoGenerateColumns = false;
            dgvColumns.MultiSelect = false;
            dgvColumns.Columns.AddRange(new DataGridViewColumn[] {
                new DataGridViewTextBoxColumn(){ Name = "TableId", DataPropertyName = "TableId", Visible = false },
                new DataGridViewTextBoxColumn(){ Name = "ColumnOrder", DataPropertyName = "ColumnOrder", Visible = false },
                new DataGridViewTextBoxColumn(){ Name= "ColumnName", DataPropertyName = "ColumnName", Width = 100, HeaderText = "列名" },
                new DataGridViewComboBoxColumn(){ Name= "SqlType", DataPropertyName = "SqlType", Width = 120, HeaderText = "类型", DropDownWidth = 100, MaxDropDownItems = 5, DataSource = sqlTypes },
                new DataGridViewTextBoxColumn(){ Name= "Precision", DataPropertyName = "Precision", Width = 60, HeaderText = "长度" },
                new DataGridViewTextBoxColumn(){ Name= "Scale", DataPropertyName = "Scale", Width = 60, HeaderText = "精度" },
                new DataGridViewCheckBoxColumn(){ Name= "IsPrimaryKey", DataPropertyName = "IsPrimaryKey", Width = 60, HeaderText = "主键", TrueValue = true, FalseValue = false  },
                new DataGridViewCheckBoxColumn(){ Name= "IsNullable", DataPropertyName = "IsNullable", Width = 60, HeaderText = "可空", TrueValue = true, FalseValue = false },
                new DataGridViewCheckBoxColumn(){ Name= "IsIdentity", DataPropertyName = "IsIdentity", Width = 60, HeaderText = "标识", TrueValue = true, FalseValue = false },
                new DataGridViewTextBoxColumn(){ Name= "DefaultValue", DataPropertyName = "DefaultValue", Width = 100, HeaderText = "默认值" },
                new DataGridViewTextBoxColumn(){ Name= "ColumnDescription", DataPropertyName = "ColumnDescription", Width = 160, HeaderText = "描述"},
            });
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                this.btnSave.Enabled = false;
                this.btnSave.Text = "正在执行...";

                if (string.IsNullOrWhiteSpace(txtName.Text)) { MessageBox.Show("表名必填且不能为空字符串", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
                if (string.IsNullOrWhiteSpace(txtDescription.Text)) { MessageBox.Show("表描述必填且不能为空字符串", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
                if (dgvColumns.Rows.Count == 0) { MessageBox.Show("表结构不能为空!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
                var lst = dgvColumns.DataSource as BindingList<Column>;
                if (lst.Count == 0) { MessageBox.Show("表结构不能为空!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
                bool hasPrimaryKeyDefined = false;
                if (_tableId == 0)         //创建表
                {
                    _table.TableName = txtName.Text.Trim();
                    _table.TableDescription = txtDescription.Text.Trim();
                }
                foreach (Column column in lst)
                {
                    if (string.IsNullOrWhiteSpace(column.ColumnName)) { continue; }
                    if (string.IsNullOrWhiteSpace(column.ColumnDescription))
                    {
                        MessageBox.Show(string.Format("列:{0},缺少描述信息", column.ColumnName), "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                    else
                    {
                        if (chkBatchColumns.Checked)
                        {
                            this.UpdateColumnDescription(txtName.Text.Trim(), column.ColumnName, column.ColumnDescription);
                        }
                    }
                    if (string.IsNullOrWhiteSpace(column.SqlType) || column.SqlType.Contains("请选择")) { MessageBox.Show(string.Format("没有为列:{0}指定数据类型", column.ColumnName), "提示", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
                    if (column.IsPrimaryKey) { hasPrimaryKeyDefined = true; }
                    _table.Columns.Add(column);
                }
                if (_tableId == 0)
                {
                    _table.Columns = lst;
                    if (!hasPrimaryKeyDefined) { MessageBox.Show(string.Format("还没有没表:{0}指定主键!", txtName.Text.Trim()), "提示", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
                    _table.Create(_mssqlHelper);
                    MessageBox.Show(string.Format("创建表表:{0}成功!", _table.TableName), "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = System.Windows.Forms.DialogResult.OK;
                    this.Close();
                    return;
                }
                else
                {
                    if (!txtName.ReadOnly)          //修改表名
                    {
                        string renameSql = string.Format("sp_rename '{0}', '{1}', 'object'", _table.TableName, txtName.Text.Trim());
                        _mssqlHelper.ExecuteNonQuery(renameSql, CommandType.Text);
                        DatabaseSchemaHistory log = new DatabaseSchemaHistory() { Description = string.Format("表名:{0},修改为:{1}", _table.TableName, txtName.Text.Trim()), SqlScript = renameSql };
                        log.Insert(_mssqlHelper);
                        MessageBox.Show("表名修改成功!请注意同步修改数据库中的存储过程,视图...等引用此表的对象~", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    if (!txtDescription.ReadOnly)   //修改描述
                    {
                        bool existsDescriptionSql = Convert.ToInt32(_mssqlHelper.ExecuteQueryAsSingle(string.Format("SELECT ISNULL(COUNT(0), 0) FROM ::fn_listextendedproperty ('MS_Description', 'user', 'dbo', 'table', '{0}', NULL, NULL)", txtName.Text.Trim()), CommandType.Text)) > 0;
                        string sql = string.Format("EXEC sp_addextendedproperty 'MS_Description', '{0}', 'user', dbo, 'table', [{1}]", txtDescription.Text.Trim(), txtName.Text.Trim());
                        if (existsDescriptionSql)
                        {
                            sql = string.Format("EXEC sp_updateextendedproperty 'MS_Description', '{0}', 'user', dbo, 'table', [{1}]", txtDescription.Text.Trim(), txtName.Text.Trim());
                        }
                        _mssqlHelper.ExecuteNonQuery(sql, CommandType.Text);
                        DatabaseSchemaHistory log = new DatabaseSchemaHistory() { Description = string.Format("修改表描述信息", _table.TableName, txtName.Text.Trim()), SqlScript = sql };
                        log.Insert(_mssqlHelper);
                        MessageBox.Show("描述信息已成功修改!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    _table = _mssqlHelper.ExecuteQueryAsList<Table>("SELECT TOP 1 TableId, TableName, TableDescription FROM dbo.v_sys_DataDict WHERE TableId = @TableId", CommandType.Text, new DbParameter[] { new SqlParameter("@TableId", _tableId) }).FirstOrDefault();
                    if (_table == null) { throw new DataException(string.Format("数据库中不存在Id为:{0}的对象", _tableId)); }
                    btnSave.Enabled = false;
                    txtName.ReadOnly = true;
                    txtDescription.ReadOnly = true;
                    this.DialogResult = System.Windows.Forms.DialogResult.OK;
                }

            }
            catch (Exception ex)
            {
            }
            finally
            {
                // TODO: taojf:try catch无意义，主要为了finally能执行  如能用弹出提示 正在执行中更好
                btnSave.Enabled = true;
                this.btnSave.Text = "保存";
            }

        }

        private void dgvColumns_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex < 0 || e.RowIndex < 0) { return; }
            string name = dgvColumns.Columns[e.ColumnIndex].DataPropertyName;
            if (name.Equals("IsPrimaryKey") || name.Equals("IsIdentity"))
            {
                var chk = dgvColumns.CurrentCell as DataGridViewCheckBoxCell;
                //if (chk != null)
                //{
                //    chk.Value = !Convert.ToBoolean(chk.Value);
                //}
            }
        }

        private void Button_ModifyClick(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            if (btn == null) { return; }
            string name = btn.Name.Replace("btn", "").ToLower();
            switch (name)
            {
                case "description":
                    txtDescription.ReadOnly = false;
                    btnSave.Enabled = true;
                    if (chkBatchColumns.Checked)
                    {
                        this.dgvColumns.ReadOnly = false;
                        // taojf:禁止绑定时自动创建新列
                        this.dgvColumns.AllowUserToAddRows = false;
                        this.dgvColumns.AllowUserToDeleteRows = false;
                        this.dgvColumns.EditMode = DataGridViewEditMode.EditOnEnter;
                        this.dgvColumns.CurrentCell = this.dgvColumns.Rows[0].Cells["ColumnDescription"];
                    }
                    break;
                case "tablename": if (MessageBox.Show("修改表名会造成引用此表对象的存储过程,函数,视图等无法正常执行\r\n是否继续?", "警告", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == System.Windows.Forms.DialogResult.Yes) { txtName.ReadOnly = false; btnSave.Enabled = true; } break;
                default: break;
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// 修改列名描述
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="column">列名</param>
        /// <param name="description">描述</param>
        private void UpdateColumnDescription(string tableName, string column, string description)
        {
            bool existsDescriptionSql = Convert.ToInt32(_mssqlHelper.ExecuteQueryAsSingle(string.Format("SELECT ISNULL(COUNT(0), 0) FROM ::fn_listextendedproperty ('MS_Description', 'user', 'dbo', 'table', '{0}', 'column', '{1}')", tableName, column), CommandType.Text)) > 0;
            string sql = string.Format("EXEC sp_addextendedproperty 'MS_Description', '{0}', 'user', dbo, 'table', [{1}], 'column', [{2}]", description.Trim(), tableName, column);
            if (existsDescriptionSql)
            {
                sql = string.Format("EXEC sp_updateextendedproperty 'MS_Description', '{0}', 'user', dbo, 'table', [{1}], 'column', [{2}]", description.Trim(), tableName, column);
            }
            _mssqlHelper.ExecuteNonQuery(sql, CommandType.Text);
            DatabaseSchemaHistory log = new DatabaseSchemaHistory() { Description = string.Format("修改列描述信息", tableName, column), SqlScript = sql };
            log.Insert(_mssqlHelper);
        }

        //private void btnNewColumn_Click(object sender, EventArgs e)
        //{
        //    //var columns = dgvColumns.DataSource as BindingList<Column>;
        //    //if (columns == null) { columns = new BindingList<Column>(); }
        //    ////var newColumn = new Column();
        //    //columns.Add(new Column());
        //    ////dgvColumns.DataSource = null;
        //    //dgvColumns.DataSource = columns;

        //    //dgvColumns.Refresh();

        //    //2、数据绑定后的添加删除问题：如果要对绑定在DataGridView中的List<T>进行数据的添加删除，
        //    //先要把List<T>转换成BindingList<T>，
        //    //再进行绑定：DataGridView1.DataSource=new BindingList<MyClass>(new List<MyClass>())。
        //    //否则的话会产生许多意想不到的错误。如：初始绑定空数据后再添加数据绑定后，
        //    //却取不到DataGridView.CurrentCell属性。
        //}
    }
}
