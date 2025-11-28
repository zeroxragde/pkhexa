using PKHeX.Core;
using PkHexA.LibSprites.Util;
using PkHexA.Services;
using PkHexA.Views.Pickers;
using SkiaSharp;

namespace PkHexA.Views;

public partial class PkmEditor : ContentPage
{
    // Variable para guardar el ID actual (para cuando guardes el archivo .pkm)
    private int _currentSpeciesId;
    private PKM _pkmActual;
    // Variable para guardar las formas disponibles del Pokémon actual
    private string[] _formasDisponibles = Array.Empty<string>();
    // Colores para las pestañas
    private readonly Color ColorTabActivo = Color.FromArgb("#512BD4"); // Tu Morado
    private readonly Color ColorTabInactivo = Colors.Transparent;
    private readonly Color TextoActivo = Colors.White;
    private readonly Color TextoInactivo = Colors.Gray;
    public PkmEditor()
	{
		InitializeComponent();
		//Title = LanguageService.Get("winPkmEditor");
       // tabInicioEditPkm.Text = LanguageService.Get("tabPkmEditInicio");
       // lblPokemonSeleccionado.Text = LanguageService.Get("lblPokemonSeleccionado");
        AutoTraductor.Traducir(this);
        var speciesList = GameInfo.Sources.SpeciesDataSource.ToList();
  
    }
    private void OnTabClicked(object sender, EventArgs e)
    {
        var botonPresionado = sender as Button;
        if (botonPresionado == null) return;

        // ---------------------------------------------------------
        // 1. APAGAR TODOS LOS BOTONES (Automático)
        // ---------------------------------------------------------
        // Recorremos los hijos del StackBotones definido en el XAML
        if (StackBotones != null)
        {
            foreach (var hijo in StackBotones.Children)
            {
                if (hijo is Button btn)
                {
                    btn.BackgroundColor = ColorTabInactivo;
                    btn.TextColor = TextoInactivo;
                }
            }
        }

        // 2. ENCENDER EL SELECCIONADO
        botonPresionado.BackgroundColor = ColorTabActivo;
        botonPresionado.TextColor = TextoActivo;

        // ---------------------------------------------------------
        // 3. MOSTRAR EL PANEL CORRECTO (Automático)
        // ---------------------------------------------------------
        // Obtenemos el panel desde el CommandParameter
        var panelDestino = botonPresionado.CommandParameter as View;

        if (panelDestino != null && GridPaneles != null)
        {
            // A. Ocultar TODOS los paneles del Grid
            foreach (var hijo in GridPaneles.Children)
            {
                if (hijo is View v) v.IsVisible = false;
            }

            // B. Mostrar SOLO el destino
            panelDestino.IsVisible = true;
        }
    }
    private void RestaurarTabs()
    {
        // 1. Poner todos los botones en estilo "Apagado/Gris"
        // Asumo que tienes un estilo llamado 'TabButtonStyle' en tu XAML
        btnTabInicio.Style = (Style)Resources["TabButtonStyle"];
        btnTabEncuentro.Style = (Style)Resources["TabButtonStyle"];

        // Si tienes más botones (Estadísticas, Movimientos), agrégalos aquí:
        // btnTabEstadisticas.Style = (Style)Resources["TabButtonStyle"];

        // 2. Ocultar todos los contenidos
        TabInicio.IsVisible = false;
        TabEncuentro.IsVisible = false;

        //TabEstadisticas.IsVisible = false;
    }
    /*private void OnTabClicked(object sender, EventArgs e)
    {
        var botonPresionado = sender as Button;
        if (botonPresionado == null) return;

        // 1. APAGAR TODOS (Asegúrate de que existan en el XAML con x:Name)
        // Usamos una lista rápida o helper para no repetir código
        ApagarBoton(btnTabInicio);
        ApagarBoton(btnTabEncuentro);
        ApagarBoton(btnTabStats);
        ApagarBoton(btnTabMoves);
        ApagarBoton(btnTabCosmetics);
        ApagarBoton(btnTabOT);

        // 2. OCULTAR PANELES
        if (TabInicio != null) TabInicio.IsVisible = false;
        if (TabEncuentro != null) TabEncuentro.IsVisible = false;
        // Agrega aquí los demás paneles cuando los crees (TabStats, etc.)

        // 3. ENCENDER SELECCIONADO
        botonPresionado.BackgroundColor = ColorTabActivo;
        botonPresionado.TextColor = TextoActivo;

        // 4. MOSTRAR PANEL
        if (botonPresionado == btnTabInicio) TabInicio.IsVisible = true;
        else if (botonPresionado == btnTabEncuentro) TabEncuentro.IsVisible = true;

    }*/
    // Helper pequeño para limpiar el código de arriba
    private void ApagarBoton(Button btn)
    {
        if (btn != null)
        {
            btn.BackgroundColor = ColorTabInactivo;
            btn.TextColor = TextoInactivo;
        }
    }

