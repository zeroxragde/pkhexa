using Microsoft.Maui.Controls;
using PKHeX.Core;
using PkHexA.Services;
using System;
using System.Reflection;

namespace PkHexA.Views.Tabs;

public partial class TabCosmeticsView : ContentView
{
    private PKM _pkm;
    private bool _isLoading = false;

    // Constantes de Marcas (Bits estándar de Pokémon)
    private const int MARK_CIRCLE = 0;
    private const int MARK_TRIANGLE = 1;
    private const int MARK_SQUARE = 2;
    private const int MARK_HEART = 3;
    private const int MARK_STAR = 4;
    private const int MARK_DIAMOND = 5;

    public TabCosmeticsView()
    {
        InitializeComponent();

        // Inicializar Picker de Tera con Tipos
        if (pickerTeraType != null)
        {
            try { pickerTeraType.ItemsSource = GameInfo.Strings.types; }
            catch { pickerTeraType.ItemsSource = new string[] { "Normal", "Fire", "Water", "Grass", "Electric", "Ice", "Fighting", "Poison", "Ground", "Flying", "Psychic", "Bug", "Rock", "Ghost", "Dragon", "Steel", "Dark", "Fairy", "Stellar" }; }
        }
    }

    /// <summary>
    /// LLAMA A ESTE MÉTODO DESDE TU MAINPAGE AL CARGAR EL SAV
    /// </summary>
    public void CargarDatos(PKM pkm)
    {
        _pkm = pkm;

        if (_pkm == null)
        {
            // Resetear UI si no hay Pokémon
            if (txtHeight != null) txtHeight.Text = "0";
            return;
        }

        _isLoading = true;

        try
        {
            // 1. Visibilidad (Detectar Z-A, SV, PLA)
            ActualizarVisibilidadPorJuego();

            // 2. Marcas y Estado Visual
            ActualizarVisualMarcas();
            ActualizarIconosEstado();

            // 3. Botones Especiales (Tera, Alpha, Mega/Gmax)
            CargarBotonesEspeciales();

            // 4. Dimensiones (Altura/Peso)
            CargarDimensiones();

            // 5. Concursos
            CargarContestStats();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[TabCosmetics] Error: {ex.Message}");
        }
        finally
        {
            _isLoading = false;
        }
    }

    // =========================================================
    // LÓGICA DE DETECCIÓN DE JUEGO
    // =========================================================
    private void ActualizarVisibilidadPorJuego()
    {
        int format = _pkm.Format;
        GameVersion ver = _pkm.Version;

        // Detección "Hack" para Z-A si la librería aún no lo soporta nativamente
        bool isZA = ver.ToString().Contains("ZA") || (format == 9 && ver != GameVersion.SL && ver != GameVersion.VL);

        bool isPLA = ver == GameVersion.PLA;
        bool isSV = format == 9 && !isZA;
        bool isLetGo = (ver == GameVersion.GP || ver == GameVersion.GE);

        // Visibilidad de Paneles
        if (boxTera != null) boxTera.IsVisible = isSV || isZA;
        if (boxAlpha != null) boxAlpha.IsVisible = isPLA || isZA;
        if (boxGmax != null) boxGmax.IsVisible = true; // Mostrar siempre el 3er slot en gens modernas

        bool showDimensions = isPLA || isSV || isZA || isLetGo;
        if (grpDimensions != null) grpDimensions.IsVisible = showDimensions;

        bool showContest = (_pkm.Generation == 3 || _pkm.Generation == 4 ||
                           (_pkm.Generation == 6 && format < 7) ||
                           (_pkm.Generation == 8 && (ver == GameVersion.BD || ver == GameVersion.SP)));

        if (grpContest != null) grpContest.IsVisible = showContest;
    }

    // =========================================================
    // EVENTOS Y LÓGICA DE MARCAS (COLOR AZUL/ROSA)
    // =========================================================

