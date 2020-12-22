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
                /// hdmx — Horizontal Device Metrics
                /// https://docs.microsoft.com/en-us/typography/opentype/spec/hdmx
                /// 
                /// The 'hdmx' table relates to OpenType™ fonts with TrueType outlines. 
                /// The Horizontal Device Metrics table stores integer advance widths 
                /// scaled to particular pixel sizes. This allows the font manager to 
                /// build integer width tables without calling the scaler for each glyph. 
                /// Typically this table contains only selected screen sizes. This table 
                /// is sorted by pixel size. The checksum for this table applies to both 
                /// subtables listed.
                /// </summary>
                public struct hdmx
                {
                    public struct DeviceRecord
                    {
                        public char pixelSize;          // Pixel size for following widths (as ppem).
                        public char maxWidth;           // Maximum width.
                        public List<char > widths;      // Array of widths (numGlyphs is from the 'maxp' table).
                    }

                    public const string TagName = "hdmx";

                    public ushort version;
                    public ushort numRecords;
                    public uint sizeDeviceRecord;
                    public List<DeviceRecord> records;

                    public void Read(TTFReader r, int numGlyphs)
                    {
                        r.ReadInt(out this.version);
                        r.ReadInt(out this.numRecords);
                        r.ReadInt(out this.sizeDeviceRecord);

                        this.records = new List<DeviceRecord>();
                        for (int i = 0; i < this.numRecords; ++i)
                        {
                            DeviceRecord dr = new DeviceRecord();

                            r.ReadInt(out dr.pixelSize);
                            r.ReadInt(out dr.maxWidth);
                            //
                            dr.widths = new List<char>();
                            for(int j = 0; j < numGlyphs; ++j)
                                dr.widths.Add(r.ReadUint8());

                            this.records.Add(dr);
                        }
                        
                    }
                }

            }
        }
    }
}
