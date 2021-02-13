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
using UnityEngine;

namespace PxPre.Berny.CFF
{
    /// <summary>
    /// Execution context for a Type 2 Charstring for CFF to generate a Berny 
    /// representation of a glyph.
    /// </summary>
    public struct ExecContext
    {
        /// <summary>
        /// The glyph to fill information for with the program execution.
        /// </summary>
        public Font.Glyph glyph;

        /// <summary>
        /// The current contour that geometry is being inserted into.
        /// </summary>
        public Font.Contour contour;

        /// <summary>
        /// References to the local subroutines available.
        /// </summary>
        /// <remarks>Placeholder</remarks>
        public Dictionary<int, Type2Charstring> local;

        /// <summary>
        /// References to the global subroutines available.
        /// </summary>
        /// <remarks>Placeholder</remarks>
        public Dictionary<int, Type2Charstring> global;

        /// <summary>
        /// The program is in progress of being executed from the very start.
        /// Used to identify if the width value should be expected for parsing.
        /// </summary>
        public bool atStart;

        /// <summary>
        /// A cached record if the program successfully finished.
        /// </summary>
        public bool ended;

        /// <summary>
        /// A cached record of at least one moveto opcode has already been processed.
        /// </summary>
        public bool processedFirstMT;

        /// <summary>
        /// The parsed with of the program. This is the first value of a charstring program.
        /// </summary>
        public float width;

        public List<float> hstems;  // Loaded as a delta
        public List<float> vstems;  // Loaded as a delta

        public List<Operand> stack;

        /// <summary>
        /// Loaded hint masks. Can be null or empty if the program
        /// did not have any hint masks.
        /// </summary>
        public byte [] hintMasks;

        /// <summary>
        /// Loaded contour masks. Can be null or empty if the program
        /// did not have any contour masks.
        /// </summary>
        public byte [] cntrMasks;

        /// <summary>
        /// aka, "position cursor".
        /// The current read/write position for generating glyph geometry.
        /// </summary>
        public Vector2 pos;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="glyph">The glyph being constructed.</param>
        /// <param name="local">Local subroutines.</param>
        /// <param name="global">Global subroutines.</param>
        public ExecContext(
            Font.Glyph glyph, 
            Dictionary<int, Type2Charstring> local, 
            Dictionary<int, Type2Charstring> global)
        {
            this.atStart = true;

            this.glyph = glyph;
            this.contour = null;

            this.pos = Vector2.zero;

            this.local = local;
            this.global = global;
            this.ended = false;
            this.processedFirstMT = false;
            this.width = 0.0f;

            this.hstems = new List<float>();
            this.vstems = new List<float>();

            this.hintMasks = null;
            this.cntrMasks = null;

            this.stack = new List<Operand>();
        }

        /// <summary>
        /// Add an Operand to the stack.
        /// </summary>
        /// <param name="op">The operand to add.</param>
        public void AddOp(Operand op)
        { 
            this.stack.Add(op);
        }

        /// <summary>
        /// Pop Operand(s) from the stack.
        /// </summary>
        /// <param name="num">The number of Operands to pop.</param>
        public void PopOp(int num)
        {
            if (num == 1)
                this.stack.RemoveAt(this.stack.Count - 1);
            else
                this.stack.RemoveRange(this.stack.Count - num, num);
        }

        /// <summary>
        /// The number of Operands on the parameter stack.
        /// </summary>
        /// <returns>The number of Operands on the stack.</returns>
        public int OpCt()
        { 
            return this.stack.Count;
        }

        /// <summary>
        /// The number of Operands on the parameter stack.
        /// </summary>
        /// <remarks>This is the same as OpCt() but in a more familiar properties form.</remarks>
        public int Count {get => this.stack.Count; }

        /// <summary>
        /// Clear all Operands on the stack.
        /// </summary>
        public void ClearOps()
        { 
            this.stack.Clear();
        }

        /// <summary>
        /// Accessor to the Operand stack.
        /// </summary>
        /// <param name="key">The entry to retrieve.</param>
        /// <returns>The requested Operand.</returns>
        public Operand this[int key]
        {
            get => this.stack[key];
            set{ this.stack[key] = value; }
        }

        /// <summary>
        /// Set the outgoing tangent of the last point of a contour.
        /// </summary>
        /// <param name="c">The contour to modify the end of.</param>
        /// <param name="v">The outgoing control or tangent - depending on parameter abs.</param>
        /// <param name="abs">If true, the parameter v is an absolute control position (which will
        /// be converted to a relative tangent).</param>
        public void SetLastTangent(Font.Contour c, Vector2 v, bool abs = true)
        {
            int lastIdx = c.points.Count - 1;
            Font.Point p = c.points[lastIdx];

            p.useTangentOut = true;

            if (abs == true)
                p.tangentOut = v - p.position;
            else
                p.tangentOut = v;

            c.points[lastIdx] = p;
        }

