using Gma.System.MouseKeyHook;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace DoubleShiftHelper
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Subscribe();
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new HostForm());
        }

        private static IKeyboardMouseEvents m_GlobalHook;

        public static void Subscribe()
        {
            // Note: for the application hook, use the Hook.AppEvents() instead
            m_GlobalHook = Hook.GlobalEvents();

            //m_GlobalHook.MouseDownExt += GlobalHookMouseDownExt;
            //m_GlobalHook.KeyPress += GlobalHookKeyPress;
            m_GlobalHook.KeyDown += M_GlobalHook_KeyDown;
        }

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, out uint ProcessId);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();
        static string GetActiveProcessFileName()
        {
            IntPtr hwnd = GetForegroundWindow();
            uint pid;
            GetWindowThreadProcessId(hwnd, out pid);
            Process p = Process.GetProcessById((int)pid);
            String? name = Path.GetFileName(p?.MainModule?.FileName); 
            if (name == null)
            {
                return "";
            }
            return name;
        }

        static Keys? lastKeyCode;
        static DateTime? lastPress;

        private static bool isShiftKey(Keys key) => (key == Keys.LShiftKey || key == Keys.RShiftKey);

        private static void M_GlobalHook_KeyDown(object? sender, KeyEventArgs e)
        {
            //Debug.Print("KeyPress: \t{0}\t{1}\t{2}", e.KeyValue, e.Shift, e.KeyData);
            //Debug.Print("Active: \t{0}", GetActiveProcessFileName());

            if (GetActiveProcessFileName() != "devenv.exe" || e.Shift)
            {
                lastKeyCode = null;
                lastPress = null;
                return;
            }

            if (lastKeyCode != null && isShiftKey((Keys)lastKeyCode) && isShiftKey(e.KeyCode)
                && lastKeyCode == e.KeyCode
                && lastPress != null
                && DateTime.Now.Subtract((DateTime)lastPress).TotalMilliseconds < 800)
            {
                SendKeys.Send("^,");
                lastKeyCode = null;
                lastPress = null;
                return;
            }

            lastKeyCode = e.KeyCode;
            lastPress = DateTime.Now;
        }

        private static void GlobalHookKeyPress(object sender, KeyPressEventArgs e)
        {
            Debug.Print("KeyPress: \t{0}", e.KeyChar);
        }

        private static void GlobalHookMouseDownExt(object sender, MouseEventExtArgs e)
        {
            Debug.Print("MouseDown: \t{0}; \t System Timestamp: \t{1}", e.Button, e.Timestamp);

            // uncommenting the following line will suppress the middle mouse button click
            // if (e.Buttons == MouseButtons.Middle) { e.Handled = true; }
        }

        public static void Unsubscribe()
        {
            m_GlobalHook.MouseDownExt -= GlobalHookMouseDownExt;
            m_GlobalHook.KeyPress -= GlobalHookKeyPress;

            //It is recommened to dispose it
            m_GlobalHook.Dispose();
        }
    }
}