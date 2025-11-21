using PKHeX.Core;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace PkHexA.LibSprites.Util
{
    public static class TypeColor
    {
        public static SKColor GetTypeSpriteColor(byte type) =>
            ((MoveType)type).GetTypeSpriteColor();

        public static SKColor GetTypeSpriteColor(this MoveType type) => type switch
        {
            MoveType.Normal => new SKColor(159, 161, 159),
            MoveType.Fighting => new SKColor(255, 128, 0),
            MoveType.Flying => new SKColor(129, 185, 239),
            MoveType.Poison => new SKColor(143, 65, 203),
            MoveType.Ground => new SKColor(145, 81, 33),
            MoveType.Rock => new SKColor(175, 169, 129),
            MoveType.Bug => new SKColor(145, 161, 25),
            MoveType.Ghost => new SKColor(112, 65, 112),
            MoveType.Steel => new SKColor(96, 161, 184),
            MoveType.Fire => new SKColor(230, 40, 41),
            MoveType.Water => new SKColor(41, 128, 239),
            MoveType.Grass => new SKColor(63, 161, 41),
            MoveType.Electric => new SKColor(250, 192, 0),
            MoveType.Psychic => new SKColor(239, 65, 121),
            MoveType.Ice => new SKColor(63, 216, 255),
            MoveType.Dragon => new SKColor(80, 97, 225),
            MoveType.Dark => new SKColor(80, 65, 63),
            MoveType.Fairy => new SKColor(239, 113, 239),

            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
        };

        /// <summary>
        /// Color to show for a <see cref="MoveType"/> of <see cref="TeraTypeUtil.Stellar"/>.
        /// </summary>
        public static SKColor Stellar => SKColors.LightYellow;

        /// <summary>
        /// Gets the color of a <see cref="MoveType"/> for a Tera sprite.
        /// </summary>
        public static SKColor GetTeraSpriteColor(byte elementalType)
        {
            if (elementalType == TeraTypeUtil.Stellar)
                return Stellar;

            return GetTypeSpriteColor(elementalType);
        }

    }
}
