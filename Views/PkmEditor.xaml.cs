using PKHeX.Core;
using PkHexA.LibSprites.Util;
using PkHexA.Services;
using PkHexA.Views.Pickers;
using SkiaSharp;

namespace PkHexA.Views;

public partial class PkmEditor : ContentPage
{
    #region 1. Variables y Constructor

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
        AutoTraductor.Traducir(this);
        var speciesList = GameInfo.Sources.SpeciesDataSource.ToList();
    }

    #endregion

    #region 2. Lógica de Pestañas (Tabs)

    private void OnTabClicked(object sender, EventArgs e)
    {
        var botonPresionado = sender as Button;
        if (botonPresionado == null) return;

        // 1. APAGAR TODOS LOS BOTONES (Automático)
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

        // 3. MOSTRAR EL PANEL CORRECTO (Automático)
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

    // Helper pequeño para limpiar el código de arriba
    private void ApagarBoton(Button btn)
    {
        if (btn != null)
        {
            btn.BackgroundColor = ColorTabInactivo;
            btn.TextColor = TextoInactivo;
        }
    }

    #endregion

    #region 3. Carga de Datos (Main)

    public void CargarDatos(PKM pkm)
    {
        // 1. Guardamos la referencia global
        _pkmActual = pkm;

        // 2. DETECTAR SI ES NUEVO O EXISTENTE
        if (pkm.Species == 0)
        {
            // CASO A: Es un espacio vacío o Nuevo
            FijarPokemon(1); // Empezamos con Bulbasaur (ID 1)

            // Valores default
            NicknameEntry.Text = "Bulbasaur";
            LevelEntry.Text = "1";
            ExpEntry.Text = "0";
            FriendshipEntry.Text = "70";
            _pkmActual.Ball = 4;
            imgPokeBall.Source = GlobalService.SKBitmapToImageSource(SpriteUtil.GetBallSprite((byte)_pkmActual.Ball));
        }
        else
        {
            // CASO B: Es un Pokémon existente
            FijarPokemon(pkm.Species);

            // 2. Llenamos los campos de texto con los datos reales
            NicknameEntry.Text = pkm.Nickname;
            LevelEntry.Text = pkm.CurrentLevel.ToString();
            ExpEntry.Text = pkm.EXP.ToString();
            FriendshipEntry.Text = pkm.CurrentFriendship.ToString();

            // 3. Cargamos valores de los Pickers
            GenderPicker.SelectedIndex = pkm.Gender;
        }

        // 4. Forzar actualización visual de forma y habilidad
        ActualizarFormas();

        int itemID = _pkmActual.HeldItem;
        var listaItems = GameInfo.Strings.itemlist;

        if (itemID < listaItems.Length)
            lblObjetoSeleccionado.Text = listaItems[itemID];
        else
            lblObjetoSeleccionado.Text = $"Item {itemID}";

        // --- LLAMADAS A SUB-MÉTODOS ---
        CargarDatosEncuentro();
        CargarDatosStats();                 // <--- NUEVO: Para llenar la pestaña Stats
       
    }

    public void FijarPokemon(int speciesId)
    {
        if (_pkmActual == null) return;

        _pkmActual.Species = (ushort)speciesId;
        _pkmActual.Form = 0; // SIEMPRE resetear forma al cambiar de bicho

        var info = PokemonSearchPage.ObtenerInfoPokemon(speciesId);

        if (info != null)
        {
            lblPokemonSeleccionado.Text = ((dynamic)info).Text;
            NicknameEntry.Text = "";  // Limpiamos el nickname al cambiar de especie
            _currentSpeciesId = speciesId;
            ActualizarFormas();
        }
        else
        {
            lblPokemonSeleccionado.Text = $"Desconocido ({speciesId})";
            _currentSpeciesId = speciesId;
        }
    }

    private void ActualizarFormas()
    {
        if (_pkmActual == null) return;

        _formasDisponibles = FormaSearchPage.ObtenerFormasDelPokemon(_pkmActual);
        bool tieneFormas = _formasDisponibles.Length > 1;

        LayoutForma.IsVisible = tieneFormas;

        if (tieneFormas)
        {
            if (_pkmActual.Form < _formasDisponibles.Length)
                lblFormaSeleccionada.Text = _formasDisponibles[_pkmActual.Form];
            else
            {
                _pkmActual.Form = 0;
                lblFormaSeleccionada.Text = _formasDisponibles[0];
            }
        }
        else
        {
            _pkmActual.Form = 0;
        }
    }

    #endregion

    #region 4. Pestaña Encuentro

    private void CargarDatosEncuentro()
    {
        if (_pkmActual == null) return;

        string[] juegos = GameInfo.Strings.gamelist;

        if ((int)_pkmActual.Version < juegos.Length)
            lblJuegoOrigen.Text = juegos[(int)_pkmActual.Version];
        else
            lblJuegoOrigen.Text = $"ID {_pkmActual.Version}";

        lblLugarEncuentro.Text = GetLocationName(_pkmActual.MetLocation, false);

        string[] balls = GameInfo.Strings.balllist;
        if (_pkmActual.Ball < balls.Length)
            lblPokeBall.Text = balls[_pkmActual.Ball];
        else
            lblPokeBall.Text = $"Ball {_pkmActual.Ball}";

        txtMetLevel.Text = _pkmActual.MetLevel.ToString();

        try
        {
            int year = Math.Clamp((int)_pkmActual.MetYear, 2000, 2099);
            int month = Math.Clamp((int)_pkmActual.MetMonth, 1, 12);
            int day = Math.Clamp((int)_pkmActual.MetDay, 1, DateTime.DaysInMonth(year, month));
            dateEncuentro.Date = new DateTime(year, month, day);
        }
        catch { dateEncuentro.Date = DateTime.Now; }

        chkFateful.IsChecked = _pkmActual.FatefulEncounter;
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

        var fuenteVersiones = GameInfo.Sources.VersionDataSource;
        int idJuego = (int)_pkmActual.Version;
        var juegoEncontrado = fuenteVersiones.FirstOrDefault(x => x.Value == idJuego);
        if (juegoEncontrado != null) lblJuegoOrigen.Text = juegoEncontrado.Text;
    }

    private string GetLocationName(int locationId, bool isEgg)
    {
        var formato = (byte)_pkmActual.Format;
        var generacion = (byte)_pkmActual.Generation;
        var versionJuego = (GameVersion)_pkmActual.Version;
        var locId = (ushort)locationId;

        return GameInfo.GetLocationName(isEgg, locId, formato, generacion, versionJuego);
    }

    #endregion

    #region 5. Pestaña Estadísticas (NUEVO)





    // ===========================================
    //   CARGA COMPLETA DE DATOS DEL PKM
    // ===========================================


    // MÉTODO DE VISIBILIDAD POR VERSIÓN DE JUEGO (SAVE)
    // =========================================================

    private void ActualizarVisibilidadPorGeneracionEstadisticas()
    {
        var save = GlobalService.ACTUAL_FILE;
        if (save == null || _pkmActual == null) return;

        GameVersion game = save.Version;
        int gen = save.Generation;

        // --------------------------------------------------------------------
        // 1. DEFINICIÓN DE VARIABLES (ESTRICTAS POR VERSIÓN)
        // --------------------------------------------------------------------

        // TERA: Solo Scarlet (SL) y Violet (VL).
        bool esSV = (game == GameVersion.SL || game == GameVersion.VL);

        // DYNAMAX: Solo Sword (SW) y Shield (SH).
        bool esSwSh = (game == GameVersion.SW || game == GameVersion.SH);

        // LEGENDS: Arceus (PLA) y asumiendo Z-A (ZA).
        bool esPLA = (game == GameVersion.PLA);
        bool esZA = (game == GameVersion.ZA);
        bool esLegends = esPLA || esZA;

        // LET'S GO: Pikachu (GP) y Eevee (GE). 
        // AJUSTA ESTOS ENUMS si tu código usa otros nombres (ej. LGPE).
        bool esLetsGo = (game == GameVersion.GP || game == GameVersion.GE);

        // --------------------------------------------------------------------
        // 2. CONFIGURACIÓN DE VISIBILIDAD (GIMMICKS Y EXTRAS)
        // --------------------------------------------------------------------

        // --- CARACTERÍSTICA ---
        if (LayoutCharacteristic != null)
            LayoutCharacteristic.IsVisible = (gen >= 4);

        // --- PODER OCULTO (Hidden Power) ---
        // Gen 2-7 y BDSP. Excluye SV, SwSh, PLA, ZA, LGPE.
        bool esBDSP = (game == GameVersion.BD || game == GameVersion.SP);
        bool rangoClasico = (gen >= 2 && gen <= 7);
        bool tieneHiddenPower = (rangoClasico || esBDSP) && !esSV && !esSwSh && !esLegends && !esLetsGo;

        if (LayoutHiddenPower != null)
            LayoutHiddenPower.IsVisible = tieneHiddenPower;

        // --- GIMMICKS VISUALES ---
        if (LayoutTera != null) LayoutTera.IsVisible = esSV;
        if (LayoutTeraOriginal != null) LayoutTeraOriginal.IsVisible = esSV;
        if (LayoutDynamax != null) LayoutDynamax.IsVisible = esSwSh;
        if (hsAlpha != null) hsAlpha.IsVisible = esLegends;
        if (hsNoble != null) hsNoble.IsVisible = esPLA; // Noble quizás solo en PLA, no ZA

        if (BorderGimmicks != null)
            BorderGimmicks.IsVisible = esSV || esSwSh || esLegends;

        // --------------------------------------------------------------------
        // 3. CONFIGURACIÓN DE COLUMNAS DE ESTADÍSTICAS (NUEVO)
        // --------------------------------------------------------------------
        // Lógica:
        // - IVs: Siempre visibles (Gen 3+). En Gen 1-2 se calculan desde DVs, pero se suelen mostrar.
        // - EVs: Visibles en todo MENOS Legends y Let's Go.
        // - AVs (Legends): Visibles SOLO en PLA/ZA.
        // - GVs (Let's Go): Visibles SOLO en GP/GE.

        bool mostrarEVs = !esLegends && !esLetsGo;
        bool mostrarAVs = esLegends;
        bool mostrarGVs = esLetsGo;

        // Aplicar anchos de columna (GridLength 0 oculta la columna y reajusta la tabla)
        // ColIV siempre visible (Width = *)
        if (ColIV != null) ColIV.Width = new GridLength(1, GridUnitType.Star);

        // ColEV
        if (ColEV != null)
            ColEV.Width = mostrarEVs ? new GridLength(1, GridUnitType.Star) : new GridLength(0);

        // ColAV (Legends)
        if (ColAV != null)
            ColAV.Width = mostrarAVs ? new GridLength(1, GridUnitType.Star) : new GridLength(0);

        // ColGV (Let's Go)
        if (ColGV != null)
            ColGV.Width = mostrarGVs ? new GridLength(1, GridUnitType.Star) : new GridLength(0);

        // OPCIONAL: Ocultar también los Headers explícitamente para evitar problemas de "Focus" con Tab
        if (HeaderEV != null) HeaderEV.IsVisible = mostrarEVs;
        if (HeaderAV != null) HeaderAV.IsVisible = mostrarAVs;
        if (HeaderGV != null) HeaderGV.IsVisible = mostrarGVs;

        // NOTA: No necesitamos ocultar los Entry fila por fila (txtEVHP, etc), 
        // porque al poner el ancho de la columna en 0, visualmente desaparecen.
        // Sin embargo, si quieres deshabilitar el TAB en ellos, tendrías que recorrerlos.
        // Por ahora, con el Column Width basta para el diseño.
    }
    private void CargarDatosStats()
    {
        if (_pkmActual == null)
            return;

        // -------------------------------------------------
        // 1. BASE STATS
        // -------------------------------------------------
        var pi = _pkmActual.PersonalInfo;

        lblBaseHP.Text = pi.HP.ToString();
        lblBaseAtk.Text = pi.ATK.ToString();
        lblBaseDef.Text = pi.DEF.ToString();
        lblBaseSpa.Text = pi.SPA.ToString();
        lblBaseSpd.Text = pi.SPD.ToString();
        lblBaseSpe.Text = pi.SPE.ToString();

        // -------------------------------------------------
        // 2. IVs
        // -------------------------------------------------
        txtIVHP.Text = _pkmActual.IV_HP.ToString();
        txtIVAtk.Text = _pkmActual.IV_ATK.ToString();
        txtIVDef.Text = _pkmActual.IV_DEF.ToString();
        txtIVSpa.Text = _pkmActual.IV_SPA.ToString();
        txtIVSpd.Text = _pkmActual.IV_SPD.ToString();
        txtIVSpe.Text = _pkmActual.IV_SPE.ToString();

        // -------------------------------------------------
        // 3. EVs
        // -------------------------------------------------
        txtEVHP.Text = _pkmActual.EV_HP.ToString();
        txtEVAtk.Text = _pkmActual.EV_ATK.ToString();
        txtEVDef.Text = _pkmActual.EV_DEF.ToString();
        txtEVSpa.Text = _pkmActual.EV_SPA.ToString();
        txtEVSpd.Text = _pkmActual.EV_SPD.ToString();
        txtEVSpe.Text = _pkmActual.EV_SPE.ToString();

        dynamic pkm = _pkmActual;

        // =========================================================
        // 4. ACTIVAR / DESACTIVAR SECCIONES SEGÚN EL SAVE FILE (JUEGO)
        // =========================================================

        ActualizarVisibilidadPorGeneracionEstadisticas();

        // =========================================================
        // 10. TOTAL STATS
        // =========================================================
        RecalcularStatsTotal();
    }



    // ===========================================
    //   RECALCULAR STATS (COMBATE + TOTALES)
    // ===========================================
    private void RecalcularStatsTotal()
    {
        if (_pkmActual == null)
            return;

        try { ((dynamic)_pkmActual).RefreshStats(); } catch { }

        int[] stats = _pkmActual.Stats;

        lblStatHP.Text = stats[0].ToString();
        lblStatAtk.Text = stats[1].ToString();
        lblStatDef.Text = stats[2].ToString();
        lblStatSpe.Text = stats[3].ToString();
        lblStatSpa.Text = stats[4].ToString();
        lblStatSpd.Text = stats[5].ToString();

        // Totales IV / EV
        int tIV = _pkmActual.IV_HP + _pkmActual.IV_ATK + _pkmActual.IV_DEF +
                  _pkmActual.IV_SPA + _pkmActual.IV_SPD + _pkmActual.IV_SPE;

        int tEV = _pkmActual.EV_HP + _pkmActual.EV_ATK + _pkmActual.EV_DEF +
                  _pkmActual.EV_SPA + _pkmActual.EV_SPD + _pkmActual.EV_SPE;

        lblTotalIVs.Text = $"IV Total: {tIV}/186";
        lblTotalEVs.Text = $"EV Total: {tEV}/510";

        // Totales concursos
        try
        {
            dynamic p = _pkmActual;
            int tC = p.ContestCool + p.ContestBeauty + p.ContestCute +
                     p.ContestSmart + p.ContestTough;

            lblTotalContest.Text = $"Contest Total: {tC}";
        }
        catch
        {
            lblTotalContest.Text = "Contest Total: 0";
        }
    }


    // ===========================================
    //   EVENTOS
    // ===========================================
    private void OnStatChanged(object sender, TextChangedEventArgs e)
    {
        if (_pkmActual == null)
            return;

        // ---- IVs ----
        if (int.TryParse(txtIVHP.Text, out int ivhp)) _pkmActual.IV_HP = ivhp;
        if (int.TryParse(txtIVAtk.Text, out int ivatk)) _pkmActual.IV_ATK = ivatk;
        if (int.TryParse(txtIVDef.Text, out int ivdef)) _pkmActual.IV_DEF = ivdef;
        if (int.TryParse(txtIVSpa.Text, out int ivspa)) _pkmActual.IV_SPA = ivspa;
        if (int.TryParse(txtIVSpd.Text, out int ivspd)) _pkmActual.IV_SPD = ivspd;
        if (int.TryParse(txtIVSpe.Text, out int ivspe)) _pkmActual.IV_SPE = ivspe;

        // ---- EVs ----
        if (int.TryParse(txtEVHP.Text, out int evhp)) _pkmActual.EV_HP = evhp;
        if (int.TryParse(txtEVAtk.Text, out int evatk)) _pkmActual.EV_ATK = evatk;
        if (int.TryParse(txtEVDef.Text, out int evdef)) _pkmActual.EV_DEF = evdef;
        if (int.TryParse(txtEVSpa.Text, out int evspa)) _pkmActual.EV_SPA = evspa;
        if (int.TryParse(txtEVSpd.Text, out int evspd)) _pkmActual.EV_SPD = evspd;
        if (int.TryParse(txtEVSpe.Text, out int evspe)) _pkmActual.EV_SPE = evspe;

        // ---- CONCURSOS ----
        try
        {
            dynamic p = _pkmActual;

            if (int.TryParse(txtContestCool.Text, out int ct1)) p.ContestCool = ct1;
            if (int.TryParse(txtContestBeauty.Text, out int ct2)) p.ContestBeauty = ct2;
            if (int.TryParse(txtContestCute.Text, out int ct3)) p.ContestCute = ct3;
            if (int.TryParse(txtContestSmart.Text, out int ct4)) p.ContestSmart = ct4;
            if (int.TryParse(txtContestTough.Text, out int ct5)) p.ContestTough = ct5;
            if (int.TryParse(txtContestSheen.Text, out int ct6)) p.ContestSheen = ct6;
        }
        catch { }

        // Recalcular todo
        RecalcularStatsTotal();
    }




    // ===========================================
    //   RANDOMIZADORES
    // ===========================================
    private void OnRandomizeIVs(object sender, EventArgs e)
    {
        if (_pkmActual == null) return;

        Random r = new Random();
        _pkmActual.IV_HP = r.Next(32);
        _pkmActual.IV_ATK = r.Next(32);
        _pkmActual.IV_DEF = r.Next(32);
        _pkmActual.IV_SPA = r.Next(32);
        _pkmActual.IV_SPD = r.Next(32);
        _pkmActual.IV_SPE = r.Next(32);

        CargarDatosStats();
    }

    private void OnRandomizeEVs(object sender, EventArgs e)
    {
        if (_pkmActual == null) return;

        Random r = new Random();
        _pkmActual.EV_HP = r.Next(86);
        _pkmActual.EV_ATK = r.Next(86);
        _pkmActual.EV_DEF = r.Next(86);
        _pkmActual.EV_SPA = r.Next(86);
        _pkmActual.EV_SPD = r.Next(86);
        _pkmActual.EV_SPE = r.Next(86);

        CargarDatosStats();
    }


    private void OnRandomizeAll(object sender, EventArgs e)
    {

    }
    // ===========================================
    //   GIMMICKS
    // ===========================================
    private void OnDynamaxLevelChanged(object sender, TextChangedEventArgs e)
    {
        if (_pkmActual == null) return;
        if (int.TryParse(e.NewTextValue, out int val))
        {
            try { ((dynamic)_pkmActual).DynamaxLevel = val; } catch { }
        }
    }

    private void OnGigantamaxChanged(object sender, CheckedChangedEventArgs e)
    {
        if (_pkmActual == null) return;
        try { ((dynamic)_pkmActual).CanGigantamax = e.Value; } catch { }
    }

    private void OnTeraTypeChanged(object sender, EventArgs e)
    {
        if (_pkmActual == null) return;
        if (TeraTypePicker.SelectedIndex >= 0)
        {
            try { ((dynamic)_pkmActual).TeraType = (byte)TeraTypePicker.SelectedIndex; } catch { }
        }
    }









    #endregion


    #region 6. Eventos Simples y Buscadores

    private void OnGeneratePID(object sender, EventArgs e)
    {
        if (_pkmActual != null)
        {
            _pkmActual.PID = PKHeX.Core.Util.Rand32();
            txtPID.Text = _pkmActual.PID.ToString("X8");
            ActualizarFormas();
        }
    }

    private void OnGenerateShinyPID(object sender, EventArgs e)
    {
        if (_pkmActual != null)
        {
            _pkmActual.SetShiny();
            txtPID.Text = _pkmActual.PID.ToString("X8");
        }
    }

    // Buscadores
    private async void AbrirBuscador_Tapped(object sender, TappedEventArgs e)
    {
        var searchPage = new PokemonSearchPage();
        searchPage.AlSeleccionarPokemon = (item) =>
        {
            lblPokemonSeleccionado.Text = item.Text;
            FijarPokemon(item.Value);
        };
        await Navigation.PushModalAsync(searchPage);
    }

    private async void AbrirBuscadorJuego_Tapped(object sender, TappedEventArgs e)
    {
        var page = new GamesSearchPage();
        if (_pkmActual != null) page.Preseleccionar(_pkmActual.Version);
        page.AlSeleccionarJuego = (item) =>
        {
            lblJuegoOrigen.Text = item.Text;
            if (_pkmActual != null)
            {
                _pkmActual.Version = (GameVersion)item.Value;
                _pkmActual.MetLocation = 0;
                lblLugarEncuentro.Text = "- (0)";
            }
        };
        await Navigation.PushModalAsync(page);
    }

    private async void AbrirBuscadorBall_Tapped(object sender, TappedEventArgs e)
    {
        if (_pkmActual == null) return;
        var page = new BallSearchPage();
        page.Preseleccionar(_pkmActual.Ball);
        page.AlSeleccionarBall = (item) =>
        {
            lblPokeBall.Text = item.Text;
            _pkmActual.Ball = (byte)item.Value;
            imgPokeBall.Source = GlobalService.SKBitmapToImageSource(SpriteUtil.GetBallSprite((byte)_pkmActual.Ball));
        };
        await Navigation.PushModalAsync(page);
    }

    private async void AbrirBuscadorObjeto_Tapped(object sender, TappedEventArgs e)
    {
        if (_pkmActual == null) return;
        var page = new ItemSearchPage();
        page.Preseleccionar(_pkmActual.HeldItem);
        page.AlSeleccionarItem = (item) =>
        {
            _pkmActual.HeldItem = item.Value;
            lblObjetoSeleccionado.Text = item.Text;
        };
        await Navigation.PushModalAsync(page);
    }

    private async void AbrirBuscadorLugar_Tapped(object sender, TappedEventArgs e)
    {
        if (_pkmActual == null) return;
        var page = new LocationSearchPage();
        page.CargarLugares(_pkmActual.Version, _pkmActual.Context, false);
        page.Preseleccionar(_pkmActual.MetLocation);
        page.AlSeleccionarLugar = (item) =>
        {
            lblLugarEncuentro.Text = item.Text;
            _pkmActual.MetLocation = (ushort)item.Value;
        };
        await Navigation.PushModalAsync(page);
    }

    private async void AbrirBuscadorForma_Tapped(object sender, TappedEventArgs e)
    {
        if (_formasDisponibles.Length <= 1) return;
        var page = new FormaSearchPage();
        page.CargarFormas(_formasDisponibles);
        page.AlSeleccionarForma = (item) =>
        {
            lblFormaSeleccionada.Text = item.Text;
            if (_pkmActual != null) _pkmActual.Form = (byte)item.Value;
        };
        await Navigation.PushModalAsync(page);
    }

    private async void AbrirBuscadorBatalla_Tapped(object sender, TappedEventArgs e)
    {
        var p = new GamesSearchPage();
        p.AlSeleccionarJuego = (i) => lblVersionBatalla.Text = i.Text;
        await Navigation.PushModalAsync(p);
    }
    private async void AbrirBuscadorNaturaleza_Tapped(object sender, TappedEventArgs e)
    {
        var p = new NaturalezaSearchPage();
        p.AlSeleccionarNatura = (i) => lblNaturalezaSeleccionado.Text = i.Text;
        await Navigation.PushModalAsync(p);
    }
    private async void AbrirBuscadorEstadoNaturaleza_Tapped(object sender, TappedEventArgs e)
    {
        var p = new NaturalezaSearchPage();
        p.AlSeleccionarNatura = (i) => lblNaturalezaEstadoSeleccionado.Text = i.Text;
        await Navigation.PushModalAsync(p);
    }
    private async void AbrirBuscadorHabilidad_Tapped(object sender, TappedEventArgs e)
    {
        var p = new HabilidadSearchPage();
        p.AlSeleccionarHabilidad = (i) => lblHabilidadSeleccionado.Text = i.Text;
        await Navigation.PushModalAsync(p);
    }
    private void AbrirBuscadorLugarHuevo_Tapped(object sender, TappedEventArgs e) { }

    // Eventos Simples (Text changed, checkbox, etc)
    private void OnMetLevelChanged(object sender, TextChangedEventArgs e)
    {
        if (_pkmActual != null && int.TryParse(e.NewTextValue, out int v)) _pkmActual.MetLevel = (byte)Math.Clamp(v, 1, 100);
    }
    private void OnMetDateSelected(object sender, DateChangedEventArgs e)
    {
        if (_pkmActual != null && e.NewDate.HasValue)
        {
            _pkmActual.MetYear = (byte)e.NewDate.Value.Year;
            _pkmActual.MetMonth = (byte)e.NewDate.Value.Month;
            _pkmActual.MetDay = (byte)e.NewDate.Value.Day;
        }
    }
    private void OnEggCheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        if (LayoutHuevo != null) LayoutHuevo.IsVisible = e.Value;
        if (_pkmActual != null) _pkmActual.IsEgg = e.Value;
    }
    private void OnFatefulCheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        if (_pkmActual != null) _pkmActual.FatefulEncounter = e.Value;
    }

    // Eventos Extra (Usando dynamic para CatchRate/Shadow)
    private void OnCatchRateChanged(object sender, TextChangedEventArgs e)
    {
        if (_pkmActual != null && int.TryParse(e.NewTextValue, out int v))
        {
            try { ((dynamic)_pkmActual).CatchRate = (byte)Math.Clamp(v, 0, 255); } catch { }
        }
    }
    private void OnShadowIDChanged(object sender, TextChangedEventArgs e)
    {
        if (_pkmActual != null && int.TryParse(e.NewTextValue, out int v))
        {
            try { ((dynamic)_pkmActual).ShadowID = v; } catch { }
        }
    }
    private void OnHeartGaugeChanged(object sender, TextChangedEventArgs e)
    {
        if (_pkmActual != null && int.TryParse(e.NewTextValue, out int v))
        {
            try { ((dynamic)_pkmActual).Purification = v; } catch { }
        }
    }
    private void OnShadowChanged(object sender, CheckedChangedEventArgs e)
    {
        if (_pkmActual == null) return;
        try
        {
            dynamic pkm = _pkmActual;
            if (!e.Value)
            {
                txtShadowID.Text = "0";
                pkm.ShadowID = 0;
            }
            else if (pkm.ShadowID == 0)
            {
                txtShadowID.Text = "1";
            }
        }
        catch { }
    }
    private void OnNSparkleChanged(object sender, CheckedChangedEventArgs e) { }
    private void OnClearClicked(object sender, EventArgs e)
    {
        txtShadowID.Text = "0";
        txtHeartGauge.Text = "0";
        chkIsShadow.IsChecked = false;
        chkNSparkle.IsChecked = false;
    }
    private void OnResetClicked(object sender, EventArgs e)
    {
        txtCatchRate.Text = "255";
        OnClearClicked(sender, e);
    }

    #endregion



    #region Lógica de Movimientos (Carga, Edición y Búsqueda) - CORREGIDO (MoveX_PP)

    /// <summary>
    /// Pasa los datos del Pokémon a los controles visuales usando las propiedades estándar MoveX_PP.
    /// </summary>
    private void CargarDatosMovimientos()
    {
        if (_pkmActual == null) return;

        // --- MOVIMIENTO 1 ---
        viewMove1.MoveId = _pkmActual.Move1;
        viewMove1.PPUps = _pkmActual.Move1_PPUps; // Propiedad Correcta: Move1_PPUps
        viewMove1.PP = _pkmActual.Move1_PP;       // Propiedad Correcta: Move1_PP

        // --- MOVIMIENTO 2 ---
        viewMove2.MoveId = _pkmActual.Move2;
        viewMove2.PPUps = _pkmActual.Move2_PPUps;
        viewMove2.PP = _pkmActual.Move2_PP;

        // --- MOVIMIENTO 3 ---
        viewMove3.MoveId = _pkmActual.Move3;
        viewMove3.PPUps = _pkmActual.Move3_PPUps;
        viewMove3.PP = _pkmActual.Move3_PP;

        // --- MOVIMIENTO 4 ---
        viewMove4.MoveId = _pkmActual.Move4;
        viewMove4.PPUps = _pkmActual.Move4_PPUps;
        viewMove4.PP = _pkmActual.Move4_PP;
    }

    /// <summary>
    /// Guarda los cambios manuales (PP/Más) usando las propiedades MoveX_PP.
    /// </summary>
    private void GuardarStats_Changed(object sender, EventArgs e)
    {
        if (_pkmActual == null) return;

        // Movimiento 1
        _pkmActual.Move1_PP = viewMove1.PP;
        _pkmActual.Move1_PPUps = viewMove1.PPUps;

        // Movimiento 2
        _pkmActual.Move2_PP = viewMove2.PP;
        _pkmActual.Move2_PPUps = viewMove2.PPUps;

        // Movimiento 3
        _pkmActual.Move3_PP = viewMove3.PP;
        _pkmActual.Move3_PPUps = viewMove3.PPUps;

        // Movimiento 4
        _pkmActual.Move4_PP = viewMove4.PP;
        _pkmActual.Move4_PPUps = viewMove4.PPUps;
    }

    // --- EVENTOS DE APERTURA DEL BUSCADOR ---

    private async void AbrirBuscadorMovimiento1_Tapped(object sender, EventArgs e) => await AbrirBuscadorMovimiento(1);
    private async void AbrirBuscadorMovimiento2_Tapped(object sender, EventArgs e) => await AbrirBuscadorMovimiento(2);
    private async void AbrirBuscadorMovimiento3_Tapped(object sender, EventArgs e) => await AbrirBuscadorMovimiento(3);
    private async void AbrirBuscadorMovimiento4_Tapped(object sender, EventArgs e) => await AbrirBuscadorMovimiento(4);

    /// <summary>
    /// Lógica central para asignar movimiento.
    /// </summary>
    private async Task AbrirBuscadorMovimiento(int slotIndex)
    {
        if (_pkmActual == null) return;

        // 1. Abrir buscador
        var searchPage = new PkHexA.Views.Pickers.MoveSearchPage(_pkmActual.Context);

        // 2. Al seleccionar
        searchPage.AlSeleccionarMovimiento = (moveId) =>
        {
            ushort id = (ushort)moveId;

            // Calculamos los PP base (sin PP Ups inicialmente)
            int nuevosPP = _pkmActual.GetMovePP(id, 0);

            // Asignamos usando las propiedades MoveX_...
            switch (slotIndex)
            {
                case 1:
                    _pkmActual.Move1 = id;
                    _pkmActual.Move1_PPUps = 0;      // Reset Vitaminas
                    _pkmActual.Move1_PP = nuevosPP;  // Asignar PP Base
                    break;
                case 2:
                    _pkmActual.Move2 = id;
                    _pkmActual.Move2_PPUps = 0;
                    _pkmActual.Move2_PP = nuevosPP;
                    break;
                case 3:
                    _pkmActual.Move3 = id;
                    _pkmActual.Move3_PPUps = 0;
                    _pkmActual.Move3_PP = nuevosPP;
                    break;
                case 4:
                    _pkmActual.Move4 = id;
                    _pkmActual.Move4_PPUps = 0;
                    _pkmActual.Move4_PP = nuevosPP;
                    break;
            }

            // 3. Refrescar la pantalla
            CargarDatosMovimientos();
        };

        await Navigation.PushModalAsync(searchPage);
    }

    #endregion


}