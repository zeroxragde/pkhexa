using PKHeX.Core;
using PkHexA.LibSprites.Util;
using PkHexA.Services;
using PkHexA.Views.Pickers;

namespace PkHexA.Views.Tabs;

public partial class TabEncuentroView : ContentView
{
    private PKM _pkm;

    public TabEncuentroView()
    {
        InitializeComponent();
    }

    // =========================================================
    // 1. CARGA DE DATOS
    // =========================================================
    public void CargarDatos(PKM pkm)
    {
        _pkm = pkm;
        if (_pkm == null) return;

        // --- Juego de Origen ---
        string[] juegos = GameInfo.Strings.gamelist;
        if ((int)_pkm.Version < juegos.Length)
            lblJuegoOrigen.Text = juegos[(int)_pkm.Version];
        else
            lblJuegoOrigen.Text = $"ID {_pkm.Version}";

        // --- Lugar de Encuentro ---
        lblLugarEncuentro.Text = GetLocationName(_pkm.MetLocation, false);

        // --- Poké Ball ---
        string[] balls = GameInfo.Strings.balllist;
        if (_pkm.Ball < balls.Length)
            lblPokeBall.Text = balls[_pkm.Ball];
        else
            lblPokeBall.Text = $"Ball {_pkm.Ball}";

        // Cargar imagen de la ball (Usando GlobalService/SpriteUtil)
        imgPokeBall.Source = GlobalService.SKBitmapToImageSource(SpriteUtil.GetBallSprite((byte)_pkm.Ball));

        // --- Niveles ---
        txtMetLevel.Text = _pkm.MetLevel.ToString();
        // Obediencia (si aplica en la estructura del PKM)
        try
        {
            dynamic p = _pkm;
            txtObedienceLevel.Text = p.OtFriendship.ToString(); // A veces se usa este campo u otro dependiente de la Gen
        }
        catch { txtObedienceLevel.Text = "0"; }

        // --- Fecha Encuentro ---
        try
        {
            int year = Math.Clamp((int)_pkm.MetYear, 2000, 2099);
            int month = Math.Clamp((int)_pkm.MetMonth, 1, 12);
            int day = Math.Clamp((int)_pkm.MetDay, 1, DateTime.DaysInMonth(year, month));
            dateEncuentro.Date = new DateTime(year, month, day);
        }
        catch { dateEncuentro.Date = DateTime.Now; }

        // --- Checks ---
        chkFateful.IsChecked = _pkm.FatefulEncounter;
        chkIsEgg.IsChecked = _pkm.IsEgg;
        LayoutHuevo.IsVisible = _pkm.IsEgg;

        // --- Datos de Huevo ---
        if (_pkm.IsEgg)
        {
            lblLugarHuevo.Text = GetLocationName(_pkm.EggLocation, true);
            try
            {
                int year = Math.Clamp((int)_pkm.EggYear, 2000, 2099);
                int month = Math.Clamp((int)_pkm.EggMonth, 1, 12);
                int day = Math.Clamp((int)_pkm.EggDay, 1, DateTime.DaysInMonth(year, month));
                dateHuevo.Date = new DateTime(year, month, day);
            }
            catch { dateHuevo.Date = DateTime.Now; }
        }
    }

    // =========================================================
    // 2. MÉTODOS DE APOYO
    // =========================================================
    private string GetLocationName(int locationId, bool isEgg)
    {
        if (_pkm == null) return $"- ({locationId})";

        var formato = (byte)_pkm.Format;
        var generacion = (byte)_pkm.Generation;
        var versionJuego = (GameVersion)_pkm.Version;
        var locId = (ushort)locationId;

        return GameInfo.GetLocationName(isEgg, locId, formato, generacion, versionJuego);
    }

    // =========================================================
    // 3. EVENTOS (Buscadores)
    // =========================================================

    private async void AbrirBuscadorJuego_Tapped(object sender, TappedEventArgs e)
    {
        var page = new GamesSearchPage();
        if (_pkm != null) page.Preseleccionar(_pkm.Version);

        page.AlSeleccionarJuego = (item) =>
        {
            if (_pkm == null) return;

            lblJuegoOrigen.Text = item.Text;
            _pkm.Version = (GameVersion)item.Value;

            // Al cambiar de juego, el lugar de encuentro suele invalidarse o cambiar de contexto
            _pkm.MetLocation = 0;
            lblLugarEncuentro.Text = GetLocationName(0, false);
        };
        await Navigation.PushModalAsync(page);
    }

