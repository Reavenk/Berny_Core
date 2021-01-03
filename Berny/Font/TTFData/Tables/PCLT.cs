// MIT License
// 
// Copyright (c) 2021 Pixel Precision LLC
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
                /// PCLT - PCL 5 Table
                /// https://docs.microsoft.com/en-us/typography/opentype/spec/pclt
                /// 
                /// Use of the PCLT table in OpenType fonts is strongly discouraged. Extra information on 
                /// many of these fields can be found in the HP PCL 5 Printer Language Technical Reference 
                /// Manual available from Hewlett-Packard Boise Printer Division.
                /// </summary>
                /// <remarks>There's a lot of break down in this compacted data, that's somewhat obfuscated
                /// through alignment and array indexing of things that should be discrete variables. The task
                /// of breaking down this table's data correctly is unfinished due to lack or priority.</remarks>
                public struct PCLT
                {
                    /// <summary>
                    /// The most significant 6 bits (of the style) are reserved. The 5 next most significant 
                    /// bits encode structure. The next 3 most significant bits encode appearance width. The 2 
                    /// least significant bits encode posture.
                    /// </summary>
                    public enum Structure
                    {
                        Solid                   = 0,
                        Outline                 = 1,
                        Inline                  = 2,
                        Contour                 = 3,
                        SolidShadow             = 4,
                        OutlineShadow           = 5,
                        InlineShadow            = 6,
                        ContourShadow           = 7,
                        PatternFilled_0         = 8,
                        PatternFilled_1         = 9,
                        PatternFilled_2         = 10,
                        PatternFilled_3         = 11,
                        PatternFilledShadow_0   = 12,
                        PatternFilledShadow_1   = 13,
                        PatternFilledShadow_2   = 14,
                        PatternFilledShadow_3   = 15,
                        Inverse                 = 16,
                        InverseBorder           = 17
                    }
                

                    public enum Width
                    { 
                        Normal          = 0,
                        Condensed       = 1,
                        Compressed      = 2,
                        ExtraCompressed = 3,
                        UltraCompressed = 4,
                        //Reserved      = 5,
                        Expanded        = 6,
                        ExtraExpanded   = 7
                    }

                    public enum Posture
                    { 
                        Upright         = 0,
                        Oblique         = 1,
                        AlternateItalic = 2
                    }

                    /// <summary>
                    /// The 4 most significant bits (for the type family) are font vendor codes. 
                    /// The 12 least significant bits are typeface family codes. Both are assigned 
                    /// by HP Boise Division.
                    /// </summary>
                    public enum VendorCode
                    {
                        Agfa            = 1,
                        Bitstream       = 2,
                        Linotype        = 3,
                        Monotype        = 4,
                        Adobe           = 5,
                        Repackagers     = 6,
                        UniqueVendors
                    }

                    /// <summary>
                    /// 
                    /// </summary>
                    public enum StrokeWeight
                    { 
                        UltraThin   = -7,
                        ExtraThin   = -6,
                        Thin        = -5,
                        ExtraLight  = -4,
                        Light       = -3,
                        Demilight   = -2,
                        Semilight   = -1,
                        Regular     = 0,
                        Semibold    = 1,
                        Demibold    = 2,
                        Bold        = 3,
                        ExtraBold   = 4,
                        Black       = 5,
                        ExtraBlack  = 6,
                        UltraBlack  = 7
                    }

                    public enum WidthType
                    {
                        UltraCompressed = -5,
                        ExtraCompressed = -4,
                        Compressed      = -3,
                        Condensed       = -2,
                        Normal          = 0,
                        Expanded        = 2,
                        ExtraExpanded   = 3
                    }

                    public enum SerifType
                    {
                        Square          = 0, // Sans Serif Square
                        Round           = 1, // Sans Serif Round
                        Line            = 2, //Serif Line
                        Triangle        = 3, // Serif Triangle
                        Swath           = 4, // Serif Swath
                        Block           = 5, // Serif Block
                        Bracket         = 6, // Serif Bracket
                        Rounded         = 7, // Rounded Bracket
                        Flair           = 8, // Flair Serif, Modified Sans
                        Nonconnecting   = 9, // Script Nonconnecting
                        Joining         = 10, // Script Joining
                        Calligraphic    = 11, // Script Calligraphic
                        Broken          = 12 // Script Broken Letter
                    }

                    public enum SerifCharacteristic
                    { 
                        //Reserved = 0,
                        Monoline = 1,
                        Constrasting = 2,
                        //Reserved = 3,
                    }

                    public const string TagName = "PCLT";

                    public ushort majorVersion;
                    public ushort minorVersion;
                    public uint fontNumber;
                    public ushort pitch;
                    public ushort xHeight;
                    public ushort style;
                    public ushort typeFamily;
                    public ushort capHeight;
                    public ushort symbolSet;
                    public sbyte [] typeface;
                    public sbyte [] characterComplement;
                    public sbyte [] filename;
                    public sbyte strokeWeight;
                    public sbyte widthType;
                    public byte serifStyle;
                    public byte reserved;

                    public void Read(TTFReader r)
                    {
                        r.ReadInt(out this.majorVersion);
                        r.ReadInt(out this.minorVersion);
                        r.ReadInt(out this.fontNumber);
                        r.ReadInt(out this.pitch);
                        r.ReadInt(out this.xHeight);
                        r.ReadInt(out this.style);
                        r.ReadInt(out this.typeFamily);
                        r.ReadInt(out this.capHeight);
                        r.ReadInt(out this.symbolSet);

                        this.typeface = r.ReadSBytes(16);
                        this.characterComplement = r.ReadSBytes(8);
                        this.filename = r.ReadSBytes(6);

                        r.ReadInt(out this.strokeWeight);
                        r.ReadInt(out this.widthType);
                        r.ReadInt(out this.serifStyle);

                        // Consume the reserved at the end
                        r.ReadUInt8();

                    }

                    public Structure GetStructure()
                    {
                        return (Structure)((this.style >> 6 ) & (2 * 2 * 2 * 2 * 2 - 1));
                    }

                    public Width GetWidth()
                    {
                        return (Width)((this.style >> 11) & (2 * 2 * 2 - 1));
                    }

                    public Posture GetPosture()
                    { 
                        return (Posture)(this.style >> 14);
                    }

                    public VendorCode GetVendorCode()
                    { 
                        return (VendorCode)(this.typeFamily & (2 * 2 * 2 * 2 - 1));
                    }

                    public StrokeWeight GetStrokeWeight()
                    { 
                        return (StrokeWeight)this.strokeWeight;
                    }

                    public WidthType GetWidthType()
                    { 
                        return (WidthType)this.widthType;
                    }

                    public SerifType GetSerifType()
                    { 
                        return (SerifType)(this.serifStyle >> 6);
                    }

                    public SerifCharacteristic GetSerifCharacteristic()
                    { 
                        return (SerifCharacteristic)(this.serifStyle & (2 * 2 * 2 * 2 * 2 * 2 - 1));
                    }
                }
            }
        }
    }
}