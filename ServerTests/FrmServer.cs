using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FlyingFive;
using FlyingSocket.Server;

namespace ServerTests
{
    public partial class FrmServer : Form
    {
        private FlyingSocketServer _socketServer = null;

        public FrmServer()
        {
            InitializeComponent();
            this.FormClosing += FrmServer_FormClosing;
            _socketServer = FlyingSocketFactory.Default;
            _socketServer.ServerStarted += (s, e) => { DisplayMsg(string.Format("服务运行中,地址:{0}", _socketServer.WorkingAddress.ToString())); };
            _socketServer.ClientConnected += _socketServer_OnClientConnected;
        }

        private void FrmServer_FormClosing(object sender, FormClosingEventArgs e)
        {
            _socketServer.Stop();
        }

        private void _socketServer_OnClientConnected(object sender, UserTokenEventArgs e)
        {
            var msg = string.Format("接收到新客户端{0}连接。地址:{1}", e.UserToken.TokenId, e.UserToken.ConnectSocket.RemoteEndPoint.ToString());
            DisplayMsg(msg);
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

        private void FrmServer_Load(object sender, EventArgs e)
        {
            _socketServer.Start();
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }
    }
}
