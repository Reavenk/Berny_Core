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
        /// <summary>
        /// Implements a procedural shape generator for the SVG line shape.
        /// 
        /// https://www.w3schools.com/graphics/svg_line.asp
        /// </summary>
        public class BShapeGenLine : BShapeGen
        {

            public override string ShapeType => "line";

            /// <summary>
            /// A point on the line.
            /// </summary>
            Vector2 pt0 = Vector2.zero;

            /// <summary>
            /// Another point on the line.
            /// </summary>
            Vector2 pt1 = Vector2.zero;

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="shape">The shape to attach to.</param>
            /// <param name="point1">An endpoint of the line.</param>
            /// <param name="point2">The other endpoint of the line.</param>
            public BShapeGenLine(BShape shape, Vector2 point1, Vector2 point2)
                : base(shape)
            {
                this.pt0 = point1;
                this.pt1 = point2;
            }

            /// <summary>
            /// Property for an endpoing of the line.
            /// </summary>
            public Vector2 Point1
            {
                get => this.pt0;
                set { this.pt0 = value; this.FlagDirty(); }
            }

            /// <summary>
            /// Property for the other endpoint of the line.
            /// </summary>
            public Vector2 Point2 
            {
                get => this.pt1;
                set{ this.pt1 = value; this.FlagDirty(); }
            }

            /// <summary>
            /// Property for the x component of the first endpoint.
            /// </summary>
            public float x1
            { 
                get => this.pt0.x;
                set { this.pt0.x = value; this.FlagDirty(); }
            }

            /// <summary>
            /// Property for the y component of the first endpoint.
            /// </summary>
            public float y1
            { 
                get => this.pt0.y;
                set { this.pt0.y = value; this.FlagDirty(); }
            }

            /// <summary>
            /// Property for the x component of the second endpoint.
            /// </summary>
            public float x2
            { 
                get => this.pt1.x;
                set { this.pt1.x = value; this.FlagDirty(); }
            }

            /// <summary>
            /// Property for the y component of the second endpoint.
            /// </summary>
            public float y2
            { 
                get => this.pt1.y;
                set{ this.pt1.y = value; this.FlagDirty(); }
            }

            public override void Reconstruct()
            {
                this.shape.Clear();

                if(this.pt0 == this.pt1)
                {
                    BLoop bl =
                    new BLoop(
                        this.shape,
                        true,
                        new Vector2[]{ this.pt0 });
                }
                else
                {
                    BLoop bl =
                    new BLoop(
                        this.shape,
                        true,
                        new Vector2[] { this.pt0, this.pt1 });
                }

                this.FlagDirty();
            }

            public override bool LoadFromSVGXML(System.Xml.XmlElement shapeEle, bool invertY)
            {
                System.Xml.XmlAttribute attrX1 = shapeEle.GetAttributeNode("x1");
                System.Xml.XmlAttribute attrY1 = shapeEle.GetAttributeNode("y1");
                System.Xml.XmlAttribute attrX2 = shapeEle.GetAttributeNode("x2");
                System.Xml.XmlAttribute attrY2 = shapeEle.GetAttributeNode("y2");

                SVGSerializer.AttribToFloat(attrX1, ref this.pt0.x);
                SVGSerializer.AttribToFloat(attrY1, ref this.pt0.y, invertY);
                SVGSerializer.AttribToFloat(attrX2, ref this.pt1.x);
                SVGSerializer.AttribToFloat(attrY2, ref this.pt1.y, invertY);
                return true;
            }

            public override bool SaveToSVGXML(System.Xml.XmlElement shapeEle, bool invertY)
            {
                shapeEle.SetAttributeNode("x1", this.pt0.x.ToString());
                shapeEle.SetAttributeNode("y1", SVGSerializer.InvertBranch(this.pt0.y, invertY).ToString());
                shapeEle.SetAttributeNode("x2", this.pt1.x.ToString());
                shapeEle.SetAttributeNode("y2", SVGSerializer.InvertBranch(this.pt1.y, invertY).ToString());

                return true;
            }

        }
    }
}
