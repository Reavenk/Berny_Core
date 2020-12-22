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
        public class BShapeGenEllipse : BShapeGen
        {
            public float cx = 1.0f;
            public float cy = 1.0f;
            public float rx = 1.0f;
            public float ry = 1.0f;

            public override string ShapeType => "ellipse";

            public Vector2 Center
            { 
                get => new Vector2(this.cx, this.cy);
                set
                { 
                    this.cx = value.x;
                    this.cy = value.y;
                    this.FlagDirty();
                }
            }

            public Vector2 Radius
            { 
                get => new Vector2(this.rx, this.ry);
                set
                { 
                    this.rx = value.x;
                    this.ry = value.y;
                    this.FlagDirty();
                }
            }

            public float CX 
            { 
                get => this.cx; 
                set { this.cx = value; this.FlagDirty(); } 
            }

            public float CY 
            { 
                get => this.cy; 
                set { this.cy = value; this.FlagDirty(); } 
            }

            public float RadX 
            { 
                get => this.rx; 
                set { this.rx = value; this.FlagDirty(); } 
            }

            public float RadY 
            { 
                get => this.ry; 
                set { this.ry = value; this.FlagDirty(); } 
            }

            public BShapeGenEllipse(BShape shape, Vector2 center, Vector2 radius)
                : base(shape)
            { 
                this.cx = center.x;
                this.cy = center.y;
                this.rx = radius.x;
                this.ry = radius.y;
            }

            public override void Reconstruct()
            {
                this.shape.Clear();

                List<BNode.BezierInfo> nodes = new List<BNode.BezierInfo>();

                const int circSegs = 16;
                float circR = 2.0f * Mathf.PI / circSegs;
                
                // The tangents aren't as simple as the circle case. What was chosen was to pick the
                // delta to the neighbors - to do this, 
                // we :
                // first just fill in the points
                // next we set the tangent as the delta to the next and previous point
                // We then do line collision
                // and then pull back 2/3rd to turn the quadratic to cubic.

                // Fill in the points
                for(int i = 0; i < circSegs; ++i)
                {
                    float thR = -circR * i;

                    float x = Mathf.Cos(thR) * this.rx;
                    float y = Mathf.Sin(thR) * this.ry;

                    nodes.Add(
                        new BNode.BezierInfo(
                            new Vector2(this.cx + x, this.cy + y)));
                }

                BLoop bl = this.shape.AddLoop(nodes.ToArray());

                // We're going to travel instead of iterate over the list - I suspect
                // the list might turn into a HashSet in the future so I don't want to
                // over-leverage it.
                BNode bnStart = bl.nodes[0];
                foreach(BNode bnit in bnStart.Travel())
                { 
                    bnit.UseTanIn = true;
                    bnit.UseTanOut = true;

                    Vector2 prevToNext = bnit.next.Pos - bnit.prev.Pos;
                    bnit.TanIn =  -prevToNext;
                    bnit.TanOut = prevToNext;
                }

                // Treat the tangents are rays and find the intersection. This is where
                // we emulate quadratic tangents and convert them to cubic.
                foreach(BNode bnit in bnStart.Travel())
                { 
                    float s, t;

                    // Previous tangent to be 2/3rds the intersection
                    Utils.ProjectSegmentToSegment(bnit.Pos, bnit.Pos + bnit.TanIn, bnit.prev.Pos, bnit.prev.Pos + bnit.prev.TanOut, out s, out t);
                    bnit.TanIn *= (s * 2.0f / 3.0f);

                    // Next tangent to be 2/3rds the intersection
                    Utils.ProjectSegmentToSegment(bnit.Pos, bnit.Pos + bnit.TanOut, bnit.next.Pos, bnit.next.Pos + bnit.next.TanIn, out s, out t);
                    bnit.TanOut *= (s * 2.0f / 3.0f);
                }
            }

            public override bool LoadFromSVGXML(System.Xml.XmlElement shapeEle)
            {
                System.Xml.XmlAttribute attrCX = shapeEle.GetAttributeNode("cx");
                SVGSerializer.AttribToFloat(attrCX, ref this.cx);

                System.Xml.XmlAttribute attrCY = shapeEle.GetAttributeNode("cy");
                SVGSerializer.AttribToFloat(attrCY, ref this.cy);

                System.Xml.XmlAttribute attrRX = shapeEle.GetAttributeNode("rx");
                SVGSerializer.AttribToFloat(attrRX, ref this.rx);

                System.Xml.XmlAttribute attrRY = shapeEle.GetAttributeNode("ry");
                SVGSerializer.AttribToFloat(attrRY, ref this.ry);

                return true;
            }

            public override bool SaveToSVGXML(System.Xml.XmlElement shapeEle)
            {
                shapeEle.SetAttribute("cx", this.cx.ToString());
                shapeEle.SetAttribute("cy", this.cy.ToString());

                shapeEle.SetAttribute("rx", this.rx.ToString());
                shapeEle.SetAttribute("ry", this.ry.ToString());

                return true;
            }
        }
    }
}