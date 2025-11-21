using PKHeX.Core;
using PKHeX.Drawing;
using PkHexA.LibSprites.Enums;
using PkHexA.LibSprites.Models;
using SkiaSharp;
using System.Buffers;


namespace PkHexA.LibSprites.Util
{
    /// <summary>
    /// Singleton that builds sprite images.
    /// </summary>
    public static class SpriteUtil
    {
        /// <summary>Square sprite builder instance</summary>
        public static readonly SpriteBuilder5668s SB8s = new();
        /// <summary>Circle sprite builder instance (used in Legends: Arceus)</summary>
        public static readonly SpriteBuilder5668c SB8c = new();
        /// <summary>Circle sprite builder instance (used in Brilliant Diamond, Shining Pearl, Scarlet, and Violet)</summary>
        public static readonly SpriteBuilder5668a SB8a = new();

        /// <summary>Current sprite builder reference used to build sprites.</summary>
        public static SpriteBuilder Spriter { get; private set; } = SB8s;

        /// <summary>
        /// Changes the builder mode to the requested mode.
        /// </summary>
        /// <param name="mode">Requested sprite builder mode</param>
        /// <remarks>If an out-of-bounds value is provided, will not change.</remarks>
        public static void ChangeMode(SpriteBuilderMode mode) => Spriter = mode switch
        {
            SpriteBuilderMode.SpritesArtwork5668 => SB8a,
            SpriteBuilderMode.CircleMugshot5668 => SB8c,
            SpriteBuilderMode.SpritesClassic5668 => SB8s,
            _ => Spriter,
        };
        private const int MaxSlotCount = 30; // slots in a box
        private static int SpriteWidth => Spriter.Width;
        private static int SpriteHeight => Spriter.Height;
        private static int PartyMarkShiftX => SpriteWidth - 16;
        private static int SlotLockShiftX => SpriteWidth - 14;
        private static int SlotTeamShiftX => SpriteWidth - 19;
        private static int FlagIllegalShiftY => SpriteHeight - 16;


        /// <summary>
        /// Sets up the sprite builder to behave with the input <see cref="sav"/>.
        /// </summary>
        /// <param name="sav">Save File to be generating sprites for.</param>
        public static void Initialize(SaveFile sav)
        {
            ChangeMode(SpriteBuilderUtil.GetSuggestedMode(sav));
            Spriter.Initialize(sav);
        }
        public static SKBitmap GetBallSprite(byte ball)
        {
            string resource = SpriteName.GetResourceStringBall(ball);

            // Intentar cargar el sprite del ball
            var bmp = SpriteImgLoader.LoadSprite(resource);
            if (bmp != null)
                return bmp;

            // Default: Poké Ball
            return SpriteImgLoader.LoadSprite("_ball4")!;
        }
        public static SKBitmap? GetItemSprite(int item)
        {
            string name = $"item_{item}";
            return SpriteImgLoader.LoadSprite(name);
        }
        public static SKBitmap GetSprite(ushort species, byte form, byte gender, uint formarg, int item, bool isegg, Shiny shiny, EntityContext context = EntityContext.None)
        {
            return Spriter.GetSprite(species, form, gender, formarg, item, isegg, shiny, context);
        }
        private static SKBitmap GetSprite(PKM pk)
        {
            var formarg = pk is IFormArgument f ? f.FormArgument : 0;
            var shiny = ShinyExtensions.GetType(pk);

            var img = GetSprite(pk.Species, pk.Form, pk.Gender, formarg,
                                pk.SpriteItem, pk.IsEgg, shiny, pk.Context);

            // --- Shadow Lugia ---
            if (pk is IShadowCapture { IsShadow: true })
            {
                const ushort Lugia = (int)Species.Lugia;

                if (pk.Species is Lugia)
                {
                    // --> ANTES: Spriter.ShadowLugia (Resources.b_249x)
                    var shadow = SpriteImgLoader.LoadSprite("b_249x");
                    img = Spriter.GetSprite(shadow!, Lugia,
                                            pk.SpriteItem, pk.IsEgg, shiny, pk.Context);
                }

                GetSpriteGlow(pk, 75, 0, 130, out var pixels, out var baseSprite, true);

                var glowImg = ImageUtil.GetBitmap(pixels, baseSprite.Width, baseSprite.Height);
                return ImageUtil.LayerImage(glowImg, img, 0, 0);
            }

            // --- Gigantamax ---
            if (pk is IGigantamaxReadOnly { CanGigantamax: true })
            {
                // --> ANTES: Resources.dyna
                var gm = SpriteImgLoader.LoadSprite("dyna");

                return ImageUtil.LayerImage(
                    img,
                    gm!,
                    (img.Width - gm.Width) / 2,
                    0
                );
            }

            // --- Alpha ---
            if (pk is IAlphaReadOnly { IsAlpha: true })
            {
                // --> ANTES: Resources.alpha_alt
                var alpha = SpriteImgLoader.LoadSprite("alpha_alt");

                return ImageUtil.LayerImage(
                    img,
                    alpha!,
                    SlotTeamShiftX,
                    0
                );
            }

            return img;
        }