    private async void AbrirBuscadorBatalla_Tapped(object sender, TappedEventArgs e)
    {
        // Este campo suele ser para "Battle Version" en generaciones recientes
        var p = new GamesSearchPage();
        p.AlSeleccionarJuego = (item) =>
        {
            lblVersionBatalla.Text = item.Text;
            // Aquí deberías asignar a la propiedad correspondiente si existe en PKM, ej:
            // _pkm.BattleVersion = item.Value; (Depende de la API de PKHeX)
        };
        await Navigation.PushModalAsync(p);
    }

    private async void AbrirBuscadorLugar_Tapped(object sender, TappedEventArgs e)
    {
        if (_pkm == null) return;
        var page = new LocationSearchPage();

        // Cargamos lugares filtrados por la versión actual del PKM
        page.CargarLugares(_pkm.Version, _pkm.Context, false); // false = no huevo
        page.Preseleccionar(_pkm.MetLocation);

        page.AlSeleccionarLugar = (item) =>
        {
            lblLugarEncuentro.Text = item.Text;
            _pkm.MetLocation = (ushort)item.Value;
        };
        await Navigation.PushModalAsync(page);
    }

    private async void AbrirBuscadorLugarHuevo_Tapped(object sender, TappedEventArgs e)
    {
        if (_pkm == null) return;
        var page = new LocationSearchPage();

        // true = lugares de huevo (Daycare, Nursery, etc.)
        page.CargarLugares(_pkm.Version, _pkm.Context, true);
        page.Preseleccionar(_pkm.EggLocation);

        page.AlSeleccionarLugar = (item) =>
        {
            lblLugarHuevo.Text = item.Text;
            _pkm.EggLocation = (ushort)item.Value;
        };
        await Navigation.PushModalAsync(page);
    }

    private async void AbrirBuscadorBall_Tapped(object sender, TappedEventArgs e)
    {
        if (_pkm == null) return;
        var page = new BallSearchPage();
        page.Preseleccionar(_pkm.Ball);

        page.AlSeleccionarBall = (item) =>
        {
            lblPokeBall.Text = item.Text;
            _pkm.Ball = (byte)item.Value;
            imgPokeBall.Source = GlobalService.SKBitmapToImageSource(SpriteUtil.GetBallSprite((byte)_pkm.Ball));
        };
        await Navigation.PushModalAsync(page);
    }

    // =========================================================
    // 4. EVENTOS (Cambios de Valor)
    // =========================================================

    private void OnMetLevelChanged(object sender, TextChangedEventArgs e)
    {
        if (_pkm != null && int.TryParse(e.NewTextValue, out int v))
            _pkm.MetLevel = (byte)Math.Clamp(v, 1, 100);
    }

    private void OnObedienceLevelChanged(object sender, TextChangedEventArgs e)
    {
        // Lógica opcional si manejas obediencia/afecto
    }

    private void OnMetDateSelected(object sender, DateChangedEventArgs e)
    {
        if (_pkm != null && e.NewDate.HasValue)
        {
            _pkm.MetYear = (byte)e.NewDate.Value.Year;
            _pkm.MetMonth = (byte)e.NewDate.Value.Month;
            _pkm.MetDay = (byte)e.NewDate.Value.Day;
        }
    }

    private void OnEggDateSelected(object sender, DateChangedEventArgs e)
    {
        if (_pkm != null && e.NewDate.HasValue)
        {
            _pkm.EggYear = (byte)e.NewDate.Value.Year;
            _pkm.EggMonth = (byte)e.NewDate.Value.Month;
            _pkm.EggDay = (byte)e.NewDate.Value.Day;
        }
    }

    private void OnFatefulCheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        if (_pkm != null) _pkm.FatefulEncounter = e.Value;
    }

    private void OnEggCheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        if (LayoutHuevo != null) LayoutHuevo.IsVisible = e.Value;
        if (_pkm != null) _pkm.IsEgg = e.Value;
    }
}