namespace PkHexA.Views.Pickers;

public partial class MoveSelector : ContentView
{
    // Evento para cuando tocas el control (Click)
    public event EventHandler<EventArgs>? MoveTapped;

    public MoveSelector()
    {
        InitializeComponent();
    }

    // Propiedad para el Nombre del Movimiento
    public string MoveName
    {
        get => lblMoveName.Text;
        set => lblMoveName.Text = value;
    }

    // Propiedad para mostrar/ocultar Alerta
    public bool IsIllegal
    {
        get => lblAlert.IsVisible;
        set => lblAlert.IsVisible = value;
    }

    // Propiedad para la imagen del tipo
    public ImageSource TypeImage
    {
        get => imgType.Source;
        set => imgType.Source = value;
    }

    // Manejador del Tap interno
    private void OnTapped(object sender, TappedEventArgs e)
    {
        MoveTapped?.Invoke(this, EventArgs.Empty);
    }
}