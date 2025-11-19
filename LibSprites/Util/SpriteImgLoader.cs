using PkHexA.Services;
using SkiaSharp;

namespace PkHexA.LibSprites.Util
{
    public static class SpriteImgLoader
    {
        private static string Root => SpriteDataInitializerService.GetLocalSpritesRoot();

        private static readonly string[] SearchPaths =
        {
            "sprites",
            "sprites/accents",
            "sprites/ball",
            "sprites/big_items",
            "sprites/big_pokemon_sprites",
            "sprites/big_shiny_sprites",
            "sprites/artwork_items",
            "sprites/artwork_pokemon_sprites",
            "sprites/legends_arceus_sprites",
            "sprites/legends_arceus_shiny_sprites",
            "sprites/pokemon_sprite_overlays",
            "sprites/status",
            "img",
            "img/box",
            "img/misc",
            "img/ribbons",
            "img/trainer",
            "img/types"
        };

        public static SKBitmap CreateFallback()
        {
            var bmp = new SKBitmap(68, 68);
            using var canvas = new SKCanvas(bmp);
            canvas.Clear(SKColors.Magenta);
            return bmp;
        }

        public static SKBitmap? LoadSprite(string filename)
        {
            string file = $"{filename}.png";

            foreach (var rel in SearchPaths)
            {
                string full = Path.Combine(Root, rel, file);

                Console.WriteLine("[SEARCH] " + full);

                if (File.Exists(full))
                {
                    using var fs = File.OpenRead(full);
                    return SKBitmap.Decode(fs);
                }
            }

            return null;
        }

        public static SKBitmap? LoadBitmapFromResource(string filename)
        {
            string primary = Path.Combine(Root, "sprites", $"{filename}.png");

            if (File.Exists(primary))
            {
                using var fs = File.OpenRead(primary);
                return SKBitmap.Decode(fs);
            }

            return LoadSprite(filename);
        }
    }
}
