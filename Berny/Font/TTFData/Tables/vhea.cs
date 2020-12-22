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
                /// vhea — Vertical Header Table
                /// https://docs.microsoft.com/en-us/typography/opentype/spec/vhea
                /// 
                /// The vertical header table (tag name: 'vhea') contains information needed 
                /// for vertical layout of Chinese, Japanese, Korean (CJK) and other ideographic 
                /// scripts. In vertical layout, these scripts are written either top to bottom 
                /// or bottom to top. This table contains information that is general to the 
                /// font as a whole. Information that pertains to specific glyphs is given in 
                /// the vertical metrics table (tag name: 'vmtx') described separately. The 
                /// formats of these tables are similar to those for horizontal metrics ('hhea' and 'hmtx').
                /// </summary>
                public struct vhea
                {
                    public const string TagName = "vhea";

                    public ushort majorVersion;             // Version number of the vertical header table; 0x00010000 for version 1.0
                    public ushort minorVersion;             // Version number of the vertical header table; 0x00010000 for version 1.0
                    public short ascent;                    // Distance in FUnits from the centerline to the previous line’s descent.
                    public short descent;                   // Distance in FUnits from the centerline to the next line’s ascent.
                    public short lineGap;                   // Reserved; set to 0
                    public short advanceHeightMax;          // The maximum advance height measurement -in FUnits found in the font. This value must be consistent with the entries in the vertical metrics table.
                    public short minTopSideBearing;         // The minimum top sidebearing measurement found in the font, in FUnits. This value must be consistent with the entries in the vertical metrics table.
                    public short minBottomSideBearing;      // The minimum bottom sidebearing measurement found in the font, in FUnits. This value must be consistent with the entries in the vertical metrics table.
                    public short yMaxExtent;                // Defined as yMaxExtent = max(tsb + (yMax - yMin)).
                    public short caretSlopeRise;            // The value of the caretSlopeRise field divided by the value of the caretSlopeRun Field determines the slope of the caret. A value of 0 for the rise and a value of 1 for the run specifies a horizontal caret. A value of 1 for the rise and a value of 0 for the run specifies a vertical caret. Intermediate values are desirable for fonts whose glyphs are oblique or italic. For a vertical font, a horizontal caret is best.
                    public short caretSlopeRun;             // See the caretSlopeRise field. Value=1 for nonslanted vertical fonts.
                    public short caredOffset;               // The amount by which the highlight on a slanted glyph needs to be shifted away from the glyph in order to produce the best appearance. Set value equal to 0 for nonslanted fonts.
                    public short metricDataFormat;          // Set to 0.
                    public ushort numOfLongMetrics;         // Number of advance heights in the vertical metrics table.

                    public void Read(TTFReader r)
                    {
                        r.ReadInt(out this.majorVersion);
                        r.ReadInt(out this.minorVersion);
                        r.ReadInt(out this.ascent);
                        r.ReadInt(out this.descent);
                        r.ReadInt(out this.lineGap);
                        r.ReadInt(out this.advanceHeightMax);
                        r.ReadInt(out this.minTopSideBearing);
                        r.ReadInt(out this.minBottomSideBearing);
                        r.ReadInt(out this.yMaxExtent);
                        r.ReadInt(out this.caretSlopeRise);
                        r.ReadInt(out this.caretSlopeRun);
                        r.ReadInt(out this.caredOffset);

                        r.ReadInt16();
                        r.ReadInt16();
                        r.ReadInt16();
                        r.ReadInt16();

                        r.ReadInt(out this.metricDataFormat);
                        r.ReadInt(out this.numOfLongMetrics);
                    }
                }
            }
        }
    }
}
