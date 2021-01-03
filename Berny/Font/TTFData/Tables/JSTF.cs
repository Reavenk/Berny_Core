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

namespace PxPre
{
    namespace Berny
    {
        namespace TTF
        {
            namespace Table
            {
                /// <summary>
                /// JSTF — Justification Table
                /// https://docs.microsoft.com/en-us/typography/opentype/spec/jstf
                /// 
                /// The Justification table (JSTF) provides font developers with additional control 
                /// over glyph substitution and positioning in justified text. Text-processing clients 
                /// now have more options to expand or shrink word and glyph spacing so text fills 
                /// the specified line length.
                /// </summary>
                public struct JSTF
                {
                    public struct JstfScriptRecord
                    { 
                        public string jstfScriptTag;        // 4-byte JstfScript identification
                        public ushort jstfScriptOffset;     // Offset to JstfScript table, from beginning of JSTF Header

                        public void Read(TTFReader r)
                        {
                            this.jstfScriptTag = r.ReadString(4);
                            r.ReadInt(out this.jstfScriptOffset);
                        }
                    }

                    public const string TagName = "JSTF";

                    public ushort majorVersion;             // Major version of the JSTF table, = 1
                    public ushort minorVersion;             // Minor version of the JSTF table, = 0
                    public ushort jsftScriptCount;          // Number of JstfScriptRecords in this table
                    public List<JstfScriptRecord> jstfScriptRecords; // Array of JstfScriptRecords, in alphabetical order by jstfScriptTag

                    public void Read(TTFReader r)
                    { 
                        r.ReadInt(out this.majorVersion);
                        r.ReadInt(out this.minorVersion);
                        r.ReadInt(out this.jsftScriptCount);

                        this.jstfScriptRecords = new List<JstfScriptRecord>();
                        for(int i = 0; i < jsftScriptCount; ++i)
                        {
                            JstfScriptRecord rec = new JstfScriptRecord();
                            rec.Read(r);
                            this.jstfScriptRecords.Add(rec);
                        }
                    }
                }
            }
        }
    }
}