    public void CargarDatos(PKM pkm)
    {
        // 1. Guardamos la referencia global
        _pkmActual = pkm;

        // 2. DETECTAR SI ES NUEVO O EXISTENTE
        if (pkm.Species == 0)
        {
            // CASO A: Es un espacio vacío o Nuevo
            // Le ponemos valores por defecto para empezar a editar
            FijarPokemon(1); // Empezamos con Bulbasaur (ID 1)

            // Valores default
            NicknameEntry.Text = "Bulbasaur";
            LevelEntry.Text = "1";
            ExpEntry.Text = "0";
            FriendshipEntry.Text = "70";
            _pkmActual.Ball = 4;
            imgPokeBall.Source =GlobalService.SKBitmapToImageSource(SpriteUtil.GetBallSprite((byte)_pkmActual.Ball));
        }
        else
        {
            // CASO B: Es un Pokémon existente
            // 1. Cargamos la especie y formas (esto actualiza la imagen y labels)
            FijarPokemon(pkm.Species);

            // 2. Llenamos los campos de texto con los datos reales
            NicknameEntry.Text = pkm.Nickname;
            LevelEntry.Text = pkm.CurrentLevel.ToString();
            ExpEntry.Text = pkm.EXP.ToString();
            FriendshipEntry.Text = pkm.CurrentFriendship.ToString();

            // 3. Cargamos valores de los Pickers
            // (Asumiendo que los índices coinciden)
            GenderPicker.SelectedIndex = pkm.Gender;

            // Nota: Para Naturaleza e Idioma, necesitas buscar el índice en tu lista
            // NaturePicker.SelectedIndex = pkm.Nature;
        }
     //   lblGameNameForm.Text = GlobalService.ACTUAL_FILE.
        // 4. Forzar actualización visual de forma y habilidad
        ActualizarFormas();

        int itemID = _pkmActual.HeldItem;
        // Obtener nombre del objeto
        // GameInfo.Strings.itemlist tiene los nombres en el idioma cargado
        var listaItems = GameInfo.Strings.itemlist;

        if (itemID < listaItems.Length)
            lblObjetoSeleccionado.Text = listaItems[itemID];
        else
            lblObjetoSeleccionado.Text = $"Item {itemID}";
        //FijarHabilidad(pkm.Ability);
        // 4. Cargar datos de la pestaña Encuentro (Ball, Lugar, etc.)
        CargarDatosEncuentro();

        // 5. Ejecutar la lógica de visibilidad (EC, CatchRate, Shadow)
        ActualizarVisibilidadPorGeneracion();
        CargarDatosStats();
    }
    // Versión SEGURA: No crashea si falta un botón en el XAML










