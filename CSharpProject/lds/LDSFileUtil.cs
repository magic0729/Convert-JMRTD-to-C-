using System;
using System.Collections.Generic;

namespace org.jmrtd.lds
{
    public static class LDSFileUtil
    {
        public static readonly Dictionary<short, byte> FID_TO_SFI = new Dictionary<short, byte>
        {
            { 257, 1 },  // EF_DG1
            { 258, 2 },  // EF_DG2
            { 259, 3 },  // EF_DG3
            { 260, 4 },  // EF_DG4
            { 261, 5 },  // EF_DG5
            { 262, 6 },  // EF_DG6
            { 263, 7 },  // EF_DG7
            { 264, 8 },  // EF_DG8
            { 265, 9 },  // EF_DG9
            { 266, 10 }, // EF_DG10
            { 267, 11 }, // EF_DG11
            { 268, 12 }, // EF_DG12
            { 269, 13 }, // EF_DG13
            { 270, 14 }, // EF_DG14
            { 271, 15 }, // EF_DG15
            { 272, 16 }, // EF_DG16
            { 284, 28 }, // EF_CARD_ACCESS
            { 285, 29 }, // EF_CARD_SECURITY / EF_SOD
            { 286, 30 }  // EF_COM
        };
    }
}
