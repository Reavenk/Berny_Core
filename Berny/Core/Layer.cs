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

namespace PxPre.Berny
{
    /// <summary>
    /// A layer in the document. Designed to mimic an Inkscape layer.
    /// </summary>
    /// <remarks>While Inkscape works with SVG documents, the layer is analagous to
    /// Inkscape layers and not SVGs, because SVGs don't actually have layers. Instead,
    /// Inkscape has special SVG groups that are labeled as layers.</remarks>
    public class Layer
    {
        /// <summary>
        /// The parent document the layer belongs to.
        /// </summary>
        public Document document;

        /// <summary>
        /// The children shapes in the layer.
        /// </summary>
        public List<BShape> shapes = new List<BShape>();

        /// <summary>
        /// The name of the layer.
        /// </summary>
        public string name = "";

        /// <summary>
        /// If true, the layer is locked. This variable does not actually
        /// do anything on its own. Instead, it's merely used to keep 
        /// track of a state that can be referenced by anything that cares
        /// to use this information.
        /// </summary>
        bool locked = false;

        /// <summary>
        /// If true, the layer is visible. Else, false. This variable does 
        /// not actually do anything on its own. Instead, it's merely used
        /// to keep track of a state that can be referenced by anything that
        /// cares to use this information.
        /// </summary>
        bool visible = true;

        /// <summary>
        /// Public accessor and modifier for the locked state.
        /// </summary>
        public bool Locked 
        {
            get => this.locked;
            set { this.locked = value; }
        }

        /// <summary>
        /// Public accessor and modifier for the visible state.
        /// </summary>
        public bool Visible
        { 
            get => this.visible;
            set 
            { 
                if(this.visible == value)
                    return;

                this.visible = value;
                this.FlagDirty();
            }
        }

        /// <summary>
        /// The dirty flag. Set by FlagDirty() and reset by FlushDirty().
        /// </summary>
        bool dirty = true;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="doc">Parent document.</param>
        public Layer(Document doc)
        { 
            this.document = doc;
        }

        /// <summary>
        /// Flag the layer as dirty.
        /// </summary>
        public void FlagDirty()
        {
            this.dirty = true;

            if(this.document != null)
                this.document.FlagDirty();
        }

        /// <summary>
        /// Query if the layer is dirty.
        /// </summary>
        /// <returns>True, if the layer is dirty. Else, false.</returns>
        public bool IsDirty()
        { 
            return this.dirty;
        }

        /// <summary>
        /// Update the object and flush the dirty flag.
        /// </summary>
        public void FlushDirty()
        {
            foreach (BShape bs in this.shapes)
            {
                bs.FlushDirty();
            }

            this.dirty = false;
        }

        /// <summary>
        /// Create an IEnumerable for all the shapes in the layer.
        /// </summary>
        /// <returns>The IEnumerable for all shapes in the layer.</returns>
        public IEnumerable<BShape> Shapes()
        { 
            return this.shapes;
        }

        /// <summary>
        /// Test the validity of the layer. Not meant to be used in practice.
        /// </summary>
        public void TestValidity()
        {
            foreach (BShape bs in this.shapes)
            {
                if (bs.layer != this)
                    Debug.LogError("Validity Error: Document layer referencing incorrect parent document.");

                bs.TestValidity();
            }

            Debug.Log("Finished validity check.");
        }

        /// <summary>
        /// Create an empty shape parented to this layer.
        /// </summary>
        /// <returns>The created shape.</returns>
        public BShape CreateEmptyShape()
        { 
            BShape bs = new BShape(Vector2.zero, 0.0f);
            bs.layer = this;
            this.shapes.Add(bs);
            return bs;
        }

        /// <summary>
        /// Create a deep copy of the layer.
        /// </summary>
        /// <returns></returns>
        public Layer Clone()
        { 
            return Clone(this.document);
        }

