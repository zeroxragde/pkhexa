using PKHeX.Core;
using PkHexA.Services;
using PkHexA.Views.Pickers;

namespace PkHexA.Views;

public partial class PkmEditor : ContentPage
{
    // Variable para guardar el ID actual (para cuando guardes el archivo .pkm)
    private int _currentSpeciesId;
    private PKM _pkmActual;
    // Variable para guardar las formas disponibles del Pokémon actual
    private string[] _formasDisponibles = Array.Empty<string>();

    public PkmEditor()
	{
		InitializeComponent();
		//Title = LanguageService.Get("winPkmEditor");
       // tabInicioEditPkm.Text = LanguageService.Get("tabPkmEditInicio");
       // lblPokemonSeleccionado.Text = LanguageService.Get("lblPokemonSeleccionado");
        AutoTraductor.Traducir(this);
        var speciesList = GameInfo.Sources.SpeciesDataSource.ToList();

    }
    private void RestaurarTabs()
    {
        // 1. Poner todos los botones en estilo "Apagado/Gris"
        // Asumo que tienes un estilo llamado 'TabButtonStyle' en tu XAML
        btnTabInicio.Style = (Style)Resources["TabButtonStyle"];
        btnTabEncuentro.Style = (Style)Resources["TabButtonStyle"];

        // Si tienes más botones (Estadísticas, Movimientos), agrégalos aquí:
        // btnTabEstadisticas.Style = (Style)Resources["TabButtonStyle"];

        // 2. Ocultar todos los contenidos
        TabInicio.IsVisible = false;
        TabEncuentro.IsVisible = false;

        // TabEstadisticas.IsVisible = false;
    }
    private void OnTabClicked(object sender, EventArgs e)
    {
        var botonPresionado = sender as Button;
        if (botonPresionado == null) return;

        // 1. Apagamos todo primero
        RestaurarTabs();

        // 2. Encendemos visualmente el botón que se tocó (Azul/Brillante)
        botonPresionado.Style = (Style)Resources["TabButtonSelectedStyle"];

        // 3. Mostramos el contenido correspondiente según el botón
        if (botonPresionado == btnTabInicio)
        {
            TabInicio.IsVisible = true;
        }
        else if (botonPresionado == btnTabEncuentro)
        {
            TabEncuentro.IsVisible = true;
        }
        // else if (botonPresionado == btnTabEstadisticas)
        // {
        //     TabEstadisticas.IsVisible = true;
        // }
    }
    public void CargarDatos(PKM pkm)
    {
        // 1. Guardamos la referencia global
        _pkmActual = pkm;

        // 2. DETECTAR SI ES NUEVO O EXISTENTE
        if (pkm.Species == 0)
        {
            // CASO A: Es un espacio vacío o Nuevo
            // Le ponemos valores por defecto para empezar a editar
            FijarPokemon(1); // Empezamos con Bulbasaur (ID 1)

            // Valores default
            NicknameEntry.Text = "Bulbasaur";
            LevelEntry.Text = "1";
            ExpEntry.Text = "0";
            FriendshipEntry.Text = "70";
        }
        else
        {
            // CASO B: Es un Pokémon existente
            // 1. Cargamos la especie y formas (esto actualiza la imagen y labels)
            FijarPokemon(pkm.Species);

            // 2. Llenamos los campos de texto con los datos reales
            NicknameEntry.Text = pkm.Nickname;
            LevelEntry.Text = pkm.CurrentLevel.ToString();
            ExpEntry.Text = pkm.EXP.ToString();
            FriendshipEntry.Text = pkm.CurrentFriendship.ToString();

            // 3. Cargamos valores de los Pickers
            // (Asumiendo que los índices coinciden)
            GenderPicker.SelectedIndex = pkm.Gender;

            // Nota: Para Naturaleza e Idioma, necesitas buscar el índice en tu lista
            // NaturePicker.SelectedIndex = pkm.Nature;
        }

        // 4. Forzar actualización visual de forma y habilidad
        ActualizarFormas();
        //FijarHabilidad(pkm.Ability);
    }
    // AL HACER CLICK EN EL "PICKER FALSO"

    private async void AbrirBuscador_Tapped(object sender, TappedEventArgs e)
    {
        var searchPage = new PokemonSearchPage();

        // CONECTAMOS EL CABLE
        searchPage.AlSeleccionarPokemon = (item) =>
        {
            // "item" es tu objeto de la base de datos
            // Asumiendo que tiene propiedades Text y Value:

            // 1. Poner el nombre en el Label visual
            lblPokemonSeleccionado.Text = item.Text;
            FijarPokemon(item.Value);
            // 2. (Opcional) Usar el ID para lógica interna
            // int idPokemon = item.Value; 
            // Console.WriteLine($"Seleccionó ID: {idPokemon}");
        };

        await Navigation.PushModalAsync(searchPage);
    }
    public void FijarPokemon(int speciesId)
    {
        if (_pkmActual == null) return;

        _pkmActual.Species = (ushort)speciesId; 
        _pkmActual.Form = 0; // SIEMPRE resetear forma al cambiar de bicho

        // 1. Le preguntamos al experto (PokemonSearchPage) quién es este ID
        var info = PokemonSearchPage.ObtenerInfoPokemon(speciesId);

        if (info != null)
        {
            // 2. Actualizamos la pantalla
            // Usamos (dynamic) para acceder a .Text sin líos
            lblPokemonSeleccionado.Text = ((dynamic)info).Text;
            NicknameEntry.Text = "";  // Limpiamos el nickname al cambiar de especie
            // 3. Guardamos el ID internamente
            _currentSpeciesId = speciesId;
            // AGREGAR ESTO:
            ActualizarFormas(); // Recalcular formas para la nueva especie
        }
        else
        {
            lblPokemonSeleccionado.Text = $"Desconocido ({speciesId})";
            _currentSpeciesId = speciesId;
        }
    }

