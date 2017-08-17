using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MyDBAssistant.Data;
using MyDBAssistant.Schema;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace MyDBAssistant
{
    public partial class FrmMain : FrmBase
    {
        private MsSqlHelper _mssqlHelper = null;
        public ConnectionString CurrentConnecttion { get; private set; }
        public FrmMain()
        {
            InitializeComponent();
            SwitchDB();
        }

        private void MenuItem_Click(object sender, EventArgs e)
        {
            var menu = sender as ToolStripMenuItem;
            if (menu == null) { return; }
            string name = menu.Name.Replace("tsmi", "").ToUpper();
            switch (name)
            {
                case "NEWDB": CreateDatabase(); break;                              //创建数据库
                case "GENERATEENTITY": OpenFunction<FrmModelCreator>(); break;                     //实体模型工具
                case "TABLEDESIGN": OpenFunction<FrmDbSchema>(); break;                           //数据库维护(表设计)
                case "SWITCHDB": SwitchDB(); break;                                 //切换/登陆数据库
                case "DBOBJECT": OpenFunction<FrmDBObject>(); break;                       //数据库对象管理
                case "ABOUT": About(); break;
                //case "MAKECERT": var frm = new FrmMakeCert(); frm.MdiParent = this; frm.Show(); break;
                case "EXIT": this.Close(); Application.Exit(); break;               //退出
                default: break;
            }
        }

        private void About()
        {
            var frmAbout = new FrmAbout() { StartPosition = FormStartPosition.CenterScreen };
            frmAbout.ShowDialog(this);
        }

        void item_Click(object sender, EventArgs e)
        {
            var item = sender as ToolStripMenuItem;
            if (item == null) { return; }
            var name = item.Name.Substring(4);
            var frm = Application.OpenForms.OfType<Form>().SingleOrDefault(f => f.GetType().FullName.Equals(name, StringComparison.CurrentCultureIgnoreCase));
            if (frm == null) { return; }
            frm.BringToFront();
        }

        private void SwitchDB()
        {
            FrmDatabase frmDb = new FrmDatabase() { ShowInTaskbar = false };
            if (frmDb.ShowDialog() != System.Windows.Forms.DialogResult.OK) { return; }
            CurrentConnecttion = new ConnectionString() { DataBase = frmDb.DataBase, Server = frmDb.Server, LoginUser = frmDb.DbUser, LoginPwd = frmDb.DbPassword };
            if (!CurrentConnecttion.IsValid()) { MessageBox.Show("输入的数据库连接信息不合法!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            if (!CurrentConnecttion.HasDataBaseExists()) { MessageBox.Show(string.Format("请求数据库: '{0}'不存在!", CurrentConnecttion.DataBase), "错误", MessageBoxButtons.OK, MessageBoxIcon.Stop); return; }
            _mssqlHelper = new MsSqlHelper(CurrentConnecttion.ToString());
            //int id = _mssqlHelper.ExecuteQueryAsSingle<int>("SELECT OBJECT_ID('v_sys_DataDict')", CommandType.Text);
            //if (id <= 0) { CreateSchemaView(); }
            CreateSchemaViewIfNotExists();
            MessageBox.Show(string.Format("切换成功,当前登陆数据库为:{0}", CurrentConnecttion.DataBase), "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            Application.OpenForms.OfType<Form>().Where(f => f.GetType() != typeof(FrmMain)).ToList().ForEach(f => f.Close());
            #region 保存到配置文件中
            var ConfigFile = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase
                    , string.Concat(Path.GetFileNameWithoutExtension(AppDomain.CurrentDomain.SetupInformation.ApplicationName)
                    , ".xml"));
            if (File.Exists(ConfigFile))
            {
                SaveSetting("ServerAddress", frmDb.Server);
                SaveSetting("DataBase", frmDb.DataBase);
                SaveSetting("DbUser", frmDb.DbUser);
                SaveSetting("Dbpass", frmDb.DbPassword);
            }
            else
            {
                var xmlDoc = new XmlDocument();
                var root = xmlDoc.CreateElement("Configuration");
                var addressNode = xmlDoc.CreateElement("ServerAddress");
                var dataNode = xmlDoc.CreateElement("DataBase");
                var dbuserNode = xmlDoc.CreateElement("DbUser");
                var dbPassNode = xmlDoc.CreateElement("Dbpass");
                addressNode.InnerText = frmDb.Server;
                dataNode.InnerText = frmDb.DataBase;
                dbuserNode.InnerText = frmDb.DbUser;
                dbPassNode.InnerText = frmDb.DbPassword;
                root.AppendChild(addressNode);
                root.AppendChild(dataNode);
                root.AppendChild(dbuserNode);
                root.AppendChild(dbPassNode);
                xmlDoc.AppendChild(root);
                xmlDoc.Save(ConfigFile);
            }
            #endregion

            tsslServer.Text = string.Concat("Server: ", CurrentConnecttion.Server);
            tsslDB.Text = string.Concat("DataBase: ", CurrentConnecttion.DataBase);
            tsslDBUser.Text = string.Concat("Login User: ", CurrentConnecttion.LoginUser);
            tsslDBVer.Text = string.Concat("DB Version: ", CurrentConnecttion.Version.ToString());
        }


        private bool ExistsConfig(string configName, XDocument xDoc)
        {
            return xDoc.Root.Element(configName) != null;
        }

        /// <summary>
        /// 更新本地配置
        /// </summary>
        /// <param name="configName">配置节点名称</param>
        /// <param name="value">配置值</param>
        public void SaveSetting(string configName, string value)
        {
            var ConfigFile = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase
                    , string.Concat(Path.GetFileNameWithoutExtension(AppDomain.CurrentDomain.SetupInformation.ApplicationName)
                    , ".xml"));
            var xDoc = XDocument.Load(ConfigFile);
            if (ExistsConfig(configName, xDoc))
            {
                xDoc.Root.Element(configName).SetValue(value);
            }
            else
            {
                var element = new XElement(configName) { Value = value };
                xDoc.Root.Add(element);
            }
            xDoc.Save(ConfigFile);
        }

        private void OpenFunction<T>() where T : Form, new()
        {
            var frm = Application.OpenForms.OfType<Form>().SingleOrDefault(f => f.GetType() == typeof(T));
            if (frm != null) { frm.BringToFront(); return; }
            if (CurrentConnecttion == null) { MessageBox.Show("请先连接数据库!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            if (!CurrentConnecttion.IsValid()) { MessageBox.Show("输入的数据库连接信息不合法!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            if (!CurrentConnecttion.HasDataBaseExists()) { MessageBox.Show(string.Format("数据库: '{0}'不存在!", CurrentConnecttion.DataBase), "错误", MessageBoxButtons.OK, MessageBoxIcon.Stop); return; }
            T frmView = new T() { WindowState = FormWindowState.Maximized, StartPosition = FormStartPosition.CenterParent, MdiParent = this };
            AddMenuItem(frmView);
            frmView.Show();
        }

        private void CreateDatabase()
        {
            var frm = Application.OpenForms.OfType<Form>().SingleOrDefault(f => f.GetType() == typeof(FrmDatabase));
            if (frm != null) { frm.BringToFront(); return; }
            FrmDatabase frmDb = new FrmDatabase();
            frmDb.Text = string.Concat(frmDb.Text, "创建");
            if (frmDb.ShowDialog() != System.Windows.Forms.DialogResult.OK) { return; }
            CurrentConnecttion = new ConnectionString() { DataBase = frmDb.DataBase, Server = frmDb.Server, LoginUser = frmDb.DbUser, LoginPwd = frmDb.DbPassword };
            if (!CurrentConnecttion.IsValid()) { MessageBox.Show("输入的数据库连接信息不合法!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            if (CurrentConnecttion.HasDataBaseExists()) { MessageBox.Show(string.Format("数据库:{0}已存在!", CurrentConnecttion.DataBase), "错误", MessageBoxButtons.OK, MessageBoxIcon.Stop); return; }
            string mainDb = CurrentConnecttion.ToString().Replace(CurrentConnecttion.DataBase, "master");
            _mssqlHelper = new MsSqlHelper(mainDb);
            _mssqlHelper.CreateDatabase(CurrentConnecttion.DataBase);
            _mssqlHelper.ChangeDatabase(CurrentConnecttion.DataBase);
            InitDatabase();
            MessageBox.Show(string.Format("数据库: {0}创建成功!", CurrentConnecttion.DataBase), "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            tsslServer.Text = string.Concat("Server: ", CurrentConnecttion.Server);
            tsslDB.Text = string.Concat("DataBase: ", CurrentConnecttion.DataBase);
            tsslDBUser.Text = string.Concat("Login User: ", CurrentConnecttion.LoginUser);
            tsslDBVer.Text = string.Concat("DB Version: ", CurrentConnecttion.Version.ToString());
        }

        private void InitDatabase()
        {
            _mssqlHelper.ExecuteNonQuery(DatabaseSchemaHistory.CREATE_SCHEMA_TABLE_SQL, CommandType.Text);
            AppendExtendProperties();
            CreateSchemaViewIfNotExists();
            var log = new DatabaseSchemaHistory() { Description = "创建数据库", SqlScript = "" };
            log.Insert(_mssqlHelper);
        }

        public void AddMenu(ToolStripMenuItem menu)
        {
            //if (index < 0) { return; }
            if (msMain.Items.OfType<ToolStripItem>().Any(i => i.Name.Equals(menu.Name))) { return; }
            var index = msMain.Items.IndexOfKey("tsmiUtility") + 1;
            msMain.Items.Insert(index, menu);
        }

        public void RemoveMenu(ToolStripMenuItem menu)
        {
            if (msMain.Items.OfType<ToolStripItem>().Any(i => i.Name.Equals(menu.Name)))
            {
                msMain.Items.Remove(menu);
            }
        }

        private void AddMenuItem(Form frm)
        {
            if (frm == null) { return; }
            var name = frm.GetType().FullName;
            if (tsmiWindow.DropDownItems.OfType<ToolStripItem>().Any(menu => menu.Name.Contains(name))) { return; }
            var item = new ToolStripMenuItem() { Text = frm.Text, Name = string.Concat("tsmi", name) };
            item.Click += new EventHandler(item_Click);
            frm.FormClosed += (s, e) =>
            {
                if (tsmiWindow.DropDownItems.Contains(item))
                {
                    tsmiWindow.DropDownItems.Remove(item);
                }
            };
            tsmiWindow.DropDownItems.Add(item);
        }

        private void AppendExtendProperties()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(string.Format("EXEC sp_addextendedproperty 'MS_Description', '{0}', 'user', dbo, 'table', [{1}]", "数据库架构历史记录", "sys_DatabaseSchemaHistory"));
            sb.AppendLine(string.Format("EXEC sp_addextendedproperty 'MS_Description', '{0}', 'user', dbo, 'table', [{1}], 'column', [{2}]", "主键ID", "sys_DatabaseSchemaHistory", "Id"));
            sb.AppendLine(string.Format("EXEC sp_addextendedproperty 'MS_Description', '{0}', 'user', dbo, 'table', [{1}], 'column', [{2}]", "修改描述", "sys_DatabaseSchemaHistory", "Description"));
            sb.AppendLine(string.Format("EXEC sp_addextendedproperty 'MS_Description', '{0}', 'user', dbo, 'table', [{1}], 'column', [{2}]", "开发者", "sys_DatabaseSchemaHistory", "DeveloperId"));
            sb.AppendLine(string.Format("EXEC sp_addextendedproperty 'MS_Description', '{0}', 'user', dbo, 'table', [{1}], 'column', [{2}]", "记录日期", "sys_DatabaseSchemaHistory", "RecordDate"));
            sb.AppendLine(string.Format("EXEC sp_addextendedproperty 'MS_Description', '{0}', 'user', dbo, 'table', [{1}], 'column', [{2}]", "客户端IP", "sys_DatabaseSchemaHistory", "ClientIp"));
            sb.AppendLine(string.Format("EXEC sp_addextendedproperty 'MS_Description', '{0}', 'user', dbo, 'table', [{1}], 'column', [{2}]", "修改脚本内容", "sys_DatabaseSchemaHistory", "SqlScript"));
            sb.AppendLine(string.Format("EXEC sp_addextendedproperty 'MS_Description', '{0}', 'user', dbo, 'table', [{1}], 'column', [{2}]", "备注", "sys_DatabaseSchemaHistory", "Remark"));
            _mssqlHelper.ExecuteNonQuery(sb.ToString(), CommandType.Text);
        }

        private void CreateSchemaViewIfNotExists()
        {
            int dbVer = Convert.ToInt32(CurrentConnecttion.Version);
            string createDictViewSql = MsSqlHelper.CREATE_DATA_DICT_VIEW_SQL2000;
            if (dbVer > 2000)
            {
                createDictViewSql = MsSqlHelper.CREATE_DATA_DICT_VIEW_SQL2005;
            }
            var id = Convert.ToInt64(_mssqlHelper.ExecuteQueryAsSingle(@"SELECT ISNULL(OBJECT_ID('v_sys_DataDict'), 0)", CommandType.Text));
            if (id <= 0)
            {
                _mssqlHelper.ExecuteNonQuery(createDictViewSql, CommandType.Text);
            }
        }
    }
}
