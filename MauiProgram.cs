using Microsoft.Extensions.Logging;
using PKHexA.Services;

namespace PKHexA;

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
			});
        // 🔹 Inicializar el servicio de idiomas al arrancar la app
        Task.Run(async () => await LanguageService.InitializeAsync());

#if DEBUG
        builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
