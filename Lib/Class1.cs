using System.Diagnostics;

namespace Lib {
    public class Class1 {
        public enum LID_CLOSE_ACTION : int {
            DO_NOTHING = 0,
            SLEEP = 1,
            HIBERNATE = 2,
            SHUT_DOWN = 3
        }

        public void updateCurrentPowerPlanLidCloseAction(LID_CLOSE_ACTION action) {
            // Update the current power plan 
            callPowercfg(String.Format("-setdcvalueindex SCHEME_CURRENT 4f971e89-eebd-4455-a8de-9e59040e7347 5ca83367-6e45-459f-a27b-476b1d01c936 {0}", (int)action));
            callPowercfg(String.Format("-setacvalueindex SCHEME_CURRENT 4f971e89-eebd-4455-a8de-9e59040e7347 5ca83367-6e45-459f-a27b-476b1d01c936 {0}", (int)action));
             callPowercfg("-SetActive SCHEME_CURRENT");
        }

        public void callPowercfg(String args) {
            ProcessStartInfo startInfo = new ProcessStartInfo() {
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
    }
}