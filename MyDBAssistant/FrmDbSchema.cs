using MyDBAssistant.Data;
using MyDBAssistant.Schema;
using FlyingFive.Data;
using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using System.Data.Common;
using System.IO;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using System.Reflection;
using System.Text;

namespace MyDBAssistant
{
    public partial class FrmDbSchema : FrmBase
    {
        private ConnectionString _currentConnection = null;
        private MsSqlHelper _mssqlHelper = null;
        private ToolStripMenuItem _menuItem = null;

        private CurrentRow columnCurrRow = new CurrentRow();

        private CurrentRow tableCurrRow = new CurrentRow();

        //private int scrollingRowIndex = 0;
        //private int currentRowIndex = 0;

        public FrmDbSchema()
        {
            this.InitializeComponent();
            base.Load += new EventHandler(this.FrmDbSchema_Load);
            _menuItem = new ToolStripMenuItem("数据库&D", null, new ToolStripItem[] { 
                new ToolStripMenuItem("创建表&C", null, new EventHandler((s, arg) => { CreateTable(); })){ Name = "tsmiCreateTable", DisplayStyle = ToolStripItemDisplayStyle.Text },
                new ToolStripMenuItem("删除表&D", null, new EventHandler((s, arg) => { DropTable(); })){ Name = "tsmiDropTable", DisplayStyle = ToolStripItemDisplayStyle.Text },
                new ToolStripMenuItem("添加列&A", null, new EventHandler((s, arg) => { AddColumn(); })){ Name = "tsmiAddColumn", DisplayStyle = ToolStripItemDisplayStyle.Text },
                new ToolStripMenuItem("删除列&R", null, new EventHandler((s, arg) => { DropColumn(); })){ Name = "tsmiDropColumn", DisplayStyle = ToolStripItemDisplayStyle.Text },
                new ToolStripMenuItem("添加脚本&S", null, new EventHandler((s, arg) => { MakeScript(); })){ Name = "tsmiMakeScript", DisplayStyle = ToolStripItemDisplayStyle.Text },
                new ToolStripMenuItem("导入&I", null, new EventHandler((s, arg) => { Import(); })){ Name = "tsmiImport", DisplayStyle = ToolStripItemDisplayStyle.Text },
                new ToolStripMenuItem("导出&E", null, new EventHandler((s, arg) => { Export(); })){ Name = "tsmiExport", DisplayStyle = ToolStripItemDisplayStyle.Text },
            }) { Name = "tsmiDB" };
            this.FormClosed += (s, e) =>
            {
                FrmMain mdiParent = base.MdiParent as FrmMain;
                if (mdiParent == null) { return; }
                mdiParent.RemoveMenu(_menuItem);
            };
        }

        private void BindColumns(int tableId)
        {
            string str = "SELECT a.* FROM dbo.v_sys_DataDict a WHERE a.TableId = @TableId ORDER BY a.ColumnOrder";
            IList<Column> list = this._mssqlHelper.ExecuteQueryAsList<Column>(str, CommandType.Text, new DbParameter[] { new SqlParameter("@TableId", tableId) });
            this.dgvColumns.DataSource = list;
            this.dgvColumns.FirstDisplayedScrollingRowIndex = this.columnCurrRow.ScrollingIndex;
            this.dgvColumns.Rows[this.columnCurrRow.SelectIndex].Selected = true;
            this.dgvColumns.CurrentCell = this.dgvColumns[3, this.dgvColumns.Rows[this.columnCurrRow.SelectIndex].Index];
        }

