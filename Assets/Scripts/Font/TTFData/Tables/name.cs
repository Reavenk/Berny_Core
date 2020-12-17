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
                /// name — Naming Table
                /// https://docs.microsoft.com/en-us/typography/opentype/spec/name
                /// 
                /// The naming table allows multilingual strings to be associated with the OpenType™ font. 
                /// These strings can represent copyright notices, font names, family names, style names, 
                /// and so on. To keep this table short, the font manufacturer may wish to make a limited 
                /// set of entries in some small set of languages; later, the font can be “localized” and 
                /// the strings translated or added. Other parts of the OpenType font that require these 
                /// strings can refer to them using a language-independent name ID. In addition to language 
                /// variants, the table also allows for platform-specific character-encoding variants. 
                /// Applications that need a particular string can look it up by its platform ID, encoding 
                /// ID, language ID and name ID. Note that different platforms may have different requirements 
                /// for the encoding of strings.
                /// </summary>
                public struct name
                {
                    public struct LangTagRecord
                    {
                        public ushort length;                   // Language-tag string length (in bytes)
                        public ushort langTagOffset;            // Language-tag string offset from start of storage area (in bytes).
                    }

                    /// <summary>
                    /// Each string in the string storage is referenced by a name record. 
                    /// The name record has a multi-part key, to identify the logical type 
                    /// of string and its language or platform-specific implementation variants, 
                    /// plus the location of the string in the string storage.
                    /// </summary>
                    public struct NameRecords
                    {
                        public ushort platformID;       // Platform ID.
                        public ushort encodingID;       // Platform-specific encoding ID.
                        public ushort languageID;       // Language ID.
                        public ushort nameID;           // Name ID.
                        public ushort length;           // String length (in bytes).
                        public ushort stringOffset;     // String offset from start of storage area (in bytes).

                    }

                    public const string TagName = "name";

                    public ushort version;                      // Table version number(=1).
                    public ushort count;                        // Number of name records.
                    public ushort storageOffset;                // Offset to start of string storage (from start of table).
                    public List<string> nameRecord;             // The name records where count is the number of records.

                    public ushort langTagCount;                 // Number of language-tag records.
                    public List<LangTagRecord> langTagRecord;   // The language-tag records where langTagCount is the number of records.

                    public void Read(TTFReader r)
                    {
                        r.ReadInt(out this.version);
                        r.ReadInt(out this.count);
                        r.ReadInt(out this.storageOffset);

                        this.nameRecord = new List<string>();
                        for(int i = 0; i < this.count; ++i)
                            this.nameRecord.Add(r.ReadNameRecord());

                        if (this.version == 1)
                        {
                            r.ReadInt(out this.langTagCount);
                            this.langTagRecord = new List<LangTagRecord>();

                            for (int i = 0; i < this.langTagCount; ++i)
                            {
                                LangTagRecord ltr = new LangTagRecord();
                                r.ReadInt(out ltr.length);
                                r.ReadInt(out ltr.langTagOffset);
                                this.langTagRecord.Add(ltr);
                            }
                        }

                        // There's more data in the header we could load, but it's being
                        // ignored for now.
                    }
                }
            }
        }
    }
}
