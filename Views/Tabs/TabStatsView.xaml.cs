using PKHeX.Core;
using PkHexA.Services;

namespace PkHexA.Views.Tabs;

public partial class TabStatsView : ContentView
{

    private PKM? _pkm;

    public TabStatsView()
    {
        InitializeComponent();
        InicializarTipos();
    }

    private void InicializarTipos()
    {
        // Cargar lista de Tipos para Tera Type Picker
        if (TeraTypePicker.ItemsSource == null || TeraTypePicker.ItemsSource.Count == 0)
        {
            // Usamos GameInfo.Strings.types si está disponible
            TeraTypePicker.ItemsSource = GameInfo.Strings.types;
            if (TeraTypeOriginalPicker != null)
                TeraTypeOriginalPicker.ItemsSource = GameInfo.Strings.types;
        }
    }

    public void CargarDatos(PKM pkm)
    {
        _pkm = pkm;
        if (_pkm == null) return;

        // 1. Cargar Base Stats y Valores
        CargarStatsNumericos();

        // 2. Configurar Visibilidad según Juego/Generación
        ActualizarVisibilidadPorGeneracionEstadisticas();

        // 3. Totales
        RecalcularStatsTotal();

        // 4. Extras (Hidden Power, Gimmicks)
        CargarExtras();
    }

    private void CargarStatsNumericos()
    {
        var pi = _pkm.PersonalInfo;

        // Bases
        lblBaseHP.Text = pi.HP.ToString();
        lblBaseAtk.Text = pi.ATK.ToString();
        lblBaseDef.Text = pi.DEF.ToString();
        lblBaseSpa.Text = pi.SPA.ToString();
        lblBaseSpd.Text = pi.SPD.ToString();
        lblBaseSpe.Text = pi.SPE.ToString();

        // IVs
        txtIVHP.Text = _pkm.IV_HP.ToString();
        txtIVAtk.Text = _pkm.IV_ATK.ToString();
        txtIVDef.Text = _pkm.IV_DEF.ToString();
        txtIVSpa.Text = _pkm.IV_SPA.ToString();
        txtIVSpd.Text = _pkm.IV_SPD.ToString();
        txtIVSpe.Text = _pkm.IV_SPE.ToString();

        // EVs
        txtEVHP.Text = _pkm.EV_HP.ToString();
        txtEVAtk.Text = _pkm.EV_ATK.ToString();
        txtEVDef.Text = _pkm.EV_DEF.ToString();
        txtEVSpa.Text = _pkm.EV_SPA.ToString();
        txtEVSpd.Text = _pkm.EV_SPD.ToString();
        txtEVSpe.Text = _pkm.EV_SPE.ToString();
    }

    private void CargarExtras()
    {
        // Gimmicks usando dynamic para evitar errores de compilación
        try
        {
            dynamic p = _pkm;
            if (LayoutDynamax.IsVisible)
            {
                txtDynamaxLevel.Text = p.DynamaxLevel.ToString();
                chkGigantamax.IsChecked = p.CanGigantamax;
            }
            if (LayoutTera.IsVisible)
            {
                TeraTypePicker.SelectedIndex = p.TeraType;
                if (LayoutTeraOriginal.IsVisible)
                    TeraTypeOriginalPicker.SelectedIndex = p.TeraTypeOriginal;
            }
        }
        catch { }
    }

    private void ActualizarVisibilidadPorGeneracionEstadisticas()
    {
        var save = GlobalService.ACTUAL_FILE;
        if (save == null || _pkm == null) return;

        GameVersion game = save.Version;
        int gen = save.Generation;

        // Variables de entorno
        bool esSV = (game == GameVersion.SL || game == GameVersion.VL);
        bool esSwSh = (game == GameVersion.SW || game == GameVersion.SH);
        bool esLegends = (game == GameVersion.PLA || game == GameVersion.ZA);
        bool esLetsGo = (game == GameVersion.GP || game == GameVersion.GE);

        // Visibilidad Gimmicks
        if (LayoutCharacteristic != null) LayoutCharacteristic.IsVisible = (gen >= 4);

        bool esBDSP = (game == GameVersion.BD || game == GameVersion.SP);
        bool rangoClasico = (gen >= 2 && gen <= 7);
        bool tieneHP = (rangoClasico || esBDSP) && !esSV && !esSwSh && !esLegends && !esLetsGo;
        if (LayoutHiddenPower != null) LayoutHiddenPower.IsVisible = tieneHP;

        if (LayoutTera != null) LayoutTera.IsVisible = esSV;
        if (LayoutTeraOriginal != null) LayoutTeraOriginal.IsVisible = esSV;
        if (LayoutDynamax != null) LayoutDynamax.IsVisible = esSwSh;
        if (hsAlpha != null) hsAlpha.IsVisible = esLegends;

        if (BorderGimmicks != null)
            BorderGimmicks.IsVisible = esSV || esSwSh || esLegends;

        // Columnas
        bool mostrarEVs = !esLegends && !esLetsGo;
        bool mostrarAVs = esLegends;
        bool mostrarGVs = esLetsGo;

        if (ColEV != null) ColEV.Width = mostrarEVs ? new GridLength(1, GridUnitType.Star) : new GridLength(0);
        if (ColAV != null) ColAV.Width = mostrarAVs ? new GridLength(1, GridUnitType.Star) : new GridLength(0);
        if (ColGV != null) ColGV.Width = mostrarGVs ? new GridLength(1, GridUnitType.Star) : new GridLength(0);

        if (HeaderEV != null) HeaderEV.IsVisible = mostrarEVs;
        if (HeaderAV != null) HeaderAV.IsVisible = mostrarAVs;
        if (HeaderGV != null) HeaderGV.IsVisible = mostrarGVs;
    }

