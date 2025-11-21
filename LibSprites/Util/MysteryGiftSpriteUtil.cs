using PKHeX.Core;
using PKHeX.Drawing;
using PkHexA.LibSprites.Enums;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace PkHexA.LibSprites.Util
{
    /// <summary>
    /// Provides utility methods for retrieving and composing sprites for Mystery Gifts.
    /// </summary>
    public static class MysteryGiftSpriteUtil
    {
        public static SKBitmap Sprite(this MysteryGift gift) => GetSprite(gift);
        private static SKBitmap GetSprite(MysteryGift gift)
        {
            if (gift.IsEmpty)
                return SpriteUtil.Spriter.None;   // Ya es SKBitmap en tu nueva versión

            // 1. Obtener sprite base en SKBitmap
            var img = GetBaseImage(gift); // Debes tener este método convertido a SKBitmap también

            // 2. Aplicar color de encuentro (si aplica)
            if (SpriteBuilder.ShowEncounterColor != SpriteBackgroundType.None)
                img = SpriteUtil.ApplyEncounterColor(gift, img, SpriteBuilder.ShowEncounterColor);
            // Este método ya lo convertimos a SKBitmap en mensajes previos

            // 3. Marcar como usado con opacidad
            if (gift.GiftUsed)
                img = ImageUtil.ChangeOpacity(img, 0.3f);
            // Ya tienes ImageUtil con versión SKBitmap

            return img;
        }

        private static SKBitmap GetBaseImage(MysteryGift gift)
        {
            // 🥚 Manaphy Egg
            if (gift is { IsEgg: true, Species: (int)Species.Manaphy })
                return SpriteUtil.GetMysteryGiftPreviewPoke(gift);

            // Pokémon normal
            if (gift.IsEntity)
                return SpriteUtil.GetMysteryGiftPreviewPoke(gift);

            // 🎁 Item
            if (gift.IsItem)
            {
                ushort item = (ushort)gift.ItemID;

                if (ItemStorage7USUM.GetCrystalHeld(item, out var value))
                    item = value;

                return SpriteUtil.GetItemSprite(item)
                       ?? SpriteImgLoader.LoadSprite("Bag_Key")     // reemplazo de Resources.Bag_Key
                       ?? SpriteImgLoader.LoadSprite("b_unknown")   // fallback extra
                       ?? SpriteUtil.Spriter.None;                  // fallback final
            }

            // ❓ Imagen desconocida
            return SpriteImgLoader.LoadSprite("b_unknown")
                   ?? SpriteUtil.Spriter.None;
        }


        ///////////////////////////////////////////////
    }
}
