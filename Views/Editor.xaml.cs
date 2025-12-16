using PKHeX.Core;
using PkHexA.Services;
using Microsoft.Maui.Controls.Shapes;

namespace PkHexA.Views;

public partial class Editor : ContentPage
{
    // Variables de selección visual
    private Border? _bordeSeleccionadoVisualmente;
    private PKM? _pkmSeleccionado;

    // Colores
    private readonly Color ColorVacio = Color.FromArgb("#1A1A1A");
    private readonly Color ColorOcupado = Color.FromArgb("#303080");   // Azul para Caja
    private readonly Color ColorParty = Color.FromArgb("#308030");     // Verde para Equipo
    private readonly Color ColorBordeNormal = Color.FromArgb("#555");
    private readonly Color ColorBordeSeleccion = Colors.Yellow;

    public Editor()
    {
        InitializeComponent();

        AutoTraductor.Traducir(this);

        // 1. Cargar lista de cajas
        CargarSelectorCajas();

        // 2. Cargar Equipo (Party)
        CargarEquipo();
    }

    // ========================================================================
    // SECCIÓN: SELECTOR DE CAJAS
    // ========================================================================
    private void CargarSelectorCajas()
    {
        var save = GlobalService.ACTUAL_FILE;
        if (save == null) return;

        var listaCajas = new List<string>();

        // SOLUCIÓN A PRUEBA DE BALAS:
        // Generamos los nombres manualmente: "Caja 1", "Caja 2", etc.
        for (int i = 0; i < save.BoxCount; i++)
        {
            listaCajas.Add($"{LanguageService.Get("txtCaja")} {i + 1}");
        }

        BoxPicker.ItemsSource = listaCajas;

        // Validación para evitar crash si el índice está fuera de rango
        if (save.CurrentBox >= 0 && save.CurrentBox < listaCajas.Count)
        {
            BoxPicker.SelectedIndex = save.CurrentBox;
        }
        else
        {
            BoxPicker.SelectedIndex = 0;
        }
    }

