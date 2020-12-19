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
            public class TTFReaderFile : TTFReader
            {
                System.IO.BinaryReader reader = null;
                System.IO.FileStream filestream = null;

                public TTFReaderFile(string path)
                {
                    if (this.Open(path) == false)
                        throw new System.Exception("Could not open file");
                }

                public bool Open(string path)
                {
                    this.Close();

                    this.filestream = System.IO.File.Open(path, System.IO.FileMode.Open);
                    reader = new System.IO.BinaryReader(this.filestream);
                    return true;
                }

                public override bool IsOpen()
                {
                    return this.reader != null;
                }

                public override bool Close()
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

                public override byte ReadInt8()
                {
                    return this.reader.ReadByte();
                }

                public override char ReadUint8()
                {
                    return (char)this.reader.ReadByte();
                }

                public override long GetPosition()
                {
                    return this.filestream.Position;
                }

                public override bool SetPosition(long pos)
                {
                    if (this.filestream == null)
                        return false;

                    return this.filestream.Seek(pos, System.IO.SeekOrigin.Begin) == pos;
                }

                public override byte [] ReadBytes(int length)
                { 
                    return this.reader.ReadBytes(length);
                }
            }
        }
    }
}