    private void RecalcularStatsTotal()
    {
        if (_pkm == null) return;

        try { ((dynamic)_pkm).RefreshStats(); } catch { }

        int[] stats = _pkm.Stats;
        lblStatHP.Text = stats[0].ToString();
        lblStatAtk.Text = stats[1].ToString();
        lblStatDef.Text = stats[2].ToString();
        lblStatSpe.Text = stats[3].ToString();
        lblStatSpa.Text = stats[4].ToString();
        lblStatSpd.Text = stats[5].ToString();

        // Totales Sumados
        int tIV = _pkm.IV_HP + _pkm.IV_ATK + _pkm.IV_DEF + _pkm.IV_SPA + _pkm.IV_SPD + _pkm.IV_SPE;
        int tEV = _pkm.EV_HP + _pkm.EV_ATK + _pkm.EV_DEF + _pkm.EV_SPA + _pkm.EV_SPD + _pkm.EV_SPE;

        lblTotalIVs.Text = $"IV Total: {tIV}/186";
        lblTotalEVs.Text = $"EV Total: {tEV}/510";
    }

    // --- EVENTOS ---

    private void OnStatChanged(object sender, TextChangedEventArgs e)
    {
        if (_pkm == null) return;

        // Parsing básico para IVs/EVs
        if (int.TryParse(txtIVHP.Text, out int ivhp)) _pkm.IV_HP = ivhp;
        if (int.TryParse(txtIVAtk.Text, out int ivatk)) _pkm.IV_ATK = ivatk;
        if (int.TryParse(txtIVDef.Text, out int ivdef)) _pkm.IV_DEF = ivdef;
        if (int.TryParse(txtIVSpa.Text, out int ivspa)) _pkm.IV_SPA = ivspa;
        if (int.TryParse(txtIVSpd.Text, out int ivspd)) _pkm.IV_SPD = ivspd;
        if (int.TryParse(txtIVSpe.Text, out int ivspe)) _pkm.IV_SPE = ivspe;

        if (int.TryParse(txtEVHP.Text, out int evhp)) _pkm.EV_HP = evhp;
        if (int.TryParse(txtEVAtk.Text, out int evatk)) _pkm.EV_ATK = evatk;
        if (int.TryParse(txtEVDef.Text, out int evdef)) _pkm.EV_DEF = evdef;
        if (int.TryParse(txtEVSpa.Text, out int evspa)) _pkm.EV_SPA = evspa;
        if (int.TryParse(txtEVSpd.Text, out int evspd)) _pkm.EV_SPD = evspd;
        if (int.TryParse(txtEVSpe.Text, out int evspe)) _pkm.EV_SPE = evspe;

        // Contest (Concursos)
        try
        {
            dynamic p = _pkm;
            if (int.TryParse(txtContestCool.Text, out int c1)) p.ContestCool = c1;
            // ... agregar resto de contest stats si se desea
        }
        catch { }

        RecalcularStatsTotal();
    }

    private void OnRandomizeIVs(object sender, EventArgs e)
    {
        if (_pkm == null) return;
        var r = new Random();
        _pkm.IV_HP = r.Next(32); _pkm.IV_ATK = r.Next(32); _pkm.IV_DEF = r.Next(32);
        _pkm.IV_SPA = r.Next(32); _pkm.IV_SPD = r.Next(32); _pkm.IV_SPE = r.Next(32);
        CargarStatsNumericos();
        RecalcularStatsTotal();
    }

    private void OnRandomizeEVs(object sender, EventArgs e)
    {
        if (_pkm == null) return;
        var r = new Random();
        // Lógica simple, no legal 100%
        _pkm.EV_HP = r.Next(86); _pkm.EV_ATK = r.Next(86); _pkm.EV_DEF = r.Next(86);
        _pkm.EV_SPA = r.Next(86); _pkm.EV_SPD = r.Next(86); _pkm.EV_SPE = r.Next(86);
        CargarStatsNumericos();
        RecalcularStatsTotal();
    }

    private void OnRandomizeAll(object sender, EventArgs e)
    {
        OnRandomizeIVs(sender, e);
        OnRandomizeEVs(sender, e);
    }

    // --- Gimmicks ---
    private void OnDynamaxLevelChanged(object sender, TextChangedEventArgs e)
    {
        if (_pkm == null) return;
        if (int.TryParse(e.NewTextValue, out int val))
            try { ((dynamic)_pkm).DynamaxLevel = val; } catch { }
    }

    private void OnGigantamaxChanged(object sender, CheckedChangedEventArgs e)
    {
        if (_pkm == null) return;
        try { ((dynamic)_pkm).CanGigantamax = e.Value; } catch { }
    }

    private void OnTeraTypeChanged(object sender, EventArgs e)
    {
        if (_pkm == null) return;
        if (TeraTypePicker.SelectedIndex >= 0)
            try { ((dynamic)_pkm).TeraType = (byte)TeraTypePicker.SelectedIndex; } catch { }
    }

}