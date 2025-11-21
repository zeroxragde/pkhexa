
using PkHexA.LibSprites.Enums;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SkiaSharp;
using System.Runtime.InteropServices;


namespace PKHeX.Drawing;

/// <summary>
/// Image Layering/Blending Utility
/// </summary>
public static class ImageUtil
{
    public static SKBitmap LayerImage(SKBitmap baseLayer, SKBitmap overLayer, int x, int y, double transparency)
    {
        overLayer = ChangeOpacity(overLayer, transparency);
        return LayerImage(baseLayer, overLayer, x, y);
    }

    public static SKBitmap LayerImage(SKBitmap baseLayer, SKBitmap overLayer, int x, int y)
    {
        // Crear bitmap destino
        SKBitmap result = new SKBitmap(baseLayer.Width, baseLayer.Height);

        using (var canvas = new SKCanvas(result))
        {
            canvas.DrawBitmap(baseLayer, 0, 0);
            canvas.DrawBitmap(overLayer, new SKPoint(x, y));
            canvas.Flush();
        }

        return result;
    }
    public static SKBitmap ChangeOpacity(SKBitmap img, double trans)
    {
        // Crear copia, igual que tu versión original
        SKBitmap bmp = new SKBitmap(img.Width, img.Height);
        img.CopyTo(bmp);

        // Obtener pixeles (SKColor[])
        var pixels = bmp.Pixels;

        // Cambiar transparencia tal como SetAllTransparencyTo lo hacía
        for (int i = 0; i < pixels.Length; i++)
        {
            var c = pixels[i];
            byte newA = (byte)(c.Alpha * trans); // igual que (alpha * trans)
            pixels[i] = new SKColor(c.Red, c.Green, c.Blue, newA);
        }

        // Asignar de vuelta
        bmp.Pixels = pixels;

        return bmp;
    }
    public static SKBitmap ChangeAllColorTo(SKBitmap img, SKColor c)
    {
        // Clonar igual que img.Clone()
        SKBitmap bmp = new SKBitmap(img.Width, img.Height);
        img.CopyTo(bmp);

        var pixels = bmp.Pixels; // SKColor[]

        byte R = c.Red;
        byte G = c.Green;
        byte B = c.Blue;

        for (int i = 0; i < pixels.Length; i++)
        {
            var p = pixels[i];

            if (p.Alpha == 0)
                continue;

            pixels[i] = new SKColor(R, G, B, p.Alpha);
        }

        bmp.Pixels = pixels;
        return bmp;
    }

    public static SKBitmap ChangeTransparentTo(SKBitmap img, SKColor c, byte trans, int start = 0, int end = -1)
    {
        SKBitmap bmp = new SKBitmap(img.Width, img.Height);
        img.CopyTo(bmp);

        var pixels = bmp.Pixels;

        if (end == -1)
            end = pixels.Length - 1;

        SKColor newColor = new SKColor(c.Red, c.Green, c.Blue, trans);

        for (int i = start; i <= end; i++)
        {
            var p = pixels[i];

            if (p.Alpha == 0)
                pixels[i] = newColor; // mismo comportamiento que tu SetAllTransparencyTo
        }

        bmp.Pixels = pixels;
        return bmp;
    }
    public static SKBitmap BlendTransparentTo(SKBitmap img, SKColor c, byte trans, int start = 0, int end = -1)
    {
        // Clonar igual que img.Clone()
        SKBitmap bmp = new SKBitmap(img.Width, img.Height);
        img.CopyTo(bmp);

        var pixels = bmp.Pixels; // SKColor[]

        if (end == -1)
            end = pixels.Length - 1;

        SKColor newColor = new SKColor(c.Red, c.Green, c.Blue, trans);

        for (int i = start; i <= end; i++)
        {
            var p = pixels[i];

            // Caso 1: Transparente → reemplazar
            if (p.Alpha == 0)
            {
                pixels[i] = newColor;
                continue;
            }

            // Caso 2: Semi-transparente → mezclar (igual que tu BlendColor)
            if (p.Alpha != 255)
            {
                // blend con peso 0.2 como tu método original
                double amount = 0.2;

                byte a = (byte)((p.Alpha * amount) + (newColor.Alpha * (1 - amount)));
                byte r = (byte)((p.Red * amount) + (newColor.Red * (1 - amount)));
                byte g = (byte)((p.Green * amount) + (newColor.Green * (1 - amount)));
                byte b = (byte)((p.Blue * amount) + (newColor.Blue * (1 - amount)));

                pixels[i] = new SKColor(r, g, b, a);
            }
        }

        bmp.Pixels = pixels;
        return bmp;
    }
    public static SKBitmap WritePixels(SKBitmap img, SKColor c, int start, int end)
    {
        // Clonar SKBitmap (equivalente a img.Clone())
        SKBitmap bmp = new SKBitmap(img.Width, img.Height);
        img.CopyTo(bmp);

        // Obtener arreglo de pixeles (SKColor[])
        var pixels = bmp.Pixels;

        // Asegurar rangos válidos
        if (start < 0) start = 0;
        if (end >= pixels.Length) end = pixels.Length - 1;
        if (start > end) return bmp;

        // Escribir color en el rango especificado (igual que ChangeAllTo)
        for (int i = start; i <= end; i++)
        {
            pixels[i] = c;
        }

        // Guardar cambios
        bmp.Pixels = pixels;

        return bmp;
    }
    public static SKBitmap ToGrayscale(SKBitmap img)
    {
        // Clonar igual que (Bitmap)img.Clone()
        SKBitmap bmp = new SKBitmap(img.Width, img.Height);
        img.CopyTo(bmp);

        var pixels = bmp.Pixels; // SKColor[]

        for (int i = 0; i < pixels.Length; i++)
        {
            var p = pixels[i];
            if (p.Alpha == 0)
                continue;

            // Fórmula EXACTA que usabas:
            // grey = (0.3 * R) + (0.59 * G) + (0.11 * B)
            byte grey = (byte)(
                (0.3 * p.Red) +
                (0.59 * p.Green) +
                (0.11 * p.Blue)
            );

            pixels[i] = new SKColor(grey, grey, grey, p.Alpha);
        }

        bmp.Pixels = pixels;

        return bmp;
    }

