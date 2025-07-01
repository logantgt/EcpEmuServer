using System.Runtime.InteropServices;
using AutoHotkey.Interop;

public class AutoHotKey
{
    public static void SendRawCommand(string scancode)
    {
        var ahk = AutoHotkeyEngine.Instance;

        ahk.ExecRaw($"SendInput {scancode}");
    }
}