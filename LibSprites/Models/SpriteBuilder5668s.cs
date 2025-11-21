using PKHeX.Core;
using PkHexA.LibSprites.Util;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace PkHexA.LibSprites.Models
{

    /// <summary>
    /// 56 high, 68 wide sprite builder
    /// </summary>
    public sealed class SpriteBuilder5668s : SpriteBuilder
    {

        public override int Height => 56;
        public override int Width => 68;

        protected override int ItemShiftX => 2;
        protected override int ItemShiftY => 2;
        protected override int ItemMaxSize => 32;
        protected override int EggItemShiftX => 18;
        protected override int EggItemShiftY => 1;
        public override bool HasFallbackMethod => true;

        protected override string GetSpriteStringSpeciesOnly(ushort species) => 'b' + $"_{species}";
        protected override string GetSpriteAll(ushort species, byte form, byte gender, uint formarg, bool shiny, EntityContext context) => 'b' + SpriteName.GetResourceStringSprite(species, form, gender, formarg, context, shiny);
        protected override string GetSpriteAllSecondary(ushort species, byte form, byte gender, uint formarg, bool shiny, EntityContext context) => 'c' + SpriteName.GetResourceStringSprite(species, form, gender, formarg, context, shiny);
        protected override string GetItemResourceName(int item) => 'b' + $"item_{item}";

        protected override SKBitmap Unknown
       => SpriteImgLoader.LoadBitmapFromResource("b_unknown")!;

        protected override SKBitmap GetEggSprite(ushort species)
            => species == (int)Species.Manaphy
                ? SpriteImgLoader.LoadBitmapFromResource("b_490_e")!
                : SpriteImgLoader.LoadBitmapFromResource("b_egg")!;

        // --- Buttons & overlays ---
        public override SKBitmap Hover
            => SpriteImgLoader.LoadBitmapFromResource("slotHover68")!;

        public override SKBitmap View
            => SpriteImgLoader.LoadBitmapFromResource("slotView68")!;

        public override SKBitmap Set
            => SpriteImgLoader.LoadBitmapFromResource("slotSet68")!;

        public override SKBitmap Delete
            => SpriteImgLoader.LoadBitmapFromResource("slotDel68")!;

        public override SKBitmap Transparent
            => SpriteImgLoader.LoadBitmapFromResource("slotTrans68")!;

        public override SKBitmap Drag
            => SpriteImgLoader.LoadBitmapFromResource("slotDrag68")!;

        // --- Items ---
        public override SKBitmap UnknownItem
            => SpriteImgLoader.LoadBitmapFromResource("bitem_unk")!;

        public override SKBitmap None
            => SpriteImgLoader.LoadBitmapFromResource("b_0")!;

        public override SKBitmap ItemTM
            => SpriteImgLoader.LoadBitmapFromResource("bitem_tm")!;

        public override SKBitmap ItemTR
            => SpriteImgLoader.LoadBitmapFromResource("bitem_tr")!;

        // --- Special sprite ---
        public override SKBitmap ShadowLugia
            => SpriteImgLoader.LoadBitmapFromResource("b_249x")!;


    }
}
