namespace BitmapToRaster;

using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
class BitmapToRaster {

    private static void Main (string[] args) => Convert(args[0], args[1]);
    private static void Convert (string filepath, string outputRoot) {
        using (var image = new Bitmap(filepath)) {
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
            Console.WriteLine($"{filepath}: {image.Width}x{image.Height}, {channels} channels, {bytes.Length} bytes");
            using (var f = new BinaryWriter(File.Create(outputFilepath))) {
                f.Write(image.Width);
                f.Write(image.Height);
                f.Write(channels);
                f.Write(bytesPerChannel);
                f.Write(bytes.Length);
                using (var zip = new DeflateStream(f.BaseStream, CompressionLevel.Optimal))
                    zip.Write(bytes, 0, bytes.Length);
            }
        }
    }
}
