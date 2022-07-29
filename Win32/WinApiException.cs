namespace Win32;

using System;

public class WinApiException:Exception {
    public ulong LastError { get; } = Kernel.GetLastError();
    public WinApiException (string message = null) : base(message ?? "unspecified") { }
}
