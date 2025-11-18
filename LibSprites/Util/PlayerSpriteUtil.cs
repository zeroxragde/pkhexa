using PKHeX.Core;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace PkHexA.LibSprites.Util
{
    /// <summary>
    /// Provides utility methods for retrieving player sprite images from save files.
    /// </summary>
    public static class PlayerSpriteUtil
    {

        /// <summary>
        /// Gets the player sprite image for the specified <see cref="SaveFile"/>.
        /// </summary>
        /// <param name="sav">The save file to get the player sprite for.</param>
        /// <returns>An <see cref="SKBitmap"/> representing the player sprite, or null if not available.</returns>
        public static SKBitmap? Sprite(this SaveFile sav)
        {
            return GetSprite(sav);
        }

        private static SKBitmap? GetSprite(SaveFile sav)
        {
            if (sav is IMultiplayerSprite ms)
            {
                string file = $"tr_{ms.MultiplayerSpriteID:00}";

                // Intenta cargar el PNG desde las carpetas que ya tienes definidas
                var bmp = SpriteImgLoader.LoadSprite(file);

                // Si no existe, carga el fallback tr_00
                return bmp ?? SpriteImgLoader.LoadSprite("tr_00");
            }

            return null;
        }
    }
}
