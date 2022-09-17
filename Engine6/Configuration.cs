using System.Collections.Generic;
using System.Windows.Forms;
using System;
using System.Threading.Tasks;
using Win32;

namespace Engine6;
public partial class Configuration:Form {
    const string WhenReady = "If you are certain you can make a better choice than the default, go ahead.";
    const string NoPixelFormat = "Could not find any appropriate configuration.";
    public PixelFormatDescriptor? PixelFormatDescriptor { get; private set; }
    public int Index { get; private set; }

    class Datum {
        public int Index { get; init; }
        public string Description { get; init; }
        public PixelFormatDescriptor Pfd { get; init; }
        public override string ToString () => Description;
    }

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
        FormClosing += FormClosing_self;
        startButton.Click += Click_start;
        quitButton.Click += Click_quit;
    }

    private void FormClosing_self (object sender, FormClosingEventArgs e) {
        Dc.Dispose();
        Dc = null;
    }

    private void Click_quit (object sender, System.EventArgs e) {
        PixelFormatDescriptor = null;
        Index = 0;
        DialogResult = DialogResult.Cancel;
        Close();
    }

    private void Click_start (object sender, System.EventArgs e) {
        var datum = (Datum)listBox1.SelectedItem;
        PixelFormatDescriptor = datum.Pfd;
        Index = datum.Index;
        DialogResult = DialogResult.OK;
        Close();
    }

    private DeviceContext Dc;

    static string PfdToString (in PixelFormatDescriptor p) =>
        $"color bits: {p.colorBits}, depth bits: {p.depthBits}, stencil bits: {p.stencilBits}, acc. bits: {p.accBits}, swap method: {SwapMethod(p.flags)}";

    static string SwapMethod (PixelFlag f) {
        if (f.HasFlag(PixelFlag.SwapCopy))
            return nameof(PixelFlag.SwapCopy);
        if (f.HasFlag(PixelFlag.SwapExchange))
            return nameof(PixelFlag.SwapExchange);
        return "Undefined";
    }

    private List<Datum> GetPixelFormatDescriptors () {
        var count = Gdi32.GetPixelFormatCount(Dc);
        PixelFormatDescriptor p = new() {
            size = Win32.PixelFormatDescriptor.Size,
            version = 1,
        };
        var l = new List<Datum>();
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
