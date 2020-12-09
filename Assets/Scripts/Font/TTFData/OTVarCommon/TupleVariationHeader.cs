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

namespace PxPre
{
    namespace Berny
    {
        namespace TTF
        {
            /// <summary>
            /// TupleVariationHeader
            /// https://docs.microsoft.com/en-us/typography/opentype/spec/otvarcommonformats
            /// 
            /// The GlyphVariationData and 'cvar' header formats include an array of tuple variation 
            /// headers. The TupleVariationHeader format is as follows.
            /// </summary>
            public struct TupleVariationHeader
            {
                // The tupleIndex field contains a packed value that includes flags and an
                // index into a shared tuple records array(not used in the 'cvar' table). 
                // The format of the tupleIndex field is as follows.
                public const ushort EMBEDDED_PEAK_TUPLE = 0x8000;
                public const ushort INTERMEDIATE_REGION = 0x4000;
                public const ushort PRIVATE_POINT_NUMBERS = 0x2000;
                public const ushort Reserved = 0x1000;
                public const ushort TUPLE_INDEX_MASK = 0xFFFF;

                public ushort variationDataSize;        // The size in bytes of the serialized data for this tuple variation table.
                public ushort tupleIndex;               // A packed field. The high 4 bits are flags (see below). The low 12 bits are an index into a shared tuple records array.
                public Tuple peakTuple;                 // Peak tuple record for this tuple variation table — optional, determined by flags in the tupleIndex value.
                public Tuple intermediateStartTuple;    // Intermediate start tuple record for this tuple variation table — optional, determined by flags in the tupleIndex value.
                public Tuple intermediateEndTuple;      // Intermediate end tuple record for this tuple variation table — optional, determined by flags in the tupleIndex value.

                public void Read(TTFReader r, int axisCount)
                {
                    r.ReadInt(out this.variationDataSize);
                    r.ReadInt(out this.tupleIndex);

                    this.peakTuple = new Tuple();
                    this.peakTuple.Read(r, axisCount);

                    this.intermediateStartTuple = new Tuple();
                    this.intermediateStartTuple.Read(r, axisCount);

                    this.intermediateEndTuple = new Tuple();
                    this.intermediateEndTuple.Read(r, axisCount);
                }
            }
        }
    }
}
