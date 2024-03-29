﻿using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Management;
using System.Runtime.InteropServices;
using System.Threading;
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
        public enum SystemMetric {
            SM_CXSCREEN = 0,  // 0x00
            SM_CYSCREEN = 1,  // 0x01
            SM_CXVSCROLL = 2,  // 0x02
            SM_CYHSCROLL = 3,  // 0x03
            SM_CYCAPTION = 4,  // 0x04
            SM_CXBORDER = 5,  // 0x05
            SM_CYBORDER = 6,  // 0x06
            SM_CXDLGFRAME = 7,  // 0x07
                                //SM_CXFIXEDFRAME = 7,  // 0x07
            SM_CYDLGFRAME = 8,  // 0x08
                                //SM_CYFIXEDFRAME = 8,  // 0x08
            SM_CYVTHUMB = 9,  // 0x09
            SM_CXHTHUMB = 10, // 0x0A
            SM_CXICON = 11, // 0x0B
            SM_CYICON = 12, // 0x0C
            SM_CXCURSOR = 13, // 0x0D
            SM_CYCURSOR = 14, // 0x0E
            SM_CYMENU = 15, // 0x0F
            SM_CXFULLSCREEN = 16, // 0x10
            SM_CYFULLSCREEN = 17, // 0x11
            SM_CYKANJIWINDOW = 18, // 0x12
            SM_MOUSEPRESENT = 19, // 0x13
            SM_CYVSCROLL = 20, // 0x14
            SM_CXHSCROLL = 21, // 0x15
            SM_DEBUG = 22, // 0x16
            SM_SWAPBUTTON = 23, // 0x17
            SM_CXMIN = 28, // 0x1C
            SM_CYMIN = 29, // 0x1D
            SM_CXSIZE = 30, // 0x1E
            SM_CYSIZE = 31, // 0x1F
                            //SM_CXSIZEFRAME = 32, // 0x20
            SM_CXFRAME = 32, // 0x20
                             //SM_CYSIZEFRAME = 33, // 0x21
            SM_CYFRAME = 33, // 0x21
            SM_CXMINTRACK = 34, // 0x22
            SM_CYMINTRACK = 35, // 0x23
            SM_CXDOUBLECLK = 36, // 0x24
            SM_CYDOUBLECLK = 37, // 0x25
            SM_CXICONSPACING = 38, // 0x26
            SM_CYICONSPACING = 39, // 0x27
            SM_MENUDROPALIGNMENT = 40, // 0x28
            SM_PENWINDOWS = 41, // 0x29
            SM_DBCSENABLED = 42, // 0x2A
            SM_CMOUSEBUTTONS = 43, // 0x2B
            SM_SECURE = 44, // 0x2C
            SM_CXEDGE = 45, // 0x2D
            SM_CYEDGE = 46, // 0x2E
            SM_CXMINSPACING = 47, // 0x2F
            SM_CYMINSPACING = 48, // 0x30
            SM_CXSMICON = 49, // 0x31
            SM_CYSMICON = 50, // 0x32
            SM_CYSMCAPTION = 51, // 0x33
            SM_CXSMSIZE = 52, // 0x34
            SM_CYSMSIZE = 53, // 0x35
            SM_CXMENUSIZE = 54, // 0x36
            SM_CYMENUSIZE = 55, // 0x37
            SM_ARRANGE = 56, // 0x38
            SM_CXMINIMIZED = 57, // 0x39
            SM_CYMINIMIZED = 58, // 0x3A
            SM_CXMAXTRACK = 59, // 0x3B
            SM_CYMAXTRACK = 60, // 0x3C
            SM_CXMAXIMIZED = 61, // 0x3D
            SM_CYMAXIMIZED = 62, // 0x3E
            SM_NETWORK = 63, // 0x3F
            SM_CLEANBOOT = 67, // 0x43
            SM_CXDRAG = 68, // 0x44
            SM_CYDRAG = 69, // 0x45
            SM_SHOWSOUNDS = 70, // 0x46
            SM_CXMENUCHECK = 71, // 0x47
            SM_CYMENUCHECK = 72, // 0x48
            SM_SLOWMACHINE = 73, // 0x49
            SM_MIDEASTENABLED = 74, // 0x4A
            SM_MOUSEWHEELPRESENT = 75, // 0x4B
            SM_XVIRTUALSCREEN = 76, // 0x4C
            SM_YVIRTUALSCREEN = 77, // 0x4D
            SM_CXVIRTUALSCREEN = 78, // 0x4E
            SM_CYVIRTUALSCREEN = 79, // 0x4F
            SM_CMONITORS = 80, // 0x50
            SM_SAMEDISPLAYFORMAT = 81, // 0x51
            SM_IMMENABLED = 82, // 0x52
            SM_CXFOCUSBORDER = 83, // 0x53
            SM_CYFOCUSBORDER = 84, // 0x54
            SM_TABLETPC = 86, // 0x56
            SM_MEDIACENTER = 87, // 0x57
            SM_STARTER = 88, // 0x58
            SM_SERVERR2 = 89, // 0x59
            SM_MOUSEHORIZONTALWHEELPRESENT = 91, // 0x5B
            SM_CXPADDEDBORDER = 92, // 0x5C
            SM_DIGITIZER = 94, // 0x5E
            SM_MAXIMUMTOUCHES = 95, // 0x5F

            SM_REMOTESESSION = 0x1000, // 0x1000
            SM_SHUTTINGDOWN = 0x2000, // 0x2000
            SM_REMOTECONTROL = 0x2001, // 0x2001
        }
        [DllImport("user32.dll")]
        static extern int GetSystemMetrics(SystemMetric smIndex);

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

        const int PBT_POWERSETTINGCHANGE = 0x8013;

        private bool? _previousLidState = null;
        private void onPowerBroadcast(Message m)
        {
            Debug.Print($"onPowerBroadcast: {m.WParam}");
            if ((int)m.WParam == PBT_POWERSETTINGCHANGE)
            {
                POWERBROADCAST_SETTING ps = (POWERBROADCAST_SETTING)Marshal.PtrToStructure(m.LParam, typeof(POWERBROADCAST_SETTING));

                if (ps.PowerSetting == GUID_LIDSWITCH_STATE_CHANGE)
                {
                    bool isLidOpen = ps.Data != 0;
                    Debug.Print($"GUID_LIDSWITCH_STATE_CHANGE: isLidOpen=[{isLidOpen}]");


                    if (!isLidOpen == _previousLidState)
                    {
                        Debug.Print("lid state toggled");
                        // lid status has changed to 'open', now refresh our monitor state
                        // (designed to catch if StayWoke failed to work for a prior "lid close" action)
                        if (isLidOpen) {
                            onMonitorStateChange();
                        }
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
                    Debug.Print("PowerMode -> Resume");
                    onMonitorStateChange();
                    break;
            }
        }

        #endregion

        #region "event handler"
        //
        private void onMonitorStateChange()
        {
            Thread.Sleep(1500);

            LID_CLOSE_ACTION action;

            // count of monitors connected to the PC
            int monitorCount = GetSystemMetrics(SystemMetric.SM_CMONITORS);

#if DEBUG
            Debug.Print($"onMonitorStateChange: monitorCount=[{monitorCount}]");
#endif

            if (monitorCount > 1) // TODO use WMI to get *physical* monitor count, not just *enabled* monitors
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
