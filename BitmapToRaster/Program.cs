namespace BitmapToRaster;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;

class BitmapToRaster {

    private static Font CreateFont (string familyName, float emSize) {
        var f = new Font(familyName, emSize, GraphicsUnit.Pixel);
        return string.Equals(f.FontFamily.Name, familyName, StringComparison.OrdinalIgnoreCase) ? f : throw new ArgumentException("no such font", nameof(familyName));
    }

    private static int Main (string[] args) {
        try {
            if (args.Length == 2 && float.TryParse(args[1], out var emsize)) {
                FontToTextFont(CreateFont(args[0], emsize));
            } else {
                ImageToRaster(args[0], args[1]);
            }
        } catch {
            return -1;
        }
        return 0;
    }

    static Graphics FromImage (Bitmap bitmap) {
        var graphics = Graphics.FromImage(bitmap);
        graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.None;
        graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;
        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
        graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
        return graphics;
    }

    private static void FontToTextFont (Font font) {
        const char OnChar = 'X', OffChar = '.';
        var alignment = new StringFormat() { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center };

        var maxSize = (int)Math.Ceiling(15f * font.SizeInPoints);
        using var bitmap = new Bitmap(maxSize, maxSize, PixelFormat.Format32bppArgb);
        var entireBitmapF = new RectangleF(new(), bitmap.Size);

        using (var graphics = FromImage(bitmap)) {
            graphics.Clear(Color.Transparent);
            for (var c = '!'; c <= '~'; ++c) {
                graphics.DrawString($"{c}", font, Brushes.White, entireBitmapF, alignment);
            }
        }

        var glyphRectangle = FindRegion(bitmap, out var stride);
        Debug.Assert(stride == bitmap.Width * sizeof(int));
        var size = glyphRectangle.Size;
        var text = Console.Out;// new StreamWriter(filepath, false, System.Text.Encoding.ASCII) { NewLine = "\n" };
        text.Write("{0}\n", font.FontFamily.Name);
        text.Write("{0},{1}\n", size.Height, (int)OnChar);
        var blank = new string('.', size.Width) + "\n";
        int blankLineCount = size.Height * '!';
        for (var i = 0; i < blankLineCount; ++i)
            text.Write(blank);

        var entireBitmap = new Rectangle(new(), bitmap.Size);
        var intBuffer = new int[size.Width];
        var charBuffer = new char[size.Width + 1];
        charBuffer[size.Width] = '\n';
        var firstPixelOffset = sizeof(int) * (bitmap.Width * glyphRectangle.Top + glyphRectangle.Left);

        for (var c = '!'; c <= '~'; ++c) {
            using (var graphics = FromImage(bitmap)) {
                graphics.Clear(Color.Transparent);
                graphics.DrawString($"{c}", font, Brushes.White, entireBitmapF, alignment);
            }

            var lb = bitmap.LockBits(entireBitmap, ImageLockMode.ReadOnly, bitmap.PixelFormat);

            for (int y = 0, rowOffset = firstPixelOffset; y < size.Height; ++y, rowOffset += stride) {
                Marshal.Copy(lb.Scan0 + rowOffset, intBuffer, 0, size.Width);
                for (var i = 0; i < size.Width; ++i)
                    charBuffer[i] = (intBuffer[i] & 0xffff00) != 0 ? OnChar : OffChar;
                text.Write(charBuffer);
            }
            bitmap.UnlockBits(lb);
        }
        var moreBlankLines = size.Height * (256 - '~');
        while (--moreBlankLines >= 0)
            text.Write(blank);
    }

    private static Rectangle FindRegion (in Bitmap bitmap, out int stride) {
        Debug.Assert(bitmap.PixelFormat == PixelFormat.Format32bppArgb);
        var lb = bitmap.LockBits(new(new(), bitmap.Size), ImageLockMode.ReadOnly, bitmap.PixelFormat);
        Debug.Assert(lb.Stride == lb.Width * sizeof(int));
        stride = lb.Stride;
        var indices = new Point[lb.Height];
        var pixels = new int[lb.Width];
        for (var y = 0; y < lb.Height; ++y) {
            Marshal.Copy(lb.Scan0 + y * lb.Stride, pixels, 0, lb.Width);
            var first = Array.FindIndex(pixels, i => i != 0);
            var last = Array.FindLastIndex(pixels, i => i != 0);
            indices[y] = new(first, last);
        }
        var top = Array.FindIndex(indices, p => p.X != -1);
        var bottom = Array.FindLastIndex(indices, p => p.X != -1);
        if (top < 0)
            return Rectangle.Empty;
        var left = lb.Width;
        var right = 0;
        foreach (var p in indices) {
            if (p.X < 0)
                continue;
            left = Math.Min(p.X, left);
            right = Math.Max(p.Y, right);
        }
        bitmap.UnlockBits(lb);
        return left < right ? (new(left - 1, top - 1, right - left + 3, bottom - top + 3)) : Rectangle.Empty;
    }

    private static void ImageToRaster (string filepath, string outputRoot) {
        using var image = new Bitmap(filepath);
        var l = image.LockBits(new Rectangle(Point.Empty, image.Size), ImageLockMode.ReadOnly, image.PixelFormat);
        var bytes = new byte[l.Stride * l.Height];
        var channels = l.Stride / l.Width;
        const int bytesPerChannel = 1;
        if (l.Stride % l.Width != 0)
            throw new Exception();
        Marshal.Copy(l.Scan0, bytes, 0, bytes.Length);
        image.UnlockBits(l);
        var name = Path.GetFileNameWithoutExtension(filepath);
        var root = Path.GetDirectoryName(filepath);
        var outputFilename = $"{name}.raw";
        var outputDir = Path.Combine(outputRoot, root);
        _ = Directory.CreateDirectory(outputDir);
        var outputFilepath = Path.Combine(outputDir, outputFilename);
        Console.Write($"{filepath}: {image.Width}x{image.Height}, {channels} channels, {bytes.Length} bytes");
        using (var f = new BinaryWriter(File.Create(outputFilepath))) {
            f.Write(image.Width);
            f.Write(image.Height);
            f.Write(channels);
            f.Write(bytesPerChannel);
            f.Write(bytes.Length);
            using var zip = new DeflateStream(f.BaseStream, CompressionLevel.Optimal);
            zip.Write(bytes, 0, bytes.Length);
        }
        var compressedSize = new FileInfo(outputFilepath).Length;
        Console.WriteLine($" => {outputFilepath} {compressedSize} compressed ({Math.Round(100.0 * compressedSize / bytes.Length, 2)} % of original)");
    }
}
