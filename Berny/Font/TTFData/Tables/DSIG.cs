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

using System.Collections.Generic;

namespace PxPre
{
    namespace Berny
    {
        namespace TTF
        {
            namespace Table
            {
                /// <summary>
                /// DSIG — Digital Signature Table
                /// https://docs.microsoft.com/en-us/typography/opentype/spec/dsig
                /// 
                /// The DSIG table contains the digital signature of the OpenType™ font. Signature formats 
                /// are widely documented and rely on a key pair architecture. Software developers, or publishers 
                /// posting material on the Internet, create signatures using a private key. Operating systems or 
                /// applications authenticate the signature using a public key.
                /// 
                /// The W3C and major software and operating system developers have specified security standards 
                /// that describe signature formats, specify secure collections of web objects, and recommend 
                /// authentication architecture. OpenType fonts with signatures will support these standards.
                /// </summary>
                public struct DSIG
                {
                    /// <summary>
                    /// The DSIG header has an array of signature records, which specifying the format and 
                    /// offset of signature blocks.
                    /// </summary>
                    public struct SignatureRecord
                    { 
                        public uint format;                 // Format of the signature
                        public uint length;                 // Length of signature in bytes
                        public uint signatureBlockOffset;   // Offset to the signature block from the beginning of the (DSIG) table

                        public void Read(TTFReader r)
                        {
                            r.ReadInt(out this.format);
                            r.ReadInt(out this.length);
                            r.ReadInt(out this.signatureBlockOffset);
                        }
                    }

                    /// <summary>
                    /// Signatures are contained in one or more signature blocks. Signature blocks may have 
                    /// various formats; currently one format is defined. The format identifier specifies 
                    /// both the format of the signature block, as well as the hashing algorithm used to 
                    /// create and authenticate the signature.
                    /// </summary>
                    public struct SignatureBlockFormat1
                    { 
                        public ushort reserved1;            // Reserved for future use; set to zero.
                        public ushort reserved2;            // Reserved for future use; set to zero.
                        public uint signatureLength;        // Length (in bytes) of the PKCS#7 packet in the signature field.
                        public char [] signature;           // PKCS#7 packet

                        public void Read(TTFReader r)
                        {
                            r.ReadInt(out this.reserved1);
                            r.ReadInt(out this.reserved2);
                            r.ReadInt(out this.signatureLength);
                        }
                    }

                    [System.Flags]
                    public enum Permissions
                    { 
                        Sealed      = 1 << 0, // cannot be resigned
                        Reserved_1  = 1 << 1,
                        Reserved_2  = 1 << 2,
                        Reserved_3  = 1 << 3,
                        Reserved_4  = 1 << 4,
                        Reserved_5  = 1 << 5,
                        Reserved_6  = 1 << 6,
                        Reserved_7  = 1 << 7,
                        Reserved = Reserved_1 | Reserved_2 | Reserved_3 | Reserved_4 | Reserved_5 | Reserved_6 | Reserved_7

                    }

                    public const string TagName = "DSIG";

                    public uint version;                    // Format of the signature
                    public ushort numSignatures;            // Length of signature in bytes
                    public ushort flags;                    // Offset to the signature block from the beginning of the table

                    public List<SignatureRecord> signatureRecords;  // Array of signature records

                    public void Read(TTFReader r)
                    {
                        r.ReadInt(out this.version);
                        r.ReadInt(out this.numSignatures);
                        r.ReadInt(out this.flags);

                        this.signatureRecords = new List<SignatureRecord>();
                        for(int i = 0; i < this.numSignatures; ++i)
                        {
                            SignatureRecord sr = new SignatureRecord();
                            sr.Read(r);
                            this.signatureRecords.Add(sr);
                        }
                    }
                }
            }
        }
    }
}
