namespace Win32;

using System;

public class WinApiException:Exception {

    private const string ToStringFormat = "win32 says {0} '{1}' ({2})";

    public uint LastError { get; }

    public WinApiException (string functionName, uint? lastError = null) : base(functionName) {
        LastError = lastError ?? Kernel32.GetLastError();
    }

    public override string ToString () =>
        string.Format(ToStringFormat, LastError, Message, (Win32ErrorCode)LastError);
}
