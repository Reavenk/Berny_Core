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
    /// Utility class to perform boolean operations between closed shapes.
    /// </summary>
    public static partial class Boolean
    {
        public delegate BoundingMode BooleanImpl(BLoop dstloop, List<BNode> dstSegs, List<BNode> otherSegs, out BNode onIslA);

        /// <summary>
        /// A description of the collision between two nodes (a left and right island parameter).
        /// </summary>
        public enum BoundingMode
        { 
            /// <summary>
            /// The nodes do not collide or overlap.
            /// </summary>
            NoCollision,

            /// <summary>
            /// The nodes intersect each other.
            /// </summary>
            Collision,

            /// <summary>
            /// An error condition was encountered. The result is undefined and considered unusable.
            /// </summary>
            Degenerate,

            /// <summary>
            /// The nodes do not intersect paths, but the left island completely surround the right island.
            /// </summary>
            LeftSurroundsRight,

            /// <summary>
            /// The nodes do not intersect paths, but the right island completly surrounds the left island.
            /// </summary>
            RightSurroundsLeft
        }

        /// <summary>
        /// Remove a loop from its parent shape, and optionally remove
        /// the shape from the layer if it's null.
        /// </summary>
        /// <param name="loop">The loop to remove from its parent.</param>
        /// <param name="rmShapeIfEmpty">If true, the parent shape will be removed from
        /// its parent layer if the operation leaves that shape empty. </param>
        public static void RemoveLoop(BLoop loop, bool rmShapeIfEmpty)
        { 
            if(loop.shape == null)
                return;

            BShape shape = loop.shape;
            shape.loops.Remove(loop);
            loop.shape = null;

            if(rmShapeIfEmpty == false || shape.loops.Count > 0)
                return;

            if(shape.layer != null)
            {
                shape.layer.shapes.Remove(shape);
                shape.layer = null;
            }
        }

        /// <summary>
        /// Find the list of unique parent loops from a set of nodes.
        /// 
        /// This function is not actually means for boolean operations, but instead for
        /// parsing potential targets from a group of nodes to be involved in boolean operations. 
        /// To paraphrase: instead of only involving the nodes or islands from the nodes parameter,
        /// this function is used to involve their entire parent loops.
        /// </summary>
        /// <param name="firstLoop">The first found parent loop - or null if none were found.</param>
        /// <param name="nodes">All the parent loops found, EXCEPT for the first one which will be 
        /// placed in output parameter firstLoop.</param>
        /// <returns>The list of unique loops. They are sorted as the order they were first encountered
        /// from iterating through the nodes parameter.</returns>
        public static List<BLoop> GetUniqueLoopsInEncounteredOrder(out BLoop firstLoop, IEnumerable<BNode> nodes)
        {
            List<BLoop> ret = new List<BLoop>();
            firstLoop = null;

            HashSet<BLoop> loopHash = new HashSet<BLoop>();
            foreach (BNode bn in nodes)
            {
                if(loopHash.Add(bn.parent) == false)
                    continue;

                if(firstLoop == null)
                    firstLoop = bn.parent;
                else
                    ret.Add(bn.parent);
            }
            return ret;
        }

        /// <summary>
        /// Find the list of unique parent loops from a set of nodes.
        /// 
        /// This function is not actually means for boolean operations, but instead for
        /// parsing potential targets from a group of nodes to be involved in boolean operations. 
        /// To paraphrase: instead of only involving the nodes or islands from the nodes parameter,
        /// this function is used to involve their entire parent loops.
        /// </summary>
        /// <param name="nodes">All the parent loops found.</param>
        /// <returns>The list of unique loops. They are sorted as the order they were first encountered
        /// from iterating through the nodes parameter.</returns>
        public static List<BLoop> GetUniqueLoopsInEncounteredOrder(IEnumerable<BNode> nodes)
        { 
            List<BLoop> ret = new List<BLoop>();
            HashSet<BLoop> loopHash = new HashSet<BLoop>();
            foreach (BNode bn in nodes)
            {
                if (loopHash.Add(bn.parent) == false)
                    continue;

                ret.Add(bn.parent);
            }
            return ret;
        }

        /// <summary>
        /// Given two loops, calculate information on where they collide with each other.
        /// 
        /// Does not handle self intersections.
        /// </summary>
        /// <param name="loopA">The loop containing the geometry for the left path.</param>
        /// <param name="loopB">The loop containing the geometry for the right path.</param>
        /// <returns>The points where segments from the left path and the right path intersect 
        /// each other.</returns>
        /// <remarks>The terms "left" path and "right" path do not specifically refer to left and right,
        /// but instead are used to different the two paths from each other.</remarks>
        public static List<Utils.BezierSubdivSample> GetLoopCollisionInfo(
            BLoop loopA, 
            BLoop loopB)
        { 
            List<BNode> islandsA = loopA.GetIslands();
            List<BNode> islandsB = loopB.GetIslands();

            List<Utils.BezierSubdivSample> collisions = new List<Utils.BezierSubdivSample>();
                
            foreach (BNode isA in islandsA)
            { 
                BNode.EndpointQuery eqA = isA.GetPathLeftmost();
                // Only closed loops count
                if (eqA.result == BNode.EndpointResult.SuccessfulEdge)
                    continue;

                List<BNode> segsA = new List<BNode>(eqA.Enumerate());
                foreach (BNode isB in islandsB)
                { 
                    BNode.EndpointQuery eqB = isB.GetPathLeftmost();
                    // Only closed loops count
                    if(eqB.result == BNode.EndpointResult.SuccessfulEdge)
                        continue;

                    List<BNode> segsB = new List<BNode>(eqB.Enumerate());

                    GetLoopCollisionInfo(segsA, segsB, collisions);
                }
            }

            Utils.BezierSubdivSample.CleanIntersectionList(collisions);
            return collisions;
        }

        // This function should probably be moved from Boolean to Utils
        /// <summary>
        /// Given two loops, calculate information on where they collide with each other.
        /// 
        /// Does not handle self intersections.
        /// </summary>
        /// <param name="islAs">The nodes in the island of the left path.</param>
        /// <param name="islBs">The nodes in the island of the right path.</param>
        /// <param name="lstOut">The destination for output collision info.</param>
        public static void GetLoopCollisionInfo(
            List<BNode> islAs, 
            List<BNode> islBs, 
            List<Utils.BezierSubdivSample> lstOut)
        {
            foreach (BNode na in islAs)
            {
                foreach (BNode nb in islBs)
                {
                    Utils.NodeIntersections(na, nb, 20, Mathf.Epsilon, lstOut);
                }
            }
        }

        /// <summary>
        /// Check the bounding relationships between two islands.
        /// </summary>
        /// <param name="leftIsl">A node belonging to the first island.</param>
        /// <param name="rightIsl">A node belonging to the second island.</param>
        /// <param name="recheck">If true, validates that both islands are
        /// closed loops before doing the actual check.</param>
        /// <returns></returns>
        public static BoundingMode GetLoopBoundingMode(BNode leftIsl, BNode rightIsl, bool recheck)
        {
            // The basic algorithm works by taking a point on the path at a most extreme 
            // (e.g., minx, miny, maxx, maxy - in this case we're doing maxx) and moving farther
            // in that direction to test intersection with the other island.
            //
            // Even though we're focused on rightward movement and ray casting, keep in mind
            // the leftIsl and rightIsl are just two separate islands - it's not to be implied that
            // leftIsl is to the left of rightIsl - that kind of information is unknown going
            // in (and because of the arbitrary shapes of these islands, it's actually impossible
            // to define such things).
            if(recheck == true)
            {
                BNode.EndpointQuery eqL = leftIsl.GetPathLeftmost();
                BNode.EndpointQuery eqR = rightIsl.GetPathLeftmost();

                if(eqL.result == BNode.EndpointResult.SuccessfulEdge)
                    return BoundingMode.NoCollision;

                if(eqR.result == BNode.EndpointResult.SuccessfulEdge)
                    return BoundingMode.NoCollision;

                leftIsl = eqL.node;
                rightIsl = eqL.node;
            }

            List<BNode> leftNodes = new List<BNode>(leftIsl.Travel());
            List<BNode> righttNodes = new List<BNode>(rightIsl.Travel());

            return GetLoopBoundingMode(leftNodes, righttNodes);
        }

        /// <summary>
        /// Get the containment relationship between two closed loops that do not directly intersect. 
        /// Do they not overlap at all? Or does one completly encompass the other without touching.
        /// </summary>
        /// <param name="islandSegsA">One of the closed loops to compare.</param>
        /// <param name="islandSegsB">The other closed loop to compare.</param>
        /// <returns>The overlap state.</returns>
        /// <remarks>The islands parameters are assumed to not have any intersecting segment paths.</remarks>
        public static BoundingMode GetLoopBoundingMode(List<BNode> islandSegsA, List<BNode> islandSegsB)
        { 

            // Check if the right most point of the left collides with anything on 
            // a ray moving further to the right. If so, how many intersect points are there?
            //
            ////////////////////////////////////////////////////////////////////////////////
                
            Vector2 mptL = islandSegsA[0].Pos;
            BNode maxXLeft = islandSegsA[0];
            float filler = 0.0f;
            foreach(BNode bn in islandSegsA)
            { 
                if(bn.GetMaxPoint(ref mptL, ref filler, 0) == true)
                    maxXLeft = bn;
            }

            Vector2 rayEnd = mptL + new Vector2(1.0f, 0.0f);
            List<float> interCurve = new List<float>();     
            List<float> interLine = new List<float>();
            List<BNode> interNodes = new List<BNode>();
            foreach(BNode nOth in islandSegsB)
            {
                int proj = nOth.ProjectSegment(mptL, rayEnd, interCurve, interLine);
                for(int i = 0; i < proj; ++i)
                    interNodes.Add(nOth);
            }
            _CleanProjectIntersections(interCurve, interLine, interNodes, mptL);

            // If the right loop is physically more to the right, it could still be untouching - but only if for
            // every entry, there's an exit. Meaning if there's an odd number of collisions, the left is completely
            // inside the right.
            //
            // Keep in mind this logic only works because of the constraint that left and right should not intersect.
            if (interCurve.Count > 0)
            { 
                if((interCurve.Count % 2) == 1)
                    return BoundingMode.RightSurroundsLeft;

                return BoundingMode.NoCollision;
            }

            // Now we do the same for the right.
            //
            ////////////////////////////////////////////////////////////////////////////////

            // Check if the right most point on the right collides with anything on
            // a ray moving further to the right. If so, how many intersect points are there?
            Vector2 mptR = islandSegsB[0].Pos;
            BNode maxXRight = islandSegsB[0];
            foreach(BNode bn in islandSegsB)
            { 
                if(bn.GetMaxPoint(ref mptR, ref filler, 0) == true)
                    maxXRight = bn;
            }

            rayEnd = mptR + new Vector2(1.0f, 0.0f);
            interCurve.Clear();
            interLine.Clear();
            interNodes.Clear();
            foreach (BNode nOth in islandSegsA)
            {
                int proj = nOth.ProjectSegment(mptR, rayEnd, interCurve, interLine);
                for(int i = 0; i < proj; ++i)
                    interNodes.Add(nOth);
            }
            _CleanProjectIntersections(interCurve, interLine, interNodes, mptR);

            if((interCurve.Count % 2) == 1)
                return BoundingMode.LeftSurroundsRight;

            return BoundingMode.NoCollision;
        }

        /// <summary>
        /// Utility function used by GetLoopBoundingMode().
        /// </summary>
        /// <param name="lstC">List of curve interpolation points.</param>
        /// <param name="lstL">Lise of line interpolation points.</param>
        /// <param name="lstN">List of nodes interpolation points.</param>
        private static void _CleanProjectIntersections(List<float> lstC, List<float> lstL, List<BNode> lstN, Vector2 projPt)
        {
            // The edgeEps is not very sensitive - we may want to also compare the line
            // similarity as a redundancy
            const float edgeEps = 0.01f;
            // Take out invalid intersections
            for (int i = lstC.Count - 1; i >= 0; --i)
            {
                bool rm = false;

                do
                {
                    float l = lstL[i];
                    if (l < 0.0f)
                    {
                        rm = true;
                        break;
                    }

                    float c = lstC[i];
                    if (c < 0.0f || c > 1.0f)
                    {
                        rm = true;
                        break;
                    }
                }
                while (false);

                if (rm == true)
                {
                    lstC.RemoveAt(i);
                    lstL.RemoveAt(i);
                    lstN.RemoveAt(i);
                }
            }

            // Take out neighboring connections that are similar.
            for (int i = 0; i < lstC.Count - 1; ++i)
            {
                float c = lstC[i];

                for (int j = lstC.Count - 1; j > i; --j)
                {
                    // If the candidate is at the left edge, and the previous is at the right edge
                    if (
                        lstN[i] == lstN[j] &&
                        Mathf.Abs(lstC[i] - lstC[j]) <= edgeEps)
                    {
                        lstC.RemoveAt(j);
                        lstL.RemoveAt(j);
                        lstN.RemoveAt(j);
                    }
                    else if (c <= edgeEps)
                    {
                        if (
                            lstN[i].prev == lstN[j] &&
                            lstC[j] >= 1.0f - edgeEps)
                        {
                            lstC.RemoveAt(j);
                            lstL.RemoveAt(j);
                            lstN.RemoveAt(j);
                        }
                    }
                    else if (c >= 1.0f - edgeEps)
                    {
                        if (
                            lstN[i].next == lstN[j] &&
                            lstC[j] <= edgeEps)
                        {
                            lstC.RemoveAt(j);
                            lstL.RemoveAt(j);
                            lstN.RemoveAt(j);
                        }
                    }
                }
            }

            // If we're hitting things that are perfectly on projPt and perfectly horizontal,
            // we need to make sure that only counts as one collision.
            //
            // If multiple items on the same segment-chain are colinear with the test ray, this
            // should remove all but one of them. Which one it is does not matter since 
            // GetLoopBoundingMode() is just interested in collision counting, and not its 
            // position.
            for (int i = 0; i < lstC.Count - 1;)
            {
                // We're hard-coding the fact that GetLoopBoundingMode() does its test by
                //raycasting to the right, so we just need to check similarities on the y.
                float sameLineEps = 0.01f;

                bool removeIdx = false;
                // If we're exactly on the Y, every other neighboring item needs to go because it's 
                // on an edge case.
                if (_HasYEndOnLine(lstN[i], projPt.y, sameLineEps) == true)
                {
                    List<BNode> sameHorizAndNeigh =
                        _OnSameChainAndY(lstN[i], projPt.y, sameLineEps);


                    // If we start at one Y, and end at one Y, get rid of everything
                    if (Mathf.Sign(sameHorizAndNeigh[0].prev.Pos.y - projPt.y) == Mathf.Sign(sameHorizAndNeigh[sameHorizAndNeigh.Count - 1].next.Pos.y - projPt.y))
                        removeIdx = true;

                    // Now that we've figured if everything should stay or go, and handled the first 
                    // item (deffered for later), work on every other part of the segment.

                    // We don't need to find the closest to the proj pt or anything
                    // like that, we just need to collapse away edge cases.
                    for (int j = lstN.Count - 1; j > i; --j)
                    {
                        bool consolRm = false;
                        if (sameHorizAndNeigh.Contains(lstN[j]) == true &&
                            (lstC[j] <= sameLineEps || lstC[j] >= sameLineEps))
                        {
                            consolRm = true;
                        }
                        else if (
                            sameHorizAndNeigh.Contains(lstN[j].next) == true &&
                            lstC[j] >= 1.0f - sameLineEps)
                        {
                            consolRm = true;
                        }
                        else if (
                            sameHorizAndNeigh.Contains(lstN[j].prev) == true &&
                            lstC[j] <= sameLineEps)
                        {
                            consolRm = true;
                        }

                        if (consolRm == true)
                        {
                            lstC.RemoveAt(j);
                            lstL.RemoveAt(j);
                            lstN.RemoveAt(j);
                        }
                    }
                }

                if (removeIdx == true)
                {
                    lstC.RemoveAt(i);
                    lstL.RemoveAt(i);
                    lstN.RemoveAt(i);
                }
                else
                    ++i;
            }

            // Check if we're hitting any points. If so, they need to go because they're an edge
            // case that doesn't actually change the containment test.
            //
            // For some reason, it seems the epsilon needs to be very high in some cases -
            // although I might be doing something wrong. It's hard to imagine floating point
            // precision being this bad.
            // (wleu 01/02/2021)
            float cornerEps = 0.1f;
            for (int i = 0; i < lstC.Count;)
            {
                bool rem = false;
                if (lstC[i] <= cornerEps)
                {
                    if (Mathf.Abs(lstN[i].Pos.y - projPt.y) <= edgeEps && _IsVerticalPoint(lstN[i]) == true)
                        rem = true;
                }

                else if (lstC[i] >= 1.0f - cornerEps)
                {
                    if (Mathf.Abs(lstN[i].next.Pos.y - projPt.y) <= edgeEps && _IsVerticalPoint(lstN[i].next) == true)
                        rem = true;
                }

                if (rem == true)
                {
                    lstC.RemoveAt(i);
                    lstL.RemoveAt(i);
                    lstN.RemoveAt(i);
                }
                else
                    ++i;
            }
        }

        /// <summary>
        /// Checks if the node is in the middle of two lines that are moving in the same
        /// vertical direction.
        /// 
        /// This is done for edge case detection in _CleanProjectIntersections(), where vertical
        /// "elbows" can cause.
        /// </summary>
        /// <param name="bn">The node to check for vertical "elbowness."</param>
        /// <returns>If true, the point at bn is the corner of a formed elbow - i.e., where the 
        /// previous segment will travel vertical in one direction, and then afterwards the node 
        /// the segment travels in the opposite vertical direction.
        /// </returns>
        private static bool _IsVerticalPoint(BNode bn)
        {
            // There's a small optimization we can do by just calculating the Y 
            // instead of vectors.
            Vector2 ptPrev, ptNext;
            BNode.PathBridge pbNext = bn.GetPathBridgeInfo();
            BNode.PathBridge pbPrev = bn.prev.GetPathBridgeInfo();

            if (pbNext.pathType == BNode.PathType.Line)
                ptNext = bn.next.Pos - bn.Pos;
            else
            {
                float a, b, c, d;
                Utils.GetBezierDerivativeWeights(0.0f, out a, out b, out c, out d);

                Vector2 pt0 = bn.prev.Pos;
                Vector2 pt1 = bn.prev.Pos + pbNext.prevTanOut;
                Vector2 pt2 = bn.Pos + pbNext.nextTanIn;
                Vector2 pt3 = bn.Pos;

                ptNext =
                    a * pt0 +
                    b * pt1 +
                    c * pt2 +
                    d * pt2;

                if (ptNext.y == 0.0f)
                {
                    float lroot = 1.0f;
                    float ra, rb;
                    int r = Utils.GetRoots1DCubic(pt0.y, pt1.y, pt2.y, pt3.y, out ra, out rb);
                    for (int i = 0; i < r; ++i)
                    {
                        if (i == 0)
                            lroot = Mathf.Min(ra, lroot);
                        else if (i == 1)
                            lroot = Mathf.Min(rb, lroot);
                    }

                    Utils.GetBezierWeights(lroot, out a, out b, out c, out d);
                    Vector2 rpt =
                        a * pt0 +
                        b * pt1 +
                        c * pt2 +
                        d * pt3;

                    ptNext = rpt - bn.Pos;
                }
            }

            if (bn.prev.IsLine() == true)
                ptPrev = bn.Pos - bn.prev.Pos;
            else
            {
                float a, b, c, d;
                Utils.GetBezierDerivativeWeights(1.0f, out a, out b, out c, out d);

                Vector2 pt0 = bn.prev.Pos;
                Vector2 pt1 = bn.prev.Pos + pbPrev.prevTanOut;
                Vector2 pt2 = bn.Pos + pbPrev.nextTanIn;
                Vector2 pt3 = bn.Pos;

                ptPrev =
                    a * pt0 +
                    b * pt1 +
                    c * pt2 +
                    d * pt3;

                if (ptPrev.y == 0.0f)
                {
                    float lroot = 0.0f;
                    float ra, rb;
                    int r = Utils.GetRoots1DCubic(pt0.y, pt1.y, pt2.y, pt3.y, out ra, out rb);
                    for (int i = 0; i < r; ++i)
                    {
                        if (i == 0)
                            lroot = Mathf.Max(ra, lroot);
                        else if (i == 1)
                            lroot = Mathf.Max(rb, lroot);
                    }

                    Utils.GetBezierWeights(lroot, out a, out b, out c, out d);
                    Vector2 rpt =
                        a * pt0 +
                        b * pt1 +
                        c * pt2 +
                        d * pt3;

                    ptPrev = bn.Pos - rpt;
                }
            }

            if (ptPrev.y == 0.0f || ptNext.y == 0.0f)
                return false;

            return Mathf.Sign(ptPrev.y) != Mathf.Sign(ptNext.y);
        }
    }
}