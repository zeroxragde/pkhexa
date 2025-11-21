using SkiaSharp;
using System.IO;
using Microsoft.Maui.Controls;

namespace PkHexA.Helper
{
    public static class SpriteHelper
    {
        public static ImageSource SafeImageSourceFromSKBitmap(SKBitmap bitmap)
        {
            if (bitmap == null)
                return null;

            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);

            // Copia segura
            var bytes = data.ToArray();

            return ImageSource.FromStream(() => new MemoryStream(bytes));
        }
    }
}
