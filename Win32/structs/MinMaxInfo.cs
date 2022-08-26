namespace Win32;

using Common;

public struct MinMaxInfo {
    public Vector2i reserved, maxSize, maxPosition, minTrackSize, maxTrackSize;
    public override string ToString () => $"{maxSize}, {maxPosition}, {minTrackSize}, {maxTrackSize}";
}
