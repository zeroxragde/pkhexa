using PKHeX.Core;

namespace PkHexA.Views.Pickers;

public partial class GamesSearchPage : ContentPage
{
    private List<dynamic>? _todosLosJuegos;
    public Action<dynamic>? AlSeleccionarJuego;

    public GamesSearchPage()
    {
        InitializeComponent();
        CargarDatos();
    }

    private void CargarDatos()
    {
        // Obtenemos la lista. Sus elementos tienen .Text (string) y .Value (int)
        var rawList = GameInfo.Sources.VersionDataSource.ToList();
        _todosLosJuegos = rawList.Cast<dynamic>().ToList();
        ListaGamePokemon.ItemsSource = _todosLosJuegos;
    }

    // MÉTODO PRESELECCIONAR
    // Recibe GameVersion porque así viene del SaveFile
    public void Preseleccionar(GameVersion versionActual)
    {
        if (_todosLosJuegos == null) return;

        // Convertimos el Enum a int para buscarlo en la lista
        int idBuscado = (int)versionActual;

        var item = _todosLosJuegos.FirstOrDefault(x => x.Value == idBuscado);

        if (item != null)
        {
            ListaGamePokemon.SelectedItem = item;
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Task.Delay(100);
                ListaGamePokemon.ScrollTo(item, -1, ScrollToPosition.Center, animate: false);
            });
        }
    }

    // MÉTODO OBTENER JUEGO (Estático)
    // Recibe GameVersion y devuelve el objeto con el nombre
    public static dynamic? ObtenerJuego(GameVersion idGame)
    {
        var lista = GameInfo.Sources.VersionDataSource;

        // 🔥 CORRECCIÓN CLAVE:
        // Convertimos (int)idGame para poder compararlo con p.Value (que es int)
        return lista.FirstOrDefault(p => p.Value == (int)idGame);
    }

    private async void OnSeleccionado(object sender, SelectionChangedEventArgs e)
    {
        var item = e.CurrentSelection.FirstOrDefault();
        if (item != null)
        {
            AlSeleccionarJuego?.Invoke((dynamic)item);
            await Navigation.PopModalAsync();
        }
    }

    private void OnBusquedaChanged(object sender, TextChangedEventArgs e)
    {
        var texto = e.NewTextValue?.ToLower() ?? "";
        if (_todosLosJuegos == null) return;

        if (string.IsNullOrWhiteSpace(texto))
            ListaGamePokemon.ItemsSource = _todosLosJuegos;
        else
            ListaGamePokemon.ItemsSource = _todosLosJuegos
                .Where(p => p.Text.ToLower().Contains(texto))
                .ToList();
    }
}