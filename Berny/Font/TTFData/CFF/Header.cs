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

namespace PxPre.Berny.CFF
{
    /// <summary>
    /// A header for a CFF file.
    /// 
    /// For more information see https://wwwimages2.adobe.com/content/dam/acom/en/devnet/font/pdfs/5176.CFF.pdf,
    /// page 13.
    /// </summary>
    public struct Header
    {
        /// <summary>
        /// The file's major version.
        /// </summary>
        public byte major;

        /// <summary>
        /// The file's minor version.
        /// </summary>
        public byte minor;

        /// <summary>
        /// Header size
        /// </summary>
        public byte hdrSize;

        /// <summary>
        /// Absolute offset.
        /// </summary>
        public byte offSize;

        public void Read(TTF.TTFReader r)
        {
            r.ReadInt(out this.major);
            r.ReadInt(out this.minor);
            r.ReadInt(out this.hdrSize);
            r.ReadInt(out this.offSize);
        }
    }
}