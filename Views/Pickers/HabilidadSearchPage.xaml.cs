using PKHeX.Core;

namespace PkHexA.Views.Pickers;

public partial class HabilidadSearchPage : ContentPage
{    // Variable para guardar la lista completa original
    private List<dynamic>? _todasLasHabilidades;
    public Action<dynamic>? AlSeleccionarHabilidad;
    public HabilidadSearchPage()
	{
		InitializeComponent();
        CargarDatos();
    }
    private void CargarDatos()
    {

        // 2. AQUÍ METEMOS TU LÍNEA DE DATOS REALES
        // Convertimos a List<dynamic> para manipularlo fácil aquí
        var rawList = GameInfo.Sources.AbilityDataSource.ToList();

        // Guardamos la copia completa en memoria
        _todasLasHabilidades = rawList.Cast<dynamic>().ToList();

        // Mostramos todo al inicio
        ListaHabilidadPokemon.ItemsSource = _todasLasHabilidades = rawList.Cast<dynamic>().ToList();

    }
    // ? NUEVO MÉTODO ESTÁTICO (Accesible desde cualquier parte de la app)
    // Le das un ID (6) y te devuelve el Objeto (Charizard)
    public static dynamic? ObtenerHabilidad(int idHabilidad)
    {
        // Usamos la misma fuente de datos que usa la lista visual
        var lista = GameInfo.Sources.AbilityDataSource;

        // Buscamos el que coincida
        var pokemon = lista.FirstOrDefault(p => p.Value == idHabilidad);

        return pokemon; // Devuelve el objeto o null si no existe
    }

    private async void OnHabilidadSeleccionado(object sender, SelectionChangedEventArgs e)
    {
        var itemSeleccionado = e.CurrentSelection.FirstOrDefault();

        if (itemSeleccionado != null)
        {
            // Disparamos la acción devolviendo TODO el objeto (Texto y Valor)
            AlSeleccionarHabilidad?.Invoke(itemSeleccionado);

            await Navigation.PopModalAsync();
        }
    }

    private void OnBusquedaChanged(object sender, TextChangedEventArgs e)
    {
        var textoBusqueda = e.NewTextValue?.ToLower() ?? "";

        if (string.IsNullOrWhiteSpace(textoBusqueda))
        {
            // Si borran el texto, regresamos a la lista completa
            ListaHabilidadPokemon.ItemsSource = _todasLasHabilidades;
        }
        else
        {
            // FILTRAMOS BUSCANDO EN LA PROPIEDAD '.Text'
            // (Asumiendo que tu objeto tiene una propiedad 'Text')
            var filtrados = _todasLasHabilidades
                .Where(p => p.Text.ToLower().Contains(textoBusqueda))
                .ToList();

            ListaHabilidadPokemon.ItemsSource = filtrados;
        }
    }
}