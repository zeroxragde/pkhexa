using PkHexA.LibSprites.Util;
using PkHexA.Services;

namespace PkHexA.Views;

public partial class Splash : ContentPage
{
    private bool _initialized = false;

    public Splash()
	{
		InitializeComponent();
           
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_initialized)
            return;

        _initialized = true;

        // Animación inicial
        await LoadingBar.ProgressTo(0.25, 500, Easing.Linear);

        try
        {
            // Mostrar un poquito la pantalla
            await Task.Delay(600);
            await LoadingBar.ProgressTo(0.45, 500, Easing.Linear);

            // Descargar datos
            await SpriteDataInitializerService.EnsureSpriteDataAsync();
            await LoadingBar.ProgressTo(0.85, 600, Easing.Linear);

            // Última parte
            await Task.Delay(300);
            await LoadingBar.ProgressTo(1.0, 400, Easing.Linear);
            // Cambiar ventana
            Application.Current.MainPage = new AppShell();
        }
        catch (Exception ex)
        {
            await DisplayAlert("ERROR", $"Falló la carga de los datos:\n{ex.Message}", "OK");
        }
    }



}