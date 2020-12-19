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
                public int flags;
                public bool control;
                public Vector2 position;
                public Vector2 tangentIn;
                public Vector2 tangentOut;
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
            }
        }
    }
}
