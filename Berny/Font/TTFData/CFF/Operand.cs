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
        namespace CFF
        {
            /// <summary>
            /// A parsed piece of data in a CCF and Charstring DICT or program.
            /// </summary>
            /// <remarks>The name Operand is a misnomer, because it's also used to represent
            /// Operators, as well as other things.</remarks>
            public struct Operand
            {
                /// <summary>
                /// The datatype of the operand.
                /// </summary>
                public enum Type
                {
                    /// <summary>
                    /// Error code. The opcode is either uninitialized, or there was an
                    /// error when parsing the Operand from the datastream or interpreting
                    /// the data.
                    /// </summary>
                    Error,

                    /// <summary>
                    /// Integer value. Reference this.intVal for the correct value. 
                    /// </summary>
                    Int,

                    /// <summary>
                    /// Float value. Reference this.floatVal for the correct value.
                    /// </summary>
                    Real,

                    /// <summary>
                    /// Operator/Opcode. Reference this.intVal for the correct value.
                    /// </summary>
                    Operator
                }

                /// <summary>
                /// The data type.
                /// </summary>
                public Type type;

                // If this was C++, this would be a union. But alas, we're doing this in C# with a restriction
                // to use only safe code, so these values live side-by-side.

                /// <summary>
                /// The integer value. Only reference the value if the type is an Int or Operator.
                /// </summary>
                public int intVal;

                /// <summary>
                /// The floating point value. Only reference the value if the type is a Real.
                /// </summary>
                public float realVal;

                /// <summary>
                /// TTFs sometimes store array of numbers as a delta format, where the first entry is an absolute
                /// value, with each other value being an offset from its previous value. This function takes raw
                /// delta values (including the first-entry absolute value) and converts the entire array of values
                /// into absolute values.
                /// </summary>
                /// <param name="lst">The delta collection of values to convert.</param>
                static public void ConvertDeltasToAbsolute(List<int> lst)
                {
                    for (int i = 1; i < lst.Count; ++i)
                    {
                        lst[i] += lst[i - 1];
                    }
                }

                /// <summary>
                /// TTFs sometimes store array of numbers as a delta format, where the first entry is an absolute
                /// value, with each other value being an offset from its previous value. This function takes raw
                /// delta values (including the first-entry absolute value) and converts the entire array of values
                /// into absolute values.
                /// </summary>
                /// <param name="lst">The delta collection of values to convert.</param>
                static public void ConvertDeltasToAbsolute(int[] ri)
                {
                    for (int i = 1; i < ri.Length; ++i)
                    {
                        ri[i] += ri[i - 1];
                    }
                }

                /// <summary>
                /// Converts a collection of Operands to a List of their int values.
                /// </summary>
                /// <param name="vals">The collection of Operands to convert into an list.</param>
                /// <returns>The converted Operands as an int list.</returns>
                /// <remarks>If the collection is a delta, see Operand.ConvertDeltasToAbsolute().</remarks>
                public static List<int> ToIntList(IEnumerable<Operand> vals)
                {
                    List<int> ret = new List<int>();

                    foreach(Operand tv in vals)
                        ret.Add(tv.GetInt());

                    return ret;
                }

                /// <summary>
                /// Converts a collection of Operands to an array of their int values.
                /// </summary>
                /// <param name="vals">The collection of Operands to convert into an array.</param>
                /// <returns>The converted Operands as an int array.</returns>
                /// <remarks>If the collection is a delta, see Operand.ConvertDeltasToAbsolute().</remarks>
                public static int[] ToIntArray(IEnumerable<Operand> vals)
                {
                    return ToIntList(vals).ToArray();
                }

                /// <summary>
                /// Converts a collection of Operands to a list of their float values.
                /// </summary>
                /// <param name="vals">The collection of Operands to convert into a list.</param>
                /// <returns>The converted Operands as a float list.</returns>
                public static List<float> ToFloatList(IEnumerable<Operand> vals)
                {
                    List<float> ret = new List<float>();

                    foreach(Operand tv in vals)
                        ret.Add(tv.GetReal());

                    return ret;
                }

                /// <summary>
                /// Convert a collection of Operands to an array of their float values.
                /// </summary>
                /// <param name="vals">The collection of Operands to convert into an array.</param>
                /// <returns>The converted Operands as a float array.</returns>
                public static float[] ToFloatArray(IEnumerable<Operand> vals)
                {
                    return ToFloatList(vals).ToArray();
                }

                /// <summary>
                /// Constructor.
                /// 
                /// It's main purpose is to allow creating Operators via a constructor.
                /// </summary>
                /// <param name="val">The int value for the operand.</param>
                /// <param name="t">The type of the Operand.</param>
                public Operand(int val, Type t)
                { 
                    this.type = t;
                    this.realVal = val;
                    this.intVal = val;
                }

                /// <summary>
                /// Constructor.
                /// </summary>
                /// <param name="val">The int value for the Operand.</param>
                public Operand(int val)
                {
                    this.type = Type.Int;
                    this.realVal = val;
                    this.intVal = val;
                }

                /// <summary>
                /// Constructor.
                /// </summary>
                /// <param name="val">The float value for the Operand.</param>
                public Operand(float val)
                { 
                    this.type = Type.Real;
                    this.realVal = val;
                    this.intVal = (int)val;
                }

                /// <summary>
                /// Creates an error Operand.
                /// </summary>
                /// <returns>An error Operand.</returns>
                public static Operand Error()
                { 
                    return new Operand(0, Type.Error);
                }

                /// <summary>
                /// Returns the int value of an operand.
                /// </summary>
                /// <returns>The Operand's int value.</returns>
                /// <remarks>Automatically does value type conversion.</remarks>
                public int GetInt()
                {
                    if (this.type != Type.Real)
                        return this.intVal;

                    return (int)this.realVal;

                }

                /// <summary>
                /// Returns the float (real number) value of an operand.
                /// </summary>
                /// <returns>The Operand's float value.</returns>
                /// <remarks>Automatically does value type conversion.</remarks>
                public float GetReal()
                {
                    if (this.type == Type.Real)
                        return this.realVal;

                    return (float)this.intVal;
                }

                /// <summary>
                /// Checks if the Operand's value is zero.
                /// </summary>
                /// <returns>True, if the operand's value is zero. Else, false.</returns>
                public bool IsZero()
                {
                    if (this.type == Type.Real)
                        return this.realVal == 0.0f;

                    return this.intVal == 0;
                }

                /// <summary>
                /// Checks if the Operand's value is not zero.
                /// </summary>
                /// <returns>True, if the operand's value is not zero. Else, false.</returns>
                public bool NonZero()
                {
                    if (this.type == Type.Real)
                        return this.realVal != 0.0f;

                    return this.intVal != 0;
                }

                /// <summary>
                /// Read an operand value from a CFF file.
                /// </summary>
                /// <param name="r">The reader, with the read position at the Operand to read.</param>
                /// <returns>The loaded Operand value.</returns>
                /// <remarks>This is for CFF (DICT) values. For other similar formats such as CFF2s or Charstrings, make sure
                /// the correct Read*() function is used.</remarks>
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

                /// <summary>
                /// Read an operand value from a Charstring Type 2 byte array..
                /// </summary>
                /// <param name="r">The reader, with the read position at the Charstring Operand to read.</param>
                /// <returns>The loaded operand.</returns>
                /// /// <remarks>This is for Charstring data. For other similar formats such as CFFs or CFF2s, make sure
                /// the correct Read*() function is used.</remarks>
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
                        byte b1 = r.ReadUInt8();
                        byte b2 = r.ReadUInt8();
                        // This is a two's complement signed number, so we can't just
                        // shift and OR them as an int, but also have to be weary of
                        // the sign bit and two's complement conversion if it's negative.
                        short merged = (short)(b1 << 8 | b2);
                        return new Operand(merged);
                    }
                    if(b0 <= 31)
                        return new Operand(b0, Type.Operator);
                    if (b0 <= 246)
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

                /// <summary>
                /// Load the first operand as a bool and clear the stack.
                /// </summary>
                /// <param name="lst">The parameter list to load the value from.</param>
                /// <param name="val">The value output.</param>
                /// <remarks>Primarily used for reading DICT values where there's an assertion that
                /// the stack has exactly 1 bool value on it.</remarks>
                public static void LoadParsed(List<Operand> lst, out bool val)
                {
                    if (lst.Count != 1)
                        throw new System.Exception("Unexpected parsed parameter count.");

                    val = lst[0].GetInt() != 0;
                    lst.Clear();
                }

                /// <summary>
                /// Load the first operand as an int and clear the stack.
                /// </summary>
                /// <param name="lst">The parameter list to load the value from.</param>
                /// <param name="val">The value output.</param>
                /// <remarks>Primarily used for reading DICT values where there's an assertion that
                /// the stack has exactly 1 int value on it.</remarks>
                public static void LoadParsed(List<Operand> lst, out int val)
                { 
                    if(lst.Count != 1)
                        throw new System.Exception("Unexpected parsed parameter count.");

                    val = lst[0].GetInt();
                    lst.Clear();
                }

                /// <summary>
                /// Load the first operand as a float and clear the stack.
                /// </summary>
                /// <param name="lst">The parameter list to load the value from.</param>
                /// <param name="val">The value output.</param>
                /// <remarks>Autoamtically does value type conversion.</remarks>
                /// <remark>Primarily used for reading DICT values where there's an assertion that
                /// the stack has exactly 1 float value on it.</remark>
                public static void LoadParsed(List<Operand> lst, out float val)
                {
                    if (lst.Count != 1)
                        throw new System.Exception("Unexpected parsed parameter count.");

                    val = lst[0].GetReal();
                    lst.Clear();
                }

                /// Given an expected count of int values, load an array of operands
                /// into those values.
                /// </summary>
                /// <param name="lst">The operands to load into the array.</param>
                /// <param name="rvals">The array to load the operand values into.</param>
                /// <remarks>Does automatic collection size checking.</remarks>
                public static void LoadParsed(List<Operand> lst, int [] rvals)
                {
                    if (lst.Count != rvals.Length)
                        throw new System.Exception("Unexpected parsed parameter count.");

                    for(int i = 0; i < lst.Count; ++i)
                        rvals[i] = lst[i].GetInt();

                    lst.Clear();
                }

                /// <summary>
                /// Given an expected count of floating point values, load an array of operands
                /// into those values.
                /// </summary>
                /// <param name="lst">The operands to load into the array.</param>
                /// <param name="rvals">The array to load the operand values into.</param>
                /// <remarks>Does automatic collection size checking.</remarks>
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
