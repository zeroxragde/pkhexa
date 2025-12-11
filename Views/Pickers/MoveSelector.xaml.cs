using PKHeX.Core;
using Microsoft.Maui.Controls;

namespace PkHexA.Views.Pickers;

public partial class MoveSelector : ContentView
{
    // Evento para cuando tocan el nombre (abrir buscador)
    public event EventHandler<EventArgs>? MoveTapped;

    // Evento para cuando cambian los PP o PP Ups manualmente
    public event EventHandler? OnStatsChanged;

    private int _moveId;

    public MoveSelector()
    {
        InitializeComponent();
        pickerPPUps.SelectedIndex = 0;
    }

    // --- PROPIEDADES ---

    public ImageSource TypeImage
    {
        get => imgType.Source;
        set => imgType.Source = value;
    }

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
                TypeImage = null;
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
        get
        {
            if (int.TryParse(entryPP.Text, out int val)) return val;
            return 0;
        }
        set => entryPP.Text = value.ToString();
    }

    public int PPUps
    {
        get => pickerPPUps.SelectedIndex == -1 ? 0 : pickerPPUps.SelectedIndex;
        set
        {
            if (value < 0) value = 0;
            if (value > 3) value = 3;
            pickerPPUps.SelectedIndex = value;
        }
    }

    public bool IsIllegal
    {
        get => lblAlert.IsVisible;
        set => lblAlert.IsVisible = value;
    }

    // --- MÉTODOS LÓGICOS ---

    private void UpdateTypeSprite(int moveId)
    {
        if (moveId <= 0)
        {
            imgType.Source = null;
            return;
        }

        // Obtener ID del tipo usando PKHeX (Gen 9 por defecto)
        int typeId = MoveInfo.GetType((ushort)moveId, EntityContext.Gen9);

        // SEGURIDAD: Si el tipo es mayor a 18 (Estelar), forzamos 0 para evitar error de Glide
        // Asegúrate de tener type_0.png hasta type_18.png en Resources/Images
        if (typeId > 18 || typeId < 0) typeId = 0;

        imgType.Source = $"type_{typeId}.png";
    }

    public void HealPP(PKM pk)
    {
        if (MoveId == 0)
        {
            PP = 0;
            PPUps = 0;
            return;
        }

        // Calcula los PP máximos legales
        int maxPP = pk.GetMovePP((ushort)MoveId, PPUps);
        PP = maxPP;
    }

    // --- MANEJADORES DE EVENTOS ---

    private void OnTapped(object sender, TappedEventArgs e)
    {
        MoveTapped?.Invoke(this, EventArgs.Empty);
    }

    private void OnStatInputChanged(object sender, EventArgs e)
    {
        // Disparar evento para que la página principal actualice el PKM
        OnStatsChanged?.Invoke(this, EventArgs.Empty);
    }
}