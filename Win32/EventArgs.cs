namespace Win32;

using System;
using Common;
public record struct SizeArgs (SizeType sizeType, in Vector2i size);
public record struct ShowWindowArgs (bool Shown, ShowWindowReason Reason);
public record struct PaintArgs (nint Dc, in PaintStruct Ps);
public record struct MoveArgs (in Vector2i ClientRelativePosition);
public record struct KeyArgs (Key Key, bool Repeat);
public record InputArgs (int Dx, int Dy);
public record struct FocusChangedArgs (bool Focused);
public record struct ButtonArgs (MouseButton Button, in PointShort Location);
