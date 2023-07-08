using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
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
        [DllImport(@"User32", SetLastError = true, EntryPoint = "RegisterPowerSettingNotification",
           CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr RegisterPowerSettingNotification(IntPtr hRecipient, ref Guid PowerSettingGuid,
           Int32 Flags);

        Guid GUID_LIDSWITCH_STATE_CHANGE = new Guid(0xBA3E0F4D, 0xB817, 0x4094, 0xA2, 0xD1, 0xD5, 0x63, 0x79, 0xE6, 0xA0, 0xF3);
        const int DEVICE_NOTIFY_WINDOW_HANDLE = 0x00000000;

        public MainApp()
        {
            InitializeComponent();
            onMonitorStateChange();
            SystemEvents.PowerModeChanged += OnPowerChange;

            IntPtr hLIDSWITCHSTATECHANGE = RegisterPowerSettingNotification(this.Handle,
       ref GUID_LIDSWITCH_STATE_CHANGE,
       DEVICE_NOTIFY_WINDOW_HANDLE);
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
            const uint WM_POWERBROADCAST = 0x0218;

            switch ((uint)m.Msg)
            {
                case WM_DISPLAYCHANGE:
                    onMonitorStateChange();
                    break;
                case WM_POWERBROADCAST:
                    onPowerBroadcast(m);
                    break;
                default:
                    break;
            }

            base.WndProc(ref m);
        }
        internal struct POWERBROADCAST_SETTING
        {
            public Guid PowerSetting;
            public uint DataLength;
            public byte Data;
        }

        private bool? _previousLidState = null;
        private void onPowerBroadcast(Message m)
        {

            const int PBT_POWERSETTINGCHANGE = 0x8013;
            const int PBT_APMPOWERSTATUSCHANGE = 0xA;

            if ((int)m.WParam == PBT_POWERSETTINGCHANGE)
            {
                POWERBROADCAST_SETTING ps = (POWERBROADCAST_SETTING)Marshal.PtrToStructure(m.LParam, typeof(POWERBROADCAST_SETTING));

                if (ps.PowerSetting == GUID_LIDSWITCH_STATE_CHANGE)
                {
                    bool isLidOpen = ps.Data != 0;

                    if (!isLidOpen == _previousLidState)
                    {
                        // lid status has changed to 'open', now refresh our monitor state
                        if (isLidOpen)
                            onMonitorStateChange();
                    }

                    _previousLidState = isLidOpen;
                }
            }
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
#if DEBUG
            Debug.Print("onMonitorStateChange");
#endif

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
