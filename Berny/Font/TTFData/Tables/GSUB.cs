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
                /// GSUB — Glyph Substitution Table
                /// https://docs.microsoft.com/en-us/typography/opentype/spec/gsub
                /// 
                /// The Glyph Substitution (GSUB) table provides data for substition of glyphs for 
                /// appropriate rendering of scripts, such as cursively-connecting forms in Arabic 
                /// script, or for advanced typographic effects, such as ligatures.
                /// </summary>
                public class GSUB
                { 
                    public enum LookupType
                    { 
                        Single = 1,             // Replace one glyph with one glyph
                        Multiple,               // Replace one glyph with more than one glyph
                        Alternate,              // Replace one glyph with one of many glyphs
                        Ligature,               // Replace multiple glyphs with one glyph
                        Context,                // Replace one or more glyphs in context
                        Chaining,               // Replace one or more glyphs in chained context
                        Extension,              // Extension mechanism for other substitutions (i.e. this excludes the Extension type substitution itself)
                        ReverseChaining,        // Applied in reverse order, replace single glyph in chaining context
                        Reserve_Base            // For future use (set to zero)
                    }

                    /// <summary>
                    /// Format 1 calculates the indices of the output glyphs, which are not explicitly 
                    /// defined in the subtable. To calculate an output glyph index, Format 1 adds a 
                    /// constant delta value to the input glyph index. For the substitutions to occur 
                    /// properly, the glyph indices in the input and output ranges must be in the same 
                    /// order. This format does not use the Coverage index that is returned from the 
                    /// Coverage table.
                    ///
                    /// The SingleSubstFormat1 subtable begins with a format identifier(substFormat) of 
                    /// 1. An offset references a Coverage table that specifies the indices of the input 
                    /// glyphs.The deltaGlyphID is a constant value added to each input glyph index to 
                    /// calculate the index of the corresponding output glyph. Addition of deltaGlyphID
                    /// is modulo 65536.
                    /// </summary>
                    public struct SingleSubstFormat1
                    {
                        public ushort substFormat;          // Format identifier: format = 1
                        public ushort coverageOffset;       // Offset to Coverage table, from beginning of substitution subtable
                        public short deltaGlyphID;          // Add to original glyph ID to get substitute glyph ID

                        public void Read(TTFReader r, bool readFormat)
                        {
                            if(readFormat == true)
                                r.ReadInt(out this.substFormat);

                            r.ReadInt(out this.coverageOffset);
                            r.ReadInt(out this.deltaGlyphID);
                        }
                    }

                    /// <summary>
                    /// Format 2 is more flexible than Format 1, but requires more space. It provides 
                    /// an array of output glyph indices (substituteGlyphIDs) explicitly matched to 
                    /// the input glyph indices specified in the Coverage table.
                    /// </summary>
                    public struct SingleSubstFormat2
                    { 
                        public ushort substFormat;          // Format identifier: format = 2
                        public ushort coverageOffset;       // Offset to Coverage table, from beginning of substitution subtable
                        public ushort glyphCount;           // Number of glyph IDs in the substituteGlyphIDs array
                        public List<ushort> substituteGlyphIDs; // Array of substitute glyph IDs — ordered by Coverage index

                        public void Read(TTFReader r, bool readFormat)
                        {
                            if(readFormat == true)
                                r.ReadInt(out this.substFormat);

                            r.ReadInt(out this.coverageOffset);
                            r.ReadInt(out this.glyphCount);

                            this.substituteGlyphIDs = new List<ushort>();
                            for (int i = 0; i < this.glyphCount; ++i)
                                this.substituteGlyphIDs.Add(r.ReadUint16());
                        }
                    }

                    /// <summary>
                    /// The Multiple Substitution Format 1 subtable specifies a format identifier 
                    /// (substFormat), an offset to a Coverage table that defines the input glyph 
                    /// indices, a count of offsets in the sequenceOffsets array (sequenceCount), 
                    /// and an array of offsets to Sequence tables that define the output glyph 
                    /// indices (sequenceOffsets). The Sequence table offsets are ordered by the 
                    /// Coverage index of the input glyphs.
                    /// </summary>
                    public struct MultipleSubstFormat1
                    {
                        public ushort substFormat;
                        public ushort coverageOffset;
                        public ushort sequenceCount;
                        public List<ushort> sequenceOffsets;

                        public void Read(TTFReader r, bool readFormat)
                        {
                            if(readFormat == true)
                                r.ReadInt(out this.substFormat);

                            r.ReadInt(out this.coverageOffset);
                            r.ReadInt(out this.sequenceCount);

                            this.sequenceOffsets = new List<ushort>();
                            for(int i = 0; i < this.sequenceCount; ++i)
                                this.sequenceOffsets.Add( r.ReadUint16());

                        }
                    }

                    public struct Sequence
                    { 
                        public ushort glyphCount;               // Number of glyph IDs in the substituteGlyphIDs array. This must always be greater than 0.
                        public List<ushort> substituteGlyphIDs; // String of glyph IDs to substitute

                        public void Read(TTFReader r)
                        { 
                            r.ReadInt(out this.glyphCount);

                            this.substituteGlyphIDs = new List<ushort>();
                            for(int i = 0; i < this.glyphCount; ++i)
                                this.substituteGlyphIDs.Add(r.ReadUint16());
                        }
                    }

                    /// <summary>
                    /// The Alternate Substitution Format 1 subtable contains a format identifier 
                    /// (substFormat), an offset to a Coverage table containing the indices of glyphs 
                    /// with alternative forms (coverageOffset), a count of offsets to AlternateSet 
                    /// tables (alternateSetCount), and an array of offsets to AlternateSet tables 
                    /// (alternateSetOffsets).
                    /// </summary>
                    public struct AlternateSubstFormat1
                    { 
                        public ushort substrFormat;
                        public ushort coverageOffset;
                        public ushort alternateSetCount;
                        public List<ushort> alternateSetOffsets;

                        public void Read(TTFReader r, bool readFormat)
                        {
                            if(readFormat == true)
                                r.ReadInt(out this.substrFormat);

                            r.ReadInt(out this.coverageOffset);
                            r.ReadInt(out this.alternateSetCount);

                            this.alternateSetOffsets = new List<ushort>();
                            for(int i = 0; i < this.alternateSetCount; ++i)
                                this.alternateSetOffsets.Add( r.ReadUint16());
                        }
                    }

                    public struct AlternateSet
                    { 
                        public ushort glyphCount;               // Number of glyph IDs in the alternateGlyphIDs array
                        public List<ushort> alternateGlyphIDs;  // Array of alternate glyph IDs, in arbitrary order

                        public void Read(TTFReader r)
                        {
                            r.ReadInt(out this.glyphCount);
                            this.alternateGlyphIDs = new List<ushort>();
                            for(int i = 0; i < this.glyphCount; ++i)
                                this.alternateGlyphIDs.Add(r.ReadUint16());
                        }
                    }

                    /// <summary>
                    /// It contains a format identifier (substFormat), a Coverage table offset 
                    /// (coverageOffset), a count of the ligature sets defined in this table 
                    /// (ligatureSetCount), and an array of offsets to LigatureSet tables 
                    /// (ligatureSetOffsets). The Coverage table specifies only the index of 
                    /// the first glyph component of each ligature set.
                    /// </summary>
                    public struct LigatureSubstFormat1
                    { 
                        public ushort substFormat;              // Format identifier: format = 1
                        public ushort coverageOffset;           // Offset to Coverage table, from beginning of substitution subtable
                        public ushort ligatureSetCount;         // Number of LigatureSet tables
                        public List<ushort> ligatureSetOffset;  // Array of offsets to LigatureSet tables. Offsets are from beginning of substitution subtable, ordered by Coverage index

                        public void Read(TTFReader r, bool readFormat)
                        {
                            if(readFormat == true)
                                r.ReadInt(out this.substFormat);

                            r.ReadInt(out this.coverageOffset);
                            r.ReadInt(out this.ligatureSetCount);

                            this.ligatureSetCount = new ushort();
                            for(int i = 0; i < this.ligatureSetCount; ++i)
                                this.ligatureSetOffset.Add( r.ReadUint16());
                        }
                    }

                    /// <summary>
                    /// A LigatureSet table, one for each covered glyph, specifies all the ligature strings 
                    /// that begin with the covered glyph. For example, if the Coverage table lists the glyph 
                    /// index for a lowercase “f,” then a LigatureSet table will define the “ffl,” “fl,” “ffi,” 
                    /// “fi,” and “ff” ligatures. If the Coverage table also lists the glyph index for a 
                    /// lowercase “e,” then a different LigatureSet table will define the “etc” ligature.
                    ///
                    /// A LigatureSet table consists of a count of the ligatures that begin with the covered 
                    /// glyph(ligatureCount) and an array of offsets(ligatureSetOffsets) to Ligature tables, 
                    /// which define the glyphs in each ligature.The order in the Ligature offset array defines 
                    /// the preference for using the ligatures.For example, if the “ffl” ligature is preferable 
                    /// to the “ff” ligature, then the Ligature array would list the offset to the “ffl” Ligature 
                    /// table before the offset to the “ff” Ligature table.
                    /// </summary>
                    public struct LigatureSet
                    { 
                        public ushort ligatureCount;            // Number of Ligature tables
                        public List<ushort> ligatureOffsets;    // Array of offsets to Ligature tables. Offsets are from beginning of LigatureSet table, ordered by preference.

                        public void Read(TTFReader r)
                        { 
                            r.ReadInt(out this.ligatureCount);

                            this.ligatureOffsets = new List<ushort>();
                            for(int i = 0; i < this.ligatureCount; ++i)
                                this.ligatureOffsets.Add(r.ReadUint16());
                        }
                    }

                    /// <summary>
                    /// For each ligature in the set, a Ligature table specifies the glyph ID of the output 
                    /// ligature glyph (ligatureGlyph); a count of the total number of component glyphs in 
                    /// the ligature, including the first component (componentCount); and an array of glyph 
                    /// IDs for the components (componentGlyphIDs). The array starts with the second 
                    /// component glyph in the ligature (glyph sequence index = 1, componentGlyphIDs array 
                    /// index = 0) because the first component glyph is specified in the Coverage table.
                    /// </summary>
                    public struct Ligature
                    { 
                        public ushort ligatureGlyph;            // glyph ID of ligature to substitute
                        public ushort componentCount;           // Number of components in the ligature
                        public List<ushort> componentGlyphIDs;  // Array of component glyph IDs — start with the second component, ordered in writing direction

                        public void Read(TTFReader r)
                        {
                            r.ReadInt(out this.ligatureGlyph);
                            r.ReadInt(out this.componentCount);

                            this.componentGlyphIDs = new List<ushort>();
                            for(int i = 0; i < this.componentCount; ++i)
                                this.componentGlyphIDs.Add(r.ReadUint16());
                        }
                    }

                    public struct ExtensionSubstFormat1
                    {
                        public ushort substFormat;              //Format identifier. Set to 1.
                        public ushort extensionLookupType;      // Lookup type of subtable referenced by extensionOffset (that is, the extension subtable).
                        public uint extensionOffset;            // Offset to the extension subtable, of lookup type extensionLookupType, relative to the start of the ExtensionSubstFormat1 subtable.

                        public void Read(TTFReader r, bool readFormat)
                        { 
                            if(readFormat == true)
                                r.ReadInt(out this.substFormat);

                            r.ReadInt(out this.extensionLookupType);
                            r.ReadInt(out this.extensionOffset);
                        }
                    }

                    /// <summary>
                    /// Format 1 defines a chaining context rule as a sequence of Coverage tables. Each position in the sequence may define a different Coverage table for the set of glyphs that matches the context pattern. With Format 1, the glyph sets defined in the different Coverage tables may intersect.
                    /// </summary>
                    public struct ReverseChainSingleSubstFormat1
                    { 
                        public ushort substFormat;
                        public ushort coverageOffset;
                        public ushort backtrackGlyphCount;
                        public List<ushort> backtrackCoverageOffsets;
                        public ushort lookaheadGlyphCount;
                        public ushort glyphCount;
                        public List<ushort> substituteGlyphIDs;

                        public void Read(TTFReader r, bool readFormat)
                        { 
                            if(readFormat == true)
                                r.ReadInt(out this.substFormat);

                            r.ReadInt(out this.coverageOffset);
                            r.ReadInt(out this.backtrackGlyphCount);

                            this.backtrackCoverageOffsets = new List<ushort>();
                            for(int i = 0; i < this.backtrackGlyphCount; ++i)
                                this.backtrackCoverageOffsets.Add(r.ReadUint16());

                            r.ReadInt(out this.lookaheadGlyphCount);
                            r.ReadInt(out this.glyphCount);

                            this.substituteGlyphIDs = new List<ushort>();
                            for(int i = 0; i < this.glyphCount; ++i)
                                this.substituteGlyphIDs.Add(r.ReadUint16());
                        }
                    }

                    public ushort majorVersion;                 // Major version of the GSUB table
                    public ushort minorVersion;                 // Minor version of the GSUB table
                    public ushort scriptListOffset;             // Offset to ScriptList table, from beginning of GSUB table
                    public ushort featureListOffset;            // Offset to FeatureList table, from beginning of GSUB table
                    public ushort lookupListOffset;             // Offset to LookupList table, from beginning of GSUB table
                    public uint featureVariationsOffset;        // Offset to FeatureVariations table, from beginning of the GSUB table (may be NULL)

                    public void Read(TTFReader r)
                    {
                        r.ReadInt(out this.majorVersion);
                        r.ReadInt(out this.minorVersion);
                        r.ReadInt(out this.scriptListOffset);
                        r.ReadInt(out this.featureListOffset);
                        r.ReadInt(out this.lookupListOffset);

                        if(this.majorVersion == 1 && this.minorVersion == 1)
                            r.ReadInt(out this.majorVersion);
                    }
                }
            }
        }
    }
}