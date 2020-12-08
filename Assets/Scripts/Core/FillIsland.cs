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

namespace PxPre
{
    namespace Berny
    {
        /// <summary>
        /// Represents a closed loop to be filled in with the ear clipping algorithm.
        /// </summary>
        public class FillIsland
        {
            /// <summary>
            /// The precalculated winding value of the shape.
            /// </summary>
            float cachedWinding;

            /// <summary>
            /// The public property of the shape's winding value.
            /// </summary>
            public float Winding {get=> cachedWinding; }

            /// <summary>
            /// A collection of all the segments left to be processed.
            /// </summary>
            public HashSet<FillSegment> segments = new HashSet<FillSegment>();

            /// <summary>
            /// Calculates and caches the winding value. 
            /// </summary>
            /// <returns>The calculated winding value.</returns>
            public float CalculateWinding()
            { 
                this.cachedWinding = 0.0f;

                FillSegment fsstart = this.GetAStartingPoint();
                FillSegment it = fsstart;

                while(true)
                { 
                    Vector2 towa = it.prev.nextPos - it.prev.prevPos;
                    Vector2 from = it.nextPos - it.prevPos;

                    this.cachedWinding += towa.x * from.y - towa.y * from.x;

                    it = it.next;
                    if(it == fsstart)
                        break;
                }

                return this.cachedWinding;
            }

            /// <summary>
            /// Get a point on the cyclical link list to start processing.
            /// </summary>
            /// <returns>A segment in the linked list.</returns>
            public FillSegment GetAStartingPoint()
            { 
                return GetAStartingPoint(this.segments);
            }

            /// <summary>
            /// Gets the first value it encounters from a FillSegment hash. Used for
            /// FillSegment.GetAStartingPoint().
            /// </summary>
            /// <param name="hs">The hash set to extract a value from.</param>
            /// <returns>The extracted segment.</returns>
            public static FillSegment GetAStartingPoint(HashSet<FillSegment> hs)
            {
                // Return a point, any point.
                foreach (FillSegment fs in hs)
                    return fs;

                return null;
            }

            /// <summary>
            /// Processes the segments in the island's link list until all segments 
            /// are gone and the shape is tesselated with triangles.
            /// </summary>
            /// <param name="triangles">The output of triangle mesh indices.</param>
            /// <param name="vectors">The output and manager of Vector2 vertices.</param>
            /// <remarks>This function will clear out the object.</remarks>
            public void ConsumeIntoTriangles(List<int> triangles, Vector2Repo vectors)
            { 
                // !DELME
                if(Utils.verboseDebug == true && this.TestValidity() == false)
                    throw new System.Exception("Error discovered in FillIsland.ConsumeIntoTriangles, aborting.");

                // Note that this currently doens't handle clipping through concave
                // edges.

                // Simple ear clipping. This will dissassemble the path and clear
                // the island. If this is not preffered, sue GetTriangles() instead with
                // consume set to false to make a sacraficial copy instead.

                float wind = this.CalculateWinding();
                // I doubt this will ever happen.
                if(wind == 0.0f)
                    wind = 1.0f;

                FillSegment lastAdded = null;
                FillSegment it = GetAStartingPoint();
                while(segments.Count > 2)
                { 
                    Vector2 towa = it.nextPos - it.prevPos;
                    Vector2 from = it.next.nextPos - it.next.prevPos;
                    float localWind = towa.x * from.y - towa.y * from.x;

                    bool skip = false;
                    // Wrong winding, can't earclip.
                    if(segments.Count != 3 && (wind >= 0.0f) != (localWind >= 0.0f))
                        skip = true;
                    else
                    {
                        FillSegment fsPtCheck = it;
                        while(true)
                        { 
                            if(Utils.PointInTriangle(fsPtCheck.nextPos, it.prevPos, it.nextPos, it.next.nextPos) == true)
                            { 
                                skip = true;
                                break;
                            }

                            fsPtCheck = fsPtCheck.next;
                            if(fsPtCheck == it)
                                break;
                        }
                    }

                    if(skip == true)
                    {
                        it = it.next;
                        if (it == lastAdded)
                            wind = this.CalculateWinding();

                        continue;
                    }

                    // Record the triangle
                    triangles.Add(vectors.GetVectorID(it.prevPos));
                    triangles.Add(vectors.GetVectorID(it.nextPos));
                    triangles.Add(vectors.GetVectorID(it.next.nextPos));


                    // Get rid of the prev (and don't advance the iterator) to
                    // clip out the vertice
                    FillSegment nextIt = it.next;
                    FillSegment prevIt = it.prev;
                    segments.Remove(it);
                    nextIt.prev = prevIt;
                    prevIt.next = nextIt;
                    nextIt.prevPos = it.prevPos;
                    //
                    lastAdded = it.prev;
                    it = nextIt;
                }
            }

