using PKHeX.Core;
using PkHexA.Views;
using System;
using System.Collections.Generic;
using System.Text;

namespace PkHexA.Services
{
    public static class GlobalService
    {
        public static SaveFile ACTUAL_FILE = FakeSaveFile.Default;
        public static string? CurrentFilePath;
        public static Task ShowAlertAsync(string? message, string? cancel = "OK")
        {
            string title = "PkHexA"; // Título por defecto
            message ??= string.Empty;

            // Nos aseguramos de correr en el Hilo Principal
            return MainThread.InvokeOnMainThreadAsync(async () =>
            {
                if (Shell.Current != null)
                {
                    // 1. Creamos tu alerta personalizada
                    // (Pasamos el botón también para que puedas personalizar el texto "OK")
                    var customAlert = new CustomAlert(title, message, cancel);

                    // 2. La mostramos como una ventana modal transparente
                    // 'false' es para quitar la animación de "deslizar hacia arriba"
                    await Shell.Current.Navigation.PushModalAsync(customAlert, false);
                }
            });
        }
    }
}
