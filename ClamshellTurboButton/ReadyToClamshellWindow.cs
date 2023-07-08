using Lib;

namespace ClamshellTurboButton
{
    public partial class ReadyToClamshellWindow : Form
    {
        Class1 lib = new Class1();
        public ReadyToClamshellWindow()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Location = new Point((Screen.PrimaryScreen.WorkingArea.Width - this.Width),
                                     (Screen.PrimaryScreen.WorkingArea.Height - this.Height));
            lib.updateCurrentPowerPlanLidCloseAction(Class1.LID_CLOSE_ACTION.DO_NOTHING);

        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            lib.updateCurrentPowerPlanLidCloseAction(Class1.LID_CLOSE_ACTION.SLEEP);

        }
    }
}