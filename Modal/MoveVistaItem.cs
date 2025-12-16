using System;
using System.Collections.Generic;
using System.Text;

namespace PkHexA.Modal
{
    public class MoveVistaItem
    {
        public ushort Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public ImageSource TipoImagen { get; set; } = null;
    }
}
