using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Timers;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.X64;
using VinesauceModSettings.Configuration;

namespace VinesauceModSettings
{
    internal class CmpBgColor
    {
        public static void SetCmpBgColor(uint color)
        {
            CmpBgColor._r = (color >> 24 & 255U);
            CmpBgColor._g = (color >> 16 & 255U);
            CmpBgColor._b = (color >> 8 & 255U);
        }

        public static void UpdateConfig(Config config)
        {
            CmpBgColor._config = config;
            uint num;
            if (CmpBgColor._config.CmpBgColor.Length == 7 && uint.TryParse(CmpBgColor._config.CmpBgColor.TrimStart('#'), NumberStyles.HexNumber, null, out num))
            {
                Logger logger = CmpBgColor._logger;
                DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(19, 1);
                defaultInterpolatedStringHandler.AppendLiteral("Parsed CmpBgColor #");
                defaultInterpolatedStringHandler.AppendFormatted<uint>(num, "X6");
                logger.Info(defaultInterpolatedStringHandler.ToStringAndClear());
                CmpBgColor._color = num;
            }
            else
            {
                CmpBgColor._logger.Error("Failed to parse CmpBgColor - format #RRGGBB, e.g. #FF0023");
            }
            switch (CmpBgColor._config.CmpBgColorMode)
            {
                case Config.Mode.Off:
                    CmpBgColor._enable = false;
                    CmpBgColor._colorSweep.Enabled = false;
                    return;
                case Config.Mode.Color:
                    CmpBgColor.SetCmpBgColor(CmpBgColor._color << 8 | 255U);
                    CmpBgColor._enable = true;
                    CmpBgColor._colorSweep.Enabled = false;
                    return;
                case Config.Mode.ColorSweep:
                    CmpBgColor._enable = true;
                    CmpBgColor._colorSweep.Enabled = true;
                    return;
                default:
                    return;
            }
        }

        public static void Activate(in PatchContext context)
        {
            IReloadedHooks hooks = context.Hooks;
            IntPtr baseAddress = context.BaseAddress;
            CmpBgColor._logger = context.Logger;
            CmpBgColor._colorSweep = new System.Timers.Timer
            {
                Interval = 16.66666603088379
            };
            CmpBgColor._colorSweep.Elapsed += CmpBgColor.ColorSweep;
            CmpBgColor.UpdateConfig(_config);
            context.ScanHelper.FindPatternOffset("48 8B C4 48 89 58 08 48 89 68 10 48 89 70 18 48 89 78 20 41 56 48 81 EC 90 00 00 00 48 8B BC 24 D0 00 00 00 0F", delegate (uint o)
            {
                CmpBgColor._testHook = hooks.CreateHook<CmpBgColor.sub_140C7C9E0>(new CmpBgColor.sub_140C7C9E0(CmpBgColor.TestHookImpl), (long)baseAddress + (long)((ulong)o));
                CmpBgColor._testHook.Activate();
            }, "P_CMP_BG_COLOR");
            context.ScanHelper.FindPatternOffset("48 8B C4 48 89 58 18 48 89 70 20 F3 0F 11 40 08 55 57 41 55", delegate (uint o)
            {
                unsafe
                {
                    CmpBgColor._testHook2 = hooks.CreateHook<CmpBgColor.sub_1416F3F10>(new CmpBgColor.sub_1416F3F10(CmpBgColor.TestHook2Impl), (long)baseAddress + (long)((ulong)o));
                }
                CmpBgColor._testHook2.Activate();
            }, "P_CMP_BG_COLOR_2");
            context.ScanHelper.FindPatternOffset("48 8B C4 53 48 81 EC E0 00 00 00 44", delegate (uint o)
            {
                CmpBgColor._testHook3 = hooks.CreateHook<CmpBgColor.sub_140C7DB80>(new CmpBgColor.sub_140C7DB80(CmpBgColor.TestHook3Impl), (long)baseAddress + (long)((ulong)o));
                CmpBgColor._testHook3.Activate();
            }, "P_CMP_BG_COLOR_3");
            context.ScanHelper.FindPatternOffset("40 56 48 81 EC 80 00 00 00 80", delegate (uint o)
            {
                CmpBgColor._testHook4 = hooks.CreateHook<CmpBgColor.sub_140C72590>(new CmpBgColor.sub_140C72590(CmpBgColor.TestHook4Impl), (long)baseAddress + (long)((ulong)o));
                CmpBgColor._testHook4.Activate();
            }, "P_CMP_BG_COLOR_4");
        }

