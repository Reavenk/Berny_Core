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

namespace PxPre.Berny.Font
{
    /// <summary>
    /// The information for a Berny font typeface.
    /// </summary>
    public class Typeface
    {
        /// <summary>
        /// The name of the typeface. (Currently UNSET and UNUSED).
        /// </summary>
        public string name = "";

        /// <summary>
        /// The glyphs, in index order.
        /// </summary>
        public List<Glyph> glyphs = new List<Glyph>();

        /// <summary>
        /// The glyphs, mapped to ASCII/UNICODE values.
        /// </summary>
        public Dictionary<int, Glyph> glyphLookup = new Dictionary<int, Glyph>();

        /// <summary>
        /// The vertical distance for a newline.
        /// </summary>
        public float newlineDst = 0.0f;

    }
}
