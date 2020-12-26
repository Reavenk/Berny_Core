﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PxPre
{
    namespace Berny
    {
        namespace CFF
        {
            public class Strings
            {
                // The last predefined string in the CFF (5176.CFF) appendix (+1)
                const int StdNStrings = 391;

                public static readonly string[] StandardStrings =
                    new string[]
                    {
                        ".notdef",
                        "space",
                        "exclam",
                        "quotedbl",
                        "numbersign",
                        "dollar",
                        "percent",
                        "ampersand",
                        "quoteright",
                        "parenleft",
                        "parenright",
                        "asterisk",
                        "plus",
                        "comma",
                        "hyphen",
                        "period",
                        "slash",
                        "zero",
                        "one",
                        "two",
                        "three",
                        "four",
                        "five",
                        "six",
                        "seven",
                        "eight",
                        "nine",
                        "colon",
                        "semicolon",
                        "less",
                        "equal",
                        "greater",
                        "question",
                        "at",
                        "A",
                        "B",
                        "C",
                        "D",
                        "E",
                        "F",
                        "G",
                        "H",
                        "I",
                        "J",
                        "K",
                        "L",
                        "M",
                        "N",
                        "O",
                        "P",
                        "Q",
                        "R",
                        "S",
                        "T",
                        "U",
                        "V",
                        "W",
                        "X",
                        "Y",
                        "Z",
                        "bracketleft",
                        "backslash",
                        "bracketright",
                        "AppendixAStandardStrings(18Mar98)",
                        "asciicircum",
                        "underscore",
                        "quoteleft",
                        "a",
                        "b",
                        "c",
                        "d",
                        "e",
                        "f",
                        "g",
                        "h",
                        "i",
                        "j",
                        "k",
                        "l",
                        "m",
                        "n",
                        "o",
                        "p",
                        "q",
                        "r",
                        "s",
                        "t",
                        "u",
                        "v",
                        "w",
                        "x",
                        "y",
                        "z",
                        "braceleft",
                        "bar",
                        "braceright",
                        "asciitilde",
                        "exclamdown",
                        "cent",
                        "sterling",
                        "fraction",
                        "yen",
                        "florin",
                        "section",
                        "currency",
                        "quotesingle",
                        "quotedblleft",
                        "guillemotleft",
                        "guilsinglleft",
                        "guilsinglright",
                        "fi",
                        "fl",
                        "endash",
                        "dagger",
                        "daggerdbl",
                        "periodcentered",
                        "paragraph",
                        "bullet",
                        "quotesinglbase",
                        "quotedblbase",
                        "quotedblright",
                        "guillemotright",
                        "ellipsis",
                        "perthousand",
                        "questiondown",
                        "grave",
                        "acute",
                        "circumflex",
                        "tilde",
                        "macron",
                        "breve",
                        "dotaccent",
                        "dieresis",
                        "ring",
                        "cedilla",
                        "hungarumlaut",
                        "ogonek",
                        "caron",
                        "emdash",
                        "AE",
                        "ordfeminine",
                        "Lslash",
                        "Oslash",
                        "OE",
                        "ordmasculine",
                        "ae",
                        "dotlessi",
                        "lslash",
                        "oslash",
                        "oe",
                        "germandbls",
                        "onesuperior",
                        "logicalnot",
                        "mu",
                        "trademark",
                        "Eth",
                        "onehalf",
                        "plusminus",
                        "Thorn",
                        "onequarter",
                        "divide",
                        "brokenbar",
                        "degree",
                        "thorn",
                        "threequarters",
                        "twosuperior",
                        "registered",
                        "minus",
                        "eth",
                        "multiply",
                        "threesuperior",
                        "copyright",
                        "Aacute",
                        "Acircumflex",
                        "Adieresis",
                        "Agrave",
                        "Aring",
                        "Atilde",
                        "Ccedilla",
                        "Eacute",
                        "Ecircumflex",
                        "Edieresis",
                        "Egrave",
                        "Iacute",
                        "Icircumflex",
                        "Idieresis",
                        "Igrave",
                        "Ntilde",
                        "Oacute",
                        "Ocircumflex",
                        "Odieresis",
                        "Ograve",
                        "Otilde",
                        "Scaron",
                        "Uacute",
                        "Ucircumflex",
                        "Udieresis",
                        "Ugrave",
                        "Yacute",
                        "Ydieresis",
                        "Zcaron",
                        "aacute",
                        "acircumflex",
                        "adieresis",
                        "agrave",
                        "aring",
                        "atilde",
                        "ccedilla",
                        "eacute",
                        "ecircumflex",
                        "edieresis",
                        "egrave",
                        "iacute",
                        "icircumflex",
                        "idieresis",
                        "igrave",
                        "ntilde",
                        "oacute",
                        "ocircumflex",
                        "odieresis",
                        "ograve",
                        "otilde",
                        "scaron",
                        "uacute",
                        "ucircumflex",
                        "udieresis",
                        "ugrave",
                        "yacute",
                        "ydieresis",
                        "zcaron",
                        "exclamsmall",
                        "Hungarumlautsmall",
                        "dollaroldstyle",
                        "dollarsuperior",
                        "ampersandsmall",
                        "Acutesmall",
                        "parenleftsuperior",
                        "parenrightsuperior",
                        "twodotenleader",
                        "onedotenleader",
                        "zerooldstyle",
                        "oneoldstyle",
                        "twooldstyle",
                        "threeoldstyle",
                        "fouroldstyle",
                        "fiveoldstyle",
                        "sixoldstyle",
                        "sevenoldstyle",
                        "eightoldstyle",
                        "nineoldstyle",
                        "commasuperior",
                        "threequartersemdash",
                        "periodsuperior",
                        "questionsmall",
                        "asuperior",
                        "bsuperior",
                        "centsuperior",
                        "dsuperior",
                        "esuperior",
                        "isuperior",
                        "lsuperior",
                        "msuperior",
                        "nsuperior",
                        "osuperior",
                        "rsuperior",
                        "ssuperior",
                        "tsuperior",
                        "ff",
                        "ffi",
                        "ffl",
                        "parenleftinferior",
                        "parenrightinferior",
                        "Circumflexsmall",
                        "hyphensuperior",
                        "Gravesmall",
                        "Asmall",
                        "Bsmall",
                        "Csmall",
                        "Dsmall",
                        "Esmall",
                        "Fsmall",
                        "Gsmall",
                        "Hsmall",
                        "Ismall",
                        "Jsmall",
                        "Ksmall",
                        "Lsmall",
                        "Msmall",
                        "Nsmall",
                        "Osmall",
                        "Psmall",
                        "Qsmall",
                        "Rsmall",
                        "Ssmall",
                        "Tsmall",
                        "Usmall",
                        "Vsmall",
                        "Wsmall",
                        "Xsmall",
                        "Ysmall",
                        "Zsmall",
                        "colonmonetary",
                        "onefitted",
                        "rupiah",
                        "Tildesmall",
                        "exclamdownsmall",
                        "centoldstyle",
                        "Lslashsmall",
                        "Scaronsmall",
                        "Zcaronsmall",
                        "Dieresissmall",
                        "Brevesmall",
                        "Caronsmall",
                        "Dotaccentsmall",
                        "Macronsmall",
                        "figuredash",
                        "hypheninferior",
                        "Ogoneksmall",
                        "Ringsmall",
                        "Cedillasmall",
                        "questiondownsmall",
                        "oneeighth",
                        "threeeighths",
                        "fiveeighths",
                        "seveneighths",
                        "onethird",
                        "twothirds",
                        "zerosuperior",
                        "foursuperior",
                        "fivesuperior",
                        "sixsuperior",
                        "sevensuperior",
                        "eightsuperior",
                        "ninesuperior",
                        "zeroinferior",
                        "oneinferior",
                        "twoinferior",
                        "threeinferior",
                        "fourinferior",
                        "fiveinferior",
                        "sixinferior",
                        "seveninferior",
                        "eightinferior",
                        "nineinferior",
                        "centinferior",
                        "dollarinferior",
                        "periodinferior",
                        "commainferior",
                        "Agravesmall",
                        "Aacutesmall",
                        "Acircumflexsmall",
                        "Atildesmall",
                        "Adieresissmall",
                        "Aringsmall",
                        "AEsmall",
                        "Ccedillasmall",
                        "Egravesmall",
                        "Eacutesmall",
                        "Ecircumflexsmall",
                        "Edieresissmall",
                        "Igravesmall",
                        "Iacutesmall",
                        "Icircumflexsmall",
                        "Idieresissmall",
                        "Ethsmall",
                        "Ntildesmall",
                        "Ogravesmall",
                        "Oacutesmall",
                        "Ocircumflexsmall",
                        "Otildesmall",
                        "Odieresissmall",
                        "OEsmall",
                        "Oslashsmall",
                        "Ugravesmall",
                        "Uacutesmall",
                        "Ucircumflexsmall",
                        "Udieresissmall",
                        "Yacutesmall",
                        "Thornsmall",
                        "Ydieresissmall",
                        "001.000",
                        "001.001",
                        "001.002",
                        "001.003",
                        "Black",
                        "Bold",
                        "Book",
                        "Light",
                        "Medium",
                        "Regular",
                        "Roman",
                        "Semibold"
                    };

                public List<string> uniqueStrings = new List<string>();

                public int Add(string str)
                { 
                    int ct = this.uniqueStrings.Count;
                    this.uniqueStrings.Add(str);
                    return StdNStrings + ct;
                }

                public string GetString(int sid)
                {
                    if (sid == -1)
                        return "Value not set.";

                    if (sid < StdNStrings)
                        return "Value not stored.";

                    int idx = sid - StdNStrings;
                    if (idx < 0 || idx >= this.uniqueStrings.Count)
                        return "SID out of bounds.";

                    return this.uniqueStrings[idx];
                }

                public void Clear()
                { 
                    this.uniqueStrings.Clear();
                }
            }
        }
    }
}