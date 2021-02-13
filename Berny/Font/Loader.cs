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

using System.Collections.Generic;
using UnityEngine;

// A note on font loading for the Berny library:
//
// Everything in this specific class is going to be as public and un-abstract as 
// possible. We're loading an old low-level format so there's no reason for the
// immediate loader to protect or abstract anything.

namespace PxPre.Berny.TTF
{
    /// <summary>
    /// TTF Font loader.
    /// Loads, caches and processes information needed for turning a TrueType font into 
    /// a vector shape.
    /// </summary>
    public class Loader
    {
        /// <summary>
        /// A table directory entry.
        /// 
        /// The OpenType font starts with the table directory, which is a directory of the top-level 
        /// tables in a font. If the font file contains only one font, the table directory will begin 
        /// at byte 0 of the file. If the font file is an OpenType Font Collection file (see below), 
        /// the beginning point of the table directory for each font is indicated in the TTCHeader.
        /// </summary>
        public struct Table
        { 
            /// <summary>
            /// The 4 byte identifier.
            /// </summary>
            public string tag;

            /// <summary>
            /// The error detection checksum.
            /// </summary>
            public uint checksum;

            /// <summary>
            /// The offset from the start of the file, where the actual data payload is.
            /// </summary>
            public uint offset;

            /// <summary>
            /// The size, in bytes, of the data payload.
            /// </summary>
            public uint length;

            /// <summary>
            /// Read the table from a TTFReader.
            /// </summary>
            /// <param name="r">The reader.</param>
            public void Read(TTFReader r)
            {
                this.tag = r.ReadString(4);
                r.ReadInt( out this.checksum);
                r.ReadInt(out this.offset);
                r.ReadInt(out this.length);
            }
        }

        // https://docs.microsoft.com/en-us/typography/opentype/spec/otff#organization-of-an-opentype-font
        public const int FormatTrueType = 0x00010000;
        public const int FormatOTF = 0x4F54544F;

        // Table directory
        /// <summary>
        /// 0x00010000 or 0x4F54544F ('OTTO')
        /// </summary>
        public uint sfntVersion;

        /// <summary>
        /// Number of tables.
        /// </summary>
        public ushort numTables;

        /// <summary>
        /// Maximum power of 2 less than or equal to numTables, times 16 ((2**floor(log2(numTables))) * 16, where “**” is an exponentiation operator).
        /// </summary>
        public ushort searchRange;

        /// <summary>
        /// Log2 of the maximum power of 2 less than or equal to numTables (log2(searchRange/16), which is equal to floor(log2(numTables))).
        /// </summary>
        public ushort entrySelector;

        /// <summary>
        /// numTables times 16, minus searchRange((numTables* 16) - searchRange).
        /// </summary>
        public ushort rangeShift;

        /// <summary>
        /// Dictionary of all tables parsed. Accessible by table tag name.
        /// </summary>
        /// <remarks>It contains the same information as this.records, but allows
        /// access and searching by tag name.</remarks>
        public Dictionary<string, Table> tables = new Dictionary<string, Table>();

        /// <summary>
        /// List of all tables parsed.
        /// </summary>
        /// <remarks>It contains the same information as this.tables, but keeps the
        /// order the records were encountered during parsing.</remarks>
        public List<Table> records = new List<Table>();

        /// <summary>
        /// If true, the loaded file is a TrueType format. Else, false.
        /// </summary>
        public bool IsTTF { get => this.sfntVersion == FormatTrueType; }

        /// <summary>
        /// If true, the loaded file is a OpenType format. Else, false.
        /// </summary>
        public bool IsOTF { get => this.sfntVersion == FormatOTF; }

        /// <summary>
        /// The integer value for a unit distance in the font.
        /// </summary>
        public int unitsPerEm = 0;

        /// <summary>
        /// Offset width, relevant when certain peices of data in the file.
        /// </summary>
        public int offsetByteWidth = 0;

        /// <summary>
        /// The number of glyphs in the font file.
        /// </summary>
        /// <remarks>This is not the number of glyphs we actually read from the font, 
        /// but the count value read from the font. 
        /// 
        /// These should ultimately be the same, but it's mentioned to accurately note 
        /// where this value comes from.</remarks>
        public int numGlyphs = 0;

        /// <summary>
        /// Read a TTF file.
        /// </summary>
        /// <param name="path">The file path to a TTF or OTF file.</param>
        /// <returns>The loaded font.</returns>
        public Font.Typeface ReadTTF(string path)
        {
            TTFReader r = new TTFReaderFile(path);
            return this.ReadTTF(r);
        }

