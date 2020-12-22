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
using UnityEngine;

namespace PxPre
{
    namespace Berny
    {
        namespace TTF
        {
            /// <summary>
            /// Tuple Records
            /// https://docs.microsoft.com/en-us/typography/opentype/spec/otvarcommonformats
            /// 
            /// he tuple variation store formats reference regions within the font’s variation space using tuple records. These references identify positions in terms of normalized coordinates, which use F2DOT14 values.
            /// </summary>
            public struct Tuple
            {
                public List<float> coordinates;

                public void Read(TTFReader r, int ct)
                {
                    if(this.coordinates == null)
                        this.coordinates = new List<float>();

                    for(int i = 0; i < ct; ++i)
                        coordinates.Add(r.ReadFDot14());
                }
            }

            public struct Tuple2
            {
                public Vector2 v;

                public void Read(TTFReader r)
                {
                    v.x = r.ReadFDot14();
                    v.y = r.ReadFDot14();
                }
            }

            public struct Tuple3
            {
                public Vector3 v;

                public void Read(TTFReader r)
                {
                    v.x = r.ReadFDot14();
                    v.y = r.ReadFDot14();
                    v.z = r.ReadFDot14();
                }
            }

            public struct Tuple4
            {
                public Vector4 v;

                public void Read(TTFReader r)
                {
                    v.x = r.ReadFDot14();
                    v.y = r.ReadFDot14();
                    v.z = r.ReadFDot14();
                    v.w = r.ReadFDot14();
                }
            }
        }
    }
}