    private void ToggleMarking_Clicked(object sender, EventArgs e)
    {
        if (sender is not Button btn) return;

        // ALERTA DE SEGURIDAD: Si olvidas conectar CargarDatos
        if (_pkm == null)
        {
            Application.Current.MainPage.DisplayAlert("Error", "No se han cargado datos del Pokémon. Revisa MainPage.xaml.cs", "OK");
            return;
        }

        int bit = -1;
        if (btn == btnMarkCircle) bit = MARK_CIRCLE;
        else if (btn == btnMarkTriangle) bit = MARK_TRIANGLE;
        else if (btn == btnMarkSquare) bit = MARK_SQUARE;
        else if (btn == btnMarkHeart) bit = MARK_HEART;
        else if (btn == btnMarkStar) bit = MARK_STAR;
        else if (btn == btnMarkDiamond) bit = MARK_DIAMOND;

        if (bit == -1) return;

        // Leer
        string propName = "MarkValue"; // Gen 7+
        int? currentMarks = GetIntValue(propName);
        if (currentMarks == null)
        {
            propName = "Markings"; // Gen 3-6
            currentMarks = GetIntValue(propName);
        }

        if (currentMarks == null) return;

        // Toggle (XOR)
        int newMarks = currentMarks.Value ^ (1 << bit);
        SetIntValue(propName, newMarks);

        // Actualizar UI
        ActualizarVisualMarcas();
    }

    private void ActualizarVisualMarcas()
    {
        int marks = GetIntValue("MarkValue") ?? GetIntValue("Markings") ?? 0;

        SetMarkColor(btnMarkCircle, marks, MARK_CIRCLE);
        SetMarkColor(btnMarkTriangle, marks, MARK_TRIANGLE);
        SetMarkColor(btnMarkSquare, marks, MARK_SQUARE);
        SetMarkColor(btnMarkHeart, marks, MARK_HEART);
        SetMarkColor(btnMarkStar, marks, MARK_STAR);
        SetMarkColor(btnMarkDiamond, marks, MARK_DIAMOND);
    }

    private void SetMarkColor(Button btn, int map, int bit)
    {
        if (btn == null) return;
        bool isActive = ((map >> bit) & 1) == 1;

        if (isActive)
        {
            // Lógica PKHeX: Corazón y Estrella son ROSA, el resto AZUL
            bool esRosa = (bit == MARK_HEART || bit == MARK_STAR);

            btn.TextColor = esRosa ? Color.FromArgb("#FF66CC") : Color.FromArgb("#3399FF");
            btn.Opacity = 1.0;
            // Un pequeño background sutil ayuda al tacto
            btn.BackgroundColor = Colors.Transparent;
        }
        else
        {
            btn.TextColor = Colors.Gray;
            btn.Opacity = 0.3;
            btn.BackgroundColor = Colors.Transparent;
        }
    }

    // =========================================================
    // LÓGICA DEL RESTO DE LA UI
    // =========================================================

    private void ToggleAlpha_Tapped(object sender, EventArgs e)
    {
        if (_pkm == null) return;
        bool isAlpha = GetBoolValue("IsAlpha");
        SetBoolValue("IsAlpha", !isAlpha);
        if (boxAlpha != null) boxAlpha.Opacity = !isAlpha ? 1.0 : 0.3;
    }

    private void ToggleGmax_Tapped(object sender, EventArgs e)
    {
        if (_pkm == null) return;
        // Intentar detectar propiedad (Gmax, Noble, o Titan/Mega en Z-A)
        string prop = "IsGigantamax";
        if (_pkm.GetType().GetProperty(prop) == null) prop = "IsNoble";

        bool current = GetBoolValue(prop);
        SetBoolValue(prop, !current);
        if (boxGmax != null) boxGmax.Opacity = !current ? 1.0 : 0.3;
    }

    private void CargarBotonesEspeciales()
    {
        // Tera
        if (boxTera != null && boxTera.IsVisible)
        {
            int tera = GetIntValue("TeraTypeOriginal") ?? GetIntValue("TeraType") ?? 0;
            if (tera >= 0 && tera < pickerTeraType.ItemsSource.Count)
                pickerTeraType.SelectedIndex = tera;
        }
        // Alpha
        if (boxAlpha != null)
        {
            bool isAlpha = GetBoolValue("IsAlpha");
            boxAlpha.Opacity = isAlpha ? 1.0 : 0.3;
        }
        // Gmax
        if (boxGmax != null)
        {
            bool state = GetBoolValue("IsGigantamax") || GetBoolValue("IsNoble");
            boxGmax.Opacity = state ? 1.0 : 0.3;
        }
    }

    private void CargarDimensiones()
    {
        CargarEntry(txtHeight, "HeightScalar");
        CargarEntry(txtWeight, "WeightScalar");
        if (txtHeight != null) ActualizarEtiquetaTamano(lblHeightSize, txtHeight.Text);
        if (txtWeight != null) ActualizarEtiquetaTamano(lblWeightSize, txtWeight.Text);
    }

