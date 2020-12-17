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
        public class TTFReader
        {
            System.IO.BinaryReader reader = null;
            System.IO.FileStream filestream = null;

            public TTFReader(string path)
            { 
                if(this.Open(path) == false)
                    throw new System.Exception("Could not open file");
            }

            public bool IsOpen()
            { 
                return this.reader != null;
            }

            public bool Open(string path)
            {
                this.Close();

                this.filestream = System.IO.File.Open(path, System.IO.FileMode.Open);
                reader = new System.IO.BinaryReader(this.filestream);
                return true;
            }

            public bool Close()
            {
                if (this.filestream != null)
                {
                    this.filestream.Close();
                    this.filestream = null;
                    this.reader = null;

                    return true;
                }
                return false;
            }

            public byte ReadInt8()
            {
                return this.reader.ReadByte();
            }

            public char ReadUint8()
            {
                return (char)this.reader.ReadByte();
            }

            public ushort ReadUint16()
            {
                return (ushort)(this.reader.ReadByte() << 8 | this.reader.ReadByte());
            }

            public uint ReadUInt24()
            {
                return (uint)((this.reader.ReadByte() << 16) | (this.reader.ReadByte() << 8) | (this.reader.ReadByte() << 0));
            }


            public uint ReadUInt32()
            {
                return (uint)((this.reader.ReadByte() << 24) | (this.reader.ReadByte() << 16) | (this.reader.ReadByte() << 8) | (this.reader.ReadByte() << 0));
            }

            public short ReadInt16()
            {
                return (short)(this.reader.ReadByte() << 8 | this.reader.ReadByte());
            }

            public int ReadInt32()
            {
                return (int)((this.reader.ReadByte() << 24) | (this.reader.ReadByte() << 16) | (this.reader.ReadByte() << 8) | (this.reader.ReadByte() << 0));
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
                byte[] rbStr = this.reader.ReadBytes(length);
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
            }

            public long GetPosition()
            {
                return this.filestream.Position;
            }

            public bool SetPosition(long pos)
            {
                if (this.filestream == null)
                    return false;

                return this.filestream.Seek(pos, System.IO.SeekOrigin.Begin) == pos;
            }
        }
    }
}