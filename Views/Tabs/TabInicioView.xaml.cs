using PKHeX.Core;
using PkHexA.Views.Pickers;

namespace PkHexA.Views.Tabs;

public partial class TabInicioView : ContentView
{
    private PKM _pkm;
    private string[] _formasDisponibles = Array.Empty<string>();
    private int _currentSpeciesId; // Variable auxiliar que usabas en el original

    public TabInicioView()
    {
        InitializeComponent();
    }

    // =========================================================
    // 1. CARGA DE DATOS (Fiel al original)
    // =========================================================
    public void CargarDatos(PKM pkm)
    {
        _pkm = pkm;
        if (_pkm == null) return;

        // --- 1. IDs y Claves ---
        txtPID.Text = _pkm.PID.ToString("X8");

        // Verificamos existencia por si el control está oculto/borrado en XAML
        if (this.FindByName("txtEC") != null)
            txtEC.Text = _pkm.EncryptionConstant.ToString("X8");

        // --- 2. Especie y Forma ---
        // Usamos tu método original lógica "FijarPokemon"
        FijarEspecie(_pkm.Species);

        // --- 3. Textos Simples ---
        NicknameEntry.Text = _pkm.Nickname;
        LevelEntry.Text = _pkm.CurrentLevel.ToString();
        ExpEntry.Text = _pkm.EXP.ToString();
        FriendshipEntry.Text = _pkm.CurrentFriendship.ToString();

        // --- 4. Pickers (Género / Idioma) ---
        if (_pkm.Gender <= 2) GenderPicker.SelectedIndex = _pkm.Gender;

        // Ajuste de idioma (PKHeX usa 1-based, Picker usa 0-based)
        int langIndex = Math.Clamp(_pkm.Language - 1, 0, LanguagePicker.Items.Count - 1);
        LanguagePicker.SelectedIndex = langIndex;

        // --- 5. Objeto (Lógica original) ---
        int itemID = _pkm.HeldItem;
        var listaItems = GameInfo.Strings.itemlist;

        if (itemID < listaItems.Length)
            lblObjetoSeleccionado.Text = listaItems[itemID];
        else
            lblObjetoSeleccionado.Text = $"Item {itemID}";

        // --- 6. Checkboxes de Estado ---
        EggCheck.IsChecked = _pkm.IsEgg;

        // Lógica Pokerus segura
        try
        {
            dynamic p = _pkm;
            int strain = p.PKRS_Strain;
            int days = p.PKRS_Days;
            InfectedCheck.IsChecked = strain > 0 && days > 0;
            CuredCheck.IsChecked = strain > 0 && days == 0;
        }
        catch
        {
            InfectedCheck.IsChecked = false;
            CuredCheck.IsChecked = false;
        }

        // --- 7. Extras ---
        CargarDatosExtra();
    }

    // =========================================================
    // 2. MÉTODOS DE APOYO (Lógica Restaurada)
    // =========================================================

