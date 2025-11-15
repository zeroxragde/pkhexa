using SkiaSharp;

namespace PKHeX.Drawing
{
    /// <summary>
    /// Utility class for manipulating <see cref="SKColor"/> values.
    /// </summary>
    public static class ColorUtil
    {
        private const byte MaxStat = 180;
        private const byte MinStat = 0;

        private const byte ShiftDownBST = 175;
        private const float ShiftDivBST = 3;

        /// <summary>
        /// Gets the <see cref="SKColor"/> for a single stat value.
        /// </summary>
        public static SKColor ColorBaseStat(int stat)
        {
            float x = (uint)stat >= MaxStat ? 1f : ((float)stat) / MaxStat;
            return GetPastelRYG(x);
        }

        /// <summary>
        /// Gets the <see cref="SKColor"/> for a BST (Base Stat Total).
        /// </summary>
        public static SKColor ColorBaseStatTotal(int bst)
        {
            var sumToSingle = Math.Max(MinStat, bst - ShiftDownBST) / ShiftDivBST;
            return ColorBaseStat((int)sumToSingle);
        }

        /// <summary>
        /// Gets a pastel color from Red → Yellow → Green blended with white.
        /// </summary>
        public static SKColor GetPastelRYG(float x)
        {
            float r = x > .5f ? 510f * (1 - x) : 255f;
            float g = x > .5f ? 255f : 510f * x;

            const float white = 0.4f;
            byte b = (byte)(255 * (1 - white));

            byte br = (byte)((r * white) + b);
            byte bg = (byte)((g * white) + b);

            return new SKColor(br, bg, b);
        }

        /// <summary>
        /// Blends two <see cref="SKColor"/> values.
        /// </summary>
        public static SKColor Blend(SKColor color, SKColor backColor, double amount)
        {
            byte r = MixComponent(color.Red, backColor.Red, amount);
            byte g = MixComponent(color.Green, backColor.Green, amount);
            byte b = MixComponent(color.Blue, backColor.Blue, amount);

            return new SKColor(r, g, b);
        }

        /// <summary>
        /// Mixes two byte components with weight.
        /// </summary>
        public static byte MixComponent(byte a, byte b, double x)
        {
            return (byte)((a * x) + (b * (1 - x)));
        }
    }
}
