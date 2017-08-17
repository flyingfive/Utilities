namespace MyDBAssistant
{
    partial class FrmMain
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.msMain = new System.Windows.Forms.MenuStrip();
            this.tsmiUtility = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiNewDb = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiSwitchDB = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiGenerateEntity = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiTableDesign = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiDBObject = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiExit = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiMakeCert = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiWindow = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiHelp = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiAbout = new System.Windows.Forms.ToolStripMenuItem();
            this.ssMain = new System.Windows.Forms.StatusStrip();
            this.tsslServer = new System.Windows.Forms.ToolStripStatusLabel();
            this.tsslDB = new System.Windows.Forms.ToolStripStatusLabel();
            this.tsslDBUser = new System.Windows.Forms.ToolStripStatusLabel();
            this.tsslDBVer = new System.Windows.Forms.ToolStripStatusLabel();
            this.msMain.SuspendLayout();
            this.ssMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // msMain
            // 
            this.msMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiUtility,
            this.tsmiMakeCert,
            this.tsmiWindow,
            this.tsmiHelp});
            this.msMain.Location = new System.Drawing.Point(0, 0);
            this.msMain.Name = "msMain";
            this.msMain.Size = new System.Drawing.Size(984, 25);
            this.msMain.TabIndex = 0;
            this.msMain.Text = "menuStrip1";
            // 
            // tsmiUtility
            // 
            this.tsmiUtility.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiNewDb,
            this.tsmiSwitchDB,
            this.tsmiGenerateEntity,
            this.tsmiTableDesign,
            this.tsmiDBObject,
            this.tsmiExit});
            this.tsmiUtility.Name = "tsmiUtility";
            this.tsmiUtility.Size = new System.Drawing.Size(53, 21);
            this.tsmiUtility.Text = "工具&U";
            // 
            // tsmiNewDb
            // 
            this.tsmiNewDb.Name = "tsmiNewDb";
            this.tsmiNewDb.Size = new System.Drawing.Size(152, 22);
            this.tsmiNewDb.Text = "创建&N...";
            this.tsmiNewDb.Click += new System.EventHandler(this.MenuItem_Click);
            // 
            // tsmiSwitchDB
            // 
            this.tsmiSwitchDB.Name = "tsmiSwitchDB";
            this.tsmiSwitchDB.Size = new System.Drawing.Size(152, 22);
            this.tsmiSwitchDB.Text = "连接数据库&S";
            this.tsmiSwitchDB.Click += new System.EventHandler(this.MenuItem_Click);
            // 
            // tsmiGenerateEntity
            // 
            this.tsmiGenerateEntity.Name = "tsmiGenerateEntity";
            this.tsmiGenerateEntity.Size = new System.Drawing.Size(152, 22);
            this.tsmiGenerateEntity.Text = "模型工具&E";
            this.tsmiGenerateEntity.Click += new System.EventHandler(this.MenuItem_Click);
            // 
            // tsmiTableDesign
            // 
            this.tsmiTableDesign.Name = "tsmiTableDesign";
            this.tsmiTableDesign.Size = new System.Drawing.Size(152, 22);
            this.tsmiTableDesign.Text = "表设计器&T";
            this.tsmiTableDesign.Click += new System.EventHandler(this.MenuItem_Click);
            // 
            // tsmiDBObject
            // 
            this.tsmiDBObject.Name = "tsmiDBObject";
            this.tsmiDBObject.Size = new System.Drawing.Size(152, 22);
            this.tsmiDBObject.Text = "DB对象维护&O";
            this.tsmiDBObject.Click += new System.EventHandler(this.MenuItem_Click);
            // 
            // tsmiExit
            // 
            this.tsmiExit.Name = "tsmiExit";
            this.tsmiExit.Size = new System.Drawing.Size(152, 22);
            this.tsmiExit.Text = "退出&E";
            this.tsmiExit.Click += new System.EventHandler(this.MenuItem_Click);
            // 
            // tsmiMakeCert
            // 
            this.tsmiMakeCert.Name = "tsmiMakeCert";
            this.tsmiMakeCert.Size = new System.Drawing.Size(68, 21);
            this.tsmiMakeCert.Text = "数字证书";
            this.tsmiMakeCert.Click += new System.EventHandler(this.MenuItem_Click);
            // 
            // tsmiWindow
            // 
            this.tsmiWindow.Name = "tsmiWindow";
            this.tsmiWindow.Size = new System.Drawing.Size(56, 21);
            this.tsmiWindow.Text = "窗口&W";
            // 
            // tsmiHelp
            // 
            this.tsmiHelp.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiAbout});
            this.tsmiHelp.Name = "tsmiHelp";
            this.tsmiHelp.Size = new System.Drawing.Size(53, 21);
            this.tsmiHelp.Text = "帮助&H";
            // 
            // tsmiAbout
            // 
            this.tsmiAbout.Name = "tsmiAbout";
            this.tsmiAbout.Size = new System.Drawing.Size(108, 22);
            this.tsmiAbout.Text = "关于&A";
            this.tsmiAbout.Click += new System.EventHandler(this.MenuItem_Click);
            // 
            // ssMain
            // 
            this.ssMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsslServer,
            this.tsslDB,
            this.tsslDBUser,
            this.tsslDBVer});
            this.ssMain.Location = new System.Drawing.Point(0, 740);
            this.ssMain.Name = "ssMain";
            this.ssMain.Size = new System.Drawing.Size(984, 22);
            this.ssMain.TabIndex = 2;
            this.ssMain.Text = "statusStrip1";
            // 
            // tsslServer
            // 
            this.tsslServer.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsslServer.Name = "tsslServer";
            this.tsslServer.Size = new System.Drawing.Size(0, 17);
            this.tsslServer.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tsslDB
            // 
            this.tsslDB.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsslDB.Name = "tsslDB";
            this.tsslDB.Size = new System.Drawing.Size(0, 17);
            this.tsslDB.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tsslDBUser
            // 
            this.tsslDBUser.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsslDBUser.Name = "tsslDBUser";
            this.tsslDBUser.Size = new System.Drawing.Size(0, 17);
            this.tsslDBUser.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tsslDBVer
            // 
            this.tsslDBVer.Name = "tsslDBVer";
            this.tsslDBVer.Size = new System.Drawing.Size(0, 17);
            // 
            // FrmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(984, 762);
            this.Controls.Add(this.ssMain);
            this.Controls.Add(this.msMain);
            this.IsMdiContainer = true;
            this.MainMenuStrip = this.msMain;
            this.Name = "FrmMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Xen数据库[模型]工具";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.msMain.ResumeLayout(false);
            this.msMain.PerformLayout();
            this.ssMain.ResumeLayout(false);
            this.ssMain.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip msMain;
        private System.Windows.Forms.ToolStripMenuItem tsmiUtility;
        private System.Windows.Forms.ToolStripMenuItem tsmiNewDb;
        private System.Windows.Forms.ToolStripMenuItem tsmiGenerateEntity;
        private System.Windows.Forms.ToolStripMenuItem tsmiTableDesign;
        private System.Windows.Forms.ToolStripMenuItem tsmiSwitchDB;
        private System.Windows.Forms.StatusStrip ssMain;
        private System.Windows.Forms.ToolStripStatusLabel tsslServer;
        private System.Windows.Forms.ToolStripStatusLabel tsslDB;
        private System.Windows.Forms.ToolStripStatusLabel tsslDBUser;
        private System.Windows.Forms.ToolStripStatusLabel tsslDBVer;
        private System.Windows.Forms.ToolStripMenuItem tsmiExit;
        private System.Windows.Forms.ToolStripMenuItem tsmiMakeCert;
        private System.Windows.Forms.ToolStripMenuItem tsmiWindow;
        private System.Windows.Forms.ToolStripMenuItem tsmiDBObject;
        private System.Windows.Forms.ToolStripMenuItem tsmiHelp;
        private System.Windows.Forms.ToolStripMenuItem tsmiAbout;
    }
}