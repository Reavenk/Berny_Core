// MIT License
// 
// Copyright (c) 2020 Pixel Precision LLC
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PxPre
{
    namespace Berny
    {
        namespace TTF
        {
            namespace Table
            {
                /// <summary>
                /// OS/2 — OS/2 and Windows Metrics Table
                /// https://docs.microsoft.com/en-us/typography/opentype/spec/os2
                /// 
                /// The OS/2 table consists of a set of metrics and other data that 
                /// are required in OpenType fonts.
                /// 
                /// Six versions of the OS/2 table have been defined: versions 0 to 5. 
                /// All versions are supported, but use of version 4 or later is strongly 
                /// recommended.
                /// </summary>
                public struct OS2
                {
                    public enum WeightClass
                    { 
                        Thin            = 100,
                        ExtraLight      = 200,
                        Light           = 300,
                        Normal          = 400,
                        Medium          = 500,
                        SemiBold        = 600, 
                        Bold            = 700,
                        ExtraBold       = 800,
                        Black           = 900
                    }

                    public enum WidthClass
                    { 
                        UltraCondensed  = 1,
                        ExtraCondensed  = 2,
                        Condensed       = 3,
                        SemiCondensed   = 4,
                        Medium          = 5,
                        SemiExpanded    = 6,
                        Expanded        = 7,
                        ExtraExpanded   = 8,
                        UltaExpanded    = 9
                    }

                    /// <summary>
                    /// In a variable font, various font-metric values within the OS/2 table may need to be adjusted for different variation instances. 
                    /// Variation data for OS/2 entries can be provided in the metrics variations (MVAR) table. Different OS/2 entries are associated 
                    /// with particular variation data in the MVAR table using value tags
                    /// </summary>
                    public static class VarTag
                    {
                        public const string sCapHeight          = "cpht";
                        public const string sTypoAscender	    = "hasc";
                        public const string sTypoDescender	    = "hdsc";
                        public const string sTypoLineGap	    = "hlgp";
                        public const string sxHeight	        = "xhgt";
                        public const string usWinAscent	        = "hcla";
                        public const string usWinDescent	    = "hcld";
                        public const string yStrikeoutPosition	= "stro";
                        public const string yStrikeoutSize	    = "strs";
                        public const string ySubscriptXOffset	= "sbxo";
                        public const string ySubScriptXSize	    = "sbxs";
                        public const string ySubscriptYOffset	= "sbyo";
                        public const string ySubscriptYSize	    = "sbys";
                        public const string ySuperscriptXOffset	= "spxo";
                        public const string ySuperscriptXSize	= "spxs";
                        public const string ySuperscriptYOffset	= "spyo";
                        public const string ySuperscriptYSize	= "spys";
                    }

                    public const string TagName = "OS/2";

                    public ushort version;
                    public short xAvgCharWidth;
                    public ushort usWeightClass;
                    public ushort usWidthClass;
                    public ushort fsType;
                    public short ySubscriptXSize;
                    public short ySubscriptYSize;
                    public short ySubscriptXOffset;
                    public short ySubscriptYOffset;
                    public short ySuperscriptXSize;
                    public short ySuperscriptYSize;
                    public short ySuperscriptXOffset;
                    public short ySuperscriptYOffset;
                    public short yStrikeoutSize;
                    public short yStrikeoutPosition;
                    public short sFamilyClass;
                    public char [] panose;
                    public uint ulUnicodeRange1;
                    public uint ulUnicodeRange2;
                    public uint ulUnicodeRange3;
                    public uint ulUnicodeRange4;
                    public string achVendID;
                    public ushort fsSelection;
                    public ushort usFirstCharIndex;
                    public ushort usLastCharIndex;
                    public short sTypoAscender;
                    public short sTypoDescender;
                    public short sTypoLineGap;
                    public ushort usWinAscent;
                    public ushort usWinDescent;

                    public int ulCodePageRange1;
                    public int ulCodePageRange2;

                    public short sxHeight;
                    public short sCapHeight;
                    public ushort usDefaultChar;
                    public ushort usBreakChar;
                    public ushort usMaxContext;

                    public ushort usLowerOpticalPointSize;
                    public ushort usUpperOpticalPointSize;

                    static char [] ReadPanose(TTFReader r)
                    { 
                        byte [] rb = r.ReadBytes(10);
                        char [] rc = 
                            new char[] 
                            { 
                                (char)rb[0], (char)rb[1], (char)rb[2], (char)rb[3], (char)rb[4], 
                                (char)rb[5], (char)rb[6], (char)rb[7], (char)rb[8], (char)rb[9]
                            };

                        return rc;
                    }

                    public void Read(TTFReader r)
                    { 
                        r.ReadInt(out this.version);

                        if(this.version == 0)
                        {
                            r.ReadInt(out this.xAvgCharWidth);
                            r.ReadInt(out this.usWeightClass);
                            r.ReadInt(out this.usWidthClass);
                            r.ReadInt(out this.fsType);
                            r.ReadInt(out this.ySubscriptXSize);
                            r.ReadInt(out this.ySubscriptYSize);
                            r.ReadInt(out this.ySubscriptXOffset);
                            r.ReadInt(out this.ySubscriptYOffset);
                            r.ReadInt(out this.ySuperscriptXSize);
                            r.ReadInt(out this.ySuperscriptYSize);
                            r.ReadInt(out this.ySuperscriptXOffset);
                            r.ReadInt(out this.ySuperscriptYOffset);
                            r.ReadInt(out this.yStrikeoutSize);
                            r.ReadInt(out this.yStrikeoutPosition);
                            r.ReadInt(out this.sFamilyClass);
                            this.panose = ReadPanose(r);
                            r.ReadInt(out this.ulUnicodeRange1);
                            r.ReadInt(out this.ulUnicodeRange2);
                            r.ReadInt(out this.ulUnicodeRange3);
                            r.ReadInt(out this.ulUnicodeRange4);
                            this.achVendID = r.ReadString(4);
                            r.ReadInt(out this.fsSelection);
                            r.ReadInt(out this.usFirstCharIndex);
                            r.ReadInt(out this.usLastCharIndex);
                            r.ReadInt(out this.sTypoAscender);
                            r.ReadInt(out this.sTypoDescender);
                            r.ReadInt(out this.sTypoLineGap);
                            r.ReadInt(out this.usWinAscent);
                            r.ReadInt(out this.usWinDescent);
                        }
                        else if(this.version == 1)
                        {
                            r.ReadInt(out this.version);
                            r.ReadInt(out this.xAvgCharWidth);
                            r.ReadInt(out this.usWeightClass);
                            r.ReadInt(out this.usWidthClass);
                            r.ReadInt(out this.fsType);
                            r.ReadInt(out this.ySubscriptXSize);
                            r.ReadInt(out this.ySubscriptYSize);
                            r.ReadInt(out this.ySubscriptXOffset);
                            r.ReadInt(out this.ySubscriptYOffset);
                            r.ReadInt(out this.ySuperscriptXSize);
                            r.ReadInt(out this.ySuperscriptYSize);
                            r.ReadInt(out this.ySuperscriptXOffset);
                            r.ReadInt(out this.ySuperscriptYOffset);
                            r.ReadInt(out this.yStrikeoutSize);
                            r.ReadInt(out this.yStrikeoutPosition);
                            r.ReadInt(out this.sFamilyClass);
                            this.panose = ReadPanose(r);
                            r.ReadInt(out this.ulUnicodeRange1);
                            r.ReadInt(out this.ulUnicodeRange2);
                            r.ReadInt(out this.ulUnicodeRange3);
                            r.ReadInt(out this.ulUnicodeRange4);
                            this.achVendID = r.ReadString(4);
                            r.ReadInt(out this.fsSelection);
                            r.ReadInt(out this.usFirstCharIndex);
                            r.ReadInt(out this.usLastCharIndex);
                            r.ReadInt(out this.sTypoAscender);
                            r.ReadInt(out this.sTypoDescender);
                            r.ReadInt(out this.sTypoLineGap);
                            r.ReadInt(out this.usWinAscent);
                            r.ReadInt(out this.usWinDescent);
                            r.ReadInt(out this.ulCodePageRange1);
                            r.ReadInt(out this.ulCodePageRange2);
                        }
                        else if(this.version == 2 || this.version == 3 || this.version == 4)
                        {
                            r.ReadInt(out this.xAvgCharWidth);
                            r.ReadInt(out this.usWeightClass);
                            r.ReadInt(out this.usWidthClass);
                            r.ReadInt(out this.fsType);
                            r.ReadInt(out this.ySubscriptXSize);
                            r.ReadInt(out this.ySubscriptYSize);
                            r.ReadInt(out this.ySubscriptXOffset);
                            r.ReadInt(out this.ySubscriptYOffset);
                            r.ReadInt(out this.ySuperscriptXSize);
                            r.ReadInt(out this.ySuperscriptYSize);
                            r.ReadInt(out this.ySuperscriptXOffset);
                            r.ReadInt(out this.ySuperscriptYOffset);
                            r.ReadInt(out this.yStrikeoutSize);
                            r.ReadInt(out this.yStrikeoutPosition);
                            r.ReadInt(out this.sFamilyClass);
                            this.panose = ReadPanose(r);
                            r.ReadInt(out this.ulUnicodeRange1);
                            r.ReadInt(out this.ulUnicodeRange2);
                            r.ReadInt(out this.ulUnicodeRange3);
                            r.ReadInt(out this.ulUnicodeRange4);
                            this.achVendID = r.ReadString(4);
                            r.ReadInt(out this.fsSelection);
                            r.ReadInt(out this.usFirstCharIndex);
                            r.ReadInt(out this.usLastCharIndex);
                            r.ReadInt(out this.sTypoAscender);
                            r.ReadInt(out this.sTypoDescender);
                            r.ReadInt(out this.sTypoLineGap);
                            r.ReadInt(out this.usWinAscent);
                            r.ReadInt(out this.usWinDescent);
                            r.ReadInt(out this.ulCodePageRange1);
                            r.ReadInt(out this.ulCodePageRange2);
                            r.ReadInt(out this.sxHeight);
                            r.ReadInt(out this.sCapHeight);
                            r.ReadInt(out this.usDefaultChar);
                            r.ReadInt(out this.usBreakChar);
                            r.ReadInt(out this.usMaxContext);
                        }
                        else if(this.version == 5)
                        {
                            r.ReadInt(out this.xAvgCharWidth);
                            r.ReadInt(out this.usWeightClass);
                            r.ReadInt(out this.usWidthClass);
                            r.ReadInt(out this.fsType);
                            r.ReadInt(out this.ySubscriptXSize);
                            r.ReadInt(out this.ySubscriptYSize);
                            r.ReadInt(out this.ySubscriptXOffset);
                            r.ReadInt(out this.ySubscriptYOffset);
                            r.ReadInt(out this.ySuperscriptXSize);
                            r.ReadInt(out this.ySuperscriptYSize);
                            r.ReadInt(out this.ySuperscriptXOffset);
                            r.ReadInt(out this.ySuperscriptYOffset);
                            r.ReadInt(out this.yStrikeoutSize);
                            r.ReadInt(out this.yStrikeoutPosition);
                            r.ReadInt(out this.sFamilyClass);
                            this.panose = ReadPanose(r);
                            r.ReadInt(out this.ulUnicodeRange1);
                            r.ReadInt(out this.ulUnicodeRange2);
                            r.ReadInt(out this.ulUnicodeRange3);
                            r.ReadInt(out this.ulUnicodeRange4);
                            this.achVendID = r.ReadString(4);
                            r.ReadInt(out this.fsSelection);
                            r.ReadInt(out this.usFirstCharIndex);
                            r.ReadInt(out this.usLastCharIndex);
                            r.ReadInt(out this.sTypoAscender);
                            r.ReadInt(out this.sTypoDescender);
                            r.ReadInt(out this.sTypoLineGap);
                            r.ReadInt(out this.usWinAscent);
                            r.ReadInt(out this.usWinDescent);
                            r.ReadInt(out this.ulCodePageRange1);
                            r.ReadInt(out this.ulCodePageRange2);
                            r.ReadInt(out this.sxHeight);
                            r.ReadInt(out this.sCapHeight);
                            r.ReadInt(out this.usDefaultChar);
                            r.ReadInt(out this.usBreakChar);
                            r.ReadInt(out this.usMaxContext);
                            r.ReadInt(out this.usLowerOpticalPointSize);
                            r.ReadInt(out this.usUpperOpticalPointSize);
                        }
                    }
                }
            }
        }
    }
}
