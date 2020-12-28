using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PxPre
{
    namespace Berny
    {
        namespace CFF
        {
            public struct ExecContext
            {
                public Font.Glyph glyph;
                public Font.Contour contour;

                public Dictionary<int, Type2Charstring> local;
                public Dictionary<int, Type2Charstring> global;

                public bool atStart;
                public bool ended;
                public bool processedFirstMT;
                public float width;

                public List<float> hstems;  // Loaded as a delta
                public List<float> vstems;  // Loaded as a delta

                public List<Operand> stack;

                public byte [] hintMasks;
                public byte [] cntrMasks;

                public Vector2 pos;

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

                public void AddOp(Operand op)
                { 
                    this.stack.Add(op);
                }

                public void PopOp(int num)
                {
                    if (num == 1)
                        this.stack.RemoveAt(this.stack.Count - 1);
                    else
                        this.stack.RemoveRange(this.stack.Count - num, num);
                }

                public int OpCt()
                { 
                    return this.stack.Count;
                }

                public int Count {get => this.stack.Count; }

                public void ClearOps()
                { 
                    this.stack.Clear();
                }

                public Operand this[int key]
                {
                    get => this.stack[key];
                    set{ this.stack[key] = value; }
                }

                /// <summary>
                /// 
                /// </summary>
                /// <param name="c"></param>
                /// <param name="v"></param>
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

                public void AddLine(Vector2 pt)
                { 
                    this.contour.points.Add( new Font.Point(pt));
                    this.pos = pt;
                }

                public void AddLine()
                {
                    this.contour.points.Add(new Font.Point(this.pos));
                }

                public void AddLineDelta(Vector2 dv)
                {
                    this.pos += dv;
                    this.contour.points.Add(new Font.Point(this.pos));
                }

                public void SetPos(Vector2 v)
                { 
                    this.pos = v;
                }

                public void SetPosX(float x)
                { 
                    this.pos.x = x;
                }

                public void SetPosY(float y)
                { 
                    this.pos.y = y;
                }

                public Vector2 AddPos(Vector2 v)
                { 
                    this.pos += v;
                    return this.pos;
                }

                public Vector2 AddPosX(float x)
                { 
                    this.pos.x += x;
                    return this.pos;
                }

                public Vector2 AddPosY(float y)
                { 
                    this.pos.y += y;
                    return this.pos;
                }

                public Vector2 GetPos()
                { 
                    return this.pos;
                }

                public Vector2 PopVector()
                { 
                    Vector2 ret = 
                        new Vector2(
                            this.stack[this.stack.Count - 2].GetReal(),
                            this.stack[this.stack.Count - 1].GetReal());

                    this.PopOp(2);

                    return ret;
                }

                public float PopReal()
                { 
                    float f = this.stack[this.stack.Count - 1].GetReal();
                    this.PopOp(1);
                    return f;
                }

                public Vector2 ExtractVector(int i)
                { 
                    return new Vector2(
                        this.stack[i + 0].GetReal(),
                        this.stack[i + 1].GetReal());
                }

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
                /// 
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

                public void FlagProcessedMoveTo()
                {
                    this.SealCurrentContour();
                    this.processedFirstMT = true;
                }

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
    }
}