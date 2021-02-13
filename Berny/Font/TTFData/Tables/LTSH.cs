// MIT License
// 
// Copyright (c) 2021 Pixel Precision LLC
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

namespace PxPre.Berny.TTF.Table
{
    /// <summary>
    /// LTSH - Linear Threshold
    /// https://docs.microsoft.com/en-us/typography/opentype/spec/ltsh
    /// 
    /// The LTSH table relates to OpenType™ fonts containing TrueType outlines. There are
    /// noticeable improvements to fonts on the screen when instructions are carefully 
    /// applied to the sidebearings. The gain in readability is offset by the necessity 
    /// for the OS to grid fit the glyphs in order to find the actual advance width for 
    /// the glyphs (since instructions may be moving the sidebearing points). The TrueType 
    /// outline format already has two mechanisms to side step the speed issues: the 'hdmx' 
    /// table, where precomputed advance widths may be saved for selected ppem sizes, and 
    /// the VDMX table, where precomputed vertical advance widths may be saved for selected 
    /// ppem sizes. The LTSH table (Linear ThreSHold) is a second, complementary method.
    /// </summary>
    public struct LTSH
    {
        public const string TagName = "LTSH";

        public ushort version;          // Version number (starts at 0).
        public ushort numGlyphs;        // Number of glyphs (from “numGlyphs” in 'maxp' table).
        public byte [] yPels;           // The vertical pel height at which the glyph can be assumed to scale linearly. On a per glyph basis.

        public void Read(TTFReader r)
        { 
            r.ReadInt(out this.version);
            r.ReadInt(out this.numGlyphs);
            this.yPels = r.ReadBytes(this.numGlyphs);
        }
    }
}