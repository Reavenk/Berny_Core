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
        namespace CFF
        {
            public class CFFFile
            {
                public Header header;
                public INDEX nameIndex;
                public INDEX topDictIndex;
                public INDEX stringIndex;
                public INDEX globalSubrsIndex;
                public INDEX charStringsIndex;

                public Strings strings = new Strings();

                public int versionSID       = -1;
                public int noticeSID        = -1;
                public int copyrightSID     = -1;
                public int fullNameSID      = -1;
                public int familyNameSID    = -1;
                public int weightSID        = -1;
                public bool isFixedPitch;
                public float italicAngle;
                public float underlinePosition;
                public float underlineThickness;
                public int paintType;
                public int charstringType;
                public float [] fontMatrix = new float[]{ 0.001f, 0.0f, 0.0f, 0.001f, 0.0f, 0.0f };
                public int uniqueID;
                public int [] fontBBox = new int[]{0, 0, 0, 0};
                public float strokeWidth;
                public List<int> XUID;
                public int charset;
                public int encodingOffset;
                public int charStrings;
                public int privateSize;
                public int privateOffset;
                public int syntheticBase;
                public int postScriptSID      = -1;
                public int baseFontNameSID    = -1;
                public List<int> baseFontBlend;

                public int CID_ROS_1SID;
                public int CID_ROS_2SID;
                public int CID_ROS_3;
                public int CID_FontVersion;
                public int CID_FontRevision;
                public int CID_FontType;
                public int CID_Count;
                public int CID_UIDBase;
                public int CID_FDArray;
                public int CID_FDSelect_1;
                public int CID_FDSelect_2;
                public string CID_fontNameSID;

                // private values
                public List<int> blueValues;
                public List<int> otherBlues;
                public List<int> familyBlues;
                public List<int> familyOtherBlues;
                public float blueScale;
                public int blueShift;
                public int blueFuzz;
                public int stdHW;
                public int stdVW;
                public List<int> stemSnapH;
                public List<int> stemSnapV;
                public bool forceBold;
                public int languageGroup;
                public float expansionFactor;
                public int initialRandomSeed; // WhuuUU?
                public int subrs;
                public int defaultWidthX;
                public int nominalWidthX;

                public List<Type2Charstring> loadedCharstrings;

                static public byte ReadCard8(TTF.TTFReader r)
                {
                    return r.ReadUInt8();
                }

                // duplicate of TFF.ReadUInt16()
                static public ushort ReadCard16(TTF.TTFReader r)
                {
                    return (ushort)(r.ReadUInt8() << 8 | r.ReadUInt8());
                }

                static public byte ReadOffSize(TTF.TTFReader r)
                {
                    return r.ReadUInt8();
                }

                public uint ReadOffset(TTF.TTFReader r)
                {
                    byte c = ReadOffSize(r);
                    if (c == 1)
                        return (uint)(r.ReadUInt8() << 0);
                    else if (c == 2)
                        return (uint)((r.ReadUInt8() << 8) | (r.ReadUInt8() << 0));
                    else if (c == 3)
                        return (uint)((r.ReadUInt8() << 16) | (r.ReadUInt8() << 8) | (r.ReadUInt8() << 0));
                    else
                        return (uint)((r.ReadUInt8() << 24) | (r.ReadUInt8() << 16) | (r.ReadUInt8() << 8) | (r.ReadUInt8() << 0));
                }

                public static uint ReadOffset(TTF.TTFReader r, int offSize)
                {
                    if (offSize == 1)
                        return (uint)(r.ReadUInt8() << 0);
                    else if (offSize == 2)
                        return (uint)((r.ReadUInt8() << 8) | (r.ReadUInt8() << 0));
                    else if (offSize == 3)
                        return (uint)((r.ReadUInt8() << 16) | (r.ReadUInt8() << 8) | (r.ReadUInt8() << 0));
                    else
                        return (uint)((r.ReadUInt8() << 24) | (r.ReadUInt8() << 16) | (r.ReadUInt8() << 8) | (r.ReadUInt8() << 0));
                }

                public void Read(TTF.TTFReader r)
                {
                    long basePos = r.GetPosition();

                    this.header = new Header();
                    this.header.Read(r);

                    this.nameIndex = new INDEX();
                    this.nameIndex.Read(r);
                    string name = System.Text.ASCIIEncoding.ASCII.GetString(this.nameIndex.data);

                    this.topDictIndex = new INDEX();
                    this.topDictIndex.Read(r);
                    string topDict = System.Text.ASCIIEncoding.ASCII.GetString(this.topDictIndex.data);

                    this.SetTopDICTDefaults();

                    
                    // Kinda weird using the byte reader inside a read, but 
                    // it turns out to be convenient.
                    TTF.TTFReaderBytes trbDict = new TTF.TTFReaderBytes(this.topDictIndex.data);

                    List<Operand> dP = new List<Operand>(); // DICT Params (aka: operands)
                    while (trbDict.GetPosition() < this.topDictIndex.data.Length)
                    {
                        Operand val = Operand.Read(trbDict);
                        if(val.type == Operand.Type.Error)
                        {
                            // TODO: Error
                        }
                        else if(val.type == Operand.Type.Int || val.type == Operand.Type.Real)
                        {
                            dP.Add(val);
                            continue;
                        }

                        // If it made it down to here, the type value is an operator
                        switch(val.intVal)
                        {
                            case 0: // Version
                                Operand.LoadParsed(dP, out this.versionSID);
                                break;
                            case 1: // Notice
                                Operand.LoadParsed(dP, out this.noticeSID);
                                break;
                            case 2: // FullName
                                Operand.LoadParsed(dP, out this.fullNameSID);
                                break;
                            case 3: // FamilyName
                                Operand.LoadParsed(dP, out this.familyNameSID);
                                break;
                            case 4: // Weight
                                Operand.LoadParsed(dP, out this.weightSID);
                                break;
                            case 5: // FontBBox
                                Operand.LoadParsed(dP, this.fontBBox);
                                break;
                            case (12 << 8) | 0: // Copyright
                                Operand.LoadParsed(dP, out this.copyrightSID);
                                break;
                            case (12 << 8) | 1: // isFixedPitch
                                Operand.LoadParsed(dP, out this.isFixedPitch); //Bool
                                break;
                            case (12 << 8) | 2: // italicAngle
                                Operand.LoadParsed(dP, out this.italicAngle);
                                break;
                            case (12 << 8) | 3: // UnderlinePosition
                                Operand.LoadParsed(dP, out this.underlinePosition);
                                break;
                            case (12 << 8) | 4: // UnderlineThickness
                                Operand.LoadParsed(dP, out this.underlineThickness);
                                break;
                            case (12 << 8) | 5: // PaintType
                                Operand.LoadParsed(dP, out this.paintType);
                                break;
                            case (12 << 8) | 6: // CharstringType
                                Operand.LoadParsed(dP, out this.charstringType);
                                break;
                            case (12 << 8) | 7: // FontMatrix
                                Operand.LoadParsed(dP, this.fontMatrix);
                                break;
                            case (12 << 8) | 8: // StrokeWidth
                                Operand.LoadParsed(dP, out this.strokeWidth);
                                break;
                            case (12 << 8) | 20: // SyntheticBase
                                Operand.LoadParsed(dP, out this.syntheticBase);
                                break;
                            case (12 << 8) |21: // PosScript
                                Operand.LoadParsed(dP, out this.postScriptSID);
                                break;
                            case (12 << 8) | 22: // BaseFontName
                                Operand.LoadParsed(dP, out this.baseFontNameSID);
                                break;
                            case (12 << 8) | 23:    // BaseFontBlend
                                {
                                    this.baseFontBlend = Operand.ToIntList(dP);
                                    Operand.ConvertDeltasToAbsolute(this.baseFontBlend);
                                    dP.Clear();
                                }
                                break;
                            case (12 << 8) | 30:
                                {
                                    this.CID_ROS_1SID = dP[0].GetInt();
                                    this.CID_ROS_2SID = dP[1].GetInt();
                                    this.CID_ROS_3 = dP[2].GetInt();
                                    dP.Clear();
                                }
                                break;
                            case (12 << 8) | 31:
                                Operand.LoadParsed(dP, out this.CID_FontVersion);
                                break;
                            case (12 << 8) | 32:
                                Operand.LoadParsed(dP, out this.CID_FontRevision);
                                break;
                            case (12 << 8) | 33:
                                Operand.LoadParsed(dP, out this.CID_FontType);
                                break;
                            case (12 << 8) | 34:
                                Operand.LoadParsed(dP, out this.CID_Count);
                                break;
                            case (12 << 8) | 35:
                                Operand.LoadParsed(dP, out this.CID_UIDBase);
                                break;
                            case (12 << 8) | 36:
                                Operand.LoadParsed(dP, out this.CID_FDArray);
                                break;
                            case 13:
                                Operand.LoadParsed(dP, out this.uniqueID);
                                break;
                            case 14:
                                this.XUID = Operand.ToIntList(dP);
                                dP.Clear();
                                break;
                            case 15:
                                Operand.LoadParsed(dP, out this.charset);
                                break;
                            case 16:
                                Operand.LoadParsed(dP, out this.encodingOffset);
                                break;
                            case 17:
                                Operand.LoadParsed(dP, out this.charStrings);
                                break;
                            case 18:
                                this.privateSize = dP[0].GetInt();
                                this.privateOffset = dP[1].GetInt();
                                dP.Clear();
                                break;
                        }
                    } // End of parsing Top DICT loop

                    this.stringIndex = new INDEX();
                    this.stringIndex.Read(r);

                    this.strings.Clear();
                    for (int i = 0; i < this.stringIndex.offset.Count - 1; ++i)
                    {
                        string s =
                            System.Text.ASCIIEncoding.ASCII.GetString(
                                this.stringIndex.data,
                                (int)this.stringIndex.offset[i],
                                (int)(this.stringIndex.offset[i + 1] - this.stringIndex.offset[i]));

                        this.strings.Add(s);
                    }

                    this.globalSubrsIndex = new INDEX();
                    this.globalSubrsIndex.Read(r);

                    //// Encodings
                    //const int EncodingSupplement = (1 << 8);
                    //byte encodingFormat = r.ReadUInt8();
                    //Format0 f0;
                    //Format1 f1;
                    //if (encodingFormat == 0)
                    //{
                    //    f0 = new Format0();
                    //    f0.format = 0;
                    //    f0.nCodes = r.ReadUInt8();
                    //    f0.code = r.ReadBytes(f0.nCodes);
                    //
                    //}
                    //else if (encodingFormat == 1)
                    //{
                    //    f1 = new Format1();
                    //    f1.format = 1;
                    //    f1.nRanges = r.ReadUInt8();
                    //
                    //    f1.range1s = new List<Range1>();
                    //    for (int i = 0; i < f1.nRanges; ++i)
                    //    {
                    //        Range1 r1 = new Range1();
                    //        r1.first = r.ReadUInt8();
                    //        r1.nLeft = r.ReadUInt8();
                    //        f1.range1s.Add(r1);
                    //    }
                    //}

                    // Load privates
                    if (this.privateOffset != 0)
                    {
                        this.SetPrivateDICTDefaults();
                        dP.Clear();
                        
                        r.SetPosition(basePos + this.privateOffset);
                        long privateEndPos = basePos + this.privateOffset + this.privateSize;
                        while (r.GetPosition() < privateEndPos)
                        {
                            Operand val = Operand.Read(r);
                            if (val.type == Operand.Type.Error)
                            {
                                // TODO: Error
                            }
                            else if (val.type == Operand.Type.Int || val.type == Operand.Type.Real)
                            {
                                dP.Add(val);
                                continue;
                            }

                            switch(val.intVal)
                            { 
                                case 6:         // BlueValues
                                    this.blueValues = Operand.ToIntList(dP);
                                    Operand.ConvertDeltasToAbsolute(this.blueValues);
                                    dP.Clear();
                                    break;
                                case 7:         // OtherBlues
                                    this.otherBlues = Operand.ToIntList(dP);
                                    Operand.ConvertDeltasToAbsolute(this.otherBlues);
                                    dP.Clear();
                                    break;
                                case 8:         // FamilyBlues
                                    this.familyBlues = Operand.ToIntList(dP);
                                    Operand.ConvertDeltasToAbsolute(this.familyBlues);
                                    dP.Clear();
                                    break;
                                case 9:         // FamilyOtherBlues
                                    this.familyOtherBlues = Operand.ToIntList(dP);
                                    Operand.ConvertDeltasToAbsolute(this.familyOtherBlues);
                                    dP.Clear();
                                    break;
                                case 10:        // StdHW
                                    Operand.LoadParsed(dP, out this.stdHW);
                                    break;
                                case 11:        // StdVW
                                    Operand.LoadParsed(dP, out this.stdVW);
                                    break;
                                case (12 << 8) | 9: // BlueScale
                                    Operand.LoadParsed(dP, out this.blueScale);
                                    break;
                                case (12 << 8) | 10:    // BlueShift
                                    Operand.LoadParsed(dP, out this.blueShift);
                                    break;
                                case (12 << 8) | 11:    // BlueFuzz
                                    Operand.LoadParsed(dP, out this.blueFuzz);
                                    break;
                                case (12 << 8) | 12:    // StemSnapH
                                    this.stemSnapH = Operand.ToIntList(dP);
                                    Operand.ConvertDeltasToAbsolute(this.stemSnapH);
                                    dP.Clear();
                                    break;
                                case (12 << 8) | 13:    // StemSnapV
                                    this.stemSnapV = Operand.ToIntList(dP);
                                    Operand.ConvertDeltasToAbsolute(this.stemSnapV);
                                    dP.Clear();
                                    break;
                                case (12 << 8) | 14:    // ForceBold
                                    Operand.LoadParsed(dP, out this.forceBold);
                                    break;
                                case (12 << 8) | 17:    // LanguageGroup
                                    Operand.LoadParsed(dP, out this.languageGroup);
                                    break;
                                case (12 << 8) | 18:    // ExpansionFactor
                                    Operand.LoadParsed(dP, out this.expansionFactor);
                                    break;
                                case (12 << 8) | 19:    // initialRandomSeed
                                    Operand.LoadParsed(dP, out this.initialRandomSeed);
                                    break;
                                case 19:                // Subrs
                                    Operand.LoadParsed(dP, out this.subrs);
                                    break;
                                case 20:                // defaultWidthX
                                    Operand.LoadParsed(dP, out this.defaultWidthX);
                                    break;
                                case 21:                // nominalWidthX
                                    Operand.LoadParsed(dP, out this.nominalWidthX);
                                    break;
                            }
                        }
                    }

                    // Charsets
                    //r.SetPosition(basePos + this.charStrings);
                    //byte charsetFormat = r.ReadUInt8();
                    //
                    //CharsetFormat0 cf0;
                    //CharsetFormat1 cf1;
                    //CharsetFormat2 cf2;
                    //
                    //if(charsetFormat == 0)
                    //{ 
                    //    cf0 = new CharsetFormat0();
                    //    cf0.format = 0;
                    //    cf0.glyph = new List<int>();
                    //    for(int i = 0; i < 3; ++i)
                    //    { 
                    //        cf0.glyph.Add(ReadDictValueInt(r));
                    //    }
                    //}
                    //else if(charsetFormat == 1)
                    //{ 
                    //    cf1 = new CharsetFormat1();
                    //    cf1.format = 1;
                    //}
                    //else if(charsetFormat == 2)
                    //{ 
                    //    cf2 = new CharsetFormat2();
                    //    cf2.format = 2;
                    //}

                    this.loadedCharstrings = new List<Type2Charstring>();
                    r.SetPosition(basePos + this.charStrings);
                    this.charStringsIndex.Read(r);

                    for(int i = 0; i < this.charStringsIndex.offset.Count - 1; ++i)
                    { 
                        int offset = (int)this.charStringsIndex.offset[i];
                        int size = (int)(this.charStringsIndex.offset[i + 1] - this.charStringsIndex.offset[i]);

                        byte [] rb = new byte[size];
                        System.Buffer.BlockCopy(this.charStringsIndex.data, offset, rb, 0, size);

                        Type2Charstring t2c = new Type2Charstring(rb);
                        loadedCharstrings.Add(t2c);
                    }

                    //FSSelect (CID Fonts only)

                    // CharStrings Index (per font)

                    // Font DICT Index

                    // Private DICT

                    // Local Subr INDEX

                    // Copyright and trademark notices

                }

                public void SetTopDICTDefaults()
                {
                    this.isFixedPitch = false;
                    this.italicAngle = 0;
                    this.underlinePosition = -100;
                    this.underlineThickness = 50;
                    this.paintType = 0;
                    this.charstringType = 2;
                    this.fontMatrix = new float [] { 0.001f, 0.0f, 0.0f, 0.001f, 0.0f, 0.0f };
                    this.fontBBox = new int []{ 0, 0, 0, 0 };
                    this.strokeWidth = 0;
                    this.charset = 0;
                    this.CID_FontVersion = 0;
                    this.CID_FontRevision = 0;
                    this.CID_FontType = 0;
                    this.CID_Count = 8720;
                }

                public void SetPrivateDICTDefaults()
                { 
                    this.blueScale = 0.039625f;
                    this.blueShift = 7;
                    this.blueFuzz = 1;
                    this.forceBold = false;
                    this.expansionFactor = 0.6f;
                    this.initialRandomSeed = 0;
                    this.defaultWidthX = 0;
                    this.nominalWidthX = 0;
                }

                public string GetString(int sid)
                { 
                    return this.strings.GetString(sid);
                }

                public string GetVersionID()
                { 
                    return this.strings.GetString(this.versionSID);
                }

                public string GetNoticeID()
                {
                    return this.strings.GetString(this.noticeSID);
                }

                public string GetCopyrightID()
                {
                    return this.strings.GetString(this.copyrightSID);
                }

                public string GetFullName()
                {
                    return this.strings.GetString(this.fullNameSID);
                }

                public string GetFamilyName()
                {
                    return this.strings.GetString(this.familyNameSID);
                }

                public string GetWeight()
                { 
                    return this.strings.GetString(this.weightSID);
                }
            }
        }
    }
}