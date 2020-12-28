using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PxPre
{
    namespace Berny
    {
        namespace Font
        {
            public struct Point
            { 
                [System.Flags]
                public enum Flags
                { 
                    Control         = 1 << 0,
                    UsesTangentIn   = 1 << 1,
                    UsesTangentOut  = 1 << 2
                }

                public Flags flags;
                public Vector2 position;
                public Vector2 tangentIn;
                public Vector2 tangentOut;

                public Point(Vector2 point)
                { 
                    this.flags = 0;
                    this.position = point;
                    this.tangentIn = Vector2.zero;
                    this.tangentOut = Vector2.zero;
                }

                public Point(Vector2 point, Flags flags)
                { 
                    this.flags = flags;
                    this.position = point;
                    this.tangentIn = Vector2.zero;
                    this.tangentOut = Vector2.zero;
                }

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
            }

            public class Contour
            { 
                public List<Point> points = new List<Point>();
            }

            public class Glyph
            {
                public Vector2 min;
                public Vector2 max;

                public float leftSideBearing;
                public float advance;

                public List<Contour> contours = new List<Contour>();

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
