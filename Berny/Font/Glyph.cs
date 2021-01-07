using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PxPre
{
    namespace Berny
    {
        namespace Font
        {
            /// <summary>
            /// A loaded point in the glyph.
            /// </summary>
            public struct Point
            { 
                /// <summary>
                /// Flag data for the point.
                /// </summary>
                [System.Flags]
                public enum Flags
                { 
                    /// <summary>
                    /// If true the point is a quadratic Bezier control point.
                    /// </summary>
                    /// <remarks>For TTF, ignore for TTF/OTF.</remarks>
                    Control         = 1 << 0,

                    /// <summary>
                    /// If true, the point was loaded from file as a Bezier with an incoming
                    /// control point.
                    /// </summary>
                    /// <remarks>For CFF fonts, ignore for TTF/OTF.</remarks>
                    UsesTangentIn = 1 << 1,

                    /// <summary>
                    /// If true, the point was loaded from file as a Bezier with an outgoing
                    /// control point.
                    /// </summary>
                    /// <remarks>For CFF fonts, ignore for TTF/OTF.</remarks>
                    UsesTangentOut = 1 << 2,

                    /// <summary>
                    /// Marks the point as implied - which are points that weren't explicitly
                    /// in the font data, but who's points could be calculated from its data -
                    /// usually by averaging consecutive quadratic control points.
                    /// </summary>
                    Implied = 1 << 3
                }

                /// <summary>
                /// Flag data for the point.
                /// </summary>
                public Flags flags;

                /// <summary>
                /// The position of the point.
                /// </summary>
                public Vector2 position;

                /// <summary>
                /// Incoming tangent.
                /// </summary>
                /// <remarks>Only relevant in CCFs</remarks>
                public Vector2 tangentIn;

                /// <summary>
                /// Outgoing tangent.
                /// </summary>
                public Vector2 tangentOut;

                /// <summary>
                /// Constructor.
                /// </summary>
                /// <param name="point">Point position.</param>
                public Point(Vector2 point)
                { 
                    this.flags = 0;
                    this.position = point;
                    this.tangentIn = Vector2.zero;
                    this.tangentOut = Vector2.zero;
                }

                /// <summary>
                /// Constructor.
                /// </summary>
                /// <param name="point">Point position.</param>
                /// <param name="flags">Point flag values.</param>
                public Point(Vector2 point, Flags flags)
                { 
                    this.flags = flags;
                    this.position = point;
                    this.tangentIn = Vector2.zero;
                    this.tangentOut = Vector2.zero;
                }

                /// <summary>
                /// Convenience property for the control flag.
                /// </summary>
                public bool isControl 
                {
                    get => (this.flags & Flags.Control) != 0; 
                    set
                    { 
                        if(value == true)
                            this.flags = this.flags | Flags.Control;
                        else
                            this.flags = this.flags & ~Flags.Control;
                    }
                }

                /// <summary>
                /// Convenience property for the UseTangentIn flag.
                /// </summary>
                public bool useTangentIn
                {
                    get => (this.flags & Flags.UsesTangentIn) != 0;
                    set
                    { 
                        if(value == true)
                            this.flags = this.flags | Flags.UsesTangentIn;
                        else
                            this.flags = this.flags & ~Flags.UsesTangentIn;
                    }
                }

                /// <summary>
                /// Convenience property for the UseTangentOut flag.
                /// </summary>
                public bool useTangentOut
                { 
                    get => (this.flags & Flags.UsesTangentOut) != 0;
                    set
                    { 
                        if(value == true)
                            this.flags = this.flags | Flags.UsesTangentOut;
                        else
                            this.flags = this.flags & ~Flags.UsesTangentOut;
                    }
                }

                /// <summary>
                /// Control the implied flag.
                /// </summary>
                public bool implied
                { 
                    get => (this.flags & Flags.Implied) != 0;
                    set
                    { 
                        if(value == true)
                            this.flags = this.flags | Flags.Implied;
                        else
                            this.flags = this.flags & ~Flags.Implied;
                    }
                }
            }

            /// <summary>
            /// A path in a font glyph.
            /// </summary>
            public class Contour
            { 
                /// <summary>
                /// The ordered (closed) path for the contour.
                /// </summary>
                public List<Point> points = new List<Point>();
            }

            /// <summary>
            /// A representation of a loaded (format-independent) font glyph.
            /// </summary>
            public class Glyph
            {

                public struct CompositeReference
                { 
                    public Vector2 offset;
                    public Vector2 xAxis;
                    public Vector2 yAxis;
                    public int glyphRef;
                }

                /// <summary>
                /// The minimum (bottom left) of the glyph's bounding box.
                /// </summary>
                /// <remarks>Not always guaranteed to be filled in.</remarks>
                public Vector2 min;

                /// <summary>
                /// The maximum (top right) of the glyph's bounding box.
                /// </summary>
                /// <remarks>Not always guaranteed to be filled in.</remarks>
                public Vector2 max;

                /// <summary>
                /// The left side bearing of the glyph.
                /// </summary>
                /// <remarks>Not always guaranteed to be filled in. Defaulted to zero.</remarks>
                /// <remarks>May not be useful, as the LSB may be baked into the glyph's shape.</remarks>
                public float leftSideBearing = 0.0f;

                /// <summary>
                /// How much to horizontally advance the placement cursor upon placing the glyph.
                /// </summary>
                public float advance = 1.0f;

                /// <summary>
                /// The various contours of the shape. 
                /// 
                /// Clockwise values are positive fills, counter clockwise are negative.
                /// </summary>
                public List<Contour> contours = new List<Contour>();

                public List<CompositeReference> compositeRefs = null;

                /// <summary>
                /// Scale the contents of the font.
                /// </summary>
                /// <param name="s">The scale amount.</param>
                /// <param name="scaleAdv">
                /// If true, everything is scaled. 
                /// If false, only the point data (including their tangents) are scaled.</param>
                public void Scale(float s, bool scaleAdv = true)
                { 
                    foreach(Contour c in this.contours)
                    { 
                        for(int i = 0; i < c.points.Count; ++i)
                        { 
                            Point p = c.points[i];
                            p.position *= s;
                            p.tangentIn *= s;
                            p.tangentOut *= s;
                            c.points[i] = p;
                        }
                    }

                    if(scaleAdv == true)
                    { 
                        this.min *= s;
                        this.max *= s;
                        this.leftSideBearing *= s;
                        this.advance *= s;
                    }
                }
            }
        }
    }
}