    private void CargarDatosStats()
    {
        if (_pkmActual == null) return;

        // 1. CARGAR BASES
        // Usamos el objeto PersonalInfo (sin corchetes)
        var pi = _pkmActual.PersonalInfo;
        lblBaseHP.Text = pi.HP.ToString();
        lblBaseAtk.Text = pi.ATK.ToString();
        lblBaseDef.Text = pi.DEF.ToString();
        lblBaseSpe.Text = pi.SPE.ToString();
        lblBaseSpa.Text = pi.SPA.ToString();
        lblBaseSpd.Text = pi.SPD.ToString();

        // 2. CARGAR IVs y EVs (Propiedades estándar)
        txtIVHP.Text = _pkmActual.IV_HP.ToString();
        txtIVAtk.Text = _pkmActual.IV_ATK.ToString();
        txtIVDef.Text = _pkmActual.IV_DEF.ToString();
        txtIVSpa.Text = _pkmActual.IV_SPA.ToString();
        txtIVSpd.Text = _pkmActual.IV_SPD.ToString();
        txtIVSpe.Text = _pkmActual.IV_SPE.ToString();

        txtEVHP.Text = _pkmActual.EV_HP.ToString();
        txtEVAtk.Text = _pkmActual.EV_ATK.ToString();
        txtEVDef.Text = _pkmActual.EV_DEF.ToString();
        txtEVSpa.Text = _pkmActual.EV_SPA.ToString();
        txtEVSpd.Text = _pkmActual.EV_SPD.ToString();
        txtEVSpe.Text = _pkmActual.EV_SPE.ToString();

        // 3. EXTRAS (Usando 'dynamic' para evitar errores de compilación)
        // El objeto real (PK8, PK9) tiene estas propiedades, pero la clase base PKM no.
        dynamic pkm = _pkmActual;

        // -- Dynamax (Gen 8) --
        try
        {
            txtDynamaxLevel.Text = pkm.DynamaxLevel.ToString();
            txtDynamaxLevel.IsEnabled = true;
        }
        catch
        {
            txtDynamaxLevel.Text = "0";
            txtDynamaxLevel.IsEnabled = false;
        }

        // -- Gigantamax (Gen 8) --
        try
        {
            chkGigantamax.IsChecked = pkm.CanGigantamax;
            chkGigantamax.IsEnabled = true;
        }
        catch
        {
            chkGigantamax.IsChecked = false;
            chkGigantamax.IsEnabled = false;
        }

        // -- Tera Type (Gen 9) --
        if (TeraTypePicker.ItemsSource == null)
            TeraTypePicker.ItemsSource = GameInfo.Strings.types;

        try
        {
            // TeraType suele ser un byte o enum. Lo convertimos a int para el Picker.
            int teraIndex = (int)pkm.TeraType;
            TeraTypePicker.SelectedIndex = teraIndex;
            TeraTypePicker.IsEnabled = true;
        }
        catch
        {
            TeraTypePicker.SelectedIndex = -1;
            TeraTypePicker.IsEnabled = false;
        }

        // -- Concursos (Gen 3, 4, ORAS, BDSP) --
        try
        {
            // Intentamos leer propiedades de concurso
            txtContestCool.Text = pkm.ContestCool.ToString();
            txtContestBeauty.Text = pkm.ContestBeauty.ToString();
            txtContestCute.Text = pkm.ContestCute.ToString();
            txtContestSmart.Text = pkm.ContestSmart.ToString();
            txtContestTough.Text = pkm.ContestTough.ToString();
            txtContestSheen.Text = pkm.ContestSheen.ToString();

            HabilitarConcursos(true);
        }
        catch
        {
            HabilitarConcursos(false);
        }

        // 4. Calcular Totales
        RecalcularStatsTotal();
    }

    private void HabilitarConcursos(bool habilitar)
    {
        txtContestCool.IsEnabled = habilitar;
        txtContestBeauty.IsEnabled = habilitar;
        txtContestCute.IsEnabled = habilitar;
        txtContestSmart.IsEnabled = habilitar;
        txtContestTough.IsEnabled = habilitar;
        txtContestSheen.IsEnabled = habilitar;

        if (!habilitar)
        {
            txtContestCool.Text = "0"; txtContestBeauty.Text = "0"; txtContestCute.Text = "0";
            txtContestSmart.Text = "0"; txtContestTough.Text = "0"; txtContestSheen.Text = "0";
        }
    }
    private void RecalcularStatsTotal()
    {
        if (_pkmActual == null) return;

        // En PKHeX.Core, la propiedad .Stats devuelve el arreglo calculado:
        // [0]=HP, [1]=ATK, [2]=DEF, [3]=SPE, [4]=SPA, [5]=SPD
        // (Nota: Speed suele ser el índice 3 en el core interno, revisa si tus labels coinciden)

        int[] stats = _pkmActual.Stats;

        lblStatHP.Text = stats[0].ToString();
        lblStatAtk.Text = stats[1].ToString();
        lblStatDef.Text = stats[2].ToString();
        lblStatSpe.Text = stats[3].ToString(); // Velocidad
        lblStatSpa.Text = stats[4].ToString();
        lblStatSpd.Text = stats[5].ToString();
    }
    private void OnDynamaxLevelChanged(object sender, TextChangedEventArgs e)
    {
        if (_pkmActual != null && int.TryParse(e.NewTextValue, out int val))
        {
            try { ((dynamic)_pkmActual).DynamaxLevel = val; } catch { }
        }
    }

