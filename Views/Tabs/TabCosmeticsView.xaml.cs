using PKHeX.Core;
using PkHexA.Services;

namespace PkHexA.Views.Tabs;

public partial class TabCosmeticsView : ContentView
{




    private PKM _pkm;
    private bool _isLoading = false;

    // Índices de bits (Marcas)
    private const int MARK_CIRCLE = 0;
    private const int MARK_TRIANGLE = 1;
    private const int MARK_SQUARE = 2;
    private const int MARK_HEART = 3;
    private const int MARK_STAR = 4;
    private const int MARK_DIAMOND = 5;

    public TabCosmeticsView()
    {
        InitializeComponent();

        // --- ARREGLO VISUAL (QUITA LO ROJO) ---
        // Cambiamos el texto de los botones para usar símbolos que aceptan color
        if (btnMarkHeart != null) btnMarkHeart.Text = "❤";
        if (btnMarkDiamond != null) btnMarkDiamond.Text = "◆";

        if (pickerTeraType != null)
            pickerTeraType.ItemsSource = GameInfo.Strings.types;
    }

    public void CargarDatos(PKM pkm)
    {
        _pkm = pkm;
        if (_pkm == null) return;

        _isLoading = true;

        // 1. Visibilidad (Ocultar Tera en Z-A, etc.)
        ActualizarVisibilidadPorJuego();

        // 2. Cargar Marcas
        // Aquí está el cambio: Si el PKM no tiene soporte para marcas, 
        // ActualizarVisualMarcas simplemente cargará 0 y no dará error.
        ActualizarVisualMarcas();

        // Habilitar botones solo si no es Gen 1/2
        HabilitarMarcas(_pkm.Format >= 3);

        // 3. Cargar Stats (Con protección anti-errores)
        CargarEntry(nudSpirit, "CurrentFriendship", "OT_Friendship");
        CargarEntry(nudMood, "CurrentAffection", "OT_Affection");
        CargarEntry(nudWalkingMood, "WalkingMood");
        CargarEntry(nudFame, "PokeStarFame");
        CargarEntry(txtScale, "Scale");
        CargarEntry(txtCP, "CombatPower", "CP");

        // 4. Iconos
        ActualizarIconosEstado();

        _isLoading = false;
    }

    // =========================================================
    // LÓGICA DE MARCAS
    // =========================================================

