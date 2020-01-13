using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FlyingFive;
using FlyingFive.Utils;
using FlyingSocket.Client;

namespace ClientTests
{
    public partial class FrmClient : Form
    {
        private UploadSocketClient _socketClient = null;
        private DefaultSocketClient _flyingSocketClient = null;
        public FrmClient()
        {
            InitializeComponent();
            _flyingSocketClient = new DefaultSocketClient("01010001", "AB23522F7A734D39B984FC1F0B52E465");
            _socketClient = new UploadSocketClient();
            _socketClient.OnConnected += (s, e) => { DisplayMsg(string.Format("客户端已连接，会话ID:{0}。", _socketClient.SessionID)); };
            _flyingSocketClient.OnConnected += (s, e) =>
            {
                DisplayMsg(string.Format("客户端已连接，会话ID:{0}。", _flyingSocketClient.SessionID));
                //var text = "客户端已连接".PadLeft(1000000, '你');
                //for (int i = 0; i < 20; i++)
                //{
                //    _flyingSocketClient.SendAsync(text);
                //} 
            };
            _flyingSocketClient.OnDisconnected += (s, e) => { DisplayMsg("客户端已断开。"); };
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
            btnConnect_Click(btnConnect, EventArgs.Empty);
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            var host = txtHost.Text.Trim().Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
            //_flyingSocketClient.ConnectAsync(host.First(), host.Last().TryConvert<int>(52520));
            if (_flyingSocketClient.IsConnected)
            {
                //_socketClient.Disconnect();
                _flyingSocketClient.Disconnect();
            }
            else
            {
                _socketClient.Connect(host.First(), host.Last().TryConvert<int>(52520));
                _flyingSocketClient.ConnectAsync(host.First(), host.Last().TryConvert<int>(52520));
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var flag = true;
            if (flag)
            {
                var fileName = "";
                using (var ofd = new OpenFileDialog() { Multiselect = false })
                {
                    if (ofd.ShowDialog() != DialogResult.OK)
                    {
                        return;
                    }
                    fileName = ofd.FileName;
                }
                var dir = new DirectoryInfo(Path.GetDirectoryName(fileName)).Name;
                var size = 0L;
                var time = CodeTimer.Time("aa", 1, () =>
                {
                    flag = _socketClient.Upload("", fileName, ref size);
                    if (flag)
                    {
                        DisplayMsg(string.Format("文件：{0}上传成功。", fileName));
                    }
                });
                DisplayMsg("上传耗时：" + time.ElapsedMillisecondsOnce);
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            var msg = txtContent.Text;
            if (string.IsNullOrWhiteSpace(msg)) { return; }
            var time = CodeTimer.Time("aaa", 1, () =>
            {
                //多线程下会服务端接收处理异常。
                //Parallel.For(1, 10, (i) => { _flyingSocketClient.SendAsync(msg); });
                _flyingSocketClient.SendAsync(msg);
            });
            DisplayMsg("发送耗时：" + time.ElapsedMillisecondsOnce);
        }
    }
}
