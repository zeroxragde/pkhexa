using PKHeX.Core;
using PkHexA.LibSprites.Util;

namespace PkHexA.Views.Pickers;

public partial class ItemSearchPage : ContentPage
{
    private List<dynamic>? _todosLosItems;
    public Action<dynamic>? AlSeleccionarItem;
    public ItemSearchPage()
	{
		InitializeComponent();
        CargarDatos();
    }
    private void CargarDatos()
    {
        // 1. Obtenemos la lista de nombres de objetos de PKHeX
        string[] nombresItems = GameInfo.Strings.itemlist;

        var listaProcesada = new List<dynamic>();

        // Recorremos el array. El índice 'i' es el ID del objeto.
        for (int i = 0; i < nombresItems.Length; i++)
        {
            string nombre = nombresItems[i];

            // Filtros básicos
            if (string.IsNullOrEmpty(nombre) || nombre.Contains("???")) continue;

            // SOLO DATOS DE TEXTO Y VALOR
            listaProcesada.Add(new
            {
                Text = nombre,
                Value = i
                // No hay ImageUrl
            });
        }

        _todosLosItems = listaProcesada;
        ListaItems.ItemsSource = _todosLosItems;
    }

    public void Preseleccionar(int itemId)
    {
        if (_todosLosItems == null) return;

        var item = _todosLosItems.FirstOrDefault(x => x.Value == itemId);
        if (item != null)
        {
            ListaItems.SelectedItem = item;

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Task.Delay(100);
                ListaItems.ScrollTo(item, -1, ScrollToPosition.Center, animate: false);
            });
        }
    }

    private async void OnItemSeleccionado(object sender, SelectionChangedEventArgs e)
    {
        var item = e.CurrentSelection.FirstOrDefault();
        if (item != null)
        {
            AlSeleccionarItem?.Invoke((dynamic)item);
            await Navigation.PopModalAsync();
        }
        ((CollectionView)sender).SelectedItem = null;
    }

    private void OnBusquedaChanged(object sender, TextChangedEventArgs e)
    {
        var texto = e.NewTextValue?.ToLower() ?? "";
        if (_todosLosItems == null) return;

        if (string.IsNullOrWhiteSpace(texto))
        {
            ListaItems.ItemsSource = _todosLosItems;
        }
        else
        {
            // Filtramos por nombre
            ListaItems.ItemsSource = _todosLosItems
                .Where(p => p.Text.ToLower().Contains(texto))
                .ToList();
        }
    }
}