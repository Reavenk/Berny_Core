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
                /// kern - Kerning
                /// https://docs.microsoft.com/en-us/typography/opentype/spec/kern
                /// 
                /// The kerning table contains the values that control the inter-character spacing 
                /// for the glyphs in a font. There is currently no system level support for kerning 
                /// (other than returning the kern pairs and kern values). OpenType™ fonts containing 
                /// CFF outlines are not supported by the 'kern' table and must use the GPOS OpenType
                /// Layout table.
                /// </summary>
                public struct kern
                {
                    public struct SubTable
                    {
                        [System.Flags]
                        public enum Flags
                        {
                            horizontal      = (1 << 0),
                            minimum         = (1 << 1),
                            crossstream     = (1 << 2),
                            overrideacc     = (1 << 3) // the reference calls this "override", but that's a C# keyword
                        }

                        public ushort version;
                        public ushort length;
                        public char flags;
                        public char format;

                        public void Read(TTFReader r)
                        {
                            r.ReadInt(out this.version);
                            r.ReadInt(out this.length);
                            r.ReadInt(out this.flags);
                            r.ReadInt(out this.format);
                        }
                    }

                    public struct Format0
                    {
                        public ushort nPairs;
                        public ushort searchRange;
                        public ushort entrySelector;
                        public ushort rangeShift;
                        public List<KerningPair> kerningPairs;

                        public void Read(TTFReader r)
                        {
                            r.ReadInt(out this.nPairs);
                            r.ReadInt(out this.searchRange);
                            r.ReadInt(out this.entrySelector);
                            r.ReadInt(out this.rangeShift);

                            this.kerningPairs = new List<KerningPair>();
                            for (int i = 0; i < this.nPairs; ++i)
                            {
                                KerningPair kp = new KerningPair();
                                kp.Read(r);
                                this.kerningPairs.Add(kp);
                            }
                        }
                    }

                    public struct KerningPair
                    {
                        public ushort left;
                        public ushort right;
                        public short value;

                        public void Read(TTFReader r)
                        {
                            r.ReadInt(out this.left);
                            r.ReadInt(out this.right);
                            r.ReadInt(out this.value);
                        }
                    }

                    public const string TagName = "kern";

                    public short version;
                    public short nTables;
                }
            }
        }
    }
}
