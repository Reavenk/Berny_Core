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
            /// ItemVariationStore
            /// https://docs.microsoft.com/en-us/typography/opentype/spec/otvarcommonformats
            /// </summary>
            public struct ItemVariationStore
            {
                public ushort format;                       // Format — set to 1
                public uint variationRegionListOffset;      // Offset in bytes from the start of the item variation store to the variation region list.
                public ushort itemVariationDataCount;       // The number of item variation data subtables.
                public List<uint> itemVariationDataOffsets; // Offsets in bytes from the start of the item variation store to each item variation data subtable.

                public void Read(TTFReader r)
                {
                    r.ReadInt(out this.format);
                    r.ReadInt(out this.variationRegionListOffset);
                    r.ReadInt(out this.itemVariationDataCount);

                    this.itemVariationDataOffsets = new List<uint>();
                    for(int i = 0; i < this.itemVariationDataCount; ++i)
                        itemVariationDataOffsets.Add(r.ReadUint16());
                }
            }
        }
    }
}