    private void OnGigantamaxChanged(object sender, CheckedChangedEventArgs e)
    {
        if (_pkmActual != null)
        {
            try { ((dynamic)_pkmActual).CanGigantamax = e.Value; } catch { }
        }
    }

    private void OnTeraTypeChanged(object sender, EventArgs e)
    {
        if (_pkmActual != null && TeraTypePicker.SelectedIndex >= 0)
        {
            try
            {
                // CORRECCIÓN: Casteo explícito a (byte) para solucionar el error de tipos.
                // Si tu versión usa MoveType, cámbialo a (MoveType).
                ((dynamic)_pkmActual).TeraType = (byte)TeraTypePicker.SelectedIndex;
            }
            catch { }
        }
    }



















    private void ActualizarVisibilidadPorGeneracion()
    {
        if (_pkmActual == null) return;

        int format = _pkmActual.Format;
        int gameVersion = (int)_pkmActual.Version;

        // --- 1. PESTAÑAS (Con protección contra nulos) ---

        // Si el botón no existe (es null), el signo '?' evita el crash
        btnTabCosmetics?.SetValue(IsVisibleProperty, format >= 3);

        // Ocultar Encuentro en Gen 1 (Opcional, si quieres)
        btnTabEncuentro?.SetValue(IsVisibleProperty, format >= 2);


        // --- 2. CAMPOS ESPECÍFICOS ---

        // EC (Solo Gen 6+)
        if (LayoutEC != null)
        {
            LayoutEC.IsVisible = (format >= 6);
            if (LayoutEC.IsVisible)
                txtEC.Text = _pkmActual.EncryptionConstant.ToString("X8");
        }

        // --- 3. BLOQUE EXTRA (Catch Rate / Shadow) ---
        bool esGen1 = (format == 1);
        bool esGen5 = (format == 5);
        bool esGameCube = (gameVersion == 15 || gameVersion == 24);

        bool mostrarCatchRate = esGen1 || esGameCube;

        // Protección para el Grid de Catch Rate
        if (GridCatchRate != null)
        {
            GridCatchRate.IsVisible = mostrarCatchRate;
            if (mostrarCatchRate && txtCatchRate != null)
            {
                try
                {
                    dynamic pkm = _pkmActual;
                    txtCatchRate.Text = pkm.CatchRate.ToString();
                }
                catch { txtCatchRate.Text = "255"; }
            }
        }

        // Protección para otros layouts
        if (LayoutNSparkle != null) LayoutNSparkle.IsVisible = esGen5;
        if (LayoutShadowFields != null) LayoutShadowFields.IsVisible = esGameCube;
        if (BorderExtra != null)
            BorderExtra.IsVisible = mostrarCatchRate || esGen5 || esGameCube;
    }
 
    /*  private void ActualizarVisibilidadPorGeneracion()
      {
          if (_pkmActual == null) return;

          int format = _pkmActual.Format;
          int gameVersion = (int)_pkmActual.Version;

          // 1. EC (Encryption Constant): Solo Gen 6 en adelante
          // Esto controla el layout que acabamos de restaurar
          LayoutEC.IsVisible = format >= 6;
          if (LayoutEC.IsVisible)
          {
              txtEC.Text = _pkmActual.EncryptionConstant.ToString("X8");
          }

          // --- Lógica Bloque Extra ---

          bool esGen1 = (format == 1);
          bool esGen5 = (format == 5);
          bool esGameCube = (gameVersion == 15 || gameVersion == 24);

          // Catch Rate (Gen 1 o GameCube)
          bool mostrarCatchRate = esGen1 || esGameCube;
          GridCatchRate.IsVisible = mostrarCatchRate;
          if (mostrarCatchRate)
          {
              // CORRECCIÓN AQUÍ: Usamos 'dynamic' y 'CatchRate' (PascalCase)
              try
              {
                  dynamic pkm = _pkmActual;
                  txtCatchRate.Text = pkm.CatchRate.ToString();
              }
              catch
              {
                  txtCatchRate.Text = "255"; // Fallback
              }
          }

          // N's Sparkle (Gen 5)
          LayoutNSparkle.IsVisible = esGen5;

          // Shadow (GameCube)
          LayoutShadowFields.IsVisible = esGameCube;

          // Visibilidad del Marco Completo
          BorderExtra.IsVisible = mostrarCatchRate || esGen5 || esGameCube;
      }*/








