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
        public class BShapeGenPolyline : BShapeGen
        {
            // https://www.w3schools.com/graphics/svg_polyline.asp

            public override string ShapeType => "polyline";

            public List<Vector2> polyPoints = new List<Vector2>();

            public BShapeGenPolyline(BShape shape, params Vector2 [] points)
                : base(shape)
            { 
                this.polyPoints = new List<Vector2>(points);
            }

            public int PointCt()
            {
                return this.polyPoints.Count;
            }

            public int AddPoint(Vector2 v2)
            {
                int idx = this.polyPoints.Count;
                this.polyPoints.Add(v2);
                this.FlagDirty();
                return idx;
            }

            public Vector2 GetPoint(int idx)
            {
                return this.polyPoints[idx];
            }

            public override void Reconstruct()
            {
                this.shape.Clear();

                BLoop bl = this.shape.AddLoop();

                BNode lastNode = null;
                foreach(Vector2 v2 in this.polyPoints)
                { 
                    BNode bn = new BNode(bl,v2);
                    bl.nodes.Add(bn);

                    if(lastNode != null)
                    { 
                        lastNode.next = bn;
                        bn.prev = lastNode;
                    }

                    lastNode = bn;
                }

                this.FlagDirty();
            }

            public override bool LoadFromSVGXML(System.Xml.XmlElement shapeEle)
            {
                System.Xml.XmlAttribute attrPoints = shapeEle.GetAttributeNode("points");

                if(attrPoints == null)
                    return false;

                string val = attrPoints.Value;
                this.polyPoints = SVGSerializer.SplitPointsString(val);

                return true;
            }

            public override bool SaveToSVGXML(System.Xml.XmlElement shapeEle)
            {
                shapeEle.SetAttributeNode(
                    "points", 
                    SVGSerializer.PointsToPointsString(this.polyPoints));

                return true;
            }
        }
    }
}
