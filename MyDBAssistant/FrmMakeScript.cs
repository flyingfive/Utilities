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

namespace MyDBAssistant
{
    public partial class FrmMakeScript : Form
    {
        private MsSqlHelper _mssqlHelper = null;

        public FrmMakeScript(ConnectionString currentConnection)
        {
            InitializeComponent();
            btnClose.Click += (s, e) => { this.Close(); };
            _mssqlHelper = new MsSqlHelper(currentConnection.ToString());
            btnSave.Click += (s, e) => {
                if (string.IsNullOrWhiteSpace(txtScript.Text)) { return; }
                string sql = txtScript.Text;
                _mssqlHelper.ExecuteNonQuery(sql, CommandType.Text);
                DatabaseSchemaHistory log = new DatabaseSchemaHistory() { Description = string.Format("创建自定义脚本内容."), SqlScript = sql };
                log.Insert(_mssqlHelper);
                MessageBox.Show("创建成功!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);            
            };
        }
    }
}