    // AL HACER CLICK EN EL "PICKER FALSO"



    private async void AbrirBuscador_Tapped(object sender, TappedEventArgs e)
    {
        var searchPage = new PokemonSearchPage();

        // CONECTAMOS EL CABLE
        searchPage.AlSeleccionarPokemon = (item) =>
        {
            // "item" es tu objeto de la base de datos
            // Asumiendo que tiene propiedades Text y Value:

            // 1. Poner el nombre en el Label visual
            lblPokemonSeleccionado.Text = item.Text;
            FijarPokemon(item.Value);
            // 2. (Opcional) Usar el ID para lógica interna
            // int idPokemon = item.Value; 
            // Console.WriteLine($"Seleccionó ID: {idPokemon}");
        };

        await Navigation.PushModalAsync(searchPage);
    }
    public void FijarPokemon(int speciesId)
    {
        if (_pkmActual == null) return;

        _pkmActual.Species = (ushort)speciesId; 
        _pkmActual.Form = 0; // SIEMPRE resetear forma al cambiar de bicho

        // 1. Le preguntamos al experto (PokemonSearchPage) quién es este ID
        var info = PokemonSearchPage.ObtenerInfoPokemon(speciesId);

        if (info != null)
        {
            // 2. Actualizamos la pantalla
            // Usamos (dynamic) para acceder a .Text sin líos
            lblPokemonSeleccionado.Text = ((dynamic)info).Text;
            NicknameEntry.Text = "";  // Limpiamos el nickname al cambiar de especie
            // 3. Guardamos el ID internamente
            _currentSpeciesId = speciesId;
            // AGREGAR ESTO:
            ActualizarFormas(); // Recalcular formas para la nueva especie
        }
        else
        {
            lblPokemonSeleccionado.Text = $"Desconocido ({speciesId})";
            _currentSpeciesId = speciesId;
        }
    }

    private async void AbrirBuscadorEstadoNaturaleza_Tapped(object sender, TappedEventArgs e)
    {
        var searchPage = new NaturalezaSearchPage();
        // CONECTAMOS EL CABLE
        searchPage.AlSeleccionarNatura = (item) =>
        {
            // 1. Poner el nombre en el Label visual
            lblNaturalezaEstadoSeleccionado.Text = item.Text;
        };
        await Navigation.PushModalAsync(searchPage);
    }

    private async void AbrirBuscadorNaturaleza_Tapped(object sender, TappedEventArgs e)
    {
        var searchPage = new NaturalezaSearchPage();
        // CONECTAMOS EL CABLE
        searchPage.AlSeleccionarNatura = (item) =>
        {
            // 1. Poner el nombre en el Label visual
            lblNaturalezaSeleccionado.Text = item.Text;
        };
        await Navigation.PushModalAsync(searchPage);
    }

    private async void AbrirBuscadorHabilidad_Tapped(object sender, TappedEventArgs e)
    {
        var searchPage = new HabilidadSearchPage();
        // CONECTAMOS EL CABLE
        searchPage.AlSeleccionarHabilidad = (item) =>
        {
            // 1. Poner el nombre en el Label visual
            lblHabilidadSeleccionado.Text = item.Text;
        };
        await Navigation.PushModalAsync(searchPage);
    }

