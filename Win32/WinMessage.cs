﻿namespace Win32;

public enum WinMessage:ushort {
    Activate = 0x0006,
    ActivateApp = 0x001C,
    AFXFIRST = 0x0360,
    AFXLAST = 0x037F,
    AppCommand = 0x0319,
    ASKCBFORMATNAME = 0x030C,
    CANCELJOURNAL = 0x004B,
    CANCELMODE = 0x001F,
    CaptureChanged = 0x0215,
    CHANGECBCHAIN = 0x030D,
    CHANGEUISTATE = 0x0127,
    Char = 0x0102,
    CHARTOITEM = 0x002F,
    ChildActivate = 0x0022,
    Clear = 0x0303,
    ClipboardUpdate = 0x031D,
    Close = 0x0010,
    Command = 0x0111,
    Compacting = 0x0041,
    COMPAREITEM = 0x0039,
    ContextMenu = 0x007B,
    Copy = 0x0301,
    CopyData = 0x004A,
    Create = 0x0001,
    CTLCOLORBTN = 0x0135,
    CTLCOLORDLG = 0x0136,
    CTLCOLOREDIT = 0x0133,
    CTLCOLORLISTBOX = 0x0134,
    CTLCOLORMSGBOX = 0x0132,
    CTLCOLORSCROLLBAR = 0x0137,
    CTLCOLORSTATIC = 0x0138,
    Cut = 0x0300,
    DeadChar = 0x0103,
    DeleteItem = 0x002D,
    Destroy = 0x0002,
    DestroyClipboard = 0x0307,
    DeviceChange = 0x0219,
    DevModeChange = 0x001B,
    DisplayChange = 0x007E,
    DPICHANGED = 0x02E0,
    DPICHANGED_AFTERPARENT = 0x02E3,
    DPICHANGED_BEFOREPARENT = 0x02E2,
    DrawClipboard = 0x0308,
    DrawItem = 0x002B,
    DropFiles = 0x0233,
    DwmColorizationColorChanged = 0x0320,
    DwmCompositionChanged = 0x031E,
    DwmNCRenderingChanged = 0x031F,
    DwmWindowMaximizedChange = 0x0321,
    Enable = 0x000A,
    ENDSESSION = 0x0016,
    EnterIdle = 0x0121,
    EnterMenuLoop = 0x0211,
    EnterSizeMove = 0x0231,
    EraseBkgnd = 0x0014,
    ExitMenuLoop = 0x0212,
    ExitSizeMove = 0x0232,
    FontChange = 0x001D,
    GESTURE = 0x0119,
    GESTURENOTIFY = 0x011A,
    GETDLGCODE = 0x0087,
    GETDPISCALEDSIZE = 0x02E4,
    GETFONT = 0x0031,
    GETHOTKEY = 0x0033,
    GETICON = 0x007F,
    GETMINMAXINFO = 0x0024,
    GETOBJECT = 0x003D,
    GETTEXT = 0x000D,
    GETTEXTLENGTH = 0x000E,
    GETTITLEBARINFOEX = 0x033F,
    HANDHELDFIRST = 0x0358,
    HANDHELDLAST = 0x035F,
    Help = 0x0053,
    HOTKEY = 0x0312,
    HScroll = 0x0114,
    HSCROLLCLIPBOARD = 0x030E,
    ICONERASEBKGND = 0x0027,
    IME_CHAR = 0x0286,
    IME_COMPOSITION = 0x010F,
    IME_COMPOSITIONFULL = 0x0284,
    IME_CONTROL = 0x0283,
    IME_ENDCOMPOSITION = 0x010E,
    IME_KEYDOWN = 0x0290,
    IME_KEYUP = 0x0291,
    IME_NOTIFY = 0x0282,
    IME_REQUEST = 0x0288,
    IME_SELECT = 0x0285,
    IME_SETCONTEXT = 0x0281,
    IME_STARTCOMPOSITION = 0x010D,
    INITDIALOG = 0x0110,
    INITMENU = 0x0116,
    INITMENUPOPUP = 0x0117,
    Input = 0x00FF,
    InputDeviceChange = 0x00FE,
    INPUTLANGCHANGE = 0x0051,
    INPUTLANGCHANGEREQUEST = 0x0050,
    KeyDown = 0x0100,
    KEYLAST = 0x0108,
    KeyUp = 0x0101,
    KillFocus = 0x0008,
    LButtonDblClk = 0x0203,
    LButtonDown = 0x0201,
    LButtonUp = 0x0202,
    MButtonDblClk = 0x0209,
    MButtonDown = 0x0207,
    MButtonUp = 0x0208,
    MDIACTIVATE = 0x0222,
    MDICASCADE = 0x0227,
    MDICREATE = 0x0220,
    MDIDESTROY = 0x0221,
    MDIGETACTIVE = 0x0229,
    MDIICONARRANGE = 0x0228,
    MDIMAXIMIZE = 0x0225,
    MDINEXT = 0x0224,
    MDIREFRESHMENU = 0x0234,
    MDIRESTORE = 0x0223,
    MDISETMENU = 0x0230,
    MDITILE = 0x0226,
    MEASUREITEM = 0x002C,
    MENUCHAR = 0x0120,
    MENUCOMMAND = 0x0126,
    MENUDRAG = 0x0123,
    MENUGETOBJECT = 0x0124,
    MENURBUTTONUP = 0x0122,
    MENUSELECT = 0x011F,
    MouseActivate = 0x0021,
    MouseHover = 0x02A1,
    MouseHorizontalWheel = 0x020E,
    MouseLeave = 0x02A3,
    MouseMove = 0x0200,
    MouseWheel = 0x020A,
    Move = 0x0003,
    Moving = 0x0216,
    NCACTIVATE = 0x0086,
    NCCALCSIZE = 0x0083,
    NCCREATE = 0x0081,
    NCDESTROY = 0x0082,
    NCHITTEST = 0x0084,
    NCLBUTTONDBLCLK = 0x00A3,
    NCLBUTTONDOWN = 0x00A1,
    NCLBUTTONUP = 0x00A2,
    NCMBUTTONDBLCLK = 0x00A9,
    NCMBUTTONDOWN = 0x00A7,
    NCMBUTTONUP = 0x00A8,
    NCMOUSEHOVER = 0x02A0,
    NCMOUSELEAVE = 0x02A2,
    NCMOUSEMOVE = 0x00A0,
    NCPAINT = 0x0085,
    NCPOINTERDOWN = 0x0242,
    NCPOINTERUP = 0x0243,
    NCPOINTERUPDATE = 0x0241,
    NCRBUTTONDBLCLK = 0x00A6,
    NCRBUTTONDOWN = 0x00A4,
    NCRBUTTONUP = 0x00A5,
    NCXBUTTONDBLCLK = 0x00AD,
    NCXBUTTONDOWN = 0x00AB,
    NCXBUTTONUP = 0x00AC,
    NEXTDLGCTL = 0x0028,
    NEXTMENU = 0x0213,
    Notify = 0x004E,
    NOTIFYFORMAT = 0x0055,
    Null = 0x0000,
    Paint = 0x000F,
    PAINTCLIPBOARD = 0x0309,
    PAINTICON = 0x0026,
    PALETTECHANGED = 0x0311,
    PALETTEISCHANGING = 0x0310,
    PARENTNOTIFY = 0x0210,
    PASTE = 0x0302,
    PENWINFIRST = 0x0380,
    PENWINLAST = 0x038F,
    POINTERACTIVATE = 0x024B,
    POINTERCAPTURECHANGED = 0x024C,
    POINTERDOWN = 0x0246,
    POINTERENTER = 0x0249,
    POINTERHWHEEL = 0x024F,
    POINTERLEAVE = 0x024A,
    POINTERROUTEDAWAY = 0x0252,
    POINTERROUTEDRELEASED = 0x0253,
    POINTERROUTEDTO = 0x0251,
    POINTERUP = 0x0247,
    POINTERUPDATE = 0x0245,
    POINTERWHEEL = 0x024E,
    Power = 0x0048,
    POWERBROADCAST = 0x0218,
    PRINT = 0x0317,
    PRINTCLIENT = 0x0318,
    QUERYDRAGICON = 0x0037,
    QUERYENDSESSION = 0x0011,
    QUERYNEWPALETTE = 0x030F,
    QUERYOPEN = 0x0013,
    QUERYUISTATE = 0x0129,
    QUEUESYNC = 0x0023,
    Quit = 0x0012,
    RButtonDblClk = 0x0206,
    RButtonDown = 0x0204,
    RButtonUp = 0x0205,
    RENDERALLFORMATS = 0x0306,
    RENDERFORMAT = 0x0305,
    SETCURSOR = 0x0020,
    SetFocus = 0x0007,
    SETFONT = 0x0030,
    SETHOTKEY = 0x0032,
    SETICON = 0x0080,
    SETREDRAW = 0x000B,
    SETTEXT = 0x000C,
    ShowWindow = 0x0018,
    Size = 0x0005,
    SIZECLIPBOARD = 0x030B,
    Sizing = 0x0214,
    SPOOLERSTATUS = 0x002A,
    STYLECHANGED = 0x007D,
    STYLECHANGING = 0x007C,
    SYNCPAINT = 0x0088,
    SYSCHAR = 0x0106,
    SYSCOLORCHANGE = 0x0015,
    SysCommand = 0x0112,
    SYSDEADCHAR = 0x0107,
    SysKeyDown = 0x0104,
    SysKeyUp = 0x0105,
    TCARD = 0x0052,
    THEMECHANGED = 0x031A,
    TIMECHANGE = 0x001E,
    TIMER = 0x0113,
    TOUCH = 0x0240,
    TOUCHHITTESTING = 0x024D,
    Undo = 0x0304,
    UNICHAR = 0x0109,
    UNINITMENUPOPUP = 0x0125,
    UPDATEUISTATE = 0x0128,
    USERCHANGED = 0x0054,
    VKEYTOITEM = 0x002E,
    VScroll = 0x0115,
    VSCROLLCLIPBOARD = 0x030A,
    WindowPosChanged = 0x0047,
    WindowPosChanging = 0x0046,
    WININICHANGE = 0x001A,
    WTSSESSION_CHANGE = 0x02B1,
    XButtonDlbClk = 0x020D,
    XButtonDown = 0x020B,
    XButtonUp = 0x020C,
}