        private static SKBitmap GetSprite(PKM pk,SaveFile sav,int box,int slot,bool flagIllegal = false,StorageSlotType storage = StorageSlotType.None)
        {
            bool inBox = (uint)slot < MaxSlotCount;
            bool empty = pk.Species == 0;

            // ⚠️ pk.Sprite() ya devuelve SKBitmap en tu nuevo sistema
            var sprite = empty ? Spriter.None : pk.Sprite();

            if (!empty)
            {
                // --- TERA TYPE ---
                if (SpriteBuilder.ShowTeraType != SpriteBackgroundType.None && pk is ITeraType t)
                {
                    var type = t.TeraType;
                    if (TeraTypeUtil.IsOverrideValid((byte)type))
                        sprite = ApplyTeraColor((byte)type, sprite, SpriteBuilder.ShowTeraType);
                }

                // --- LEGALIDAD ---
                if (flagIllegal)
                {
                    var la = pk.GetType() == sav.PKMType
                        ? new LegalityAnalysis(pk, sav.Personal, storage)
                        : new LegalityAnalysis(pk, pk.PersonalInfo, storage);

                    if (!la.Valid)
                        sprite = ImageUtil.LayerImage(sprite, SpriteImgLoader.LoadSprite("warn")!, 0, FlagIllegalShiftY);

                    else if (pk.Format >= 8 && MoveInfo.IsDummiedMoveAny(pk))
                        sprite = ImageUtil.LayerImage(sprite, SpriteImgLoader.LoadSprite("hint")!, 0, FlagIllegalShiftY);

                    if (SpriteBuilder.ShowEncounterColorPKM != SpriteBackgroundType.None)
                        sprite = ApplyEncounterColor(la.EncounterOriginal, sprite, SpriteBuilder.ShowEncounterColorPKM);

                    if (SpriteBuilder.ShowExperiencePercent)
                        sprite = ApplyExperience(pk, sprite, la.EncounterMatch);
                }
            }

            // --- FLAGS DE CAJA ---
            if (inBox)
            {
                var flags = sav.GetBoxSlotFlags(box, slot);

                // Battle Team
                int team = flags.IsBattleTeam();
                if (team >= 0)
                    sprite = ImageUtil.LayerImage(
                        sprite,
                        SpriteImgLoader.LoadSprite("team")!,
                        SlotTeamShiftX,
                        0
                    );

                // Locked
                if (flags.HasFlag(StorageSlotSource.Locked))
                    sprite = ImageUtil.LayerImage(
                        sprite,
                        SpriteImgLoader.LoadSprite("locked")!,
                        SlotLockShiftX,
                        0
                    );

                // Party Mark
                int party = flags.IsParty();
                if (party >= 0)
                    sprite = ImageUtil.LayerImage(
                        sprite,
                        PartyMarks[party],   // ya convertidos a SKBitmap
                        PartyMarkShiftX,
                        0
                    );

                // Starter
                if (flags.HasFlag(StorageSlotSource.Starter))
                    sprite = ImageUtil.LayerImage(
                        sprite,
                        SpriteImgLoader.LoadSprite("starter")!,
                        0,
                        0
                    );
            }

            // --- EXPERIENCE BAR ---
            if (SpriteBuilder.ShowExperiencePercent && !flagIllegal)
                sprite = ApplyExperience(pk, sprite);

            return sprite;
        }



