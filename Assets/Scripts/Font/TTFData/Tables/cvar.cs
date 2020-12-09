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
                /// cvar — CVT Variations Table
                /// https://docs.microsoft.com/en-us/typography/opentype/spec/cvar
                /// 
                /// The control value table (CVT) variations table is used in variable fonts to 
                /// provide variation data for CVT values. For a general overview of OpenType 
                /// Font Variations, see the chapter, OpenType Font Variations Overview.
                /// </summary>
                public struct cvar
                {
                    public const string TagName = "cvar";

                    //The tupleVariationCount field contains a packed value that includes 
                    // flags and the number of logical tuple variation tables — which is 
                    // also the number of physical tuple variation headers. The format of 
                    // the tupleVariationCount value is as follows:
                    const ushort SHARED_POINT_NUMBERS = 0x8000;     // Flag indicating that some or all tuple variation tables reference a shared set of “point” numbers. These shared numbers are represented as packed point number data at the start of the serialized data.
                    const ushort Reserved = 0x7000;                 // Reserved for future use — set to 0.
                    const ushort COUNT_MARK = 0xFFF;                // Mask for the low bits to give the number of tuple variation tables.

                    public ushort majorVersion;         // Major version number of the 'cvar' table — set to 1.
                    public ushort minorVersion;         // Minor version number of the 'cvar' table — set to 0.
                    public ushort tupleVariationCount;  // A packed field. The high 4 bits are flags (see below), and the low 12 bits are the number of tuple variation tables. The count can be any number between 1 and 4095.
                    public short dataOffset;            // Offset from the start of the 'cvar' table to the serialized data.
                    public List<TupleVariationHeader> tupleVariationHeaders;

                    public void Read(TTFReader r, int axisCount)
                    {
                        r.ReadInt(out this.majorVersion);
                        r.ReadInt(out this.minorVersion);
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
}