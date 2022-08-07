namespace Win32;
using System;

public enum CmdShow { 
Hide = 0,
ShowNormal = 1,
ShowMinimized,
ShowMaximized,
ShowNoActivate,
Show,
Minimize,

}

[Flags]
public enum Buttons:ushort {
    None = 0,
    Left = 0x1,
    Right = 0x2,
    Middle = 0x10,
    ButtonX1 = 0x20,
    ButtonX2 = 0x40,
}
