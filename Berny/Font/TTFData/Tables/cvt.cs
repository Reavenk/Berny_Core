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
                /// cvt — Control Value Table
                /// https://docs.microsoft.com/en-us/typography/opentype/spec/cvt
                /// 
                /// This table contains a list of values that can be referenced by instructions. 
                /// They can be used, among other things, to control characteristics for 
                /// different glyphs. The length of the table must be an integral number of 
                /// FWORD units.
                /// </summary>
                public struct cvt
                {
                    public const string TagName = "cvt";

                    // List of n values referenceable by instructions. n is the number of FWORD 
                    // items that fit in the size of the table.
                    public List<short> values;

                    public void Read(TTFReader r, int n)
                    {
                        this.values = new List<short>();
                        for(int i = 0; i < n; ++i)
                            this.values.Add(r.ReadInt16());
                    }
                }
            }
        }
    }
}
