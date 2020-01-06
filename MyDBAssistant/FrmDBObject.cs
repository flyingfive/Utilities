using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MyDBAssistant.Schema;
using MyDBAssistant.Data;

namespace MyDBAssistant
{
    public partial class FrmDBObject : FrmBase
    {
        private MsSqlHelper _mssqlHelper = null;
        private ConnectionString _currentConnection = null;
        public IList<DatabaseObject> Objects { get; private set; }

        public FrmDBObject()
        {
            InitializeComponent();
            InitControls();
            this.Load += new EventHandler(FrmDBObject_Load);
        }

        void FrmDBObject_Load(object sender, EventArgs e)
        {
            var mdiParent = this.MdiParent as FrmMain;
            if (mdiParent == null || mdiParent.CurrentConnecttion == null) { throw new ArgumentException("找不到MdiParent或当前数据库连接"); }
            _currentConnection = mdiParent.CurrentConnecttion;
            _mssqlHelper = new MsSqlHelper(_currentConnection.ToString());
        }

        private void InitControls()
        {
            cboObjectType.Items.Clear();
            var lst = new List<Object>();
            lst.Add(new { Name = "--请选择--", Value = 0 });
            lst.Add(new { Name = "视图", Value = 2 });
            lst.Add(new { Name = "存储过程", Value = 3 });
            lst.Add(new { Name = "用户函数", Value = 4 });
            cboObjectType.DisplayMember = "Name";
            cboObjectType.ValueMember = "Value";
            cboObjectType.DataSource = lst;
            cboObjectType.SelectedIndexChanged += new EventHandler(cboObjectType_SelectedIndexChanged);
            lbObjects.MultiColumn = false;
            lbObjects.DoubleClick += new EventHandler(lbObjects_DoubleClick);
        }

        void lbObjects_DoubleClick(object sender, EventArgs e)
        {
            //if (lbObjects.SelectedIndex < 0) { return; }
            var obj = lbObjects.SelectedItem as DatabaseObject;
            if (obj == null) { return; }
            var dt = _mssqlHelper.ExecuteQueryAsDataTable(string.Format("exec sp_helptext @objname = '{0}'", obj.Name), CommandType.Text);
            if (dt.Rows.Count <= 0) { MessageBox.Show("读取失败,对象可能加密!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            StringBuilder text = new StringBuilder();
            foreach (DataRow row in dt.Rows)
            {
                text.Append(row[0].ToString());
            }
            txtScript.Text = text.ToString();
        }

        void cboObjectType_SelectedIndexChanged(object sender, EventArgs e)
        {            
            if(cboObjectType.SelectedIndex <= 0){return;}
            lbObjects.DataSource = null;
            var type = (DatabaseObjectType)Enum.Parse(typeof(DatabaseObjectType), cboObjectType.SelectedValue.ToString());
            if (Objects == null)
            {
                Objects = _mssqlHelper.ExecuteQueryAsList<DatabaseObject>(DatabaseObject.QUERY_SQL, CommandType.Text);
            }
            var lst = Objects.Where(o => o.ObjectType == type).ToList();
            lbObjects.DisplayMember = "Name";
            lbObjects.ValueMember = "Id";
            lbObjects.DataSource = lst;
            lbObjects.Refresh();
        }
    }
}
