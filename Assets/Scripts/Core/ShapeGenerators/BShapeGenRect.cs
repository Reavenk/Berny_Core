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
        public class BShapeGenRect : BShapeGen
        {
            float fx = 0.0f;
            float fy = 0.0f;
            float width = 1.0f;
            float height = 1.0f;
            float rx = 0.0f;
            float ry = 0.0f;

            public override string ShapeType => "rect";

            public float FX 
            {
                get => this.fx; 
                set{ this.fx = value; this.FlagDirty(); } 
            }

            public float FY 
            { 
                get => this.fy; 
                set { this.fy = value; this.FlagDirty(); } 
            }

            public float Width 
            { 
                get => this.width; 
                set { this.width = value; this.FlagDirty(); } 
            }

            public float Height 
            { 
                get => this.height; 
                set { this.height = value; this.FlagDirty(); } 
            }

            public float RoundX 
            { 
                get => this.rx; 
                set { this.rx = value; this.FlagDirty(); } 
            }

            public float RoundY 
            { 
                get => this.ry; 
                set { this.ry = value; this.FlagDirty(); } 
            }

            public BShapeGenRect(BShape shape, Vector2 topLeft, Vector2 dim, Vector2 ? round = null)
                : base(shape)
            { 
                this.fx = topLeft.x;
                this.fy = topLeft.y;
                this.width = dim.x;
                this.height = dim.y;

                if(round.HasValue == true)
                {
                    this.rx = round.Value.x;
                    this.ry = round.Value.y;
                }
            }

            public override void Reconstruct()
            {
                this.shape.Clear();

                float roundx = Mathf.Max(this.rx, 0.0f);
                float roundy = Mathf.Max(this.ry, 0.0f);

                // If one of them is round and the other isn't, treat
                // the non-zero value as the value for both, or else
                // the rounding is pointless.
                if(roundx == 0.0f)
                    roundx = roundy;
                else if(roundy == 0.0f)
                    roundy = roundx;

                

                if(roundx == 0.0f && roundy == 0.0f)
                {
                    BLoop bl = 
                        new BLoop(
                            this.shape,
                            new BNode.BezierInfo(this.fx, this.fy + this.height),
                            new BNode.BezierInfo(this.fx + this.width, this.fy + this.height),
                            new BNode.BezierInfo(this.fx + this.width, this.fy),
                            new BNode.BezierInfo(this.fx, this.fy)
                            );

                    return;
                }
                else
                {
                    roundx = Mathf.Min(this.width * 0.5f, roundx);
                    roundy = Mathf.Min(this.width * 0.5f, roundy);

                    // Multiple the distance by 2/3 to turn Bezier tangents 
                    // into cubic tangents.
                    float bezX = roundx * (2.0f / 3.0f);
                    float bezY = roundy * (2.0f / 3.0f);

                    bool hasW = roundx * 0.5f < this.width;
                    bool hasH = roundy * 0.5f < this.height;

                    List<BNode.BezierInfo> binfos = new List<BNode.BezierInfo>();

                    float farx = this.fx + this.width;
                    float fary = this.fy + this.height;

                    // Do the top, going from left to right
                    if(hasW == true)
                    { 
                        binfos.Add( new BNode.BezierInfo(new Vector2(this.fx + roundx, fary), new Vector2(-bezX, 0.0f), Vector2.zero, true, false, BNode.TangentMode.Disconnected));
                        binfos.Add( new BNode.BezierInfo(new Vector2(farx - roundx, fary), Vector2.zero, new Vector2(bezX, 0.0f), false, true, BNode.TangentMode.Disconnected));
                    }
                    else
                    {
                        binfos.Add(new BNode.BezierInfo(new Vector2(this.fx + this.width * 0.5f, fary), new Vector2(-bezX, 0.0f), new Vector2(bezX, 0.0f), true, true, BNode.TangentMode.Disconnected));
                    }

                    // Do the right, going from top to bottom.
                    if(hasH == true)
                    {
                        binfos.Add(new BNode.BezierInfo(new Vector2(farx, fary - roundy), new Vector2(0.0f, bezY), Vector2.zero, true, false, BNode.TangentMode.Disconnected));
                        binfos.Add(new BNode.BezierInfo(new Vector2(farx, this.fy + roundy), Vector2.zero, new Vector2(0.0f, -bezY), false, true, BNode.TangentMode.Disconnected));
                    }
                    else
                    {
                        binfos.Add(new BNode.BezierInfo(new Vector2(farx, this.fy + this.height * 0.5f), new Vector2(0.0f, bezY), new Vector2(0.0f, -bezY), true, true, BNode.TangentMode.Disconnected));
                    }

                    // Do the bottom, going from right to left.
                    if(hasW == true)
                    {
                        binfos.Add(new BNode.BezierInfo(new Vector2(farx - roundx, this.fy), new Vector2(bezX, 0.0f), Vector2.zero, true, false, BNode.TangentMode.Disconnected));
                        binfos.Add(new BNode.BezierInfo(new Vector2(this.fx + roundx, this.fy), Vector2.zero, new Vector2(-bezX, 0.0f), false, true, BNode.TangentMode.Disconnected));
                    }
                    else
                    {
                        binfos.Add(new BNode.BezierInfo(new Vector2(this.fx + this.width * 0.5f, this.fy), new Vector2(bezX, 0.0f), new Vector2(-bezX, 0.0f), true, true, BNode.TangentMode.Disconnected));
                    }

                    // Do the left, going from bottom to top.
                    if(hasH == true)
                    {
                        binfos.Add(new BNode.BezierInfo(new Vector2(this.fx, this.fy + roundy), new Vector2(0.0f, -bezY), Vector2.zero, true, false, BNode.TangentMode.Disconnected));
                        binfos.Add(new BNode.BezierInfo(new Vector2(this.fx, fary - roundy), Vector2.zero, new Vector2(0.0f, bezY), false, true, BNode.TangentMode.Disconnected));
                    }
                    else
                    {
                        binfos.Add(new BNode.BezierInfo(new Vector2(this.fx, this.fx + this.height * 0.5f), new Vector2(0.0f, -bezY), new Vector2(0.0f, bezY), true, true, BNode.TangentMode.Disconnected));
                    }

                    BLoop bl =
                        new BLoop(
                            this.shape,
                            binfos.ToArray());
                }
            }

            public override bool LoadFromSVGXML(System.Xml.XmlElement shapeEle)
            {
                System.Xml.XmlAttribute attrX = shapeEle.GetAttributeNode("x");
                SVGSerializer.AttribToFloat(attrX, ref this.fx);
                System.Xml.XmlAttribute attrY = shapeEle.GetAttributeNode("y");
                SVGSerializer.AttribToFloat(attrY, ref this.fy);

                System.Xml.XmlAttribute attrWidth = shapeEle.GetAttributeNode("width");
                SVGSerializer.AttribToFloat(attrWidth, ref this.width);
                System.Xml.XmlAttribute attrHeight = shapeEle.GetAttributeNode("height");
                SVGSerializer.AttribToFloat(attrHeight, ref this.height);

                System.Xml.XmlAttribute attrRX = shapeEle.GetAttributeNode("rx");
                SVGSerializer.AttribToFloat(attrRX, ref this.rx);
                System.Xml.XmlAttribute attrRY = shapeEle.GetAttributeNode("ry");
                SVGSerializer.AttribToFloat(attrY, ref this.ry);

                return true;
            }

            public override bool SaveToSVGXML(System.Xml.XmlElement shapeEle)
            {
                shapeEle.SetAttribute("x", this.fx.ToString());
                shapeEle.SetAttribute("y", this.fy.ToString());

                shapeEle.SetAttribute("width", this.width.ToString());
                shapeEle.SetAttribute("height", this.height.ToString());

                shapeEle.SetAttribute("rx", this.rx.ToString());
                shapeEle.SetAttribute("ry", this.ry.ToString());

                return true;
            }
        }
    }
}
