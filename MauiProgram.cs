using Microsoft.Extensions.Logging;
using PKHeX.Core;
using PkHexA.Controls;
#if ANDROID
using PkHexA.Platforms.Android;
#endif
using PkHexA.Services;

namespace PkHexA
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                })
                .ConfigureMauiHandlers(handlers =>
                {
#if ANDROID
                    // AQUÍ ESTÁ LA MAGIA:
                    handlers.AddHandler(typeof(CustomPicker), typeof(CustomPickerHandler));
#endif
                });
            Task.Run(async () => await LanguageService.InitializeAsync()).Wait();

#if DEBUG
            builder.Logging.AddDebug();
#endif
            LanguageService.CambiarIdiomaPKHeX();
            return builder.Build();
        }
    }
}
