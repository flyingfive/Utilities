using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FlyingFive;
using FlyingSocket.Client;

namespace ClientTests
{
    public partial class FrmClient : Form
    {
        private UploadSocketClient _socketClient = null;
        private FlyingSocketClient _flyingSocketClient = null;
        public FrmClient()
        {
            InitializeComponent();
            _flyingSocketClient = new FlyingSocketClient();
            _socketClient = new UploadSocketClient();
            _socketClient.OnConnected += (s, e) => { DisplayMsg("客户端已连接。"); };
            _flyingSocketClient.OnConnected += (s, e) => { DisplayMsg("客户端已连接。"); };
        }


        private void DisplayMsg(string msg)
        {
            if (string.IsNullOrEmpty(msg)) { return; }
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<string>(this.DisplayMsg), new object[] { msg });
            }
            else
            {
                msg = string.Format("[{0}] {1}{2}", DateTime.Now.ToString("HH:mm:ss"), msg, Environment.NewLine);
                txtMsg.AppendText(msg);
                txtMsg.ScrollToCaret();
            }
        }

        private void FrmClient_Load(object sender, EventArgs e)
        {

        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            var host = txtHost.Text.Trim().Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
            _flyingSocketClient.ConnectAsync(host.First(), host.Last().TryConvert<int>(52520));
            //_socketClient.Connect(host.First(), host.Last().TryConvert<int>(52520));

        }

        private void button1_Click(object sender, EventArgs e)
        {
            _flyingSocketClient.SendAsync("你好，我是刘碧清。");
            //_socketClient.DoLogin("admin", "123456");
        }
    }
}
