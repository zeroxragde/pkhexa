using PKHeX.Core;
using PKHeX.Drawing.PokeSprite;
using PkHexA.Services;

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
        // Pokémon básico (Pikachu como ejemplo)
        var pk = new PK8
        {
            Species = 25,
            Form = 0,
            Gender = 0,
            HeldItem = 0,
            CurrentLevel = 50
        };

        // Generar sprite con tus DLL
        var bmp = pk.Sprite();

        if (bmp == null)
        {
            Console.WriteLine("SPRITE ES NULL");
            return;
        }

        Console.WriteLine($"W={bmp.Width}, H={bmp.Height}");

        // Convertir y aplicar a tu Image
        imgPokemon.Source = bmp.ToImageSource();
    }
}