        public static void GetSpriteGlow(
            PKM pk,
            byte blue,
            byte green,
            byte red,
            out byte[] pixels,
            out SKBitmap baseSprite,
            bool forceHollow = false)
        {
            bool egg = pk.IsEgg;

            var formarg = pk is IFormArgument f ? f.FormArgument : 0;
            var shiny = pk.IsShiny ? Shiny.Always : Shiny.Never;

            // GetSprite YA DEVUELVE SKBitmap
            baseSprite = GetSprite(
                pk.Species,
                pk.Form,
                pk.Gender,
                formarg,
                0,
                egg,
                shiny,
                pk.Context
            );

            // Llama a la versión SKBitmap
            GetSpriteGlow(
                baseSprite,
                blue,
                green,
                red,
                out pixels,
                forceHollow || egg
            );
        }

        public static void GetSpriteGlow(SKBitmap baseSprite, byte blue, byte green, byte red, out byte[] pixels, bool forceHollow = false)
        {
            // baseSprite ya viene como SKBitmap
            pixels = ImageUtil.GetPixelData(baseSprite);

            if (!forceHollow)
            {
                ImageUtil.GlowEdges(pixels, blue, green, red, baseSprite.Width);
                return;
            }

            // --- Modo hollow (idéntico al original) ---

            var temp = ArrayPool<byte>.Shared.Rent(pixels.Length);
            var original = temp.AsSpan(0, pixels.Length);

            pixels.CopyTo(original);

            ImageUtil.SetAllUsedPixelsOpaque(pixels);
            ImageUtil.GlowEdges(pixels, blue, green, red, baseSprite.Width);
            ImageUtil.RemovePixels(pixels, original);

            original.Clear();
            ArrayPool<byte>.Shared.Return(temp);
        }


        private static SKBitmap ApplyTeraColor(byte elementalType, SKBitmap img, SpriteBackgroundType type)
        {
            var color = TypeColor.GetTeraSpriteColor(elementalType);
            var thk = SpriteBuilder.ShowTeraThicknessStripe;
            var op = SpriteBuilder.ShowTeraOpacityStripe;
            var bg = SpriteBuilder.ShowTeraOpacityBackground;

            return ApplyColor(img, type, color, thk, op, bg);
        }
        public static SKBitmap ApplyEncounterColor(IEncounterTemplate enc,SKBitmap img,SpriteBackgroundType type)
        {
            // Crear color desde entero ARGB
            var index = enc.GetType().Name.GetHashCode() * 0x43FD43FD;
            uint argb = unchecked((uint)index);

            // Skia usa: A, R, G, B por separado
            byte a = (byte)((argb >> 24) & 0xFF);
            byte r = (byte)((argb >> 16) & 0xFF);
            byte g = (byte)((argb >> 8) & 0xFF);
            byte b = (byte)(argb & 0xFF);

            var color = new SKColor(r, g, b, a);

            int thk = SpriteBuilder.ShowEncounterThicknessStripe;
            byte op = SpriteBuilder.ShowEncounterOpacityStripe;
            byte bg = SpriteBuilder.ShowEncounterOpacityBackground;

            return ApplyColor(img, type, color, thk, op, bg);
        }

