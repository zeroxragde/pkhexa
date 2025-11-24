using PKHeX.Core;
using PkHexA.Services;

namespace PkHexA.Views.Pickers;

public partial class FormaSearchPage : ContentPage
{
    // Almacenamos la lista completa para poder filtrar luego con el buscador
    private List<dynamic>? _todasLasFormas;

    // El "cable" para devolver el resultado al Editor
    public Action<dynamic>? AlSeleccionarForma;

    public FormaSearchPage()
    {
        InitializeComponent();
    }

    // =========================================================
    // PARTE 1: LÓGICA DE NEGOCIO (ESTÁTICA)
    // Este método es el "Experto" en saber qué formas tiene un Pokémon
    // =========================================================
    public static string[] ObtenerFormasDelPokemon(PKM pkm)
    {
        // Necesitamos el archivo de guardado para saber la generación y contexto
        var saveFile = GlobalService.ACTUAL_FILE;
        if (saveFile == null) return Array.Empty<string>();

        var species = pkm.Species;

        // Obtenemos la información personal de la especie
        var pi = saveFile.Personal[species];

        // PKHeX verifica si esta especie tiene formas en este juego específico
        bool hasForms = FormInfo.HasFormSelection(pi, species, pkm.Format);

        if (!hasForms)
        {
            return Array.Empty<string>(); // No tiene formas, devolvemos vacío
        }

        // Preparamos los datos para el convertidor
        var str = GameInfo.Strings;
        string[] genderSymbols = { "♂", "♀", "-" }; // Necesario para formas visuales por género

        // Obtenemos la lista de textos (ej: ["Normal", "Mega X", "Mega Y"])
        string[] forms = FormConverter.GetFormList(species, str.types, str.forms, genderSymbols, pkm.Context);

        return forms;
    }

    // =========================================================
    // PARTE 2: LÓGICA VISUAL (DE LA PÁGINA)
    // =========================================================

    /// <summary>
    /// Recibe la lista de nombres (strings) y la convierte en objetos para la lista visual
    /// </summary>
    public void CargarFormas(string[] listaNombres)
    {
        var listaProcesada = new List<dynamic>();

        // Convertimos el array de strings a objetos { Text, Value }
        // Value = Índice (0, 1, 2...) que es lo que se guarda en el .pkm
        for (int i = 0; i < listaNombres.Length; i++)
        {
            listaProcesada.Add(new
            {
                Text = listaNombres[i],
                Value = i
            });
        }

        // Guardamos copia y asignamos a la lista visual
        _todasLasFormas = listaProcesada;
        ListaFormas.ItemsSource = _todasLasFormas;
    }

    /// <summary>
    /// Evento al hacer click en una forma de la lista
    /// </summary>
    private async void OnFormaSeleccionada(object sender, SelectionChangedEventArgs e)
    {
        var item = e.CurrentSelection.FirstOrDefault();

        if (item != null)
        {
            // Devolvemos el objeto seleccionado al Editor
            AlSeleccionarForma?.Invoke((dynamic)item);

            // Cerramos la ventana
            await Navigation.PopModalAsync();
        }

        // Deseleccionar visualmente (opcional)
        ((CollectionView)sender).SelectedItem = null;
    }

    /// <summary>
    /// Filtrado en tiempo real cuando escribes en la barra
    /// </summary>
    private void OnBusquedaChanged(object sender, TextChangedEventArgs e)
    {
        var texto = e.NewTextValue?.ToLower() ?? "";

        // Seguridad por si la lista no se cargó
        if (_todasLasFormas == null) return;

        if (string.IsNullOrWhiteSpace(texto))
        {
            // Si borran el texto, mostramos todo
            ListaFormas.ItemsSource = _todasLasFormas;
        }
        else
        {
            // Filtramos por nombre
            var filtrados = _todasLasFormas
                .Where(p => p.Text.ToLower().Contains(texto))
                .ToList();

            ListaFormas.ItemsSource = filtrados;
        }
    }
}