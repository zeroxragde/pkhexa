using PKHeX.Core;
using PkHexA.Helper;
using PkHexA.Helper;
using PkHexA.LibSprites.Util;
using PkHexA.Services;
using SkiaSharp;

namespace PkHexA.Views;

public partial class Editor : ContentPage
{
    public Editor()
    {
        InitializeComponent();
        MostrarSpriteDePrueba();

    }

    private void MostrarSpriteDePrueba()
    {
        var pk = new PK8
        {
            Species = 25,
            Form = 0,
            Gender = 0,
            HeldItem = 0,
            CurrentLevel = 50
        };
        //PKHeX.Core.CommonEdits.SetShiny(pk,Shiny.Never);
       

        SKBitmap bmp = pk.Sprite();  // Tu loader ya funciona

        Task.Run(() =>
        {
            // CONVERSIÓN SEGURA
            var safeSource = SpriteHelper.SafeImageSourceFromSKBitmap(bmp);

            MainThread.BeginInvokeOnMainThread(() =>
            {
                imgPokemon.Source = safeSource;
            });
        });
    }

    private void OnExitClicked(object sender, EventArgs e)
    {

    }

    private async void OnEditPkmClicked(object sender, EventArgs e)
    {
        await Application.Current.MainPage.Navigation.PushModalAsync(
                new NavigationPage(new Views.PkmEditor())
                {
                    BarBackgroundColor = Colors.Transparent,
                    BarTextColor = Colors.White
                });
    }
}
