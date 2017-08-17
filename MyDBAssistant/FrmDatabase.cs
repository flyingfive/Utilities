using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.IO;

namespace MyDBAssistant
{
    public partial class FrmDatabase : Form
    {
        public string DataBase { get; private set; }

        public string Server { get; private set; }
        public string DbUser { get; private set; }
        public string DbPassword { get; private set; }
        //public string ConnectionString { get; private set; }
        public FrmDatabase()
        {
            InitializeComponent();
            this.KeyPreview = true;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.KeyDown += (s, e) => { if (e.KeyCode == Keys.Escape) { btnNO_Click(btnNO, EventArgs.Empty); } };
#if DEBUG
            txtServer.Text = @"127.0.0.1";
            //txtDB.Text = "Store";
            txtPwd.Text = "sa";
#endif
            var ConfigFile = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase
                    , string.Concat(Path.GetFileNameWithoutExtension(AppDomain.CurrentDomain.SetupInformation.ApplicationName)
                    , ".xml"));
            if (File.Exists(ConfigFile))
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(ConfigFile);    //加载Xml文件  
                XmlElement rootElem = doc.DocumentElement;   //获取根节点  
                XmlNodeList personNodes = rootElem.ChildNodes; //获取person子节点集合  
                txtServer.Text = personNodes[0].InnerText;
                txtDB.Text = personNodes[1].InnerText;
                txtUser.Text = personNodes[2].InnerText;
                txtPwd.Text = personNodes[3].InnerText;
            }
        }

        private void btnNO_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.No;
            this.Close();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtServer.Text.Trim())) { return; }
            if (string.IsNullOrEmpty(txtDB.Text.Trim())) { return; }
            if (string.IsNullOrEmpty(txtUser.Text.Trim())) { return; }
            //if (string.IsNullOrEmpty(txtPwd.Text.Trim())) { return; }
            //FrmModelCreator frmMain = new FrmModelCreator(connectionString, txtDB.Text.Trim()) { WindowState = FormWindowState.Maximized, StartPosition = FormStartPosition.CenterScreen };
            //frmMain.Show();
            //this.Hide();
            //ConnectionString = connectionString;
            DataBase = this.txtDB.Text.Trim();
            Server = txtServer.Text.Trim();
            DbUser = txtUser.Text.Trim();
            DbPassword = txtPwd.Text.Trim();
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }
    }
}