        private void BindTables()
        {
            Table current = null;
            if (dgvTables.SelectedRows.Count > 0)
            {
                current = this.dgvTables.SelectedRows[0].DataBoundItem as Table;
            }
            string str = "SELECT DISTINCT TableName, TableId, TableDescription,IsView FROM dbo.v_sys_DataDict WHERE TableName NOT IN('v_sys_DataDict', 'sys_DatabaseSchemaHistory') ORDER BY TableName";
            IList<Table> list = this._mssqlHelper.ExecuteQueryAsList<Table>(str, CommandType.Text, new DbParameter[0]);
            this.dgvTables.DataSource = list;
            if (current != null)
            {
                var row = dgvTables.Rows.OfType<DataGridViewRow>().Where(r => ((r.DataBoundItem as Table).TableName.Equals(current.TableName, StringComparison.CurrentCultureIgnoreCase))).SingleOrDefault();
                if (row != null) { row.Selected = true; }
                dgvTables.FirstDisplayedCell = row.Cells["TableName"];

                this.dgvTables.FirstDisplayedScrollingRowIndex = this.tableCurrRow.ScrollingIndex;
                this.dgvTables.Rows[this.tableCurrRow.SelectIndex].Selected = true;
                this.dgvTables.CurrentCell = this.dgvTables[1, this.dgvTables.Rows[this.tableCurrRow.SelectIndex].Index];
            }
        }

        private void AddColumn()
        {
            if (this.dgvTables.CurrentRow != null)
            {
                Table dataBoundItem = this.dgvTables.CurrentRow.DataBoundItem as Table;
                if (dataBoundItem != null)
                {
                    new FrmColumnEditor(dataBoundItem.TableId, "").ShowDialog(this);
                    this.BindColumns(dataBoundItem.TableId);
                }
            }
        }

        private void CreateTable()
        {
            FrmTableEditor editor2 = new FrmTableEditor(0, this._currentConnection)
            {
                StartPosition = FormStartPosition.CenterParent
            };
            editor2.ShowDialog(this);
            this.BindTables();
        }