        /// <summary>
        /// Create a deep copy of the layer.
        /// </summary>
        /// <param name="doc">The copy's parent document.</param>
        /// <returns></returns>
        public Layer Clone(Document doc)
        {
            Layer ret = null;
            if(doc != null)
                ret = doc.AddLayer();
            else
                ret = new Layer(null);

            ret.locked = this.locked;
            ret.visible = this.visible;
            ret.name = this.name;

            foreach(BShape shape in this.shapes)
                this.shapes.Add(shape.Clone(this));

            return new Layer(this.document);
        }

        /// <summary>
        /// Create a shape that's generated by a circle. And add that circle
        /// to the layer.
        /// </summary>
        /// <param name="center">The center of the circle.</param>
        /// <param name="radius">The radius of the circle.</param>
        /// <returns></returns>
        public BShapeGenCircle AddCircle(Vector2 center, float radius)
        { 
            BShape bs = new BShape(Vector2.zero, 0.0f);
            bs.layer = this;
            this.shapes.Add(bs);

            BShapeGenCircle gen = new BShapeGenCircle(bs, center, radius);
            bs.shapeGenerator = gen;
            gen.FlagDirty();
            return gen;
        }

        /// <summary>
        /// Create a shape that's generated by an ellipse. And add that ellipse
        /// to the layer.
        /// </summary>
        /// <param name="center">The center of the ellipse.</param>
        /// <param name="radius">The radius of the ellipse.</param>
        /// <returns>The generator for the ellipse.</returns>
        public BShapeGenEllipse AddEllipse(Vector2 center, Vector2 radius)
        {
            BShape bs = new BShape(Vector2.zero, 0.0f);
            bs.layer = this;
            this.shapes.Add(bs);

            BShapeGenEllipse gen = new BShapeGenEllipse(bs, center, radius);
            bs.shapeGenerator = gen;
            gen.FlagDirty();
            return gen;
        }

        /// <summary>
        /// Create a shape that's generated by a rectangle. And add that rectangle
        /// to the layer.
        /// </summary>
        /// <param name="pos">The center of the rectangle.</param>
        /// <param name="dim">The radius of the rectangle.</param>
        /// <param name="round">The beveling of the rectangles.</param>
        /// <returns>The generator for the rectangle.</returns>
        public BShapeGenRect AddRect(Vector2 pos, Vector2 dim, Vector2 ? round = null)
        {
            BShape bs = new BShape(Vector2.zero, 0.0f);
            bs.layer = this;
            this.shapes.Add(bs);

            BShapeGenRect gen = new BShapeGenRect(bs, pos, dim, round);
            bs.shapeGenerator = gen;
            gen.FlagDirty();
            return gen;
        }

        /// <summary>
        /// Create a shape that's generated by a polygon. And add that shape
        /// to the layer.
        /// </summary>
        /// <param name="points"The points in the polygon.></param>
        /// <returns>The generator for the polygon.</returns>
        public BShapeGenPolygon AddPolygon(params Vector2 [] points)
        {
            BShape bs = new BShape(Vector2.zero, 0.0f);
            bs.layer = this;
            this.shapes.Add(bs);

            BShapeGenPolygon gen = new BShapeGenPolygon(bs, points);
            bs.layer = this;
            gen.FlagDirty();
            return gen;
        }

        /// <summary>
        /// Create shape that's generated by a polyline. And add that shape
        /// to the layer.
        /// </summary>
        /// <param name="points">The points in the polyline.</param>
        /// <returns>The generator for the polyline.</returns>
        public BShapeGenPolyline AddPolyline(params Vector2 [] points)
        {
            BShape bs = new BShape(Vector2.zero, 0.0f);
            bs.layer = this;
            this.shapes.Add(bs);

            BShapeGenPolyline gen = new BShapeGenPolyline(bs, points);
            bs.layer = this;
            gen.FlagDirty();
            return gen;
        }
    }
}