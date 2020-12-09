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
                public struct vtmx
                {
                    public ushort advanceHeight;
                    public ushort topSideBearing;

                    public void Read(TTFReader r)
                    {
                        r.ReadInt(out this.advanceHeight);
                        r.ReadInt(out this.topSideBearing);
                    }
                }
            }
        }
    }
}

