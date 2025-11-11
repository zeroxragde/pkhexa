using PKHexA.Services;

namespace PKHexA
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            MainThread.BeginInvokeOnMainThread(UpdateLanguageUI);
        }
        private void UpdateLanguageUI()
        {
            LblNew.Text = LanguageService.Get("btnNew");
            LblOpen.Text = LanguageService.Get("btnOpen");
          //  lblCreator.Text = LanguageService.Get("creatorText");
        }
        private async void OnNewFileClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Nuevo", "Aquí podrías crear un nuevo archivo de guardado.", "OK");
        }

        private async void OnOpenFileClicked(object sender, EventArgs e)
        {
            /*  try
              {
                  var result = await FilePicker.PickAsync(new PickOptions
                  {
                      PickerTitle = "Selecciona un archivo .sav",
                      FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                  {
                      { DevicePlatform.Android, new[] { ".sav", ".main", ".bin", ".dat" } },
                      { DevicePlatform.WinUI, new[] { ".sav", ".main", ".bin", ".dat" } }
                  })
                  });

                  if (result == null)
                      return;

                  byte[] bytes = File.ReadAllBytes(result.FullPath);
                  var save = SaveUtil.GetVariantSAV(result.FullPath);

                  await DisplayAlert("Archivo cargado", $"Juego detectado: {save.Game}", "OK");
              }
              catch (Exception ex)
              {
                  await DisplayAlert("Error", ex.Message, "Cerrar");
              }*/
            await DisplayAlert("Open", "Abrir pantalla de opciones o ajustes", "OK");
        }

        private async void OnSettingsClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Configuración", "Abrir pantalla de opciones o ajustes", "OK");
        }




    }

}
