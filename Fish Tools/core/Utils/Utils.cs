/*
------------------------------------------------------------
Made By notfishvr
github: https://github.com/official-notfishvr/Fish-Tools
------------------------------------------------------------
*/
using Fish_Tools.core.FileManagementTools;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Fish_Tools.core.Utils
{
    internal class Utils
    {
        public static void ExecuteCommand(string command, Logger logger)
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = "/C " + command,
                        WindowStyle = WindowsCleaner._showOperationWindows ? ProcessWindowStyle.Normal : ProcessWindowStyle.Hidden,
                        UseShellExecute = true,
                        Verb = "runas"
                    }
                };
                process.Start();
                process.WaitForExit();
            }
            catch (Exception ex)
            {
                logger.Error($"Command '{command}' failed. Error: {ex.Message}");
            }
        }
        public static async Task RunCommand(string arguments, string workingDirectory, Logger logger)
        {
            await Task.Run(() =>
            {
                try
                {
                    var process = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "cmd.exe",
                            Arguments = arguments,
                            WorkingDirectory = workingDirectory,
                            WindowStyle = WindowsCleaner._showOperationWindows ? ProcessWindowStyle.Normal : ProcessWindowStyle.Hidden,
                            UseShellExecute = true,
                            Verb = "runas"
                        }
                    };
                    process.Start();
                    process.WaitForExit();
                }
                catch (Exception ex)
                {
                    logger.Error($"Command '{arguments}' failed. Error: {ex.Message}");
                }
            });
        }
        public static void RunCommand(string command)
        {
            using (Process process = new Process())
            {
                process.StartInfo.FileName = "cmd.exe";
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardInput = true;
                process.StartInfo.RedirectStandardOutput = true;
                try
                {
                    process.Start();
                    process.StandardInput.WriteLine(command);
                    process.StandardInput.Close();
                    process.WaitForExit();
                }
                catch (Exception)
                {
                }
            }
        }
        public static async Task KillProcessAndDeleteDirectories(string processName, string[] directories, Logger logger)
        {
            await Task.Run(() =>
            {
                ExecuteCommand($"/C TASKKILL /F /IM {processName}", logger);
                foreach (var directory in directories)
                {
                    if (Directory.Exists(directory))
                    {
                        try
                        {
                            Directory.Delete(directory, true);
                            logger.Success($"Deleted {directory}");
                        }
                        catch (Exception ex)
                        {
                            logger.Error($"Failed to delete {directory}. Error: {ex.Message}");
                        }
                    }
                }
            });
        }
        public static void Wait(int miliseconds) { Task.Run(async () => await Task.Delay(miliseconds)).Wait(); }
        public static void NewThread(Action action) { Task.Run(() => action.Invoke()); }
        public class Constants
        {
            public const int NOMOD = 0x0000;
            public const int ALT = 0x0001;
            public const int CTRL = 0x0002;
            public const int SHIFT = 0x0004;
            public const int WIN = 0x0008;

            public const int WM_HOTKEY_MSG_ID = 0x0312;
        }
        public class KeyHandler
        {
            [DllImport("user32.dll")]
            private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);

            [DllImport("user32.dll")]
            private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

            private int modifier;
            private int key;
            private IntPtr hWnd;
            private int id;

            public KeyHandler(int modifier, Keys key, Form form)
            {
                this.modifier = modifier;
                this.key = (int)key;
                this.hWnd = form.Handle;
                id = this.GetHashCode();
            }

            public override int GetHashCode()
            {
                return modifier ^ key ^ hWnd.ToInt32();
            }

            public bool Register()
            {
                return RegisterHotKey(hWnd, id, modifier, key);
            }

            public bool Unregiser()
            {
                return UnregisterHotKey(hWnd, id);
            }
        }
    }
}
