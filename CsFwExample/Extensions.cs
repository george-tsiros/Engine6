namespace CsFwExample;

using System.Drawing;
using System.Drawing.Imaging;

internal static class Extensions {
    internal static BitmapData LockBits (this Bitmap self, ImageLockMode mode = ImageLockMode.ReadWrite) {
        return self.LockBits(new Rectangle(new Point(), self.Size), mode, self.PixelFormat);
    }
}
