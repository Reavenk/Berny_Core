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
            public abstract class TTFReader
            {
                public abstract bool IsOpen();
                public abstract bool Close();

                public abstract sbyte ReadInt8();
                public abstract byte ReadUInt8();

                public abstract long GetPosition();
                public abstract bool SetPosition(long pos);

                public abstract byte[] ReadBytes(int length);

                public abstract bool AtEnd();

                public ushort ReadUInt16()
                {
                    return (ushort)(this.ReadUInt8() << 8 | this.ReadUInt8());
                }

                public uint ReadUInt24()
                {
                    return (uint)((this.ReadUInt8() << 16) | (this.ReadUInt8() << 8) | (this.ReadUInt8() << 0));
                }

                public uint ReadUInt32()
                {
                    return (uint)((this.ReadUInt8() << 24) | (this.ReadUInt8() << 16) | (this.ReadUInt8() << 8) | (this.ReadUInt8() << 0));
                }

                public short ReadInt16()
                {
                    return (short)(this.ReadUInt8() << 8 | this.ReadUInt8());
                }

                public int ReadInt32()
                {
                    return (int)((this.ReadUInt8() << 24) | (this.ReadUInt8() << 16) | (this.ReadUInt8() << 8) | (this.ReadUInt8() << 0));
                }

                public short ReadFWord()
                {
                    return this.ReadInt16();
                }

                public ushort ReadUFWord()
                {
                    return this.ReadUInt16();
                }

                public ushort ReadOffset16()
                {
                    return this.ReadUInt16();
                }

                public int ReadOffset32()
                {
                    return this.ReadOffset32();
                }

                public float ReadFDot14()
                {
                    return (float)this.ReadInt16() / (float)(1 << 14);
                }

                public float ReadFixed()
                {
                    return (float)this.ReadInt32() / (float)(1 << 16);
                }

                /// <summary>
                /// Read an ASCII string of a known length from file.
                /// </summary>
                /// <param name="length">The length of the string.</param>
                /// <returns>The string read from the data stream.</returns>
                public string ReadString(int length)
                {
                    byte[] rbStr = this.ReadBytes(length);
                    return System.Text.ASCIIEncoding.ASCII.GetString(rbStr);
                }

                /// <summary>
                /// Read a record string from file. A record string is a pascal
                /// string with a 2 byte length.
                /// </summary>
                /// <returns>The string read from the data stream.</returns>
                public string ReadNameRecord()
                {
                    ushort len = this.ReadUInt16();
                    return this.ReadString(len);
                }

                /// <summary>
                /// Read a string from file in pascal format - where the first byte
                /// defines the length of the string, directly followed by the ASCII
                /// data.
                /// </summary>
                /// <returns>The string read from the datastream.</returns>
                public string ReadPascalString()
                {
                    byte c = this.ReadUInt8();
                    return this.ReadString(c);
                }

                // A strategy used in reading is to create file member variables
                // of the correct byte width and "signed-ness", and just use
                // ReadInt(out * i) and let the overloading resolve figure out
                // the proper Read*() function.

                /// <summary>
                /// Overload of ReadInt() for 1 byte unsigned int.
                /// </summary>
                /// <param name="i">The output variable.</param>
                public void ReadInt(out sbyte i)
                {
                    i = this.ReadInt8();
                }

                /// <summary>
                /// Overload of ReadInt() for 1 byte signed int.
                /// </summary>
                /// <param name="i">The output variable.</param>
                public void ReadInt(out byte i)
                {
                    i = this.ReadUInt8();
                }

                /// <summary>
                /// Overload of ReadInt() for 2 byte signed int.
                /// </summary>
                /// <param name="i">The output variable.</param>
                public void ReadInt(out short i)
                {
                    i = this.ReadInt16();
                }

                /// <summary>
                /// Overload of ReadInt() for 2 byte unsigned int.
                /// </summary>
                /// <param name="i">The output variable.</param>
                public void ReadInt(out ushort i)
                {
                    i = this.ReadUInt16();
                }

                /// <summary>
                /// Overload of ReadInt() for 4 byte signed int.
                /// </summary>
                /// <param name="i">The output variable.</param>
                public void ReadInt(out int i)
                {
                    i = this.ReadInt32();
                }

                /// <summary>
                /// Overload of ReadInt() for 4 byte unsigned int.
                /// </summary>
                /// <param name="i">The output variable.</param>
                public void ReadInt(out uint i)
                {
                    i = this.ReadUInt32();
                }

                public System.DateTime ReadDate()
                {
                    long macTime = this.ReadUInt32() * 0x100000000 + this.ReadUInt32();
                    return new System.DateTime(macTime * 1000);
                }
            }
        }
    }
}