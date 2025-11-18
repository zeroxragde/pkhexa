using PkHexA.Services;
using SkiaSharp;
using System;
using System.IO;

namespace PkHexA.LibSprites.Util
{
    public static class SpriteImgLoader
    {
        // Ruta raíz REAL donde MAUI descomprime el ZIP:
        // /data/user/0/com.zerox.pkhexa/files/pokehex/pokehex/
        private static string Root =>
            Path.Combine(SpriteDataInitializerService.GetLocalSpritesRoot(), "pokehex");

        // Subcarpetas internas dentro del ZIP
        private static readonly string[] SearchPaths = new[]
        {
            "sprites/",
            "sprites/accents/",
            "sprites/ball/",
            "sprites/big_items/",
            "sprites/big_pokemon_sprites/",
            "sprites/big_shiny_sprites/",
            "sprites/artwork_items/",
            "sprites/artwork_pokemon_sprites/",
            "sprites/legends_arceus_sprites/",
            "sprites/legends_arceus_shiny_sprites/",
            "sprites/pokemon_sprite_overlays/",
            "sprites/status/",

            "img/",
            "img/box/",
            "img/misc/",
            "img/ribbons/",
            "img/trainer/",
            "img/types/"
        };

        // Sprite fallback/debug
        public const int FALLBACK_SIZE = 68;

        public static SKBitmap CreateFallback(int size = FALLBACK_SIZE)
        {
            var bmp = new SKBitmap(size, size);
            using var canvas = new SKCanvas(bmp);
            canvas.Clear(new SKColor(255, 0, 255)); // Magenta
            return bmp;
        }

        /// <summary>
        /// Busca filename.png en todas las subcarpetas del ZIP descomprimido.
        /// </summary>
        public static SKBitmap? LoadSprite(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
                return null;

            string target = $"{filename}.png";

            foreach (var relative in SearchPaths)
            {
                string fullPath = Path.Combine(Root, relative, target);

                // Log para verificar en debug
                Console.WriteLine("[SEARCH] " + fullPath);

                try
                {
                    if (File.Exists(fullPath))
                    {
                        using var fs = File.OpenRead(fullPath);
                        return SKBitmap.Decode(fs);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[ERR] " + ex.Message);
                }
            }

            return null;
        }

        /// <summary>
        /// Intenta primero en 'sprites/', luego usa LoadSprite.
        /// </summary>
        public static SKBitmap? LoadBitmapFromResource(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
                return null;

            string target = $"{filename}.png";
            string primary = Path.Combine(Root, "sprites", target);

            Console.WriteLine("[PRIMARY CHECK] " + primary);

            try
            {
                if (File.Exists(primary))
                {
                    using var fs = File.OpenRead(primary);
                    return SKBitmap.Decode(fs);
                }
            }
            catch { }

            return LoadSprite(filename);
        }
    }
}