    private void ActualizarVisualMarcas()
    {
        // Si MarkValue no existe, esto devuelve 0 silenciosamente
        int marks = GetIntValue("MarkValue") ?? 0;

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
            // === ACTIVADO: DORADO ===
            btn.TextColor = Colors.Gold;
            btn.Opacity = 1.0;
            btn.Scale = 1.2;
        }
        else
        {
            // === DESACTIVADO: GRIS ===
            btn.TextColor = Colors.Gray;
            btn.Opacity = 0.3;
            btn.Scale = 1.0;
        }
    }

    private void HabilitarMarcas(bool habilitar)
    {
        if (btnMarkCircle == null) return;

        btnMarkCircle.IsEnabled = habilitar;
        btnMarkTriangle.IsEnabled = habilitar;
        btnMarkSquare.IsEnabled = habilitar;
        btnMarkHeart.IsEnabled = habilitar;
        btnMarkStar.IsEnabled = habilitar;
        btnMarkDiamond.IsEnabled = habilitar;
    }

    private void ToggleMarking_Clicked(object sender, EventArgs e)
    {
        if (_pkm == null || sender is not Button btn) return;

        // 1. Identificar qué botón fue
        int bit = -1;
        if (btn == btnMarkCircle) bit = MARK_CIRCLE;
        else if (btn == btnMarkTriangle) bit = MARK_TRIANGLE;
        else if (btn == btnMarkSquare) bit = MARK_SQUARE;
        else if (btn == btnMarkHeart) bit = MARK_HEART;
        else if (btn == btnMarkStar) bit = MARK_STAR;
        else if (btn == btnMarkDiamond) bit = MARK_DIAMOND;

        if (bit == -1) return;

        // 2. Intentar leer valor actual
        int? currentMarksNull = GetIntValue("MarkValue");

        // Si devuelve NULL, significa que este PKM no soporta marcas. Salimos.
        if (currentMarksNull == null) return;

        int currentMarks = currentMarksNull.Value;
        int newMarks = currentMarks ^ (1 << bit); // Invertir bit

        // 3. Guardar
        SetIntValue("MarkValue", newMarks);

        // 4. Refrescar UI
        ActualizarVisualMarcas();
    }

    // =========================================================
    // VISIBILIDAD Y OTROS
    // =========================================================

    private void ActualizarVisibilidadPorJuego()
    {
        if (_pkm == null) return;

        // Usamos propiedades de la librería PKHeX
        bool esSV = _pkm.SV;
        bool esZA = _pkm.ZA;
        bool esPLA = _pkm.LA;

        if (boxTera != null) boxTera.IsVisible = esSV;
        if (boxAlpha != null) boxAlpha.IsVisible = (esPLA || esZA);
    }

    private void ToggleAlpha_Tapped(object sender, EventArgs e)
    {
        if (_pkm == null) return;
        bool isAlpha = GetBoolValue("IsAlpha");
        SetBoolValue("IsAlpha", !isAlpha);
        if (boxAlpha != null) boxAlpha.Opacity = !isAlpha ? 1.0 : 0.5;
    }

    private void ToggleFavorite_Tapped(object sender, EventArgs e)
    {
        if (_pkm == null) return;
        bool isFav = GetBoolValue("IsFavorite");
        SetBoolValue("IsFavorite", !isFav);
        ActualizarIconosEstado();
    }

    private void ActualizarIconosEstado()
    {
        bool isFav = GetBoolValue("IsFavorite");
        lblFavorite.TextColor = isFav ? Colors.DeepPink : Colors.Gray;

        bool esShiny = _pkm.IsShiny;
        lblShinyIcon.Opacity = esShiny ? 1.0 : 0.1;

        int pkrs = _pkm.PokerusStrain;
        lblPokerusIcon.Opacity = pkrs > 0 ? 1.0 : 0.1;
    }

    private void OnTeraTypeChanged(object sender, EventArgs e)
    {
        if (_isLoading || _pkm == null) return;
        int idx = pickerTeraType.SelectedIndex;
        SetIntValue("TeraType", idx);
        SetIntValue("TeraTypeOriginal", idx);
    }

    private void OnStatInputChanged(object sender, TextChangedEventArgs e)
    {
        if (_isLoading || _pkm == null || sender is not Entry entry) return;
        if (int.TryParse(entry.Text, out int val))
        {
            if (entry == nudSpirit) SetIntValue("CurrentFriendship", val);
            else if (entry == nudMood) SetIntValue("CurrentAffection", val);
            else if (entry == nudWalkingMood) SetIntValue("WalkingMood", val);
            else if (entry == nudFame) SetIntValue("PokeStarFame", val);
            else if (entry == txtScale) SetIntValue("Scale", val);
            else if (entry == txtCP) SetIntValue("CombatPower", val);
        }
    }

    private void OpenRibbons_Clicked(object sender, EventArgs e)
    {
        if (Application.Current?.MainPage != null)
            Application.Current.MainPage.DisplayAlert("Info", "Editor de Cintas próximamente.", "OK");
    }

    // =========================================================
    // HELPERS SILENCIOSOS (La clave para que no falle)
    // =========================================================

    private void CargarEntry(Entry entry, string prop1, string prop2 = null)
    {
        if (entry == null) return;
        int? val = GetIntValue(prop1);
        if (val == null && prop2 != null) val = GetIntValue(prop2);

        if (val.HasValue)
        {
            entry.Text = val.Value.ToString();
            entry.IsEnabled = true;
        }
        else
        {
            entry.Text = "0";
            entry.IsEnabled = false;
            entry.Opacity = 0.3;
        }
    }

    // Devuelve NULL si la propiedad no existe, en vez de lanzar error
    private int? GetIntValue(string propName)
    {
        try
        {
            var prop = _pkm.GetType().GetProperty(propName);

            // Intento secundario para versiones viejas
            if (prop == null && propName == "MarkValue")
                prop = _pkm.GetType().GetProperty("Markings");

            if (prop != null)
                return Convert.ToInt32(prop.GetValue(_pkm));
        }
        catch { }

        return null; // NO encontró nada, regresa null calladito
    }

    // No hace nada si la propiedad no existe
    private void SetIntValue(string propName, int value)
    {
        try
        {
            var prop = _pkm.GetType().GetProperty(propName);

            if (prop == null && propName == "MarkValue")
                prop = _pkm.GetType().GetProperty("Markings");

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
        try
        {
            var prop = _pkm.GetType().GetProperty(propName);
            if (prop != null) return (bool)prop.GetValue(_pkm);
        }
        catch { }
        return false;
    }

    private void SetBoolValue(string propName, bool value)
    {
        try
        {
            var prop = _pkm.GetType().GetProperty(propName);
            if (prop != null && prop.CanWrite) prop.SetValue(_pkm, value);
        }
        catch { }
    }








}