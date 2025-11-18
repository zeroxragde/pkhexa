using PKHeX.Core;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace PkHexA.LibSprites.Util
{
    /// <summary>
    /// Utility class for getting the color of a <see cref="StatusCondition"/>.
    /// </summary>
    public static class StatusColor
    {
        public static SKColor Sleep => new SKColor(200, 200, 200);
        public static SKColor Freeze => new SKColor(0, 255, 255);
        public static SKColor Paralysis => new SKColor(255, 255, 0);
        public static SKColor Burn => new SKColor(255, 0, 0);
        public static SKColor Poison => new SKColor(128, 0, 255);
        public static SKColor PoisonBad => new SKColor(200, 0, 255);
        public static SKColor None => new SKColor(255, 255, 255, 255);
        /// <summary>
        /// Gets the color of a <see cref="StatusCondition"/>.
        /// </summary>
        /// <param name="value">Status to get the color of.</param>
        /// <returns>Color of the status.</returns>
        public static SKColor GetStatusColor(int value) => ((StatusCondition)value).GetStatusColor();

        /// <inheritdoc cref="GetStatusColor(int)"/>
        public static SKColor GetStatusColor(this StatusType value) => value switch
        {
            StatusType.None => None,
            StatusType.Sleep => Sleep,
            StatusType.Freeze => Freeze,
            StatusType.Paralysis => Paralysis,
            StatusType.Burn => Burn,
            StatusType.Poison => Poison,
            _ => None,
        };

        /// <inheritdoc cref="GetStatusColor(int)"/>
        public static SKColor GetStatusColor(this PKM pk) => ((StatusCondition)pk.Status_Condition).GetStatusColor();

        /// <inheritdoc cref="GetStatusColor(int)"/>
        public static SKColor GetStatusColor(this StatusCondition value)
        {
            if (value == StatusCondition.None)
                return None;
            if (value < StatusCondition.Poison)
                return Sleep;
            if (value.HasFlag(StatusCondition.Poison))
                return Poison;
            if (value.HasFlag(StatusCondition.Freeze))
                return Freeze;
            if (value.HasFlag(StatusCondition.Paralysis))
                return Paralysis;
            if (value.HasFlag(StatusCondition.Burn))
                return Burn;
            if (value.HasFlag(StatusCondition.PoisonBad))
                return PoisonBad;
            return default;
        }

    }
}
