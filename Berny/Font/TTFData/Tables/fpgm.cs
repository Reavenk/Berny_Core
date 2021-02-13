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
    /// fpgm — Font Program
    /// https://docs.microsoft.com/en-us/typography/opentype/spec/fpgm
    /// 
    /// This table is similar to the CVT Program, except that it is only run once, when the font is 
    /// first used. It is used only for FDEFs and IDEFs. Thus the CVT Program need not contain function 
    /// definitions. However, the CVT Program may redefine existing FDEFs or IDEFs.
    /// </summary>
    public struct fpgm
    {
        public const string TagName = "fpgm";

        public byte [] instructions;

        public void Read(TTFReader r, int tableSize)
        { 
            this.instructions = r.ReadBytes(tableSize);
        }
    }
}