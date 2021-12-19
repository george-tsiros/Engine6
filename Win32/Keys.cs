namespace Win32;

public enum Keys:byte {
    None = 0x00,
    LButton = 0x01,
    RButton = 0x02,
    Cancel = 0x03,
    MButton = 0x04,
    XButton1 = 0x05,
    XButton2 = 0x06,
    X_07 = 0x07,
    Back = 0x08,
    Tab = 0x09,
    LineFeed = 0x0A,
    X_0B = 0x0B,
    Clear = 0x0C,
    Return = 0x0D,
    X_0E = 0x0E,
    X_0F = 0x0F,
    ShiftKey = 0x10,
    ControlKey = 0x11,
    Menu = 0x12,
    Pause = 0x13,
    CapsLock = 0x14,
    KanaMode = 0x15,
    X_16 = 0x16,
    JunjaMode = 0x17,
    FinalMode = 0x18,
    KanjiMode = 0x19,
    X_1A = 0x1A,
    Escape = 0x1B,
    IMEConvert = 0x1C,
    IMENonconvert = 0x1D,
    IMEAccept = 0x1E,
    IMEModeChange = 0x1F,
    Space = 0x20,
    PageUp = 0x21,
    PageDown = 0x22,
    End = 0x23,
    Home = 0x24,
    Left = 0x25,
    Up = 0x26,
    Right = 0x27,
    Down = 0x28,
    Select = 0x29,
    Print = 0x2A,
    Execute = 0x2B,
    PrintScreen = 0x2C,
    Insert = 0x2D,
    Delete = 0x2E,
    Help = 0x2F,
    D0 = 0x30,
    D1 = 0x31,
    D2 = 0x32,
    D3 = 0x33,
    D4 = 0x34,
    D5 = 0x35,
    D6 = 0x36,
    D7 = 0x37,
    D8 = 0x38,
    D9 = 0x39,
    X_3A = 0x3A,
    X_3B = 0x3B,
    X_3C = 0x3C,
    X_3D = 0x3D,
    X_3E = 0x3E,
    X_3F = 0x3F,
    X_40 = 0x40,
    A = 0x41,
    B = 0x42,
    C = 0x43,
    D = 0x44,
    E = 0x45,
    F = 0x46,
    G = 0x47,
    H = 0x48,
    I = 0x49,
    J = 0x4A,
    K = 0x4B,
    L = 0x4C,
    M = 0x4D,
    N = 0x4E,
    O = 0x4F,
    P = 0x50,
    Q = 0x51,
    R = 0x52,
    S = 0x53,
    T = 0x54,
    U = 0x55,
    V = 0x56,
    W = 0x57,
    X = 0x58,
    Y = 0x59,
    Z = 0x5A,
    LWin = 0x5B,
    RWin = 0x5C,
    Apps = 0x5D,
    X_5E = 0x5E,
    Sleep = 0x5F,
    NumPad0 = 0x60,
    NumPad1 = 0x61,
    NumPad2 = 0x62,
    NumPad3 = 0x63,
    NumPad4 = 0x64,
    NumPad5 = 0x65,
    NumPad6 = 0x66,
    NumPad7 = 0x67,
    NumPad8 = 0x68,
    NumPad9 = 0x69,
    Multiply = 0x6A,
    Add = 0x6B,
    Separator = 0x6C,
    Subtract = 0x6D,
    Decimal = 0x6E,
    Divide = 0x6F,
    F1 = 0x70,
    F2 = 0x71,
    F3 = 0x72,
    F4 = 0x73,
    F5 = 0x74,
    F6 = 0x75,
    F7 = 0x76,
    F8 = 0x77,
    F9 = 0x78,
    F10 = 0x79,
    F11 = 0x7A,
    F12 = 0x7B,
    F13 = 0x7C,
    F14 = 0x7D,
    F15 = 0x7E,
    F16 = 0x7F,
    F17 = 0x80,
    F18 = 0x81,
    F19 = 0x82,
    F20 = 0x83,
    F21 = 0x84,
    F22 = 0x85,
    F23 = 0x86,
    F24 = 0x87,
    X_88 = 0x88,
    X_89 = 0x89,
    X_8A = 0x8A,
    X_8B = 0x8B,
    X_8C = 0x8C,
    X_8D = 0x8D,
    X_8E = 0x8E,
    X_8F = 0x8F,
    NumLock = 0x90,
    Scroll = 0x91,
    X_92 = 0x92,
    X_93 = 0x93,
    X_94 = 0x94,
    X_95 = 0x95,
    X_96 = 0x96,
    X_97 = 0x97,
    X_98 = 0x98,
    X_99 = 0x99,
    X_9A = 0x9A,
    X_9B = 0x9B,
    X_9C = 0x9C,
    X_9D = 0x9D,
    X_9E = 0x9E,
    X_9F = 0x9F,
    LShiftKey = 0xA0,
    RShiftKey = 0xA1,
    LControlKey = 0xA2,
    RControlKey = 0xA3,
    LMenu = 0xA4,
    RMenu = 0xA5,
    BrowserBack = 0xA6,
    BrowserForward = 0xA7,
    BrowserRefresh = 0xA8,
    BrowserStop = 0xA9,
    BrowserSearch = 0xAA,
    BrowserFavorites = 0xAB,
    BrowserHome = 0xAC,
    VolumeMute = 0xAD,
    VolumeDown = 0xAE,
    VolumeUp = 0xAF,
    MediaNextTrack = 0xB0,
    MediaPreviousTrack = 0xB1,
    MediaStop = 0xB2,
    MediaPlayPause = 0xB3,
    LaunchMail = 0xB4,
    SelectMedia = 0xB5,
    LaunchApplication1 = 0xB6,
    LaunchApplication2 = 0xB7,
    X_B8 = 0xB8,
    X_B9 = 0xB9,
    OemSemicolon = 0xBA,
    Oemplus = 0xBB,
    Oemcomma = 0xBC,
    OemMinus = 0xBD,
    OemPeriod = 0xBE,
    OemQuestion = 0xBF,
    Oemtilde = 0xC0,
    X_C1 = 0xC1,
    X_C2 = 0xC2,
    X_C3 = 0xC3,
    X_C4 = 0xC4,
    X_C5 = 0xC5,
    X_C6 = 0xC6,
    X_C7 = 0xC7,
    X_C8 = 0xC8,
    X_C9 = 0xC9,
    X_CA = 0xCA,
    X_CB = 0xCB,
    X_CC = 0xCC,
    X_CD = 0xCD,
    X_CE = 0xCE,
    X_CF = 0xCF,
    X_D0 = 0xD0,
    X_D1 = 0xD1,
    X_D2 = 0xD2,
    X_D3 = 0xD3,
    X_D4 = 0xD4,
    X_D5 = 0xD5,
    X_D6 = 0xD6,
    X_D7 = 0xD7,
    X_D8 = 0xD8,
    X_D9 = 0xD9,
    X_DA = 0xDA,
    OemOpenBrackets = 0xDB,
    OemPipe = 0xDC,
    OemCloseBrackets = 0xDD,
    OemQuotes = 0xDE,
    Oem8 = 0xDF,
    X_E0 = 0xE0,
    X_E1 = 0xE1,
    OemBackslash = 0xE2,
    X_E3 = 0xE3,
    X_E4 = 0xE4,
    ProcessKey = 0xE5,
    X_E6 = 0xE6,
    Packet = 0xE7,
    X_E8 = 0xE8,
    X_E9 = 0xE9,
    X_EA = 0xEA,
    X_EB = 0xEB,
    X_EC = 0xEC,
    X_ED = 0xED,
    X_EE = 0xEE,
    X_EF = 0xEF,
    X_F0 = 0xF0,
    X_F1 = 0xF1,
    X_F2 = 0xF2,
    X_F3 = 0xF3,
    X_F4 = 0xF4,
    X_F5 = 0xF5,
    Attn = 0xF6,
    Crsel = 0xF7,
    Exsel = 0xF8,
    EraseEof = 0xF9,
    Play = 0xFA,
    Zoom = 0xFB,
    NoName = 0xFC,
    Pa1 = 0xFD,
    OemClear = 0xFE,
    X_FF = 0xFF,
}
