using Microsoft.Maui.Controls;
using PKHeX.Core;
using PkHexA.Helper;
using PkHexA.LibSprites.Util;
using PkHexA.Modal;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PkHexA.Views.Pickers;

public partial class MoveSearchPage : ContentPage
{
    // Caché de todos los movimientos (se carga una sola vez)
    private List<MoveVistaItem> _todosLosMovimientos = new();

    // Token para cancelar búsquedas viejas mientras escribes
    private CancellationTokenSource? _searchCts;

    // Bandera para evitar doble navegación (clics rápidos)
    private bool _isNavigating = false;

    // Acción para devolver el resultado
    public Action<ushort,string>? AlSeleccionarMovimiento;

    // --- CONSTRUCTOR PRINCIPAL ---
    public MoveSearchPage()
    {
        InitializeComponent();
        LoadingSpinner.IsRunning = true;

        // Cargamos los datos en segundo plano para no congelar la apertura de la ventana
        Task.Run(() => CargarDatos());
    }

    // --- CONSTRUCTOR DE COMPATIBILIDAD (Para arreglar tu error anterior) ---
    public MoveSearchPage(object parametroIgnorado) : this()
    {
    }

    private void CargarDatos()
    {
        var listaRaw = GameInfo.Strings.Move;
        //var source = GameInfo.FilteredSources; //otra forma de obtener datod
       
        var listaTemp = new List<MoveVistaItem>();

        // Usamos Count para listas, Length para arrays. PKHeX suele usar Lists o Arrays.
        int total = listaRaw.Count;

        for (int i = 0; i < total; i++)
        {
            string nombre = listaRaw[i];

            // Ignoramos movimientos vacíos o placeholders
            if (string.IsNullOrEmpty(nombre) || nombre.StartsWith("---")) continue;

            // Obtenemos el tipo para la imagen (Gen9 es el estándar actual)
            int typeId = MoveInfo.GetType((ushort)i, EntityContext.Gen9);
            if (typeId > 18) typeId = 0; // Seguridad para tipos desconocidos
                                         // 3. Formato exacto de tus archivos: type_icon_01.png
            string resourceName = $"type_icon_{typeId:00}";

            // 4. Cargar con tu librería

            var skBitmap = SpriteImgLoader.LoadSprite(resourceName);
            ImageSource? imgTipo = null;

            if (skBitmap != null)
                imgTipo = SpriteHelper.SafeImageSourceFromSKBitmap(skBitmap);

            listaTemp.Add(new MoveVistaItem
            {
                Id = (ushort)i,
                Nombre = nombre,
                TipoImagen = imgTipo
            });
        }

        _todosLosMovimientos = listaTemp;

        // Actualizamos la UI en el Hilo Principal
        MainThread.BeginInvokeOnMainThread(() =>
        {
            ListaMovimientos.ItemsSource = _todosLosMovimientos;
            LoadingSpinner.IsRunning = false;
            LoadingSpinner.IsVisible = false;
        });
    }

    // --- LÓGICA DE BÚSQUEDA OPTIMIZADA (DEBOUNCING) ---
    private async void OnBusquedaChanged(object sender, TextChangedEventArgs e)
    {
        var textoBusqueda = e.NewTextValue;

        // 1. CANCELAR: Si había una búsqueda pendiente (el usuario sigue escribiendo), la matamos.
        if (_searchCts != null)
        {
            _searchCts.Cancel();
            _searchCts.Dispose();
        }

        // 2. NUEVO TOKEN: Creamos uno nuevo para esta letra que acaba de escribir
        _searchCts = new CancellationTokenSource();
        var token = _searchCts.Token;

        try
        {
            // 3. ESPERAR (DEBOUNCE): Esperamos 300ms. Si el usuario escribe otra letra aquí,
            // saltará al 'catch' y no ejecutará el filtrado pesado.
            await Task.Delay(300, token);

            // 4. FILTRAR EN FONDO: Si pasaron los 300ms, filtramos en un Task separado.
            var resultados = await Task.Run(() => FiltrarLogica(textoBusqueda), token);

            // 5. ACTUALIZAR UI: Solo si no se canceló en el proceso.
            if (!token.IsCancellationRequested)
            {
                ListaMovimientos.ItemsSource = resultados;
            }
        }
        catch (TaskCanceledException)
        {
            // La búsqueda fue cancelada, no pasa nada (es normal al escribir rápido).
        }
    }

    // Método puro de filtrado (corre en hilo secundario)
    private List<MoveVistaItem> FiltrarLogica(string? texto)
    {
        // Si no hay texto, devolvemos todo
        if (string.IsNullOrWhiteSpace(texto))
            return _todosLosMovimientos;

        // Filtramos (OrdinalIgnoreCase es más rápido que CurrentCulture)
        return _todosLosMovimientos
                .Where(m => m.Nombre.Contains(texto, StringComparison.OrdinalIgnoreCase))
                .ToList();
    }

    private async void OnMoveSelected(object sender, SelectionChangedEventArgs e)
    {
        if (_isNavigating) return;

        var item = e.CurrentSelection.FirstOrDefault() as MoveVistaItem;
        if (item == null) return;

        try
        {
            _isNavigating = true;

            // Invocamos el evento de retorno
            AlSeleccionarMovimiento?.Invoke(item.Id,item.Nombre);

            // Cerramos la ventana de forma segura
            if (Navigation.ModalStack.Count > 0)
                await Navigation.PopModalAsync();
            else
                await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error navegando: {ex.Message}");
        }
        finally
        {
            _isNavigating = false;
            // Limpiamos la selección visual
            if (sender is CollectionView cv) cv.SelectedItem = null;
        }
    }
}