        /// <summary>
        /// Adds two consecutive Bezier curves to the current glyph.
        /// </summary>
        /// <param name="pt1">Outgoing control of the first curve.</param>
        /// <param name="pt2">Intcoming control of the first curve.</param>
        /// <param name="pt3">New point between the two curves.</param>
        /// <param name="pt4">Outgoing control of the second curve.</param>
        /// <param name="pt5">Incoming control of the second curve.</param>
        /// <param name="pt6">Endpoint of the second curve.</param>
        /// <remarks>It uses the last point added on the current contour as the previous segment point.</remarks>
        public void Add6Bezier(
            Vector2 pt1,
            Vector2 pt2,
            Vector2 pt3,
            Vector2 pt4,
            Vector2 pt5,
            Vector2 pt6)
        {
            SetLastTangent(this.contour, pt1);

            Font.Point newPt = new Font.Point(pt3);
            newPt.useTangentIn = true;
            newPt.useTangentOut = true;
            newPt.tangentIn = pt2 - pt3;
            newPt.tangentOut = pt4 - pt3;
            this.contour.points.Add(newPt);

            Font.Point capPt = new Font.Point(pt6);
            capPt.useTangentIn = true;
            capPt.tangentIn = pt5 - pt6;

            this.contour.points.Add(capPt);

            this.pos = pt6;
        }

        /// <summary>
        /// Adds a Bezier curve.
        /// </summary>
        /// <param name="pt1">The absolute position of the preious point's output control</param>
        /// <param name="pt2">The absolute position of the new point's intput control</param>
        /// <param name="pt3">The absolute position of the new point.</param>
        /// <remarks>It uses the last point addded on the current contour as the previous segment point.</remarks>
        public void Add3Bezier(
            Vector2 pt1,
            Vector2 pt2,
            Vector2 pt3)
        {
            SetLastTangent(this.contour, pt1);

            Font.Point newPt = new Font.Point(pt3);
            newPt.useTangentIn = true;
            newPt.tangentIn = pt2 - pt3;

            this.contour.points.Add(newPt);

            this.pos = pt3;
        }

        /// <summary>
        /// Starts a new contour.
        /// </summary>
        /// <remarks>This is expected to be called upon a new charstring MoveTo command, right after a 
        /// call to SealCurrentContour() to finalize the old contour.</remarks>
        public void StartContour()
        {
            if(this.contour == null || this.contour.points.Count > 0)
            {
                this.contour = new Font.Contour();
                this.glyph.contours.Add(this.contour);
            }

            Font.Point pt = new Font.Point(this.pos);
            this.contour.points.Add(pt);
        }

        /// <summary>
        /// Specify an absolute position as the endpoint of a new line segment.
        /// </summary>
        /// <param name="pt">The end position of the line.</param>
        /// <remarks>The start position of the line segment will be the current cusor position.</remarks>
        public void AddLine(Vector2 pt)
        { 
            this.contour.points.Add( new Font.Point(pt));
            this.pos = pt;
        }

        /// <summary>
        /// Set the cursor position as a new line segment.
        /// </summary>
        public void AddLine()
        {
            this.contour.points.Add(new Font.Point(this.pos));
        }

        /// <summary>
        /// Add a Vector to the cursor and add the new cursor position as
        /// a new line.
        /// </summary>
        /// <param name="dv">The length of the line.</param>
        public void AddLineDelta(Vector2 dv)
        {
            this.pos += dv;
            this.contour.points.Add(new Font.Point(this.pos));
        }

        /// <summary>
        /// Set the position of the cursor.
        /// </summary>
        /// <param name="v">The cursor position.</param>
        public void SetPos(Vector2 v)
        { 
            this.pos = v;
        }

        /// <summary>
        /// Set the X component of the cursor.
        /// </summary>
        /// <param name="x">The x component.</param>
        public void SetPosX(float x)
        { 
            this.pos.x = x;
        }

        /// <summary>
        /// Set the Y component of the cursor.
        /// </summary>
        /// <param name="y">The y component.</param>
        public void SetPosY(float y)
        { 
            this.pos.y = y;
        }

        /// <summary>
        /// Add a Vector to the cursor.
        /// </summary>
        /// <param name="v">The vector to add.</param>
        /// <returns>The new cursor position.</returns>
        public Vector2 AddPos(Vector2 v)
        { 
            this.pos += v;
            return this.pos;
        }

