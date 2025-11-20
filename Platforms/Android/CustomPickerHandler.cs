

using Android.Content.Res;
using Android.Graphics.Drawables;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;

namespace PkHexA.Platforms.Android
{
    public class CustomPickerHandler : PickerHandler
    {





        protected override void ConnectHandler(MauiPicker platformView)
        {
            base.ConnectHandler(platformView);

            // 1. Pinta la línea inferior y el "tinte" del control de negro
            platformView.BackgroundTintList = ColorStateList.ValueOf(global::Android.Graphics.Color.Black);

            // 2. Asegura que el texto sea blanco (aunque esto también se hace en XAML)
            platformView.SetTextColor(global::Android.Graphics.Color.Aqua);
            platformView.SetPadding(40, 0, 40, 0);
            // BORRA LA LÍNEA DE 'PopupBackgroundDrawable', NO EXISTE EN MAUIPICKER
        }



    }
}
