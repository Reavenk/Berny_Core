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
                /// gasp — Grid-fitting and Scan-conversion Procedure Table
                /// https://docs.microsoft.com/en-us/typography/opentype/spec/gasp
                /// 
                /// This table contains information which describes the preferred rasterization 
                /// techniques for the typeface when it is rendered on grayscale-capable devices. 
                /// This table also has some use for monochrome devices, which may use the table 
                /// to turn off hinting at very large or small sizes, to improve performance.
                /// </summary>
                public struct gasp
                {
                    public struct GaspRange
                    {
                        // There are four RangeGaspBehavior flags defined.
                        public const ushort GASP_GRIDFIT                = 0x0001; // Use gridfitting
                        public const ushort GASP_DOGRAY                 = 0x0002;  // Use grayscale rendering
                        public const ushort GASP_SYMMETRIC_GRIDFIT      = 0x0004; // Use gridfitting with ClearType symmetric smoothing Only supported in version 1 'gasp'
                        public const ushort GASP_SYMMETRIC_SMOOTHING    = 0x0008; // Use smoothing along multiple axes with ClearType® Only supported in version 1 'gasp'
                        public const ushort GASP_REVERSED               = 0xFFF0; //Reserved Reserved flags — set to 0
                        public const ushort GASP_NEITHER	            = 0x0000; //optional for very large sizes, typically ppem>2048

                        public ushort rangeMaxPPEM;             // Upper limit of range, in PPEM
                        public ushort rangeGaspBehaviour;       // Flags describing desired rasterizer behavior.

                        public void Read(TTFReader r)
                        {
                            r.ReadInt(out this.rangeMaxPPEM);
                            r.ReadInt(out this.rangeGaspBehaviour);
                        }
                    }

                    public ushort version;                  // Version number (set to 1)
                    public ushort numRanges;                // Number of records to follow
                    public List<GaspRange> gaspRanges;      // Sorted by ppem

                    public void Read(TTFReader r)
                    {
                        r.ReadInt(out this.version);
                        r.ReadInt(out this.numRanges);

                        this.gaspRanges = new List<GaspRange>();
                        for (int i = 0; i < this.numRanges; ++i)
                        {
                            GaspRange gr = new GaspRange();
                            gr.Read(r);
                            this.gaspRanges.Add(gr);
                        }
                    }
                }
            }
        }
    }
}