    // Renombrado a FijarEspecie (o puedes ponerle FijarPokemon si prefieres)
    // Pero mantiene TU lógica original de llamar a PokemonSearchPage.ObtenerInfoPokemon
    private void FijarEspecie(int speciesId)
    {
        if (_pkm == null) return;

        // Nota: En el original asignabas _pkmActual.Species aquí.
        // Como en esta vista recibimos el objeto ya cargado, solo actualizamos UI si es necesario,
        // pero por seguridad mantenemos la asignación.
        _pkm.Species = (ushort)speciesId;

        // Llamada a tu método estático original
        var info = PokemonSearchPage.ObtenerInfoPokemon(speciesId);

        if (info != null)
        {
            // Usando dynamic como en tu código original
            lblPokemonSeleccionado.Text = ((dynamic)info).Text;
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
        if (_pkm == null) return;

        // Lógica original: usar FormaSearchPage para obtener lista
        _formasDisponibles = FormaSearchPage.ObtenerFormasDelPokemon(_pkm);

        bool tieneFormas = _formasDisponibles.Length > 1;
        LayoutForma.IsVisible = tieneFormas;

        if (tieneFormas)
        {
            if (_pkm.Form < _formasDisponibles.Length)
                lblFormaSeleccionada.Text = _formasDisponibles[_pkm.Form];
            else
            {
                _pkm.Form = 0;
                lblFormaSeleccionada.Text = _formasDisponibles[0];
            }
        }
        else
        {
            _pkm.Form = 0;
            lblFormaSeleccionada.Text = "Normal";
        }
    }

  

    private void CargarDatosExtra()
    {
        try
        {
            dynamic p = _pkm;
            txtCatchRate.Text = p.CatchRate.ToString();

            if (BorderExtra.IsVisible && LayoutShadowFields.IsVisible)
            {
                txtShadowID.Text = p.ShadowID.ToString();
                txtHeartGauge.Text = p.Purification.ToString();
                chkIsShadow.IsChecked = p.ShadowID > 0;
            }
        }
        catch { }
    }

    // =========================================================
    // 3. EVENTOS (Botones, Taps, Cambios)
    // =========================================================

    private void OnGeneratePID(object sender, EventArgs e)
    {
        if (_pkm != null)
        {
            _pkm.PID = PKHeX.Core.Util.Rand32();
            txtPID.Text = _pkm.PID.ToString("X8");
            ActualizarFormas();
        }
    }

    private void OnGenerateShinyPID(object sender, EventArgs e)
    {
        if (_pkm != null)
        {
            _pkm.SetShiny();
            txtPID.Text = _pkm.PID.ToString("X8");
        }
    }

    private async void AbrirBuscador_Tapped(object sender, TappedEventArgs e)
    {
        // Protección contra doble clic rápido o datos no cargados
        if (_pkm == null) return;

        var page = new PokemonSearchPage();

        page.AlSeleccionarPokemon = (item) =>
        {
            // 1. Actualizamos el objeto PKM
            _pkm.Species = (ushort)item.Value;
            _pkm.Form = 0; // Resetear forma

            // 2. Limpiamos mote (Opcional, igual que el original)
            _pkm.Nickname = "";
            NicknameEntry.Text = "";

            // 3. Llamamos al método central que actualiza la UI y las Formas
            // Esto evita que tengas código duplicado o errores de variables
            FijarEspecie(item.Value);
        };

        await Navigation.PushModalAsync(page);
    }

    // Evento: Click en Naturaleza
    private async void AbrirBuscadorNaturaleza_Tapped(object sender, TappedEventArgs e)
    {
        var page = new PkHexA.Views.Pickers.NaturalezaSearchPage();
        page.AlSeleccionarNatura = (item) =>
        {
            // CORRECCIÓN: Agregamos (Nature) para convertir el int
            _pkm.Nature = (Nature)item.Value;
            lblNaturalezaSeleccionado.Text = item.Text;
        };
        await Navigation.PushModalAsync(page);
    }

    // Evento: Click en Estado (Mentas/StatNature)
    private async void AbrirBuscadorEstadoNaturaleza_Tapped(object sender, TappedEventArgs e)
    {
        var page = new PkHexA.Views.Pickers.NaturalezaSearchPage();
        page.AlSeleccionarNatura = (item) =>
        {
            // CORRECCIÓN: Agregamos (Nature) aquí también
            _pkm.StatNature = (Nature)item.Value;
            lblNaturalezaEstadoSeleccionado.Text = item.Text;
        };
        await Navigation.PushModalAsync(page);
    }

    private async void AbrirBuscadorObjeto_Tapped(object sender, TappedEventArgs e)
    {
        var page = new ItemSearchPage();
        page.Preseleccionar(_pkm.HeldItem);
        page.AlSeleccionarItem = (item) =>
        {
            _pkm.HeldItem = item.Value;
            lblObjetoSeleccionado.Text = item.Text;
        };
        await Navigation.PushModalAsync(page);
    }

    private async void AbrirBuscadorHabilidad_Tapped(object sender, TappedEventArgs e)
    {
        var page = new HabilidadSearchPage();
        page.AlSeleccionarHabilidad = (item) =>
        {
            _pkm.Ability = item.Value;
            lblHabilidadSeleccionado.Text = item.Text;
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
            _pkm.Form = (byte)item.Value;
            lblFormaSeleccionada.Text = item.Text;
        };
        await Navigation.PushModalAsync(page);
    }

    private void OnEggCheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        if (_pkm != null) _pkm.IsEgg = e.Value;
    }

    private void OnCatchRateChanged(object sender, TextChangedEventArgs e)
    {
        if (_pkm != null && int.TryParse(e.NewTextValue, out int v))
        {
            try { ((dynamic)_pkm).CatchRate = (byte)Math.Clamp(v, 0, 255); } catch { }
        }
    }

    private void OnShadowIDChanged(object sender, TextChangedEventArgs e)
    {
        if (_pkm != null && int.TryParse(e.NewTextValue, out int v))
        {
            try { ((dynamic)_pkm).ShadowID = v; } catch { }
        }
    }

    private void OnHeartGaugeChanged(object sender, TextChangedEventArgs e)
    {
        if (_pkm != null && int.TryParse(e.NewTextValue, out int v))
        {
            try { ((dynamic)_pkm).Purification = v; } catch { }
        }
    }

    private void OnShadowChanged(object sender, CheckedChangedEventArgs e)
    {
        if (_pkm == null) return;
        try
        {
            dynamic pkm = _pkm;
            if (!e.Value)
            {
                txtShadowID.Text = "0";
                pkm.ShadowID = 0;
            }
            else if (pkm.ShadowID == 0)
            {
                txtShadowID.Text = "1";
                pkm.ShadowID = 1;
            }
        }
        catch { }
    }

    private void OnNSparkleChanged(object sender, CheckedChangedEventArgs e)
    {
    }

    private void OnClearClicked(object sender, EventArgs e)
    {
        txtShadowID.Text = "0";
        txtHeartGauge.Text = "0";
        chkIsShadow.IsChecked = false;
        if (chkNSparkle != null) chkNSparkle.IsChecked = false;
    }

    private void OnResetClicked(object sender, EventArgs e)
    {
        txtCatchRate.Text = "255";
        OnClearClicked(sender, e);
    }
}