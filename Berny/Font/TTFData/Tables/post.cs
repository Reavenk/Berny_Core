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
                /// post — PostScript Table
                /// https://docs.microsoft.com/en-us/typography/opentype/spec/post
                /// 
                /// This table contains additional information needed to use TrueType or 
                /// OpenType™ fonts on PostScript printers. This includes data for the 
                /// FontInfo dictionary entry and the PostScript names of all the glyphs. 
                /// For more information about PostScript names, see the Adobe Glyph List 
                /// Specification.
                /// </summary>
                public struct post
                {
                    public const string TagName = "post";

                    public ushort minorVersion;
                    public ushort majorVersion;
                    public float italicAngle;
                    public short underlinePosition;
                    public short underlineThickness;
                    public uint isFixedPitch;
                    public uint mimMemType42;
                    public uint maxMemType42;
                    public uint minMemType1;
                    public uint maxMemType1;

                    // Version 2
                    public ushort numGlyphs;
                    public List<ushort> glyphNameIndex;
                    public List<string> stringData;

                    // Version 2.5
                    // numGlyphs
                    public List<byte> offset;

                    public void Read(TTFReader r, int end)
                    {
                        r.ReadInt(out this.minorVersion);
                        r.ReadInt(out this.majorVersion);
                        this.italicAngle = r.ReadFixed();
                        r.ReadInt(out this.underlinePosition);
                        r.ReadInt(out this.underlineThickness);
                        r.ReadInt(out this.isFixedPitch);
                        r.ReadInt(out this.mimMemType42);
                        r.ReadInt(out this.maxMemType42);
                        r.ReadInt(out this.minMemType1);
                        r.ReadInt(out this.maxMemType1);

                        if (this.majorVersion == 2 && this.minorVersion == 0)
                        {
                            r.ReadInt(out this.numGlyphs);

                            this.glyphNameIndex = new List<ushort>();
                            for(int i = 0; i < this.numGlyphs; ++i)
                                this.glyphNameIndex.Add(r.ReadUint16());

                            this.stringData = new List<string>();
                            while(r.GetPosition() < end)
                                this.stringData.Add(r.ReadPascalString());

                            //for(int i = 0; i < )
                        }
                        else if (this.majorVersion == 2 && this.minorVersion == 5)
                        {
                            r.ReadInt(out this.numGlyphs);

                            this.offset = new List<byte>();
                            for(int i = 0; i < this.numGlyphs; ++i)
                                this.offset.Add(r.ReadInt8());
                        }
                        else if (this.majorVersion == 3 && this.minorVersion == 0)
                        {} // Do nothing
                    }
                }
            }
        }
    }
}
