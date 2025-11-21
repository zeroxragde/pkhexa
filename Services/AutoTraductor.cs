using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace PkHexA.Services
{
    public static class AutoTraductor
    {
        /// <summary>
        /// Busca todos los controles con x:Name en la página y les asigna su texto
        /// desde el LanguageService automáticamente.
        /// </summary>
        public static void Traducir(ContentPage pagina)
        {
            // 1. Obtenemos todos los campos privados de la página.
            // (Cuando pones x:Name en XAML, MAUI crea un campo privado con ese nombre)
            var campos = pagina.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            foreach (var campo in campos)
            {
                // El nombre de la variable (ej: "lblEspecie", "txtSearchBoxPlaceholde")
                string nombreCampo = campo.Name;

                // 2. Pedimos la traducción
                string traduccion = LanguageService.Get(nombreCampo);

                // 3. VALIDACIÓN DE SEGURIDAD:
                // Tu LanguageService devuelve la 'key' si no encuentra traducción.
                // Si la traducción es igual al nombre de la variable, significa que NO existe traducción.
                // En ese caso, no hacemos nada para no borrar lo que ya tenías escrito.
                if (string.IsNullOrEmpty(traduccion) || traduccion == nombreCampo)
                    continue;

                // 4. Obtenemos el control real (el Label, Entry, etc.)
                var control = campo.GetValue(pagina);

                // Si el control es nulo, pasamos al siguiente
                if (control == null) continue;

                // 5. ASIGNACIÓN INTELIGENTE SEGÚN EL TIPO
                if (control is Label lbl)
                {
                    lbl.Text = traduccion;
                }
                else if (control is Button btn)
                {
                    btn.Text = traduccion;
                }
                else if (control is Entry entry)
                {
                    // A los Entry les cambiamos el Placeholder (lo que sale en gris)
                    entry.Placeholder = traduccion;
                }
                else if (control is SearchBar searchBar)
                {
                    // A las barras de búsqueda también el Placeholder
                    searchBar.Placeholder = traduccion;
                }
                else if (control is Picker picker)
                {
                    picker.Title = traduccion;
                }
                else if (control is ContentPage page)
                {
                    // Esto sirve para el título de la ventana (ej: x:Name="winPkmEditor")
                    page.Title = traduccion;
                }
                // Puedes agregar más tipos aquí (ej: CheckBox, RadioButton) si los necesitas
            }
        }

        /// <summary>
        /// Método especial para traducir AppShell y sus elementos (FlyoutItem, Tab, etc.)
        /// </summary>
        public static void TraducirShell(Shell shell)
        {
            // 1. Traducir el Título del propio Shell (la barra de arriba)
            // Si el Shell tiene x:Name (ej: x:Name="shellPrincipal")
            if (!string.IsNullOrEmpty(shell.StyleId))
            {
                string tituloShell = LanguageService.Get(shell.StyleId);
                if (!string.IsNullOrEmpty(tituloShell))
                    shell.Title = tituloShell;
            }

            // 2. Buscar elementos internos con x:Name
            var campos = shell.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            foreach (var campo in campos)
            {
                string nombreKey = campo.Name;
                string traduccion = LanguageService.Get(nombreKey);

                if (string.IsNullOrEmpty(traduccion) || traduccion == nombreKey)
                    continue;

                var control = campo.GetValue(shell);

                if (control == null) continue;

                // 3. APLICAR TRADUCCIÓN A ELEMENTOS DE SHELL

                // BaseShellItem cubre: FlyoutItem, Tab, ShellContent
                if (control is BaseShellItem item)
                {
                    item.Title = traduccion;
                }
                // MenuItem son los botones extra que a veces se ponen en el menú
                else if (control is MenuItem menuItem)
                {
                    menuItem.Text = traduccion;
                }
            }
        }






        //////////////////
    }
}
