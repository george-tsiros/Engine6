namespace Gl;

using System;

public class WinApiException:Exception {
    public ulong LastError { get; } = Kernel.GetLastError();
    public WinApiException (string message) : base(message) { }
}
