/* ==============================================================================
* 功能描述：FrmAbout  
* 创 建 者：liubq
* 创建日期：2015/12/24 14:30:08
* ==============================================================================*/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MyDBAssistant
{
    /// <summary>
    /// 
    /// </summary>
    public partial class FrmAbout : Form
    {
        public FrmAbout()
        {
            InitializeComponent();
            this.KeyDown += new KeyEventHandler(FrmAbout_KeyDown);
        }

        void FrmAbout_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                btnOK_Click(btnOK, EventArgs.Empty);
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
