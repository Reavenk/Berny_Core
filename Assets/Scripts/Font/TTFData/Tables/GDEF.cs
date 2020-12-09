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
                /// GDEF — Glyph Definition Table
                /// https://docs.microsoft.com/en-us/typography/opentype/spec/gdef
                /// 
                /// The Glyph Definition (GDEF) table provides various glyph properties used 
                /// in OpenType Layout processing.
                /// 
                /// The GDEF table contains six types of information in six independent subtables:
                ///
                /// - The GlyphClassDef table classifies the different types of glyphs in the font.
                /// - The AttachmentList table identifies all attachment points on the glyphs, which 
                ///     streamlines data access and bitmap caching.
                /// - The LigatureCaretList table contains positioning data for ligature carets, 
                ///     which the text-processing client uses on screen to select and highlight 
                ///     the individual components of a ligature glyph.
                /// - The MarkAttachClassDef table classifies mark glyphs, to help group together 
                ///     marks that are positioned similarly.
                /// - The MarkGlyphSets table allows the enumeration of an arbitrary number of glyph 
                ///     sets that can be used as an extension of the mark attachment class definition 
                ///     to allow lookups to filter mark glyphs by arbitrary sets of marks.
                /// - The ItemVariationStore table is used in variable fonts to contain variation data 
                ///     used for adjustment of values in the GDEF, GPOS or JSTF tables.
                ///     
                /// </summary>
                public struct GDEF
                {
                    public const string TagName = "GDEF";

                    public enum GlyphClassDef
                    {
                        Base = 1,
                        Ligature,
                        Mark,
                        Component
                    }

                    /// <summary>
                    /// The Attachment Point List table (AttachList) may be used to 
                    /// cache attachment point coordinates along with glyph bitmaps.
                    /// </summary>
                    public struct AttachList
                    {
                        public ushort coverageOffset;               // Offset to Coverage table - from beginning of AttachList table
                        public short glyphCount;                    // Number of glyphs with attachment points
                        public List<ushort> attachPointOffsets;     // Array of offsets to AttachPoint tables-from beginning of AttachList table-in Coverage Index order

                        public void Read(TTFReader r)
                        {
                            r.ReadInt(out this.coverageOffset);
                            r.ReadInt(out this.glyphCount);

                            this.attachPointOffsets = new List<ushort>();
                            for(int i = 0; i < this.glyphCount; ++i)
                                this.attachPointOffsets.Add(r.ReadUint16());
                        }
                    }

                    /// <summary>
                    /// An AttachPoint table consists of a count of the attachment 
                    /// points on a single glyph (PointCount) and an array of contour 
                    /// indices of those points (PointIndex), listed in increasing 
                    /// numerical order.
                    /// </summary>
                    public struct AttachPoint
                    {
                        public ushort pointCount;               // Number of attachment points on this glyph
                        public List<ushort> pointIndices;       // Array of contour point indices -in increasing numerical order

                        public void Read(TTFReader r)
                        {
                            r.ReadInt(out this.pointCount);

                            this.pointIndices = new List<ushort>();
                            for(int i = 0; i < this.pointCount; ++i)
                                this.pointIndices.Add( r.ReadUint16());
                        }
                    }

                    /// <summary>
                    /// The Ligature Caret List table (LigCaretList) defines caret 
                    /// positions for all the ligatures in a font. The table consists 
                    /// of an offset to a Coverage table that lists all the ligature 
                    /// glyphs (Coverage), a count of the defined ligatures (LigGlyphCount), 
                    /// and an array of offsets to LigGlyph tables (LigGlyph). The array 
                    /// lists the LigGlyph tables, one for each ligature in the Coverage 
                    /// table, in the same order as the Coverage Index.
                    /// </summary>
                    public struct LigCaretList
                    {
                        public ushort coverageOffset;           // Offset to Coverage table - from beginning of LigCaretList table
                        public ushort ligGlyphCount;            // Number of ligature glyphs
                        public List<ushort> ligGlyphOffsets;    // Array of offsets to LigGlyph tables, from beginning of LigCaretList table —in Coverage Index order

                        public void Read(TTFReader r)
                        {

                        }
                    }

                    /// <summary>
                    /// The first format (CaretValueFormat1) consists of a format identifier 
                    /// (CaretValueFormat), followed by a single coordinate for the caret 
                    /// position (Coordinate). The Coordinate is in design units.
                    /// </summary>
                    public struct CaretValueFormat
                    {
                        public ushort caretValueFormat;         // Format identifier: format = 1
                        public short coordinate;                // X or Y value, in design units
                        public short caretValuePointIndex;      // Contour point index on glyph
                        public ushort deviceOffset;             // Offset to Device table (non-variable font) / Variation Index table (variable font) for X or Y value-from beginning of CaretValue table

                        public void Read(TTFReader r)
                        {
                            r.ReadInt(out this.caretValueFormat);

                            if (this.caretValueFormat == 1)
                            {
                                r.ReadInt(out this.coordinate);
                            }
                            else if (this.caretValueFormat == 2)
                            {
                                r.ReadInt(out this.caretValuePointIndex);
                            }
                            else if (this.caretValueFormat == 3)
                            {
                                r.ReadInt(out this.coordinate);
                                r.ReadInt(out this.deviceOffset);
                            }
                        }
                    }

                    // 1.1
                    public ushort majorVersion;
                    public ushort minorVersion;
                    public ushort glyphClassDefOffset;
                    public ushort attachListOffset;
                    public ushort ligCaretListOffset;
                    public ushort markAttachClassDefOffset;

                    // 1.2
                    public ushort markGlyphSetsDefOffset;

                    // 1.3
                    public uint itemVarStoreOffset;

                    public void Read(TTFReader r)
                    {
                        r.ReadInt(out this.majorVersion);
                        r.ReadInt(out this.minorVersion);
                        r.ReadInt(out this.glyphClassDefOffset);
                        r.ReadInt(out this.attachListOffset);
                        r.ReadInt(out this.ligCaretListOffset);
                        r.ReadInt(out this.markAttachClassDefOffset);

                        if(minorVersion >= 2)
                            r.ReadInt(out this.markGlyphSetsDefOffset);

                        if(minorVersion == 3)
                            r.ReadInt(out this.itemVarStoreOffset);
                    }
                }
            }
        }
    }
}
