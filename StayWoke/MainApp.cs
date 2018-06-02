using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Windows.Forms;

/***
 * StayWoke
 * A utility to manage the lid close action depending on the current state of the machine.
 * Prevents the PC from sleeping on lid close if an external monitor is connected and active.
 ***/
namespace StayWoke
{
    public partial class MainApp : Form
    {
        public MainApp()
        {
            InitializeComponent();
            onMonitorStateChange();
            SystemEvents.PowerModeChanged += OnPowerChange;
        }

        private DateTime lastUpdate;
        private LID_CLOSE_ACTION lastAction;

        #region "hide winform"
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

        #endregion

        #region "event listeners"
        // Ensure that the current state is always up to date

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

        // Listen for PowerMode change event
        private void OnPowerChange(object sender, PowerModeChangedEventArgs e)
        {
            switch (e.Mode)
            {
                case PowerModes.Resume:
                    onMonitorStateChange();
                    break;
            }
        }

        #endregion

        #region "event handler"
        //
        private void onMonitorStateChange()
        {
            Debug.Print("onMonitorStateChange");

            LID_CLOSE_ACTION action;
            if (SystemInformation.MonitorCount > 1) // TODO use WMI to get *physical* monitor count, not just *enabled* monitors
            {
                action = LID_CLOSE_ACTION.DO_NOTHING;
            }
            else
            {
                action = LID_CLOSE_ACTION.SLEEP;
            }
            updateCurrentPowerPlanLidCloseAction(action);
        }

        #endregion

        #region "util"

        enum LID_CLOSE_ACTION :int
        {
            DO_NOTHING = 0,
            SLEEP = 1,
            HIBERNATE = 2,
            SHUT_DOWN = 3
        }

        void updateCurrentPowerPlanLidCloseAction(LID_CLOSE_ACTION action)
        {
            if (action != lastAction || lastUpdate != null && DateTime.Now.Subtract(lastUpdate).Seconds > 10)
            {
                // Update the current power plan 
                callPowercfg(String.Format("-setdcvalueindex SCHEME_CURRENT 4f971e89-eebd-4455-a8de-9e59040e7347 5ca83367-6e45-459f-a27b-476b1d01c936 {0}", (int)action));
                callPowercfg(String.Format("-setacvalueindex SCHEME_CURRENT 4f971e89-eebd-4455-a8de-9e59040e7347 5ca83367-6e45-459f-a27b-476b1d01c936 {0}", (int)action));
                callPowercfg("-SetActive SCHEME_CURRENT");

                lastUpdate = DateTime.Now;
                lastAction = action;
            }
        }

        void callPowercfg(String args)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = "powercfg",
                Arguments = args,
                RedirectStandardInput = true,
                RedirectStandardOutput = false,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true
        };

            Process.Start(startInfo);
        }

        bool isRunningOnBattery()
        {
            return (SystemInformation.PowerStatus.PowerLineStatus == PowerLineStatus.Offline);
        }

        #endregion

    }
}
