using SkiaSharp;

namespace PkHexA.Services
{

    public static class SKBitmapExtensions
    {
        public static ImageSource ToImageSource(this SKBitmap bmp)
        {
            using var image = SKImage.FromBitmap(bmp);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            return ImageSource.FromStream(() => data.AsStream());
        }
    }

}