        private void DropColumn()
        {
            if ((this.dgvTables.CurrentRow != null) && (this.dgvColumns.CurrentRow != null))
            {
                Table dataBoundItem = this.dgvTables.CurrentRow.DataBoundItem as Table;
                Column column = this.dgvColumns.CurrentRow.DataBoundItem as Column;
                if (((dataBoundItem != null) && (column != null)) && (MessageBox.Show("删除列会造成引用该列的存储过程,函数,视图等无法正确运行,且数据会丢失.\r\n确认继续?", "警告", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes))
                {
                    try
                    {
                        string cmdText = string.Format("ALTER TABLE [{0}] DROP COLUMN [{1}]", dataBoundItem.TableName, column.ColumnName);
                        this._mssqlHelper.ExecuteNonQuery(cmdText, CommandType.Text, new DbParameter[0]);
                        DatabaseSchemaHistory history2 = new DatabaseSchemaHistory
                        {
                            Description = string.Format("修改表：{0},删除列:{1}", dataBoundItem.TableName, column.ColumnName),
                            SqlScript = cmdText
                        };
                        history2.Insert(this._mssqlHelper);
                        MessageBox.Show("列删除成功!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                        this.BindColumns(column.TableId);
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("列删除失败,请检查该列上是否有索引或其它对象引用...", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    }
                }
            }
        }

        private void DropTable()
        {
            if ((this.dgvTables.CurrentRow != null) && (this.dgvColumns.CurrentRow != null))
            {
                Table dataBoundItem = this.dgvTables.CurrentRow.DataBoundItem as Table;
                if ((dataBoundItem != null) && (MessageBox.Show("删除表会造成引用该表的存储过程,函数,视图等无法正确运行,且数据会丢失.\r\n确认继续?", "警告", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes))
                {
                    string cmdText = string.Format("DROP TABLE [{0}]", dataBoundItem.TableName);
                    this._mssqlHelper.ExecuteNonQuery(cmdText, CommandType.Text, new DbParameter[0]);
                    DatabaseSchemaHistory history2 = new DatabaseSchemaHistory
                    {
                        Description = string.Format("删除表:{0}", dataBoundItem.TableName),
                        SqlScript = cmdText
                    };
                    history2.Insert(this._mssqlHelper);
                    MessageBox.Show("删除成功!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    this.BindTables();
                    this.dgvColumns.Rows.Clear();
                }
            }
        }

        private void MakeScript()
        {
            FrmMakeScript script2 = new FrmMakeScript(this._currentConnection)
            {
                WindowState = FormWindowState.Normal,
                StartPosition = FormStartPosition.CenterParent
            };
            script2.ShowDialog(this);
        }

        private void dgvColumns_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (((e.RowIndex >= 0) && (e.ColumnIndex >= 0)) && this.dgvColumns.Columns[e.ColumnIndex].Name.Equals("btnModifyColumn"))
            {
                this.columnCurrRow.ScrollingIndex = this.dgvColumns.FirstDisplayedScrollingRowIndex;
                this.columnCurrRow.SelectIndex = e.RowIndex;
                Column dataBoundItem = this.dgvColumns.CurrentRow.DataBoundItem as Column;
                if (dataBoundItem != null)
                {
                    new FrmColumnEditor(dataBoundItem.TableId, dataBoundItem.ColumnName).ShowDialog(this);
                    this.BindColumns(dataBoundItem.TableId);
                }
            }
        }

        private void dgvTables_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (this.dgvTables.CurrentRow != null)
            {
                this.tableCurrRow.ScrollingIndex = this.dgvTables.FirstDisplayedScrollingRowIndex;
                this.tableCurrRow.SelectIndex = e.RowIndex;
                Table dataBoundItem = this.dgvTables.CurrentRow.DataBoundItem as Table;
                if (dataBoundItem != null)
                {
                    string str = "SELECT a.* FROM dbo.v_sys_DataDict a WHERE a.TableId = @TableId ORDER BY a.ColumnOrder";
                    IList<Column> list = this._mssqlHelper.ExecuteQueryAsList<Column>(str, CommandType.Text, new DbParameter[] { new SqlParameter("@TableId", dataBoundItem.TableId) });
                    this.dgvColumns.DataSource = list;
                }
            }
        }

        private void dgvTables_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (this.dgvTables.CurrentRow != null)
            {
                Table dataBoundItem = this.dgvTables.CurrentRow.DataBoundItem as Table;
                if (dataBoundItem != null)
                {
                    FrmTableEditor editor2 = new FrmTableEditor(dataBoundItem.TableId, this._currentConnection)
                    {
                        StartPosition = FormStartPosition.CenterParent
                    };
                    //dgvTables.SelectedRows
                    editor2.ShowDialog(this);
                    this.BindTables();
                }
            }
        }


        private void FrmDbSchema_Load(object sender, EventArgs e)
        {
            FrmMain mdiParent = base.MdiParent as FrmMain;
            if (mdiParent == null)
            {
                Application.Exit();
            }
            else
            {
                this._currentConnection = mdiParent.CurrentConnecttion;
                this._mssqlHelper = new MsSqlHelper(this._currentConnection.ToString());
                mdiParent.AddMenu(_menuItem);
                mdiParent.MdiChildActivate += (s, arg) =>
                {
                    if (mdiParent.ActiveMdiChild == this)
                    {
                        mdiParent.AddMenu(_menuItem);
                    }
                    else
                    {
                        mdiParent.RemoveMenu(_menuItem);
                    }
                };
                this.SetupColumns();
                this.BindTables();
            }
        }

        private void Export()
        {
            var tables = GetCurrentSchema();
            var sfg = new SaveFileDialog() { AddExtension = true, DefaultExt = "xls", FileName = _currentConnection.DataBase, Filter = "Excel Files(*.xls)|" };
            if (sfg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var flag = ExportExcelFile(sfg.FileName, tables);
                if (flag)
                {
                    MessageBox.Show(string.Format("文件: {0}导出成功!", sfg.FileName));
                }
            }
        }

        private bool ExportExcelFile(string destFile, IList<Table> tables)
        {
            var templateFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "templates", "schema.xls");
            var bindingRowIndex = 1;
            if (!File.Exists(templateFile)) { throw new ArgumentException(string.Format("上模板文件:{0}不存在!", templateFile)); }
            if (bindingRowIndex < 0) { throw new ArgumentException("绑定行索引不能小于0"); }
            HSSFWorkbook templateBook = null;
            var exportBook = new HSSFWorkbook();
            var databaseSheet = exportBook.CreateSheet(_currentConnection.DataBase);
            IDictionary<int, string> summaryHeaderColumns = new Dictionary<int, string>();                         //要导出的所有列名称
            var tableHeaderColumns = new Dictionary<int, string>();
            ISheet templateSummarySheet = null;
            ISheet templateTableSheet = null;
            var summaryCellStyles = new List<ICellStyle>();         //汇总内容单元格样式
            var tableCellStyles = new List<ICellStyle>();           //明细内容单元格样式
            var headerStyle = exportBook.CreateCellStyle();

            using (var stream = System.IO.File.OpenRead(templateFile))
            {
                var template = new NPOI.POIFS.FileSystem.POIFSFileSystem(stream);
                templateBook = new HSSFWorkbook(template);
                templateSummarySheet = templateBook.GetSheet("summary");
                if (templateSummarySheet == null) { throw new ArgumentException(string.Format("模板文件:{0}不存在指定的工作薄名称:{1}!", templateFile, "summary")); }
                var format = templateBook.CreateDataFormat() as HSSFDataFormat;
                var summaryBindingRow = templateSummarySheet.GetRow(bindingRowIndex);                                                //模板文件中的绑定行
                foreach (var cell in summaryBindingRow.Cells)
                {
                    if (string.IsNullOrEmpty(cell.StringCellValue)) { continue; }
                    summaryHeaderColumns.Add(cell.ColumnIndex, cell.StringCellValue.Replace("#", string.Empty));
                    var cellStyle = exportBook.CreateCellStyle();               //获取模板文件中表汇总工作薄的样式及设置列宽
                    cellStyle.CloneStyleFrom(cell.CellStyle);
                    summaryCellStyles.Add(cellStyle);
                    databaseSheet.SetColumnWidth(cell.ColumnIndex, templateSummarySheet.GetColumnWidth(cell.ColumnIndex));
                }

                templateTableSheet = templateBook.GetSheet("tables");
                if (templateTableSheet == null) { throw new ArgumentException(string.Format("模板文件:{0}不存在指定的工作薄名称:{1}!", templateFile, "tables")); }
                var tableBindingRow = templateTableSheet.GetRow(bindingRowIndex);                                                //模板文件中的绑定行
                foreach (var cell in tableBindingRow.Cells)
                {
                    if (string.IsNullOrEmpty(cell.StringCellValue)) { continue; }           //获取模板文件中table工作薄的样式
                    tableHeaderColumns.Add(cell.ColumnIndex, cell.StringCellValue.Replace("#", string.Empty));
                    var cellStyle = exportBook.CreateCellStyle();
                    cellStyle.CloneStyleFrom(cell.CellStyle);
                    tableCellStyles.Add(cellStyle);
                }
                tableCellStyles.Add(headerStyle);

                foreach (var item in tables)
                {
                    var tableSheet = exportBook.CreateSheet(item.ExportSheetName);                //每个表创建一个工作薄
                    foreach (var cell in tableBindingRow.Cells)
                    {
                        if (string.IsNullOrEmpty(cell.StringCellValue)) { continue; }       //设置列宽跟模板文件一致
                        tableSheet.SetColumnWidth(cell.ColumnIndex, templateTableSheet.GetColumnWidth(cell.ColumnIndex));
                    }
                }
            }

            //开始导数据
            var properties = typeof(Table).GetProperties();                                         //从模板文件写导出汇总表工作薄的列标题
            for (int headerRowIndex = 0; headerRowIndex < bindingRowIndex; headerRowIndex++)
            {
                var templateRow = templateSummarySheet.GetRow(headerRowIndex);
                IRow row = databaseSheet.CreateRow(headerRowIndex);
                row.Height = templateRow.Height;
                foreach (var columnIndex in summaryHeaderColumns.Keys)
                {
                    var templateHeadCell = templateSummarySheet.GetRow(bindingRowIndex < 1 ? bindingRowIndex : bindingRowIndex - 1).Cells[columnIndex];
                    headerStyle.CloneStyleFrom(templateHeadCell.CellStyle);
                    ICell cell = row.CreateCell(columnIndex);
                    cell.CellStyle = headerStyle;
                    var templateCell = templateRow.GetCell(columnIndex);
                    cell.SetCellValue(templateCell.StringCellValue);
                }
            }

            int startRowIndex = bindingRowIndex;
            foreach (var table in tables)
            {
                IRow tableRow = databaseSheet.CreateRow(startRowIndex);             //写表汇总工作薄中的行
                var cellIndex = 0;
                var sheet = WriteTableSchema(startRowIndex, bindingRowIndex, table, exportBook, templateTableSheet, tableHeaderColumns, tableCellStyles);
                foreach (var columnIndex in summaryHeaderColumns.Keys)
                {
                    var prop = properties.SingleOrDefault(p => p.Name.Equals(summaryHeaderColumns[columnIndex], StringComparison.CurrentCultureIgnoreCase));
                    if (prop == null)
                    {
                        continue;
                    }
                    object value = ExcelExporter.GetExportValue(prop.Name, table);          //导出的值
                    ICell cell = tableRow.CreateCell(columnIndex);
                    cell.CellStyle = summaryCellStyles[cellIndex];
                    SetCellValue(cell, prop, value);
                    if (cellIndex == 0)
                    {
                        var link = new HSSFHyperlink(HyperlinkType.Document);
                        link.Address = "#" + sheet.SheetName + "!A1";
                        tableRow.Cells[cellIndex].Hyperlink = link;
                        tableRow.Cells[cellIndex].CellStyle.GetFont(exportBook).Underline = FontUnderlineType.Single;
                        tableRow.Cells[cellIndex].CellStyle.GetFont(exportBook).Color = NPOI.HSSF.Util.HSSFColor.Blue.Index;
                    }
                    cellIndex++;
                }
                startRowIndex++;
            }
            using (var data = File.Create(destFile))
            {
                exportBook.Write(data);
                data.Close();
            }
            return true;
        }

        private void SetCellValue(ICell cell, PropertyInfo prop, object value)
        {
            if (value == null || value == DBNull.Value)
            {
                cell.SetCellValue(" ");
            }
            else
            {
                if (prop.PropertyType == typeof(DateTime) || prop.PropertyType == typeof(DateTime?))
                {
                    cell.SetCellValue(Convert.ToDateTime(value).ToString("yyyy/MM/dd"));
                }
                else if (prop.PropertyType == typeof(decimal) || prop.PropertyType == typeof(decimal?)
                     || prop.PropertyType == typeof(int) || prop.PropertyType == typeof(int?))
                {
                    cell.SetCellValue(Convert.ToDouble(value));
                }
                else
                {
                    cell.SetCellValue(Convert.ToString(value));
                }
            }
        }

        /// <summary>
        /// 写表结构明细
        /// </summary>
        /// <param name="bindingRowIndex"></param>
        /// <param name="table"></param>
        /// <param name="exportBook"></param>
        /// <param name="templateTableSheet"></param>
        /// <param name="tableHeaderColumns"></param>
        /// <param name="tableCellStyles"></param>
        /// <returns></returns>
        private ISheet WriteTableSchema(int tableRowIndex, int bindingRowIndex, Table table, HSSFWorkbook exportBook, ISheet templateTableSheet, IDictionary<int, string> tableHeaderColumns, IList<ICellStyle> tableCellStyles)
        {
            var itemRowIndex = bindingRowIndex;
            var tableSheet = exportBook.GetSheet(table.ExportSheetName);
            foreach (var item in table.Columns)                                                 //写表明细
            {
                var columnProperties = typeof(Column).GetProperties();
                for (int headerRowIndex = 0; headerRowIndex < bindingRowIndex; headerRowIndex++)        //写标题
                {
                    var templateRow = templateTableSheet.GetRow(headerRowIndex);
                    IRow row = tableSheet.CreateRow(headerRowIndex);
                    row.Height = templateRow.Height;
                    foreach (var columnIndex in tableHeaderColumns.Keys)
                    {
                        ICell cell = row.CreateCell(columnIndex);
                        cell.CellStyle = tableCellStyles.Last();
                        var templateCell = templateRow.GetCell(columnIndex);
                        cell.SetCellValue(templateCell.StringCellValue);
                    }
                }

                IRow columnRow = tableSheet.CreateRow(itemRowIndex);
                var itemCellIndex = 0;
                foreach (var columnIndex in tableHeaderColumns.Keys)
                {
                    var prop = columnProperties.SingleOrDefault(p => p.Name.Equals(tableHeaderColumns[columnIndex], StringComparison.CurrentCultureIgnoreCase));
                    if (prop == null)
                    {
                        if (tableHeaderColumns[columnIndex].ToLower().Equals("TableName".ToLower()))
                        {
                            ICell tCell = columnRow.CreateCell(columnIndex);
                            tCell.CellStyle = tableCellStyles[itemCellIndex];
                            tCell.SetCellValue(table.TableName);
                        }
                        continue;
                    }
                    object value = ExcelExporter.GetExportValue(prop.Name, item);          //导出的值
                    ICell cell = columnRow.CreateCell(columnIndex);
                    cell.CellStyle = tableCellStyles[itemCellIndex];
                    SetCellValue(cell, prop, value);
                    itemCellIndex++;
                }
                itemRowIndex++;
            }
            var backspaceRow = tableSheet.CreateRow(itemRowIndex);
            var backspaceCell = backspaceRow.CreateCell(0);
            backspaceCell.SetCellValue("返回");
            var link = new HSSFHyperlink(HyperlinkType.Document);
            link.Address = string.Format("#{0}!A{1}", _currentConnection.DataBase, tableRowIndex + bindingRowIndex);// "#" + _currentConnection.DataBase + "!A" + tableRowIndex.ToString();
            backspaceCell.Hyperlink = link;
            backspaceCell.CellStyle.GetFont(exportBook).Underline = FontUnderlineType.Single;
            backspaceCell.CellStyle.GetFont(exportBook).Color = NPOI.HSSF.Util.HSSFColor.Blue.Index;
            return tableSheet;
        }


        private void SetupColumns()
        {
            this.dgvTables.Columns.Clear();
            this.dgvTables.ReadOnly = true;
            this.dgvTables.AutoGenerateColumns = false;
            this.dgvTables.MultiSelect = false;
            this.dgvTables.AllowUserToAddRows = false;
            this.dgvTables.AllowUserToDeleteRows = false;
            this.dgvTables.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            DataGridViewColumn[] dataGridViewColumns = new DataGridViewColumn[4];
            DataGridViewTextBoxColumn column = new DataGridViewTextBoxColumn
            {
                Name = "TableId",
                DataPropertyName = "TableId",
                Visible = false
            };
            dataGridViewColumns[0] = column;
            DataGridViewTextBoxColumn column2 = new DataGridViewTextBoxColumn
            {
                Name = "TableName",
                DataPropertyName = "TableName",
                Width = 150,
                HeaderText = "表名"
            };
            dataGridViewColumns[1] = column2;
            DataGridViewTextBoxColumn column3 = new DataGridViewTextBoxColumn
            {
                Name = "TableDescription",
                DataPropertyName = "TableDescription",
                Width = 180,
                HeaderText = "描述"
            };
            dataGridViewColumns[2] = column3;
            DataGridViewTextBoxColumn column4 = new DataGridViewTextBoxColumn
            {
                Name = "IsView",
                DataPropertyName = "IsView",
                Width = 80,
                HeaderText = "是否视图"
            };
            dataGridViewColumns[3] = column4;
            this.dgvTables.Columns.AddRange(dataGridViewColumns);
            IList<string> sqlTypes = Column.GetSqlTypes();
            sqlTypes.Insert(0, "--请选择--");
            this.dgvColumns.Columns.Clear();
            this.dgvColumns.ReadOnly = true;
            this.dgvColumns.AutoGenerateColumns = false;
            this.dgvColumns.MultiSelect = false;
            this.dgvColumns.AllowUserToAddRows = false;
            this.dgvColumns.AllowUserToDeleteRows = false;
            this.dgvColumns.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewColumns = new DataGridViewColumn[12];
            DataGridViewTextBoxColumn column5 = new DataGridViewTextBoxColumn
            {
                Name = "TableId",
                DataPropertyName = "TableId",
                Visible = false
            };
            dataGridViewColumns[0] = column5;
            DataGridViewTextBoxColumn column6 = new DataGridViewTextBoxColumn
            {
                Name = "ColumnOrder",
                DataPropertyName = "ColumnOrder",
                Visible = false
            };
            dataGridViewColumns[1] = column6;
            DataGridViewTextBoxColumn column7 = new DataGridViewTextBoxColumn
            {
                Name = "ColumnName",
                DataPropertyName = "ColumnName",
                Width = 100,
                HeaderText = "列名"
            };
            dataGridViewColumns[2] = column7;
            DataGridViewCheckBoxColumn column8 = new DataGridViewCheckBoxColumn
            {
                Name = "IsPrimaryKey",
                DataPropertyName = "IsPrimaryKey",
                Width = 60,
                HeaderText = "主键",
                TrueValue = true,
                FalseValue = false
            };
            dataGridViewColumns[3] = column8;
            DataGridViewCheckBoxColumn column9 = new DataGridViewCheckBoxColumn
            {
                Name = "IsNullable",
                DataPropertyName = "IsNullable",
                Width = 60,
                HeaderText = "可空",
                TrueValue = true,
                FalseValue = false
            };
            dataGridViewColumns[4] = column9;
            DataGridViewCheckBoxColumn column10 = new DataGridViewCheckBoxColumn
            {
                Name = "IsIdentity",
                DataPropertyName = "IsIdentity",
                Width = 60,
                HeaderText = "标识",
                TrueValue = true,
                FalseValue = false
            };
            dataGridViewColumns[5] = column10;
            DataGridViewComboBoxColumn column11 = new DataGridViewComboBoxColumn
            {
                Name = "SqlType",
                DataPropertyName = "SqlType",
                Width = 80,
                HeaderText = "类型",
                DropDownWidth = 100,
                MaxDropDownItems = 5,
                DataSource = sqlTypes
            };
            dataGridViewColumns[6] = column11;
            DataGridViewTextBoxColumn column12 = new DataGridViewTextBoxColumn
            {
                Name = "Precision",
                DataPropertyName = "Precision",
                Width = 60,
                HeaderText = "长度"
            };
            dataGridViewColumns[7] = column12;
            DataGridViewTextBoxColumn column13 = new DataGridViewTextBoxColumn
            {
                Name = "Scale",
                DataPropertyName = "Scale",
                Width = 60,
                HeaderText = "精度"
            };
            dataGridViewColumns[8] = column13;
            DataGridViewTextBoxColumn column14 = new DataGridViewTextBoxColumn
            {
                Name = "DefaultValue",
                DataPropertyName = "DefaultValue",
                Width = 100,
                HeaderText = "默认值"
            };
            dataGridViewColumns[9] = column14;
            DataGridViewTextBoxColumn column15 = new DataGridViewTextBoxColumn
            {
                Name = "ColumnDescription",
                DataPropertyName = "ColumnDescription",
                Width = 160,
                HeaderText = "描述"
            };
            dataGridViewColumns[10] = column15;
            DataGridViewButtonColumn column16 = new DataGridViewButtonColumn
            {
                Name = "btnModifyColumn",
                HeaderText = "编辑",
                UseColumnTextForButtonValue = true,
                Width = 80,
                Text = "修改"
            };
            dataGridViewColumns[11] = column16;
            this.dgvColumns.Columns.AddRange(dataGridViewColumns);
        }

        private IList<Table> GetCurrentSchema()
        {
            var str = "SELECT a.* FROM dbo.v_sys_DataDict a  ORDER BY a.ColumnOrder";
            var data = _mssqlHelper.ExecuteQueryAsDataTable(str, CommandType.Text);
            var dtTable = data.DefaultView.ToTable(true, new string[] { "TableName", "TableId", "IsView", "TableDescription" });
            var tables = dtTable.ToList<Table>();
            foreach (var item in tables)
            {
                var columns = data.Select(string.Format("TableId={0}", item.TableId)).CopyToDataTable();
                item.Columns.AddRange(columns.ToList<Column>());
            }
            return tables;
        }

        private void Import()
        {
            var sfg = new OpenFileDialog() { DefaultExt = "xls", FileName = _currentConnection.DataBase, Filter = "Excel Files(*.xls)|", CheckFileExists = true, Multiselect = false };
            if (sfg.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                return;
            }
            var captionRowIndex = 1;
            var lst = new List<Table>();
            var schema = GetCurrentSchema();
            using (var fs = System.IO.File.OpenRead(sfg.FileName))
            {
                var stream = new NPOI.POIFS.FileSystem.POIFSFileSystem(fs);
                var book = new HSSFWorkbook(stream);
                var tableBook = book.GetSheet(_currentConnection.DataBase);
                if (tableBook == null) { return; }
                var headerRow = tableBook.GetRow(captionRowIndex);
                var enumator = tableBook.GetRowEnumerator();
                for (int i = captionRowIndex; i < tableBook.LastRowNum; i++)
                {
                    var row = tableBook.GetRow(i);
                    var table = ReadTable(book, row, schema);
                    if (table == null) { continue; }
                    lst.Add(table);
                }
            }
            var sb = new StringBuilder();
            lst.Where(t => !string.IsNullOrWhiteSpace(t.TableDescription))
                .ToList()
                .ForEach(t => sb.AppendLine(t.MakeExtendPropertiesSql()));
            var flag = _mssqlHelper.ExecuteNonQuery(sb.ToString(), CommandType.Text);

            MessageBox.Show("导入成功!", "消息", MessageBoxButtons.OK);
            BindTables();
        }

        private Table ReadTable(HSSFWorkbook book, IRow row, IList<Table> schema)
        {
            var table = new Table();

            var isView = bool.Parse(row.Cells[2].StringCellValue);          //导出后不能改单元格的值
            if (isView) { return null; }
            table.TableName = row.Cells[0].StringCellValue;
            table.TableDescription = row.Cells[1].StringCellValue;
            var original = schema.Where(t => t.TableName.Equals(table.TableName, StringComparison.CurrentCultureIgnoreCase)).SingleOrDefault();
            if (original == null) { return null; }
            var sheet = book.GetSheet(table.TableName);
            if (sheet == null) { return table; }
            ReadColumns(table, sheet, original);
            return table;
        }

        private void ReadColumns(Table table, ISheet sheet, Table original)
        {
            var captionRowIndex = 1;
            for (int i = captionRowIndex; i < sheet.LastRowNum; i++)
            {
                var row = sheet.GetRow(i);
                var c = new Column();
                c.ColumnName = row.Cells[1].StringCellValue;
                c.ColumnDescription = row.Cells[2].StringCellValue;
                if (original.Columns.Any(item => item.ColumnName.Equals(c.ColumnName, StringComparison.CurrentCultureIgnoreCase)))
                {
                    table.Columns.Add(c);
                }
            }
        }
    }
}

