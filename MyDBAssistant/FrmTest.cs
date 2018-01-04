using FlyingFive.Win;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MyDBAssistant
{
    public partial class FrmTest : Form
    {
        protected override CreateParams CreateParams
        {
            get
            {
                //控件重绘只对vista以上系统生效，xp会cpu满载导致界面显示有问题
                if (Environment.OSVersion.Version.Major >= 6)
                {
                    CreateParams cp = base.CreateParams;
                    cp.ExStyle |= 0x02000000; // Turn on WS_EX_COMPOSITED 
                    return cp;
                }
                else
                {
                    //CreateParams cp = base.CreateParams;
                    //if (this.IsXpOr2003 == true)
                    //{
                    //    cp.ExStyle |= 0x00080000;  // Turn on WS_EX_LAYERED
                    //    this.Opacity = 1;
                    //}
                    return base.CreateParams;
                }
            }
        }
        public FrmTest()
        {
            InitializeComponent();
            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
            //Win32Utility.SetCueText(textBox1, "test");
            //Win32Utility.SetCueText(comboBox1, "ffff");
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Task.Factory.StartNew(() => { textBox1.SetCueText("test"); comboBox1.SetCueText("ffff"); });
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //Task.Factory.StartNew(() => { var txt = textBox1.GetCueText(); Console.WriteLine(txt); });
            //var txt = textBox1.GetCueText();
            //txt = Win32Utility.GetCueText(textBox1);
            //return;
            Task.Factory.StartNew(() =>
            {
                button1.UIThreadInvoke(new Action(() => { button1.Text = "处理中..."; button1.Enabled = false; MsgBox.Information(button1.Text); }));
                System.Threading.Thread.Sleep(10000);
            }).ContinueWith(t =>
            {
                var ret = button1.UIThreadInvoke(new Func<int>(() =>
                {
                    button1.Text = "已完成";
                    button1.Enabled = true;
                    return int.MaxValue;
                }));
                Console.WriteLine("返回：" + ret.ToString());
            });
        }

        private void button2_Click(object sender, EventArgs e)
        {
            button1.UIThreadInvoke(new Action(() => { button2.Text = "处理中..."; button2.Enabled = false; MsgBox.Information(button2.Text); }));
            System.Threading.Thread.Sleep(10000);
            var ret = button1.UIThreadInvoke(new Func<int>(() =>
            {

                button2.Text = "已完成";
                button2.Enabled = true;
                return int.MaxValue;
            }));
            Console.WriteLine("返回：" + ret.ToString());
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var panel = new MyPanel() { Name = "myPanel", Dock = DockStyle.Fill, Alpha = 80, BackColor = Color.Transparent };
            var btn = new Button() { Name = "btnClose", Text = "关闭", Location = new Point(this.Width / 2, this.Height / 2) };
            btn.Click += btn_Click;
            panel.Controls.Add(btn);
            this.Controls.Add(panel);
            panel.BringToFront();
        }

        void btn_Click(object sender, EventArgs e)
        {
            var pnl = this.Controls.Find("myPanel", true).FirstOrDefault() as MyPanel;
            if (pnl != null)
            {
                this.Controls.Remove(pnl);
            }
        }
    }

    public class MyPanel : Panel
    {
        public int Alpha { get; set; }
        public MyPanel()
        {
            //this.SetStyle(ControlStyles.Opaque | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
        }

        /// <summary>
        /// 控件透明特性
        /// </summary>
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                // 开启 WS_EX_TRANSPARENT,使控件支持透明
                cp.ExStyle |= 0x20;
                return cp;
            }
        }


        protected override void OnPaint(PaintEventArgs e)
        {
            //// 定义颜色的透明度
            Color drawColor = Color.FromArgb(this.Alpha, this.BackColor);
            //// 定义画笔
            Pen labelBorderPen = new Pen(drawColor, 0);
            SolidBrush labelBackColorBrush = new SolidBrush(drawColor);
            //// 绘制背景色
            e.Graphics.DrawRectangle(labelBorderPen, 0, 0, Size.Width, Size.Height);
            e.Graphics.FillRectangle(labelBackColorBrush, 0, 0, Size.Width, Size.Height);

            base.OnPaint(e);
        }



    }
}
