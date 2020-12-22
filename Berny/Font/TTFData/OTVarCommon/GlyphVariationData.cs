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
            /// Tuple Variation Store Header
            /// https://docs.microsoft.com/en-us/typography/opentype/spec/otvarcommonformats
            /// 
            /// The two variants of a tuple variation store header, the GlyphVariationData table 
            /// header and the 'cvar' header, are only slightly different. 
            /// </summary>
            public struct GlyphVariationData
            {
                ushort tupleVariationCount; // A packed field. The high 4 bits are flags (see below), and the low 12 bits are the number of tuple variation tables for this glyph. The count can be any number between 1 and 4095.
                ushort dataOffset;          // Offset from the start of the GlyphVariationData table to the serialized data.
                List<TupleVariationHeader> tupleVariationHeaders;   // Array of tuple variation headers.

                public void Read(TTFReader r, int axisCount)
                {
                    r.ReadInt(out this.tupleVariationCount);
                    r.ReadInt(out this.dataOffset);

                    this.tupleVariationHeaders = new List<TupleVariationHeader>();
                    for (int i = 0; i < this.tupleVariationCount; ++i)
                    {
                        TupleVariationHeader tvh = new TupleVariationHeader();
                        tvh.Read(r, axisCount);

                        this.tupleVariationHeaders.Add(tvh);
                    }
                }
            }
        }
    }
}