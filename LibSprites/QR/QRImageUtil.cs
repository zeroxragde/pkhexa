using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace PkHexA.LibSprites.QR
{
    public static class QRImageUtil
    {
        public static SKBitmap GetQRImage(SKBitmap qr, SKBitmap preview)
        {
            // foreground = preview + borde blanco de 4 px
            int fgW = preview.Width + 4;
            int fgH = preview.Height + 4;

            var foreground = new SKBitmap(fgW, fgH);
            using (var canvas = new SKCanvas(foreground))
            {
                // fondo blanco
                canvas.Clear(SKColors.White);

                // centrar preview
                int px = (fgW - preview.Width) / 2;
                int py = (fgH - preview.Height) / 2;

                canvas.DrawBitmap(preview, new SKPoint(px, py));
            }

            // Crear el bitmap final
            var result = new SKBitmap(qr.Width, qr.Height);
            using (var canvas = new SKCanvas(result))
            {
                // dibujar el QR original
                canvas.DrawBitmap(qr, 0, 0);

                // centrar el preview generado
                int x = (qr.Width - foreground.Width) / 2;
                int y = (qr.Height - foreground.Height) / 2;

                canvas.DrawBitmap(foreground, new SKPoint(x, y));
            }

            return result;
        }


        public static SKBitmap GetQRImageExtended(
    SKFont font,
    SKBitmap qr,
    SKBitmap pk,
    int width,
    int height,
    ReadOnlySpan<string> lines,
    string extraText)
        {
            // Este método YA lo convertimos arriba
            var pic = GetQRImage(qr, pk);

            // ExtendImage también debe tener su versión SKBitmap
            return ExtendImage(font, qr, width, height, pic, lines, extraText);
        }

        private static SKBitmap ExtendImage(
    SKFont font,
    SKBitmap qr,
    int width,
    int height,
    SKBitmap pic,
    ReadOnlySpan<string> lines,
    string extraText)
        {
            // Crear la imagen final
            var newpic = new SKBitmap(width, height);

            using var canvas = new SKCanvas(newpic);

            // Fondo blanco
            canvas.Clear(SKColors.White);

            // Dibujar la imagen pic encima
            canvas.DrawBitmap(pic, 0, 0);

            // Color negro para texto
            var paint = new SKPaint
            {
                Color = SKColors.Black,
                IsAntialias = true,
                Typeface = font.Typeface,
                TextSize = font.Size
            };

            const int indent = 18;

            // Líneas de texto igual que en tu versión original
            canvas.DrawText(GetLine(lines, 0), indent, qr.Height - 5, paint);
            canvas.DrawText(GetLine(lines, 1), indent, qr.Height + 8, paint);
            canvas.DrawText(GetLine2(lines), indent, qr.Height + 20, paint);
            canvas.DrawText(GetLine(lines, 3) + extraText, indent, qr.Height + 32, paint);

            return newpic;
        }

        /// <summary>
        /// Gets and formats the second line of text for display.
        /// </summary>
        /// <param name="lines">The lines of text.</param>
        /// <returns>The formatted second line.</returns>
        private static string GetLine2(ReadOnlySpan<string> lines) => GetLine(lines, 2)
            .Replace(Environment.NewLine, "/")
            .Replace("//", "   ")
            .Replace(":/", ": ");
        /// <summary>
        /// Gets a specific line of text or an empty string if the line does not exist.
        /// </summary>
        /// <param name="lines">The lines of text.</param>
        /// <param name="line">The line index to retrieve.</param>
        /// <returns>The requested line or an empty string.</returns>
        private static string GetLine(ReadOnlySpan<string> lines, int line) =>
            lines.Length <= line ? string.Empty : lines[line];



    }
}
