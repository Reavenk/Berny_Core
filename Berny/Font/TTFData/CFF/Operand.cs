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
        namespace CFF
        {
            public struct Operand
            {
                public enum Type
                {
                    Int,
                    Real,
                    Operator,
                    Error
                }

                public enum Type2Op
                { 
                    HStem       = 1,    // Hint operator
                    VStem       = 3,    // Hint operator
                    VMoveTo     = 4,
                    RLineTo     = 5,
                    HLineTo     = 6,
                    VLineTo     = 7,
                    RRCurveTo   = 8,
                    DotSection  = (12<<8)|0, //Deprecated
                    And         = (12<<8)|3,
                    Or          = (12<<8)|4,
                    Not         = (12<<8)|5,
                    Abs         = (12<<8)|9,
                    Add         = (12<<8)|10,
                    Sub         = (12<<8)|11,
                    Div         = (12<<8)|12,
                    Neg         = (12<<8)|14,
                    Eq          = (12<<8)|15,
                    Drop        = (12<<8)|18,
                    Put         = (12<<8)|20,
                    Get         = (12<<8)|21,
                    Ifelse      = (12<<8)|22,
                    Random      = (12<<8)|23,
                    Mul         = (12<<8)|24,
                    Sqrt        = (12<<8)|26,
                    Dup         = (12<<8)|27,
                    Exch        = (12<<8)|28,
                    Index       = (12<<8)|29,
                    Roll        = (12<<8)|30,
                    Flex        = (12<<8)|35,
                    Flex1       = (12<<8)|37,
                    HFlex       = (12<<8)|34,
                    HFlex1      = (12<<36)|36,
                    CallSubR    = 10,
                    Return      = 11,
                    EndChar     = 14,
                    HStemHM     = 18,   // Hint operator
                    HintMask    = 19,   // + mask byte  - Hint operator
                    CntrMask    = 20,   // + mask byte  - Hint operator
                    VStemHM     = 23,   // Hint operator
                    RMoveTo     = 21,
                    HMoveTo     = 22,
                    RCurveLine  = 24,
                    RLineCurve  = 25,
                    VVCurveTo   = 26,
                    HHCurveTo   = 27,
                    CallGSubR   = 29,
                    VHCurveTo   = 30,
                    HVCurveTo   = 31,
                }

                public Type type;

                // If this was C++, this would be a union.
                public int intVal;
                public float realVal;

                static bool IsB0Operator(byte b0)
                {
                    return b0 <= 21;
                }

                static bool IsB0Real(byte b0)
                {
                    return b0 == 30;
                }

                static bool IsB0Integer(byte b0)
                {
                    return b0 == 28 || b0 == 29 || b0 == 30 || (b0 >= 32 && b0 <= 254);
                }

                static public void ConvertDeltasToAbsolute(List<int> lst)
                {
                    for (int i = 1; i < lst.Count; ++i)
                    {
                        lst[i] += lst[i - 1];
                    }
                }

                static public void ConvertDeltasToAbsolute(int[] ri)
                {
                    for (int i = 1; i < ri.Length; ++i)
                    {
                        ri[i] += ri[i - 1];
                    }
                }

                public static List<int> ToIntList(IEnumerable<Operand> vals)
                {
                    List<int> ret = new List<int>();

                    foreach(Operand tv in vals)
                        ret.Add(tv.GetInt());

                    return ret;
                }

                public static int[] ToIntArray(IEnumerable<Operand> vals)
                {
                    return ToIntList(vals).ToArray();
                }

                public static List<float> ToFloatList(IEnumerable<Operand> vals)
                {
                    List<float> ret = new List<float>();

                    foreach(Operand tv in vals)
                        ret.Add(tv.GetReal());

                    return ret;
                }

                public static float[] ToFloatArray(IEnumerable<Operand> vals)
                {
                    return ToFloatList(vals).ToArray();
                }

                public Operand(int val, Type t)
                { 
                    this.type = t;
                    this.realVal = val;
                    this.intVal = val;
                }

                public Operand(int val)
                {
                    this.type = Type.Int;
                    this.realVal = val;
                    this.intVal = val;
                }

                public Operand(float val)
                { 
                    this.type = Type.Real;
                    this.realVal = val;
                    this.intVal = (int)val;
                }

                public static Operand Error()
                { 
                    return new Operand(0, Type.Error);
                }

                public int GetInt()
                {
                    if (this.type != Type.Real)
                        return this.intVal;

                    return (int)this.realVal;

                }

                public float GetReal()
                {
                    if (this.type == Type.Real)
                        return this.realVal;

                    return (float)this.intVal;
                }

                public static Operand Read(TTF.TTFReader r)
                {
                    // The results are all signed, so might as well
                    // work on an int level from the beginning.

                    // Personal note - this compressed number representation is
                    // pure insanity.
                    // (12/24/2020)

                    byte b0 = r.ReadUInt8();
                    if(b0 <= 21)
                    { 
                        if(b0 == 12)
                            return new Operand((b0 << 8) | r.ReadUInt8(), Type.Operator);
                        else
                            return new Operand(b0, Type.Operator);
                    }
                    else if(b0 == 30)
                    {
                        // AUGHHHHHHHHHH! Who thinks of this!? The code isn't going to
                        // even try to be clever or optimized, we're just "getting by"
                        // with these floating point values...

                        string str = "";
                        bool cont = true;
                        while (cont)
                        {
                            const int upperBitMask = (1 << 4) | (1 << 5) | (1 << 6) | (1 << 7);
                            const int lowerBitMask = (1 << 0) | (1 << 1) | (1 << 2) | (1 << 3);

                            int c = r.ReadUInt8();

                            int upperBit = (c & upperBitMask) >> 4;
                            int lowerBit = c & lowerBitMask;

                            foreach (int i in new int[] { upperBit, lowerBit })
                            {
                                switch (i)
                                {
                                    case 0: str += '0'; break;
                                    case 1: str += '1'; break;
                                    case 2: str += '2'; break;
                                    case 3: str += '3'; break;
                                    case 4: str += '4'; break;
                                    case 5: str += '5'; break;
                                    case 6: str += '6'; break;
                                    case 7: str += '7'; break;
                                    case 8: str += '8'; break;
                                    case 9: str += '9'; break;
                                    case 10: str += '.'; break;      // a
                                    case 11: str += 'E'; break;      // b
                                    case 12: str += "E-"; break;     // c
                                    case 13: break;                  // d (reserved)
                                    case 14: str += '-'; break;      // e
                                    case 15:                        // f
                                        cont = false;
                                        break;
                                }
                            }
                        }
                        float f;
                        float.TryParse(str, out f);
                        return new Operand(f);
                    }
                    if (b0 == 28)
                    {
                        int b1 = r.ReadUInt8();
                        int b2 = r.ReadUInt8();
                        return new Operand(b1 << 8 | b2);
                    }
                    else if (b0 == 29)
                    {
                        int b1 = r.ReadUInt8();
                        int b2 = r.ReadUInt8();
                        int b3 = r.ReadUInt8();
                        int b4 = r.ReadUInt8();
                        return new Operand((b1 << 24) | (b2 << 16) | (b3 << 8) | (b4 << 0));
                    }
                    else if (b0 >= 32 && b0 <= 246)
                    {
                        return new Operand(b0 - 139);
                    }
                    else if (b0 >= 247 && b0 <= 250)
                    {
                        int b1 = r.ReadUInt8();
                        return new Operand((b0 - 247) * 256 + b1 + 108);
                    }
                    else if (b0 >= 251 && b0 <= 254)
                    {
                        int b1 = r.ReadUInt8();
                        return new Operand(-(b0 - 251) * 256 - b1 - 108);
                    }

                    return Error();
                }

                public static Operand ReadType2Op(TTF.TTFReader r)
                { 
                    byte b0 = r.ReadUInt8();
                    if(b0 <= 11)
                        return new Operand(b0, Type.Operator);

                    if(b0 == 12)
                    { 
                        byte b1 = r.ReadUInt8();
                        return new Operand((b0<<8)|b1, Type.Operator);
                    }
                    if(b0 <= 18)
                        return new Operand(b0, Type.Operator);
                    // hintmask and cntrmask
                    if(b0 <= 20)
                        return new Operand(b0, Type.Operator);
                    if (b0 <= 27)
                        return new Operand(b0, Type.Operator);
                    if(b0 == 28)
                    {
                        int b1 = r.ReadUInt8();
                        int b2 = r.ReadUInt8();
                        return new Operand(b1 << 8 | b2);
                    }
                    if(b0 <= 31)
                        return new Operand(b0, Type.Operator);
                    if (b0 < 246)
                        return new Operand(b0 - 139, Type.Int);
                    if(b0 <= 250)
                    {
                        int b1 = r.ReadUInt8();
                        return new Operand((b0 - 247) * 256 + b1 + 108);
                    }
                    if(b0 <= 254)
                    {
                        int b1 = r.ReadUInt8();
                        return new Operand(-(b0 - 251) * 256 - b1 - 108);
                    }
                    if(b0 == 255)
                    {
                        // Doc says this is 16-bit signed integer with 16 bits of fraction.
                        // So this might actually be a fixed point float.
                        int b1 = r.ReadUInt8();
                        int b2 = r.ReadUInt8();
                        int b3 = r.ReadUInt8();
                        int b4 = r.ReadUInt8();

                        return new Operand((b1 << 24) | (b2 << 16) | (b3 << 8) | (b4 << 0));
                    }

                    // This really isn't possible, all 255 value of the byte should be covered.
                    return Error();
                }

                public static void LoadParsed(List<Operand> lst, out bool val)
                {
                    if (lst.Count != 1)
                        throw new System.Exception("Unexpected parsed parameter count.");

                    val = lst[0].GetInt() != 0;
                    lst.Clear();
                }

                public static void LoadParsed(List<Operand> lst, out int val)
                { 
                    if(lst.Count != 1)
                        throw new System.Exception("Unexpected parsed parameter count.");

                    val = lst[0].GetInt();
                    lst.Clear();
                }

                public static void LoadParsed(List<Operand> lst, out float val)
                {
                    if (lst.Count != 1)
                        throw new System.Exception("Unexpected parsed parameter count.");

                    val = lst[0].GetReal();
                    lst.Clear();
                }

                public static void LoadParsed(List<Operand> lst, int [] rvals)
                {
                    if (lst.Count != rvals.Length)
                        throw new System.Exception("Unexpected parsed parameter count.");

                    for(int i = 0; i < lst.Count; ++i)
                        rvals[i] = lst[i].GetInt();

                    lst.Clear();
                }

                public static void LoadParsed(List<Operand> lst, float [] rvals)
                {
                    if (lst.Count != rvals.Length)
                        throw new System.Exception("Unexpected parsed parameter count.");

                    for (int i = 0; i < lst.Count; ++i)
                        rvals[i] = lst[i].GetReal();

                    lst.Clear();
                }
            }
        }
    }
}
