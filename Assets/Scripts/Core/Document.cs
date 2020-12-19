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
        /// The root of a Berny system, used to hold the contents of curved graphics elements. It is designed
        /// to support features of the SVG file format.
        /// </summary>
        public class Document
        {
            /// <summary>
            /// The SVG document size, in meters.
            /// </summary>
            public Vector2 documentSize {get;set;} = new Vector2(1.0f, 1.0f);

            /// <summary>
            /// The layers of the document. These are based off Inkscape's layer system.
            /// </summary>
            List<Layer> layers = new List<Layer>();

#if DEVELOPMENT_BUILD || UNITY_EDITOR
            /// <summary>
            /// Debug ID. Each of this object created will have a unique ID that will be assigned the same way
            /// if each app session runs deterministicly the same. Used for identifying objects when
            /// debugging.
            /// </summary>
            int debugCounter;
#endif

            /// <summary>
            /// If true, the document has been changed since the last time it was prepared 
            /// for presentation.
            /// </summary>
            bool dirty = true;

            /// <summary>
            /// Enumerate through the document layers.
            /// </summary>
            /// <returns>An enumerator for the document's layers.</returns>
            public IEnumerable<Layer> Layers()
            { 
                return this.layers;
            }

            /// <summary>
            /// Document constructor.
            /// </summary>
            public Document()
            {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
                this.debugCounter = Utils.RegisterCounter();
#endif
            }

            /// <summary>
            /// Copy constructor. Create a document that's a deep
            /// copy of another document.
            /// </summary>
            /// <param name="src"></param>
            public Document(Document src)
            { 
                foreach(Layer layer in this.layers)
                { 
                    this.layers.Add( layer.Clone());
                }
            }

            /// <summary>
            /// Get the first layer in the document.
            /// 
            /// If the document is empty, a first layer is 
            /// automatically created.
            /// </summary>
            /// <returns>The first layer in the document.</returns>
            public Layer GetFirstLayer()
            {
                if(this.layers.Count == 0)
                    return this.AddLayer();
        
                return this.layers[0];
            }

            /// <summary>
            /// Create a new layer and add it to the document.
            /// </summary>
            /// <returns>The layer created layer.</returns>
            public Layer AddLayer()
            {
                Layer l = new Layer(this);
                this.layers.Add(l);
                return l;
            }

            /// <summary>
            /// Add a rectangle shape to the document.
            /// </summary>
            /// <param name="pos">The center of the rectangle.</param>
            /// <param name="rad">The radius of the rectangle.</param>
            /// <returns>The created rectangle shape.</returns>
            public BShape AddRectangle(Vector2 pos, Vector2 rad)
            {
                BShape shape = new BShape(pos, 0.0f);

                BNode.BezierInfo ptTL = new BNode.BezierInfo(new Vector2(-rad.x,  rad.y));
                BNode.BezierInfo ptTR = new BNode.BezierInfo(new Vector2( rad.x,  rad.y));
                BNode.BezierInfo ptBL = new BNode.BezierInfo(new Vector2(-rad.x, -rad.y));
                BNode.BezierInfo ptBR = new BNode.BezierInfo(new Vector2( rad.y, -rad.y));

                BLoop loop = new BLoop(shape, ptTL, ptTR, ptBR, ptBL);

                Layer layer = this.GetFirstLayer();
                layer.shapes.Add(shape);
                shape.layer = layer;
                return shape;
            }

            //public BShape AddEllipse(Vector2 position, Vector2 radius)
            //{ 
            //
            //}
            //
            //public BShape AddCircle(Vector2 position, float radius)
            //{ 
            //}

            /// <summary>
            /// Flag the document as dirty. A dirty document means that is has been modified
            /// since the last time it was prepared for presentation.
            /// </summary>
            public void FlagDirty()
            { 
                this.dirty = true;
            }

            /// <summary>
            /// Returns if the document is flagged as dirty.
            /// </summary>
            /// <returns></returns>
            public bool IsDirty()
            { 
                return this.dirty;
            }

            /// <summary>
            /// Prepares the document for presentation and clear the dirty flag.
            /// </summary>
            public void FlushDirty()
            { 
                foreach(Layer l in this.layers)
                    l.FlushDirty();

                this.dirty = false;
            }

            public IEnumerable<Layer> EnumerateLayers()
            {
                return this.layers;
            }

            public IEnumerable<BShape> EnumerateShapes()
            { 
                foreach(Layer layer in this.layers)
                { 
                    foreach(BShape bs in layer.shapes)
                        yield return bs;

                }
            }

            public IEnumerable<BLoop> EnumerateLoops()
            { 
                foreach(Layer layer in this.layers)
                { 
                    foreach(BShape bs in layer.shapes)
                    { 
                        foreach(BLoop bl in bs.loops)
                            yield return bl;
                    }
                }
            }

            /// <summary>
            /// Enumerate through all the nodes of the document.
            /// 
            /// Note that it's very possible the nodes will be spread across the
            /// layers, shapes, loops and islands of the entire document.
            /// </summary>
            /// <returns>An enumerator for all the nodes in the document.</returns>
            public IEnumerable<BNode> EnumerateNodes()
            { 
                foreach(Layer layer in this.layers)
                {
                    foreach(BShape bs in layer.shapes)
                    { 
                        foreach(BLoop bl in bs.loops)
                        { 
                            foreach(BNode bn in bl.nodes)
                                yield return bn;
                        }
                    }
                }
            }

            /// <summary>
            /// Check the content and datastructures in the document to make sure
            /// they are valid.
            /// </summary>
            public void TestValidity()
            { 
                foreach(Layer l in this.layers)
                { 
                    if(l.document != this)
                        Debug.LogError("Validity Error: Document shape referencing incorrect parent document.");

                    l.TestValidity();
                }

                Debug.Log("Finished validity check.");
            }

            /// <summary>
            /// Create a deep copy of the document.
            /// </summary>
            /// <returns>A new document with a duplicate of the invoking document's content.</returns>
            public Document Clone()
            { 
                return new Document(this);
            }

            /// <summary>
            /// Clear all the contents of the document.
            /// </summary>
            public void Clear()
            { 
                this.layers.Clear();
                this.FlagDirty();
            }
        }
    } 
}