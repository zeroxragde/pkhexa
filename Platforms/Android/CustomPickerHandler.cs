using Android.Graphics.Drawables;
using Microsoft.Maui.Handlers;

namespace PkHexA.Platforms.Android
{
    public class CustomPickerHandler : PickerHandler
    {
        protected override void ConnectHandler(global::Android.Widget.EditText platformView)
        {
            base.ConnectHandler(platformView);

            // Fondo oscuro tipo PKHeX
            platformView.SetBackgroundColor(Color.ParseColor("#202020"));

            // Texto blanco
            platformView.SetTextColor(Color.White);

            // Quitar borde feo de Android
            platformView.BackgroundTintList =
                Android.Content.Res.ColorStateList.ValueOf(Color.Transparent);

            // Popup (Spinner)
            if (platformView.Background is GradientDrawable gd)
                gd.SetColor(Color.ParseColor("#202020"));
        }
    }
}