    private async void AbrirBuscadorEstadoNaturaleza_Tapped(object sender, TappedEventArgs e)
    {
        var searchPage = new NaturalezaSearchPage();
        // CONECTAMOS EL CABLE
        searchPage.AlSeleccionarNatura = (item) =>
        {
            // 1. Poner el nombre en el Label visual
            lblNaturalezaEstadoSeleccionado.Text = item.Text;
        };
        await Navigation.PushModalAsync(searchPage);
    }

    private async void AbrirBuscadorNaturaleza_Tapped(object sender, TappedEventArgs e)
    {
        var searchPage = new NaturalezaSearchPage();
        // CONECTAMOS EL CABLE
        searchPage.AlSeleccionarNatura = (item) =>
        {
            // 1. Poner el nombre en el Label visual
            lblNaturalezaSeleccionado.Text = item.Text;
        };
        await Navigation.PushModalAsync(searchPage);
    }

    private async void AbrirBuscadorHabilidad_Tapped(object sender, TappedEventArgs e)
    {
        var searchPage = new HabilidadSearchPage();
        // CONECTAMOS EL CABLE
        searchPage.AlSeleccionarHabilidad = (item) =>
        {
            // 1. Poner el nombre en el Label visual
            lblHabilidadSeleccionado.Text = item.Text;
        };
        await Navigation.PushModalAsync(searchPage);
    }

    /// <summary>
    /// Calcula si el Pokémon actual tiene formas y actualiza la UI.
    /// </summary>
    private void ActualizarFormas()
    {
        if (_pkmActual == null) return;

        // 1. PREGUNTAMOS AL EXPERTO (Tu método estático en FormSearchPage)
        // Esto usa PKHeX internamente para saber si hay formas válidas
        _formasDisponibles = FormaSearchPage.ObtenerFormasDelPokemon(_pkmActual);

        // 2. DECIDIR VISIBILIDAD
        // Si la lista tiene más de 1 elemento (ej: Normal, Attack, Defense...), mostramos el botón.
        bool tieneFormas = _formasDisponibles.Length > 1;

        LayoutForma.IsVisible = tieneFormas;

        // 3. ACTUALIZAR TEXTO
        if (tieneFormas)
        {
            // Verificamos que el índice de forma actual sea válido
            if (_pkmActual.Form < _formasDisponibles.Length)
            {
                lblFormaSeleccionada.Text = _formasDisponibles[_pkmActual.Form];
            }
            else
            {
                // Si el archivo trae una forma inválida, forzamos la 0
                _pkmActual.Form = 0;
                lblFormaSeleccionada.Text = _formasDisponibles[0];
            }
        }
        else
        {
            // Si no tiene formas, aseguramos que sea 0 internamente
            _pkmActual.Form = 0;
        }
    }
    /// <summary>
    /// Abre la ventana para seleccionar la forma.
    /// </summary>
    private async void AbrirBuscadorForma_Tapped(object sender, TappedEventArgs e)
    {
        if (_formasDisponibles.Length <= 1) return;

        var page = new FormaSearchPage();

        // IMPORTANTE: Le pasamos la lista que ya calculamos
        page.CargarFormas(_formasDisponibles);

        page.AlSeleccionarForma = (item) =>
        {
            int idForma = item.Value;
            string nombreForma = item.Text;

            // A. Actualizar UI
            lblFormaSeleccionada.Text = nombreForma;

            // B. Actualizar Pokémon
            if (_pkmActual != null)
            {
                _pkmActual.Form = (byte)idForma;

                // Ojo: Cambiar de forma a veces cambia stats base o tipos (ej: Rotom)
                // Aquí podrías llamar a un método para refrescar la imagen del sprite si la tuvieras
            }
        };

        await Navigation.PushModalAsync(page);
    }

    private void AbrirBuscadorLugarHuevo_Tapped(object sender, TappedEventArgs e)
    {

    }

    private void AbrirBuscadorBall_Tapped(object sender, TappedEventArgs e)
    {

    }

    private void AbrirBuscadorLugar_Tapped(object sender, TappedEventArgs e)
    {

    }

    private void AbrirBuscadorBatalla_Tapped(object sender, TappedEventArgs e)
    {

    }

    private void AbrirBuscadorJuego_Tapped(object sender, TappedEventArgs e)
    {

    }
}