using PKHeX.Core;

namespace PkHexA.Views;

public partial class pkmEditorPro : ContentPage
{
    // Referencia al Pokémon que estamos editando
    private PKM? _pkm;

    // Colores para el efecto visual de las pestañas
    private readonly Color ColorTabActivo = Color.FromArgb("#512BD4");
    private readonly Color ColorTabInactivo = Colors.Transparent;
    private readonly Color TextoActivo = Colors.White;
    private readonly Color TextoInactivo = Colors.Gray;

    public pkmEditorPro()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Método llamado desde fuera para iniciar la edición.
    /// </summary>
    public void CargarDatos(PKM pkm)
    {
        _pkm = pkm;

        if (_pkm == null) return;

        // 1. Si es un Pokémon vacío, ponle datos por defecto
        if (_pkm.Species == 0)
        {
            _pkm.Species = 1; // Bulbasaur
            _pkm.Nickname = "Bulbasaur";
            _pkm.CurrentLevel = 1;
            _pkm.Version = GameVersion.SW;
            _pkm.Language = 2; // Inglés
            _pkm.Ball = 4;     // Poké Ball
        }

        // 2. Pasar el Pokémon a la pestaña de Inicio
        if (ViewTabInicio != null) ViewTabInicio.CargarDatos(_pkm);
        if (ViewTabEncuentro != null) ViewTabEncuentro.CargarDatos(_pkm);
        if (ViewTabStats != null) ViewTabStats.CargarDatos(_pkm);
        if (ViewTabMoves != null) ViewTabMoves.CargarDatos(_pkm);
        if (ViewTabCosmetic != null) ViewTabCosmetic.CargarDatos(_pkm);



        // 3. Marcar visualmente el primer botón como activo
        if (StackBotones.Children.Count > 0 && StackBotones.Children[0] is Button btnInicio)
        {
            ActualizarEstiloBotones(btnInicio);
        }
    }

    /// <summary>
    /// Maneja el cambio de pestaña al hacer clic en los botones superiores.
    /// </summary>
    private void OnTabClicked(object sender, EventArgs e)
    {
        var boton = sender as Button;
        if (boton == null) return;

        // A. Actualizar colores de los botones
        ActualizarEstiloBotones(boton);

        // B. Mostrar el panel correspondiente (Vinculado por CommandParameter en el XAML)
        var vistaDestino = boton.CommandParameter as View;

        if (vistaDestino != null)
        {
            // Ocultar todos los paneles hermanos
            if (vistaDestino.Parent is Grid gridPadre)
            {
                foreach (var hijo in gridPadre.Children)
                {
                    if (hijo is View v) v.IsVisible = false;
                }
            }
            // Mostrar solo el elegido
            vistaDestino.IsVisible = true;
        }
    }

    // Helper para pintar los botones
    private void ActualizarEstiloBotones(Button botonActivo)
    {
        if (StackBotones == null) return;

        foreach (var hijo in StackBotones.Children)
        {
            if (hijo is Button btn)
            {
                bool esElActivo = (btn == botonActivo);
                btn.BackgroundColor = esElActivo ? ColorTabActivo : ColorTabInactivo;
                btn.TextColor = esElActivo ? TextoActivo : TextoInactivo;
            }
        }
    }

    // Helper para obtener el PKM editado al guardar
    public PKM ObtenerPkmEditado()
    {
        return _pkm;
    }
}