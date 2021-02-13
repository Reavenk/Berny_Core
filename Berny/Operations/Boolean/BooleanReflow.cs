// MIT License
// 
// Copyright (c) 2021 Pixel Precision LLC
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
    /// Utility class to perform boolean operations between closed shapes.
    /// 
    /// This file focuses on boolean algorithms that work by restructuring
    /// the shape of existing paths.
    /// </summary>
    public static partial class Boolean
    {
        /// <summary>
        /// Perform a difference between the path of two loops.
        /// </summary>
        /// <remarks>This function assumes the left and right loops have only one
        /// path that's a closed island.</remarks>
        /// <param name="left">The island being subtracted from.</param>
        /// <param name="right">The island being subtracted.</param>
        /// <param name="onIslA">A node that's on the island, or null if no geometry
        /// is left afterwards.</param>
        public static void Difference(
            BLoop left, 
            BLoop right, 
            out BNode onIslA)
        {
            // we always remove B, which may contain extra stuff from 
            // subtraction shapes that didn't find a target to remove.
            PerIslandBoolean(
                left,
                right,
                Difference,
                out onIslA,
                true);

            right.Clear();
            RemoveLoop(right, true);
        }

        /// <summary>
        /// Perform a union operation between the path of two loops.
        /// </summary>
        /// <remarks>This function assumes the left and right loops have only one
        /// path that's a closed island.</remarks>
        /// <param name="dst">The loop to move the path of the created loop into.</param>
        /// <param name="onIslA">The island being added.</param>
        /// <param name="others">The other island being added.</param>
        public static void Union(BLoop dst, out BNode onIslA, params BLoop[] others)
        {
            onIslA = null;
            foreach (BLoop bl in others)
            {
                PerIslandBoolean(
                    dst,
                    bl,
                    Union,
                    out onIslA,
                    true);
            }
        }

        /// <summary>
        /// Performs a generic per-island operation between the islands contained in two loops.
        /// </summary>
        /// <remarks>Not a generic function - only meant for specific boolean reflow operations.</remarks>
        /// <param name="left">The collection of islands for the operation.</param>
        /// <param name="right">The other collection of islands for the operation.</param>
        /// <param name="op">Function delegate containing the boolean operation.</param>
        /// <param name="onIslA">An output node that exists on the remaining path(s).</param>
        /// <param name="removeRight">If true, remove the contents of the right loop parameter after 
        /// the operation.</param>
        public static void PerIslandBoolean(BLoop left, BLoop right, BooleanImpl op, out BNode onIslA, bool removeRight)
        {
            if (left == right || left == null || right == null)
            {
                onIslA = left.nodes[0];
                return;
            }

            onIslA = null;

            // If we're doing a boolean, it's no longer a generated shape - even if it ends
            // up untouched.
            if (left.shape != null)
                left.shape.shapeGenerator = null;

            right.shape = null;

            List<BNode> islandsA = left.GetIslands(IslandTypeRequest.Closed);
            List<BNode> islandB = right.GetIslands(IslandTypeRequest.Closed);


            foreach (BNode islA in islandsA)
            {
                onIslA = islA;
                foreach (BNode islB in islandB)
                {
                    List<BNode> islandSegsA = new List<BNode>(islA.Travel());
                    List<BNode> islandSegsB = new List<BNode>(islB.Travel());

                    op(left, islandSegsA, islandSegsB, out onIslA);
                }
            }
            if (removeRight == true)
                RemoveLoop(right, true);
        }

        /// <summary>
        /// Given a set of collisions, convert them into a datastructure better suited for 
        /// reflow boolean operations.
        /// </summary>
        /// <param name="collisions">The set of collision information to reorganize.</param>
        /// <returns>The collision information, reorganized into a form better suited for
        /// reflow boolean operations.</returns>
        public static Dictionary<Utils.NodeTPos, BNode.SubdivideInfo> SliceCollisionInfo(List<Utils.BezierSubdivSample> collisions)
        {
            Dictionary<Utils.NodeTPos, BNode.SubdivideInfo> ret =
                new Dictionary<Utils.NodeTPos, BNode.SubdivideInfo>();

            Dictionary<BNode, HashSet<float>> subdivLocs =
                new Dictionary<BNode, HashSet<float>>();

            // Get all the unique subdivision locations for both parts of 
            // each collision.
            foreach (Utils.BezierSubdivSample bss in collisions)
            {
                HashSet<float> hsA;
                if (subdivLocs.TryGetValue(bss.a.node, out hsA) == false)
                {
                    hsA = new HashSet<float>();
                    subdivLocs.Add(bss.a.node, hsA);
                }

                hsA.Add(bss.a.lEst);

                HashSet<float> hsB;
                if (subdivLocs.TryGetValue(bss.b.node, out hsB) == false)
                {
                    hsB = new HashSet<float>();
                    subdivLocs.Add(bss.b.node, hsB);
                }

                hsB.Add(bss.b.lEst);
            }

            foreach (KeyValuePair<BNode, HashSet<float>> kvp in subdivLocs)
            {
                BNode node = kvp.Key;
                List<float> subs = new List<float>(kvp.Value);
                subs.Sort();


                if (node.UseTanOut == false && node.next.UseTanIn == false)
                {
                    // The scale isn't useful, but the direction is, for 
                    // winding purposes later.
                    Vector2 outTan = (node.next.Pos - node.Pos);
                    for (int i = 0; i < subs.Count; ++i)
                    {
                        // Linear subdivide, the easiest to do
                        Vector2 loc = Vector2.Lerp(node.Pos, node.next.Pos, subs[i]);

                        BNode.SubdivideInfo si = new BNode.SubdivideInfo();
                        // The tangents aren't really relevant for shaping the path, but
                        // can be useful for calculating windings when making decisions for
                        // boolean operations.
                        si.prevOut = outTan;
                        si.nextIn = -outTan;
                        si.subPos = loc;
                        si.subOut = outTan;
                        si.subIn = -outTan;
                        si.windTangent = outTan;
                        //
                        ret.Add(new Utils.NodeTPos(node, subs[i]), si);
                    }
                }
                else
                {
                    float lm = 0.0f;
                    BNode.PathBridge pb = node.GetPathBridgeInfo();
                    Vector2 pt0 = node.Pos;
                    Vector2 pt1 = node.Pos + pb.prevTanOut;
                    Vector2 pt2 = node.next.Pos + pb.nextTanIn;
                    Vector2 pt3 = node.next.Pos;

                    List<Vector2> subSpots = new List<Vector2>();
                    // Breaking the cubic Bezier down into multiple parts is going to be
                    // quite a bit more difficult because every subdivision changes the 
                    // curvature between tangent neighbors - so we have to incrementally
                    // crawl and update tangents with respect to recent changes we're making.
                    subSpots.Add(pt0);
                    for (int i = 0; i < subs.Count; ++i)
                    {
                        float curT = subs[i];
                        float realT = (curT - lm) / (1.0f - lm);

                        Vector2 p00 = Vector2.Lerp(pt0, pt1, realT);
                        Vector2 p01 = Vector2.Lerp(pt1, pt2, realT);
                        Vector2 p02 = Vector2.Lerp(pt2, pt3, realT);
                        //
                        Vector2 p10 = Vector2.Lerp(p00, p01, realT);
                        Vector2 p11 = Vector2.Lerp(p01, p02, realT);
                        //
                        Vector2 npos = Vector2.Lerp(p10, p11, realT);

                        // Record some important parts of the tangent, we're focused on what's 
                        // before the point, because what comes after could still be subject
                        // to modification.
                        subSpots.Add(p00);
                        subSpots.Add(p10);
                        subSpots.Add(npos);

                        // And update our info for iteration.
                        lm = curT;
                        pt0 = npos;
                        pt1 = p11;
                        pt2 = p02;
                    }
                    subSpots.Add(pt1);
                    subSpots.Add(pt2);
                    subSpots.Add(pt3);

                    for (int i = 0; i < subs.Count; ++i)
                    {
                        int idx = 3 + i * 3;
                        BNode.SubdivideInfo si = new BNode.SubdivideInfo();
                        si.subPos = subSpots[idx];
                        si.subIn = subSpots[idx - 1] - si.subPos;
                        si.subOut = subSpots[idx + 1] - si.subPos;

                        si.prevOut = subSpots[idx - 2] - subSpots[idx - 3];
                        si.nextIn = subSpots[idx + 2] - subSpots[idx + 3];
                        si.windTangent = si.subOut;

                        ret.Add(new Utils.NodeTPos(node, subs[i]), si);
                    }
                }
            }

            return ret;
        }

        /// <summary>
        /// Perform a union operation between two islands.
        /// </summary>
        /// <param name="dst">The destination loop where created content will be placed into.</param>
        /// <param name="islandSegsA">A list of all the nodes in island A.</param>
        /// <param name="islandSegsB">A list of all the nodes in island B.</param>
        /// <param name="onIslA">If the islands were processed, this output parameter contains a node
        /// on the new shape.</param>
        /// <returns>The results from the operation.</returns>
        public static BoundingMode Union(BLoop dst, List<BNode> islandSegsA, List<BNode> islandSegsB, out BNode onIslA)
        {
            return Union(dst, islandSegsA, islandSegsB, out onIslA, true);
        }

        /// <summary>
        /// Perform a union operation between two islands using a reflow strategy.
        /// </summary>
        /// <param name="dst">The destination of where the conjoined path will be placed.</param>
        /// <param name="islandSegsA">A list of all the nodes in island A. </param>
        /// <param name="islandSegsB">A list of all the nodes in island B.</param>
        /// <param name="onIslA">If the islands were processed, this output parameter contains a node
        /// on the new shape.</param>
        /// <param name="mergeNonCol">If true, other parts that didn't collide (and weren't merged) will be
        /// moved into the final output destination (dst).</param>
        /// <returns>The results from the operation.</returns>
        /// <remarks>islandSegsA and islandSegsB should only contain elements in the island, and should not 
        /// be confused with all nodes in the parent loop.</remarks>
        public static BoundingMode Union(BLoop dst, List<BNode> islandSegsA, List<BNode> islandSegsB, out BNode onIslA, bool mergeNonCol)
        {
            float leftWind = BNode.CalculateWinding(islandSegsA[0].Travel());
            float rightWind = BNode.CalculateWinding(islandSegsB[0].Travel());

            onIslA = islandSegsA[0];

            // It can be either winding, but they must be the same - since islandB is going to be used up in the process,
            // we'll modify that winding to keep islandA the same.
            if (leftWind > 0 != rightWind > 0)
                islandSegsB[0].ReverseChainOrder();

            List<Utils.BezierSubdivSample> delCollisions = new List<Utils.BezierSubdivSample>();
            GetLoopCollisionInfo(islandSegsA, islandSegsB, delCollisions);
            Utils.BezierSubdivSample.CleanIntersectionList(delCollisions);

            // If we didn't find any collisions, it's either because they don't overlap
            // at all, or one island fully wraps around another island.
            if (delCollisions.Count == 0)
            {
                BoundingMode bm = GetLoopBoundingMode(islandSegsA, islandSegsB);

                // If an island is completely surrounded by another island, one of the 
                // islands gets "smothered out of existence."
                if (bm == BoundingMode.LeftSurroundsRight)
                {
                    // Just remember everything from the right out of existence.
                    foreach (BNode bn in islandSegsB)
                        bn.SetParent(null);

                    return BoundingMode.LeftSurroundsRight;
                }
                else if (bm == BoundingMode.RightSurroundsLeft)
                {
                    // Remove everything from the left out of existence, and 
                    // move everything from the right island into the left;
                    foreach (BNode bn in islandSegsA)
                        bn.SetParent(null, false);

                    foreach (BNode bn in islandSegsB)
                    {
                        onIslA = bn;
                        bn.SetParent(dst, false);
                    }

                    return BoundingMode.RightSurroundsLeft;
                }

                if (mergeNonCol == true)
                {
                    foreach (BNode bn in islandSegsB)
                        bn.SetParent(dst, false);
                }

                return BoundingMode.NoCollision;
            }

            // Dump B into A, and if there's anything straggling, 
            // we'll clip it as a loose end afterwards.
            foreach (BNode bn in islandSegsB)
                bn.SetParent(dst, false);

            Dictionary<Utils.NodeTPos, BNode.SubdivideInfo> colSlideInfo = SliceCollisionInfo(delCollisions);

            Dictionary<Utils.NodeTPos, BNode> createdSubdivs = new Dictionary<Utils.NodeTPos, BNode>();
            SplitCollection splitCol = new SplitCollection(dst, delCollisions, createdSubdivs);

            HashSet<BNode> looseEnds = new HashSet<BNode>();

            foreach (Utils.BezierSubdivSample bss in delCollisions)
            {
                BNode.SubdivideInfo sdiA = colSlideInfo[bss.GetTPosA()];
                BNode.SubdivideInfo sdiB = colSlideInfo[bss.GetTPosB()];
                float wind = Utils.Vector2Cross(sdiA.subOut, sdiB.subOut);
                BNode colNode = createdSubdivs[bss.GetTPosA()];
                onIslA = colNode;

                if (leftWind <= 0.0f != wind <= 0.0f)
                {
                    // A CCW transition will go from A to B.
                    BNode nA = splitCol.GetPreviousTo(bss.GetTPosA());
                    BNode nB = splitCol.GetNextTo(bss.GetTPosB());

                    nA.TanOut = sdiA.prevOut;
                    nB.TanIn = sdiB.nextIn;

                    colNode.UseTanIn = bss.a.node.IsLine() == false;
                    colNode.UseTanOut = bss.b.node.IsLine() == false;
                    colNode.TanIn = sdiA.subIn;
                    colNode.TanOut = sdiB.subOut;

                    nA.next = colNode;
                    colNode.prev = nA;
                    nB.prev = colNode;
                    colNode.next = nB;

                    looseEnds.Add(bss.b.node);
                    looseEnds.Add(splitCol.GetSplitInfo(bss.a.node).origNext);

                }
                else
                {
                    // A CW transition will go from the other to it.
                    BNode nA = splitCol.GetNextTo(bss.GetTPosA());
                    BNode nB = splitCol.GetPreviousTo(bss.GetTPosB());

                    nA.TanIn = sdiA.nextIn;
                    nB.TanOut = sdiB.prevOut;

                    colNode.UseTanIn = bss.b.node.IsLine() == false;
                    colNode.UseTanOut = bss.a.node.IsLine() == false;
                    colNode.TanIn = sdiB.subIn;
                    colNode.TanOut = sdiA.subOut;

                    nB.next = colNode;
                    colNode.prev = nB;
                    nA.prev = colNode;
                    colNode.next = nA;

                    looseEnds.Add(splitCol.GetSplitInfo(bss.b.node).origNext);
                    looseEnds.Add(bss.a.node);
                }
            }

            // Figure out what internal items need to be removed by 
            // checking which nodes have unmatching connectivity.
            ClipLooseEnds(looseEnds);

            return BoundingMode.Collision;

        }

        /// <summary>
        /// Perform a difference operation between two islands using a reflow strategy.
        /// </summary>
        /// <param name="dstloop">The destination of where the differenced path will be placed.</param>
        /// <param name="islandSegsA">A list of all the nodes in island A.</param>
        /// <param name="islandSegsB">A list of all the nodes in island B</param>
        /// <param name="onIslA">A node on the resulting path.</param>
        /// <returns>The results from the operation.</returns>
        /// <remarks>islandSegsA and islandSegsB should only contain elements in the island, and should not 
        /// be confused with all nodes in the parent loop.</remarks>
        public static BoundingMode Difference(BLoop dstloop, List<BNode> islandSegsA, List<BNode> islandSegsB, out BNode onIslA)
        {
            return Difference(dstloop, islandSegsA, islandSegsB, out onIslA, true);
        }

        /// <summary>
        /// Perform a difference operation between two islands using a reflow strategy.
        /// </summary>
        /// <param name="dstloop">The destination of where the differenced path will be placed.</param>
        /// <param name="islandSegsA">A list of all the nodes in island A.</param>
        /// <param name="islandSegsB">A list of all the nodes in island B.</param>
        /// <param name="onIslA">A node on the resulting path.</param>
        /// <param name="processFullOverlaps">If true, check and handle is one of the parameter islands 
        /// completly wraps around the other island.</param>
        /// <returns>The results from the operation.</returns>
        /// <remarks>islandSegsA and islandSegsB should only contain elements in the island, and should not 
        /// be confused with all nodes in the parent loop.</remarks>
        public static BoundingMode Difference(BLoop dstloop, List<BNode> islandSegsA, List<BNode> islandSegsB, out BNode onIslA, bool processFullOverlaps)
        {
            // If there is any interaction, a copy of islandB is made and used - this means 
            // if islandB should be removed, it is up to the caller to remove it themselves.
            //
            // This is done because we don't know if the shape being subtracted is part of a 
            // bigger operation where it's subtracted against multiple islands for multi-island
            // loop subtraction, or shape subtraction.

            float leftWind = BNode.CalculateWinding(islandSegsA[0].Travel());
            float rightWind = BNode.CalculateWinding(islandSegsB[0].Travel());

            // They need to have opposite windings.
            if (leftWind > 0.0f == rightWind > 0.0f)
                islandSegsB[0].ReverseChainOrder();


            List<Utils.BezierSubdivSample> delCollisions = new List<Utils.BezierSubdivSample>();
            GetLoopCollisionInfo(islandSegsA, islandSegsB, delCollisions);
            Utils.BezierSubdivSample.CleanIntersectionList(delCollisions);

            onIslA = null;

            // If we have an odd number of collisions, we have a problem because we have
            // any entry without an exit which will lead to corrupt topology. For now we
            // don't have a way to resolve that, but at least we can exit on 1 without
            // causing issues (in theory at least).
            if (delCollisions.Count == 1)
                return BoundingMode.NoCollision;

            if (delCollisions.Count == 0)
            {
                BoundingMode bm = GetLoopBoundingMode(islandSegsA, islandSegsB);

                if (processFullOverlaps == false)
                {
                    onIslA = islandSegsA[0];
                    return bm;
                }

                if (bm == BoundingMode.NoCollision)
                {
                    onIslA = islandSegsA[0];
                    return BoundingMode.NoCollision;
                }
                else if (bm == BoundingMode.RightSurroundsLeft)
                {
                    // Everything was subtracted out
                    foreach (BNode bn in islandSegsA)
                    {
                        onIslA = bn;
                        bn.SetParent(null);
                    }

                    return BoundingMode.RightSurroundsLeft;
                }
                else if (bm == BoundingMode.LeftSurroundsRight)
                {
                    // Leave the reverse winding inside as a hollow cavity - and 
                    // nothing needs to be changed.
                    Dictionary<BNode, BNode> insertCloneMap = BNode.CloneNodes(islandSegsB, false);
                    foreach (BNode bn in insertCloneMap.Values)
                        bn.SetParent(dstloop);

                    onIslA = islandSegsA[0];

                    return BoundingMode.LeftSurroundsRight;
                }
            }

            // Make sure we have no overlaps of intersections. We currently 
            // can't handle such things.
            HashSet<Utils.NodeTPos> intersections = new HashSet<Utils.NodeTPos>();
            foreach (Utils.BezierSubdivSample bss in delCollisions)
            {
                if (intersections.Add(bss.a.TPos()) == false)
                    return BoundingMode.Degenerate;

                if (intersections.Add(bss.b.TPos()) == false)
                    return BoundingMode.Degenerate;
            }

            // Add everything in the copy in. We'll clip loose ends later to get 
            // rid of the trash.
            Dictionary<BNode, BNode> cloneMap = BNode.CloneNodes(islandSegsB, false);
            foreach (BNode bn in cloneMap.Values)
                bn.SetParent(dstloop);

            // Well, if we're going to make a copy in its place, that means we need to remap
            // all references...
            for (int i = 0; i < delCollisions.Count; ++i)
            {
                Utils.BezierSubdivSample bss = delCollisions[i];
                bss.b.node = cloneMap[bss.b.node];
                delCollisions[i] = bss;
            }

            Dictionary<Utils.NodeTPos, BNode.SubdivideInfo> colSlideInfo = SliceCollisionInfo(delCollisions);
            Dictionary<Utils.NodeTPos, BNode> createdSubdivs = new Dictionary<Utils.NodeTPos, BNode>();
            SplitCollection splitCol = new SplitCollection(dstloop, delCollisions, createdSubdivs);

            HashSet<BNode> looseEnds = new HashSet<BNode>();

            // Note that nothing from B will be tagged as a loose end. Instead, we're
            // forcing the entire island of B to be removed after the Per-Island
            // processing.
            foreach (Utils.BezierSubdivSample bss in delCollisions)
            {
                BNode.SubdivideInfo sdiA = colSlideInfo[bss.GetTPosA()];
                BNode.SubdivideInfo sdiB = colSlideInfo[bss.GetTPosB()];
                float wind = Utils.Vector2Cross(sdiA.subOut, sdiB.subOut);
                BNode colNode = createdSubdivs[bss.GetTPosA()];
                onIslA = colNode;

                if (leftWind > 0 == wind > 0.0f)
                {
                    // A CCW transition will go from A to B.
                    BNode nA = splitCol.GetPreviousTo(bss.GetTPosA());
                    BNode nB = splitCol.GetNextTo(bss.GetTPosB());

                    nA.TanOut = sdiA.prevOut;
                    nB.TanIn = sdiB.nextIn;

                    colNode.UseTanIn = bss.a.node.IsLine() == false;
                    colNode.UseTanOut = bss.b.node.IsLine() == false;
                    colNode.TanIn = sdiA.subIn;
                    colNode.TanOut = sdiB.subOut;

                    nA.next = colNode;
                    colNode.prev = nA;
                    nB.prev = colNode;
                    colNode.next = nB;

                    looseEnds.Add(bss.b.node);
                    looseEnds.Add(bss.a.node.next);

                }
                else
                {
                    // A CW transition will go from the other to it.
                    BNode nA = splitCol.GetNextTo(bss.GetTPosA());
                    BNode nB = splitCol.GetPreviousTo(bss.GetTPosB());

                    nA.TanIn = sdiA.nextIn;
                    nB.TanOut = sdiB.prevOut;

                    colNode.UseTanIn = bss.b.node.IsLine() == false;
                    colNode.UseTanOut = bss.a.node.IsLine() == false;
                    colNode.TanIn = sdiB.subIn;
                    colNode.TanOut = sdiA.subOut;

                    nB.next = colNode;
                    colNode.prev = nB;
                    nA.prev = colNode;
                    colNode.next = nA;

                    looseEnds.Add(bss.b.node.next);
                    looseEnds.Add(bss.a.node);
                }
            }

            // Figure out what internal items need to be removed by 
            // checking which nodes have unmatching connectivity.
            ClipLooseEnds(looseEnds);

            return BoundingMode.Collision;
        }

        /// <summary>
        /// Perform an intersection operation between two islands using a reflow strategy.
        /// </summary>
        /// <param name="left">The collection of islands for the operation.</param>
        /// <param name="right">The other collection of islands for the operation.</param>
        /// <param name="onIslA">An output node that exists on the remaining path(s).</param>
        /// <param name="removeRight">If true, remove the contents of the right loop parameter after
        /// the operation.</param>
        public static void Intersection(BLoop left, BLoop right, out BNode onIslA, bool removeRight)
        {
            onIslA = null;
            // Sanity check on geometry, try to union each loop with its islands
            // so there's no overlapping regions within them.

            List<BNode> rightIslands = right.GetIslands();
            if (rightIslands.Count >= 2)
            {
                for (int i = 1; i < rightIslands.Count; ++i)
                {
                    List<BNode> RSegsA = new List<BNode>(rightIslands[0].Travel());
                    List<BNode> RSegsB = new List<BNode>(rightIslands[i].Travel());

                    Union(right, RSegsA, RSegsB, out onIslA, true);
                }
            }

            List<BNode> leftIslands = left.GetIslands();
            if (leftIslands.Count >= 2)
            {
                for (int i = 1; i < leftIslands.Count; ++i)
                {
                    List<BNode> LSegsA = new List<BNode>(leftIslands[0].Travel());
                    List<BNode> LSegsB = new List<BNode>(leftIslands[i].Travel());

                    Union(right, LSegsA, LSegsB, out onIslA, true);
                }
            }

            leftIslands = left.GetIslands();
            rightIslands = right.GetIslands();

            foreach (BNode leftIsl in leftIslands)
            {
                List<BNode> leftIslandSegs = new List<BNode>(leftIsl.Travel());
                foreach (BNode rightIsl in rightIslands)
                {
                    List<BNode> rightIslandSegs = new List<BNode>(rightIsl.Travel());

                    Intersection(
                        left,
                        leftIslandSegs,
                        rightIslandSegs,
                        out onIslA);
                }
            }

            foreach (BNode bn in leftIslands)
                bn.RemoveIsland(false);

            foreach (BNode bn in rightIslands)
                bn.RemoveIsland(false);

            // TODO: For each island left, we need to see if there's 
            // any shapes being fully contained by the other side.

            if (removeRight == true)
            {
                right.Clear();
                RemoveLoop(right, true);
            }
        }

        /// <summary>
        /// Perform an intersection operation between two islands using a reflow strategy.
        /// </summary>
        /// <param name="dst">The destination of where the intersected path will be placed.</param>
        /// <param name="islandSegsA">A list of all the nodes in island A.</param>
        /// <param name="islandSegsB">A list of all the nodes in island B.</param>
        /// <param name="onIslA">If the islands were processed, this output parameter contains a node
        /// on the new shape.</param>
        /// <returns>The results from the operation.</returns>
        /// <remarks>islandSegsA and islandSegsB should only contain elements in the island, and should not 
        /// be confused with all nodes in the parent loop.</remarks>
        public static BoundingMode Intersection(
            BLoop dst, 
            List<BNode> islandSegsA, 
            List<BNode> islandSegsB, 
            out BNode onIslA)
        {
            // For the intersection, if there's ANY geometry return, it's a copy.
            // It's expected after this operation that the original islands will
            // be destroyed and only the copies will be left.

            float leftWinding = BNode.CalculateWinding(islandSegsA[0].Travel());
            float rightWinding = BNode.CalculateWinding(islandSegsB[0].Travel());

            if (leftWinding > 0.0f != rightWinding > 0.0f)
                islandSegsB[0].ReverseChainOrder();

            onIslA = null;

            List<Utils.BezierSubdivSample> delCollisions = new List<Utils.BezierSubdivSample>();
            GetLoopCollisionInfo(islandSegsA, islandSegsB, delCollisions);
            Utils.BezierSubdivSample.CleanIntersectionList(delCollisions);

            if (delCollisions.Count == 0)
            {
                BoundingMode bm = GetLoopBoundingMode(islandSegsA, islandSegsB);

                if (bm == BoundingMode.NoCollision)
                {
                    return BoundingMode.NoCollision;
                }
                else if (bm == BoundingMode.RightSurroundsLeft)
                {
                    // If the right is fully surrounded, the right is kept.
                    Dictionary<BNode, BNode> insertCloneMap = BNode.CloneNodes(islandSegsA, false);
                    foreach (BNode bn in insertCloneMap.Values)
                    {
                        onIslA = bn;
                        bn.SetParent(dst);
                    }

                    onIslA = islandSegsA[0];
                    return BoundingMode.RightSurroundsLeft;
                }
                else if (bm == BoundingMode.LeftSurroundsRight)
                {
                    // If the left is fully surrounded, the left is kept.
                    Dictionary<BNode, BNode> insertCloneMap = BNode.CloneNodes(islandSegsB, false);
                    foreach (BNode bn in insertCloneMap.Values)
                    {
                        onIslA = bn;
                        bn.SetParent(dst);
                    }

                    onIslA = islandSegsA[0];
                    return BoundingMode.LeftSurroundsRight;
                }
            }

            // Make copies of both, remap and keep their intersection.
            Dictionary<BNode, BNode> cloneMapA = BNode.CloneNodes(islandSegsA, false);
            Dictionary<BNode, BNode> cloneMapB = BNode.CloneNodes(islandSegsB, false);
            foreach (BNode bn in cloneMapA.Values)
                bn.SetParent(dst);

            foreach (BNode bn in cloneMapB.Values)
                bn.SetParent(dst);

            for (int i = 0; i < delCollisions.Count; ++i)
            {
                Utils.BezierSubdivSample bss = delCollisions[i];

                bss.a.node = cloneMapA[bss.a.node];
                bss.b.node = cloneMapB[bss.b.node];


                delCollisions[i] = bss;
            }

            Dictionary<Utils.NodeTPos, BNode.SubdivideInfo> colSlideInfo = SliceCollisionInfo(delCollisions);
            Dictionary<Utils.NodeTPos, BNode> createdSubdivs = new Dictionary<Utils.NodeTPos, BNode>();
            SplitCollection splitCol = new SplitCollection(dst, delCollisions, createdSubdivs);

            //left.nodes.Clear();
            //foreach(BNode bn in islandSegsA)
            //    bn.SetParent(null, false);
            //
            ////right.DumpInto(dst); // Move everything in from the other loop
            foreach (BNode bn in islandSegsB)
                bn.SetParent(dst, false);

            HashSet<BNode> looseEnds = new HashSet<BNode>();

            foreach (Utils.BezierSubdivSample bss in delCollisions)
            {
                BNode.SubdivideInfo sdiA = colSlideInfo[bss.GetTPosA()];
                BNode.SubdivideInfo sdiB = colSlideInfo[bss.GetTPosB()];
                float wind = Utils.Vector2Cross(sdiA.subOut, sdiB.subOut);
                BNode colNode = createdSubdivs[bss.GetTPosA()];
                onIslA = colNode;

                if (wind < 0.0f != leftWinding < 0.0f)
                {
                    BNode nA = splitCol.GetNextTo(bss.GetTPosA());
                    BNode nB = splitCol.GetPreviousTo(bss.GetTPosB());

                    nA.TanIn = sdiA.nextIn;
                    nB.TanOut = sdiB.prevOut;

                    colNode.UseTanIn = bss.b.node.IsLine() == false;
                    colNode.UseTanOut = bss.a.node.IsLine() == false;
                    colNode.TanIn = sdiB.subIn;
                    colNode.TanOut = sdiA.subOut;

                    nB.next = colNode;
                    colNode.prev = nB;
                    nA.prev = colNode;
                    colNode.next = nA;

                    looseEnds.Add(bss.a.node);
                    looseEnds.Add(splitCol.GetSplitInfo(bss.b.node).origNext);

                }
                else
                {
                    BNode nA = splitCol.GetPreviousTo(bss.GetTPosA());
                    BNode nB = splitCol.GetNextTo(bss.GetTPosB());

                    nA.TanOut = sdiA.prevOut;
                    nB.TanIn = sdiB.nextIn;

                    colNode.UseTanIn = bss.a.node.IsLine() == false;
                    colNode.UseTanOut = bss.b.node.IsLine() == false;
                    colNode.TanIn = sdiA.subIn;
                    colNode.TanOut = sdiB.subOut;

                    nA.next = colNode;
                    colNode.prev = nA;
                    nB.prev = colNode;
                    colNode.next = nB;

                    looseEnds.Add(splitCol.GetSplitInfo(bss.a.node).origNext);
                    looseEnds.Add(bss.b.node);
                }
            }

            // Figure out what internal items need to be removed by 
            // checking which nodes have unmatching connectivity.
            ClipLooseEnds(looseEnds);
            return BoundingMode.Collision;
        }

        // TODO: Remove, looks like a placeholder that never got implemented
        public static void Exclusion(BLoop left, BLoop right, bool removeRight)
        {
            if (left == right || left == null || right == null)
                return;
        }

        /// <summary>
        /// Check if the node is horizontally on the same line as a specified y coordinate.
        /// 
        /// Certain detections in this library are done by raycasting to the right. This creates
        /// a major edgecase when horizontal lines exist on the path that are colinear to the
        /// ray being casted. This function is used to help detect that colinearity.
        /// </summary>
        /// <param name="node">The node to check.</param>
        /// <param name="y">The y position to compare against.</param>
        /// <param name="eps">The error epsilon.</param>
        /// <returns>If true, the node has a Y position similar to the y parameter.</returns>
        public static bool _HasYOnLine(BNode node, float y, float eps)
        {
            return Mathf.Abs(node.Pos.y - y) < eps;
        }

        /// <summary>
        /// Check if the node is part of a horizontal line that exists on a specified y coordinate.
        /// See _HasYOnLine() for more details.
        /// </summary>
        /// <param name="node">The node to check.</param>
        /// <param name="y">The y position to compare against.</param>
        /// <param name="eps">The error epsilon.</param>
        /// <returns>If true, the line from the node to the node's next creates a line on the y parameter.</returns>
        public static bool _HasYEndOnLine(BNode node, float y, float eps)
        {
            return
                _HasYOnLine(node, y, eps) ||
                (node.next != null && _HasYOnLine(node.next, y, eps));
        }

        /// <summary>
        /// For a specified node, check the neighbors for nodes on the same specified y position,
        /// forming a chain of horizontal line segments at a specific y. See _HasYOnLine() for more
        /// information on why.
        /// </summary>
        /// <param name="node">The node suspected to be a chain of horizontal line segments at y.</param>
        /// <param name="y">>The y position to compare against.</param>
        /// <param name="eps">The error epsilon.</param>
        /// <returns>A list of neighboring nodes that the parameter node is involved in, that make a set of 
        /// consecutive horizontal line segments at y.</returns>
        private static List<BNode> _OnSameChainAndY(BNode node, float y, float eps)
        {
            List<BNode> ret = new List<BNode>();

            if (Mathf.Abs(node.Pos.y - y) <= eps)
            { }
            else if (Mathf.Abs(node.next.Pos.y - y) <= eps)
                node = node.next;
            else
                return ret;

            // Move backwards to the start of the set of neighbors on the 
            // target y position.
            BNode bnIt = node;
            while (
                bnIt.prev != null &&
                bnIt.prev != node) // cyclic sanity check
            {
                if (Mathf.Abs(bnIt.prev.Pos.y - y) <= eps)
                    bnIt = bnIt.prev;
                else
                    break;
            }

            ret.Add(bnIt);
            bnIt = bnIt.next;

            // Move forwards through the group of neighbors all positioned
            // on the same y position.
            while (true)
            {
                if (Mathf.Abs(bnIt.Pos.y - y) > eps)
                    break;

                ret.Add(bnIt);

                bnIt = bnIt.next;
                if (bnIt.next == null ||
                    bnIt.next == node) // Cyclic sanity check
                    break;
            }

            return ret;
        }

        /// <summary>
        /// Used for cleaning up stray garbage as a result of a reflow boolean
        /// operation. When creating new booleaned shapes by redirecting existing
        /// path segments, lingering segments can remain that are undesired. These
        /// will be reffered to as "loose ends".
        /// 
        /// Given a few known loose ends, find the rest of the neighboring loose ends
        /// and remove all of them.
        /// </summary>
        /// <param name="ends">A known loose end to scan and remove from.</param>
        public static void ClipLooseEnds(IEnumerable<BNode> ends)
        {
            HashSet<BNode> alreadyChecked = new HashSet<BNode>();

            Queue<BNode> toProcess = new Queue<BNode>(ends);
            while (toProcess.Count > 0)
            {
                BNode bn = toProcess.Dequeue();
                if (alreadyChecked.Add(bn) == false)
                    continue;

                if (bn == null) //Sanity
                    continue;

                bool clip = false;
                if (bn.next != null && bn.next.prev != bn)
                    clip = true;
                else if (bn.prev != null && bn.prev.next != bn)
                    clip = true;

                if (clip == true)
                {
                    if (bn.prev != null)
                    {
                        if (alreadyChecked.Contains(bn.prev) == false)
                            toProcess.Enqueue(bn.prev);

                        bn.prev = null;
                    }

                    if (bn.next != null)
                    {
                        if (alreadyChecked.Contains(bn.next) == false)
                            toProcess.Enqueue(bn.next);

                        bn.next = null;
                    }

                    if (bn.parent != null)
                    {
                        bn.parent.RemoveNode(bn);
                        bn.parent = null;
                    }
                }
            }
        }
    }
}