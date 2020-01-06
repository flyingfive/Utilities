using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MyDBAssistant.Data;
using MyDBAssistant.Schema;

namespace MyDBAssistant
{
    public partial class FrmModelCreator : FrmBase
    {
        private MsSqlHelper _mssqlHelper = null;
        //public string ConnectionString { get; private set; }
        private ConnectionString _currentConnection = null;
        public string Database { get; set; }

        public FrmModelCreator()
        {
            InitializeComponent();
            this.Load += new EventHandler(FrmModelCreator_Load);
            var dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "templates");
            var files = Directory.GetFiles(dir, "*.vt").Select(f => new { Name = Path.GetFileNameWithoutExtension(f), Value = Path.GetFileName(f) }).ToList();
            cboType.DataSource = files;
            cboType.DisplayMember = "Name";
            cboType.ValueMember = "Value";
        }

        void FrmModelCreator_Load(object sender, EventArgs e)
        {
            var mdiParent = this.MdiParent as FrmMain;
            if (mdiParent == null || mdiParent.CurrentConnecttion == null) { throw new ArgumentException("找不到MdiParent或当前数据库连接"); }
            _currentConnection = mdiParent.CurrentConnecttion;
            Database = _currentConnection.DataBase;
            //Database = database;
            //ConnectionString = connectionString;
            //this.FormClosed += (s, e) => { Application.Exit(); };
            _mssqlHelper = new MsSqlHelper(_currentConnection.ToString());
            tabCodes.TabPages.Clear();
            BindTables();
            tvTables.ExpandAll();
            cboType.SelectedIndex = 0;
        }

        private void BindTables()
        {
            tvTables.CheckBoxes = true;
            TreeNode rootNode = new TreeNode(Database) { Name = Database };
            var lst = _mssqlHelper.ExecuteQueryAsList<Table>(@"SELECT DISTINCT TableId ,TableName, IsView, TableDescription FROM dbo.v_sys_DataDict", CommandType.Text);
            if (lst == null || lst.Count == 0) { return; }
            var tables = lst.Where(t => !t.IsView).OrderBy(t => t.TableName).ToList();
            var views = lst.Where(t => t.IsView).OrderBy(t => t.TableName).ToList();
            var tableNode = new TreeNode("Tables");
            var viewNode = new TreeNode("Views");
            tables.ForEach(t => { var node = new TreeNode(t.TableName) { Tag = t }; tableNode.Nodes.Add(node); });
            views.ForEach(t => { var node = new TreeNode(t.TableName) { Tag = t }; viewNode.Nodes.Add(node); });
            rootNode.Nodes.Add(tableNode);
            rootNode.Nodes.Add(viewNode);
            tvTables.Nodes.Add(rootNode);
            tvTables.AfterCheck += tvTables_AfterCheck;
        }

