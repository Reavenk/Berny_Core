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

namespace PxPre.Berny.TTF.Table
{
    /// <summary>
    /// CFF — Compact Font Format Table
    /// https://docs.microsoft.com/en-us/typography/opentype/spec/cff
    /// https://wwwimages2.adobe.com/content/dam/acom/en/devnet/font/pdfs/5176.CFF.pdf
    /// https://github.com/Pomax/the-cff-table
    /// OpenType fonts with TrueType outlines use a glyph index to specify and access 
    /// glyphs within a font; e.g., to index within the 'loca' table and thereby access 
    /// glyph data in the 'glyf' table. This concept is retained in OpenType CFF fonts,
    /// except that glyph data is accessed through the CharStrings INDEX of the 'CFF ' 
    /// table.
    /// </summary>
    public struct CFF
    { 
                    
        // copied from documentation, currently UNUSED
        public enum EncodingID
        { 
            Standard = 0,
            Expert = 1
        }

        // // copied from documentation, currently UNUSED
        public enum CharsetID
        { 
            ISOAdobe = 0,
            Expert = 1,
            ExpertSubset = 2
        }

        // copied from documentation, currently UNUSED
        public struct Format0
        { 
            public byte format;
            public byte nCodes;
            public byte [] code;
        }

        // copied from documentation, currently UNUSED
        public struct Format1
        { 
            public byte format;
            public byte nRanges;
            public List<Range1> range1s;
        }

        // copied from documentation, currently UNUSED
        public struct Range1
        {
            public byte first;
            public byte nLeft;
        }

        // copied from documentation, currently UNUSED
        public struct SupplementalEncoding
        { 
            byte nSups;
            public List<SupplementFormat> supplement;
        }

        // copied from documentation, currently UNUSED
        public struct SupplementFormat
        {
            public byte code;
            public string glyphSID;
        }

        // copied from documentation, currently UNUSED
        public struct CharsetFormat0
        { 
            public byte format;
            public List<int> glyph;
        }

        // copied from documentation, currently UNUSED
        public struct CharsetFormat1
        { 
            public byte format;
            public List<CharsetRange1> range1;
        }

        // copied from documentation, currently UNUSED
        public struct CharsetRange1
        { 
            public int first;
            public byte nLeft;
        }

        // copied from documentation, currently UNUSED
        public struct CharsetFormat2
        { 
            public byte format;
            public List<CharsetRange2> range2;
        }

        // copied from documentation, currently UNUSED
        public struct CharsetRange2
        { 
            public int first;
            public byte nLeft;
        }
        // CFF Data Type

        /// <summary>
        /// The TTF/OTF Table identifier
        /// </summary>
        public const string TagName = "CFF ";

        /// <summary>
        /// The CFF file information.
        /// </summary>
        public Berny.CFF.CFFFile data;

        /// <summary>
        /// Read the CFF segment in an TTF/OTF file.
        /// </summary>
        /// <param name="r">The reader for the TTF/OTF file.</param>
        /// <remarks>Assumes the reader parameter is set to the start of the CFF section
        /// before invoking.</remarks>
        public void Read(TTFReader r)
        { 
            this.data = new Berny.CFF.CFFFile();
            this.data.Read(r);
        }
    }
}