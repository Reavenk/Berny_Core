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
                /// gvar — Glyph Variations Table
                /// https://docs.microsoft.com/en-us/typography/opentype/spec/gvar
                /// 
                /// OpenType Font Variations allow a font designer to incorporate multiple faces 
                /// within a font family into a single font resource. In a variable font, the font 
                /// variations ('fvar') table defines a set of design variations supported by the 
                /// font, and then various tables provide data that specify how different font 
                /// values, such as X-height or X and Y coordinates for glyph outline points, are 
                /// adjusted for different variation instances. The glyph variations ('gvar') 
                /// table provides all of the variation data that describe how TrueType glyph 
                /// outlines in a 'glyf' table change across the font’s variation space.
                /// </summary>
                public struct gvar
                {
                    public struct TupleRecord
                    {
                        // Not sure if this implementation is correct
                        public ushort flags;
                        public short cx;
                        public short cy;
                    }
                    
                    public const string TagName = "gvar";

                    public ushort majorVersion;                     // Major version number of the glyph variations table — set to 1.
                    public ushort minorVersion;                     // Minor version number of the glyph variations table — set to 0.
                    public ushort axisCount;                        // The number of variation axes for this font. This must be the same number as axisCount in the 'fvar' table.
                    public ushort sharedTupleCount;                 // The number of shared tuple records. Shared tuple records can be referenced within glyph variation data tables for multiple glyphs, as opposed to other tuple records stored directly within a glyph variation data table.
                    public uint sharedTupleOffset;                  // Offset from the start of this table to the shared tuple records.
                    public ushort glyphCount;                       // The number of glyphs in this font. This must match the number of glyphs stored elsewhere in the font.
                    public ushort flags;                            // Bit-field that gives the format of the offset array that follows. If bit 0 is clear, the offsets are uint16; if bit 0 is set, the offsets are uint32.
                    public uint glyphVariationDataArrayOffset;      // Offset from the start of this table to the array of GlyphVariationData tables.
                    public List<uint> glyphVariationDataOffsets;    // Offsets from the start of the GlyphVariationData array to each GlyphVariationData table.

                    public List<TupleRecord> sharedTuples;
                    public GlyphVariationData glyphVariationData;

                    public void Read(TTFReader r)
                    {
                        r.ReadInt(out this.majorVersion);
                        r.ReadInt(out this.minorVersion);
                        r.ReadInt(out this.axisCount);
                        r.ReadInt(out this.sharedTupleCount);
                        r.ReadInt(out this.glyphCount);
                        r.ReadInt(out this.flags);
                        r.ReadInt(out this.glyphVariationDataArrayOffset);

                        this.glyphVariationDataOffsets = new List<uint>();
                        if ((this.flags & 0x0001) != 0)
                        {
                            for (int i = 0; i < this.glyphCount + 1; ++i)
                                this.glyphVariationDataOffsets.Add(r.ReadUInt32());
                        }
                        else
                        {
                            for (int i = 0; i < this.glyphCount + 1; ++i)
                                this.glyphVariationDataOffsets.Add((uint)r.ReadUint16());
                        }

                        // Shared tuple records
                        // This is probably not right - I cant get good documentation
                        // that I can understand for this.
                        this.sharedTuples = new List<TupleRecord>();
                        for (int i = 0; i < this.sharedTupleCount; ++i)
                        {
                            TupleRecord tr = new TupleRecord();
                            r.ReadInt(out tr.flags);
                            r.ReadInt(out tr.cx);
                            r.ReadInt(out tr.cy);
                            this.sharedTuples.Add(tr);
                        }

                        // Glyph variation data tables
                        this.glyphVariationData.Read(r, this.axisCount);
                    }

                }
            }
        }
    }
}