    /// <summary>
    /// Calcula si el Pokémon actual tiene formas y actualiza la UI.
    /// </summary>
    private void ActualizarFormas()
    {
        if (_pkmActual == null) return;

        // 1. PREGUNTAMOS AL EXPERTO (Tu método estático en FormSearchPage)
        // Esto usa PKHeX internamente para saber si hay formas válidas
        _formasDisponibles = FormaSearchPage.ObtenerFormasDelPokemon(_pkmActual);

        // 2. DECIDIR VISIBILIDAD
        // Si la lista tiene más de 1 elemento (ej: Normal, Attack, Defense...), mostramos el botón.
        bool tieneFormas = _formasDisponibles.Length > 1;

        LayoutForma.IsVisible = tieneFormas;

        // 3. ACTUALIZAR TEXTO
        if (tieneFormas)
        {
            // Verificamos que el índice de forma actual sea válido
            if (_pkmActual.Form < _formasDisponibles.Length)
            {
                lblFormaSeleccionada.Text = _formasDisponibles[_pkmActual.Form];
            }
            else
            {
                // Si el archivo trae una forma inválida, forzamos la 0
                _pkmActual.Form = 0;
                lblFormaSeleccionada.Text = _formasDisponibles[0];
            }
        }
        else
        {
            // Si no tiene formas, aseguramos que sea 0 internamente
            _pkmActual.Form = 0;
        }
    }
    /// <summary>
    /// Abre la ventana para seleccionar la forma.
    /// </summary>
    private async void AbrirBuscadorForma_Tapped(object sender, TappedEventArgs e)
    {
        if (_formasDisponibles.Length <= 1) return;

        var page = new FormaSearchPage();

        // IMPORTANTE: Le pasamos la lista que ya calculamos
        page.CargarFormas(_formasDisponibles);

        page.AlSeleccionarForma = (item) =>
        {
            int idForma = item.Value;
            string nombreForma = item.Text;

            // A. Actualizar UI
            lblFormaSeleccionada.Text = nombreForma;

            // B. Actualizar Pokémon
            if (_pkmActual != null)
            {
                _pkmActual.Form = (byte)idForma;

                // Ojo: Cambiar de forma a veces cambia stats base o tipos (ej: Rotom)
                // Aquí podrías llamar a un método para refrescar la imagen del sprite si la tuvieras
            }
        };

        await Navigation.PushModalAsync(page);
    }

    private void AbrirBuscadorLugarHuevo_Tapped(object sender, TappedEventArgs e)
    {
        //page.CargarLugares(_pkmActual.Version, _pkmActual.Context, true);
    }

    private async void AbrirBuscadorBall_Tapped(object sender, TappedEventArgs e)
    {
        if (_pkmActual == null) return;

        var page = new PkHexA.Views.Pickers.BallSearchPage();

        // Preseleccionar la ball actual
        page.Preseleccionar(_pkmActual.Ball);

        // Recibir selección
        page.AlSeleccionarBall = (item) =>
        {
            int idBall = item.Value; // ID (1 = Master, 4 = Poke, etc)
            string nombreBall = item.Text;

            lblPokeBall.Text = nombreBall;

            // Guardar en el Pokémon
            if (_pkmActual != null)
            {
                // Casteo a byte por seguridad, las balls son ID < 255
                _pkmActual.Ball = (byte)idBall;
                imgPokeBall.Source = GlobalService.SKBitmapToImageSource(SpriteUtil.GetBallSprite((byte)_pkmActual.Ball));
            }
         
        };

        await Navigation.PushModalAsync(page);
    }

    private async void AbrirBuscadorLugar_Tapped(object sender, TappedEventArgs e)
    {
        if (_pkmActual == null) return;

        var page = new PkHexA.Views.Pickers.LocationSearchPage();

        // Cargar lugares (esto está bien)
        page.CargarLugares(_pkmActual.Version, _pkmActual.Context, false);
        page.Preseleccionar(_pkmActual.MetLocation);

        // 👇 ESTA ES LA PARTE QUE TIENES QUE CAMBIAR 👇
        page.AlSeleccionarLugar = (item) =>
        {
            // 1. CORRECCIÓN: El valor es un número (int), NO un GameVersion
            int idLugar = item.Value;

            lblLugarEncuentro.Text = item.Text;

            if (_pkmActual != null)
            {
                // 2. CORRECCIÓN: Convertimos el int a (ushort) para guardarlo
                _pkmActual.MetLocation = (ushort)idLugar;
            }
        };
        // 👆 HASTA AQUÍ EL CAMBIO 👆

        await Navigation.PushModalAsync(page);
    }

    private async void AbrirBuscadorBatalla_Tapped(object sender, TappedEventArgs e)
    {
        var searchPage = new GamesSearchPage();
        // CONECTAMOS EL CABLE
        searchPage.AlSeleccionarJuego = (item) =>
        {
            // 1. Poner el nombre en el Label visual
            lblVersionBatalla.Text = item.Text;
        };
        await Navigation.PushModalAsync(searchPage);
    }

