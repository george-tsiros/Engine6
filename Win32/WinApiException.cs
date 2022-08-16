namespace Win32;

using System;

public class WinApiException:Exception {
 
    public uint LastError { get; } = Kernel32.GetLastError();

    public WinApiException (string message) : base(message) { }
    
    public override string ToString () =>
        $"win32 says {LastError} '{Message}'";
}