    private void ActualizarEtiquetaTamano(Label lbl, string valStr)
    {
        if (lbl == null) return;
        if (int.TryParse(valStr, out int val))
        {
            if (val <= 6) lbl.Text = "XXS";
            else if (val <= 76) lbl.Text = "XS";
            else if (val <= 178) lbl.Text = "M";
            else if (val <= 248) lbl.Text = "XL";
            else lbl.Text = "XXL";
        }
        else lbl.Text = "-";
    }

    private void ActualizarIconosEstado()
    {
        if (lblShinyIcon != null) lblShinyIcon.Opacity = _pkm.IsShiny ? 1.0 : 0.1;

        int pkrs = _pkm.PokerusStrain;
        bool tienePokerus = pkrs > 0 || GetBoolValue("PokerusCured");
        if (lblPokerusIcon != null) lblPokerusIcon.Opacity = tienePokerus ? 1.0 : 0.1;
    }

    private void OnTeraTypeChanged(object sender, EventArgs e)
    {
        if (_isLoading || _pkm == null || pickerTeraType == null) return;
        int idx = pickerTeraType.SelectedIndex;
        if (idx >= 0) { SetIntValue("TeraType", idx); SetIntValue("TeraTypeOriginal", idx); }
    }

    private void OnStatInputChanged(object sender, TextChangedEventArgs e)
    {
        if (_isLoading || _pkm == null || sender is not Entry entry) return;
        if (int.TryParse(entry.Text, out int val))
        {
            if (entry == txtHeight) { SetIntValue("HeightScalar", val); ActualizarEtiquetaTamano(lblHeightSize, entry.Text); }
            else if (entry == txtWeight) { SetIntValue("WeightScalar", val); ActualizarEtiquetaTamano(lblWeightSize, entry.Text); }
            // Concursos
            else if (entry == cntCool) SetIntValue("ContestCool", val);
            else if (entry == cntBeauty) SetIntValue("ContestBeauty", val);
            else if (entry == cntCute) SetIntValue("ContestCute", val);
            else if (entry == cntSmart) SetIntValue("ContestSmart", val);
            else if (entry == cntTough) SetIntValue("ContestTough", val);
            else if (entry == cntSheen) SetIntValue("ContestSheen", val);
        }
    }

    private void OpenRibbons_Clicked(object sender, EventArgs e)
    {
        Application.Current?.MainPage?.DisplayAlert("Info", "Editor de Cintas próximamente.", "OK");
    }

    // =========================================================
    // HELPERS REFLECTION (UNIVERSAL)
    // =========================================================

    private void CargarEntry(Entry entry, string prop1, string prop2 = null)
    {
        if (entry == null) return;
        int? val = GetIntValue(prop1);
        if (val == null && prop2 != null) val = GetIntValue(prop2);
        if (val.HasValue) { entry.Text = val.Value.ToString(); entry.IsEnabled = true; }
        else { entry.Text = "0"; entry.IsEnabled = false; }
    }

    private void CargarContestStats()
    {
        CargarEntry(cntCool, "ContestCool", "Cool");
        CargarEntry(cntBeauty, "ContestBeauty", "Beauty");
        CargarEntry(cntCute, "ContestCute", "Cute");
        CargarEntry(cntSmart, "ContestSmart", "Smart");
        CargarEntry(cntTough, "ContestTough", "Tough");
        CargarEntry(cntSheen, "ContestSheen", "Sheen");
    }

    private int? GetIntValue(string propName)
    {
        try { var prop = _pkm.GetType().GetProperty(propName); if (prop != null) return Convert.ToInt32(prop.GetValue(_pkm)); } catch { }
        return null;
    }

    private void SetIntValue(string propName, int value)
    {
        try
        {
            var prop = _pkm.GetType().GetProperty(propName);
            if (prop == null && propName == "ContestCool") prop = _pkm.GetType().GetProperty("Cool");
            if (prop != null && prop.CanWrite)
            {
                Type t = prop.PropertyType;
                if (t.IsEnum) prop.SetValue(_pkm, Enum.ToObject(t, value));
                else prop.SetValue(_pkm, Convert.ChangeType(value, t));
            }
        }
        catch { }
    }

    private bool GetBoolValue(string propName)
    {
        try { var prop = _pkm.GetType().GetProperty(propName); if (prop != null) return (bool)prop.GetValue(_pkm); } catch { }
        return false;
    }

    private void SetBoolValue(string propName, bool value)
    {
        try { var prop = _pkm.GetType().GetProperty(propName); if (prop != null && prop.CanWrite) prop.SetValue(_pkm, value); } catch { }
    }
}