/*
------------------------------------------------------------
Made By notfishvr
github: https://github.com/official-notfishvr/Fish-Tools
------------------------------------------------------------
*/

using System.Net.Http.Headers;

namespace Fish_Tools.core.MiscTools.AccountChecker.Checkers.Steam
{
    class WindowsDialogsHelper
    {
        public static void FindPlaceToExport()
        {
            try
            {
                FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();

                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                {
                    SteamAccountHelper.localToExport = folderBrowserDialog.SelectedPath;
                    Settings.mw.textBoxLocalToExport.Text = SteamAccountHelper.localToExport;
                    //TODO: Change this, so instead of showing this message, it automatically exports without requiring user intervention
                    MessageBox.Show("You've successfully set the place to export accounts from.\n\nClick the button again to export your selected accounts.", "Path is now set", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed. Error: {ex.Message}", "Internal Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
    class UIHelper
    {
        delegate void SetVisibleCallback(bool isShown);
        delegate void SetEnabledCallback(bool isEnabled);
        delegate void UpdateRemainingLabelCallback(string text);
        delegate void UpdateLoggableLabelCallback(string text);
        delegate void UpdateBadLabelCallback(string text);
        delegate void UpdateSGProtectedLabelCallback(string text);
        delegate void UpdateCheckedLabelCallback(string text);
        delegate void UpdateFileTextBoxACCallback(string text);

        public static void ShowUI(bool isShown)
        {
            if (Settings.mw.InvokeRequired)
            {
                SetVisibleCallback d = new SetVisibleCallback(ShowUI);
                Settings.mw.loadingImage.Invoke(d, new object[] { isShown });
            }
            else
                Settings.mw.loadingImage.Visible = isShown;
        }

        public static void EnableUI(bool isEnabled)
        {
            if (Settings.mw.InvokeRequired)
            {
                SetEnabledCallback d = new SetEnabledCallback(EnableUI);
                Settings.mw.tabControl1.Invoke(d, new object[] { isEnabled });
            }
            else
            {
                Settings.mw.tabControl1.Enabled = isEnabled;
                Settings.mw.ButtonStart.Enabled = isEnabled;
            }
        }

        public static void UpdateFileTextBoxAC(string text)
        {
            if (Settings.mw.InvokeRequired)
            {
                UpdateFileTextBoxACCallback d = new UpdateFileTextBoxACCallback(UpdateFileTextBoxAC);
                Settings.mw.textBoxFile.Invoke(d, new object[] { text });
            }
            else
                Settings.mw.textBoxFile.Text = text;
        }

        public static void UpdateRemainingLabel(string text)
        {
            if (Settings.mw.InvokeRequired)
            {
                UpdateRemainingLabelCallback d = new UpdateRemainingLabelCallback(UpdateRemainingLabel);
                Settings.mw.remainingLabel.Invoke(d, new object[] { text });
            }
            else
                Settings.mw.remainingLabel.Text = text;
        }

        public static void UpdateLoggableLabel(string text)
        {
            if (Settings.mw.InvokeRequired)
            {
                UpdateLoggableLabelCallback d = new UpdateLoggableLabelCallback(UpdateLoggableLabel);
                Settings.mw.loggableLabel.Invoke(d, new object[] { text });
            }
            else
                Settings.mw.loggableLabel.Text = text;
        }
        public static void UpdateBadLabel(string text)
        {
            if (Settings.mw.InvokeRequired)
            {
                UpdateBadLabelCallback d = new UpdateBadLabelCallback(UpdateBadLabel);
                Settings.mw.badLabel.Invoke(d, new object[] { text });
            }
            else
                Settings.mw.badLabel.Text = text;
        }

        public static void UpdateSGProjectedLabel(string text)
        {
            if (Settings.mw.InvokeRequired)
            {
                UpdateSGProtectedLabelCallback d = new UpdateSGProtectedLabelCallback(UpdateSGProjectedLabel);
                Settings.mw.steamGuardLabel.Invoke(d, new object[] { text });
            }
            else
                Settings.mw.steamGuardLabel.Text = text;
        }
        public static void UpdateCheckedLabel(string text)
        {
            if (Settings.mw.InvokeRequired)
            {
                UpdateCheckedLabelCallback d = new UpdateCheckedLabelCallback(UpdateCheckedLabel);
                Settings.mw.checkedLabel.Invoke(d, new object[] { text });
            }
            else
                Settings.mw.checkedLabel.Text = text;
        }
    }
    class LogHelper
    {
        delegate void SetListAccountsCallback(Result result, string userName, string password, string extendedResultMessage);
        delegate void SetLogCallback(string output);

        public enum Result
        {
            Success,
            SteamGuardProtected,
            Fail,
            AccountLocked,
        }

        public static void Log(string output)
        {
            if (Settings.mw.InvokeRequired)
            {
                SetLogCallback d = new SetLogCallback(Log);
                Settings.mw.labelStatus.Invoke(d, new object[] { output });
            }
            else
                Settings.mw.labelStatus.Text = output;
        }

        public static void ListAccountOnGUI(Result result, string userName, string password, string extendedResultMessage)
        {
            if (Settings.mw.InvokeRequired)
            {
                SetListAccountsCallback d = new SetListAccountsCallback(ListAccountOnGUI);
                Settings.mw.whatsHappening.Invoke(d, new object[] { result, userName, password, extendedResultMessage });
            }
            else
            {
                switch (result)
                {
                    case Result.AccountLocked:
                        ListViewItem alProtectedItem = new ListViewItem("🗲");
                        if (Settings.showColouredItemsInAccountList == true)
                            alProtectedItem.ForeColor = Color.Orange;
                        alProtectedItem.SubItems.Add(userName);
                        alProtectedItem.SubItems.Add(password);
                        alProtectedItem.SubItems.Add(extendedResultMessage);
                        Settings.mw.whatsHappening.Items.Add(alProtectedItem);
                        break;
                    case Result.Success:
                        ListViewItem successItem = new ListViewItem("✔");
                        if (Settings.showColouredItemsInAccountList == true)
                            successItem.ForeColor = Color.Green;
                        successItem.SubItems.Add(userName);
                        successItem.SubItems.Add(password);
                        successItem.SubItems.Add(extendedResultMessage);
                        Settings.mw.whatsHappening.Items.Add(successItem);
                        break;
                    case Result.SteamGuardProtected:
                        ListViewItem sgProtectedItem = new ListViewItem("🗲");
                        if (Settings.showColouredItemsInAccountList == true)
                            sgProtectedItem.ForeColor = Color.Orange;
                        sgProtectedItem.SubItems.Add(userName);
                        sgProtectedItem.SubItems.Add(password);
                        sgProtectedItem.SubItems.Add(extendedResultMessage);
                        Settings.mw.whatsHappening.Items.Add(sgProtectedItem);
                        break;
                    case Result.Fail:
                        ListViewItem failItem = new ListViewItem("✘");
                        if (Settings.showColouredItemsInAccountList == true)
                            failItem.ForeColor = Color.Red;
                        failItem.SubItems.Add(userName);
                        failItem.SubItems.Add(password);
                        failItem.SubItems.Add(extendedResultMessage);
                        Settings.mw.whatsHappening.Items.Add(failItem);
                        break;
                    default:
                        ListViewItem unknownItem = new ListViewItem("???");
                        if (Settings.showColouredItemsInAccountList == true)
                            unknownItem.ForeColor = Color.Red;
                        unknownItem.SubItems.Add(userName);
                        unknownItem.SubItems.Add(password);
                        unknownItem.SubItems.Add($"APPLICATION EXCEPTION. FIX ASAP! -- {extendedResultMessage}");
                        Settings.mw.whatsHappening.Items.Add(unknownItem);
                        break;
                }
            }
        }


        public static void LogClear()
        {
            if (!Settings.mw.InvokeRequired)
                Settings.mw.whatsHappening.Clear();
            else
                MessageBox.Show("jxhdjkxdhjk lmao Invoke required on control 'whatsHappening'", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    class AccountExporterHelper
    {
        public enum WhatToExport
        {
            GOODACCOUNTS,
            BADACCOUNTS,
            SGPROTECTEDACCOUNTS
        }

        public async static Task Export(WhatToExport whatToExport)
        {
            try
            {
                string filePath = string.Empty;

                switch (whatToExport)
                {
                    case WhatToExport.GOODACCOUNTS:
                        filePath = $"{SteamAccountHelper.localToExport}\\Good Accounts.txt";
                        string goodAccounts = string.Join("", SteamAccountHelper.GoodAccountsList.ToArray());
                        File.WriteAllText(filePath, goodAccounts);
                        break;

                    case WhatToExport.BADACCOUNTS:
                        filePath = $"{SteamAccountHelper.localToExport}\\Bad Accounts.txt";
                        string badAccounts = string.Join("", SteamAccountHelper.badAccountsList.ToArray());
                        File.WriteAllText(filePath, badAccounts);
                        break;

                    case WhatToExport.SGPROTECTEDACCOUNTS:
                        filePath = $"{SteamAccountHelper.localToExport}\\SteamGuard protected Accounts.txt";
                        string sGProtectedAccounts = string.Join("", SteamAccountHelper.sGProtectedAccountsList.ToArray());
                        File.WriteAllText(filePath, sGProtectedAccounts);
                        break;
                }
                MessageBox.Show("Done!", "SAS.exe", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Couldn't write. Error: {ex.Message}", "SAS.exe", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
