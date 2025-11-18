using PKHeX.Core;
using PKHeX.Drawing;
using PkHexA.LibSprites.Enums;
using PkHexA.LibSprites.Interface;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace PkHexA.LibSprites.Util
{
    public abstract class SpriteBuilder : ISpriteBuilder<SKBitmap>
    {
        public static bool ShowEggSpriteAsItem { get; set; } = true;
        public static bool ShowEncounterBall { get; set; } = true;
        public static SpriteBackgroundType ShowEncounterColor { get; set; } = SpriteBackgroundType.FullBackground;
        public static SpriteBackgroundType ShowEncounterColorPKM { get; set; }
        public static SpriteBackgroundType ShowTeraType { get; set; } = SpriteBackgroundType.TopStripe;
        public static bool ShowExperiencePercent { get; set; }
        public static byte ShowTeraOpacityStripe { get; set; }
        public static int ShowTeraThicknessStripe { get; set; }
        public static byte ShowTeraOpacityBackground { get; set; }
        public static byte ShowEncounterOpacityStripe { get; set; }
        public static byte ShowEncounterOpacityBackground { get; set; }
        public static int ShowEncounterThicknessStripe { get; set; }

        /// <summary> Width of the generated Sprite image. </summary>
        public abstract int Width { get; }
        /// <summary> Height of the generated Sprite image. </summary>
        public abstract int Height { get; }

        /// <summary> Minimum amount of padding on the right side of the image when layering an item sprite. </summary>
        protected abstract int ItemShiftX { get; }
        /// <summary> Minimum amount of padding on the bottom side of the image when layering an item sprite. </summary>
        protected abstract int ItemShiftY { get; }
        /// <summary> Max width / height of an item image. </summary>
        protected abstract int ItemMaxSize { get; }

        protected abstract int EggItemShiftX { get; }
        protected abstract int EggItemShiftY { get; }

        public abstract bool HasFallbackMethod { get; }
        public abstract SKBitmap Hover { get; }
        public abstract SKBitmap View { get; }
        public abstract SKBitmap Set { get; }
        public abstract SKBitmap Delete { get; }
        public abstract SKBitmap Transparent { get; }
        public abstract SKBitmap Drag { get; }
        public abstract SKBitmap UnknownItem { get; }
        public abstract SKBitmap None { get; }
        public abstract SKBitmap ItemTM { get; }
        public abstract SKBitmap ItemTR { get; }
        private const double UnknownFormTransparency = 0.5;
        private const double ShinyTransparency = 0.7;
        private const double EggUnderLayerTransparency = 0.33;

        protected abstract string GetSpriteStringSpeciesOnly(ushort species);

        protected abstract string GetSpriteAll(ushort species, byte form, byte gender, uint formarg, bool shiny, EntityContext context);
        protected abstract string GetSpriteAllSecondary(ushort species, byte form, byte gender, uint formarg, bool shiny, EntityContext context);
        protected abstract string GetItemResourceName(int item);

        protected abstract SKBitmap Unknown { get; }
        protected abstract SKBitmap GetEggSprite(ushort species);
        public abstract SKBitmap ShadowLugia { get; }
        private GameVersion Version;

        /// <summary>
        /// Ensures all data is set up to generate sprites for the save file.
        /// </summary>
        public void Initialize(SaveFile sav)
        {
            if (sav.Generation != 3)
                return;

            // If the game is indeterminate, we might have different form sprites.
            // Currently, this only applies to Gen3's FireRed / LeafGreen
            Version = sav.Version;
            if (Version == GameVersion.FRLG)
                Version = ReferenceEquals(sav.Personal, PersonalTable.FR) ? GameVersion.FR : GameVersion.LG;
        }
        private static byte GetDeoxysForm(GameVersion version) => version switch
        {
            GameVersion.FR => 1, // Attack
            GameVersion.LG => 2, // Defense
            GameVersion.E => 3, // Speed
            _ => 0,
        };

        private static byte GetArceusForm4(byte form) => form switch
        {
            > 9 => --form, // Realign to Gen5+ type indexes
            9 => byte.MaxValue, // Curse, make it show as unrecognized form since we don't have a sprite.
            _ => form,
        };

        /// <summary>
        /// Builds a new sprite image with the requested parameters.
        /// </summary>
        /// <param name="species">Entity Species ID</param>
        /// <param name="form">Entity Form index</param>
        /// <param name="gender">Entity gender</param>
        /// <param name="formarg">Entity <see cref="IFormArgument.FormArgument"/> raw value</param>
        /// <param name="heldItem">Entity held item ID</param>
        /// <param name="isEgg">Is currently in an egg</param>
        /// <param name="shiny">Is it shiny</param>
        /// <param name="context">Context the sprite is for</param>
        public SKBitmap GetSprite(ushort species,byte form,byte gender,uint formarg,int heldItem,bool isEgg,Shiny shiny = Shiny.Never,EntityContext context = EntityContext.None)
        {
            if (species == 0)
                return None;

            // Gen 3 Deoxys: depende de la versión
            if (context == EntityContext.Gen3 && species == (int)Species.Deoxys)
                form = GetDeoxysForm(Version);

            // Gen 4 Arceus: depende del "curse type"
            else if (context == EntityContext.Gen4 && species == (int)Species.Arceus)
                form = GetArceusForm4(form);

            // Igual que tu código original, pero ahora GetBaseImage debe devolver SKBitmap
            var baseImage = GetBaseImage(
                species,
                form,
                gender,
                formarg,
                shiny.IsShiny(),
                context
            );

            // Igual que antes, pero llamando al GetSprite(SKBitmap…)
            return GetSprite(
                baseImage,
                species,
                heldItem,
                isEgg,
                shiny,
                context
            );
        }


        public SKBitmap GetSprite(SKBitmap baseSprite,ushort species,int heldItem,bool isEgg,Shiny shiny,EntityContext context = EntityContext.None)
        {
            if (isEgg)
                baseSprite = LayerOverImageEgg(baseSprite, species, heldItem != 0);

            if (heldItem > 0)
                baseSprite = LayerOverImageItem(baseSprite, heldItem, context);

            if (shiny.IsShiny())
            {
                if (shiny == Shiny.AlwaysSquare && context.Generation() != 8)
                    shiny = Shiny.Always;

                baseSprite = LayerOverImageShiny(baseSprite, shiny);
            }

            return baseSprite;
        }


        private SKBitmap GetBaseImage(ushort species,byte form,byte gender,uint formarg,bool shiny,EntityContext context)
        {
            var img = FormInfo.IsTotemForm(species, form, context)
                ? GetBaseImageTotem(species, form, gender, formarg, shiny, context)
                : GetBaseImageDefault(species, form, gender, formarg, shiny, context);

            return img ?? GetBaseImageFallback(species, form, gender, formarg, shiny, context);
        }

        private SKBitmap? GetBaseImageTotem(ushort species,byte form,byte gender,uint formarg,bool shiny,EntityContext context)
        {
            var baseform = FormInfo.GetTotemBaseForm(species, form);

            // GetBaseImageDefault ya convertido a SKBitmap?
            var b = GetBaseImageDefault(species, baseform, gender, formarg, shiny, context);
            if (b is null)
                return null;

            // Glow sobre SKBitmap
            SpriteUtil.GetSpriteGlow(
                b,
                0,
                165,
                255,
                out var pixels,
                true
            );

            // Crear una SKBitmap desde los bytes
            var layer = ImageUtil.GetBitmap(
                pixels,
                b.Width,
                b.Height
            );

            // Capa encima
            return ImageUtil.LayerImage(
                b,
                layer,
                0,
                0
            );
        }


        private SKBitmap? GetBaseImageDefault(ushort species,byte form,
            byte gender,uint formarg,bool shiny,EntityContext context)
        {
            // Archivo principal
            var file = GetSpriteAll(species, form, gender, formarg, shiny, context);

            // Intentar cargar sprite desde MAUI (pokehex/Sprites/ o pokehex/img/)
            var resource = SpriteImgLoader.LoadSprite(file);

            // Si no existe y hay fallback, probar con el secundario
            if (resource is null && HasFallbackMethod)
            {
                file = GetSpriteAllSecondary(species, form, gender, formarg, shiny, context);
                resource = SpriteImgLoader.LoadSprite(file);
            }

            return resource;
        }

        private SKBitmap GetBaseImageFallback(
            ushort species,
            byte form,
            byte gender,
            uint formarg,
            bool shiny,
            EntityContext context)
        {
            // Si está shiny, intentamos sin shiny primero
            if (shiny)
            {
                var img = GetBaseImageDefault(
                    species,
                    form,
                    gender,
                    formarg,
                    false,
                    context);

                if (img is not null)
                    return img;
            }

            // Intentar sin form
            var file = GetSpriteStringSpeciesOnly(species);

            // Cargar sprite real desde las carpetas MAUI
            SKBitmap? baseImage = SpriteImgLoader.LoadSprite(file);

            if (baseImage is null)
                return Unknown; // fallback final

            // Poner encima el "Unknown" con transparencia
            return ImageUtil.LayerImage(
                baseImage,
                Unknown,
                0,
                0,
                UnknownFormTransparency
            );
        }

        private SKBitmap LayerOverImageItem(SKBitmap baseImage, int item, EntityContext context)
        {
            // Identificar si es TM o TR
            var lump = HeldItemLumpUtil.GetIsLump(item, context);

            SKBitmap? itemImg = lump switch
            {
                HeldItemLumpImage.TechnicalMachine => ItemTM,   // ya SKBitmap en tu builder
                HeldItemLumpImage.TechnicalRecord => ItemTR,    // ya SKBitmap
                _ => SpriteImgLoader.LoadSprite(GetItemResourceName(item))
                     ?? UnknownItem,                            // fallback SKBitmap
            };

            if (itemImg == null)
                return baseImage;

            // reposición igual que antes
            int x = baseImage.Width - itemImg.Width
                    - ((ItemMaxSize - itemImg.Width) / 4)
                    - ItemShiftX;

            int y = baseImage.Height - itemImg.Height
                    - ItemShiftY;

            return ImageUtil.LayerImage(baseImage, itemImg, x, y);
        }

        private static SKBitmap LayerOverImageShiny(SKBitmap baseImage, Shiny shiny)
        {
            // Cargar shiny star correcta
            SKBitmap? rare = shiny is Shiny.AlwaysSquare
                ? SpriteImgLoader.LoadSprite("rare_icon_alt_2")
                : SpriteImgLoader.LoadSprite("rare_icon_alt");

            if (rare == null)
                return baseImage; // fallback silencioso

            return ImageUtil.LayerImage(baseImage, rare, 0, 0, ShinyTransparency);
        }


        private SKBitmap LayerOverImageEgg(SKBitmap baseImage, ushort species, bool hasItem)
        {
            if (ShowEggSpriteAsItem && !hasItem)
                return LayerOverImageEggAsItem(baseImage, species);

            return LayerOverImageEggTransparentSpecies(baseImage, species);
        }


        private SKBitmap LayerOverImageEggTransparentSpecies(SKBitmap baseImage, ushort species)
        {
            // Hacer parcialmente transparente el sprite
            baseImage = ImageUtil.ChangeOpacity(baseImage, EggUnderLayerTransparency);

            // Obtener sprite del huevo (YA debe devolver SKBitmap)
            var egg = GetEggSprite(species);

            // Colocar el huevo encima, full opacity
            return ImageUtil.LayerImage(baseImage, egg, 0, 0);
        }


        private SKBitmap LayerOverImageEggAsItem(SKBitmap baseImage, ushort species)
        {
            // Obtener el sprite del huevo como SKBitmap
            var egg = GetEggSprite(species); // ← ya debe devolver SKBitmap en tu clase

            // Colocar el huevo como si fuera un item (mismos offsets que el original)
            return ImageUtil.LayerImage(
                baseImage,
                egg,
                EggItemShiftX,
                EggItemShiftY
            );
        }

        public static void LoadSettings(ISpriteSettings sprite)
        {
            ShowEggSpriteAsItem = sprite.ShowEggSpriteAsHeldItem;
            ShowEncounterBall = sprite.ShowEncounterBall;

            ShowEncounterColor = sprite.ShowEncounterColor;
            ShowEncounterColorPKM = sprite.ShowEncounterColorPKM;
            ShowEncounterThicknessStripe = sprite.ShowEncounterThicknessStripe;
            ShowEncounterOpacityBackground = sprite.ShowEncounterOpacityBackground;
            ShowEncounterOpacityStripe = sprite.ShowEncounterOpacityStripe;
            ShowExperiencePercent = sprite.ShowExperiencePercent;

            ShowTeraType = sprite.ShowTeraType;
            ShowTeraThicknessStripe = sprite.ShowTeraThicknessStripe;
            ShowTeraOpacityBackground = sprite.ShowTeraOpacityBackground;
            ShowTeraOpacityStripe = sprite.ShowTeraOpacityStripe;
        }


        /////////////////////////////////////////////////
    }
}
