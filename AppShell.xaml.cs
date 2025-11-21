using PkHexA.Services;
using static System.Net.Mime.MediaTypeNames;

namespace PkHexA
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
     
        }
        protected override void OnAppearing() // <--- AGREGAR ESTO
        {
            base.OnAppearing();

            // Usamos el método nuevo específico para Shell
            AutoTraductor.TraducirShell(this);
        }
        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }
    }
}
