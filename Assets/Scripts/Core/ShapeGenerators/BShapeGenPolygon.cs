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
        public class BShapeGenPolygon : BShapeGen
        {
            // https://www.w3schools.com/graphics/svg_polygon.asp

            public enum FillRule
            { 
                NonZero,
                EvenOdd
            }

            FillRule fillRule = FillRule.NonZero;

            public override string ShapeType => "polygon";

            public FillRule FillingRule
            { 
                get => this.fillRule;
                set { this.fillRule = value; this.FlagDirty(); }
            }

            List<Vector2> points = new List<Vector2>();

            public BShapeGenPolygon(BShape shape, params Vector2 [] points)
                : base(shape)
            { 
                this.points = new List<Vector2>(points);
            }

            public int PointCt()
            { 
                return this.points.Count;
            }

            public int AddPoint(Vector2 v2)
            { 
                int idx = this.points.Count;
                this.points.Add(v2);
                this.FlagDirty();
                return idx;
            }

            public Vector2 GetPoint(int idx)
            { 
                return this.points[idx];
            }

            public override void Reconstruct()
            {
                this.shape.Clear();

                BLoop bl = 
                    new BLoop(
                        this.shape, 
                        true,
                        this.points.ToArray());

                // There should only be 1 island, but let's extract it he proper way.
                List<BNode> islands = bl.GetIslands( IslandTypeRequest.Closed);
                foreach(BNode isl in islands)
                {
                    
                    //List<BNode> islandSegs = new List<BNode>(isl.Travel());
                    //List<Utils.BezierSubdivSample> delCollisions = new List<Utils.BezierSubdivSample>();
                    ////
                    ////// Self collision
                    //Boolean.GetLoopCollisionInfo(islandSegs, islandSegs, delCollisions);
                    //
                    //// TODO: Based on the fill rule - do something I haven't decided/figured-out
                    //// yet.
                    //// (wleu 12/21/2020)
                    //if (this.fillRule == FillRule.NonZero)
                    //{ 
                    //}
                    //else if(this.fillRule == FillRule.EvenOdd)
                    //{ 
                    //}
                }

                this.FlagDirty();
            }

            public override bool LoadFromSVGXML(System.Xml.XmlElement shapeEle)
            {
                System.Xml.XmlAttribute attribPoints = shapeEle.GetAttributeNode("points");
                if(attribPoints != null && string.IsNullOrEmpty(attribPoints.Value) == false)
                    this.points = SVGSerializer.SplitPointsString(attribPoints.Value);

                System.Xml.XmlAttribute attribFillRule = shapeEle.GetAttributeNode("fill-rule");
                if(attribFillRule != null && string.IsNullOrEmpty(attribFillRule.Value) == false)
                { 
                    string fillRule = attribFillRule.Value.Trim();

                    if(fillRule == "evenodd")
                        this.fillRule = FillRule.EvenOdd;
                    else if(fillRule == "nonzero")
                        this.fillRule = FillRule.NonZero;
                }

                return true;
            }

            public override bool SaveToSVGXML(System.Xml.XmlElement shapeEle)
            {
                shapeEle.SetAttribute("points", SVGSerializer.PointsToPointsString(this.points));

                switch(this.fillRule)
                {
                    case FillRule.EvenOdd:
                        shapeEle.SetAttribute("fill-rule", "evenodd");
                        break;

                    case FillRule.NonZero:
                        shapeEle.SetAttribute("fill-rule", "nonzero");
                        break;
                }

                return true;
            }
        }
    }
}