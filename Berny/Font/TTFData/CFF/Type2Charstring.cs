using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PxPre
{
    namespace Berny
    {
        namespace CFF
        {
            /// <summary>
            /// The Type 2 format provides a method for compact encoding of
            /// glyph procedures in an outline font program.Type 2 charstrings
            /// must be used in a CFF (Compact Font Format) or OpenType font
            /// file to create a complete font program.
            /// 
            /// https://wwwimages2.adobe.com/content/dam/acom/en/devnet/font/pdfs/5177.Type2.pdf
            /// </summary>
            public class Type2Charstring
            { 
                List<Operand> program = new List<Operand>();

                public Type2Charstring()
                { }

                public Type2Charstring(byte [] data)
                { 
                    this.program = new List<Operand>();
                    this.Parse(data);
                }

                public bool Parse(byte [] data)
                { 
                    TTF.TTFReaderBytes r = new TTF.TTFReaderBytes(data);
                    while(r.GetPosition() < data.Length)
                    { 
                        Operand o = Operand.ReadType2Op(r);
                        if(o.type == Operand.Type.Error)
                            return false;

                        program.Add(o);
                    }
                    return true;
                }

                Font.Glyph ExecuteProgram()
                { 
                    // The program stack seems to require some "flexibility" so
                    // we forgo an actual stack
                    List<Operand> stack = new List<Operand>();

                    return null;
                }
            }
        }
    }
}