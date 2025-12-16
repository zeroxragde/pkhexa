using Microsoft.Maui.Controls;
using PKHeX.Core;
using PkHexA.Helper;          // TU LIBRERÍA: Para SpriteHelper
using PkHexA.LibSprites.Util; // TU LIBRERÍA: Para SpriteImgLoader
using SkiaSharp;              // Necesario para manejar SKBitmap

namespace PkHexA.Views.Pickers;

public partial class MoveSelector : ContentView
{
    public event EventHandler<EventArgs>? MoveTapped;
    public event EventHandler? OnStatsChanged;
    private EntityContext _context = EntityContext.Gen9; // Valor por defecto
    private int _moveId;

    public MoveSelector()
    {
        InitializeComponent();
        pickerPPUps.SelectedIndex = 0;
    }

    // --- PROPIEDADES ---

    public int MoveId
    {
        get => _moveId;
        set
        {
            _moveId = value;
            UpdateTypeSprite(value);

            if (value == 0)
            {
                lblMoveName.Text = "(Ninguno)";
                entryPP.Text = "0";
                pickerPPUps.SelectedIndex = 0;
                chkMastery.IsChecked = false;
                imgType.Source = null;
            }
            else {
                var listaRaw = GameInfo.Strings.Move;
                lblMoveName.Text = listaRaw[value];
            }
        }
    }

    public string MoveName
    {
        get => lblMoveName.Text;
        set => lblMoveName.Text = value;
    }

    public int PP
    {
        get => int.TryParse(entryPP.Text, out int val) ? val : 0;
        set => entryPP.Text = value.ToString();
    }

    public int PPUps
    {
        get => pickerPPUps.SelectedIndex < 0 ? 0 : pickerPPUps.SelectedIndex;
        set
        {
            int safeVal = Math.Clamp(value, 0, 3);
            pickerPPUps.SelectedIndex = safeVal;
        }
    }

    public bool IsMastery
    {
        get => chkMastery.IsChecked;
        set => chkMastery.IsChecked = value;
    }

    public bool ShowMastery
    {
        get => chkMastery.IsVisible;
        set => chkMastery.IsVisible = value;
    }
    // Nueva Propiedad: Contexto (Gen1, Gen3, Gen9, etc.)
    public EntityContext Context
    {
        get => _context;
        set
        {
            if (_context != value)
            {
                _context = value;
                // Si cambia el contexto, hay que refrescar el icono (ej: Curse)
                UpdateTypeSprite(_moveId);
            }
        }
    }
    // --- MÉTODOS ---

    private void UpdateTypeSprite(int moveId)
    {
        if (moveId <= 0)
        {
            imgType.Source = null;
            return;
        }

        // 1. Obtener el ID del Tipo usando el CONTEXTO real
        // Esto devuelve el entero del tipo (0=Normal, 1=Lucha, 9=Fuego, etc.)
        int typeId = MoveInfo.GetType((ushort)moveId, _context);

        // 2. Mapeo para tus archivos (Astral 18 -> 99)
        if (typeId == 18) typeId = 99;
        else if (typeId > 18 || typeId < 0) typeId = 0;

        // 3. Formato exacto de tus archivos: type_icon_01.png
        string resourceName = $"type_icon_{typeId:00}";

        // 4. Cargar con tu librería
        var skBitmap = SpriteImgLoader.LoadSprite(resourceName);

        if (skBitmap != null)
            imgType.Source = SpriteHelper.SafeImageSourceFromSKBitmap(skBitmap);
        else
            imgType.Source = null;
    }
    private void OnTapped(object sender, TappedEventArgs e) => MoveTapped?.Invoke(this, EventArgs.Empty);

    private void OnStatInputChanged(object sender, object e) => OnStatsChanged?.Invoke(this, EventArgs.Empty);


}
