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

namespace PxPre.Berny
{
    /// <summary>
    /// Procedural path generator for shapes.
    /// </summary>
    public abstract class BShapeGen
    { 
        /// <summary>
        /// The shape that the generator is modifying.
        /// </summary>
        public readonly BShape shape;

        /// <summary>
        /// The identifier for the shape.
        /// </summary>
        /// <remarks>Since procedural shapes were designed to implement SVG shapes,
        /// the type name will often match the attribute name for SVG files, for
        /// convenience.</remarks>
        public abstract string ShapeType {get;}

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="shape">The parent shape for the generator to modify.</param>
        public BShapeGen(BShape shape)
        { 
            this.shape = shape;
        }

        /// <summary>
        /// Flag the generator's shape as dirty.
        /// </summary>
        public void FlagDirty()
        { 
            this.shape.FlagDirty();
        }

        /// <summary>
        /// Regenerate the path in the shape.
        /// </summary>
        public abstract void Reconstruct();

        /// <summary>
        /// The name of the shape for SVGs.
        /// </summary>
        public virtual string GetSVGXMLName {get => this.ShapeType;}

        /// <summary>
        /// Load the generator's properties from an SVG/XML source.
        /// </summary>
        /// <param name="shapeEle">The XML source.</param>
        /// <param name="invertY">If true, loaded Y values should be inverted when loaded.</param>
        /// <returns>True if successful. Else, false.</returns>
        public abstract bool LoadFromSVGXML(System.Xml.XmlElement shapeEle, bool invertY);

        /// <summary>
        /// Save the generator's properties into an XML source for an SVG file.
        /// </summary>
        /// <param name="shapeEle">The XML destination.</param>
        /// <param name="invertY">If true, the saved values should be inverted when loaded.</param>
        /// <returns>True if successful. Else, false.</returns>
        public abstract bool SaveToSVGXML(System.Xml.XmlElement shapeEle, bool invertY);
    }
}