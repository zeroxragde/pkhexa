using PKHeX.Core;
using PkHexA.Services;

namespace PkHexA
{
    public partial class MainPage : ContentPage
    {


        public MainPage()
        {
            InitializeComponent();    // Cargar traducciones después de renderizar
            Loaded += (s, e) => UpdateLanguageUI();
            LblVersion.Text = GlobalService.version;


        }
        private void UpdateLanguageUI()
        {
            //LblNew.Text = LanguageService.Get("btnNew");
            //  LblOpen.Text = LanguageService.Get("btnOpen");
            //  lblConfiguracion.Text = LanguageService.Get("btnSettings");
            AutoTraductor.Traducir(this);
        }


        private async void OnOpenFileClicked(object sender, EventArgs e)
        {
            try
            {

                var customFileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    // Android usa MIME types genéricos (no extensiones)
                    { DevicePlatform.Android, new[] { "application/octet-stream",
                        "application/x-binary", 
                        "application/zip",
                        "application/sav" } },

                    // Windows permite extensiones reales
                    { DevicePlatform.WinUI, new[] { ".sav", ".bin", ".pk", ".bak", ".main", ".zip" } },


                });
                var result = await FilePicker.Default.PickAsync(new PickOptions
                {
                    PickerTitle = LanguageService.Get("titleSelectSave"),
                    FileTypes = customFileTypes
                });

                if (result != null)
                {
                    string res;
                    if (!File.Exists(result.FullPath))
                        await GlobalService.ShowAlertAsync(LanguageService.Get("alertArchivoNoEncontrado"));

                    // Usa la lógica interna de PKHeX
                    var saveFile = SaveUtil.GetSaveFile(result.FullPath);
                    if (saveFile == null)
                    {
                        await GlobalService.ShowAlertAsync(LanguageService.Get("alertArchivoNoReconocido"));
                    }
                    else {

                        GlobalService.ACTUAL_FILE = saveFile;
                        LanguageService.CambiarIdiomaPKHeX();
                        var gen = saveFile.Generation;
                        var game = saveFile.GetType().Name.Replace("SAV", "");
                        res = $"Detectado: Generación {gen} - {game}";
                        await Navigation.PushModalAsync(new NavigationPage(new Views.Editor())
                        {
                            BarBackgroundColor = Colors.Transparent,
                            BarTextColor = Colors.White
                        });
                    }
                }


            }
            catch (Exception ex)
            {
                await GlobalService.ShowAlertAsync(ex.Message);
            }
            // await DisplayAlert("Open", "Abrir pantalla de opciones o ajustes", "OK");
        }

        private async void OnSettingsClicked(object sender, EventArgs e)
        {
            await GlobalService.ShowAlertAsync("Abrir pantalla de opciones o ajustes");
        }




    }
}