        /// <summary>
        /// Add a float to the x component of the cursor.
        /// </summary>
        /// <param name="x">The amount to add to the cursor's x component.</param>
        /// <returns>The new cursor position.</returns>
        public Vector2 AddPosX(float x)
        { 
            this.pos.x += x;
            return this.pos;
        }

        /// <summary>
        /// Add a float to the y component of the cursor.
        /// </summary>
        /// <param name="y">The amount to add to the cursor's y component.</param>
        /// <returns>The new cursor position.</returns>
        public Vector2 AddPosY(float y)
        { 
            this.pos.y += y;
            return this.pos;
        }

        /// <summary>
        /// Return the cursor position.
        /// </summary>
        /// <returns>The cursor position.</returns>
        public Vector2 GetPos()
        { 
            return this.pos;
        }

        /// <summary>
        /// Pop the top of the stack and return its Vector2 value. This involves popping 2 elements off, with the lower
        /// stack element being the X component, and the very top of the stack being the Y component.
        /// </summary>
        /// <returns>The stack's top's Vector value.</returns>
        public Vector2 PopVector()
        { 
            Vector2 ret = 
                new Vector2(
                    this.stack[this.stack.Count - 2].GetReal(),
                    this.stack[this.stack.Count - 1].GetReal());

            this.PopOp(2);

            return ret;
        }

        /// <summary>
        /// Pop the top of the stack and return its floating point value.
        /// </summary>
        /// <returns>The stack's top's floating point value.</returns>
        public float PopReal()
        { 
            float f = this.stack[this.stack.Count - 1].GetReal();
            this.PopOp(1);
            return f;
        }

        /// <summary>
        /// Take a vector from the stack.
        /// </summary>
        /// <param name="i">The index from the stack for the vector. The Opcode entry at 
        /// (i + 0) will be the X component, and (i + 1) will be the Y component.</param>
        /// <returns>The extracted vector.</returns>
        public Vector2 ExtractVector(int i)
        { 
            return new Vector2(
                this.stack[i + 0].GetReal(),
                this.stack[i + 1].GetReal());
        }

        /// <summary>
        /// Take a vector from the stack, and add it to the cursor.
        /// </summary>
        /// <param name="i">The index from the stack for the vector. The Opcode entry at 
        /// (i + 0) will be the X component, and (i + 1) will be the Y component.</param>
        /// <returns>The new cursor position.</returns>
        public Vector2 ExtractAddedVector(int i)
        {
            Vector2 v2 = 
                new Vector2(
                    this.stack[i + 0].GetReal(),
                    this.stack[i + 1].GetReal());

            this.pos += v2;
            return this.pos;
        }

        /// <summary>
        /// Assuming the hstem and vstem values have been parsed and loaded into the context (if there are any),
        /// calculate the number of bytes needed for the hint mask and countour mask data.
        /// </summary>
        /// <returns>The number of bytes allocated for the flags.</returns>
        public int CountMaskBytes()
        {
            // The hintmask operator is followed by one or more data bytes that specify the stem hints which 
            // are to be active for the subsequent path construction. The number of data bytes must be exactly 
            // the number needed to represent the number of stems in the original stem list (those stems specified 
            // by the hstem, vstem, hstemhm, or vstemhm commands), using one bit in the data bytes for each stem 
            // in the original stem list. Bits with a value of one indicate stems that are active, and a value 
            // of zero indicates stems that are inactive. 

            int bitCt = this.hstems.Count + this.vstems.Count;
            int ret = (int)Mathf.Ceil((float)bitCt / 8.0f);
            return ret;
        }

        /// <summary>
        /// Notify the context that a MoveTo opcode was processed. This is used to make sure we 
        /// are aware when at least one MoveTo opcode is handled.
        /// </summary>
        public void FlagProcessedMoveTo()
        {
            this.SealCurrentContour();
            this.processedFirstMT = true;
        }

        /// <summary>
        /// When finishing a contour, call SealCurrentContour to do final post processing. 
        /// 
        /// The most important part is to properly close and loop a shape if extra finalization work is needed.
        /// </summary>
        public void SealCurrentContour()
        { 
            if(this.contour == null || this.contour.points.Count < 2)
                return;

            Font.Point first = this.contour.points[0];
            int lastIdx = this.contour.points.Count - 1;
            Font.Point last = this.contour.points[lastIdx];

            if((first.position - last.position).sqrMagnitude <= Mathf.Epsilon)
            { 
                if(last.useTangentIn == true)
                {
                    first.useTangentIn = true;
                    first.tangentIn = last.tangentIn;

                    this.contour.points[0] = first;
                }
                this.contour.points.RemoveAt(lastIdx);
            }
        }
    }
}