    private async void AbrirBuscadorJuego_Tapped(object sender, TappedEventArgs e)
    {
        var page = new PkHexA.Views.Pickers.GamesSearchPage();

        // Preseleccionar
        if (_pkmActual != null)
        {
            // Convertimos el Enum a int para enviarlo al buscador
            page.Preseleccionar(_pkmActual.Version);
        }

        page.AlSeleccionarJuego = (item) =>
        {
            // item.Text es el Nombre ("Espada")
            // item.Value es el ID numérico (44)

            lblJuegoOrigen.Text = item.Text;

            if (_pkmActual != null)
            {
                // 🔥 AQUÍ ESTABA EL ERROR: Convertimos int a GameVersion
                _pkmActual.Version = (GameVersion)item.Value;

                // Resetear lugar porque cambió el juego
                _pkmActual.MetLocation = 0;
                lblLugarEncuentro.Text = "- (0)";
            }
        };

        await Navigation.PushModalAsync(page);
    }

    // Checkbox "Como Huevo"
    private void OnEggCheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        LayoutHuevo.IsVisible = e.Value;
        if (_pkmActual != null)
        {
            _pkmActual.IsEgg = e.Value;
        }
    }

    // Checkbox "Fatídico"
    private void OnFatefulCheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        if (_pkmActual != null) _pkmActual.FatefulEncounter = e.Value;
    }

    // Cambio de Nivel de Encuentro
    private void OnMetLevelChanged(object sender, TextChangedEventArgs e)
    {
        // Error CS0266: Convertir int a byte
        if (_pkmActual != null && int.TryParse(e.NewTextValue, out int val))
        {
            // Validamos que no pase de 100 para evitar crash por desbordamiento
            val = Math.Clamp(val, 1, 100);
            _pkmActual.MetLevel = (byte)val; // 👈 EL CASTEO IMPORTANTE
        }
    }

    private void OnMetDateSelected(object sender, DateChangedEventArgs e)
    {
        if (_pkmActual != null)
        {
            // 1. Obtenemos la fecha segura (si es nula, usa hoy)
            DateTime fechaSegura = e.NewDate ?? DateTime.Now;

            // 2. CORRECCIÓN: Agregamos (byte) antes de cada valor
            // Esto obliga al compilador a aceptar el número aunque sea int.
            _pkmActual.MetYear = (byte)fechaSegura.Year;   // <--- AQUÍ ESTABA EL ERROR
            _pkmActual.MetMonth = (byte)fechaSegura.Month;
            _pkmActual.MetDay = (byte)fechaSegura.Day;
        }
    }
    // ========================================================================
    // LÓGICA PESTAÑA ENCUENTRO
    // ========================================================================


    private void CargarDatosEncuentro()
    {
        if (_pkmActual == null) return;

        // 1. JUEGO DE ORIGEN
        // Error CS1061: Si GameList no existe, probamos 'gamelist' (minúscula)
        string[] juegos = GameInfo.Strings.gamelist;

        // Error CS0019: Comparar GameVersion con int
        // Solución: Convertimos el Enum a (int)
        if ((int)_pkmActual.Version < juegos.Length)
            lblJuegoOrigen.Text = juegos[(int)_pkmActual.Version];
        else
            lblJuegoOrigen.Text = $"ID {_pkmActual.Version}";

        // 2. LUGAR
        lblLugarEncuentro.Text = GetLocationName(_pkmActual.MetLocation, false);

        // 3. POKÉ BALL
        // Error CS1061: Probamos 'balllist' (minúscula)
        string[] balls = GameInfo.Strings.balllist;

        if (_pkmActual.Ball < balls.Length)
            lblPokeBall.Text = balls[_pkmActual.Ball];
        else
            lblPokeBall.Text = $"Ball {_pkmActual.Ball}";

        // 4. NIVEL
        txtMetLevel.Text = _pkmActual.MetLevel.ToString();

        // 5. FECHA
        // Error CS0121 (Ambigüedad): Convertimos todo a (int) dentro del Clamp
        try
        {
            int year = Math.Clamp((int)_pkmActual.MetYear, 2000, 2099);
            int month = Math.Clamp((int)_pkmActual.MetMonth, 1, 12);
            int day = Math.Clamp((int)_pkmActual.MetDay, 1, DateTime.DaysInMonth(year, month));

            dateEncuentro.Date = new DateTime(year, month, day);
        }
        catch { dateEncuentro.Date = DateTime.Now; }

        chkFateful.IsChecked = _pkmActual.FatefulEncounter;

        // 6. HUEVO
        chkIsEgg.IsChecked = _pkmActual.IsEgg;
        if (LayoutHuevo != null) LayoutHuevo.IsVisible = _pkmActual.IsEgg;

        if (_pkmActual.IsEgg)
        {
            lblLugarHuevo.Text = GetLocationName(_pkmActual.EggLocation, true);
            try
            {
                int year = Math.Clamp((int)_pkmActual.EggYear, 2000, 2099);
                int month = Math.Clamp((int)_pkmActual.EggMonth, 1, 12);
                int day = Math.Clamp((int)_pkmActual.EggDay, 1, DateTime.DaysInMonth(year, month));

                dateHuevo.Date = new DateTime(year, month, day);
            }
            catch { dateHuevo.Date = DateTime.Now; }
        }


        // Usamos la fuente de datos que tiene la relación { Valor, Texto }
        var fuenteVersiones = GameInfo.Sources.VersionDataSource;

        // El ID del juego que tiene el Pokémon (ej: 44 para Espada)
        int idJuego = (int)_pkmActual.Version;

        // Buscamos en la lista el objeto que tenga ese Value exacto
        // NO usamos índice directo array[i], usamos BÚSQUEDA
        var juegoEncontrado = fuenteVersiones.FirstOrDefault(x => x.Value == idJuego);

        if (juegoEncontrado != null)
        {
            lblJuegoOrigen.Text = juegoEncontrado.Text;
        }
        else
        {
            // Si es una versión rara o homebrew
            lblJuegoOrigen.Text = _pkmActual.Version.ToString();
        }
    }




    // Método auxiliar para obtener nombre del lugar
    // Método auxiliar corregido para PKHeX.Core Reciente
    private string GetLocationName(int locationId, bool isEgg)
    {
        // 1. Obtenemos los datos necesarios del Pokémon actual
        var formato = (byte)_pkmActual.Format;
        var generacion = (byte)_pkmActual.Generation;
        var versionJuego = (GameVersion)_pkmActual.Version; // Convertimos int a Enum
        var locId = (ushort)locationId; // Convertimos int a ushort

        // 2. Llamamos al método con los 5 parámetros que pide la foto
        return GameInfo.GetLocationName(
            isEgg,          // bool isEggLocation
            locId,          // ushort location
            formato,        // byte format
            generacion,     // byte generation
            versionJuego    // GameVersion version
        );
    }
    // ========================================================================
    // EVENTOS DE GENERACIÓN DE PID (Botones de arriba)
    // ========================================================================

    private void OnGeneratePID(object sender, EventArgs e)
    {
        if (_pkmActual == null) return;

        // Generamos un número aleatorio de 32 bits (uint)
        // Util.Rand32() es de PKHeX.Core, si no, usa Random de C#
        _pkmActual.PID = PKHeX.Core.Util.Rand32();

        // Actualizamos el campo de texto (Lo mostramos en Hexadecimal "X" o Decimal)
        txtPID.Text = _pkmActual.PID.ToString("X8"); // Formato Hexadecimal (ej: A1B2C3D4)

        // Al cambiar el PID, a veces cambian características (Habilidad, Naturaleza, Shininess)
        // Refrescamos la UI por si acaso
        ActualizarFormas();
        // ActualizarShinyVisual(); // Si tuvieras un indicador visual
    }

    private void OnGenerateShinyPID(object sender, EventArgs e)
    {
        if (_pkmActual == null) return;

        // PKHeX tiene un método mágico para hacerlo Shiny
        // Esto recalcula el PID para que coincida con el TID/SID y sea Shiny
        _pkmActual.SetShiny();

        // Actualizamos el texto del PID nuevo
        txtPID.Text = _pkmActual.PID.ToString("X8");

        // Opcional: Avisar al usuario
        // GlobalService.ShowAlertAsync("¡Pokémon ahora es Shiny!");
    }

    private async void AbrirBuscadorObjeto_Tapped(object sender, TappedEventArgs e)
    {
        if (_pkmActual == null) return;

        // Ahora sí existe la clase
        var page = new PkHexA.Views.Pickers.ItemSearchPage();

        // Opcional: Preseleccionar el item actual
        page.Preseleccionar(_pkmActual.HeldItem);

        page.AlSeleccionarItem = (item) =>
        {
            int idItem = item.Value;

            // Guardamos en el Pokémon
            _pkmActual.HeldItem = idItem;

            // Actualizamos solo el texto en la UI (como acordamos)
            CargarDatos(_pkmActual);
        };

        await Navigation.PushModalAsync(page);
    }
}