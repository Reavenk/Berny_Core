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

namespace PxPre
{
    namespace Berny
    {
        // TODO: Change filename to something specifically about TTFLoading
        namespace TTF
        {
            public class Loader
            {

                public struct Table
                { 
                    public string tag;
                    public uint checksum;
                    public uint offset;
                    public uint length;

                    public void Read(TTFReader r)
                    {
                        this.tag = r.ReadString(4);
                        r.ReadInt( out this.checksum);
                        r.ReadInt(out this.offset);
                        r.ReadInt(out this.length);
                    }
                }

                // https://docs.microsoft.com/en-us/typography/opentype/spec/otff#organization-of-an-opentype-font
                const int FormatTrueType = 0x00010000;
                const int FormatOTF = 0x4F54544F;

                // Table directory
                uint sfntVersion;       // 0x00010000 or 0x4F54544F ('OTTO') — see below.
                ushort numTables;       // Number of tables.
                ushort searchRange;     // Maximum power of 2 less than or equal to numTables, times 16 ((2**floor(log2(numTables))) * 16, where “**” is an exponentiation operator).
                ushort entrySelector;   // Log2 of the maximum power of 2 less than or equal to numTables (log2(searchRange/16), which is equal to floor(log2(numTables))).
                ushort rangeShift;      // numTables times 16, minus searchRange((numTables* 16) - searchRange).


                Dictionary<string, Table> tables = new Dictionary<string, Table>();
                List<Table> records = new List<Table>();

                public bool IsTTF { get => this.sfntVersion == FormatTrueType; }
                public bool IsOTF { get => this.sfntVersion == FormatOTF; }

                int unitsPerEm = 0;
                int offsetByteWidth = 0;
                int numGlyphs = 0;

                public Font.Typeface ReadTTF(string path)
                {
                    TTFReader r = new TTFReaderFile(path);
                    return this.ReadTTF(r);
                }

                public Font.Typeface ReadTTF(byte [] data)
                { 
                    TTFReaderBytes r = new TTFReaderBytes(data);
                    return this.ReadTTF(r);
                }

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

                    // glyf provides xMin, yMin, xMax, yMax
                    ////////////////////////////////////////////////////////////////////////////////
                    Table tabEntGlyf;
                    if(this.tables.TryGetValue(TTF.Table.glyf.TagName, out tabEntGlyf) == false)
                        return null;

                    r.SetPosition(tabEntGlyf.offset);
                    //TTF.Table.glyf tableGlyf = new TTF.Table.glyf();
                    //tableGlyf.Read(r);
                    //tableGlyf.xMin;
                    //tableGlyf.yMin;
                    //tableGlyf.xMax;
                    //tableGlyf.yMax;

                    // loca knows offsets of glyphs in the glyf table
                    ////////////////////////////////////////////////////////////////////////////////
                    Table tabEntLoca;
                    if(this.tables.TryGetValue(TTF.Table.loca.TagName, out tabEntLoca) == false)
                        return null;

                    r.SetPosition(tabEntLoca.offset);
                    TTF.Table.loca tableLoca = new TTF.Table.loca();
                    tableLoca.Read(r, numGlyphs, this.offsetByteWidth == 4);
                     
                    // hhea will tell us how many horizontal metrics there are defined in the hmtx 
                    // table(it doesn't have to be one for each character)
                    ////////////////////////////////////////////////////////////////////////////////
                    Table tabEntHHea;
                    if(this.tables.TryGetValue(TTF.Table.hhea.TagName, out tabEntHHea) == false)
                        return null;

                    r.SetPosition(tabEntHHea.offset);
                    TTF.Table.hhea tableHhea = new TTF.Table.hhea();
                    tableHhea.Read(r);

                    // hmtx contains information about leftSideBearing (which is how far each character 
                    // wants to be from the previous one) and advanceWidth which is how much horizontal 
                    // space it claims for itself
                    ////////////////////////////////////////////////////////////////////////////////
                    Table tabEntHmtx;
                    if(this.tables.TryGetValue(TTF.Table.hmtx.TagName, out tabEntHmtx) == false)
                        return null;

                    r.SetPosition(tabEntHmtx.offset);
                    TTF.Table.hmtx tableHmtx = new TTF.Table.hmtx();
                    tableHmtx.Read(r, tableHhea.numberOfHMetrics, numGlyphs);

                    // TODO:
                    int selectedOffset = -1;
                    for(int i = 0; i < tableCMap.numTables; ++i)
                    {
                        if(
                            tableCMap.encodingRecords[i].IsWindowsPlatform() || 
                            tableCMap.encodingRecords[i].IsUnicodePlatform())
                        { 
                            selectedOffset = (int)tableCMap.encodingRecords[i].subtableOffset;
                            break;
                        }
                    }

                    Font.Typeface ret = new Font.Typeface();

                    int glyphCount = tableLoca.GetGlyphCount();
                    for (int i = 0; i < glyphCount; ++i)
                    {
                        uint lid = tableLoca.GetGlyphOffset( tables["glyf"], i);
                        r.SetPosition(lid);

                        Font.Glyph fontGlyph = new Font.Glyph();
                        fontGlyph.advance = (float)tableHmtx.hMetrics[i].advanceWidth / (float)tableHead.unitsPerEm;
                        fontGlyph.leftSideBearing = (float)tableHmtx.hMetrics[i].lsb / (float)tableHead.unitsPerEm;

                        uint glyphSize = tableLoca.GetGlyphSize(i);
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
                            pt.control = (glyf.simpflags[j] & TTF.Table.glyf.ON_CURVE_POINT) == 0;

                            pt.position = 
                                new Vector2(
                                    ((float)glyf.xMin + (float)glyf.xCoordinates[j])/(float)tableHead.unitsPerEm,
                                    (float)glyf.yCoordinates[j]/(float)tableHead.unitsPerEm);

                            curContour.points.Add(pt);

                        }
                        
                        ret.glyphs.Add(fontGlyph);
                    }

                    Dictionary<uint, uint> characterRemap = null;
                    foreach (TTF.Table.cmap.CharacterConversionMap ccm in tableCMap.EnumCharacterMaps())
                    {
                        characterRemap = ccm.MapCodeToIndex(r);
                        break;
                    }
                    foreach(KeyValuePair<uint, uint> kvp in characterRemap)
                    {
                        int code = (int)kvp.Key;
                        int idx = (int)kvp.Value;

                        if(idx >= ret.glyphs.Count)
                            continue;

                        if(ret.glyphLookup.ContainsKey(code) == true)
                            continue;

                        ret.glyphLookup[code] = ret.glyphs[idx];
                    }


                    return ret;
                }

                public void Clear()
                {
                    this.tables.Clear();
                    this.records.Clear();
                }
            }
        }
    }
}