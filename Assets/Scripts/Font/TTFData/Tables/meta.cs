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
                /// meta — Metadata Table
                /// https://docs.microsoft.com/en-us/typography/opentype/spec/meta
                /// 
                /// The metadata table contains various metadata values for the font. Different 
                /// categories of metadata are identified by four-character tags. Values for different 
                /// categories can be either binary or text.
                /// </summary>
                public struct meta
                {
                    public const string tagApple            = "appl";   // Reserved — used by Apple.
                    public const string tagBild             = "bild";   // Reserved — used by Apple.
                    public const string tagDesignLang       = "dlng";   // Indicates languages and/or scripts for the user audiences that the font was primarily designed for. Only one instance is used. See below for additional details.
                    public const string tagSupported        = "slng";   // Indicates languages and/or scripts that the font is declared to be capable of supporting. Only one instance is used. See below for additional details.

                    public struct DataMap
                    {
                        public string tag;          // A tag indicating the type of metadata.
                        public int dataOffset;      // Offset in bytes from the beginning of the metadata table to the data for this tag.
                        public uint dataLength;     // Length of the data, in bytes. The data is not required to be padded to any byte boundary.
                    }

                    public const string TagName = "meta";

                    public uint version;                // Version number of the metadata table — set to 1.
                    public uint flags;                  // Flags — currently unused; set to 0.
                    public uint reserved;               // Not used; should be set to 0.
                    public uint dataMapsCount;          // The number of data maps in the table.
                    public List<DataMap> dataMaps;      // Array of data map records.

                    public void Read(TTFReader r)
                    {
                        r.ReadInt(out this.version);
                        r.ReadInt(out this.flags);
                        r.ReadInt(out this.reserved);
                        r.ReadInt(out this.dataMapsCount);

                        this.dataMaps = new List<DataMap>();
                        for (int i = 0; i < this.dataMapsCount; ++i)
                        {
                            DataMap dm = new DataMap();
                            dm.tag = r.ReadString(4);
                            r.ReadInt(out dm.dataOffset);
                            r.ReadInt(out dm.dataLength);
                            this.dataMaps.Add(dm);
                        }
                    }
                }
            }
        }
    }
}
