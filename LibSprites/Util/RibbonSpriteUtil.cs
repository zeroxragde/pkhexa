using PKHeX.Core;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace PkHexA.LibSprites.Util
{
    /// <summary>
    /// Provides utility methods for retrieving ribbon sprite images.
    /// </summary>
    public static class RibbonSpriteUtil
    {
        /// <summary>
        /// Gets the ribbon sprite image for the specified <see cref="RibbonIndex"/>.
        /// </summary>
        public static SKBitmap? GetRibbonSprite(RibbonIndex ribbon)
        {
            var name = $"Ribbon{ribbon}";
            return GetRibbonSprite(name);
        }
        public static SKBitmap? GetRibbonSprite(string name)
        {
            var resource = name.Replace("CountG3", "G3").ToLowerInvariant();
            return SpriteImgLoader.LoadSprite(resource);
        }

        public static SKBitmap? GetRibbonSprite(string name, int max, int value)
        {
            var resource = GetRibbonSpriteName(name, max, value);
            return SpriteImgLoader.LoadSprite(resource);
        }
        private static string GetRibbonSpriteName(string name, int max, int value)
        {
            if (max != 4) // Memory
            {
                var sprite = name.ToLowerInvariant();
                if (value >= max)
                    return sprite + "2";
                return sprite;
            }

            // Count ribbons
            string n = name.Replace("Count", string.Empty).ToLowerInvariant();
            return value switch
            {
                2 => n + "super",
                3 => n + "hyper",
                4 => n + "master",
                _ => n,
            };
        }

    }
}
