namespace Win32;

using System;

[Flags]
public enum WindowStyle:uint {
    Overlapped = /*     */ 0x00000000,
    MaximizeBox = /*    */ 0x00010000,
    MinimizeBox = /*    */ 0x00020000,
    Thickframe = /*     */ 0x00040000,
    Sysmenu = /*        */ 0x00080000,
    Hscroll = /*        */ 0x00100000,
    Vscroll = /*        */ 0x00200000,
    Dlgframe = /*       */ 0x00400000,
    Border = /*         */ 0x00800000,
    Maximize = /*       */ 0x01000000,
    ClipChildren = /*   */ 0x02000000,
    ClipSiblings = /*   */ 0x04000000,
    Disabled = /*       */ 0x08000000,
    Visible = /*        */ 0x10000000,
    Minimize = /*       */ 0x20000000,
    Child = /*          */ 0x40000000,
    Popup = unchecked(0x80000000),
    Tiled = Overlapped,
    ChildWindow = Child,
    Iconic = Minimize,
    Sizebox = Thickframe,
    Caption = Border | Dlgframe,
    OverlappedWindow = Caption | Sysmenu | Thickframe | MinimizeBox | MaximizeBox,
    TiledWindow = OverlappedWindow,
    PopupWindow = Popup | Border | Sysmenu,
    ClipPopup = ClipChildren | ClipSiblings | Popup,
}
