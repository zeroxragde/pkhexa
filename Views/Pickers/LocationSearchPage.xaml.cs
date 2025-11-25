using PKHeX.Core;

namespace PkHexA.Views.Pickers;

public partial class LocationSearchPage : ContentPage
{
    // Lista completa para filtrar
    private List<dynamic>? _todosLosLugares;

    // El cable para devolver el resultado
    public Action<dynamic>? AlSeleccionarLugar;
    public LocationSearchPage()
	{
		InitializeComponent();
	}
    /// <summary>
    /// Carga la lista de lugares válida para un juego específico.
    /// </summary>
    /// <param name="version">El juego (ej: Rojo Fuego, Espada)</param>
    /// <param name="format">La generación/formato</param>
    /// <param name="isEggLocation">Si true, carga lugares de huevo (Guardería, etc)</param>
  // ... (variables y constructor igual) ...

    // ?? CORRECCIÓN: Cambiamos 'int format' por 'EntityContext context'
    public void CargarLugares(GameVersion version, EntityContext context, bool isEggLocation)
    {
        // 1. Llamamos al método EXACTO de la foto
        // Sin casteos raros, pasamos los objetos directos
        var lugaresPKHeX = GameInfo.GetLocationList(version, context, isEggLocation);

        var listaProcesada = new List<dynamic>();

        foreach (var lugar in lugaresPKHeX)
        {
            listaProcesada.Add(new
            {
                Text = lugar.Text,
                Value = lugar.Value
            });
        }

        _todosLosLugares = listaProcesada;
        ListaLugares.ItemsSource = _todosLosLugares;
    }

    /// <summary>
    /// Marca el lugar que ya tiene el Pokémon
    /// </summary>
    public void Preseleccionar(int locationId)
    {
        if (_todosLosLugares == null) return;

        // Buscamos por ID
        var item = _todosLosLugares.FirstOrDefault(x => x.Value == locationId);

        if (item != null)
        {
            ListaLugares.SelectedItem = item;

            // Scroll automático
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Task.Delay(100);
                ListaLugares.ScrollTo(item, -1, ScrollToPosition.Center, animate: false);
            });
        }
    }

    // Evento al tocar un lugar
    private async void OnLugarSeleccionado(object sender, SelectionChangedEventArgs e)
    {
        var item = e.CurrentSelection.FirstOrDefault();

        if (item != null)
        {
            // Devolvemos el dato y cerramos
            AlSeleccionarLugar?.Invoke((dynamic)item);
            await Navigation.PopModalAsync();
        }

        ((CollectionView)sender).SelectedItem = null;
    }

    // Buscador en tiempo real
    private void OnBusquedaChanged(object sender, TextChangedEventArgs e)
    {
        var texto = e.NewTextValue?.ToLower() ?? "";
        if (_todosLosLugares == null) return;

        if (string.IsNullOrWhiteSpace(texto))
        {
            ListaLugares.ItemsSource = _todosLosLugares;
        }
        else
        {
            ListaLugares.ItemsSource = _todosLosLugares
                .Where(p => p.Text.ToLower().Contains(texto))
                .ToList();
        }
    }
}