            /// <summary>
            /// Create a deep copy of the object.
            /// </summary>
            /// <returns></returns>
            public FillIsland Clone()
            { 
                FillIsland fiNew = new FillIsland();

                // A dictionary that convert a key of the existing segment
                // to a mapped value of its created clone.
                Dictionary<FillSegment, FillSegment> cloneLookup = 
                    new Dictionary<FillSegment, FillSegment>();

                FillSegment starting = this.GetAStartingPoint();
                FillSegment it = starting;

                while(true)
                { 
                    FillSegment itCpy = new FillSegment();
                    itCpy.next = it.next;
                    itCpy.prev = it.prev;
                    itCpy.nextPos = it.nextPos;
                    itCpy.prevPos = it.prevPos;

                    fiNew.segments.Add(itCpy);
                    cloneLookup.Add(it, itCpy);

                    it = it.next;
                    if(it == starting)
                        break;
                }

                // Now that all items we may need to reference are created,
                // translate the old references to their new equivalent copies.
                foreach(FillSegment fs in fiNew.segments)
                { 
                    fs.next = cloneLookup[fs.next];
                    fs.prev = cloneLookup[fs.prev];
                }

#if DEVELOPMENT_BUILD || UNITY_EDITOR
                if (this.TestValidity() == false)
                {
                    Debug.LogError("Error discovered in FillIsland.Clone(), testing invoking object's validity.");
                    return null;
                }
                if(fiNew.TestValidity() == false)
                { 
                    Debug.LogError("Error discovered in FillIsland.Clone(), testing fiNew's validity.");
                    return null;
                }
#endif

                return fiNew;
            }

            /// <summary>
            /// Get the triangles of the island.
            /// </summary>
            /// <param name="triangles">The mesh index output of the triangles.</param>
            /// <param name="vectors">The positions output of the triangles.</param>
            /// <param name="consume">If false, a clone is made instead that is processed so 
            /// that the object does not have its contents consumed.</param>
            public void GetTriangles(List<int> triangles, Vector2Repo vectors, bool consume = false)
            {
                FillIsland fi = this;
                if(consume == false)
                    fi = this.Clone();

                fi.ConsumeIntoTriangles(triangles, vectors);
            }

            /// <summary>
            /// Test the validity of the object and internal datastructures.
            /// </summary>
            /// <returns>True, if the object is valid; else false</returns>
            public bool TestValidity()
            { 
                if(this.segments.Count < 3)
                    Debug.LogError("Validity Error: Encountered a degenerate FillsIsland, not enough verts");

                FillSegment fsstart = this.GetAStartingPoint();
                FillSegment fsit = fsstart;

                int ct = 0;
                bool ret = true;
                while(true)
                { 
                    if(fsit.next.prev != fsit)
                    {
                        Debug.LogError($"Validity Error: FillSegment {fsit.debugCtr} next doesnt match previous.");
                        ret = false;
                    }

                    if(fsit.prev.next != fsit)
                    {
                        Debug.LogError($"Validity Error: FillSegment {fsit.debugCtr} previous doesnt match next.");
                        ret = false;
                    }

                    if (fsit.prevPos != fsit.prev.nextPos)
                    {
                        Debug.LogError($"Validity Error: FillSegment {fsit.debugCtr} previous point doesn't match neighbors.");
                        ret = false;
                    }

                    if (fsit.nextPos != fsit.next.prevPos)
                    {
                        Debug.LogError($"Validity Error: FillSegment {fsit.debugCtr} next point doesn't match neighbors.");
                        ret = false;
                    }

                    ++ct;

                    if(this.segments.Contains(fsit) == false)
                    {
                        Debug.LogError($"Validity Error: FillSegment {fsit.debugCtr} not contained it parent segment list.");
                        ret = false;
                    }

                    fsit = fsit.next;
                    if(fsit == fsstart)
                        break;
                }

                if(ct != this.segments.Count)
                {
                    Debug.LogError($"Validity Error: FillSegment traversed loop differs in size of set.");
                    ret = false;
                }

                return ret;
            }

#if DEVELOPMENT_BUILD || UNITY_EDITOR
            /// <summary>
            /// Debug function - dumps the contents of the island into an ordered CSV so they can be plotted
            /// in a graph program if additional diagnostic visualization is needed.
            /// </summary>
            /// <param name="postfix">The postfix to the filename. See function implementation for prefix.</param>
            public void DumpDebugCSV(string postfix)
            { 
                string export = "";

                FillSegment fsstart = this.GetAStartingPoint();

                FillSegment fsit = fsstart;
                while (true)
                {
                    export += fsit.nextPos.x.ToString() + ", " + fsit.nextPos.y.ToString() + ", " + fsit.debugCtr + "\n";

                    fsit = fsit.next;
                    if (fsit == fsstart)
                        break;
                }
                
                fsit = fsstart;
                while (true)
                {
                    export += fsit.prevPos.x.ToString() + ", " + fsit.prevPos.y.ToString() + ", " + fsit.debugCtr +"\n";

                    fsit = fsit.prev;
                    if (fsit == fsstart)
                        break;
                }

                export += "\n\n";

                System.IO.File.WriteAllText("FillsIslandLog_" + postfix + ".csv", export);

            }
#endif
        }
    }
}
