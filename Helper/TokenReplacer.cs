using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace PkHexA.Helper
{
    /// <summary>
    /// Permite registrar tokens en formato {TOKEN} y reemplazarlos dentro de un texto.
    /// </summary>
    public class TokenReplacer
    {
        private readonly Dictionary<string, string> _tokens = new(StringComparer.OrdinalIgnoreCase);
        private static readonly Regex TokenRegex = new("\\{(?<key>[^}]+)\\}", RegexOptions.Compiled);

        /// <summary>
        /// Agrega o actualiza el valor de un token.
        /// </summary>
        /// <param name="key">Nombre del token sin llaves.</param>
        /// <param name="value">Valor a inyectar cuando se encuentre el token.</param>
        public void AddOrUpdateToken(string key, string value)
        {
            _tokens[key] = value;
        }

        /// <summary>
        /// Reemplaza todos los tokens registrados dentro del texto recibido.
        /// </summary>
        /// <param name="text">Cadena que contiene tokens en formato {TOKEN}.</param>
        /// <returns>Texto con todos los tokens sustituidos por sus valores actuales.</returns>
        public string ReplaceTokens(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            return TokenRegex.Replace(text, match =>
            {
                var key = match.Groups["key"].Value;
                return _tokens.TryGetValue(key, out var value) ? value : match.Value;
            });
        }
    }
}