    public static void GetBitmapData(SKBitmap bmp, out SKColor[] data)
    {
        data = bmp.Pixels;
    }

    public static SKBitmap GetBitmap(ReadOnlySpan<byte> data, int width, int height, PixelFormat format = PixelFormat.Format32bppArgb)
    {
        return GetBitmap(data, width, height, data.Length, format);
    }
    public static SKBitmap GetBitmap(ReadOnlySpan<byte> data, int width, int height, int length, PixelFormat format = PixelFormat.Format32bppArgb)
    {
        var info = new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);
        var bmp = new SKBitmap(info);

        var pixmap = bmp.PeekPixels();
        data[..length].CopyTo(pixmap.GetPixelSpan());

        return bmp;
    }

    public static byte[] GetPixelData(SKBitmap bitmap)
    {
        // Cada pixel = 4 bytes (BGRA en Skia)
        var data = new byte[bitmap.Width * bitmap.Height * 4];

        var span = bitmap.PeekPixels().GetPixelSpan();

        span.CopyTo(data);

        return data;
    }
    public static void SetAllUsedPixelsOpaque(Span<byte> data)
    {
        for (int i = 0; i < data.Length; i += 4)
        {
            if (data[i + 3] != 0)
                data[i + 3] = 0xFF;
        }
    }

    public static void RemovePixels(Span<byte> pixels, ReadOnlySpan<byte> original)
    {
        var arr = MemoryMarshal.Cast<byte, int>(pixels);

        for (int i = original.Length - 4; i >= 0; i -= 4)
        {
            if (original[i + 3] != 0)
                arr[i >> 2] = 0;
        }
    }

    private static void SetAllTransparencyTo(Span<byte> data, double trans)
    {
        for (int i = 0; i < data.Length; i += 4)
            data[i + 3] = (byte)(data[i + 3] * trans);
    }
    public static void SetAllTransparencyTo(Span<byte> data, SKColor c, byte trans, int start, int end)
    {
        var arr = MemoryMarshal.Cast<byte, int>(data);

        // SKColor usa BGRA, pero queremos un INT ARGB como el código original.
        int value =
            (trans << 24) |
            (c.Red << 16) |
            (c.Green << 8) |
            (c.Blue);

        for (int i = end; i >= start; i -= 4)
        {
            if (data[i + 3] == 0) // Alpha == 0
                arr[i >> 2] = value;
        }
    }

    public static void BlendAllTransparencyTo(Span<byte> data, SKColor c, byte trans, int start, int end)
    {
        var arr = MemoryMarshal.Cast<byte, int>(data);

        int value =
            (trans << 24) |
            (c.Red << 16) |
            (c.Green << 8) |
            (c.Blue);

        for (int i = end; i >= start; i -= 4)
        {
            var alpha = data[i + 3];

            if (alpha == 0)
            {
                arr[i >> 2] = value;
            }
            else if (alpha != 0xFF)
            {
                arr[i >> 2] = BlendColor(arr[i >> 2], value);
            }
        }
    }

    private static int BlendColor(int color1, int color2, double amount = 0.2)
    {
        var a1 = (color1 >> 24) & 0xFF;
        var r1 = (color1 >> 16) & 0xFF;
        var g1 = (color1 >> 8) & 0xFF;
        var b1 = color1 & 0xFF;

        var a2 = (color2 >> 24) & 0xFF;
        var r2 = (color2 >> 16) & 0xFF;
        var g2 = (color2 >> 8) & 0xFF;
        var b2 = color2 & 0xFF;

        byte a = (byte)((a1 * amount) + (a2 * (1 - amount)));
        byte r = (byte)((r1 * amount) + (r2 * (1 - amount)));
        byte g = (byte)((g1 * amount) + (g2 * (1 - amount)));
        byte b = (byte)((b1 * amount) + (b2 * (1 - amount)));

        return (a << 24) | (r << 16) | (g << 8) | b;
    }
    public static void ChangeAllTo(Span<byte> data, SKColor c, int start, int end)
    {
        var arr = MemoryMarshal.Cast<byte, int>(data[start..end]);

        int value =
            (c.Alpha << 24) |
            (c.Red << 16) |
            (c.Green << 8) |
            (c.Blue);

        arr.Fill(value);
    }
    public static void ChangeAllColorTo(Span<byte> data, SKColor c)
    {
        byte R = c.Red;
        byte G = c.Green;
        byte B = c.Blue;

        for (int i = 0; i < data.Length; i += 4)
        {
            if (data[i + 3] == 0)
                continue;

            data[i + 0] = B; // Blue
            data[i + 1] = G; // Green
            data[i + 2] = R; // Red
                             // Alpha (data[i+3]) se deja igual
        }
    }

    private static void SetAllColorToGrayScale(Span<byte> data)
    {
        for (int i = 0; i < data.Length; i += 4)
        {
            if (data[i + 3] == 0)
                continue;

            // BGRA → B = [0], G = [1], R = [2]
            byte greyS = (byte)((0.3 * data[i + 2]) +   // R
                                (0.59 * data[i + 1]) +  // G
                                (0.11 * data[i + 0]));  // B

            data[i + 0] = greyS; // Blue
            data[i + 1] = greyS; // Green
            data[i + 2] = greyS; // Red
                                 // Alpha se queda igual
        }
    }

    public static void GlowEdges(Span<byte> data, byte blue, byte green, byte red, int width, int reach = 3, double amount = 0.0777)
    {
        PollutePixels(data, width, reach, amount);
        CleanPollutedPixels(data, blue, green, red);
    }
    private const int PollutePixelColorIndex = 0;

    private static void PollutePixels(Span<byte> data, int width, int reach, double amount)
    {
        int stride = width * 4;
        int height = data.Length / stride;
        for (int i = 0; i < data.Length; i += 4)
        {
            // only pollute outwards if the current pixel is fully opaque
            if (data[i + 3] == 0)
                continue;

            int x = (i % stride) / 4;
            int y = (i / stride);
            {
                int left = Math.Max(0, x - reach);
                int right = Math.Min(width - 1, x + reach);
                int top = Math.Max(0, y - reach);
                int bottom = Math.Min(height - 1, y + reach);
                for (int ix = left; ix <= right; ix++)
                {
                    for (int iy = top; iy <= bottom; iy++)
                    {
                        // update one of the color bits
                        // it is expected that a transparent pixel RGBA value is 0.
                        var c = 4 * (ix + (iy * width));
                        ref var b = ref data[c + PollutePixelColorIndex];
                        b += (byte)(amount * (0xFF - b));
                    }
                }
            }
        }
    }

    private static void CleanPollutedPixels(Span<byte> data, byte blue, byte green, byte red)
    {
        for (int i = 0; i < data.Length; i += 4)
        {
            // only clean if the current pixel isn't transparent
            if (data[i + 3] != 0)
                continue;

            // grab the transparency from the donor byte
            var transparency = data[i + PollutePixelColorIndex];
            if (transparency == 0)
                continue;

            data[i + 0] = blue;
            data[i + 1] = green;
            data[i + 2] = red;
            data[i + 3] = transparency;
        }
    }


}