using PKHeX.Core;
using PkHexA.Services;

namespace PkHexA.Views;

public partial class PkmEditor : ContentPage
{
	public PkmEditor()
	{
		InitializeComponent();
		Title = LanguageService.Get("winPkmEditor");
        tabInicioEditPkm.Text = LanguageService.Get("tabPkmEditInicio");
        var speciesList = GameInfo.Sources.SpeciesDataSource.ToList();
        CargarListaEspecies();
    }

    private void CargarListaEspecies()
    {
        var speciesList = GameInfo.Sources.SpeciesDataSource.ToList();

        PickerSpecies.ItemsSource = speciesList;

        // Seleccionar por defecto Pikachu (especie = 25)
        var pikachuItem = speciesList.FirstOrDefault(s => s.Value == 25);
        if (pikachuItem != null)
            PickerSpecies.SelectedItem = pikachuItem;

        // Buscar la especie con ID = 25 (Pikachu)
        var pikachu = speciesList.FirstOrDefault(s => s.Value == 25);

        if (pikachu != null)
            PickerSpecies.SelectedItem = pikachu;
    }
}