using System.ComponentModel;

namespace Fish_Tools.core.MiscTools.AccountChecker.Checkers.Steam
{
    public partial class MainWindow : Form
    {
        public MainWindow() => InitializeComponent();

        private void MainWindow_Load(object sender, EventArgs e)
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            tabControl1.SelectedIndex = 1;
            InitializeUIState();
        }

        private void InitializeUIState()
        {
            whatsHappening.Visible = false;
            ButtonStart.Enabled = false;
        }

        protected override void OnResizeBegin(EventArgs e)
        {
            SuspendLayout();
            base.OnResizeBegin(e);
        }

        protected override void OnResizeEnd(EventArgs e)
        {
            ResumeLayout();
            base.OnResizeEnd(e);
        }
        private void ButtonStart_Click(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex != 0)
            {
                ShowMessage("Choose 'Check manually' or 'Check automatically' to check for steam accounts.", string.Empty);
                return;
            }

            if (string.IsNullOrWhiteSpace(SteamAccountHelper.filePath))
            {
                ShowMessage("You need to open a file with Steam accounts.", "Open a file");
                return;
            }

            if (!File.Exists(SteamAccountHelper.filePath))
            {
                ShowMessage("File Path needs to have content in it.", "File Path");
                return;
            }

            if (string.IsNullOrWhiteSpace(SteamAccountHelper.fileContent))
            {
                ShowMessage("The file you tried to check for doesn't exist or is inaccessible. Please try placing the file in a different location.", "File");
                return;
            }

            StartAutomaticCheck();
        }

        private void StartAutomaticCheck()
        {
            try
            {
                BackgroundWorker automaticWorker = new BackgroundWorker();
                LogHelper.Log("Cleaning up...\n");
                SteamAccountChecker.CleanupUpdate();
                LogHelper.Log("Starting automatic check...\n");

                automaticWorker.DoWork += AutomaticCheckBW_DoWork;
                automaticWorker.RunWorkerAsync();

                UIHelper.EnableUI(false);
                UIHelper.ShowUI(true);
            }
            catch (Exception ex)
            {
                ShowMessage($"Failed. Error: {ex.Message}", "Internal Error");
            }
        }

        public void AutomaticCheckBW_DoWork(object sender, DoWorkEventArgs e)
        {
            SteamAccountChecker.CheckAutomatically();
            UIHelper.EnableUI(true);
            UIHelper.ShowUI(false);
            LogHelper.Log("✔ Done");
        }

        private void button4_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
                FilterIndex = 2,
                RestoreDirectory = true
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                SteamAccountHelper.filePath = openFileDialog.FileName;
                textBoxFile.Text = SteamAccountHelper.filePath;
                LoadFileContent(openFileDialog);
            }
        }

        private void LoadFileContent(OpenFileDialog openFileDialog)
        {
            try
            {
                using (var fileStream = openFileDialog.OpenFile())
                using (var reader = new StreamReader(fileStream))
                {
                    SteamAccountHelper.fileContent = reader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"Failed. Error: {ex.Message}", "Internal Error");
            }
        }
        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool enableControls = tabControl1.SelectedIndex == 0 || tabControl1.SelectedIndex == 99;
            whatsHappening.Visible = enableControls;
            ButtonStart.Enabled = enableControls;
        }

        private void CopySelectedItemsToClipboard(string separator, Func<ListViewItem, string> selector)
        {
            var selectedItems = whatsHappening.SelectedItems;
            if (selectedItems.Count == 0)
            {
                ShowMessage("You have to select one or more items.", "Clipboard");
                return;
            }

            var clipboardText = string.Join(separator, selectedItems.Cast<ListViewItem>().Select(selector));
            Clipboard.SetText(clipboardText);
            ShowMessage("Selected items were copied to the clipboard.", "Clipboard");
        }
        private void MainWindow_FormClosing(object sender, FormClosingEventArgs e) => Application.Exit();
        private void checkShowPasswordInLogs_CheckedChanged(object sender, EventArgs e) => Settings.showColouredItemsInAccountList = CheckboxShowColouredItemsInAccountList.Checked;
        private void toolStripMenuItem1_Click(object sender, EventArgs e) => CopySelectedItemsToClipboard("\n", item => $"{item.SubItems[1].Text}:{item.SubItems[2].Text}");
        private void toolStripMenuItem2_Click(object sender, EventArgs e) => CopySelectedItemsToClipboard("\n", item => item.SubItems[1].Text);
        private void toolStripMenuItem3_Click(object sender, EventArgs e) => CopySelectedItemsToClipboard("\n", item => item.SubItems[2].Text);
        private void toolStripMenuItem4_Click(object sender, EventArgs e) => CopySelectedItemsToClipboard("\n", item => $"{item.SubItems[1].Text}:{item.SubItems[2].Text}");
        private void button5_Click(object sender, EventArgs e) => AccountExporterHelper.Export(AccountExporterHelper.WhatToExport.GOODACCOUNTS);
        private void button6_Click(object sender, EventArgs e) => AccountExporterHelper.Export(AccountExporterHelper.WhatToExport.BADACCOUNTS);
        private void Button8_Click(object sender, EventArgs e) => WindowsDialogsHelper.FindPlaceToExport();
        private void ShowMessage(string message, string title, MessageBoxIcon icon = MessageBoxIcon.Information) { MessageBox.Show(message, title, MessageBoxButtons.OK, icon); }
    }
}
