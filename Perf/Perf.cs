namespace Perf {
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.IO;
    using System.Diagnostics;
    using System.Windows.Forms;
    using System.Drawing.Imaging;

    class Perf:Form {

        private const int _IMAGE_WIDTH_ = 2000;
        private const int _SECTION_HEIGHT_ = 20;
        private readonly List<Entry> Entries = new List<Entry>();
        private readonly string[] Names;
        private readonly int[] Values;
        private readonly List<(int eventIndex, int maxDepth)> FrameInfo = new List<(int, int)>();

        private Perf (string filepath) {
            using (BinaryReader reader = new(File.OpenRead(filepath))) {
                var enumCount = reader.ReadInt32();
                Names = new string[enumCount];
                Values = new int[enumCount];
                for (var i = 0; i < Names.Length; ++i) {
                    Values[i] = reader.ReadByte();
                    Names[i] = reader.GetString();
                }
                while (Entry.FromBinaryReader(reader) is Entry entry)
                    Entries.Add(entry);
            }

            if (Entries.Count == 0) {
                Text = "no entries";
                return;
            }

            var depth = 0;
            for (var i = 0; i < Entries.Count; i++) {
                var entry = Entries[i];
                var isEnter = entry.IsEnter;
                if (isEnter && depth == 0)
                    FrameInfo.Add((i, MaxDepthOfFrameStartingAt(i)));
                depth += isEnter ? 1 : -1;
            }
            Load += Load_self;
        }

        private PictureBox picture, detail;
        private IntegralUpDown frameNum;

        private void Load_self (object sender, EventArgs args) {
            Load -= Load_self;
            ClientSize = new(_IMAGE_WIDTH_ + 50, 500);
            Location = new(10, 10);
            frameNum = new() { Width = 200 };
            Controls.Add(frameNum);
            picture = new() { Location = new(10, 50), ClientSize = new(_IMAGE_WIDTH_, _SECTION_HEIGHT_) };
            Controls.Add(picture);
            detail = new() { Location = new(10, 50 + _SECTION_HEIGHT_ + 50), ClientSize = new(_IMAGE_WIDTH_, _SECTION_HEIGHT_) };
            Controls.Add(detail);

            frameNum.ValueChanged += ValueChanged_frameNum;
            frameNum.Value = 1;
            frameNum.Minimum = 1;
            frameNum.Maximum = FrameInfo.Count;
            picture.MouseDown += Mouse_picture;
            picture.MouseUp += Mouse_picture;
            picture.MouseMove += Mouse_picture;
            picture.MouseWheel += Mouse_picture;
        }

        private static readonly (int r, int g, int b)[] colors = {
            (200, 131, 65),
            (142, 92, 201),
            (100, 172, 72),
            (210, 69, 149),
            (74, 172, 141),
            (206, 76, 58),
            (98, 134, 202),
            (154, 150, 63),
            (192, 125, 190),
            (192, 94, 111),
        };

        private readonly Brush[] brushes = Array.ConvertAll(colors, c => new SolidBrush(Color.FromArgb(c.r, c.g, c.b)));

        private int FrameIndex => frameNum.Value - 1;
        private double pixelsPerTick;

        private long FrameTicks => Entries[FrameIndex + 1 == FrameInfo.Count ? FrameInfo[FrameInfo.Count - 1].eventIndex : FrameInfo[FrameIndex + 1].eventIndex - 1].Ticks - Entries[FrameInfo[FrameIndex].eventIndex].Ticks;

        private void ValueChanged_frameNum (object sender, EventArgs args) {
            if (!(0 <= FrameIndex && FrameIndex < Entries.Count))
                throw new Exception();
            var (eventIndex, maxDepth) = FrameInfo[FrameIndex];

            pixelsPerTick = (double)_IMAGE_WIDTH_ / FrameTicks;
            var previousImage = picture.BackgroundImage;
            var imageHeight = maxDepth * _SECTION_HEIGHT_;
            picture.BackgroundImage = new Bitmap(_IMAGE_WIDTH_, imageHeight);
            previousImage?.Dispose();

            previousImage = picture.Image;
            picture.Image = new Bitmap(_IMAGE_WIDTH_, imageHeight, PixelFormat.Format32bppArgb);
            previousImage?.Dispose();
            previousImage = detail.Image;
            detail.Image = new Bitmap(_IMAGE_WIDTH_, imageHeight, PixelFormat.Format32bppArgb);
            previousImage?.Dispose();

            picture.ClientSize = new(_IMAGE_WIDTH_, imageHeight);
            detail.ClientSize = new(_IMAGE_WIDTH_, imageHeight);

            detail.Location = new(detail.Left, picture.Top + picture.Height + picture.Margin.Vertical);
            using (var g = Graphics.FromImage(detail.Image))
                g.Clear(Color.Black);

            using (var g = Graphics.FromImage(picture.Image))
                g.Clear(Color.Transparent);
            DrawBackgroundImage();
        }

        private void DrawBackgroundImage () {
            using (var g = DrawAndClear(picture.BackgroundImage, Color.Black)) {
                var eventIndex = FrameInfo[FrameIndex].eventIndex;
                var t0 = Entries[eventIndex].Ticks;
                Stack<Entry> stk = new();
                brushIndex = 0;
                do {
                    var entry = Entries[eventIndex++];
                    if (entry.IsEnter) {
                        stk.Push(entry);
                        continue;
                    }
                    var entering = stk.Pop();
                    var x0 = (float)((entering.Ticks - t0) * pixelsPerTick);
                    var width = (entry.Ticks - entering.Ticks) * pixelsPerTick;
                    var y0 = stk.Count * _SECTION_HEIGHT_;
                    g.FillRectangle(GetNextBrush(), x0, y0, (float)Math.Max(width, 1.0), _SECTION_HEIGHT_);
                } while (stk.Count != 0);

                var pixelsPerMilli = (float)(pixelsPerTick * Stopwatch.Frequency / 1000);

                for (var x = pixelsPerMilli; x < _IMAGE_WIDTH_; x += pixelsPerMilli)
                    g.DrawLine(Pens.Silver, x, 0f, x, _SECTION_HEIGHT_ / 2);
            }
        }

        private int brushIndex;

        private Brush GetNextBrush () {
            ++brushIndex;
            if (brushIndex == brushes.Length)
                brushIndex = 0;
            return brushes[brushIndex];
        }

        private void Mouse_picture (object sender, MouseEventArgs args) {
            var wheelSteps = Math.DivRem(args.Delta, SystemInformation.MouseWheelScrollDelta, out var rem);
            if (rem != 0)
                Debug.WriteLine($"rem : {rem}");
            scaleExponent = Math.Max(minimumExponent, Math.Min(scaleExponent - 0.2f * wheelSteps, maximumExponent));
            Text = WindowSeconds.ToString();
            DrawDetailRectangle(args.X);
        }

        private static readonly double minimumExponent = Math.Log10(0.00001);
        private static readonly double maximumExponent = Math.Log10(0.001);
        private double scaleExponent = maximumExponent;
        private double WindowSeconds => Math.Pow(10, scaleExponent);
        private void DrawDetailRectangle (int x) {
            var pixelsPerSecond = pixelsPerTick * Stopwatch.Frequency;
            var windowWidthPixels = WindowSeconds * pixelsPerSecond;

            var halfWidthPixels = windowWidthPixels / 2.0;

            var centerPixels = Math.Max(halfWidthPixels, Math.Min(x, _IMAGE_WIDTH_ - halfWidthPixels));
            var windowLeftPixels = centerPixels - halfWidthPixels;

            using (var g = DrawAndClear(picture.Image, Color.Transparent)) {
                g.DrawRectangle(Pens.White, (float)windowLeftPixels, 0, (float)windowWidthPixels, picture.Image.Height);
            }

            var windowLeftTicks = windowLeftPixels / pixelsPerTick;
            var windowWidthTicks = windowWidthPixels / pixelsPerTick;
            var windowRightTicks = windowLeftTicks + windowWidthTicks;

            using (var g = DrawAndClear(detail.Image, Color.Black)) {
                var eventIndex = FrameInfo[FrameIndex].eventIndex;
                var frameStartTicks = Entries[eventIndex].Ticks;
                Stack<Entry> stk = new();
                brushIndex = 0;
                do {
                    var entry = Entries[eventIndex++];
                    if (entry.IsEnter) {
                        stk.Push(entry);
                        continue;
                    }
                    var entering = stk.Pop();
                    var eventLeftTicks = entering.Ticks - frameStartTicks;
                    var eventRightTicks = entry.Ticks - frameStartTicks;
                    var brush = GetNextBrush();
                    var isCompletelyOutside = eventRightTicks < windowLeftTicks || windowRightTicks < eventLeftTicks;
                    if (isCompletelyOutside)
                        continue;

                    var ticksFromWindowStart = eventLeftTicks - windowLeftTicks;
                    var zoomedInPixelsPerTick = _IMAGE_WIDTH_ / windowWidthTicks;
                    var rectangleLeft = ticksFromWindowStart * zoomedInPixelsPerTick;
                    var rectangleWidth = (entry.Ticks - entering.Ticks) * zoomedInPixelsPerTick;
                    if (rectangleLeft < 0.0) {
                        rectangleWidth -= Math.Abs(rectangleLeft);
                        rectangleLeft = 0.0;
                    }
                    g.FillRectangle(brush, (float)rectangleLeft, stk.Count * _SECTION_HEIGHT_, (float)rectangleWidth, _SECTION_HEIGHT_);
                } while (stk.Count != 0);
            }

            picture.Refresh();
            detail.Refresh();
        }

        private int MaxDepthOfFrameStartingAt (int eventIndex) {
            var maxDepth = 0;
            var depth = 0;
            do {
                var isEnter = Entries[eventIndex++].IsEnter;
                depth += isEnter ? +1 : -1;
                maxDepth = Math.Max(depth, maxDepth);
            } while (depth != 0);
            return maxDepth;
        }

        private static Graphics DrawAndClear (Image image, Color clearColor) {
            var g = Graphics.FromImage(image);
            g.Clear(clearColor);
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.SmoothingMode = SmoothingMode.HighQuality;
            return g;
        }

        class Entry {
            public readonly long Ticks;
            public readonly int E;
            private Entry (long ticks, int e) => (Ticks, E) = (ticks, e);
            public static Entry FromStream (Stream stream) => stream.TryRead(out long l) && stream.TryRead(out byte b) ? new Entry(l, b) : null;
            public static Entry FromBinaryReader (BinaryReader reader) => reader.TryRead(out long l) && reader.TryRead(out byte b) ? new Entry(l, b) : null;
            public bool IsEnter => E != 0;
        }

        [STAThread]
        static void Main (string[] args) {
            using (Perf f = new(Path.Combine(Directory.GetCurrentDirectory(), args[0])))
                _ = f.ShowDialog();
        }
    }
}
