using PKHeX.Core;
using PkHexA.Services;

namespace PkHexA.Views.Pickers;

public partial class NaturalezaSearchPage : ContentPage
{
    // Variable para guardar la lista completa original
    private List<dynamic>? _todasLasNaturalezas;
    public Action<dynamic>? AlSeleccionarNatura;
    public NaturalezaSearchPage()
	{
		InitializeComponent();
        CargarDatos();
    }
    private void CargarDatos()
    {
  
        // 2. AQUÍ METEMOS TU LÍNEA DE DATOS REALES
        // Convertimos a List<dynamic> para manipularlo fácil aquí
        var rawList = GameInfo.Sources.NatureDataSource.ToList();

        // Guardamos la copia completa en memoria
        _todasLasNaturalezas = rawList.Cast<dynamic>().ToList();

        // Mostramos todo al inicio
        ListaNaturalezaPokemon.ItemsSource = _todasLasNaturalezas = rawList.Cast<dynamic>().ToList();
        
    }
    // ? NUEVO MÉTODO ESTÁTICO (Accesible desde cualquier parte de la app)
    // Le das un ID (6) y te devuelve el Objeto (Charizard)
    public static dynamic? ObtenerNaturaleza(int idNatura)
    {
        // Usamos la misma fuente de datos que usa la lista visual
        var lista = GameInfo.Sources.NatureDataSource;

        // Buscamos el que coincida
        var pokemon = lista.FirstOrDefault(p => p.Value == idNatura);

        return pokemon; // Devuelve el objeto o null si no existe
    }

    // 3. FILTRADO (Ajustado para objetos)
    private void OnBusquedaChanged(object sender, TextChangedEventArgs e)
    {
        var textoBusqueda = e.NewTextValue?.ToLower() ?? "";

        if (string.IsNullOrWhiteSpace(textoBusqueda))
        {
            // Si borran el texto, regresamos a la lista completa
            ListaNaturalezaPokemon.ItemsSource = _todasLasNaturalezas;
        }
        else
        {
            // FILTRAMOS BUSCANDO EN LA PROPIEDAD '.Text'
            // (Asumiendo que tu objeto tiene una propiedad 'Text')
            var filtrados = _todasLasNaturalezas
                .Where(p => p.Text.ToLower().Contains(textoBusqueda))
                .ToList();

            ListaNaturalezaPokemon.ItemsSource = filtrados;
        }
    }

    // 4. SELECCIÓN
    private async void OnNaturaSeleccionado(object sender, SelectionChangedEventArgs e)
    {
        var itemSeleccionado = e.CurrentSelection.FirstOrDefault();

        if (itemSeleccionado != null)
        {
            // Disparamos la acción devolviendo TODO el objeto (Texto y Valor)
            AlSeleccionarNatura?.Invoke(itemSeleccionado);

            await Navigation.PopModalAsync();
        }
    }
}