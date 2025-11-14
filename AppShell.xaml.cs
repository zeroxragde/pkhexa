using PkHexA.Services;
using static System.Net.Mime.MediaTypeNames;

namespace PkHexA
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            shellHome.Title = LanguageService.Get("menuHome");
        }
        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }
    }
}
