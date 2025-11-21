using PKHeX.Core;
using PkHexA.Services;

namespace PkHexA.Views;

public partial class PkmEditor : ContentPage
{
    // Variable para guardar el ID actual (para cuando guardes el archivo .pkm)
    private int _currentSpeciesId;
    public PkmEditor()
	{
		InitializeComponent();
		Title = LanguageService.Get("winPkmEditor");
        tabInicioEditPkm.Text = LanguageService.Get("tabPkmEditInicio");
        var speciesList = GameInfo.Sources.SpeciesDataSource.ToList();
        FijarPokemon(25);

    }

    // AL HACER CLICK EN EL "PICKER FALSO"

    private async void AbrirBuscador_Tapped(object sender, TappedEventArgs e)
    {
        var searchPage = new PokemonSearchPage();

        // CONECTAMOS EL CABLE
        searchPage.AlSeleccionarPokemon = (item) =>
        {
            // "item" es tu objeto de la base de datos
            // Asumiendo que tiene propiedades Text y Value:

            // 1. Poner el nombre en el Label visual
            lblPokemonSeleccionado.Text = item.Text;

            // 2. (Opcional) Usar el ID para lógica interna
            // int idPokemon = item.Value; 
            // Console.WriteLine($"Seleccionó ID: {idPokemon}");
        };

        await Navigation.PushModalAsync(searchPage);
    }
    public void FijarPokemon(int speciesId)
    {
        // 1. Le preguntamos al experto (PokemonSearchPage) quién es este ID
        var info = PokemonSearchPage.ObtenerInfoPokemon(speciesId);

        if (info != null)
        {
            // 2. Actualizamos la pantalla
            // Usamos (dynamic) para acceder a .Text sin líos
            lblPokemonSeleccionado.Text = ((dynamic)info).Text;

            // 3. Guardamos el ID internamente
            _currentSpeciesId = speciesId;
        }
        else
        {
            lblPokemonSeleccionado.Text = $"Desconocido ({speciesId})";
            _currentSpeciesId = speciesId;
        }
    }
    /*
private void CargarListaEspecies()
{
   var speciesList = GameInfo.Sources.SpeciesDataSource.ToList();

   PickerSpecies.ItemsSource = speciesList;

   // Seleccionar por defecto Pikachu (especie = 25) pok
   var pikachuItem = speciesList.FirstOrDefault(s => s.Value == 25);
   if (pikachuItem != null)
       PickerSpecies.SelectedItem = pikachuItem;

   // Buscar la especie con ID = 25 (Pikachu)
   var pikachu = speciesList.FirstOrDefault(s => s.Value == 25);

   if (pikachu != null)
       PickerSpecies.SelectedItem = pikachu;
}*/
}