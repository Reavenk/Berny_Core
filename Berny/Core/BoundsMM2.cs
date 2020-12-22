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

using UnityEngine;

namespace PxPre
{
    namespace Berny
    {
        /// <summary>
        /// 2D bounding box.
        /// </summary>
        public struct BoundsMM2
        {
            /// <summary>
            /// The minimum corner.
            /// </summary>
            public Vector2 min;

            /// <summary>
            /// The maximum corner.
            /// </summary>
            public Vector2 max;

            /// <summary>
            /// The width of the box.
            /// </summary>
            public float Width { get => this.max.x - this.min.x; }

            /// <summary>
            /// The height of the box.
            /// </summary>
            public float Height { get => this.max.y - this.min.y; }

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="min">The minimum point.</param>
            /// <param name="max">The maximum point.</param>
            public BoundsMM2(Vector2 min, Vector2 max)
            { 
                this.min = min;
                this.max = max;
            }

            /// <summary>
            /// Create a bounding box that represents infinite space.
            /// </summary>
            /// <returns>A bounding box representing infinite space.</returns>
            public static BoundsMM2 GetInifiniteRegion()
            { 
                return new BoundsMM2( 
                    new Vector2(float.PositiveInfinity, float.PositiveInfinity), 
                    new Vector2(float.NegativeInfinity, float.NegativeInfinity));
            }

            /// <summary>
            /// Given two points, create a bounding box that contains them.
            /// </summary>
            /// <param name="a">A point to contain in the bounding box.</param>
            /// <param name="b">A point to contain in the bounding box.</param>
            /// <returns>A bounding box that contains both parameters a and b.</returns>
            public static BoundsMM2 GetBoundsAroundPoints(Vector2 a, Vector2 b)
            { 
                return 
                    new BoundsMM2( 
                        new Vector2(Mathf.Min(a.x, b.x), Mathf.Min(a.y, b.y)), 
                        new Vector2(Mathf.Max(a.x, b.x), Mathf.Max(a.y, b.y)));
            }

            /// <summary>
            /// Expand the bounding box to ensure another bounding box is contained
            /// inside of it.
            /// </summary>
            /// <param name="other">The bounding box to contain.</param>
            public void Union(BoundsMM2 other)
            { 
                this.min.x = Mathf.Min(this.min.x, other.min.x);
                this.min.y = Mathf.Min(this.min.y, other.min.y);
                this.max.x = Mathf.Max(this.max.x, other.max.x);
                this.max.y = Mathf.Max(this.max.y, other.max.y);
            }

            /// <summary>
            /// Checks if the bounding box touches or overlaps another bounding box.
            /// </summary>
            /// <param name="other">The bounding box to check collision against.</param>
            /// <returns></returns>
            public bool Intersects(BoundsMM2 other)
            {
                // If a BB's min is farther to the right of the other's,
                // they can't possibly be touching.
                if(this.min.x > other.max.x || other.min.x > this.max.x)
                    return false;

                // If a BB's max is farther to the left of the other's,
                // they can't possibly be touching.
                if (this.min.y > other.max.y || other.min.y > this.max.y)
                    return false;

                // same things for the Y component.
                if (this.max.x < other.min.x || other.max.x < this.min.x)
                    return false;

                if (this.max.y < other.min.y || other.max.y < this.min.y)
                    return false;

                // Process of elimination, there's some kind of collision
                // or overlap.
                return true;
            }
        }
    }
}