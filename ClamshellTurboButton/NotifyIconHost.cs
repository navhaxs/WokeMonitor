﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClamshellTurboButton
{
    public partial class NotifyIconHost : Form
    {

        ReadyToClamshellWindow wnd;

        public NotifyIconHost()
        {
            InitializeComponent();
            FormBorderStyle = FormBorderStyle.None;
        }
        
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                // turn on WS_EX_TOOLWINDOW style bit
                cp.ExStyle |= 0x80;
                return cp;
            }
        }

        private void notifyIcon1_Click(object sender, EventArgs e)
        {
            if (wnd != null)
            {
                wnd.Close();
                wnd.Dispose();
            }
            wnd = new ReadyToClamshellWindow();
            wnd.Show();
        }

        private void NotifyIconHost_Load(object sender, EventArgs e)
        {
            Visible = false;
            ShowInTaskbar = false;
        }
    }
}