        private static void ColorSweep( object sender, ElapsedEventArgs e)
        {
            CmpBgColor._r = (uint)CmpBgColor._sins[CmpBgColor._currSin % 360];
            CmpBgColor._g = (uint)CmpBgColor._sins[(CmpBgColor._currSin + 120) % 360];
            CmpBgColor._b = (uint)CmpBgColor._sins[(CmpBgColor._currSin + 240) % 360];
            CmpBgColor._currSin++;
            CmpBgColor._currSin %= 360;
        }

        private static long TestHook4Impl(long a1, long a2)
        {
            CmpBgColor._setB = true;
            long result = CmpBgColor._testHook4.OriginalFunction(a1, a2);
            CmpBgColor._setB = false;
            return result;
        }

        private static long TestHook3Impl(long a1, long a2, float a3)
        {
            CmpBgColor._setA = true;
            long result = CmpBgColor._testHook3.OriginalFunction(a1, a2, a3);
            CmpBgColor._setA = false;
            return result;
        }
        
        private unsafe static long TestHook2Impl(float a1, float a2, float a3, float a4, float a5, uint* a6, float a7, float a8, int a9, byte a10, int a11)
        {
            if (CmpBgColor._enable && (CmpBgColor._setA || CmpBgColor._setB))
            {
                *a6 = (CmpBgColor._r << 24 | CmpBgColor._g << 16 | CmpBgColor._b << 8 | (*a6 & 255U));
                a6[1] = (CmpBgColor._r << 24 | CmpBgColor._g << 16 | CmpBgColor._b << 8 | (a6[1] & 255U));
                a6[2] = (CmpBgColor._r << 24 | CmpBgColor._g << 16 | CmpBgColor._b << 8 | (a6[2] & 255U));
                a6[3] = (CmpBgColor._r << 24 | CmpBgColor._g << 16 | CmpBgColor._b << 8 | (a6[3] & 255U));
            }
            return CmpBgColor._testHook2.OriginalFunction(a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11);
        }

        // Token: 0x0600006D RID: 109 RVA: 0x00002E04 File Offset: 0x00001004
        private static long TestHookImpl(long a1, long a2, byte r, byte g, byte b, uint a6, long a7)
        {
            if (CmpBgColor._enable && (CmpBgColor._setA || CmpBgColor._setB) && r == 255 && g == 0 && b == 35)
            {
                r = (byte)CmpBgColor._r;
                g = (byte)CmpBgColor._g;
                b = (byte)CmpBgColor._b;
            }
            return CmpBgColor._testHook.OriginalFunction(a1, a2, r, g, b, a6, a7);
        }

        private const string P_CMP_BG_COLOR = "48 8B C4 48 89 58 08 48 89 68 10 48 89 70 18 48 89 78 20 41 56 48 81 EC 90 00 00 00 48 8B BC 24 D0 00 00 00 0F";

        private const string P_CMP_BG_COLOR_2 = "48 8B C4 48 89 58 18 48 89 70 20 F3 0F 11 40 08 55 57 41 55";

        private const string P_CMP_BG_COLOR_3 = "48 8B C4 53 48 81 EC E0 00 00 00 44";

        private const string P_CMP_BG_COLOR_4 = "40 56 48 81 EC 80 00 00 00 80";

        private static IHook<CmpBgColor.sub_140C7C9E0> _testHook;


        private static IHook<CmpBgColor.sub_1416F3F10> _testHook2;


        private static IHook<CmpBgColor.sub_140C7DB80> _testHook3;


        private static IHook<CmpBgColor.sub_140C72590> _testHook4;

        // Token: 0x04000035 RID: 53
        private static Logger _logger = null;

        // Token: 0x04000036 RID: 54
        private static System.Timers.Timer _colorSweep = null;

        // Token: 0x04000037 RID: 55
        private static Config _config = null;

        // Token: 0x04000038 RID: 56
        private static bool _enable = false;

        // Token: 0x04000039 RID: 57
        private static uint _color = 16711715U;

        // Token: 0x0400003A RID: 58
        private static uint _r = 255U;

        // Token: 0x0400003B RID: 59
        private static uint _g = 0U;

        // Token: 0x0400003C RID: 60
        private static uint _b = 35U;

        // Token: 0x0400003D RID: 61
        private static bool _setA = false;

