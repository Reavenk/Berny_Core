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
            /// <summary>
            /// ItemVariationData
            /// https://docs.microsoft.com/en-us/typography/opentype/spec/otvarcommonformats
            /// 
            /// Each item variation data subtable includes deltas for some number of items, and 
            /// some subset of regions. The regions are indicated by an array of indices into 
            /// the variation region list.
            /// </summary>
            public struct ItemVariationData
            {
                public ushort itemCount;            // The number of delta sets for distinct items.
                public ushort shortDeltaCount;      // The number of deltas in each delta set that use a 16-bit representation. Must be less than or equal to regionIndexCount.
                public ushort regionIndexCount;     // The number of variation regions referenced.
                public List<ushort> regionIndexes;  // Array of indices into the variation region list for the regions referenced by this item variation data table.
                public List<DeltaSet> deltaSets;    // Delta-set rows.

                public void Read(TTFReader r)
                {
                    r.ReadInt(out this.itemCount);
                    r.ReadInt(out this.shortDeltaCount);
                    r.ReadInt(out this.regionIndexCount);

                    this.regionIndexes = new List<ushort>();
                    for (int i = 0; i < this.regionIndexCount; ++i)
                        this.regionIndexes.Add( r.ReadUint16());
                    
                    this.deltaSets = new List<DeltaSet>();
                    for (int i = 0; i < this.itemCount; ++i)
                    {
                        DeltaSet ds = new DeltaSet();
                        ds.Read(r, this.regionIndexCount, this.shortDeltaCount);
                        this.deltaSets.Add(ds);
                    }
                }
            }
        }
    }
}