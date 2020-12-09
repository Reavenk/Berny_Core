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
            public class Loader
            {

                public struct Table
                { 
                    public string tag;
                    public uint checksum;
                    public uint offset;
                    public uint length;

                    public void Read(TTFReader r)
                    {
                        this.tag = r.ReadString(4);
                        r.ReadInt( out this.checksum);
                        r.ReadInt(out this.offset);
                        r.ReadInt(out this.length);
                    }
                }

                // https://docs.microsoft.com/en-us/typography/opentype/spec/otff#organization-of-an-opentype-font
                const int FormatTrueType = 0x00010000;
                const int FormatOTF = 0x4F54544F;

                // Table directory
                uint sfntVersion;       // 0x00010000 or 0x4F54544F ('OTTO') — see below.
                ushort numTables;       // Number of tables.
                ushort searchRange;     // Maximum power of 2 less than or equal to numTables, times 16 ((2**floor(log2(numTables))) * 16, where “**” is an exponentiation operator).
                ushort entrySelector;   // Log2 of the maximum power of 2 less than or equal to numTables (log2(searchRange/16), which is equal to floor(log2(numTables))).
                ushort rangeShift;      // numTables times 16, minus searchRange((numTables* 16) - searchRange).


                Dictionary<string, Table> tables = new Dictionary<string, Table>();
                List<Table> records = new List<Table>();

                public bool IsTTF { get => this.sfntVersion == FormatTrueType; }
                public bool IsOTF { get => this.sfntVersion == FormatOTF; }

                public bool Read(string path)
                {
                    TTFReader r = new TTFReader(path);
                    return this.Read(r);
                }

                public bool Read(TTFReader r)
                {
                    r.SetPosition(0);
                    r.ReadInt(out this.sfntVersion);
                    r.ReadInt(out this.numTables);
                    r.ReadInt(out this.searchRange);
                    r.ReadInt(out this.entrySelector);
                    r.ReadInt(out this.rangeShift);

                    for (int i = 0; i < this.numTables; ++i)
                    {
                        Table t = new Table();
                        t.Read(r);
                        this.records.Add(t);
                        this.tables.Add(t.tag, t);
                    }

                    return true;
                }

                public void Clear()
                {
                    this.tables.Clear();
                    this.records.Clear();
                }
            }
        }
    }
}