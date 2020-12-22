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
        /// A Bezier shape. This is meant to be the equivalent of a path in an SVG document.
        /// </summary>
        public class BShape
        {
            /// <summary>
            /// The various ways corners in an SVG file can be formed.
            /// </summary>
            public enum Corner
            { 
                /// <summary>
                /// A half circle to round the end.
                /// </summary>
                Round,

                /// <summary>
                /// A flat connection
                /// </summary>
                Bevel,

                /// <summary>
                /// Extrapolate sides with straight lines to where they collide.
                /// </summary>
                Miter,

                /// <summary>
                /// Extrapolate sides continuing their curve to where they collide.
                /// </summary>
                Arc
            }

            /// <summary>
            /// The style of an unconnected edge's end.
            /// </summary>
            public enum Cap
            { 
                /// <summary>
                /// Stop instantly.
                /// </summary>
                Butt,

                /// <summary>
                /// round it out with a half circle.
                /// </summary>
                Round,

                /// <summary>
                /// Add an additional half square, based off the width of the edge.
                /// </summary>
                Square
            }

            /// <summary>
            /// The order the render the elements of the shape - similar to Inkscape.
            /// Enum currently unused. It maybe removed in the future.
            /// </summary>
            public enum Order
            { 
                FillStrokeMarker,
                StrokeFillMarker,
                FillMarkerStroke,
                MarkersFillStroke,
                StrokeMarkersFill,
                MarkerStrokeFill
            }

            /// <summary>
            /// The parent layer of the shape.
            /// 
            /// If this is set, the layer should reference this shape as a child in their
            /// Layers.shapes variable.
            /// </summary>
            public Layer layer;

            /// <summary>
            /// The SVG name of the shape (path).
            /// </summary>
            public string name;

            /// <summary>
            /// The loops contained in the shape. If a loop is 
            /// </summary>
            public List<BLoop> loops = new List<BLoop>();

            /// <summary>
            /// The position of the shape in the document. Currently unused.
            /// </summary>
            public Vector2 docPos; // Probably will be removed.

            /// <summary>
            /// The rotation of the shape in the document. Current unused.
            /// </summary>
            float rotation;// Probably will be remove

            /// <summary>
            /// Inkscape pivot point for rotation, skew and scale operations.
            /// </summary>
            public Vector2 editPivot;

            /// <summary>
            /// If true, closed shapes should be filled in, else the shape is left hollow.
            /// If the shape is not closed, a shape will not be filled in.
            /// </summary>
            public bool fill = true;

            /// <summary>
            /// The color of the shape's fill. If 
            /// </summary>
            public Color fillColor = Color.white;

            public bool stroke = true;

            /// <summary>
            /// The stroke of the shape's edges. Only relevant if the shape 
            /// is set to render a stroke outline.
            /// </summary>
            public Color strokeColor = Color.black;

            /// <summary>
            /// The width of the shape's outline. In meters. Only relevant if 
            /// the shape is set to render a stroke outline.
            /// </summary>
            public float strokeWidth = 0.001f;
            public float maxMitreLength = 4.0f;

            /// <summary>
            /// The type of rendering for corner connections.
            /// </summary>
            public Corner corner = Corner.Round;

            /// <summary>
            /// The type of rendering for edge caps.
            /// </summary>
            public Cap cap = Cap.Butt;

            /// <summary>
            /// The type of order for rendering elements.
            /// </summary>
            public Order order = Order.FillStrokeMarker; // Currently unused - maybe removed.

            /// <summary>
            /// If true, the shape has been modified since the last time data was formally
            /// prepared for presentation.
            /// </summary>
            bool dirty = true;

            public BShapeGen shapeGenerator = null;

            // Currently unused, planned for non-path SVG shapes
            //public string objectType = string.Empty;
            //public Dictionary<string, float> objectFloats = new Dictionary<string, float>();
            //public Dictionary<string, int> objectInts = new Dictionary<string, int>();
            //public Dictionary<string, string> objectString = new Dictionary<string, string>();

#if DEVELOPMENT_BUILD || UNITY_EDITOR
            /// <summary>
            /// Debug ID. Each of this object created will have a unique ID that will be assigned the same way
            /// if each app session runs deterministicly the same. Used for identifying objects when
            /// debugging.
            /// </summary>
            int debugCounter;
#endif

            /// <summary>
            /// Convert a cap enum to its SVG string value.
            /// </summary>
            /// <param name="c">The cap value to convert.</param>
            /// <returns>The SVG string value.</returns>
            public static string CapToString(Cap c)
            {
                switch(c)
                {
                    default:
                    case Cap.Butt:
                        return "butt";

                    case Cap.Round:
                        return "round";

                    case Cap.Square:
                        return "square";
                }
            }

            /// <summary>
            /// Convert a cap's SVG string value to enum.
            /// </summary>
            /// <param name="str">The string value to convert.</param>
            /// <returns>The converted cap.</returns>
            public static Cap StringToCap(string str)
            { 

                if(str == "square")
                    return Cap.Square;

                if(str == "round")
                    return Cap.Round;

                return Cap.Butt;

            }

            /// <summary>
            /// Converts a corner enum to its SVG string value.
            /// </summary>
            /// <param name="c">The corner value to convert.</param>
            /// <returns>The SVG string value.</returns>
            public static string CornerToString(Corner c)
            {
                switch (c)
                {
                    case Corner.Round:
                        return "round";

                    case Corner.Bevel:
                        return "bevel";

                    default:
                    case Corner.Miter:
                        return "miter";
                }
                
            }

            /// <summary>
            /// Convert a corner's SVG string value to enum.
            /// </summary>
            /// <param name="str">The string value to convert.</param>
            /// <returns>The converted corner.</returns>
            public static Corner StringToCorner(string str)
            { 
                if(str == "round")
                    return Corner.Round;

                if(str == "bevel")
                    return Corner.Bevel;

                return Corner.Miter;
            }

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="docPos">The position of the shape.</param>
            /// <param name="rotation">The object rotation of the shape.</param>
            public BShape(Vector2 docPos, float rotation)
            { 
                this.docPos = docPos;
                this.rotation = rotation;

#if DEVELOPMENT_BUILD || UNITY_EDITOR
                this.debugCounter = Utils.RegisterCounter();
#endif
            }

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="docPos">The position of the shape.</param>
            /// <param name="rotation">The object rotation of the shape.</param>
            /// <param name="bis">The loop to initialize the shape with.</param>
            public BShape(Vector2 docPos, float rotation, params BNode.BezierInfo [] bis)
            {
                this.docPos = docPos;
                this.rotation = rotation;

                this.debugCounter = Utils.RegisterCounter();
            }

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="docPos">The position of the shape.</param>
            /// <param name="rotation">The object rotation of the shape.</param>
            /// <param name="initialLoop">The loop to initialize the shape with.</param>
            public BShape(Vector2 docPos, float rotation, BLoop initialLoop)
            { 
                this.docPos = docPos;
                this.rotation = rotation;

                if(initialLoop != null)
                    this.AddLoop(initialLoop);

#if DEVELOPMENT_BUILD || UNITY_EDITOR
                this.debugCounter = Utils.RegisterCounter();
#endif
            }

            /// <summary>
            /// Prepare the shape for presentation.
            /// 
            /// This will also clear the dirty flag.
            /// </summary>
            public void FlushDirty()
            { 
                if(this.shapeGenerator != null)
                    this.shapeGenerator.Reconstruct();

                foreach (BLoop bl in this.loops)
                    bl.FlushDirty();

                this.dirty = false;
            }

            /// <summary>
            /// Checks if the shape is flagged as dirty.
            /// 
            /// If the shape is dirty, that means the shape has been edited since the
            /// last time it was prepared for presentation.
            /// </summary>
            /// <returns>The shape's dirty state.</returns>
            public bool IsDirty()
            { 
                return this.dirty;
            }

            /// <summary>
            /// Flag the shape as dirty.
            /// </summary>
            public void FlagDirty()
            { 
                this.dirty = true;

                if(this.layer != null)
                    this.layer.FlagDirty();
            }

            /// <summary>
            /// Add a loop to the shape.
            /// </summary>
            /// <param name="loop">The loop to add.</param>
            public void AddLoop(BLoop loop)
            { 
                if(loop.shape != null)
                { 
                    if(loop.shape == this)
                        return;

                    loop.shape.loops.Remove(loop);
                }

                loop.shape = this;
                this.loops.Add(loop);
                this.FlagDirty();
            }

            public BLoop AddLoop()
            { 
                BLoop loop = new BLoop(this);
                this.AddLoop(loop);
                return loop;
            }

            public BLoop AddLoop(params BNode.BezierInfo [] pathData)
            { 
                BLoop loop = new BLoop(this, pathData);
                this.AddLoop(loop);
                return loop;
            }

            /// <summary>
            /// Enumerate through all the nodes in the shape.
            /// </summary>
            /// <returns>An enumerator for all the nodes in the shape.</returns>
            /// <remarks>The nodes may be on different loops and islands.</remarks>
            public IEnumerable<BNode> EnumerateNodes()
            { 
                foreach(BLoop bl in this.loops)
                { 
                    foreach(BNode bn in bl.nodes)
                        yield return bn;
                }
            }

            /// <summary>
            /// Test the validity of the variables and datastructures to make sure all the
            /// rule are correct and that the data isn't corrupt.
            /// </summary>
            public void TestValidity()
            { 
                foreach(BLoop bl in this.loops)
                { 
                    if(bl.shape != this)
                        Debug.LogError("Validity Error: Shape's loop doesnt not match the reference to the loop's shape parent.");

                    bl.TestValidity();
                }
            }

            /// <summary>
            /// Create a deep copy of the shape.
            /// </summary>
            /// <param name="layer">The layer to insert the shape into.</param>
            /// <returns>The duplicate layer.</returns>
            public BShape Clone(Layer layer)
            { 
                BShape ret = new BShape(this.docPos, this.rotation);
                ret.layer = layer;
                if(layer != null)
                    layer.shapes.Add(ret);

                Dictionary<BNode, BNode> conversion = new Dictionary<BNode, BNode>();

                foreach(BLoop loop in this.loops)
                { 
                    BLoop newLoop = new BLoop(ret);
                    foreach(BNode bn in loop.nodes)
                    { 
                        BNode newNode = bn.Clone(newLoop);
                        newLoop.nodes.Add(newNode);

                        conversion.Add(bn, newNode);
                    }

                    foreach(BNode bn in newLoop.nodes)
                    { 
                        if(bn.prev != null)
                            bn.prev = conversion[bn.prev];

                        if(bn.next != null)
                            bn.next = conversion[bn.next];
                    }
                }

                return ret;

            }

            public void Clear()
            {
                if(this.loops.Count == 0)
                    return;

                this.loops.Clear();
                this.FlagDirty();
            }
        }
    }
}