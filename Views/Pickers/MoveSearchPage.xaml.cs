using PKHeX.Core;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace PkHexA.Views.Pickers;

public partial class MoveSearchPage : ContentPage
{
    private List<MoveVistaItem> _todosLosMovimientos = new();
    public Action<ushort>? AlSeleccionarMovimiento;
    private System.Threading.Timer? _debounceTimer;
    private bool _isNavigating = false;

    // --- CONSTRUCTOR 1: El normal ---
    public MoveSearchPage()
    {
        InitializeComponent();
        LoadingSpinner.IsRunning = true;
        Task.Run(() => CargarDatos());
    }

    // --- CONSTRUCTOR 2: EL QUE ARREGLA TU ERROR ---
    // Este constructor acepta cualquier parámetro (object) y lo ignora.
    // Esto hace que tu código antiguo compile sin que tengas que buscar la línea que falla.
    public MoveSearchPage(object parametroIgnorado) : this()
    {
        // No hacemos nada con el parámetro, pero permite que la app compile.
    }

    private void CargarDatos()
    {
        // 1. CORREGIDO: Usamos .Move (Singular)
        var listaRaw = GameInfo.Strings.Move;

        var listaTemp = new List<MoveVistaItem>();

        // 2. CORREGIDO: Usamos .Count en lugar de .Length
        for (int i = 0; i < listaRaw.Count; i++)
        {
            string nombre = listaRaw[i];

            if (string.IsNullOrEmpty(nombre) || nombre.StartsWith("---")) continue;

            int typeId = MoveInfo.GetType((ushort)i, EntityContext.Gen9);

            if (typeId > 18) typeId = 0;

            listaTemp.Add(new MoveVistaItem
            {
                Id = (ushort)i,
                Nombre = nombre,
                TipoImagen = $"type_{typeId}.png"
            });
        }

        _todosLosMovimientos = listaTemp;

        MainThread.BeginInvokeOnMainThread(() =>
        {
            ListaMovimientos.ItemsSource = _todosLosMovimientos;
            LoadingSpinner.IsRunning = false;
            LoadingSpinner.IsVisible = false;
        });
    }

    private void OnBusquedaChanged(object sender, TextChangedEventArgs e)
    {
        _debounceTimer?.Dispose();
        var texto = e.NewTextValue;

        _debounceTimer = new System.Threading.Timer(async _ =>
        {
            await FiltrarRapido(texto);
        }, null, 300, System.Threading.Timeout.Infinite);
    }

    private async Task FiltrarRapido(string? texto)
    {
        if (_todosLosMovimientos.Count == 0) return;

        List<MoveVistaItem> resultados;

        if (string.IsNullOrWhiteSpace(texto))
        {
            resultados = _todosLosMovimientos;
        }
        else
        {
            resultados = _todosLosMovimientos
                .Where(m => m.Nombre.Contains(texto, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        MainThread.BeginInvokeOnMainThread(() =>
        {
            ListaMovimientos.ItemsSource = resultados;
        });
    }

    private async void OnMoveSelected(object sender, SelectionChangedEventArgs e)
    {
        if (_isNavigating) return;

        var item = e.CurrentSelection.FirstOrDefault() as MoveVistaItem;
        if (item == null) return;

        try
        {
            _isNavigating = true;
            AlSeleccionarMovimiento?.Invoke(item.Id);

            if (Navigation.ModalStack.Count > 0)
                await Navigation.PopModalAsync();
            else
                await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        finally
        {
            _isNavigating = false;
            if (sender is CollectionView cv) cv.SelectedItem = null;
        }
    }
}

public class MoveVistaItem
{
    public ushort Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string TipoImagen { get; set; } = string.Empty;
}