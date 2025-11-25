using PKHeX.Core;
using PkHexA.Services;

namespace PkHexA.Views;

public partial class PokemonSearchPage : ContentPage
{


    // Variable para guardar la lista completa original
    private List<dynamic>? _todosLosPokemon;
    public Action<dynamic>? AlSeleccionarPokemon;

    public PokemonSearchPage()
    {
        InitializeComponent();
        // Al iniciar, mostramos todos
        CargarDatos();
        Title = LanguageService.Get("winPkBuscador");
        BarraBusqueda.Placeholder = LanguageService.Get("txtSearchBoxPlaceholde");
    }


    private void CargarDatos()
    {
        // 2. AQUÍ METEMOS TU LÍNEA DE DATOS REALES
        // Convertimos a List<dynamic> para manipularlo fácil aquí
        var rawList = GameInfo.Sources.SpeciesDataSource.ToList();

        // Guardamos la copia completa en memoria
        _todosLosPokemon = rawList.Cast<dynamic>().ToList();

        // Mostramos todo al inicio
        ListaPokemon.ItemsSource = _todosLosPokemon;
    }
    // Recibe el ID (int) que tiene el Pokémon actualmente
    public void Preseleccionar(int idActual)
    {
        if (_todosLosPokemon == null) return;

        // 1. Buscamos el objeto en la lista que coincida con el ID
        var itemASeleccionar = _todosLosPokemon.FirstOrDefault(x => x.Value == idActual);

        if (itemASeleccionar != null)
        {
            // 2. Marcamos el ítem como seleccionado
            ListaPokemon.SelectedItem = itemASeleccionar;

            // 3. Hacemos Scroll automático para que aparezca en el centro de la pantalla
            // Usamos un pequeño delay para asegurar que la lista ya se dibujó
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Task.Delay(100);
                ListaPokemon.ScrollTo(itemASeleccionar, -1, ScrollToPosition.Center, animate: false);
            });
        }
    }
    // ? NUEVO MÉTODO ESTÁTICO (Accesible desde cualquier parte de la app)
    // Le das un ID (6) y te devuelve el Objeto (Charizard)
    public static dynamic? ObtenerInfoPokemon(int idEspecie)
    {
        // Usamos la misma fuente de datos que usa la lista visual
        var lista = GameInfo.Sources.SpeciesDataSource;

        // Buscamos el que coincida
        var pokemon = lista.FirstOrDefault(p => p.Value == idEspecie);

        return pokemon; // Devuelve el objeto o null si no existe
    }
    // 3. FILTRADO (Ajustado para objetos)
    private void OnBusquedaChanged(object sender, TextChangedEventArgs e)
    {
        var textoBusqueda = e.NewTextValue?.ToLower() ?? "";

        if (string.IsNullOrWhiteSpace(textoBusqueda))
        {
            // Si borran el texto, regresamos a la lista completa
            ListaPokemon.ItemsSource = _todosLosPokemon;
        }
        else
        {
            // FILTRAMOS BUSCANDO EN LA PROPIEDAD '.Text'
            // (Asumiendo que tu objeto tiene una propiedad 'Text')
            var filtrados = _todosLosPokemon
                .Where(p => p.Text.ToLower().Contains(textoBusqueda))
                .ToList();

            ListaPokemon.ItemsSource = filtrados;
        }
    }

    // 4. SELECCIÓN
    private async void OnPokemonSeleccionado(object sender, SelectionChangedEventArgs e)
    {
        var itemSeleccionado = e.CurrentSelection.FirstOrDefault();

        if (itemSeleccionado != null)
        {
            // Disparamos la acción devolviendo TODO el objeto (Texto y Valor)
            AlSeleccionarPokemon?.Invoke(itemSeleccionado);

            await Navigation.PopModalAsync();
        }
    }









}