        void tvTables_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Text.Equals("Tables", StringComparison.CurrentCultureIgnoreCase) || e.Node.Text.Equals("Views", StringComparison.CurrentCultureIgnoreCase))
            {
                foreach (TreeNode node in e.Node.Nodes)
                {
                    node.Checked = e.Node.Checked;
                }
            }
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            IList<Table> selectedTables = new List<Table>();
            selectedTables = GetSelectedTables(tvTables.Nodes[Database], selectedTables);
            if (selectedTables.Count == 0) { return; }
            tabCodes.TabPages.Clear();

            string where = "";
            IList<DbParameter> paras = new List<DbParameter>();
            int i = 1;
            foreach (var table in selectedTables)
            {
                where = string.Concat(where, "@p", i.ToString(), ",");
                paras.Add(new SqlParameter(string.Concat("@p", i.ToString()), table.TableId));
                i++;
            }
            if (where.EndsWith(",")) { where = where.Substring(0, where.Length - 1); }
            string sql = string.Format("SELECT * FROM dbo.v_sys_DataDict WHERE TableId IN ({0})", where);
            var columns = _mssqlHelper.ExecuteQueryAsList<Column>(sql, CommandType.Text, paras.ToArray());
            string prefix = txtPrefix.Text.Trim();
            string ns = txtNamespace.Text.Trim();
            if (string.IsNullOrWhiteSpace(ns)) { ns = tvTables.Nodes[0].Text; ns = string.Concat("Xen.", ns.Substring(0, 1).ToUpper(), ns.Substring(1), ".Entities"); }
            string className = "";
            string templatDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "templates");
            VelocityHelper templateHelper = new VelocityHelper(templatDir);
            var templateName = cboType.SelectedValue.ToString();
            if (string.IsNullOrEmpty(templateName)) { MessageBox.Show(string.Format("没有类型:{0}对应的模板!", cboType.Text), "错误", MessageBoxButtons.OK, MessageBoxIcon.Stop); return; }
            foreach (var table in selectedTables)
            {
                className = table.GetClassName(prefix);
                var ownerColumns = columns.Where(c => c.TableId == table.TableId).ToList();
                var primaryKeys = ownerColumns.Where(c => c.IsPrimaryKey).ToList();
                if (templateName.Equals("XenEntity.vt") || templateName.Equals("XenSelfTrackingEntity.vt")) { ownerColumns = ownerColumns.Where(c => !c.IsPrimaryKey).ToList(); }
                string primaryType = primaryKeys.Count == 1 ? Column.GetCSharpType(primaryKeys[0].SqlType, "", primaryKeys[0].IsNullable) : string.Concat(className, "Id");
                if (primaryKeys.Count > 1)
                {
                    //new { entity.Id, entity.BranchNo }
                    string efKey = "";
                    foreach (var key in primaryKeys)
                    {
                        efKey = string.Concat(efKey, " entity.", key.ColumnName, ",");
                    }
                    if (efKey.EndsWith(",")) { efKey = efKey.Substring(0, efKey.Length - 1); }
                    efKey = string.Concat("entity => new {", efKey, " }");
                    templateHelper.PutSet("efKey", efKey);
                }
                templateHelper.PutSet("ns", ns);
                templateHelper.PutSet("tablePrefix", prefix);

                templateHelper.PutSet("table", table);
                templateHelper.PutSet("className", className);
                templateHelper.PutSet("primaryKeyClass", primaryType);
                templateHelper.PutSet("hasCustomPrimaryKey", primaryKeys.Count > 1);
                templateHelper.PutSet("columns", ownerColumns);
                templateHelper.PutSet("primaryColumns", primaryKeys);
                string classDefination = templateHelper.Display(templateName);
                TabPage tp = new TabPage(className) { Name = "tab_" + className };
                TextBox txt = new TextBox() { Name = "txt" + className, ReadOnly = true, Text = classDefination, Multiline = true, Dock = DockStyle.Fill, ScrollBars = ScrollBars.Both };
                tp.Controls.Add(txt);
                tabCodes.TabPages.Add(tp);
            }
        }



        private IList<Table> GetSelectedTables(TreeNode node, IList<Table> lst)
        {
            if (node.Checked)
            {
                var table = node.Tag as Table;
                if (table != null) { lst.Add(table); }
            }
            if (node.Nodes != null && node.Nodes.Count > 0)
            {
                foreach (TreeNode child in node.Nodes)
                {
                    lst = GetSelectedTables(child, lst);
                }
            }
            return lst;
        }



        private void btnSave_Click(object sender, EventArgs e)
        {
            if (tabCodes.TabPages.Count == 0) { MessageBox.Show("请先生成代码!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            string filePath = @"C:\";
            using (FolderBrowserDialog dialog = new FolderBrowserDialog() { ShowNewFolderButton = true, Description = "请选择文件保存的目标文件夹", RootFolder = Environment.SpecialFolder.MyComputer })
            {
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    filePath = dialog.SelectedPath;
                    foreach (TabPage tp in tabCodes.TabPages)
                    {
                        if (tp.Controls.Count > 0 && (tp.Controls[0] is TextBox))
                        {
                            var txt = tp.Controls[0] as TextBox;
                            if (txt == null) { continue; }
                            string content = txt.Text;
                            string fileName = txt.Name.Substring(3);
                            fileName = System.IO.Path.Combine(filePath, fileName + ".cs");
                            System.IO.File.WriteAllText(fileName, txt.Text, Encoding.UTF8);
                        }
                    }
                    MessageBox.Show("所有文件保存成功!", "消息", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }
    }
}
