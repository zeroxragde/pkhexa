using PKHeX.Core;
using PkHexA.LibSprites.Util;
using PkHexA.Services;

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
        // 1. Obtenemos la lista de nombres del Core
        var ballSource = GameInfo.Sources.BallDataSource;
        // Convertimos a lista para acceder por índice o valor fácilmente
        var listaBalls = ballSource.ToList();

        var listaProcesada = new List<dynamic>();

        // Iteramos sobre la fuente de datos real
        foreach (var ballInfo in listaBalls)
        {
            int id = ballInfo.Value;
            string nombre = ballInfo.Text;

            // Filtros básicos (evitar nombres vacíos o placeholders de PKHeX)
            if (string.IsNullOrEmpty(nombre) || nombre.Contains("???")) continue;

            // 2. CORRECCIÓN: Usar el sistema de sprites local
            // Obtenemos el SKBitmap desde tu utilidad
            var bitmap = SpriteUtil.GetBallSprite((byte)id);
            // Usamos el método de extensión directamente sobre el objeto 'bitmap'.
            // Asegúrate de tener 'using PkHexA.Services;' arriba.
            var imagenSource = bitmap.ToImageSource();
            // -----------------------

            listaProcesada.Add(new
            {
                Text = nombre,
                Value = id,
                ImageUrl = imagenSource
            });
            // Convertimos SKBitmap a ImageSource de MAUI
            /* var imagenSource = GlobalService.SKBitmapToImageSource(bitmap);

             listaProcesada.Add(new
             {
                 Text = nombre,
                 Value = id,
                 ImageUrl = imagenSource // El Binding en XAML acepta ImageSource directamente
             });*/
        }

        _todasLasBalls = listaProcesada;
        ListaBalls.ItemsSource = _todasLasBalls;
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