        private static SKBitmap ApplyColor(SKBitmap img,SpriteBackgroundType type,SKColor color,int thick,byte opacStripe,byte opacBack)
        {
            if (type == SpriteBackgroundType.BottomStripe)
            {
                int stripeHeight = thick;
                if ((uint)stripeHeight > img.Height)
                    stripeHeight = img.Height;

                return ImageUtil.BlendTransparentTo(
                    img,
                    color,
                    opacStripe,
                    img.Width * 4 * (img.Height - stripeHeight));
            }

            if (type == SpriteBackgroundType.TopStripe)
            {
                int stripeHeight = thick;
                if ((uint)stripeHeight > img.Height)
                    stripeHeight = img.Height;

                return ImageUtil.BlendTransparentTo(
                    img,
                    color,
                    opacStripe,
                    0,
                    (img.Width * 4 * stripeHeight) - 4);
            }

            if (type == SpriteBackgroundType.FullBackground)
            {
                return ImageUtil.ChangeTransparentTo(img, color, opacBack);
            }

            return img;
        }
        private static SKBitmap ApplyExperience(PKM pk, SKBitmap img, IEncounterTemplate? enc = null)
        {
            const int bpp = 4;
            int start = bpp * SpriteWidth * (SpriteHeight - 1);
            var level = pk.CurrentLevel;

            if (level == Experience.MaxLevel)
                return ImageUtil.WritePixels(img, SKColors.Lime, start, start + (SpriteWidth * bpp));

            var pct = Experience.GetEXPToLevelUpPercentage(level, pk.EXP, pk.PersonalInfo.EXPGrowth);
            if (pct is not 0)
                return ImageUtil.WritePixels(img, SKColors.DodgerBlue, start, start + (int)(SpriteWidth * pct * bpp));

            var encLevel = enc is { IsEgg: true } ? enc.LevelMin : pk.MetLevel;
            var color = level != encLevel && pk.HasOriginalMetLocation
                ? SKColors.DarkOrange
                : SKColors.Yellow;

            return ImageUtil.WritePixels(img, color, start, start + (SpriteWidth * bpp));
        }
       
        
        private static readonly SKBitmap[] PartyMarks =
        {
        LoadParty("party1"),
        LoadParty("party2"),
        LoadParty("party3"),
        LoadParty("party4"),
        LoadParty("party5"),
        LoadParty("party6"),
        };

        // Método local para cargar cada PartyMark
        private static SKBitmap LoadParty(string name)
        {
            return SpriteImgLoader.LoadBitmapFromResource(name)
                   ?? new SKBitmap(1, 1); // fallback mínimo si no existe
        }


        public static SKBitmap GetLegalIndicator(bool valid)
        {
            // Cargar desde tu sistema de archivos
            return SpriteImgLoader.LoadBitmapFromResource(valid ? "valid" : "warn")
                   ?? new SKBitmap(); // evita null
        }

        // Extension Method actualizado a SKBitmap
        public static SKBitmap Sprite(this PKM pk)
        {
            return SpriteUtil.Spriter.GetSprite(
                pk.Species,
                pk.Form,
                pk.Gender,
                pk is IFormArgument f ? f.FormArgument : 0,
                pk.SpriteItem,
                pk.IsEgg,
                ShinyExtensions.GetType(pk),
                pk.Context
            );
        }
        public static SKBitmap Sprite(this IEncounterTemplate enc)
        {
            // 1. Mystery Gift
            if (enc is MysteryGift g)
                return SpriteUtil.GetMysteryGiftPreviewPoke(g);

            // 2. Gender + Shiny
            var gender = GetDisplayGender(enc);
            var shiny = enc.IsShiny ? Shiny.Always : Shiny.Never;

            // 3. Sprite base
            SKBitmap img = SpriteUtil.Spriter.GetSprite(
                enc.Species,
                enc.Form,
                gender,
                0,              // formarg
                0,              // heldItem (encounter templates no tienen item)
                enc.IsEgg,
                shiny,
                enc.Context
            );

            // 4. Pokéball del encuentro
            if (SpriteBuilder.ShowEncounterBall && enc is { FixedBall: not Ball.None })
            {
                var ballSprite = SpriteUtil.GetBallSprite((byte)enc.FixedBall);
                if (ballSprite != null)
                    img = ImageUtil.LayerImage(img, ballSprite, 0, img.Height - ballSprite.Height);
            }

            // 5. Gigantamax
            if (enc is IGigantamaxReadOnly { CanGigantamax: true })
            {
                var gm = SpriteImgLoader.LoadSprite("dyna"); // reemplazo de Resources.dyna
                if (gm != null)
                    img = ImageUtil.LayerImage(img, gm, (img.Width - gm.Width) / 2, 0);
            }

            // 6. Alpha Pokémon
            if (enc is IAlphaReadOnly { IsAlpha: true })
            {
                var alpha = SpriteImgLoader.LoadSprite("alpha_alt"); // reemplazo de Resources.alpha_alt
                if (alpha != null)
                    img = ImageUtil.LayerImage(img, alpha, SlotTeamShiftX, 0);
            }

            // 7. Color de Encuentro
            if (SpriteBuilder.ShowEncounterColor != SpriteBackgroundType.None)
                img = SpriteUtil.ApplyEncounterColor(enc, img, SpriteBuilder.ShowEncounterColor);

            return img;
        }
        public static byte GetDisplayGender(IEncounterTemplate enc) => enc switch
        {
            IFixedGender { IsFixedGender: true } s => Math.Max((byte)0, s.Gender),
            IPogoSlot g => (byte)((int)g.Gender & 1),
            _ => 0,
        };

