namespace Engine6;

using System.Collections.Generic;
using System.Windows.Forms;
using System;
using System.Threading.Tasks;
using Win32;
using Gl;

public partial class Configuration:Form {
    public ContextConfiguration? Config { get; private set; }

    private const string WhenReady = "If you are certain you can make a better choice than the default, go ahead.";
    private const string NoPixelFormat = "Could not find any appropriate configuration.";

    private class Datum {
        public int Index { get; init; }
        public string Description { get; init; }
        public PixelFormatDescriptor Pfd { get; init; }
        public override string ToString () => Description;
    }

    private const PixelFlag RequiredFlags = PixelFlag.None
        | PixelFlag.DrawToWindow
        //| PixelFlag.DoubleBuffer
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
        FormClosing += FormClosing_self;
        startButton.Click += Click_start;
        quitButton.Click += Click_quit;
    }

    private void FormClosing_self (object sender, FormClosingEventArgs e) {
        Dc.Dispose();
        Dc = null;
    }

    private void Click_quit (object sender, System.EventArgs e) {
        Config = null;
        DialogResult = DialogResult.Cancel;
        Close();
    }

    private void Click_start (object sender, System.EventArgs e) {
        var pfd = ((Datum)listBox1.SelectedItem).Pfd;
        Config = new() {
            ColorBits = pfd.colorBits,
            DepthBits = pfd.depthBits,
            DoubleBuffer = pfd.flags.HasFlag(PixelFlag.DoubleBuffer),
            SwapMethod = SwapMethodFrom(pfd.flags),
            Profile = ProfileMask.Core,
            Flags = ContextFlag.Debug | ContextFlag.ForwardCompatible
        };
        DialogResult = DialogResult.OK;
        Close();
    }

    private DeviceContext Dc;

    private static string PfdToString (in PixelFormatDescriptor p) =>
        $"color bits: {p.colorBits}, depth bits: {p.depthBits}, swap method: {SwapMethodFrom(p.flags)}{(p.flags.HasFlag(PixelFlag.DoubleBuffer) ? ", doublebuffered" : null)}";

    private static SwapMethod SwapMethodFrom (PixelFlag f) {
        if (f.HasFlag(PixelFlag.SwapCopy))
            return SwapMethod.Copy;
        if (f.HasFlag(PixelFlag.SwapExchange))
            return SwapMethod.Swap;
        return SwapMethod.Undefined;
    }

    private List<Datum> GetPixelFormatDescriptors () {
        var count = Gdi32.GetPixelFormatCount(Dc);
        PixelFormatDescriptor p = new() {
            size = Win32.PixelFormatDescriptor.Size,
            version = 1,
        };
        List<Datum> l = new();
        for (var i = 1; i <= count; ++i) {
            Gdi32.DescribePixelFormat(Dc, i, ref p);
            if (0 == p.pixelType && 32 == p.colorBits && 24 <= p.depthBits && (RequiredFlags & p.flags) == RequiredFlags && (RejectedFlags & p.flags) == 0)
                l.Add(new() { Index = i, Description = PfdToString(p), Pfd = p });
        }
        return l;
    }

    private async void Load_self (object sender, System.EventArgs args) {
        Load -= Load_self;
        try {
            var list = await Task.Factory.StartNew(GetPixelFormatDescriptors);
            if (0 == list.Count) {
                Text = NoPixelFormat;
                return;
            }
            listBox1.BeginUpdate();
            foreach (var eh in list)
                _ = listBox1.Items.Add(eh);
            listBox1.EndUpdate();
            for (var i = 0; i < listBox1.Items.Count; ++i) {
                var x = (Datum)listBox1.Items[i];
                if (32 == x.Pfd.depthBits) {
                    listBox1.SelectedIndex = i;
                    break;
                }
            }
            if (-1 == listBox1.SelectedIndex)
                listBox1.SelectedIndex = 0;
            listBox1.Enabled = true;
            Text = WhenReady;
            startButton.Enabled = true;
        } catch (Exception e) {
            Text = e.Message;
        }
    }
}
