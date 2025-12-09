using System;
using System.Collections.Generic;
using System.Text;

namespace PkHexA.Modal
{
    public class MoveItem
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public ImageSource? TypeImage { get; set; }
        public string? SearchString { get; set; } // Para búsqueda optimizada (minúsculas)
    }
}
