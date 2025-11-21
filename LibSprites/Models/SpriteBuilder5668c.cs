using PKHeX.Core;
using PkHexA.LibSprites.Util;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace PkHexA.LibSprites.Models
{

    /// <summary>
    /// 56 high, 68 wide sprite builder using Circular Sprites
    /// </summary>
    public sealed class SpriteBuilder5668c : SpriteBuilder
    {
        public override int Height => 56;
        public override int Width => 68;

        protected override int ItemShiftX => 2;
        protected override int ItemShiftY => 2;
        protected override int ItemMaxSize => 32;
        protected override int EggItemShiftX => 18;
        protected override int EggItemShiftY => 1;
        public override bool HasFallbackMethod => true;

        protected override string GetSpriteStringSpeciesOnly(ushort species) => 'c' + $"_{species}";
        protected override string GetSpriteAll(ushort species, byte form, byte gender, uint formarg, bool shiny, EntityContext context) => 'c' + SpriteName.GetResourceStringSprite(species, form, gender, formarg, context, shiny);
        protected override string GetSpriteAllSecondary(ushort species, byte form, byte gender, uint formarg, bool shiny, EntityContext context) => 'b' + SpriteName.GetResourceStringSprite(species, form, gender, formarg, context, shiny);
        protected override string GetItemResourceName(int item) => 'b' + $"item_{item}";

        protected override SKBitmap Unknown
       => SpriteImgLoader.LoadBitmapFromResource("b_unknown")
          ?? new SKBitmap(68, 68); // tamaño del sprite (68x68)

        protected override SKBitmap GetEggSprite(ushort species)
        {
            var name = species == (int)Species.Manaphy
                ? "b_490_e"
                : "b_egg";

            return SpriteImgLoader.LoadBitmapFromResource(name)
                ?? SpriteImgLoader.CreateFallback();
        }


        public override SKBitmap Hover =>
          SpriteImgLoader.LoadBitmapFromResource("slotHover68")
          ?? SpriteImgLoader.CreateFallback();

        public override SKBitmap View =>
         SpriteImgLoader.LoadBitmapFromResource("slotView68")
         ?? SpriteImgLoader.CreateFallback();

        public override SKBitmap Set =>
            SpriteImgLoader.LoadBitmapFromResource("slotSet68")
            ?? SpriteImgLoader.LoadBitmapFromResource("b_unknown")!;

        public override SKBitmap Delete
          => SpriteImgLoader.LoadBitmapFromResource("slotDel68")!;


        public override SKBitmap Transparent
            => SpriteImgLoader.LoadBitmapFromResource("slotTrans68")!;

        public override SKBitmap Drag
            => SpriteImgLoader.LoadBitmapFromResource("slotDrag68")!;

        public override SKBitmap UnknownItem
            => SpriteImgLoader.LoadBitmapFromResource("bitem_unk")!;

        public override SKBitmap None
            => SpriteImgLoader.LoadBitmapFromResource("b_0")!;

        public override SKBitmap ItemTM =>
          SpriteImgLoader.LoadBitmapFromResource("bitem_tm")
          ?? SpriteImgLoader.LoadBitmapFromResource("b_unknown")!;


        public override SKBitmap ItemTR =>
            SpriteImgLoader.LoadBitmapFromResource("bitem_tr")
            ?? SpriteImgLoader.LoadBitmapFromResource("b_unknown")!;

        public override SKBitmap ShadowLugia =>
       SpriteImgLoader.LoadBitmapFromResource("b_249x")
       ?? SpriteImgLoader.LoadBitmapFromResource("b_unknown")!;
    }
}
