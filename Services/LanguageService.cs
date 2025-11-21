using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace PkHexA.Services
{
    public static class LanguageService
    {
        private static Dictionary<string, Dictionary<string, string>> _languages = new();
        private static string _currentLanguage = "es";

        public static string CurrentLanguage => _currentLanguage;

        public static async Task InitializeAsync()
        {
            try
            {
                Console.WriteLine("🔍 Intentando cargar archivo lang.json...");

                Stream? stream = null;

                // 🔹 Intenta las rutas más comunes
                string[] rutas = new[]
                {
                    "lang.json",
                    "Resources/lang.json",
                    "Resources/Languages/lang.json"
                };

                foreach (var ruta in rutas)
                {
                    try
                    {
                        stream = await FileSystem.OpenAppPackageFileAsync(ruta);
                        Console.WriteLine($"✅ Archivo encontrado: {ruta}");
                        break;
                    }
                    catch
                    {
                        // probar siguiente
                    }
                }

                if (stream == null)
                {
                    Console.WriteLine("❌ No se encontró lang.json en ninguna ruta válida.");
                    return;
                }

                using var reader = new StreamReader(stream);
                var json = await reader.ReadToEndAsync();

                _languages = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(json)!;
                Console.WriteLine($"✅ Idiomas cargados: {_languages.Keys.Count}");

                var savedLang = Preferences.Get("lang", "es");

                // 🔹 Detecta idioma del sistema si no hay guardado
                if (string.IsNullOrEmpty(savedLang))
                    savedLang = System.Globalization.CultureInfo.CurrentCulture.TwoLetterISOLanguageName;

                if (_languages.ContainsKey(savedLang))
                {
                    _currentLanguage = savedLang;
                    Console.WriteLine($"🌐 Idioma actual: {_currentLanguage}");
                }
                else
                {
                    _currentLanguage = "es";
                    Preferences.Set("lang", "es");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error cargando idiomas: {ex.Message}");
            }
        }


        public static void SetLanguage(string langCode)
        {
            if (_languages.ContainsKey(langCode))
            {
                _currentLanguage = langCode;
                Preferences.Set("lang", langCode); // 🔹 Guardar selección persistente
            }
        }

        public static string Get(string key)
        {
            if (_languages.TryGetValue(_currentLanguage, out var dict))
                if (dict.TryGetValue(key, out var value))
                    return value;

            return key; // Si no existe, devuelve la clave misma
        }
   
    }

}
