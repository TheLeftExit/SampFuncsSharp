using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

public unsafe struct SampFuncs
{
    private static SampFuncs _instance;

    [UnmanagedCallersOnly(EntryPoint = "RegisterExports", CallConvs = [typeof(CallConvStdcall)])]
    public static void RegisterExports(SampFuncs* funcs)
    {
        _instance = *funcs;
    }

    public delegate* unmanaged[Stdcall]<byte*, void> _sendToChat;
    public delegate* unmanaged[Stdcall]<byte*, void> _logToChat;
    public delegate* unmanaged[Stdcall]<int, bool> _isPlayerConnected;
    public delegate* unmanaged[Stdcall]<ushort> _getAimedPlayerId;
    public delegate* unmanaged[Stdcall]<int, byte*> _getPlayerName;
    public delegate* unmanaged[Stdcall]<ushort, int, byte*, byte*, byte*, byte*, void> _showDialog;
    public delegate* unmanaged[Stdcall]<delegate* unmanaged[Stdcall]<int, int, int, byte*, void>, void> _registerDialogCallback;

    public static void SendToChat(ReadOnlySpan<char> message)
    {
        Span<byte> bytes = stackalloc byte[Encoding.UTF8.GetByteCount(message) + 1];
        bytes.Clear();
        Encoding.UTF8.GetBytes(message, bytes);
        fixed (byte* pBytes = bytes) _instance._sendToChat(pBytes);
    }

    public static void LogToChat(ReadOnlySpan<char> message)
    {
        Span<byte> bytes = stackalloc byte[Encoding.UTF8.GetByteCount(message) + 1];
        bytes.Clear();
        Encoding.UTF8.GetBytes(message, bytes);
        fixed (byte* pBytes = bytes) _instance._logToChat(pBytes);
    }

    public static bool IsPlayerConnected(int playerId)
    {
        return _instance._isPlayerConnected(playerId);
    }

    public static int GetAimedPlayerId()
    {
        return _instance._getAimedPlayerId();
    }

    public static string GetPlayerName(int playerId)
    {
        byte* namePtr = _instance._getPlayerName(playerId);
        return Marshal.PtrToStringUTF8((nint)namePtr) ?? string.Empty;
    }

    public static void ShowDialog(ushort dialogId, int playerId, ReadOnlySpan<char> title, ReadOnlySpan<char> info, ReadOnlySpan<char> button1, ReadOnlySpan<char> button2)
    {
        Span<byte> titleBytes = stackalloc byte[Encoding.UTF8.GetByteCount(title) + 1];
        Span<byte> infoBytes = stackalloc byte[Encoding.UTF8.GetByteCount(info) + 1];
        Span<byte> button1Bytes = stackalloc byte[Encoding.UTF8.GetByteCount(button1) + 1];
        Span<byte> button2Bytes = stackalloc byte[Encoding.UTF8.GetByteCount(button2) + 1];
        titleBytes.Clear();
        infoBytes.Clear();
        button1Bytes.Clear();
        button2Bytes.Clear();
        Encoding.UTF8.GetBytes(title, titleBytes);
        Encoding.UTF8.GetBytes(info, infoBytes);
        Encoding.UTF8.GetBytes(button1, button1Bytes);
        Encoding.UTF8.GetBytes(button2, button2Bytes);
        fixed (byte* pTitle = titleBytes)
        fixed (byte* pInfo = infoBytes)
        fixed (byte* pButton1 = button1Bytes)
        fixed (byte* pButton2 = button2Bytes)
        {
            _instance._showDialog(dialogId, playerId, pTitle, pInfo, pButton1, pButton2);
        }
    }

    public static void RegisterDialogCallback(delegate* unmanaged[Stdcall]<int, int, int, byte*, void> callback)
    {
        _instance._registerDialogCallback(callback);
    }
}