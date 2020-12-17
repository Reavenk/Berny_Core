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
                /// hhea — Horizontal Header Table
                /// https://docs.microsoft.com/en-us/typography/opentype/spec/hhea
                /// 
                /// This table contains information for horizontal layout. The 
                /// values in the minRightSidebearing, minLeftSideBearing and 
                /// xMaxExtent should be computed using only glyphs that have 
                /// contours. Glyphs with no contours should be ignored for the 
                /// purposes of these calculations. All reserved areas must be 
                /// set to 0.
                /// </summary>
                public struct hhea
                {
                    public const string TagName = "hhea";
                    
                    public ushort majorVersion;         // Major version number of the horizontal header table — set to 1.
                    public ushort minorVersion;         // Minor version number of the horizontal header table — set to 0.
                    public short ascender;              // Apple specific. See https://developer.apple.com/fonts/TrueType-Reference-Manual/RM06/Chap6hhea.html
                    public short descender;             // Apple specific. See https://developer.apple.com/fonts/TrueType-Reference-Manual/RM06/Chap6hhea.html
                    public short lineGap;               // Typographic line gap. Negative LineGap values are treated as zero in some legacy platform implementations.
                    public ushort advanceWidthMax;      // Maximum advance width value in 'hmtx' table.
                    public short minLeftSideBearing;    // Minimum left sidebearing value in 'hmtx' table for glyphs with contours (empty glyphs should be ignored).
                    public short minRightSideBearing;   // Minimum right sidebearing value; calculated as min(aw - (lsb + xMax - xMin)) for glyphs with contours (empty glyphs should be ignored).
                    public short xMaxExtent;            // Max(lsb + (xMax - xMin)).
                    public short caretSlopeRise;        // Used to calculate the slope of the cursor (rise/run); 1 for vertical.
                    public short caretSlopeRun;         // 0 for vertical.
                    public short caredOffset;           // The amount by which a slanted highlight on a glyph needs to be shifted to produce the best appearance. Set to 0 for non-slanted fonts

                    // short reserved_0;
                    // short reserved_1;
                    // short reserved_2;
                    // short reserved_3;

                    public short metricDataFormat;      // 0 for current format.
                    public ushort numberOfHMetrics;     // Number of hMetric entries in 'hmtx' table

                    public void Read(TTFReader r)
                    {
                        r.ReadInt(out this.majorVersion);
                        r.ReadInt(out this.minorVersion);
                        r.ReadInt(out this.ascender);
                        r.ReadInt(out this.descender);
                        r.ReadInt(out this.lineGap);
                        r.ReadInt(out this.advanceWidthMax);
                        r.ReadInt(out this.minLeftSideBearing);
                        r.ReadInt(out this.minRightSideBearing);
                        r.ReadInt(out this.xMaxExtent);
                        r.ReadInt(out this.caretSlopeRise);
                        r.ReadInt(out this.caretSlopeRun);
                        r.ReadInt(out this.caredOffset);

                        // Eat up for reserved
                        r.ReadInt16();
                        r.ReadInt16();
                        r.ReadInt16();
                        r.ReadInt16();

                        r.ReadInt(out this.metricDataFormat);
                        r.ReadInt(out this.numberOfHMetrics);
                    }
                }
            }
        }
    }
}