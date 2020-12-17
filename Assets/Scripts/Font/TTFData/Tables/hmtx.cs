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
                /// hmtx — Horizontal Metrics Table
                /// https://docs.microsoft.com/en-us/typography/opentype/spec/hmtx
                /// 
                /// Glyph metrics used for horizontal text layout include glyph advance widths, 
                /// side bearings and X-direction min and max values (xMin, xMax). These are 
                /// derived using a combination of the glyph outline data ('glyf', 'CFF ' or CFF2) 
                /// and the horizontal metrics table. The horizontal metrics ('hmtx') table provides 
                /// glyph advance widths and left side bearings.
                /// </summary>
                public struct hmtx
                {
                    public struct longHorMetric
                    {
                        public ushort advanceWidth;     // Advance width, in font design units.
                        public short lsb;               // Glyph left side bearing, in font design units.
                    }

                    public const string TagName = "hmtx";

                    public List<longHorMetric> hMetrics;    // Paired advance width and left side bearing values for each glyph. Records are indexed by glyph ID.
                    public List<short> leftSideBearings;    // Left side bearings for glyph IDs greater than or equal to numberOfHMetrics.

                    public void Read(TTFReader r, int numberOfHMetrics, int numGlyphs)
                    {
                        this.hMetrics = new List<longHorMetric>();
                        for (int i = 0; i < numberOfHMetrics; ++i)
                        {
                            longHorMetric lhm = new longHorMetric();
                            r.ReadInt(out lhm.advanceWidth);
                            r.ReadInt(out lhm.lsb);
                            this.hMetrics.Add(lhm);
                        }

                        this.leftSideBearings = new List<short>();
                        // We could have them pass in the numGlyphs-numberOfHMetrics instead of 
                        // calculating this ourselves, but I think this helps add rigor.
                        for (int i = 0; i < numGlyphs - numberOfHMetrics; ++i)
                            this.leftSideBearings.Add(r.ReadInt16());
                    }
                }
            }
        }
    }
}

