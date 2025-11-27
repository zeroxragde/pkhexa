using SkiaSharp;

namespace PkHexA.Services
{

    public static class SKBitmapExtensions
    {
        public static ImageSource ToImageSource(this SKBitmap bmp)
        {
            if (bmp == null) return null;

            // 1. Convertimos el bitmap a imagen de Skia
            using var image = SKImage.FromBitmap(bmp);
            // 2. Codificamos a PNG
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);

            // 3. ¡LA SOLUCIÓN!
            // Copiamos los datos a un array de bytes independiente.
            // Esto evita que la imagen dependa del objeto 'data' que se elimina al salir del 'using'.
            var bytes = data.ToArray();

            // 4. Creamos el ImageSource usando una copia fresca de esos bytes guardados
            return ImageSource.FromStream(() => new MemoryStream(bytes));
        }
    }

}