        /// <summary>
        /// Read a TTF binary.
        /// </summary>
        /// <param name="data">The binary data of a TTF or OTF file.</param>
        /// <returns>The loaded font.</returns>
        public Font.Typeface ReadTTF(byte [] data)
        { 
            TTFReaderBytes r = new TTFReaderBytes(data);
            return this.ReadTTF(r);
        }

        /// <summary>
        /// Read a TTF or OTF file into a Font.Typefile object.
        /// </summary>
        /// <param name="r">A reader that's streaming from a TTF or OTF file.</param>
        /// <returns>The created Typeface.</returns>
        public Font.Typeface ReadTTF(TTFReader r)
        {
            // https://tchayen.github.io/ttf-file-parsing/
            r.SetPosition(0);
            r.ReadInt(out this.sfntVersion);

            if(this.sfntVersion != FormatTrueType && this.sfntVersion != FormatOTF)
                return null;

            r.ReadInt(out this.numTables);
            r.ReadInt(out this.searchRange);
            r.ReadInt(out this.entrySelector);
            r.ReadInt(out this.rangeShift);

            for (int i = 0; i < this.numTables; ++i)
            {
                Table t = new Table();
                t.Read(r);
                this.records.Add(t);
                this.tables.Add(t.tag, t);
            }

            Table tabEntHead;
            if(this.tables.TryGetValue(TTF.Table.head.TagName, out tabEntHead) == false)
                return null;

            r.SetPosition(tabEntHead.offset);
            TTF.Table.head tableHead = new TTF.Table.head();
            tableHead.Read(r);
            this.unitsPerEm = tableHead.unitsPerEm;
            this.offsetByteWidth = tableHead.OffsetByteWidth;

            // maxp will tell us how many glyphs there are in the file
            ////////////////////////////////////////////////////////////////////////////////
            Table tabEntMaxP;
            if(this.tables.TryGetValue(TTF.Table.maxp.TagName, out tabEntMaxP) == false)
                return null;

            r.SetPosition(tabEntMaxP.offset);
            TTF.Table.maxp tableMaxP = new TTF.Table.maxp();
            tableMaxP.Read(r);
            this.numGlyphs = tableMaxP.numGlyphs;

            // cmap tells us the mapping between character codes and glyph indices used 
            // throughout the font file
            ////////////////////////////////////////////////////////////////////////////////
            Table tabEntCMap;
            if(this.tables.TryGetValue(TTF.Table.cmap.TagName, out tabEntCMap) == false)
                return null;

            r.SetPosition(tabEntCMap.offset);
            TTF.Table.cmap tableCMap = new TTF.Table.cmap();
            tableCMap.Read(r, tabEntCMap.offset);
            //tableCMap.encodingRecords

            // loca knows offsets of glyphs in the glyf table
            ////////////////////////////////////////////////////////////////////////////////
            Table tabEntLoca;
            TTF.Table.loca ? tableLoca = null;
            if (this.tables.TryGetValue(TTF.Table.loca.TagName, out tabEntLoca) == true)
            {
                r.SetPosition(tabEntLoca.offset);

                // Since tableLoca is an optional,we can't set and read from it, instead we
                // need to read from outside the optional and then set it.
                TTF.Table.loca locaVal = new TTF.Table.loca();
                locaVal.Read(r, numGlyphs, this.offsetByteWidth == 4);
                tableLoca = locaVal;
            }

            // hhea will tell us how many horizontal metrics there are defined in the hmtx 
            // table(it doesn't have to be one for each character)
            ////////////////////////////////////////////////////////////////////////////////
            Table tabEntHHea;
            if (this.tables.TryGetValue(TTF.Table.hhea.TagName, out tabEntHHea) == false)
                return null;

            r.SetPosition(tabEntHHea.offset);
            TTF.Table.hhea tableHhea = new TTF.Table.hhea();
            tableHhea.Read(r);

            // hmtx contains information about leftSideBearing (which is how far each character 
            // wants to be from the previous one) and advanceWidth which is how much horizontal 
            // space it claims for itself
            ////////////////////////////////////////////////////////////////////////////////
            Table tabEntHmtx;
            if (this.tables.TryGetValue(TTF.Table.hmtx.TagName, out tabEntHmtx) == false)
                return null;

            r.SetPosition(tabEntHmtx.offset);
            TTF.Table.hmtx tableHmtx = new TTF.Table.hmtx();
            tableHmtx.Read(r, tableHhea.numberOfHMetrics, numGlyphs);

            // TODO:
            int selectedOffset = -1;
            for (int i = 0; i < tableCMap.numTables; ++i)
            {
                if (
                    tableCMap.encodingRecords[i].IsWindowsPlatform() ||
                    tableCMap.encodingRecords[i].IsUnicodePlatform())
                {
                    selectedOffset = (int)tableCMap.encodingRecords[i].subtableOffset;
                    break;
                }
            }

            Font.Typeface ret = null;

            // glyf provides xMin, yMin, xMax, yMax
            ////////////////////////////////////////////////////////////////////////////////
            Table tabEntGlyf;
            if(this.tables.TryGetValue(TTF.Table.glyf.TagName, out tabEntGlyf) == true)
            {
                // Create all typefaces in advance. This way we can reference future
                // glyphs before we load them.
                ret = new Font.Typeface();
                int glyphCount = tableLoca.Value.GetGlyphCount();
                for(int i = 0; i < glyphCount; ++i)
                {
                    Font.Glyph fontGlyph = new Font.Glyph();
                    ret.glyphs.Add(fontGlyph);
                }

                // Keeps a list of what's a composite, and we'll construct those when we're done.
                HashSet<int> composites = new HashSet<int>();
                for (int i = 0; i < glyphCount; ++i)
                {
                    uint lid = tableLoca.Value.GetGlyphOffset(tabEntGlyf, i);
                    r.SetPosition(lid);

                    Font.Glyph fontGlyph = ret.glyphs[i];

                    if(i < tableHmtx.hMetrics.Count)
                    {
                        fontGlyph.advance = (float)tableHmtx.hMetrics[i].advanceWidth / (float)tableHead.unitsPerEm;
                        fontGlyph.leftSideBearing = (float)tableHmtx.hMetrics[i].lsb / (float)tableHead.unitsPerEm;
                    }
                    else
                    {
                        int lsbIdx = i - tableHmtx.hMetrics.Count;
                        fontGlyph.advance = 0.0f;
                        fontGlyph.leftSideBearing = (float)tableHmtx.leftSideBearings[lsbIdx] / (float)tableHead.unitsPerEm;
                    }

                    uint glyphSize = tableLoca.Value.GetGlyphSize(i);
                    if (glyphSize == 0)
                    {
                        // Empty glyph?
                        // https://docs.microsoft.com/en-us/typography/opentype/spec/loca
                        // By definition, index zero points to the “missing character”, which is the 
                        // character that appears if a character is not found in the font. The missing 
                        // character is commonly represented by a blank box or a space. If the font does 
                        // not contain an outline for the missing character, then the first and second 
                        // offsets should have the same value. This also applies to any other characters 
                        // without an outline, such as the space character. If a glyph has no outline, 
                        // then loca[n] = loca [n+1]. In the particular case of the last glyph(s), loca[n] 
                        // will be equal the length of the glyph data ('glyf') table. The offsets must 
                        // be in ascending order with loca[n] <= loca[n+1].
                        ret.glyphs.Add(fontGlyph);
                        continue;
                    }
                    TTF.Table.glyf glyf = new TTF.Table.glyf();
                    glyf.Read(r);

                    if(glyf.numberOfContours >= 0)
                    {
                        // Simple
                        Font.Contour curContour = new Font.Contour();
                        fontGlyph.contours.Add(curContour);
                        int lastContourId = 0;
                        int curEnd = glyf.endPtsOfCountours[lastContourId];

                        for (int j = 0; j < glyf.xCoordinates.Count; ++j)
                        {
                            if(j > curEnd)
                            {
                                curContour = new Font.Contour();
                                fontGlyph.contours.Add(curContour);

                                ++lastContourId;
                                curEnd = glyf.endPtsOfCountours[lastContourId];
                            }


                            Font.Point pt = new Font.Point();

                            // If it's not on the curve, it's a control point for 
                            // a quadratic Bezier.
                            pt.isControl = (glyf.simpflags[j] & TTF.Table.glyf.ON_CURVE_POINT) == 0;

                            pt.position = 
                                new Vector2(
                                    ((float)glyf.xMin + (float)glyf.xCoordinates[j])/(float)tableHead.unitsPerEm,
                                    (float)glyf.yCoordinates[j]/(float)tableHead.unitsPerEm);

                            curContour.points.Add(pt);

                        }
                    }
                    else
                    {
                        fontGlyph.compositeRefs = new List<Font.Glyph.CompositeReference>();
                        composites.Add(i);

                        // Complex
                        foreach (TTF.Table.glyf.CompositeEntry ce in glyf.compositeEntries)
                        {
                            Font.Glyph.CompositeReference cref = new Font.Glyph.CompositeReference();
                            cref.xAxis = new Vector2(ce.xscale, ce.scale01);
                            cref.yAxis = new Vector2(ce.scale10, ce.yscale);
                            cref.offset = new Vector2(ce.argument1, ce.argument2) / (float)tableHead.unitsPerEm;
                            cref.glyphRef = ce.glyphIndex;

                            fontGlyph.compositeRefs.Add(cref);
                        }
                    }
                }

                // As composites are succesfully processed, they will be removed from the collection.
                while(composites.Count > 0)
                {
                    // Check if we've found anything to process, if not, we're done. Everything left
                    // is unsalvagable. This will probably never happen, just a contingency.
                    bool processed = false;

                    // We just want to find a valid glyph and process it - why we go through multiple
                    // composites when whe're already in a loop? To skip invalid ones. What's an invalid
                    // glyph? A glyph that's a composite of composites - where its dependency composite
                    // hasn't been processed yet.
                    foreach(int c in composites)
                    {
                        Font.Glyph fontGlyph = ret.glyphs[c];
                        if ( // Sanity
                            fontGlyph.compositeRefs == null || 
                            fontGlyph.compositeRefs.Count == 0 ||
                            c >= ret.glyphs.Count) 
                        {
                            continue;
                        }

                        bool invalid = false;
                        foreach(Font.Glyph.CompositeReference cr in fontGlyph.compositeRefs)
                        { 
                            if(composites.Contains(cr.glyphRef) == true)
                            { 
                                invalid = true;
                                break;
                            }
                        }
                        if(invalid == true)
                            break;

                        processed = true;
                        foreach(Font.Glyph.CompositeReference cr in fontGlyph.compositeRefs)
                        { 
                            Font.Glyph subglyf = ret.glyphs[cr.glyphRef];
                            foreach(Font.Contour subContour in subglyf.contours)
                            {
                                Font.Contour curContour = new Font.Contour();
                                fontGlyph.contours.Add(curContour);

                                foreach(Font.Point subPt in subContour.points)
                                {
                                    Font.Point pt = new Font.Point();
                                    pt.flags = subPt.flags;

                                    pt.position = 
                                        subPt.position.x * cr.xAxis + 
                                        subPt.position.y * cr.yAxis + 
                                        cr.offset;

                                    curContour.points.Add(pt);

                                    // No processing of tangents, the glyf format doesn't care about them.
                                }
                            }
                        }

                        processed = true;
                        composites.Remove(c);
                        break;

                    }
                    // If for some reason we wen't through everything without finding a composite 
                    // we could process, nothing's going to change the next go-around.
                    if(processed == false)
                        break;

                }
            }

            Table tabEntCFF;
            if (this.tables.TryGetValue(TTF.Table.CFF.TagName, out tabEntCFF) == true)
            {
                ret = new Font.Typeface(); 
                r.SetPosition(tabEntCFF.offset);

                TTF.Table.CFF cff = new TTF.Table.CFF();
                cff.Read(r);

                foreach( CFF.Type2Charstring tcs in cff.data.loadedCharstrings)
                { 
                    Font.Glyph g = tcs.ExecuteProgram();
                    g.advance += cff.data.nominalWidthX;
                    g.Scale( 1.0f / tableHead.unitsPerEm);
                    ret.glyphs.Add(g);
                }
            }

            if(ret == null)
                return null;

            Dictionary<uint, uint> characterRemap = null;
            foreach (TTF.Table.cmap.CharacterConversionMap ccm in tableCMap.EnumCharacterMaps())
            {
                characterRemap = ccm.MapCodeToIndex(r);
                break;
            }
            foreach (KeyValuePair<uint, uint> kvp in characterRemap)
            {
                int code = (int)kvp.Key;
                int idx = (int)kvp.Value;

                if (idx >= ret.glyphs.Count)
                    continue;

                if (ret.glyphLookup.ContainsKey(code) == true)
                    continue;

                ret.glyphLookup[code] = ret.glyphs[idx];
            }

            return ret;
        }

        /// <summary>
        /// Clear all containers.
        /// </summary>
        public void Clear()
        {
            this.tables.Clear();
            this.records.Clear();
        }
    }
}