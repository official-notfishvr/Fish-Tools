using System.Runtime.InteropServices;

namespace FishTools.App;

internal sealed class AutoClicker : ITool
{
    public string Id => "auto-clicker";
    public string Name => "Auto Clicker";
    public string Category => ToolCategories.Automation;
    public string Description => "Automate mouse clicks or key presses at a set interval.";

    public async Task RunAsync(ToolContext context)
    {
        ConsoleUi.ResetScreen(Name);
        var action = ConsoleUi.ShowMenu("Choose automation", ["Left Click", "Right Click", "Keyboard Key Press", "Back"]);
        if (action == 3)
            return;

        var interval = ConsoleUi.PromptInt("Interval (ms)", 1000, 1, 60000);
        var jitter = ConsoleUi.PromptInt("Jitter Randomness (ms)", 0, 0, 10000);
        var repeatCount = ConsoleUi.PromptInt("Iterations", 25, 1, 100000);
        string? keyChar = null;

        if (action == 2)
            keyChar = ConsoleUi.PromptRequired("Character to press")[0].ToString();

        ConsoleUi.ResetScreen(Name);
        ConsoleUi.Info("Script starts in 3s. Focus the target window!");
        await Task.Delay(3000);

        var random = new Random();
        for (var i = 1; i <= repeatCount; i++)
        {
            ConsoleUi.ResetScreen(Name);
            ConsoleUi.Info($"Iteration: {i} / {repeatCount}");

            switch (action)
            {
                case 0:
                    SendMouse(MouseEventFlags.LeftDown | MouseEventFlags.LeftUp);
                    break;
                case 1:
                    SendMouse(MouseEventFlags.RightDown | MouseEventFlags.RightUp);
                    break;
                case 2:
                    SendKey(keyChar![0]);
                    break;
            }

            var delay = interval + (jitter == 0 ? 0 : random.Next(0, jitter + 1));
            await Task.Delay(delay);
        }

        ConsoleUi.Success("Automation done.");
        ConsoleUi.Pause();
    }

    [Flags]
    private enum MouseEventFlags : uint
    {
        LeftDown = 0x02,
        LeftUp = 0x04,
        RightDown = 0x08,
        RightUp = 0x10,
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct INPUT
    {
        public uint type;
        public InputUnion U;
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct InputUnion
    {
        [FieldOffset(0)]
        public MOUSEINPUT mi;

        [FieldOffset(0)]
        public KEYBDINPUT ki;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MOUSEINPUT
    {
        public int dx,
            dy;
        public uint mouseData,
            dwFlags,
            time;
        public nint dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct KEYBDINPUT
    {
        public ushort wVk,
            wScan;
        public uint dwFlags,
            time;
        public nint dwExtraInfo;
    }

    [DllImport("user32.dll")]
    private static extern uint SendInput(uint n, INPUT[] p, int s);

    [DllImport("user32.dll")]
    private static extern short VkKeyScan(char ch);

    private static void SendMouse(MouseEventFlags f) =>
        SendInput(
            1,
            [
                new INPUT
                {
                    type = 0,
                    U = new InputUnion { mi = new MOUSEINPUT { dwFlags = (uint)f } },
                },
            ],
            Marshal.SizeOf<INPUT>()
        );

    private static void SendKey(char c)
    {
        var vk = VkKeyScan(c);
        if (vk == -1)
            throw new Exception($"Invalid key: {c}");
        var k = (ushort)(vk & 0xff);
        var s = (vk >> 8) & 0xff;
        var inputs = new List<INPUT>();
        if ((s & 1) != 0)
            inputs.Add(KeyInput(0x10, false));
        inputs.Add(KeyInput(k, false));
        inputs.Add(KeyInput(k, true));
        if ((s & 1) != 0)
            inputs.Add(KeyInput(0x10, true));
        SendInput((uint)inputs.Count, inputs.ToArray(), Marshal.SizeOf<INPUT>());
    }

    private static INPUT KeyInput(ushort k, bool up) =>
        new()
        {
            type = 1,
            U = new InputUnion
            {
                ki = new KEYBDINPUT { wVk = k, dwFlags = up ? 0x0002u : 0u },
            },
        };
}