    private void OnBoxChanged(object sender, EventArgs e)
    {
        if (BoxPicker.SelectedIndex == -1) return;

        var save = GlobalService.ACTUAL_FILE;
        if (save != null)
        {
            save.CurrentBox = BoxPicker.SelectedIndex;
            CargarCajaGenerada(); // Recargamos la rejilla visual
        }
    }
    // ========================================================================
    // GUARDAR EL ARCHIVO .SAV (Sobrescribir el original)
    // ========================================================================
    private async void OnSaveClicked(object sender, EventArgs e)
    {
        var save = GlobalService.ACTUAL_FILE;

        // Necesitamos saber la ruta del archivo original. 
        // Asegúrate de que en GlobalService tengas esta variable o pásala de alguna forma.
        // Si no tienes la ruta guardada, tendrás que usar un "FilePicker" para guardar como nuevo.
        string? path = GlobalService.CurrentFilePath;

        if (save == null)
        {
            await GlobalService.ShowAlertAsync(LanguageService.Get("alertNoFileLoad"));
            return;
        }

        if (string.IsNullOrEmpty(path))
        {
            await GlobalService.ShowAlertAsync(LanguageService.Get("alertNoOriginalPath"));
            return;
        }

        try
        {
            // 1. Generamos los datos (Agregamos .ToArray() aquí)
            byte[] datosNuevos = save.Write().ToArray();

            // 2. Sobrescribimos
            System.IO.File.WriteAllBytes(path, datosNuevos);

            await GlobalService.ShowAlertAsync(LanguageService.Get("msgGuardado"), "OK");
        }
        catch (Exception ex)
        {
            await GlobalService.ShowAlertAsync($"Error al guardar: {ex.Message}");
        }
    }
    // ========================================================================
    // EXPORTAR POKÉMON INDIVIDUAL (.pkm, .pk9, etc.)
    // ========================================================================
    private async void OnExportPkmClicked(object sender, EventArgs e)
    {
        // 1. Validar que haya un Pokémon seleccionado en la rejilla
        if (_pkmSeleccionado == null || !_pkmSeleccionado.Valid || _pkmSeleccionado.Species == 0)
        {
            await GlobalService.ShowAlertAsync(LanguageService.Get("alertNoPkmSel"));
            return;
        }

        try
        {
            // 2. Obtener el nombre del archivo
            string nombreArchivo = _pkmSeleccionado.FileName;

            // 3. Obtener los datos binarios (AGREGADO .ToArray())
            byte[] datosPkm = _pkmSeleccionado.Data.ToArray();

            // 4. Crear una ruta temporal en el celular
            string rutaTemporal = System.IO.Path.Combine(FileSystem.CacheDirectory, nombreArchivo);

            // 5. Escribir el archivo en la caché
            File.WriteAllBytes(rutaTemporal, datosPkm);

            // 6. Compartir
            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = LanguageService.Get("btnExportarPkm"),
                File = new ShareFile(rutaTemporal)
            });
        }
        catch (Exception ex)
        {
            await GlobalService.ShowAlertAsync($"Error: {ex.Message}");
        }
    }
    // ========================================================================
    // SECCIÓN: CAJA PRINCIPAL (GRID 6x5)
    // ========================================================================
    private void CargarCajaGenerada()
    {
        var save = GlobalService.ACTUAL_FILE;
        if (save == null) return;

        BoxGridContainer.Children.Clear(); // Limpiar lo anterior

        var data = save.GetBoxData(save.CurrentBox);

        for (int i = 0; i < data.Length; i++) // Usualmente 30
        {
            // Calcular posición (6 columnas)
            int row = i / 6;
            int col = i % 6;

            PKM pkm = data[i];

            // Creamos el cuadrito azul
            var view = CrearCuadritoPokemon(pkm, ColorOcupado);

            BoxGridContainer.Add(view, col, row);
        }
    }

    // ========================================================================
    // SECCIÓN: EQUIPO POKÉMON (PARTY)
    // ========================================================================
    private void CargarEquipo()
    {
        var save = GlobalService.ACTUAL_FILE;
        if (save == null) return;

        PartyGridContainer.Children.Clear();

        var partyData = save.PartyData;
        int maxParty = 6; // Siempre dibujamos 6 espacios

        for (int i = 0; i < maxParty; i++)
        {
            // Calcular posición (3 columnas x 2 filas)
            int row = i / 3;
            int col = i % 3;

            PKM pkm;
            if (i < partyData.Count)
                pkm = partyData[i];
            else
                pkm = save.BlankPKM; // Espacio vacío

            // Creamos el cuadrito verde
            var view = CrearCuadritoPokemon(pkm, ColorParty);

            PartyGridContainer.Add(view, col, row);
        }
    }

    // ========================================================================
    // MÉTODO AUXILIAR PARA CREAR CUADRITOS (DRY)
    // ========================================================================
    private Border CrearCuadritoPokemon(PKM pkm, Color colorBase)
    {
        bool existe = pkm.Species > 0 && pkm.Valid;

        // Label del centro
        var label = new Label
        {
            Text = existe ? pkm.Species.ToString() : "",
            TextColor = Colors.White,
            FontSize = 10,
            FontAttributes = FontAttributes.Bold,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };

        // Borde contenedor
        var border = new Border
        {
            Stroke = ColorBordeNormal,
            StrokeThickness = 1,
            BackgroundColor = existe ? colorBase : ColorVacio,
            StrokeShape = new RoundRectangle { CornerRadius = 5 },
            Content = label,
            BindingContext = pkm // Guardamos el Pokémon aquí
        };

        // Gesto de toque
        var tapGesture = new TapGestureRecognizer();
        tapGesture.Tapped += OnBordeTapped;
        border.GestureRecognizers.Add(tapGesture);

        return border;
    }

    // ========================================================================
    // EVENTOS DE INTERACCIÓN
    // ========================================================================
    private void OnBordeTapped(object sender, TappedEventArgs e)
    {
        var bordeTocado = sender as Border;
        if (bordeTocado == null) return;

        // 1. Deseleccionar anterior
        if (_bordeSeleccionadoVisualmente != null)
        {
            _bordeSeleccionadoVisualmente.Stroke = ColorBordeNormal;
            _bordeSeleccionadoVisualmente.StrokeThickness = 1;
        }

        // 2. Seleccionar nuevo
        bordeTocado.Stroke = ColorBordeSeleccion;
        bordeTocado.StrokeThickness = 2;
        _bordeSeleccionadoVisualmente = bordeTocado;

        // 3. Guardar referencia
        _pkmSeleccionado = bordeTocado.BindingContext as PKM;
    }

    private async void OnEditPkmClicked(object sender, EventArgs e)
    {
        if (_pkmSeleccionado == null || _pkmSeleccionado.Species == 0)
        {
            await GlobalService.ShowAlertAsync(LanguageService.Get("alertNoPkmSel"));
            return;
        }

        var editorPage = new pkmEditorPro();
        editorPage.CargarDatos(_pkmSeleccionado);
        await Navigation.PushModalAsync(editorPage);
    }

    private async void OnAddPokemonClicked(object sender, EventArgs e)
    {
        /*  var save = GlobalService.ACTUAL_FILE;
          if (save == null) return;

          PKM nuevoPkm = save.BlankPKM;
          var editorPage = new PkmEditor();
          editorPage.CargarDatos(nuevoPkm);
          await Navigation.PushModalAsync(editorPage);*/
        // 1. Validar que se haya seleccionado un espacio (aunque esté vacío)
        if (_pkmSeleccionado == null)
        {
            await GlobalService.ShowAlertAsync(LanguageService.Get("alertSelSpace"));
            return;
        }

        var save = GlobalService.ACTUAL_FILE;
        if (save == null) return;

        // 2. Crear el nuevo Pokémon
        PKM nuevoPkm = save.BlankPKM;

        // 3. Abrir editor
        var editorPage = new pkmEditorPro();
        editorPage.CargarDatos(nuevoPkm);

        // 4. IMPORTANTE: Aquí deberás implementar la lógica para que, al guardar en el editor,(PENDIENTE)
        // se sobrescriba el '_pkmSeleccionado' o el slot correspondiente en el SaveFile.

        await Navigation.PushModalAsync(editorPage);
    }

    private async void OnExitClicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }

    private void OnEditUserClicked(object sender, EventArgs e)
    {
        // Pendiente: Lógica para editar entrenador
    }
}