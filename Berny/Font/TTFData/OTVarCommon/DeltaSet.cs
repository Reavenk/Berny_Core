﻿// MIT License
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

namespace PxPre.Berny.TTF
{
    /// <summary>
    /// DeltaSet
    /// https://docs.microsoft.com/en-us/typography/opentype/spec/otvarcommonformats
    /// 
    /// The deltaSets array represents a logical two-dimensional table of delta values with 
    /// itemCount rows and regionIndexCount columns. Rows in the table provide sets of deltas 
    /// for particular target items, and columns correspond to regions of the variation 
    /// space. Each DeltaSet record in the array represents one row of the delta-value 
    /// table — one delta set.
    /// </summary>
    public struct DeltaSet
    {
        /// <summary>
        /// Variation delta values.
        /// </summary>
        public List<int> deltaData;

        public void Read(TTFReader r, int regionIndexCount, int shortDeltaCount)
        {
            this.deltaData = new List<int>();

            for (int i = 0; i < shortDeltaCount; ++i)
                deltaData.Add(r.ReadInt16());

            for (int i = 0; i < regionIndexCount - shortDeltaCount; ++i)
                deltaData.Add(r.ReadInt8());
        }
    }

}
