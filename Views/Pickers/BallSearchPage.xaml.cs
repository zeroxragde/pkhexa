using PKHeX.Core;

namespace PkHexA.Views.Pickers;

public partial class BallSearchPage : ContentPage
{
    private List<dynamic>? _todasLasBalls;
    public Action<dynamic>? AlSeleccionarBall;

    public BallSearchPage()
    {
        InitializeComponent();
        CargarDatos();
    }

    private void CargarDatos()
    {
        // 1. Obtenemos la lista de nombres del Core (En Español)
        // Nota: En versiones nuevas es 'BallList' (Mayúscula)
        string[] nombresBalls = GameInfo.Sources.BallDataSource.ToList().Select(b => b.Text).ToArray();

        var listaProcesada = new List<dynamic>();

        for (int i = 0; i < nombresBalls.Length; i++)
        {
            // Filtramos nombres vacíos o inválidos
            if (string.IsNullOrEmpty(nombresBalls[i]) || nombresBalls[i].Contains("???")) continue;

            // 2. Generamos la URL del ícono
            // Usamos Serebii o PokeAPI que tienen los sprites por ID
            // Serebii usa nombres, así que usamos un repo de github confiable por ID
            // O PokeAPI: https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/items/poke-ball.png
            // Pero PokeAPI usa nombres en inglés.

            // TRUCO: Usamos un repositorio de Sprites de Items genérico o local si tienes los assets.
            // Si no tienes assets locales, usamos una URL de placeholder que funciona por ID si existe, 
            // o simplemente mostramos el nombre si no queremos depender de internet.

            // Para este ejemplo, supongamos que solo mostramos texto por ahora para que no falle,
            // o usamos una URL genérica de Pokéball para todas si no tienes los IDs mapeados.

            // Si quieres usar imágenes reales, necesitarías un mapeo ID -> NombreInglés para PokeAPI.
            // Por simplicidad y robustez offline, usaremos una imagen local "ball.png" si tienes,
            // o una URL externa de prueba.

            string urlImagen = $"https://raw.githubusercontent.com/msikma/pokesprite/master/items/ball/{GetBallFileName(i)}.png";

            listaProcesada.Add(new
            {
                Text = nombresBalls[i],
                Value = i,
                ImageUrl = urlImagen // Binding para la imagen
            });
        }

        _todasLasBalls = listaProcesada;
        ListaBalls.ItemsSource = _todasLasBalls;
    }

    // Helper simple para mapear IDs comunes a nombres de archivo (Opcional)
    // Si no quieres complicarte con esto, usa una imagen fija por defecto.
    private string GetBallFileName(int id)
    {
        // Esto es un ejemplo simplificado. Lo ideal sería tener los recursos locales.
        return id switch
        {
            1 => "master",
            2 => "ultra",
            3 => "great",
            4 => "poke",
            _ => "poke" // Fallback a pokebola normal
        };
    }

    public void Preseleccionar(int ballId)
    {
        if (_todasLasBalls == null) return;
        var item = _todasLasBalls.FirstOrDefault(x => x.Value == ballId);
        if (item != null)
        {
            ListaBalls.SelectedItem = item;
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Task.Delay(100);
                ListaBalls.ScrollTo(item, -1, ScrollToPosition.Center, animate: false);
            });
        }
    }

    private async void OnBallSeleccionada(object sender, SelectionChangedEventArgs e)
    {
        var item = e.CurrentSelection.FirstOrDefault();
        if (item != null)
        {
            AlSeleccionarBall?.Invoke((dynamic)item);
            await Navigation.PopModalAsync();
        }
        ((CollectionView)sender).SelectedItem = null;
    }

    private void OnBusquedaChanged(object sender, TextChangedEventArgs e)
    {
        var texto = e.NewTextValue?.ToLower() ?? "";
        if (_todasLasBalls == null) return;

        if (string.IsNullOrWhiteSpace(texto))
            ListaBalls.ItemsSource = _todasLasBalls;
        else
            ListaBalls.ItemsSource = _todasLasBalls.Where(p => p.Text.ToLower().Contains(texto)).ToList();
    }
}