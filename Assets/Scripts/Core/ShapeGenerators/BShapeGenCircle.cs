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
        public class BShapeGenCircle : BShapeGen
        {
            /// <summary>
            /// X component of the circle's center point.
            /// </summary>
            float cx;

            /// <summary>
            /// Y component of the circle's center point.
            /// </summary>
            float cy;

            /// <summary>
            /// The radius of the center.
            /// </summary>
            float radius;

            /// <summary>
            /// Property for the circle's center.
            /// </summary>
            public Vector2 Center 
            {
                get{ return new Vector2(this.cy, this.cy); } 
                set
                { 
                    this.cx = value.x;
                    this.cy = value.y;
                    this.FlagDirty();
                }
            }

            /// <summary>
            /// Property for the X component of the circle's center point.
            /// </summary>
            public float CX
            {
                get => this.cx;
                set{ this.cx = value; this.FlagDirty(); }
            }

            /// <summary>
            /// Property for the Y component of the circle's center point.
            /// </summary>
            public float CY
            { 
                get => this.cy;
                set{ this.cy = value; this.FlagDirty(); }
            }

            /// <summary>
            /// Property for the radius of the circle.
            /// </summary>
            public float Radius
            { 
                get => this.radius;
                set{ this.radius = value; this.FlagDirty(); }
            }

            public override string ShapeType => "circle";

            public BShapeGenCircle(BShape shape, Vector2 center, float rad)
                : base(shape)
            { }

            public override void Reconstruct()
            {
                this.shape.Clear();

                List<BNode.BezierInfo> nodes = new List<BNode.BezierInfo>();

                const int circSegs = 8;
                float circR = 2.0f * Mathf.PI / circSegs;

                // SOH CAH TOA stuff, finding the far side of the opposite if 
                // the length of adjacent is the radius. We do this with half
                // the angle because we're doing with for both tangents, which
                // would double the angle when combined.
                float tanCos = Mathf.Cos(circR * 0.5f);
                float tanSin = Mathf.Sin(circR * 0.5f);
                float tanNorm = tanSin/tanCos * this.radius;

                float cuTanDst = tanNorm * (2.0f / 3.0f);

                for(int i = 0; i < circSegs; ++i)
                { 
                    // We need to move in reverse because 
                    // we want to go clockwise, but trig functions
                    // go counter-clockwise.
                    float thR = -circR * i;

                    float x = Mathf.Cos(thR);
                    float y = Mathf.Sin(thR);

                    nodes.Add( 
                        new BNode.BezierInfo( 
                            new Vector2(this.cx + x * this.radius, this.cy + y * this.radius), 
                            new Vector2(-y, x) * cuTanDst,
                            new Vector2(y, -x) * cuTanDst,
                            true, 
                            true,
                            BNode.TangentMode.Symmetric));
                }

                this.shape.AddLoop(nodes.ToArray());
            }

            public override bool LoadFromSVGXML(System.Xml.XmlElement shapeEle)
            {
                System.Xml.XmlAttribute attrCX = shapeEle.GetAttributeNode("cx");
                SVGSerializer.AttribToFloat(attrCX, ref this.cx);

                System.Xml.XmlAttribute attrCY = shapeEle.GetAttributeNode("cy");
                SVGSerializer.AttribToFloat(attrCY, ref this.cy);

                System.Xml.XmlAttribute attrR = shapeEle.GetAttributeNode("r");
                SVGSerializer.AttribToFloat(attrR, ref this.radius);

                return true;
            }

            public override bool SaveToSVGXML(System.Xml.XmlElement shapeEle)
            {
                shapeEle.SetAttribute("cx", this.cx.ToString());
                shapeEle.SetAttribute("cy", this.cy.ToString());
                shapeEle.SetAttribute("radius", this.radius.ToString());

                return true;
            }
        }
    }
}
