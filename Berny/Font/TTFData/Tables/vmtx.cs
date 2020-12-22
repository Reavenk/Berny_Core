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
                /// vmtx — Vertical Metrics Table
                /// https://docs.microsoft.com/en-us/typography/opentype/spec/vmtx
                /// 
                /// The vertical metrics table allows you to specify the vertical spacing for each 
                /// glyph in a vertical font. This table consists of either one or two arrays that 
                /// contain metric information (the advance heights and top sidebearings) for the 
                /// vertical layout of each of the glyphs in the font. The vertical metrics coordinate 
                /// system is shown below.
                /// </summary>
                public struct vmtx
                {
                    // This is basically going to be like hmtx, except we swap the word
                    // "horizontal" with "vertical"

                    /// <summary>
                    /// The format of an entry in the vertical metrics array.
                    /// </summary>
                    public struct longVerMetric
                    {
                        public ushort advanceHeight;    // The advance height of the glyph.Unsigned integer in FUnits
                        public ushort topSideBearing;   // The top sidebearing of the glyph. Signed integer in FUnits.
                    }

                    public const string TagName = "vmtx";

                    /// <summary>
                    /// In monospaced fonts, such as Courier or Kanji, all glyphs have the same 
                    /// advance height. If the font is monospaced, only one entry need be in the 
                    /// first array, but that one entry is required.
                    /// </summary>
                    public List<longVerMetric> vMetrics; 

                    public List<short> topSideBearings; // The top sidebearing of the glyph. Signed integer in FUnits.

                    public void Read(TTFReader r, int numberofVMetrics, int numGlyphs)
                    {

                        this.vMetrics = new List<longVerMetric>();
                        for (int i = 0; i < numberofVMetrics; ++i)
                        {
                            longVerMetric lvm = new longVerMetric();
                            r.ReadInt(out lvm.advanceHeight);
                            r.ReadInt(out lvm.topSideBearing);
                            this.vMetrics.Add(lvm);
                        }

                        this.topSideBearings = new List<short>();
                        // We could have them pass in the numGlyphs-numberOfHMetrics instead of 
                        // calculating this ourselves, but I think this helps add rigor.
                        for (int i = 0; i < numGlyphs - numberofVMetrics; ++i)
                            this.topSideBearings.Add(r.ReadInt16());
                    }
                }
            }
        }
    }
}

