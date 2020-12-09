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

namespace PxPre
{
    namespace Berny
    {
        namespace TTF
        {
            namespace Table
            {
                /// <summary>
                /// fvar — Font Variations Table
                /// https://docs.microsoft.com/en-us/typography/opentype/spec/fvar
                /// 
                /// OpenType Font Variations allow a font designer to incorporate multiple 
                /// faces within a font family into a single font resource. Variable fonts 
                /// can provide great flexibility for content authors and designers while 
                /// also allowing the font data to be represented in an efficient format.
                /// </summary>
                public struct fvar
                {
                    /// <summary>
                    /// The format of the variation axis record is as follows:
                    /// </summary>
                    public struct VariationAxisRecord
                    {
                        // Flags can be assigned to indicate certain uses or behaviors 
                        // for a given axis, independent of the specific axis tag and 
                        // its definition.If no flags are set, then no assumptions are 
                        // to be made beyond the definition for a registered axis.The 
                        // following flags are defined.
                        public const ushort HIDDEN_AXIS = 0x0001;
                        public const ushort Reserved = 0xFFFE; // Arguably not worth capturing

                        public string axisTag;              // Tag identifying the design variation for the axis.
                        public float minValue;              // The minimum coordinate value for the axis.
                        public float defaultValue;          // The default coordinate value for the axis.
                        public float maxValue;              // The maximum coordinate value for the axis.
                        public ushort flags;                // Axis qualifiers.
                        public ushort axisNameID;           // The name ID for entries in the 'name' table that provide a display name for this axis.

                        public void Read(TTFReader r)
                        {
                            this.axisTag = r.ReadString(4);
                            this.minValue = r.ReadFixed();
                            this.defaultValue = r.ReadFixed();
                            this.maxValue = r.ReadFixed();
                            r.ReadInt(out this.flags);
                            r.ReadInt(out this.axisNameID);
                        }
                    }

                    public struct InstanceRecord
                    {
                        public ushort subfamiltyNameID;     // The name ID for entries in the 'name' table that provide subfamily names for this instance.
                        public ushort flags;                // Reserved for future use — set to 0.
                        public Tuple coordinates;           // The coordinates array for this instance.
                        public ushort postScriptNameID;     // Optional.The name ID for entries in the 'name' table that provide PostScript names for this instance.

                        public void Read(TTFReader r, int axisCount)
                        {
                            r.ReadInt(out this.subfamiltyNameID);
                            r.ReadInt(out this.flags);
                            this.coordinates.Read(r, axisCount);
                            r.ReadInt(out this.postScriptNameID);
                        }
                    }

                    public const string TagName = "fvar";

                    public ushort majorVersion;         // Major version number of the font variations table — set to 1.
                    public ushort minorVersion;         // Minor version number of the font variations table — set to 0.
                    public ushort axesArrayOffset;      // Offset in bytes from the beginning of the table to the start of the VariationAxisRecord array.
                    public ushort reserved;             // This field is permanently reserved. Set to 2.
                    public ushort axisCount;            // The number of variation axes in the font (the number of records in the axes array).
                    public ushort axisSize;             // The size in bytes of each VariationAxisRecord — set to 20 (0x0014) for this version.
                    public ushort instanceCount;        // The number of named instances defined in the font (the number of records in the instances array).
                    public ushort instanceSize;         // The size in bytes of each InstanceRecord — set to either axisCount * sizeof(Fixed) + 4, or to axisCount * sizeof(Fixed) + 6.

                    public List<VariationAxisRecord> axes;  // The variation axis array.
                    public List<InstanceRecord> instances;  // The named instance array.

                    public void Read(TTFReader r, bool loadArrays = true)
                    {
                        r.ReadInt(out this.majorVersion);
                        r.ReadInt(out this.minorVersion);
                        r.ReadInt(out this.axesArrayOffset);
                        r.ReadInt(out this.reserved);
                        r.ReadInt(out this.axisCount);
                        r.ReadInt(out this.axisSize);
                        r.ReadInt(out this.instanceCount);
                        r.ReadInt(out this.instanceSize);

                        if(loadArrays == true)
                            this.LoadArrays(r);
                    }

                    public void LoadArrays(TTFReader r)
                    {
                        r.SetPosition(this.axesArrayOffset);

                        this.axes = new List<VariationAxisRecord>();    // The variation axis array.
                        this.instances = new List<InstanceRecord>();    // The named instance array.

                        for (int i = 0; i < this.axisCount; ++i)
                        {
                            VariationAxisRecord v = new VariationAxisRecord();
                            v.Read(r);
                            this.axes.Add(v);
                        }

                        for (int i = 0; i < this.instanceCount; ++i)
                        {
                            InstanceRecord ir = new InstanceRecord();
                            ir.Read(r, this.axisCount);
                            this.instances.Add(ir);
                        }

                    }
                }
            }
        }
    }
}