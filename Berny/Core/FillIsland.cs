﻿// MIT License
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
    /// Represents a closed loop to be filled in with the ear clipping algorithm.
    /// </summary>
    public class FillIsland
    {
        /// <summary>
        /// Specifies constraints for the winding of paths.
        /// </summary>
        public enum WindingRequirement
        { 
            /// <summary>
            /// No contraint should be used.
            /// </summary>
            DoesntMatter,

            /// <summary>
            /// The shape's positive region should be a clockwise winding.
            /// </summary>
            Clockwise,

            /// <summary>
            /// The shape's negative region should be a counter clickwise winding.
            /// </summary>
            Counter
        }

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
        /// Create an inflated edge mesh. 
        /// </summary>
        /// <param name="ie">The outline to inflate.</param>
        /// <param name="pushin">The amount to inflate inwards.</param>
        /// <param name="pushout">The amount to inflate outwards.</param>
        /// <returns>A FillIsland representing the shape of the outline.</returns>
        public static FillIsland CreateEdged(IEnumerable<Vector2> ie, float pushin, float pushout)
        { 
            List<FillSegment> segA = new List<FillSegment>();
            List<FillSegment> segB = new List<FillSegment>();

            FillIsland fiRet = new FillIsland();
            foreach(Vector2 vs2 in ie)
            { 
                FillSegment fsA = new FillSegment(vs2);
                FillSegment fsB = new FillSegment(vs2);

                segA.Add(fsA);
                segB.Add(fsB);

                fiRet.segments.Add(fsA);
                fiRet.segments.Add(fsB);
            }

            for(int i = 0; i < segA.Count - 1; ++i)
            { 
                FillSegment a = segA[i];
                FillSegment b = segA[i + 1];

                a.next = b;
                b.prev = a;
            }

            for(int i = 1; i < segB.Count; ++i)
            { 
                FillSegment a = segB[i - 1];
                FillSegment b = segB[i];

                a.prev = b;
                b.next = a;
            }

            List<Vector2> pushes = new List<Vector2>();
            for(int i = 0; i < segA.Count; ++i)
                pushes.Add(segA[i].InflateDir());

            for(int i = 0; i < segA.Count; ++i)
            { 
                Vector2 d = pushes[i];

                segA[i].pos += d * pushout;
                segB[i].pos -= d * pushin;
            }

            FillSegment stA = segA[0];
            FillSegment edA = segA[segA.Count - 1];
            FillSegment stB = segB[0];
            FillSegment edB = segB[segB.Count - 1];

            stA.prev = stB;
            stB.next = stA;

            edA.next = edB;
            edB.prev = edA;

            return fiRet;
        }

        /// <summary>
        /// Create a FillIsland from a shape defined by a set of points 
        /// that create a closed island.
        /// </summary>
        /// <param name="ie">The vertices.</param>
        /// <returns>The FillIsland representing the shape.</returns>
        public static FillIsland CreateLooped(IEnumerable<Vector2> ie)
        { 
            FillIsland fiRet = new FillIsland();

            List<FillSegment> lst = new List<FillSegment>();
            foreach(Vector2 v2 in ie)
            { 
                FillSegment fs = new FillSegment(v2);
                lst.Add(fs);
                fiRet.segments.Add(fs);
            }

            for(int i = 1; i < lst.Count; ++i)
            { 
                FillSegment fsP = lst[i - 1];
                FillSegment fsC = lst[i];

                fsP.next = fsC;
                fsC.prev = fsP;
            }

            FillSegment fsF = lst[0];
            FillSegment fsL = lst[lst.Count - 1];
            fsF.prev = fsL;
            fsL.next = fsF;

            return fiRet;
        }
            

        /// <summary>
        /// Calculates and caches the winding value. 
        /// </summary>
        /// <returns>The calculated winding value.</returns>
        public float CalculateWinding()
        { 
            this.cachedWinding = 0.0f;

            FillSegment fsstart = this.GetAStartingPoint();
            FillSegment it = fsstart;

            // Calculate averages
            int samples = 0;
            Vector2 avg = Vector2.zero;
            while (true)
            {
                ++samples;
                avg += it.pos;

                it = it.next;
                if (it == fsstart)
                    break;
            }
            avg /= (float)samples;

            while (true)
            {
                Vector2 fromAvg = it.pos - avg;
                Vector2 toNxt = it.next.pos - it.pos;

                this.cachedWinding += fromAvg.x * toNxt.y - fromAvg.y * toNxt.x;

                it = it.next;
                if (it == fsstart)
                    break;
            }


            // The old winding calculation method
            //it = fsstart;
            //while (true)
            //{ 
            //    Vector2 towa = (it.pos - it.prev.pos).normalized;
            //    Vector2 from = it.next.pos - it.pos;
            //
            //    this.cachedWinding += towa.x * from.y - towa.y * from.x;
            //
            //    it = it.next;
            //    if(it == fsstart)
            //        break;
            //}

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
        /// are gone and the shape is tessellated with triangles.
        /// </summary>
        /// <param name="triangles">The output of triangle mesh indices.</param>
        /// <param name="vectors">The output and manager of Vector2 vertices.</param>
        /// <remarks>This function will clear out the object.</remarks>
        public void ConsumeIntoTriangles(List<int> triangles, Vector2Repo vectors, WindingRequirement windingRequirement)
        { 
            // !DELME
            if(Utils.verboseDebug == true && this.TestValidity() == false)
                throw new System.Exception("Error discovered in FillIsland.ConsumeIntoTriangles, aborting.");

            // Note that this currently doesn't handle clipping through concave
            // edges.

            // Simple ear clipping. This will disassemble the path and clear
            // the island. If this is not preffered, sue GetTriangles() instead with
            // consume set to false to make a sacrificial copy instead.

            float wind = this.CalculateWinding();
            // I doubt this will ever happen.
            if(wind == 0.0f)
                wind = 1.0f;

            FillSegment lastAdded = null;
            FillSegment it = GetAStartingPoint();
            FillSegment start = it;

            // If we loop around too many times, we need to stop because nothing
            // would change and it would be an infinite loop.
            int unaddedLoops = 0;
            while(segments.Count > 2)
            { 
                Vector2 towa = it.pos - it.prev.pos;
                Vector2 from = it.next.pos - it.pos;
                float localWind = towa.x * from.y - towa.y * from.x;

                bool skip = false;
                if(segments.Count > 3)
                {
                    // Wrong winding, can't earclip.
                    if(Mathf.Abs(localWind) < 0.00001f)
                    {
                        // Does nothing, eats the if-chain.
                        //
                        // If the triangle with its neighbors is degenerate, just consume it. Not the best
                        // topology, but better than the risk of improper triangulation in the opposite
                        // winding.
                    }
                    else if ((wind >= 0.0f) != (localWind >= 0.0f))
                    {
                        skip = true;
                    }
                    else
                    {
                        FillSegment fsPtCheck = it;
                        fsPtCheck = fsPtCheck.next.next; // If there's at least 2 segments, this should be valid

                        bool cont = true;
                        while(cont == true)
                        {
                            if( (fsPtCheck.pos - it.pos).sqrMagnitude <= float.Epsilon ||
                                (fsPtCheck.pos - it.prev.pos).sqrMagnitude <= float.Epsilon ||
                                (fsPtCheck.pos - it.next.pos).sqrMagnitude <= float.Epsilon )
                            {
                                // Sometimes a point can be right on top of another point and it's a legitimate
                                // clipping position - especially if we're processing a cavity that's been bridged.
                                //
                                // Eat up this situation to let it pass.
                            }
                            else
                            {
                                do
                                {
                                    const float windEps = 0.00001f;

                                    // We're going to use a cross product to check if the point
                                    // if the region we're checking is on an edge of the triangle.
                                    //
                                    // If it is, we need to do further checking.
                                    //
                                    // Checking if it's on the edge of the point to next.
                                    Vector2 itToCheck = fsPtCheck.pos - it.pos;
                                    Vector2 itToNext = it.next.pos - it.pos;
                                    float degenTestWind = itToCheck.x * itToNext.y - itToNext.x * itToCheck.y;
                                    if (Mathf.Abs(degenTestWind) < windEps)
                                    { 
                                        float dot1 = Vector2.Dot(itToCheck, itToNext);
                                        float dot2 = Vector2.Dot(-itToNext, fsPtCheck.pos - it.next.pos);

                                        if(dot1 >= 0.0f && dot2 >= 0.0f)
                                        { 
                                            cont = false;
                                            skip = true;
                                            break;
                                        }
                                    }

                                    // Checking if it's on the edge of the point to prev.
                                    Vector2 prvToIt = it.pos - it.prev.pos;
                                    degenTestWind = itToCheck.x * prvToIt.y - prvToIt.x * itToCheck.y;
                                    if (Mathf.Abs(degenTestWind) < windEps)
                                    {
                                        float dot1 = Vector2.Dot(itToCheck, -prvToIt);
                                        float dot2 = Vector2.Dot(prvToIt, fsPtCheck.pos - it.prev.pos);

                                        if (dot1 >= 0.0f && dot2 >= 0.0f)
                                        {
                                            cont = false;
                                            skip = true;
                                            break;
                                        }
                                    }

                                    // If it's not on the edge, check if it's in the triangle.
                                    if (Utils.PointInTriangle(fsPtCheck.pos, it.prev.pos, it.pos, it.next.pos) == true)
                                    {
                                        // If it's not the the edge of the triangle and creates a wrong winding
                                        // triangle, we need to move on.
                                        cont = false;
                                        skip = true;
                                        break;
                                    }
                                }
                                while(false);
                            }

                            fsPtCheck = fsPtCheck.next;

                            // We don't check to go full circle, but one less than. The points should not
                            // be the actual parts of the triangle.
                            if (fsPtCheck == it.prev) 
                                break;
                        }
                    }
                }

                if(skip == true)
                {
                    it = it.next; 
                    if (it == lastAdded || (lastAdded == null && it == start))
                    {
                        wind = this.CalculateWinding();
                        ++unaddedLoops;

                        if(unaddedLoops == 2)
                            break;
                    }

                    continue;
                }

                if(
                    windingRequirement == WindingRequirement.DoesntMatter ||
                    localWind <= 0.0f && windingRequirement == WindingRequirement.Clockwise ||
                    localWind >= 0.0f && windingRequirement == WindingRequirement.Counter)
                {
                    // Record the triangle
                    triangles.Add(vectors.GetVectorID(it.prev.pos));
                    triangles.Add(vectors.GetVectorID(it.pos));
                    triangles.Add(vectors.GetVectorID(it.next.pos));
                }
                else
                {
                    triangles.Add(vectors.GetVectorID(it.pos));         // Swapped this ...
                    triangles.Add(vectors.GetVectorID(it.prev.pos));    // ... and this - to change the winding
                    triangles.Add(vectors.GetVectorID(it.next.pos));
                }
                unaddedLoops = 0;


                // Get rid of the prev (and don't advance the iterator) to
                // clip out the vertex
                FillSegment nextIt = it.next;
                FillSegment prevIt = it.prev;
                segments.Remove(it);
                nextIt.prev = prevIt;
                prevIt.next = nextIt;
                //
                lastAdded = it.prev;
                it = nextIt;
            }
        }

        /// <summary>
        /// Calculate a triangle mesh that's an outline
        /// of the FillIsland.
        /// </summary>
        /// <param name="width">The width of the outline.</param>
        /// <param name="triangles">The output triangle indices.</param>
        /// <param name="vectors">The output triangle vertices.</param>
        /// <param name="wr">Rules on how to create the winding of the triangles.</param>
        public void ConsumeIntoOulineTriangles(
            float width, 
            List<int> triangles, 
            Vector2Repo vectors, 
            WindingRequirement wr)
        {
            this.MakeOutlineBridged(width);
            this.ConsumeIntoTriangles(triangles, vectors, wr);
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
                FillSegment itCpy = it.Clone(true);;
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
        public void GetTriangles(List<int> triangles, Vector2Repo vectors, WindingRequirement windingRequirement, bool consume = false)
        {
            FillIsland fi = this;
            if(consume == false)
                fi = this.Clone();

            fi.ConsumeIntoTriangles(triangles, vectors, windingRequirement);
        }

        /// <summary>
        /// Test the validity of the object and internal data structures.
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
#if DEVELOPMENT_BUILD || UNITY_EDITOR
                    Debug.LogError($"Validity Error: FillSegment {fsit.debugCtr} next doesn't match previous.");
#else
                    Debug.LogError($"Validity Error: FillSegment next doesn't match previous.");
#endif
                    ret = false;
                }

                if(fsit.prev.next != fsit)
                {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
                    Debug.LogError($"Validity Error: FillSegment {fsit.debugCtr} previous doesn't match next.");
#else
                    Debug.LogError($"Validity Error: FillSegment previous doesn't match next.");
#endif
                    ret = false;
                }

                ++ct;

                if(this.segments.Contains(fsit) == false)
                {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
                    Debug.LogError($"Validity Error: FillSegment {fsit.debugCtr} not contained it parent segment list.");
#else
                    Debug.LogError($"Validity Error: FillSegment not contained it parent segment list.");
#endif
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

        /// <summary>
        /// Given a closed shape, turn it into a outline that has a bridge that allows it to be filled with
        /// a hollow interior.
        /// </summary>
        /// <param name="inflate">The amount to inflate the path.</param>
        public void MakeOutlineBridged(float inflate)
        {
            FillSegment first = null;
            FillSegment last = null;

            List<FillSegment> lst = new List<FillSegment>();

            foreach(FillSegment fs in this.Travel())
            { 
                FillSegment fsNew = new FillSegment(fs.pos);
                this.segments.Add(fsNew);
                lst.Add(fsNew);

                if (first == null)
                    first = fs;

                last = fs;
            }

            for(int i = 1; i < lst.Count; ++i)
            { 
                // Create as reverse winding
                FillSegment fsP = lst[i - 1];
                FillSegment fsC = lst[i];

                fsP.prev = fsC;
                fsC.next = fsP;
            }

            // After we created the duplicate (in a reverse winding) as the 
            // inside, inflate the original. We inflate the original because
            // it already has the correct winding.

            List<Vector2> inflated = new List<Vector2>();
            foreach (FillSegment fs in first.Travel())
                inflated.Add(fs.InflateDir());

            int j = 0;
            foreach (FillSegment fs in first.Travel())
            {
                fs.pos += inflated[j] * inflate;
                ++j;
            }

            FillSegment fsNewFirst = lst[0];
            FillSegment fsNewLast = lst[lst.Count - 1];

            // Stitch things up, since we iterated through everything, we didn't double the first
            // item up to simulate a closed shaped.
            FillSegment newLast = new FillSegment(first.pos);
            last.next = newLast;
            newLast.prev = last;
            last = newLast;
            this.segments.Add(newLast);

            FillSegment newInLast = new FillSegment(fsNewFirst.pos);
            fsNewLast.prev = newInLast;
            newInLast.next = fsNewLast;
            fsNewLast = newInLast;
            this.segments.Add(newInLast);

            // correct paths.
            first.prev = fsNewFirst;
            fsNewFirst.next = first;
            last.next = fsNewLast;
            fsNewLast.prev = last;
        }

        /// <summary>
        /// Inflate the outline.
        /// </summary>
        /// <param name="f">The amount to inflate by.</param>
        public void Inflate(float f)
        { 
            List<Vector2> infs = new List<Vector2>();
            FillSegment travelStart = Utils.GetFirstInHash(this.segments);
            foreach(FillSegment fs in travelStart.Travel())
                infs.Add(fs.InflateDir());

            int i = 0;
            foreach(FillSegment fs in travelStart.Travel())
            {
                fs.pos += infs[i] * f;
                ++i;
            }
        }

        /// <summary>
        /// Enumerate across the entire island.
        /// </summary>
        /// <returns>An IEnumerable that travels through the entire island.</returns>
        IEnumerable<FillSegment> Travel()
        { 
            FillSegment first = Utils.GetFirstInHash(this.segments);
            FillSegment it = first;

            while(it != null)
            { 
                yield return it;

                it = it.next;
                if(it == first)
                    yield break;
            }

            yield break;
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

            // Export going forward
            FillSegment fsit = fsstart;
            while (true)
            {
                export += fsit.pos.x.ToString() + ", " + fsit.pos.y.ToString() + ", " + fsit.debugCtr + "\n";

                fsit = fsit.next;
                if (fsit == fsstart)
                    break;
            }
                
            // Export going backwards
            fsit = fsstart;
            while (true)
            {
                export += fsit.pos.x.ToString() + ", " + fsit.pos.y.ToString() + ", " + fsit.debugCtr +"\n";

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