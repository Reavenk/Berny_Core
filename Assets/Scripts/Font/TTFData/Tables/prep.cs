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
                /// prep — Control Value Program
                /// https://docs.microsoft.com/en-us/typography/opentype/spec/prep
                /// 
                /// The Control Value Program consists of a set of TrueType instructions 
                /// that will be executed whenever the font or point size or transformation 
                /// matrix change and before each glyph is interpreted. Any instruction is 
                /// valid in the CV Program but since no glyph is associated with it, 
                /// instructions intended to move points within a particular glyph outline 
                /// cannot be used in the CV Program. The name 'prep' is anachronistic (the 
                /// table used to be known as the Pre Program table.)
                /// </summary>
                public struct prep
                {
                    public const string TagName = "prep";

                    // https://developer.apple.com/fonts/TrueType-Reference-Manual/RM03/Chap3.html
                    List<char> instructions;

                    public void Read(TTFReader r, int tableSz)
                    {
                        this.instructions = new List<char>();

                        for (int i = 0; i < tableSz; ++i)
                            this.instructions.Add( r.ReadUint8());
                    }
                }
            }
        }
    }
}