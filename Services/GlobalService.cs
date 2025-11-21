using PKHeX.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace PkHexA.Services
{
    public static class GlobalService
    {
        public static SaveFile ACTUAL_FILE;
        public static Task ShowAlertAsync(string? message, string? cancel = "OK")
        {
            string title = "PkHexA";
            message ??= string.Empty;
            cancel ??= "OK";

            return MainThread.InvokeOnMainThreadAsync(
                async () => await (Shell.Current?.DisplayAlertAsync(title, message, cancel) ?? Task.CompletedTask)
            );
        }
    }
}
