namespace Win32;

public enum WinMessage:uint {
    /*0000*/
    Null = 0x0000,
    /*0001*/
    Create = 0x0001,
    /*0002*/
    Destroy = 0x0002,
    /*0003*/
    Move = 0x0003,
    /// <summary>size has changed.<br/>
    /// <i>w</i><br/>
    /// kind of resizing<br/>
    /// <i>l</i><br/>
    /// lo-word = new width<br/>
    /// hi-word = new height</summary>
    /*0005*/
    Size = 0x0005,
    /// <summary>
    /// Sent to both the window being activated and the window being deactivated. <br />
    /// <i>wParam:</i><br />
    /// The low-order word, a <seealso cref="ActivateKind"/>, specifies whether the window is being activated or deactivated.<br />
    /// The window is minimized if the hi-order word is != 0
    /// </summary>
    /*0006*/
    Activate = 0x0006,
    /*0007*/
    SetFocus = 0x0007,
    /*0008*/
    KillFocus = 0x0008,
    /*000a*/
    Enable = 0x000a,
    /*000b*/
    SETREDRAW = 0x000b,
    /*000c*/
    SETTEXT = 0x000c,
    /*000d*/
    GETTEXT = 0x000d,
    /*000e*/
    GETTEXTLENGTH = 0x000e,
    /*000f*/
    Paint = 0x000f,
    /*0010*/
    Close = 0x0010,
    /*0011*/
    QUERYENDSESSION = 0x0011,
    /*0012*/
    Quit = 0x0012,
    /*0013*/
    QUERYOPEN = 0x0013,
    /*0014*/
    EraseBkgnd = 0x0014,
    /*0015*/
    SYSCOLORCHANGE = 0x0015,
    /*0016*/
    ENDSESSION = 0x0016,
    /*0018*/
    ShowWindow = 0x0018,
    /*001a*/
    WININICHANGE = 0x001a,
    /*001b*/
    DevModeChange = 0x001b,
    /// <summary><br/>
    /// <i>wParam:</i><br/>
    /// !=0 if the window is being activated
    /// </summary>
    /*001c*/
    ActivateApp = 0x001c,
    /*001d*/
    FontChange = 0x001d,
    /*001e*/
    TIMECHANGE = 0x001e,
    /*001f*/
    CANCELMODE = 0x001f,
    /// <summary>
    /// Mouse caused the cursor to move while mouse input is not captured.
    /// </summary>
    /*0020*/
    SETCURSOR = 0x0020,
    /*0021*/
    MouseActivate = 0x0021,
    /*0022*/
    ChildActivate = 0x0022,
    /*0023*/
    QUEUESYNC = 0x0023,
    /*0024*/
    GetMinMaxInfo = 0x0024,
    /*0026*/
    PAINTICON = 0x0026,
    /*0027*/
    ICONERASEBKGND = 0x0027,
    /*0028*/
    NEXTDLGCTL = 0x0028,
    /*002a*/
    SPOOLERSTATUS = 0x002a,
    /*002b*/
    DrawItem = 0x002b,
    /*002c*/
    MEASUREITEM = 0x002c,
    /*002d*/
    DeleteItem = 0x002d,
    /*002e*/
    VKEYTOITEM = 0x002e,
    /*002f*/
    CHARTOITEM = 0x002f,
    /*0030*/
    SETFONT = 0x0030,
    /*0031*/
    GETFONT = 0x0031,
    /*0032*/
    SETHOTKEY = 0x0032,
    /*0033*/
    GETHOTKEY = 0x0033,
    /*0037*/
    QUERYDRAGICON = 0x0037,
    /*0039*/
    COMPAREITEM = 0x0039,
    /*003d*/
    GETOBJECT = 0x003d,
    /*0041*/
    Compacting = 0x0041,
    /*0046*/
    WindowPosChanging = 0x0046,
    /*0047*/
    WindowPosChanged = 0x0047,
    /*0048*/
    Power = 0x0048,
    /*004a*/
    CopyData = 0x004a,
    /*004b*/
    CANCELJOURNAL = 0x004b,
    /*004e*/
    Notify = 0x004e,
    /*0050*/
    INPUTLANGCHANGEREQUEST = 0x0050,
    /*0051*/
    INPUTLANGCHANGE = 0x0051,
    /*0052*/
    TCARD = 0x0052,
    /*0053*/
    Help = 0x0053,
    /*0054*/
    USERCHANGED = 0x0054,
    /*0055*/
    NOTIFYFORMAT = 0x0055,
    /*007b*/
    ContextMenu = 0x007b,
    /*007c*/
    StyleChanging = 0x007c,
    /*007d*/
    StyleChanged = 0x007d,
    /*007e*/
    DisplayChange = 0x007e,
    /*007f*/
    GETICON = 0x007f,
    /*0080*/
    SETICON = 0x0080,
    /*0081*/
    NcCreate = 0x0081,
    /*0082*/
    NCDESTROY = 0x0082,
    /*0083*/
    NcCalcSize = 0x0083,
    /*0084*/
    NCHITTEST = 0x0084,
    /*0085*/
    NcPaint = 0x0085,
    /*0086*/
    NcActivate = 0x0086,
    /*0087*/
    GETDLGCODE = 0x0087,
    /*0088*/
    SYNCPAINT = 0x0088,
    /*00a0*/
    NCMOUSEMOVE = 0x00a0,
    /*00a1*/
    NCLBUTTONDOWN = 0x00a1,
    /*00a2*/
    NCLBUTTONUP = 0x00a2,
    /*00a3*/
    NCLBUTTONDBLCLK = 0x00a3,
    /*00a4*/
    NCRBUTTONDOWN = 0x00a4,
    /*00a5*/
    NCRBUTTONUP = 0x00a5,
    /*00a6*/
    NCRBUTTONDBLCLK = 0x00a6,
    /*00a7*/
    NCMBUTTONDOWN = 0x00a7,
    /*00a8*/
    NCMBUTTONUP = 0x00a8,
    /*00a9*/
    NCMBUTTONDBLCLK = 0x00a9,
    /*00ab*/
    NCXBUTTONDOWN = 0x00ab,
    /*00ac*/
    NCXBUTTONUP = 0x00ac,
    /*00ad*/
    NCXBUTTONDBLCLK = 0x00ad,
    /*00fe*/
    InputDeviceChange = 0x00fe,
    /*00ff*/
    Input = 0x00ff,
    /*0100*/
    KeyDown = 0x0100,
    /*0101*/
    KeyUp = 0x0101,
    /*0102*/
    Char = 0x0102,
    /*0103*/
    DeadChar = 0x0103,
    /*0104*/
    SysKeyDown = 0x0104,
    /*0105*/
    SysKeyUp = 0x0105,
    /*0106*/
    SYSCHAR = 0x0106,
    /*0107*/
    SYSDEADCHAR = 0x0107,
    /*0108*/
    KEYLAST = 0x0108,
    /*0109*/
    UNICHAR = 0x0109,
    /*010d*/
    IME_STARTCOMPOSITION = 0x010d,
    /*010e*/
    IME_ENDCOMPOSITION = 0x010e,
    /*010f*/
    IME_COMPOSITION = 0x010f,
    /*0110*/
    INITDIALOG = 0x0110,
    /*0111*/
    Command = 0x0111,
    /*0112*/
    SysCommand = 0x0112,
    /*0113*/
    Timer = 0x0113,
    /*0114*/
    HScroll = 0x0114,
    /*0115*/
    VScroll = 0x0115,
    /*0116*/
    INITMENU = 0x0116,
    /*0117*/
    INITMENUPOPUP = 0x0117,
    /*0119*/
    GESTURE = 0x0119,
    /*011a*/
    GESTURENOTIFY = 0x011a,
    /*011f*/
    MENUSELECT = 0x011f,
    /*0120*/
    MENUCHAR = 0x0120,
    /*0121*/
    EnterIdle = 0x0121,
    /*0122*/
    MENURBUTTONUP = 0x0122,
    /*0123*/
    MENUDRAG = 0x0123,
    /*0124*/
    MENUGETOBJECT = 0x0124,
    /*0125*/
    UNINITMENUPOPUP = 0x0125,
    /*0126*/
    MENUCOMMAND = 0x0126,
    /*0127*/
    CHANGEUISTATE = 0x0127,
    /*0128*/
    UPDATEUISTATE = 0x0128,
    /*0129*/
    QUERYUISTATE = 0x0129,
    /*0132*/
    CTLCOLORMSGBOX = 0x0132,
    /*0133*/
    CTLCOLOREDIT = 0x0133,
    /*0134*/
    CTLCOLORLISTBOX = 0x0134,
    /*0135*/
    CTLCOLORBTN = 0x0135,
    /*0136*/
    CTLCOLORDLG = 0x0136,
    /*0137*/
    CTLCOLORSCROLLBAR = 0x0137,
    /*0138*/
    CTLCOLORSTATIC = 0x0138,
    /*0200*/
    MouseMove = 0x0200,
    /*0201*/
    LButtonDown = 0x0201,
    /*0202*/
    LButtonUp = 0x0202,
    /*0203*/
    LButtonDblClk = 0x0203,
    /*0204*/
    RButtonDown = 0x0204,
    /*0205*/
    RButtonUp = 0x0205,
    /*0206*/
    RButtonDblClk = 0x0206,
    /*0207*/
    MButtonDown = 0x0207,
    /*0208*/
    MButtonUp = 0x0208,
    /*0209*/
    MButtonDblClk = 0x0209,
    /*020a*/
    MouseWheel = 0x020a,
    /*020b*/
    XButtonDown = 0x020b,
    /*020c*/
    XButtonUp = 0x020c,
    /*020d*/
    XButtonDlbClk = 0x020d,
    /*020e*/
    MouseHorizontalWheel = 0x020e,
    /*0210*/
    PARENTNOTIFY = 0x0210,
    /*0211*/
    EnterMenuLoop = 0x0211,
    /*0212*/
    ExitMenuLoop = 0x0212,
    /*0213*/
    NEXTMENU = 0x0213,
    /// <summary>
    /// Window is resizing.
    /// <br/>
    /// <i>w</i><br/>
    /// The edge of the window that is being moved<br/>
    /// <i>l</i><br/>
    /// Drag rectangle. An application may change the values to adjust the rectangle.
    /// </summary>
    /*0214*/
    Sizing = 0x0214,
    /*0215*/
    CaptureChanged = 0x0215,
    /*0216*/
    Moving = 0x0216,
    /*0218*/
    POWERBROADCAST = 0x0218,
    /*0219*/
    DeviceChange = 0x0219,
    /*0220*/
    MDICREATE = 0x0220,
    /*0221*/
    MDIDESTROY = 0x0221,
    /*0222*/
    MDIACTIVATE = 0x0222,
    /*0223*/
    MDIRESTORE = 0x0223,
    /*0224*/
    MDINEXT = 0x0224,
    /*0225*/
    MDIMAXIMIZE = 0x0225,
    /*0226*/
    MDITILE = 0x0226,
    /*0227*/
    MDICASCADE = 0x0227,
    /*0228*/
    MDIICONARRANGE = 0x0228,
    /*0229*/
    MDIGETACTIVE = 0x0229,
    /*0230*/
    MDISETMENU = 0x0230,
    /*0231*/
    EnterSizeMove = 0x0231,
    /*0232*/
    ExitSizeMove = 0x0232,
    /*0233*/
    DropFiles = 0x0233,
    /*0234*/
    MDIREFRESHMENU = 0x0234,
    /*0240*/
    TOUCH = 0x0240,
    /*0241*/
    NCPOINTERUPDATE = 0x0241,
    /*0242*/
    NCPOINTERDOWN = 0x0242,
    /*0243*/
    NCPOINTERUP = 0x0243,
    /*0245*/
    POINTERUPDATE = 0x0245,
    /*0246*/
    POINTERDOWN = 0x0246,
    /*0247*/
    POINTERUP = 0x0247,
    /*0249*/
    POINTERENTER = 0x0249,
    /*024a*/
    POINTERLEAVE = 0x024a,
    /*024b*/
    POINTERACTIVATE = 0x024b,
    /*024c*/
    POINTERCAPTURECHANGED = 0x024c,
    /*024d*/
    TOUCHHITTESTING = 0x024d,
    /*024e*/
    POINTERWHEEL = 0x024e,
    /*024f*/
    POINTERHWHEEL = 0x024f,
    /*0251*/
    POINTERROUTEDTO = 0x0251,
    /*0252*/
    POINTERROUTEDAWAY = 0x0252,
    /*0253*/
    POINTERROUTEDRELEASED = 0x0253,
    /*0281*/
    ImeSetContext = 0x0281,
    /*0282*/
    ImeNotify = 0x0282,
    /*0283*/
    ImeControl = 0x0283,
    /*0284*/
    ImeCompositionFull = 0x0284,
    /*0285*/
    ImeSelect = 0x0285,
    /*0286*/
    ImeChar = 0x0286,
    /*0288*/
    ImeRequest = 0x0288,
    /*0290*/
    ImeKeyDown = 0x0290,
    /*0291*/
    ImeKeyUp = 0x0291,
    /*02a0*/
    NCMOUSEHOVER = 0x02a0,
    /*02a1*/
    MouseHover = 0x02a1,
    /*02a2*/
    NCMOUSELEAVE = 0x02a2,
    /*02a3*/
    MouseLeave = 0x02a3,
    /*02b1*/
    WTSSESSION_CHANGE = 0x02b1,
    /*02e0*/
    DPICHANGED = 0x02e0,
    /*02e2*/
    DPICHANGED_BEFOREPARENT = 0x02e2,
    /*02e3*/
    DPICHANGED_AFTERPARENT = 0x02e3,
    /*02e4*/
    GETDPISCALEDSIZE = 0x02e4,
    /*0300*/
    Cut = 0x0300,
    /*0301*/
    Copy = 0x0301,
    /*0302*/
    PASTE = 0x0302,
    /*0303*/
    Clear = 0x0303,
    /*0304*/
    Undo = 0x0304,
    /*0305*/
    RENDERFORMAT = 0x0305,
    /*0306*/
    RENDERALLFORMATS = 0x0306,
    /*0307*/
    DestroyClipboard = 0x0307,
    /*0308*/
    DrawClipboard = 0x0308,
    /*0309*/
    PAINTCLIPBOARD = 0x0309,
    /*030a*/
    VSCROLLCLIPBOARD = 0x030a,
    /*030b*/
    SIZECLIPBOARD = 0x030b,
    /*030c*/
    ASKCBFORMATNAME = 0x030c,
    /*030d*/
    CHANGECBCHAIN = 0x030d,
    /*030e*/
    HSCROLLCLIPBOARD = 0x030e,
    /*030f*/
    QUERYNEWPALETTE = 0x030f,
    /*0310*/
    PALETTEISCHANGING = 0x0310,
    /*0311*/
    PALETTECHANGED = 0x0311,
    /*0312*/
    HOTKEY = 0x0312,
    /*0317*/
    PRINT = 0x0317,
    /*0318*/
    PRINTCLIENT = 0x0318,
    /*0319*/
    AppCommand = 0x0319,
    /*031a*/
    THEMECHANGED = 0x031a,
    /*031d*/
    ClipboardUpdate = 0x031d,
    /*031e*/
    DwmCompositionChanged = 0x031e,
    /*031f*/
    DwmNCRenderingChanged = 0x031f,
    /*0320*/
    DwmColorizationColorChanged = 0x0320,
    /*0321*/
    DwmWindowMaximizedChange = 0x0321,
    /*033f*/
    GETTITLEBARINFOEX = 0x033f,
    /*0358*/
    HANDHELDFIRST = 0x0358,
    /*035f*/
    HANDHELDLAST = 0x035f,
    /*0360*/
    AFXFIRST = 0x0360,
    /*037f*/
    AFXLAST = 0x037f,
    /*0380*/
    PENWINFIRST = 0x0380,
    /*038f*/
    PENWINLAST = 0x038f,
}
