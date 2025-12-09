using PKHeX.Core;
using PkHexA.Modal;
namespace PkHexA.Views.Pickers;

public partial class MoveSearchPage : ContentPage
{
    // Variables
    private List<MoveItem> _allMoves = new();

    // Acción que se ejecutará al seleccionar un ataque (devuelve el ID)
    public Action<int> AlSeleccionarMovimiento { get; set; }

    public MoveSearchPage(EntityContext context = EntityContext.Gen9)
    {
        InitializeComponent();
        CargarMovimientos(context);
    }

    private void CargarMovimientos(EntityContext context)
    {
        // 1. Obtener la lista de nombres desde PKHeX
        string[] moveList = GameInfo.Strings.movelist;
        _allMoves = new List<MoveItem>();

        // 2. Recorrer y crear objetos visuales
        for (int i = 0; i < moveList.Length; i++)
        {
            // Validar que el nombre no esté vacío (PKHeX a veces tiene huecos)
            if (string.IsNullOrEmpty(moveList[i])) continue;

            // Obtener el tipo según la generación (contexto)
            int typeId = MoveInfo.GetType((ushort)i, context);

            _allMoves.Add(new MoveItem
            {
                Id = i,
                Name = moveList[i],
                // IMPORTANTE: Asegúrate de tener las imágenes type_0.png, type_1.png...
                TypeImage = ImageSource.FromFile($"type_{typeId}.png"),
                SearchString = moveList[i].ToLower()
            });
        }

        // 3. Asignar a la lista visual
        cvMoves.ItemsSource = _allMoves;
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(e.NewTextValue))
        {
            // Si está vacío, mostrar todo
            cvMoves.ItemsSource = _allMoves;
        }
        else
        {
            // Filtrar por nombre (busca texto contenido)
            var filtro = e.NewTextValue.ToLower();
            var resultados = _allMoves
                .Where(x => x.SearchString.Contains(filtro))
                .ToList();

            cvMoves.ItemsSource = resultados;
        }
    }

    private async void OnMoveSelected(object sender, SelectionChangedEventArgs e)
    {
        // Verificar que hay algo seleccionado
        if (e.CurrentSelection.FirstOrDefault() is MoveItem itemSeleccionado)
        {
            // Limpiar selección para que se pueda volver a seleccionar el mismo si se reabre
            cvMoves.SelectedItem = null;

            // Invocar la acción en el PkmEditor
            AlSeleccionarMovimiento?.Invoke(itemSeleccionado.Id);

            // Cerrar la ventana
            await Navigation.PopModalAsync();
        }
    }
}