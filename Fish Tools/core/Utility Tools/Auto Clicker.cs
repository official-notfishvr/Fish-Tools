using Microsoft.VisualBasic;
using System.Runtime.InteropServices;
using static Fish_Tools.core.Utils.Utils;
using Constants = Fish_Tools.core.Utils.Utils.Constants;
using Fish_Tools.core.Utils;

namespace Fish_Tools.core.MiscTools
{
    public partial class Auto_Clicker : Form, ITool
    {
        public string Name => "Auto Clicker";
        public string Category => "Utility Tools";
        public bool IsEnabled { get; set; } = true;
        public string Description => "Automated clicking and key pressing tool";

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);

        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const int MOUSEEVENTF_RIGHTUP = 0x10;

        private KeyHandler _ghk;
        private float _interval = 10000;
        private int _offset;
        private readonly Random _random = new Random();

        public Auto_Clicker()
        {
            InitializeComponent();
            _ghk = new KeyHandler(Constants.NOMOD, Keys.F6, this);
            _ghk.Register();
            UpdateMaxProgressLabel();
        }

        public void Main(Logger Logger)
        {
            this.Show();
            this.BringToFront();
        }

        private void DoMouseClick(uint mouseEventFlags)
        {
            int x = Cursor.Position.X;
            int y = Cursor.Position.Y;
            mouse_event(mouseEventFlags, (uint)x, (uint)y, 0, 0);
        }
        private void button1_Click(object sender, EventArgs e)
        {
            ApplyIntervalSettings();
            timer1.Start();
            timer2.Start();
            ToggleControls(false);
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (KeyRdBtn.Checked)
            {
                SendKeys.Send(KeyTxtBox.Text);
            }
            else if (LeftMouseRdBtn.Checked)
            {
                DoMouseClick(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP);
            }
            else if (RightMouseRdBtn.Checked)
            {
                DoMouseClick(MOUSEEVENTF_RIGHTDOWN | MOUSEEVENTF_RIGHTUP);
            }

            IntervalProgressBar.Value = 0;
            ApplyOffsetIfNeeded();
        }
        private void button2_Click(object sender, EventArgs e)
        {
            timer1.Stop();
            timer2.Stop();
            IntervalProgressBar.Value = 0;
            ToggleControls(true);
        }
        private void HandleHotkey()
        {
            if (button1.Enabled)
            {
                button1_Click(this, null);
            }
            else
            {
                button2_Click(this, null);
            }
        }
        private void ApplyIntervalSettings()
        {
            timer1.Interval = Convert.ToInt32(_interval);
            IntervalProgressBar.Maximum = Convert.ToInt32(_interval) / 100;
            IntervalProgressBar.Minimum = 0;
            ApplyOffsetIfNeeded();
        }
        private void ApplyOffsetIfNeeded()
        {
            if (_offset > 0)
            {
                _offset = _random.Next(0, Convert.ToInt32(OffsetTxtBox.Text));
                timer1.Interval = Convert.ToInt32(_interval) + _offset;
                UpdateMaxProgressLabel();
                IntervalProgressBar.Maximum = (Convert.ToInt32(_interval + _offset) / 100);
            }
        }
        private void ToggleControls(bool enabled)
        {
            button1.Enabled = enabled;
            button2.Enabled = !enabled;
            IntervalPanel.Enabled = enabled;
            PressTypePanel.Enabled = enabled;
        }
        protected override void WndProc(ref Message m)
        {
            _interval = Convert.ToInt32(IntervalTxtBox.Text);
            _offset = _random.Next(0, Convert.ToInt32(OffsetTxtBox.Text));
            UpdateMaxProgressLabel();

            if (m.Msg == Constants.WM_HOTKEY_MSG_ID) { HandleHotkey(); }
            base.WndProc(ref m);
        }

        private void KeyRdBtn_CheckedChanged(object sender, EventArgs e) => KeyTxtBox.Enabled = KeyRdBtn.Checked;
        private void LeftMouseRdBtn_CheckedChanged(object sender, EventArgs e) => KeyTxtBox.Enabled = false;
        private void RightMouseRdBtn_CheckedChanged(object sender, EventArgs e) => KeyTxtBox.Enabled = false;
        private void timer2_Tick(object sender, EventArgs e) => IntervalProgressBar.Increment(10);
        private void UpdateMaxProgressLabel() => MaxProgressLbl.Text = ((_interval + _offset) / 1000).ToString("F1") + "s";
    }
}
