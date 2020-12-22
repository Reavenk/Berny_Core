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
                /// avar — Axis Variations Table
                /// https://docs.microsoft.com/en-us/typography/opentype/spec/avar
                /// 
                /// The axis variations table ('avar') is an optional table used in variable fonts. It can be used to modify aspects of how a design varies for different instances along a particular design-variation axis. Specifically, it allows modification of the coordinate normalization that is used when processing variation data for a particular variation instance.
                /// </summary>
                public struct avar
                {
                    public const string TagName = "avar";

                    /// <summary>
                    /// There must be one segment map for each axis defined in the 'fvar' table, and the segment maps for the different axes must be given in the order of axes specified in the 'fvar' table. The segment map for each axis is comprised of a list of axis-value mapping records.
                    /// </summary>
                    public struct SegmentMap
                    {
                        public ushort positionMapCount;             // The number of correspondence pairs for this axis.
                        public List<AxisValueMap> axisValueMaps;    // The array of axis value map records for this axis.
                    }

                    /// <summary>
                    /// Each axis value map record provides a single axis-value mapping correspondence.
                    /// </summary>
                    public struct AxisValueMap
                    {
                        public float fromCoordinate;                // A normalized coordinate value obtained using default normalization.
                        public float toCoordinate;                  // The modified, normalized coordinate value.
                    }

                    public ushort majorVersion;     // Major version number of the axis variations table — set to 1.
                    public ushort minorVersion;     // Minor version number of the axis variations table — set to 0.
                    public ushort reserved;         // Permanently reserved; set to zero.
                    public ushort axisCount;        // The number of variation axes for this font. This must be the same number as axisCount in the 'fvar' table.
                    public List<SegmentMap> axisSegmentMap; // The segment maps array — one segment map for each axis, in the order of axes specified in the 'fvar' table.

                    public void Read(TTFReader r)
                    {
                        r.ReadInt(out this.majorVersion);
                        r.ReadInt(out this.minorVersion);
                        r.ReadInt(out reserved);
                        r.ReadInt(out axisCount);

                        this.axisSegmentMap = new List<SegmentMap>();
                        for (int i = 0; i < this.axisCount; ++i)
                        {
                            SegmentMap sm = new SegmentMap();
                            sm.axisValueMaps = new List<AxisValueMap>();

                            r.ReadInt(out sm.positionMapCount);
                            for (int j = 0; j < sm.positionMapCount; ++j)
                            {
                                AxisValueMap avm = new AxisValueMap();

                                avm.fromCoordinate = r.ReadFDot14();
                                avm.toCoordinate = r.ReadFDot14();

                                sm.axisValueMaps.Add(avm);
                            }

                            this.axisSegmentMap.Add(sm);
                        }
                    }
                }
            }
        }
    }
}
