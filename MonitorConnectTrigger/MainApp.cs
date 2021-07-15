using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonitorConnectTrigger
{
    public partial class MainApp : Form
    {
        public MainApp()
        {
            InitializeComponent();
        }

        // a winform is requried to receive WndProc messages, but we want to hide the actual winform itself
        // https://stackoverflow.com/questions/357076/best-way-to-hide-a-window-from-the-alt-tab-program-switcher
        protected override CreateParams CreateParams
        {
            get
            {
                var Params = base.CreateParams;
                Params.ExStyle |= 0x80;
                return Params;
            }
        }

        // Listen for WM_DISPLAYCHANGE event
        protected override void WndProc(ref Message m)
        {
            const uint WM_DISPLAYCHANGE = 0x007e;

            switch ((uint)m.Msg)
            {
                case WM_DISPLAYCHANGE:
                    onMonitorStateChange();
                    break;
            }

            base.WndProc(ref m);
        }

        private void onMonitorStateChange()
        {
            if (SystemInformation.MonitorCount > 1) // TODO use WMI to get *physical* monitor count, not just *enabled* monitors
            {
                System.Diagnostics.Process.Start("C:\\Opt\\ControlMyMonitor.exe", @"/SetValueIfNeeded ""DELL U2515H"" 60 17");
            }    
        }
    }
}
