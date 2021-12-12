namespace Win32;
using System;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
public struct PixelFormatDescriptor {
    public ushort Ss;
    public ushort Ver;
    public PixelFlags Flags;
    public byte PxTp;
    public byte ClrBts;
    public byte RdBts;
    public byte RdSft;
    public byte GnBts;
    public byte GnSft;
    public byte BlBts;
    public byte BlSft;
    public byte AlBts;
    public byte AlSft;
    public byte AcBts;
    public byte AcRd;
    public byte AcGn;
    public byte AcBl;
    public byte AcAl;
    public byte DpBts;
    public byte SlBts;
    public byte AxBts;
    public byte Lt;
    private byte Reserved;
    public uint Lm;
    public uint Vm;
    public uint Dm;
    public static readonly Predicate<PixelFormatDescriptor> Typical = d => d.PxTp == 0 && d.RdBts == 8 && d.GnBts == 8 && d.BlBts == 8 && d.DpBts >= 16 && d.Flags.HasFlag(PixelFlags.Typical);
    public override string ToString () {
        return $"{Flags},color bits {ClrBts} (rgba bits,shift) ({RdBts},{RdSft}/{GnBts},{GnSft}/{BlBts},{BlSft}/{AlBts},{AlSft}) depth {DpBts}, accum rgba {AcRd}/{AcGn}/{AcBl}/{AcAl}, stencil {SlBts}, aux {AxBts}, layertype {Lt} layermask {Lm} visible mas {Vm} dmg mask {Dm}";
    }

    //Use this function to make a new one with Size and Version already filled in.
    public static PixelFormatDescriptor Create () {
        var pfd = new PixelFormatDescriptor {
            Ss = (ushort)Marshal.SizeOf<PixelFormatDescriptor>(),
            Ver = 1
        };

        return pfd;
    }
}