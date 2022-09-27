namespace Win32;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Explicit)]
public unsafe struct DeviceModeA {
    const int CCHDEVICENAME = 32;
    const int CCHFORMNAME = 32;
    [FieldOffset(0)]
    public fixed byte dmDeviceName[CCHDEVICENAME];
    [FieldOffset(32)]
    public ushort dmSpecVersion;
    [FieldOffset(34)]
    public ushort dmDriverVersion;
    [FieldOffset(36)]
    public ushort dmSize;
    [FieldOffset(38)]
    public ushort dmDriverExtra;
    [FieldOffset(40)]
    public uint dmFields;
    [FieldOffset(44)]
    public short dmOrientation;
    [FieldOffset(46)]
    public short dmPaperSize;
    [FieldOffset(48)]
    public short dmPaperLength;
    [FieldOffset(50)]
    public short dmPaperWidth;
    [FieldOffset(52)]
    public short dmScale;
    [FieldOffset(54)]
    public short dmCopies;
    [FieldOffset(56)]
    public short dmDefaultSource;
    [FieldOffset(58)]
    public short dmPrintQuality;
    [FieldOffset(44)]
    public Point dmPosition;
    [FieldOffset(52)]
    public uint dmDisplayOrientation;
    [FieldOffset(56)]
    public uint dmDisplayFixedOutput;
    [FieldOffset(60)]
    public short dmColor;
    [FieldOffset(62)]
    public short dmDuplex;
    [FieldOffset(64)]
    public short dmYResolution;
    [FieldOffset(66)]
    public short dmTTOption;
    [FieldOffset(68)]
    public short dmCollate;
    [FieldOffset(70)]
    public fixed byte dmFormName[CCHFORMNAME];
    [FieldOffset(102)]
    public ushort dmLogPixels;
    [FieldOffset(104)]
    public uint dmBitsPerPel;
    [FieldOffset(108)]
    public uint dmPelsWidth;
    [FieldOffset(112)]
    public uint dmPelsHeight;
    [FieldOffset(116)]
    public uint dmDisplayFlags;
    [FieldOffset(116)]
    public uint dmNup;
    [FieldOffset(120)]
    public uint dmDisplayFrequency;
    [FieldOffset(124)]
    public uint dmICMMethod;
    [FieldOffset(128)]
    public uint dmICMIntent;
    [FieldOffset(132)]
    public uint dmMediaType;
    [FieldOffset(136)]
    public uint dmDitherType;
    [FieldOffset(140)]
    public uint dmReserved1;
    [FieldOffset(144)]
    public uint dmReserved2;
    [FieldOffset(148)]
    public uint dmPanningWidth;
    [FieldOffset(152)]
    public uint dmPanningHeight;
}
