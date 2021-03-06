﻿// MIT License
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

namespace PxPre.Berny.TTF
{
    /// <summary>
    /// Implementation of TTFReader for a byte array.
    /// </summary>
    public class TTFReaderBytes : TTFReader
    {
        /// <summary>
        /// The read position.
        /// </summary>
        long pos = 0;

        /// <summary>
        /// The data buffer to read from.
        /// </summary>
        byte [] data;

        /// <summary>
        /// The state variable to simulate closing the reader.
        /// </summary>
        bool closed = true;

        public override bool AtEnd()
        {
            return this.pos >= data.Length;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="data">The data buffer to read from.</param>
        public TTFReaderBytes(byte [] data)
        { 
            this.data = data;
            this.closed = false;
        }

        public override bool IsOpen()
        { 
            return this.closed == false;
        }

        public override bool Close()
        { 
            this.closed = true;
            return true;
        }

        public override sbyte ReadInt8()
        { 
            sbyte ret = (sbyte)this.data[this.pos];
            ++this.pos;
            return ret;
        }

        public override byte ReadUInt8()
        { 
            byte ret = this.data[this.pos];
            ++this.pos;
            return ret;
        }

        public override long GetPosition()
        { 
            return this.pos;
        }

        public override bool SetPosition(long newPos)
        { 
            this.pos = newPos;
            return true;
        }

        public override byte[] ReadBytes(int length)
        { 
            byte [] ret = new byte[length];
            System.Buffer.BlockCopy(this.data, (int)this.pos, ret, 0, length);

            this.pos += length;

            return ret;
        }
    }
}