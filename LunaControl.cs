using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

internal static class LunaControl
{
    [DllImport("user32.dll")] private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);
    [DllImport("user32.dll")] private static extern int GetWindowTextLength(IntPtr hWnd);
    [DllImport("user32.dll")] private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
    [DllImport("user32.dll")] private static extern bool IsWindowVisible(IntPtr hWnd);
    [DllImport("user32.dll")] private static extern bool SetForegroundWindow(IntPtr hWnd);
    [DllImport("user32.dll")] private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    [DllImport("user32.dll")] private static extern void keybd_event(byte bVk, byte bScan, int dwFlags, IntPtr dwExtraInfo);
    [DllImport("user32.dll")] private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
    [DllImport("user32.dll")] private static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, IntPtr dwExtraInfo);
    [DllImport("user32.dll")] private static extern bool SetCursorPos(int x, int y);
    [DllImport("user32.dll")] private static extern int SendInput(int nInputs, INPUT[] pInputs, int cbSize);

    private const int SW_RESTORE = 9;
    private const int KEYEVENTF_KEYUP = 0x0002;
    private const int KEYEVENTF_UNICODE = 0x0004;
    private const int INPUT_KEYBOARD = 1;
    private const int MOUSEEVENTF_LEFTDOWN = 0x0002;
    private const int MOUSEEVENTF_LEFTUP = 0x0004;

    private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    [StructLayout(LayoutKind.Sequential)]
    private struct KEYBDINPUT { public ushort wVk; public ushort wScan; public int dwFlags; public int time; public IntPtr dwExtraInfo; }
    [StructLayout(LayoutKind.Sequential)]
    private struct MOUSEINPUT { public int dx; public int dy; public uint mouseData; public uint dwFlags; public uint time; public IntPtr dwExtraInfo; }
    [StructLayout(LayoutKind.Explicit)]
    private struct InputUnion { [FieldOffset(0)] public KEYBDINPUT ki; [FieldOffset(0)] public MOUSEINPUT mi; }
    [StructLayout(LayoutKind.Sequential)]
    private struct INPUT { public int type; public InputUnion U; }

    private static readonly Dictionary<string, ushort> KeyMap = new Dictionary<string, ushort>(StringComparer.OrdinalIgnoreCase)
    {
        {"ENTER",0x0D},{"RETURN",0x0D},{"TAB",0x09},{"ESC",0x1B},{"ESCAPE",0x1B},
        {"BACKSPACE",0x08},{"SPACE",0x20},{"HOME",0x24},{"END",0x23},{"PGUP",0x21},{"PGDOWN",0x22},
        {"UP",0x26},{"DOWN",0x28},{"LEFT",0x25},{"RIGHT",0x27},{"DELETE",0x2E},{"INSERT",0x2D},
        {"F1",0x70},{"F2",0x71},{"F3",0x72},{"F4",0x73},{"F5",0x74},{"F6",0x75},{"F7",0x76},
        {"F8",0x77},{"F9",0x78},{"F10",0x79},{"F11",0x7A},{"F12",0x7B},
    };
    private static readonly Dictionary<string, ushort> Modifiers = new Dictionary<string, ushort>(StringComparer.OrdinalIgnoreCase)
    {
        {"SHIFT",0x10},{"CTRL",0x11},{"CONTROL",0x11},{"ALT",0x12},{"WIN",0x5B},{"WINDOWS",0x5B},{"SUPER",0x5B},
    };

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT { public int X; public int Y; }

    private static int Main(string[] args)
    {
        if (args.Length < 1) { PrintUsage(); return 1; }
        var cmd = args[0].ToLowerInvariant();
        switch (cmd)
        {
            case "list": return ListWindows();
            case "focus": return FocusWindow(JoinArgs(args, 1));
            case "type": return TypeText(JoinArgs(args, 1));
            case "keys": return PressKeys(JoinArgs(args, 1));
            case "click": return Click(args);
            case "awake": return 0;
            default: Console.WriteLine("ERROR unknown command " + cmd); PrintUsage(); return 1;
        }
    }

    private static string JoinArgs(string[] args, int start)
    {
        if (start >= args.Length) return "";
        return string.Join(" ", args, start, args.Length - start);
    }

    private static int ListWindows()
    {
        var list = new List<string>();
        EnumWindows((hWnd, lParam) =>
        {
            if (!IsWindowVisible(hWnd)) return true;
            int len = GetWindowTextLength(hWnd);
            if (len <= 0) return true;
            var sb = new StringBuilder(len + 1);
            GetWindowText(hWnd, sb, sb.Capacity);
            string title = sb.ToString().Trim();
            if (string.IsNullOrWhiteSpace(title)) return true;
            uint pid;
            GetWindowThreadProcessId(hWnd, out pid);
            list.Add(pid + "|" + title);
            return true;
        }, IntPtr.Zero);
        foreach (var line in list) Console.WriteLine(line);
        return 0;
    }

    private static int FocusWindow(string query)
    {
        query = (query ?? "").Trim().ToLowerInvariant();
        if (query.Length == 0) { Console.WriteLine("ERROR usage: focus <window title substring>"); return 1; }
        IntPtr best = IntPtr.Zero;
        int bestLen = int.MaxValue;
        EnumWindows((hWnd, lParam) =>
        {
            if (!IsWindowVisible(hWnd)) return true;
            int len = GetWindowTextLength(hWnd);
            if (len <= 0) return true;
            var sb = new StringBuilder(len + 1);
            GetWindowText(hWnd, sb, sb.Capacity);
            string title = sb.ToString().Trim().ToLowerInvariant();
            int idx = title.IndexOf(query, StringComparison.Ordinal);
            if (idx >= 0 && title.Length < bestLen) { best = hWnd; bestLen = title.Length; }
            return true;
        }, IntPtr.Zero);
        if (best == IntPtr.Zero) { Console.WriteLine("NOT_FOUND no window matching '" + query + "'"); return 1; }
        ShowWindow(best, SW_RESTORE);
        const byte VK_ALT = 0x12;
        keybd_event(VK_ALT, 0, 0, IntPtr.Zero);
        keybd_event(VK_ALT, 0, KEYEVENTF_KEYUP, IntPtr.Zero);
        bool ok = SetForegroundWindow(best);
        Console.WriteLine(ok ? "FOCUSED" : "FOCUS_FAILED");
        return ok ? 0 : 1;
    }

    private static int TypeText(string text)
    {
        if (string.IsNullOrEmpty(text)) { Console.WriteLine("ERROR usage: type <text>"); return 1; }
        foreach (var ch in text)
        {
            var inputs = new INPUT[2];
            inputs[0].type = INPUT_KEYBOARD; inputs[0].U.ki.wScan = (ushort)ch; inputs[0].U.ki.dwFlags = KEYEVENTF_UNICODE;
            inputs[1].type = INPUT_KEYBOARD; inputs[1].U.ki.wScan = (ushort)ch; inputs[1].U.ki.dwFlags = KEYEVENTF_UNICODE | KEYEVENTF_KEYUP;
            SendInput(inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
        }
        Console.WriteLine("TYPED " + text.Length + " chars");
        return 0;
    }

    private static int PressKeys(string combo)
    {
        combo = (combo ?? "").Trim();
        if (combo.Length == 0) { Console.WriteLine("ERROR usage: keys <Combo+Like+Ctrl+K>"); return 1; }
        var tokens = combo.Split(new[] {'+'}, StringSplitOptions.RemoveEmptyEntries);
        var pressed = new List<ushort>();
        var sequence = new List<Tuple<ushort, bool>>();
        foreach (var raw in tokens)
        {
            string tok = raw.Trim();
            ushort mod;
            if (Modifiers.TryGetValue(tok, out mod)) { sequence.Add(Tuple.Create(mod, true)); pressed.Add(mod); continue; }
            ushort vk;
            if (KeyMap.TryGetValue(tok, out vk)) { sequence.Add(Tuple.Create(vk, true)); pressed.Add(vk); continue; }
            if (tok.Length == 1)
            {
                char c = char.ToUpperInvariant(tok[0]);
                if (c >= 'A' && c <= 'Z') { ushort letter = (ushort)(c - 'A' + 0x41); sequence.Add(Tuple.Create(letter, true)); pressed.Add(letter); continue; }
                if (c >= '0' && c <= '9') { ushort digit = (ushort)(c - '0' + 0x30); sequence.Add(Tuple.Create(digit, true)); pressed.Add(digit); continue; }
            }
            Console.WriteLine("ERROR unknown key '" + tok + "'");
            foreach (var released in pressed) keybd_event((byte)released, 0, KEYEVENTF_KEYUP, IntPtr.Zero);
            return 1;
        }
        foreach (var item in sequence) keybd_event((byte)item.Item1, 0, 0, IntPtr.Zero);
        foreach (var item in sequence) keybd_event((byte)item.Item1, 0, KEYEVENTF_KEYUP, IntPtr.Zero);
        Console.WriteLine("KEYS_SENT " + combo);
        return 0;
    }

    private static int Click(string[] args)
    {
        if (args.Length < 3) { Console.WriteLine("ERROR usage: click <x> <y>"); return 1; }
        int x, y;
        if (!int.TryParse(args[1], out x) || !int.TryParse(args[2], out y)) { Console.WriteLine("ERROR click needs int coords"); return 1; }
        SetCursorPos(x, y);
        mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, IntPtr.Zero);
        mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, IntPtr.Zero);
        Console.WriteLine("CLICKED " + x + " " + y);
        return 0;
    }

    private static void PrintUsage()
    {
        Console.WriteLine("luna-control commands:");
        Console.WriteLine("  list                     - list visible windows as 'pid|title'");
        Console.WriteLine("  focus <title>            - bring a window matching substring to foreground");
        Console.WriteLine("  type <text>              - type text into the focused window");
        Console.WriteLine("  keys <Combo+Like+Ctrl+K> - press combo like 'Ctrl+C', 'Enter', 'Win+D'");
        Console.WriteLine("  click <x> <y>            - left-click screen coords");
    }
}
