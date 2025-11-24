namespace PkHexA.Views;

public partial class CustomAlert : ContentPage
{
    public CustomAlert(string titulo, string mensaje, string textoBoton)
    {
        InitializeComponent();

        lblTitulo.Text = titulo;
        lblMensaje.Text = mensaje;

        // Asignamos el texto personalizado al botón
        // Si viene vacío o nulo, ponemos "OK" por seguridad
        btnOk.Text = string.IsNullOrEmpty(textoBoton) ? "OK" : textoBoton;
    }

    private async void OnOkClicked(object sender, EventArgs e)
    {
        // Cierra la alerta sin animación
        await Navigation.PopModalAsync(animated: false);
    }
}