using PKHeX.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace PkHexA.Modal
{
    public class BoxSlot
    {
        public int Index { get; set; }
        public PKM? Pokemon { get; set; }
        public string DisplayText { get; set; } = "";
        public Color BackgroundColor { get; set; } = Colors.Transparent;

        // Para resaltar la selección
        public Color BorderColor { get; set; } = Color.FromArgb("#555");
        public double BorderThickness { get; set; } = 1;
    }
}
