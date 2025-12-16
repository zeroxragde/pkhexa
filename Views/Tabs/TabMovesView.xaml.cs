using PKHeX.Core;
using PkHexA.Views.Pickers;

namespace PkHexA.Views.Tabs;

public partial class TabMovesView : ContentView
{
    private PKM? _pkm;

    public TabMovesView()
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

        // Actualizamos la UI con los datos del objeto
        CargarDatosMovimientosVisuales();

        // Lógica de visualización específica (ej. Legends Arceus tiene Mastery)
        // Puedes agregar aquí: lblHeaderMastery.IsVisible = (_pkm.Format == 8);
    }

    private void CargarDatosMovimientosVisuales()
    {
        // Pasamos los datos del objeto a los controles personalizados (MoveSelector)
        viewMove1.MoveId = _pkm.Move1;
        viewMove1.PPUps = _pkm.Move1_PPUps;
        viewMove1.PP = _pkm.Move1_PP;

        viewMove2.MoveId = _pkm.Move2;
        viewMove2.PPUps = _pkm.Move2_PPUps;
        viewMove2.PP = _pkm.Move2_PP;

        viewMove3.MoveId = _pkm.Move3;
        viewMove3.PPUps = _pkm.Move3_PPUps;
        viewMove3.PP = _pkm.Move3_PP;

        viewMove4.MoveId = _pkm.Move4;
        viewMove4.PPUps = _pkm.Move4_PPUps;
        viewMove4.PP = _pkm.Move4_PP;
    }

    // =========================================================
    // 2. EVENTOS DE CAMBIO MANUAL (PP / PPUps)
    // =========================================================

    // Este evento se dispara cuando el usuario cambia los valores numéricos en el control MoveSelector
    private void GuardarStats_Changed(object sender, EventArgs e)
    {
        if (_pkm == null) return;

        // Guardamos los valores de la vista al objeto PKM
        _pkm.Move1_PP = viewMove1.PP;
        _pkm.Move1_PPUps = viewMove1.PPUps;

        _pkm.Move2_PP = viewMove2.PP;
        _pkm.Move2_PPUps = viewMove2.PPUps;

        _pkm.Move3_PP = viewMove3.PP;
        _pkm.Move3_PPUps = viewMove3.PPUps;

        _pkm.Move4_PP = viewMove4.PP;
        _pkm.Move4_PPUps = viewMove4.PPUps;
    }

    // =========================================================
    // 3. EVENTOS DE BÚSQUEDA (Taps)
    // =========================================================

    private async void AbrirBuscadorMovimiento1_Tapped(object sender, EventArgs e) => await AbrirBuscadorMovimiento(1);
    private async void AbrirBuscadorMovimiento2_Tapped(object sender, EventArgs e) => await AbrirBuscadorMovimiento(2);
    private async void AbrirBuscadorMovimiento3_Tapped(object sender, EventArgs e) => await AbrirBuscadorMovimiento(3);
    private async void AbrirBuscadorMovimiento4_Tapped(object sender, EventArgs e) => await AbrirBuscadorMovimiento(4);

    private async Task AbrirBuscadorMovimiento(int slotIndex)
    {
        if (_pkm == null) return;

        // Pasamos el Context para filtrar movimientos según la generación del Pokémon
        var searchPage = new MoveSearchPage(_pkm.Context);

        searchPage.AlSeleccionarMovimiento = (moveId, moveName) =>
        {
            ushort id = (ushort)moveId;

            // Calculamos los PP base correctos para ese movimiento
            int nuevosPP = _pkm.GetMovePP(id, 0);

            // Asignamos al slot correspondiente
            switch (slotIndex)
            {
                case 1:
                    _pkm.Move1 = id;
                    _pkm.Move1_PPUps = 0;     // Resetear PP Ups
                    _pkm.Move1_PP = nuevosPP; // Asignar PP base
                    break;
                case 2:
                    _pkm.Move2 = id;
                    _pkm.Move2_PPUps = 0;
                    _pkm.Move2_PP = nuevosPP;
                    break;
                case 3:
                    _pkm.Move3 = id;
                    _pkm.Move3_PPUps = 0;
                    _pkm.Move3_PP = nuevosPP;
                    break;
                case 4:
                    _pkm.Move4 = id;
                    _pkm.Move4_PPUps = 0;
                    _pkm.Move4_PP = nuevosPP;
                    break;
            }

            // Actualizamos la vista para reflejar el nuevo ataque y sus PP
            CargarDatosMovimientosVisuales();
        };

        await Navigation.PushModalAsync(searchPage);
    }
}