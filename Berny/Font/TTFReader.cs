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

                public abstract byte ReadInt8();
                public abstract char ReadUint8();

                public abstract long GetPosition();
                public abstract bool SetPosition(long pos);

                public abstract byte[] ReadBytes(int length);

                public ushort ReadUint16()
                {
                    return (ushort)(this.ReadInt8() << 8 | this.ReadInt8());
                }

                public uint ReadUInt24()
                {
                    return (uint)((this.ReadInt8() << 16) | (this.ReadInt8() << 8) | (this.ReadInt8() << 0));
                }


                public uint ReadUInt32()
                {
                    return (uint)((this.ReadInt8() << 24) | (this.ReadInt8() << 16) | (this.ReadInt8() << 8) | (this.ReadInt8() << 0));
                }

                public short ReadInt16()
                {
                    return (short)(this.ReadInt8() << 8 | this.ReadInt8());
                }

                public int ReadInt32()
                {
                    return (int)((this.ReadInt8() << 24) | (this.ReadInt8() << 16) | (this.ReadInt8() << 8) | (this.ReadInt8() << 0));
                }

                public short ReadFWord()
                {
                    return this.ReadInt16();
                }

                public ushort ReadUFWord()
                {
                    return this.ReadUint16();
                }

                public ushort ReadOffset16()
                {
                    return this.ReadUint16();
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

                public string ReadString(int length)
                {
                    byte[] rbStr = this.ReadBytes(length);
                    return System.Text.ASCIIEncoding.ASCII.GetString(rbStr);
                }

                public string ReadNameRecord()
                {
                    ushort len = this.ReadUint16();
                    return this.ReadString(len);
                }

                public string ReadPascalString()
                {
                    char c = this.ReadUint8();
                    return this.ReadString(c);
                }

                public void ReadInt(out char i)
                {
                    i = this.ReadUint8();
                }

                public void ReadInt(out byte i)
                {
                    i = this.ReadInt8();
                }

                public void ReadInt(out short i)
                {
                    i = this.ReadInt16();
                }

                public void ReadInt(out ushort i)
                {
                    i = this.ReadUint16();
                }

                public void ReadInt(out int i)
                {
                    i = this.ReadInt32();
                }

                public void ReadInt(out uint i)
                {
                    i = this.ReadUInt32();
                }

                public System.DateTime ReadDate()
                {
                    long macTime = this.ReadUInt32() * 0x100000000 + this.ReadUInt32();
                    return new System.DateTime(macTime * 1000);
                }            }
        }
    }
}