        public static SKBitmap Sprite(this PKM pk,SaveFile sav,int box = -1,int slot = -1,bool flagIllegal = false,StorageSlotType storage = StorageSlotType.None)
        {
            return GetSprite(pk, sav, box, slot, flagIllegal, storage);
        }

        public static SKBitmap GetMysteryGiftPreviewPoke(MysteryGift gift)
        {
            // --- CASO ESPECIAL: Huevo de Manaphy ---
            if (gift is { IsEgg: true, Species: (int)Species.Manaphy })
                return SpriteUtil.Spriter.GetSprite(
                    (ushort)Species.Manaphy,
                    0,
                    2,
                    0,
                    0,
                    true,
                    Shiny.Never,
                    gift.Context
                );

            // --- GÉNERO ---
            byte gender = Math.Max((byte)0, gift.Gender);

            // --- SPRITE BASE ---
            var img = SpriteUtil.Spriter.GetSprite(
                (ushort)gift.Species,
                gift.Form,
                gender,
                0,
                gift.HeldItem,
                gift.IsEgg,
                gift.IsShiny ? Shiny.Always : Shiny.Never,
                gift.Context
            );

            // --- ENCOUNTER BALL ---
            if (SpriteBuilder.ShowEncounterBall && gift is { FixedBall: not Ball.None })
            {
                var ballSprite = GetBallSprite((byte)gift.FixedBall); // ← ya lo tienes convertido en SKBitmap
                img = ImageUtil.LayerImage(
                    img,
                    ballSprite,
                    0,
                    img.Height - ballSprite.Height
                );
            }

            // --- GIGANTAMAX ---
            if (gift is IGigantamaxReadOnly { CanGigantamax: true })
            {
                var gm = SpriteImgLoader.LoadSprite("dyna"); // ← reemplazo de Resources.dyna
                if (gm != null)
                {
                    img = ImageUtil.LayerImage(
                        img,
                        gm,
                        (img.Width - gm.Width) / 2,
                        0
                    );
                }
            }

            return img;
        }

        public static SKBitmap? GetStatusSprite(this StatusType value)
        {
            return value switch
            {
                StatusType.None => null,
                StatusType.Paralysis => SpriteImgLoader.LoadSprite("sickparalyze"),
                StatusType.Sleep => SpriteImgLoader.LoadSprite("sicksleep"),
                StatusType.Freeze => SpriteImgLoader.LoadSprite("sickfrostbite"),
                StatusType.Burn => SpriteImgLoader.LoadSprite("sickburn"),
                StatusType.Poison => SpriteImgLoader.LoadSprite("sickpoison"),
                _ => null,
            };
        }


    }
}