        // Token: 0x0400003E RID: 62
        private static bool _setB = false;

        // Token: 0x0400003F RID: 63
        private static int _currSin = 0;

        // Token: 0x04000040 RID: 64
        private static readonly byte[] _sins = new byte[]
        {
            127,
            129,
            131,
            134,
            136,
            138,
            140,
            143,
            145,
            147,
            149,
            151,
            154,
            156,
            158,
            160,
            162,
            164,
            166,
            169,
            171,
            173,
            175,
            177,
            179,
            181,
            183,
            185,
            187,
            189,
            191,
            193,
            195,
            196,
            198,
            200,
            202,
            204,
            205,
            207,
            209,
            211,
            212,
            214,
            216,
            217,
            219,
            220,
            222,
            223,
            225,
            226,
            227,
            229,
            230,
            231,
            233,
            234,
            235,
            236,
            237,
            239,
            240,
            241,
            242,
            243,
            243,
            244,
            245,
            246,
            247,
            248,
            248,
            249,
            250,
            250,
            251,
            251,
            252,
            252,
            253,
            253,
            253,
            254,
            254,
            254,
            254,
            254,
            254,
            254,
            byte.MaxValue,
            254,
            254,
            254,
            254,
            254,
            254,
            254,
            253,
            253,
            253,
            252,
            252,
            251,
            251,
            250,
            250,
            249,
            248,
            248,
            247,
            246,
            245,
            244,
            243,
            243,
            242,
            241,
            240,
            239,
            237,
            236,
            235,
            234,
            233,
            231,
            230,
            229,
            227,
            226,
            225,
            223,
            222,
            220,
            219,
            217,
            216,
            214,
            212,
            211,
            209,
            207,
            205,
            204,
            202,
            200,
            198,
            196,
            195,
            193,
            191,
            189,
            187,
            185,
            183,
            181,
            179,
            177,
            175,
            173,
            171,
            169,
            166,
            164,
            162,
            160,
            158,
            156,
            154,
            151,
            149,
            147,
            145,
            143,
            140,
            138,
            136,
            134,
            131,
            129,
            127,
            125,
            123,
            120,
            118,
            116,
            114,
            111,
            109,
            107,
            105,
            103,
            100,
            98,
            96,
            94,
            92,
            90,
            88,
            85,
            83,
            81,
            79,
            77,
            75,
            73,
            71,
            69,
            67,
            65,
            63,
            61,
            59,
            58,
            56,
            54,
            52,
            50,
            49,
            47,
            45,
            43,
            42,
            40,
            38,
            37,
            35,
            34,
            32,
            31,
            29,
            28,
            27,
            25,
            24,
            23,
            21,
            20,
            19,
            18,
            17,
            15,
            14,
            13,
            12,
            11,
            11,
            10,
            9,
            8,
            7,
            6,
            6,
            5,
            4,
            4,
            3,
            3,
            2,
            2,
            1,
            1,
            1,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            1,
            1,
            1,
            2,
            2,
            3,
            3,
            4,
            4,
            5,
            6,
            6,
            7,
            8,
            9,
            10,
            11,
            11,
            12,
            13,
            14,
            15,
            17,
            18,
            19,
            20,
            21,
            23,
            24,
            25,
            27,
            28,
            29,
            31,
            32,
            34,
            35,
            37,
            38,
            40,
            42,
            43,
            45,
            47,
            49,
            50,
            52,
            54,
            56,
            58,
            59,
            61,
            63,
            65,
            67,
            69,
            71,
            73,
            75,
            77,
            79,
            81,
            83,
            85,
            88,
            90,
            92,
            94,
            96,
            98,
            100,
            103,
            105,
            107,
            109,
            111,
            114,
            116,
            118,
            120,
            123,
            125
        };
        
        [Function(CallingConventions.Microsoft)]
        private delegate long sub_140C7C9E0(long a1, long a2, byte a3, byte a4, byte a5, uint a6, long a7);
        
        [Function(CallingConventions.Microsoft)]
        private unsafe delegate long sub_1416F3F10(float a1, float a2, float a3, float a4, float a5, uint* a6, float a7, float a8, int a9, byte a10, int a11);
        
        [Function(CallingConventions.Microsoft)]
        private delegate long sub_140C7DB80(long a1, long a2, float a3);
        
        [Function(CallingConventions.Microsoft)]
        private delegate long sub_140C72590(long a1, long a2);
    }
}
