using PKHeX.Core;
using SkiaSharp;
using static PKHeX.Core.GameVersion;


namespace PkHexA.LibSprites.Util
{
    /// <summary>
    /// Provides utility methods for retrieving and composing wallpaper images for Pokémon storage boxes.
    /// </summary>
    public static class WallpaperUtil
    {

        // Antes: Bitmap
        private static SKBitmap DefaultWallpaper =>
            SpriteImgLoader.LoadSprite("box_wp16xy")!;

        /// <summary>
        /// Gets the wallpaper image for the specified save file and box index.
        /// </summary>
        public static SKBitmap WallpaperImage(this SaveFile sav, int box) =>
            GetWallpaper(sav, box);

        private static SKBitmap GetWallpaper(SaveFile sav, int box)
        {
            if (sav is not IBoxDetailWallpaper wp)
                return DefaultWallpaper;

            // ZA usa un wallpaper especial
            if (sav is SAV9ZA)
                return SpriteImgLoader.LoadSprite("box_wp02bdsp") ?? DefaultWallpaper;

            int wallpaper = wp.GetBoxWallpaper(box);
            string name = GetWallpaperResourceName(sav.Version, wallpaper);

            return SpriteImgLoader.LoadSprite(name) ?? DefaultWallpaper;
        }

        /// <summary>
        /// Gets the resource name for the wallpaper image based on game version and index.
        /// </summary>
        public static string GetWallpaperResourceName(GameVersion version, int index)
        {
            index++; // start at 1
            var suffix = GetResourceSuffix(version, index);

            var variant = version switch
            {
                SL when index is 20 => "_n",
                VL when index is 20 => "_u",
                _ => string.Empty
            };

            return $"box_wp{index:00}{suffix}{variant}";
        }

        private static string GetResourceSuffix(GameVersion version, int index) =>
            version.GetGeneration() switch
            {
                3 when version == E => "e",
                3 when FRLG.Contains(version) && index > 12 => "frlg",
                3 => "rs",

                4 when index < 16 => "dp",
                4 when version == Pt => "pt",
                4 when HGSS.Contains(version) => "hgss",

                5 => B2W2.Contains(version) && index > 16 ? "b2w2" : "bw",
                6 => ORAS.Contains(version) && index > 16 ? "ao" : "xy",
                7 when !GG.Contains(version) => "xy",
                8 when !SWSH.Contains(version) => "bdsp",
                8 => "swsh",
                9 => "sv",
                _ => string.Empty
            };


    }
}
