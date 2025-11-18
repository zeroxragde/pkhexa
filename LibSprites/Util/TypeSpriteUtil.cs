using PKHeX.Core;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace PkHexA.LibSprites.Util
{
    /// <summary>
    /// Provides utility methods for retrieving type sprite images for Pokémon types.
    /// </summary>
    public static class TypeSpriteUtil
    {
        private static SKBitmap? Get(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            // Carga desde tus carpetas:
            return SpriteImgLoader.LoadSprite(name);
        }
        /// <summary>
        /// Gets the wide type sprite image for the specified type and generation.
        /// </summary>
        /// <param name="type">The type index.</param>
        /// <param name="generation">The game generation (default: latest).</param>
        /// <returns>A <see cref="SKBitmap"/> representing the wide type sprite, or null if not available.</returns>
        public static SKBitmap? GetTypeSpriteWide(byte type, byte generation = Latest.Generation)
        {
            if (generation <= 2)
                type = (byte)((MoveType)type).GetMoveTypeGeneration(generation);

            return Get($"type_wide_{type:00}");
        }

        /// <summary>
        /// Gets the icon type sprite image for the specified type and generation.
        /// </summary>
        /// <param name="type">The type index.</param>
        /// <param name="generation">The game generation (default: latest).</param>
        /// <returns>A <see cref="SKBitmap"/> representing the icon type sprite, or null if not available.</returns>
        public static SKBitmap? GetTypeSpriteIcon(byte type, byte generation = Latest.Generation)
        {
            if (generation <= 2)
                type = (byte)((MoveType)type).GetMoveTypeGeneration(generation);

            return Get($"type_icon_{type:00}");
        }
        /// <summary>
        /// Gets the small icon type sprite image for the specified type and generation.
        /// </summary>
        /// <param name="type">The type index.</param>
        /// <param name="generation">The game generation (default: latest).</param>
        /// <returns>A <see cref="SKBitmap"/> representing the small icon type sprite, or null if not available.</returns>
        public static SKBitmap? GetTypeSpriteIconSmall(byte type, byte generation = Latest.Generation)
        {
            if (generation <= 2)
                type = (byte)((MoveType)type).GetMoveTypeGeneration(generation);

            return Get($"type_icon_s_{type:00}");
        }
        /// <summary>
        /// Gets the gem type sprite image for the specified type.
        /// </summary>
        /// <param name="type">The type index.</param>
        /// <returns>A <see cref="SKBitmap"/> representing the gem type sprite, or null if not available.</returns>
        public static SKBitmap? GetTypeSpriteGem(byte type)
        {
            return Get($"gem_{type:00}");
        }

    }
}
