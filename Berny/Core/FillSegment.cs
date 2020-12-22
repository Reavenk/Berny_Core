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
using System.Collections.Generic;

namespace PxPre 
{ 
    namespace Berny 
    {
        /// <summary>
        /// Similar to BSample, but simpler. Used for geometry processing.
        /// </summary>
        public class FillSegment
        {
            // This class is basically like BSample, but instead of holding a flattened
            // version of the path for outlining, it's used for calculating how to tessellate
            // the path when being filled with triangles.
            //
            // The structure is admittedly a bit odd as it's different from the rest of
            // the path linked lists. Instead of holding our position and referencing the
            // next position to make a segment, this IS the segment and actually has 
            // duplicated copies of the endpoint positions that we take great effort to
            // keep synced. This was done to make some implementation issues easier at the
            // time to get the algorithm up and running, but the cleaner implementation
            // will probably involve converting this to represent point instead of line
            // segments.

            /// <summary>
            /// The next segment in the chain.
            /// </summary>
            public FillSegment next;

            /// <summary>
            /// The previous segment in the chain.
            /// </summary>
            public Vector2 pos;

            /// <summary>
            /// The previous segment in the chain.
            /// </summary>
            public FillSegment prev;


#if DEVELOPMENT_BUILD || UNITY_EDITOR
            /// <summary>
            /// Debug ID. Each of this object created will have a unique ID that will be assigned the same way
            /// if each app session runs deterministically the same. Used for identifying objects when
            /// debugging.
            /// </summary>
            public readonly int debugCtr;
#endif

            public FillSegment()
            {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
                this.debugCtr = Utils.RegisterCounter();
#endif
            }

            public FillSegment(Vector2 v2)
            {
                this.pos = v2;

#if DEVELOPMENT_BUILD || UNITY_EDITOR
                this.debugCtr = Utils.RegisterCounter();
#endif
            }

            public Vector2 InflateDir()
            {
                if (this.prev == null)
                {
                    Vector2 v = this.next.pos - this.pos;
                    return new Vector2(-v.y, v.x).normalized;
                }
                else if (this.next == null)
                {
                    Vector2 v = this.prev.pos - this.pos;
                    return new Vector2(v.y, -v.x).normalized;
                }
                else
                {
                    Vector2 vp = this.next.pos - this.pos;
                    Vector2 vn = this.prev.pos - this.pos;

                    if (vp.SqrMagnitude() <= Mathf.Epsilon)
                        return new Vector2(-vn.y, vn.x).normalized;
                    else if (vn.SqrMagnitude() <= Mathf.Epsilon)
                        return new Vector2(vp.y, -vp.x).normalized;

                    vp.Normalize();
                    vn.Normalize();
                    vp = new Vector2(-vp.y, vp.x);
                    vn = new Vector2(vn.y, -vn.x);
                    Vector2 mid = (vp + vn).normalized;

                    float dot = Vector2.Dot(vp, mid);
                    return (1.0f / dot) * mid;
                }
            }

            public FillSegment Clone(bool copyLinks)
            { 
                FillSegment fs = new FillSegment();
                fs.pos = this.pos;

                if(copyLinks == true)
                { 
                    fs.prev = this.prev;
                    fs.next = this.next;
                }
                return fs;
            }

            public IEnumerable<FillSegment> Travel()
            { 
                FillSegment it = this;
                while(true)
                { 
                    yield return it;
                    it = it.next;

                    if(it == null || it == this)
                        yield break;
                }
            }
        }
    }
}