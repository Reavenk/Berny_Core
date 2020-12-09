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
                /// cmap — Character to Glyph Index Mapping Table
                /// https://docs.microsoft.com/en-us/typography/opentype/spec/cmap
                /// 
                /// This table defines the mapping of character codes to the glyph index 
                /// values used in the font. It may contain more than one subtable, in order 
                /// to support more than one character encoding scheme.
                /// </summary>
                public struct CMap
                {
                    public const string TagName = "cmap";

                    /// <summary>
                    /// The array of encoding records specifies particular encodings and the 
                    /// offset to the subtable for each encoding.
                    /// </summary>
                    public struct EncodingRecord
                    {
                        public ushort platformID;       // Platform ID.
                        public ushort encodingID;       // Platform-specific encoding ID.
                        public uint subtableOffset;     // Byte offset from beginning of table to the subtable for this encoding.
                    }

                    public ushort version;      // Table version number (0).
                    public ushort numTables;    // Number of encoding tables that follow.
                    public List<EncodingRecord> encodingRecords;

                    public void Read(TTFReader r)
                    {
                        r.ReadInt(out this.version);
                        r.ReadInt(out this.numTables);

                        this.encodingRecords = new List<EncodingRecord>();
                        for (int i = 0; i < this.numTables; ++i)
                        {
                            EncodingRecord er = new EncodingRecord();

                            r.ReadInt(out er.platformID);
                            r.ReadInt(out er.encodingID);
                            r.ReadInt(out er.subtableOffset);

                            this.encodingRecords.Add(er);
                        }
                    }
                }
            }
        }
    }
}
