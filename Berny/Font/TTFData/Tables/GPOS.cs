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
                /// GPOS — Glyph Positioning Table
                /// https://docs.microsoft.com/en-us/typography/opentype/spec/gpos
                /// 
                /// The Glyph Positioning table (GPOS) provides precise control over 
                /// glyph placement for sophisticated text layout and rendering in each 
                /// script and language system that a font supports.
                /// </summary>
                public struct GPOS
                {

                    public struct SinglePosFormat1
                    { 
                        public ushort posFormat;            // Format identifier: format = 1
                        public ushort coverageOffset;       // Offset to Coverage table, from beginning of SinglePos subtable.
                        public ushort valueFormat;          // Defines the types of data in the ValueRecord.
                        public ValueRecord valueRecord;     // Defines positioning value(s) — applied to all glyphs in the Coverage table.

                        public void Read(TTFReader r, bool readFormat)
                        {
                            if(readFormat == true)
                                r.ReadInt(out this.posFormat);

                            r.ReadInt(out this.coverageOffset);
                            r.ReadInt(out this.valueFormat);

                            this.valueRecord = new ValueRecord();
                            this.valueRecord.Read(r);
                        }
                    }

                    public struct SinglePosFormat2
                    { 
                        public ushort posFormat;            // Format identifier: format = 2
                        public ushort coverageOffset;       // Offset to Coverage table, from beginning of SinglePos subtable.
                        public ushort valueFormat;          // Defines the types of data in the ValueRecords.
                        public ushort valueCount;           // Number of ValueRecords — must equal glyphCount in the Coverage table.
                        List<ValueRecord> valueRecords;     // Array of ValueRecords — positioning values applied to glyphs.

                        public void Read(TTFReader r, bool readFormat)
                        {
                            if(readFormat == true)
                                r.ReadInt(out this.posFormat);

                            r.ReadInt(out this.coverageOffset);
                            r.ReadInt(out this.valueFormat);
                            r.ReadInt(out this.valueCount);

                            this.valueRecords = new List<ValueRecord>();
                            for(int i = 0; i < this.valueCount; ++i)
                            { 
                                ValueRecord vr = new ValueRecord();
                                vr.Read(r);
                                this.valueRecords.Add(vr);
                            }
                        }
                    }

                    /// <summary>
                    /// An array of offsets to PairSet tables (pairSetOffsets) and a count 
                    /// of the defined tables (pairSetCount). The PairSet array contains 
                    /// one offset for each glyph listed in the Coverage table and uses 
                    /// the same order as the Coverage Index.
                    /// </summary>
                    public struct PairPosFormat1
                    { 
                        public ushort posFormat;            // Format identifier: format = 1
                        public ushort coverageOffset;       // Offset to Coverage table, from beginning of PairPos subtable.
                        public ushort valueFormat1;         // Defines the types of data in valueRecord1 — for the first glyph in the pair (may be zero).
                        public ushort valueFormat2;         // Defines the types of data in valueRecord2 — for the second glyph in the pair (may be zero).
                        public ushort pairSetCount;         // Number of PairSet tables
                        public List<ushort> pairSetOffsets; // Array of offsets to PairSet tables. Offsets are from beginning of PairPos subtable, ordered by Coverage Index.

                        public void Read(TTFReader r, bool readFormat)
                        {
                            if(readFormat == true)
                                r.ReadInt(out this.posFormat);

                            r.ReadInt(out this.coverageOffset);
                            r.ReadInt(out this.valueFormat1);
                            r.ReadInt(out this.valueFormat2);
                            r.ReadInt(out this.pairSetCount);

                            this.pairSetOffsets = new List<ushort>();
                            for(int i = 0; i < this.pairSetCount; ++i)
                                this.pairSetOffsets.Add(r.ReadUInt16());
                        }
                    }

                    public struct PairSet
                    {
                        public ushort pairValueCount;
                        public List<PairValueRecord> pairValueRecords;

                        public void Read(TTFReader r)
                        {
                            r.ReadInt(out this.pairValueCount);

                            this.pairValueRecords = new List<PairValueRecord>();
                            for(int i = 0; i < this.pairValueCount; ++i)
                            { 
                                PairValueRecord pvr = new PairValueRecord();
                                pvr.Read(r);
                                this.pairValueRecords.Add(pvr);
                            }
                        }
                    }

                    /// <summary>
                    /// A PairValueRecord specifies the second glyph in a pair (secondGlyph) and 
                    /// defines a ValueRecord for each glyph (valueRecord1 and valueRecord2). 
                    /// If valueFormat1 in the PairPos subtable is set to zero (0), valueRecord1 
                    /// will be empty; similarly, if valueFormat2 is 0, valueRecord2 will be empty.
                    /// </summary>
                    public struct PairValueRecord
                    { 
                        public ushort secondGlyph;          // Glyph ID of second glyph in the pair (first glyph is listed in the Coverage table).
                        public ValueRecord valueRecord1;    // Positioning data for the first glyph in the pair.
                        public ValueRecord valueRecord2;    // Positioning data for the second glyph in the pair.

                        public void Read(TTFReader r)
                        {
                            r.ReadInt(out this.secondGlyph);

                            this.valueRecord1 = new ValueRecord();
                            this.valueRecord1.Read(r);

                            this.valueRecord2 = new ValueRecord();
                            this.valueRecord2.Read(r);
                        }
                    }

                    public struct PairPosFormat2
                    {
                        public ushort posFormat;
                        public ushort coverageOffset;
                        public ushort valueFormat1;
                        public ushort valueFormat2;
                        public ushort classDef1Offset;
                        public ushort classDef2Offset;
                        public ushort class1Count;
                        public ushort class2Count;

                        public List<Class1Record> class1Record;

                        public void Read(TTFReader r, bool readFormat)
                        {
                            if(readFormat == true)
                                r.ReadInt(out this.posFormat);

                            r.ReadInt(out this.coverageOffset);
                            r.ReadInt(out this.valueFormat1);
                            r.ReadInt(out this.valueFormat2);
                            r.ReadInt(out this.classDef1Offset);
                            r.ReadInt(out this.classDef2Offset);
                            r.ReadInt(out this.class1Count);
                            r.ReadInt(out this.class2Count);

                            this.class1Record = new List<Class1Record>();
                            for(int i = 0; i < this.class1Count; ++i)
                            { 
                                Class1Record c1r = new Class1Record();
                                c1r.Read(r, this.class2Count);
                                this.class1Record.Add(c1r);
                            }
                        }
                    }

                    /// <summary>
                    /// Each Class1Record contains an array of Class2Records (class2Records), 
                    /// which also are ordered by class value. One Class2Record must be declared 
                    /// for each class in the classDef2 table, including Class 0.
                    /// </summary>
                    public struct Class1Record
                    { 
                        List<Class2Record> class2Record;    // Array of Class2 records, ordered by classes in classDef2.

                        public void Read(TTFReader r, int class2Count)
                        {
                            this.class2Record = new List<Class2Record>();
                            for(int i = 0; i < class2Count; ++i)
                            { 
                                Class2Record c2r = new Class2Record();
                                c2r.Read(r);
                                this.class2Record.Add(c2r);
                            }
                        }
                    }

                    /// <summary>
                    /// A Class2Record consists of two ValueRecords, one for the first glyph in a 
                    /// class pair (valueRecord1) and one for the second glyph (valueRecord2). Note 
                    /// that both fields of a Class2Record are optional: If the PairPos subtable has 
                    /// a value of zero (0) for valueFormat1 or valueFormat2, then the corresponding 
                    /// record (valueRecord1 or valueRecord2) will be empty — that is, not present. 
                    /// For example, if valueFormat1 is zero, then the Class2Record will begin with 
                    /// and consist solely of valueRecord2. The text-processing client must be aware 
                    /// of the variable nature of the Class2Record and use the valueFormat1 and 
                    /// valueFormat2 fields to determine the size and content of the Class2Record.
                    /// </summary>
                    public struct Class2Record
                    { 
                        public ValueRecord valueRecord1;        // Positioning for first glyph — empty if valueFormat1 = 0.
                        public ValueRecord valueRecord2;        // Positioning for second glyph — empty if valueFormat2 = 0.

                        public void Read(TTFReader r)
                        {
                            this.valueRecord1 = new ValueRecord();
                            this.valueRecord1.Read(r);

                            this.valueRecord2 = new ValueRecord();
                            this.valueRecord2.Read(r);
                        }
                    }

                    /// <summary>
                    /// The CursivePosFormat1 subtable begins with a format identifier (posFormat) and 
                    /// an offset to a Coverage table (coverageOffset), which lists all the glyphs 
                    /// that define cursive attachment data. In addition, the subtable contains one 
                    /// EntryExitRecord for each glyph listed in the Coverage table, a count of those 
                    /// records (entryExitCount), and an array of those records in the same order as 
                    /// the Coverage Index (entryExitRecords).
                    /// </summary>
                    public struct CursivePosFormat1
                    { 
                        public ushort posFormat;                // Format identifier: format = 1
                        public ushort coverageOffset;           // Offset to Coverage table, from beginning of CursivePos subtable.
                        public ushort entryExitCount;           // Number of EntryExit records
                        List<EntryExitRecord> entryExitRecord;  // Array of EntryExit records, in Coverage index order.

                        public void Read(TTFReader r, bool readFormat)
                        {
                            if(readFormat == true)
                                r.ReadInt(out this.posFormat);

                            r.ReadInt(out this.coverageOffset);
                            r.ReadInt(out this.entryExitCount);

                            this.entryExitRecord = new List<EntryExitRecord>();
                            for(int i = 0; i < this.entryExitCount; ++i)
                            {
                                EntryExitRecord eer = new EntryExitRecord();
                                eer.Read(r);
                                this.entryExitRecord.Add(eer);
                            }
                        }
                    }

                    /// <summary>
                    /// Each EntryExitRecord consists of two offsets: one to an Anchor table that 
                    /// identifies the entry point on the glyph (entryAnchorOffset), and an offset 
                    /// to an Anchor table that identifies the exit point on the glyph (exitAnchorOffset). 
                    /// (For a complete description of the Anchor table, see the end of this chapter.)
                    /// </summary>
                    public struct EntryExitRecord
                    { 
                        public ushort entryAnchorOffset;        // Offset to entryAnchor table, from beginning of CursivePos subtable (may be NULL).
                        public ushort exitAnchorOffset;         // Offset to exitAnchor table, from beginning of CursivePos subtable (may be NULL).

                        public void Read(TTFReader r)
                        {
                            r.ReadInt(out this.entryAnchorOffset);
                            r.ReadInt(out this.exitAnchorOffset);
                        }
                    }

                    /// <summary>
                    /// The MarkBasePosFormat1 subtable also contains an offset to a BaseArray table 
                    /// (baseArrayOffset), which defines for each base glyph an array of anchors, 
                    /// one for each mark class.
                    /// </summary>
                    public struct MarkBasePosFormat1
                    { 
                        public ushort posFormat;                // Format identifier: format = 1
                        public ushort markCoverageOffset;       // Offset to markCoverage table, from beginning of MarkBasePos subtable.
                        public ushort baseCoverageOffset;       // Offset to baseCoverage table, from beginning of MarkBasePos subtable.
                        public ushort markClassCount;           // Number of classes defined for marks
                        public ushort markArrayOffset;          // Offset to MarkArray table, from beginning of MarkBasePos subtable.
                        public ushort baseArrayOffet;           // Offset to BaseArray table, from beginning of MarkBasePos subtable.

                        public void Read(TTFReader r, bool readFormat)
                        {
                            if(readFormat == true)
                                r.ReadInt(out this.posFormat);

                            r.ReadInt(out this.markCoverageOffset);
                            r.ReadInt(out this.baseCoverageOffset);
                            r.ReadInt(out this.markClassCount);
                            r.ReadInt(out this.markArrayOffset);
                            r.ReadInt(out this.baseArrayOffet);
                        }
                    }

                    /// <summary>
                    /// The BaseArray table consists of an array (baseRecords) and count (baseCount) 
                    /// of BaseRecords. The array stores the BaseRecords in the same order as the 
                    /// baseCoverage index. Each base glyph in the baseCoverage table has a BaseRecord.
                    /// </summary>
                    public struct BaseArray
                    { 
                        public ushort baseCount;
                        public List<BaseRecord> baseRecord;

                        public void Read(TTFReader r, int markClassCount)
                        {
                            r.ReadInt(out this.baseCount);
                            this.baseRecord = new List<BaseRecord>();
                            for(int i = 0; i < this.baseCount; ++i)
                            {
                                BaseRecord br = new BaseRecord();
                                br.Read(r, markClassCount);
                                baseRecord.Add(br);
                            }
                        }
                    }

                    /// <summary>
                    /// A BaseRecord declares one Anchor table for each mark class (including Class 0) 
                    /// identified in the MarkRecords of the MarkArray table. Each Anchor table 
                    /// specifies one attachment point used to attach all the marks in a particular
                    /// class to the base glyph. A BaseRecord contains an array of offsets to Anchor 
                    /// tables (baseAnchorOffsets). The zero-based array of offsets defines the entire 
                    /// set of attachment points each base glyph uses to attach marks. The offsets to 
                    /// Anchor tables are ordered by mark class.
                    /// </summary>
                    public struct BaseRecord
                    {
                        /// <summary>
                        /// Array of offsets (one per mark class) to Anchor tables. Offsets are from 
                        /// beginning of BaseArray table, ordered by class (offsets may be NULL).
                        /// </summary>
                        public List<ushort> baseAnchorOffsets;

                        public void Read(TTFReader r, int markClassCount)
                        {
                            this.baseAnchorOffsets = new List<ushort>();
                            for(int i = 0; i < markClassCount; ++i)
                                this.baseAnchorOffsets.Add(r.ReadUInt16());
                        }
                    }

                    /// <summary>
                    /// The MarkLigPosFormat1 subtable also contains an offset to a LigatureArray table 
                    /// (ligatureArrayOffset), which defines for each ligature glyph the two-dimensional 
                    /// array of anchor data: one anchor per ligature component per mark class.
                    /// </summary>
                    public struct MarkLigPosFormat1
                    {
                        public ushort posFormat;                // Format identifier: format = 1
                        public ushort markCoverageOffset;       // Offset to markCoverage table, from beginning of MarkLigPos subtable.
                        public ushort ligatureCoverageOffset;   // Offset to ligatureCoverage table, from beginning of MarkLigPos subtable.
                        public ushort markClassCount;           // Number of defined mark classes
                        public ushort markArrayOffset;          // Offset to MarkArray table, from beginning of MarkLigPos subtable.
                        public ushort ligatureArrayOffset;      // Offset to LigatureArray table, from beginning of MarkLigPos subtable.

                        public void Read(TTFReader r, bool readFormat)
                        {
                            if(readFormat == true)
                                r.ReadInt(out this.posFormat);

                            r.ReadInt(out this.markCoverageOffset);
                            r.ReadInt(out this.ligatureCoverageOffset);
                            r.ReadInt(out this.markClassCount);
                            r.ReadInt(out this.markArrayOffset);
                            r.ReadInt(out this.ligatureArrayOffset);
                        }
                    }

                    /// <summary>
                    /// The LigatureArray table contains a count (ligatureCount) and an array of offsets 
                    /// (ligatureAttachOffsets) to LigatureAttach tables. The ligatureAttachOffsets array 
                    /// lists the offsets to
                    ///
                    /// LigatureAttach tables, one for each ligature glyph listed in the ligatureCoverage 
                    /// table, in the same order as the ligatureCoverage index.
                    /// </summary>
                    public struct LigatureArray
                    {
                        public ushort ligatureCount;                // Number of LigatureAttach table offsets
                        public List<ushort> ligatureAttachOffsets;  // Array of offsets to LigatureAttach tables. Offsets are from beginning of LigatureArray table, ordered by ligatureCoverage index.

                        public void Read(TTFReader r)
                        {
                            r.ReadInt(out this.ligatureCount);
                            this.ligatureAttachOffsets = new List<ushort>();
                            for(int i = 0; i < this.ligatureCount; ++i)
                                this.ligatureAttachOffsets.Add(r.ReadUInt16());
                        }
                    }

                    /// <summary>
                    /// Each LigatureAttach table consists of an array (componentRecords) and count 
                    /// (componentCount) of the component glyphs in a ligature. The array stores the 
                    /// ComponentRecords in the same order as the components in the ligature. The 
                    /// order of the records also corresponds to the writing direction — that is, 
                    /// the logical direction — of the text. For text written left to right, the first 
                    /// component is on the left; for text written right to left, the first component 
                    /// is on the right.
                    /// </summary>
                    public struct LigatureAttach
                    {
                        public ushort componentCount;                   // Number of ComponentRecords in this ligature
                        public List<ComponentRecord> componentRecords;  // Array of Component records, ordered in writing direction.

                        public void Read(TTFReader r, int markClassCount)
                        {
                            r.ReadInt(out this.componentCount);

                            this.componentRecords = new List<ComponentRecord>();
                            for(int i = 0; i < this.componentCount; ++i)
                            {
                                ComponentRecord cr = new ComponentRecord();
                                cr.Read(r, markClassCount);
                                this.componentRecords.Add(cr);
                            }
                        }
                    }

                    /// <summary>
                    /// A ComponentRecord, one for each component in the ligature, contains an array 
                    /// of offsets (ligatureAnchorOffsets) to the Anchor tables that define all the 
                    /// attachment points used to attach marks to the component. For each mark class 
                    /// (including Class 0) identified in the MarkArray records, an Anchor table 
                    /// specifies the point used to attach all the marks in a particular class to 
                    /// the ligature base glyph, relative to the component.
                    /// </summary>
                    public struct ComponentRecord
                    {
                        public List<ushort> ligatureAnchorOffsets;

                        public void Read(TTFReader r, int markClassCount)
                        {
                            this.ligatureAnchorOffsets = new List<ushort>();
                            for(int i = 0; i < markClassCount; ++i)
                                this.ligatureAnchorOffsets.Add(r.ReadUInt16());
                        }
                    }

                    /// <summary>
                    /// The MarkMarkPosFormat1 subtable also has an offset to a MarkArray table for 
                    /// mark2 glyph (mark2ArrayOffset), which defines for each mark2 glyph an array 
                    /// of anchors, one for each mark1 mark class.
                    /// </summary>
                    public struct MarkMarkPosFormat1
                    {
                        public ushort posFormat;                // Format identifier: format = 1
                        public ushort mark1CoverageOffset;      // Offset to Combining Mark Coverage table, from beginning of MarkMarkPos subtable.
                        public ushort mark2CoverageOffset;      // Offset to Base Mark Coverage table, from beginning of MarkMarkPos subtable.
                        public ushort markClassCount;           // Number of Combining Mark classes defined
                        public ushort mark1ArrayOffset;         // Offset to MarkArray table for mark1, from beginning of MarkMarkPos subtable.
                        public ushort mark2ArrayOffset;         // Offset to Mark2Array table for mark2, from beginning of MarkMarkPos subtable.

                        public void Read(TTFReader r, bool readFormat)
                        {
                            if(readFormat == true)
                                r.ReadInt(out this.posFormat);

                            r.ReadInt(out this.mark1CoverageOffset);
                            r.ReadInt(out this.mark2CoverageOffset);
                            r.ReadInt(out this.markClassCount);
                            r.ReadInt(out this.mark1ArrayOffset);
                            r.ReadInt(out this.mark2ArrayOffset);
                        }
                    }

                    /// <summary>
                    /// The Mark2Array table contains one Mark2Record for each mark2 glyph listed in 
                    /// the mark2Coverage table. It stores the records in the same order as the 
                    /// mark2Coverage index.
                    /// </summary>
                    public struct Mark2Array
                    {
                        public ushort mark2Count;               // Number of Mark2 records
                        public List<Mark2Record> mark2Records;  // Array of Mark2Records, in Coverage order.

                        public void Read(TTFReader r, int offsets)
                        {
                            r.ReadInt(out this.mark2Count);

                            this.mark2Records = new List<Mark2Record>();
                            for(int i = 0; i < this.mark2Count; ++i)
                            { 
                                Mark2Record m2r = new Mark2Record();
                                m2r.Read(r, offsets);
                                this.mark2Records.Add(m2r);
                            }
                        }
                    }

                    /// <summary>
                    /// A Mark2Record declares one Anchor table for each mark class (including Class 0) 
                    /// identified in the MarkRecords of the MarkArray. Each Anchor table specifies 
                    /// one mark2 attachment point used to attach all the mark1 glyphs in a particular 
                    /// class to the mark2 glyph.
                    /// </summary>
                    public struct Mark2Record
                    {
                        /// <summary>
                        /// Array of offsets (one per class) to Anchor tables. Offsets are from beginning 
                        /// of Mark2Array table, in class order (offsets may be NULL).
                        /// </summary>
                        public List<ushort> mark2AnchorOffsets;

                        public void Read(TTFReader r, int offsets)
                        {
                            this.mark2AnchorOffsets = new List<ushort>();
                            for(int i = 0; i < offsets; ++i)
                                mark2AnchorOffsets.Add(r.ReadUInt16());
                        }
                    }

                    public struct ExtensionPosFormat1
                    {
                        public ushort posFormat;                // Format identifier: format = 1
                        public ushort extensionLookupType;      // Lookup type of subtable referenced by extensionOffset (i.e. the extension subtable).
                        public uint extensionOffset;            // Offset to the extension subtable, of lookup type extensionLookupType, relative to the start of the ExtensionPosFormat1 subtable.

                        public void Read(TTFReader r)
                        {
                            r.ReadInt(out this.posFormat);
                            r.ReadInt(out this.extensionLookupType);
                            r.ReadInt(out this.extensionOffset);
                        }
                    }

                    /// <summary>
                    /// GPOS subtables use ValueRecords to describe all the variables and values used 
                    /// to adjust the position of a glyph or set of glyphs. A ValueRecord may define 
                    /// any combination of X and Y values (in design units) to add to (positive values) 
                    /// or subtract from (negative values) the placement and advance values provided in 
                    /// the font. In non-variable fonts, a ValueRecord may also contain an offset to a 
                    /// Device table for each of the specified values. In a variable font, it may also 
                    /// contain an offset to a VariationIndex table for each of the specified values.
                    /// </summary>
                    public struct ValueRecord
                    {
                        public short xPlacement;               // Horizontal adjustment for placement, in design units.
                        public short yPlacement;               // Vertical adjustment for placement, in design units.
                        public short xAdvance;                 // Horizontal adjustment for advance, in design units — only used for horizontal layout.
                        public short yAdvance;                 // Vertical adjustment for advance, in design units — only used for vertical layout.
                        public ushort xPlaDeviceOffset;        // Offset to Device table (non-variable font) / VariationIndex table (variable font) for horizontal placement, from beginning of the immediate parent table (SinglePos or PairPosFormat2 lookup subtable, PairSet table within a PairPosFormat1 lookup subtable) — may be NULL.
                        public ushort yPlaDeviceOffset;        // Offset to Device table (non-variable font) / VariationIndex table (variable font) for vertical placement, from beginning of the immediate parent table (SinglePos or PairPosFormat2 lookup subtable, PairSet table within a PairPosFormat1 lookup subtable) — may be NULL.
                        public ushort xAdvDeviceOffset;        // Offset to Device table (non-variable font) / VariationIndex table (variable font) for horizontal advance, from beginning of the immediate parent table (SinglePos or PairPosFormat2 lookup subtable, PairSet table within a PairPosFormat1 lookup subtable) — may be NULL.
                        public ushort yAdvDeviceOffset;        // Offset to Device table (non-variable font) / VariationIndex table (variable font) for vertical advance, from beginning of the immediate parent table (SinglePos or PairPosFormat2 lookup subtable, PairSet table within a PairPosFormat1 lookup subtable) — may be NULL.


                        public void Read(TTFReader r)
                        {
                            r.ReadInt(out this.xPlacement);
                            r.ReadInt(out this.yPlacement);
                            r.ReadInt(out this.xAdvance);
                            r.ReadInt(out this.yAdvance);
                            r.ReadInt(out this.xPlaDeviceOffset);
                            r.ReadInt(out this.yPlaDeviceOffset);
                            r.ReadInt(out this.xAdvDeviceOffset);
                            r.ReadInt(out this.yAdvDeviceOffset);
                        }
                    }

                    [System.Flags]
                    public enum ValueFormat
                    {
                        xPlace      = 0x0001,
                        yPlace      = 0x0002,
                        xAdv        = 0x0004,
                        yAdv        = 0x0008,
                        xPlaceDev   = 0x0010,
                        yPlaceDev   = 0x0020,
                        xAdvDev     = 0x0040,
                        yAdvDev     = 0x0080,
                        Reserved    = 0x00FF
                    }

                    /// <summary>
                    /// AnchorFormat1 consists of a format identifier (anchorFormat) and a pair 
                    /// of design-unit coordinates (xCoordinate and yCoordinate) that specify 
                    /// the location of the anchor point. This format has the benefits of small 
                    /// size and simplicity, but the anchor point cannot be hinted to adjust 
                    /// its position for different device resolutions.
                    /// </summary>
                    public struct AnchorFormat1
                    {
                        public ushort anchorFormat;         // Format identifier, = 1
                        public short xCoordinate;           // Horizontal value, in design units
                        public short yCoordinate;           // Vertical value, in design units

                        public void Read(TTFReader r, bool readFormat)
                        {
                            if(readFormat == true)
                                r.ReadInt(out this.anchorFormat);

                            r.ReadInt(out this.xCoordinate);
                            r.ReadInt(out this.yCoordinate);
                        }
                    }

                    /// <summary>
                    /// Like AnchorFormat1, AnchorFormat2 specifies a format identifier (anchorFormat) 
                    /// and a pair of design unit coordinates for the anchor point (xCoordinate and 
                    /// yCoordinate).
                    ///
                    /// For fine-tuning the location of the anchor point, AnchorFormat2 also provides 
                    /// index to a glyph contour point(anchorPoint) that is on the outline of a glyph.
                    /// Hinting can be used to move the contour anchor point.In the rendered text, the 
                    /// anchor point will provide the final positioning data for a given ppem size.
                    /// </summary>
                    public struct AnchorFormat2
                    {
                        public ushort anchorFormat;     // Format identifier, = 2
                        public short xCoordinate;       // Horizontal value, in design units
                        public short yCoordinate;       // Vertical value, in design units
                        public ushort anchorPoint;      // Index to glyph contour point

                        public void Read(TTFReader r, bool readFormat)
                        {
                            if(readFormat == true)
                                r.ReadInt(out this.anchorFormat);

                            r.ReadInt(out this.xCoordinate);
                            r.ReadInt(out this.yCoordinate);
                            r.ReadInt(out this.anchorPoint);
                        }
                    }

                    /// <summary>
                    /// Like AnchorFormat1, AnchorFormat3 specifies a format identifier (anchorFormat) 
                    /// and locates an anchor point (xCoordinate and yCoordinate). And, like AnchorFormat 2, 
                    /// it permits fine adjustments in variable fonts to the coordinate values. However, 
                    /// AnchorFormat3 uses Device tables, rather than a contour point, for this adjustment.
                    ///
                    /// With a Device table, a client can adjust the position of the anchor point for any 
                    /// font size and device resolution.AnchorFormat3 can specify offsets to Device tables 
                    /// for the X coordinate (xDeviceOffset) and the Y coordinate (yDeviceOffset). If only 
                    /// one coordinate requires adjustment, the offset to the Device table for the other 
                    /// coordinate may be set to NULL.
                    /// </summary>
                    public struct AnchorFormat3
                    {
                        public ushort anchorFormat;     // Format identifier, = 3
                        public short xCoordinate;       // Horizontal value, in design units
                        public short yCoordinate;       // Vertical value, in design units
                        public ushort xDeviceOffset;    // Offset to Device table (non-variable font) / VariationIndex table (variable font) for X coordinate, from beginning of Anchor table (may be NULL)
                        public ushort yDeviceOffset;    // Offset to Device table (non-variable font) / VariationIndex table (variable font) for Y coordinate, from beginning of Anchor table (may be NULL)

                        public void Read(TTFReader r, bool readFormat)
                        {
                            if(readFormat == true)
                                r.ReadInt(out this.anchorFormat);

                            r.ReadInt(out this.xCoordinate);
                            r.ReadInt(out this.yCoordinate);
                            r.ReadInt(out this.xDeviceOffset);
                            r.ReadInt(out this.yDeviceOffset);
                        }
                    }

                    /// <summary>
                    /// The MarkArray table defines the class and the anchor point for a mark glyph. 
                    /// Three GPOS subtable types — MarkToBase attachment, MarkToLigature attachment, 
                    /// and MarkToMark attachment — use the MarkArray table to specify data for attaching 
                    /// marks.
                    /// </summary>
                    public struct MarkArray
                    {
                        public ushort markCount;                // Number of MarkRecords
                        public List<Mark2Record> markRecords;   // Array of MarkRecords, ordered by corresponding glyphs in the associated mark Coverage table.

                        public void Read(TTFReader r)
                        {
                            r.ReadInt(out this.markCount);

                            this.markRecords = new List<Mark2Record>();
                            for(int i = 0; i < this.markCount; ++i)
                            { 
                                Mark2Record mr = new Mark2Record();
                                mr.Read(r, markCount);
                                this.markRecords.Add(mr);
                            }
                        }
                    }

                    public struct MarkRecord
                    {
                        public ushort markClass;                // Class defined for the associated mark.
                        public ushort markAnchorOffset;         // Offset to Anchor table, from beginning of MarkArray table.

                        public void Read(TTFReader r)
                        {
                            r.ReadInt(out this.markClass);
                            r.ReadInt(out this.markAnchorOffset);
                        }
                    }

                    public const string TagName = "GPOS";

                    public ushort majorVersion;
                    public ushort minorVersion;
                    public ushort scriptListOffset;
                    public ushort featureListOffset;
                    public ushort lookupListOffset;
                    public ushort featureVariableOffset;

                    public void Read(TTFReader r)
                    {
                        r.ReadInt(out this.majorVersion);
                        r.ReadInt(out this.minorVersion);
                        r.ReadInt(out this.scriptListOffset);
                        r.ReadInt(out this.featureListOffset);
                        r.ReadInt(out this.lookupListOffset);
                        r.ReadInt(out this.featureVariableOffset);

                        // The rest of the data is skipped for now
                    }
                }
            }
        }
    }
}
                