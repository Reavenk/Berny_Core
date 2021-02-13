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
    /// VDMX - Vertical Device Metrics
    /// https://docs.microsoft.com/en-us/typography/opentype/spec/vdmx
    /// 
    /// The VDMX table relates to OpenType™ fonts with TrueType outlines. Under Windows, 
    /// the usWinAscent and usWinDescent values from the OS/2 table will be used to determine 
    /// the maximum black height for a font at any given size. Windows calls this distance 
    /// the Font Height. Because TrueType instructions can lead to Font Heights that differ 
    /// from the actual scaled and rounded values, basing the Font Height strictly on the yMax 
    /// and yMin can result in “lost pixels.” Windows will clip any pixels that extend above 
    /// the yMax or below the yMin. In order to avoid grid fitting the entire font to determine 
    /// the correct height, the VDMX table has been defined.
    /// </summary>
    public struct VDMX
    {
        public struct RatioRange
        {
            public byte bCharSet;       // Character set (see below).
            public byte xRatio;         // Value to use for x-Ratio
            public byte yStartRatio;    // Starting y-Ratio value.
            public byte yEndRatio;      // Ending y-Ratio value.

            public void Read(TTFReader r)
            {
                r.ReadInt(out this.bCharSet);
                r.ReadInt(out this.xRatio);
                r.ReadInt(out this.yStartRatio);
                r.ReadInt(out this.yEndRatio);
            }
        }

        public struct VDMXGroup
        { 
            public ushort recs;            // Number of height records in this group
            public byte startsz;           // Starting yPelHeight
            public byte endsz;             // Ending yPelHeight
            public List<vTable> entry;     // The VDMX records

            public void Read(TTFReader r)
            {
                r.ReadInt(out this.recs);
                r.ReadInt(out this.startsz);
                r.ReadInt(out this.endsz);

                this.entry = new List<vTable>();
                for(int i = 0; i < this.recs; ++i)
                { 
                    vTable vt = new vTable();
                    vt.Read(r);
                    this.entry.Add(vt);
                }
            }
        }

        public struct vTable
        { 
            public ushort yPelHeight;           // yPelHeight to which values apply.
            public short yMax;                  // Maximum value (in pels) for this yPelHeight.
            public short yMin;                  // Minimum value (in pels) for this yPelHeight.

            public void Read(TTFReader r)
            { 
                r.ReadInt(out this.yPelHeight);
                r.ReadInt(out this.yMax);
                r.ReadInt(out this.yMin);
            }
        }

        public const string TagName = "VDMX";

        public ushort version;                  // Version number (0 or 1).
        public ushort numRecs;                  // Number of VDMX groups present
        public ushort numRatios;                // Number of aspect ratio groupings
        public List<RatioRange> ratRange;       // Ratio record array.
        public List<ushort> vdmxGroupOffsets;   // Offset from start of this table to the VDMXGroup table for a corresponding RatioRange record.

        public void Read(TTFReader r)
        { 
            r.ReadInt(out this.version);
            r.ReadInt(out this.numRecs);
            r.ReadInt(out this.numRatios);
                        
            this.ratRange = new List<RatioRange>();
            for(int i = 0; i < this.numRatios; ++i)
            { 
                RatioRange rr = new RatioRange();
                rr.Read(r);
                this.ratRange.Add(rr);
            }

            this.vdmxGroupOffsets = new List<ushort>();
            for (int i = 0; i < this.numRatios; ++i)
                this.vdmxGroupOffsets.Add(r.ReadUInt16());
        }
    }
}
