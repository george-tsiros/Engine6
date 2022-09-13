namespace Win32;

using System;

public class WinApiException:Exception {

    public uint LastError { get; }

    public WinApiException (string message, uint? lastError = null) : base(message) {
        LastError = lastError ?? Kernel32.GetLastError();
    }
    //const 
    public override string ToString () =>
        $"win32 says {LastError} '{Message}' ({(Win32ErrorCode)LastError})";
}
