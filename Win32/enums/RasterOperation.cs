namespace Win32;

public enum RasterOperation:uint {

    /// <summary>dest = source</summary>
    SrcCopy = 0x00CC0020u,
    /// <summary>dest = source OR dest</summary>
    SrcPaint = 0x00EE0086u,
    /// <summary>dest = source AND dest</summary>
    SrcAnd = 0x008800C6u,
    /// <summary>dest = source XOR dest</summary>
    SrcInvert = 0x00660046u,
    /// <summary>dest = source AND (NOT dest )</summary>
    SrcErase = 0x00440328u,
    /// <summary>dest = (NOT source)</summary>
    NotSrcCopy = 0x00330008u,
    /// <summary>dest = (NOT src) AND (NOT dest)</summary>
    NotSrcErase = 0x001100A6u,
    /// <summary>dest = (source AND pattern)</summary>
    MergeCopy = 0x00C000CAu,
    /// <summary>dest = (NOT source) OR dest</summary>
    MergePaint = 0x00BB0226u,
    /// <summary>dest = pattern</summary>
    PatCopy = 0x00F00021u,
    /// <summary>dest = DPSnoo</summary>
    PatPaint = 0x00FB0A09u,
    /// <summary>dest = pattern XOR dest</summary>
    PatInvert = 0x005A0049u,
    /// <summary>dest = (NOT dest)</summary>
    DstInvert = 0x00550009u,
    /// <summary>dest = BLACK</summary>
    Blackness = 0x00000042u,
    /// <summary>dest = WHITE</summary>
    Whiteness = 0x00FF0062u,
}
