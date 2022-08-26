namespace Engine6;
using System.Collections.Generic;
using System.Windows.Forms;
using System;
using Win32;

public partial class Configuration:Form {
    const string WhenReady = "If you are certain you can make a better choice than the default, go ahead.";

    private const PixelFlag RequiredFlags = PixelFlag.None
        | PixelFlag.DrawToWindow
        | PixelFlag.DoubleBuffer
        | PixelFlag.SupportOpengl
        //| PixelFlags.SwapExchange
        //| PixelFlags.SupportComposition
        ;
    private const PixelFlag RejectedFlags = PixelFlag.None
        | PixelFlag.GenericAccelerated
        | PixelFlag.GenericFormat
        ;

    public Configuration () {
        InitializeComponent();
        Dc = new(Handle);
        Load += Load_self;
    }
    DeviceContext Dc;
    private List<PixelFormatDescriptor> GetPixelFormatDescriptors () {
        var count = Gdi32.GetPixelFormatCount(Dc);
        var pfds = new List<PixelFormatDescriptor>();
        var p = new PixelFormatDescriptor() {
            size = PixelFormatDescriptor.Size,
            version = 1,
        };
        for (var i = 1; i <= count; ++i) {
            Gdi32.DescribePixelFormat(Dc, i, ref p);
            if (p.pixelType==
        }
    }
    private void Load_self (object sender, System.EventArgs e) {
        Load -= Load_self;
        System.Threading.Tasks.Task.Run(
    }
}
