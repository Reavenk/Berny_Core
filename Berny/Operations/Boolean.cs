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

namespace PxPre
{ 
    namespace Berny
    { 
        /// <summary>
        /// Utility class to perform boolean operations between closed shapes.
        /// </summary>
        public static class Boolean
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
                /// The nodes do not intersect paths, but the left island completely surround the right island.
                /// </summary>
                LeftSurroundsRight,

                /// <summary>
                /// The nodes do not intersect paths, but the right island completly surrounds the left island.
                /// </summary>
                RightSurroundsLeft
            }

            public class SplitInfo
            { 
                public enum SplitResult
                { 
                    None,
                    OnBoundary,
                    End,
                    OnlyOne,
                    Between   
                }

                public readonly BNode origPrev;
                public readonly BNode origNext;
                public readonly BNode node;

                public List<Utils.NodeTPos> splits = new List<Utils.NodeTPos>();

                public SplitInfo(BNode node)
                { 
                    this.node = node;

                    splits.Add(new Utils.NodeTPos(node, 0.0f));
                    this.origPrev = node.prev;
                    this.origNext = node.next;
                }

                public SplitResult GetNode(float t, out Utils.NodeTPos left, out Utils.NodeTPos right)
                { 
                    if(t < 0.0f || t > 1.0f)
                    { 
                        left = new Utils.NodeTPos(null, t);
                        right = new Utils.NodeTPos(null, t);
                        return SplitResult.OnBoundary;
                    }

                    if(splits.Count < 2)
                    {
                        left = this.splits[0];
                        right = this.splits[0];
                        return SplitResult.OnlyOne;
                    }

                    for(int i = 0; i < splits.Count; ++i)
                    { 
                        if(splits[i].t == t)
                        { 
                            left = this.splits[i];
                            right = this.splits[i];
                            return SplitResult.OnBoundary;
                        }
                        else if(this.splits[i].t > t)
                        {
                            left = this.splits[i - 1];
                            right = this.splits[i];
                            return SplitResult.Between;
                        }
                    }

                    left = this.splits[this.splits.Count - 2];
                    right = this.splits[this.splits.Count - 1];
                    return SplitResult.End;
                }

                public bool AddEntry(float t, BNode node)
                { 
                    if(t < 0.0f || t > 1.0f)
                        return false;

                    // It needs to be an ordered insertion.
                    for(int i = 0; i < this.splits.Count; ++i)
                    { 
                        // Collisions not allowed!
                        if(this.splits[i].t == t)
                            return false;

                        if(this.splits[i].t < t)
                            continue;
                        
                        this.splits.Insert(i, new Utils.NodeTPos(node, t));
                        return true;
                    }

                    this.splits.Add(new Utils.NodeTPos(node, t));
                    return true;
                }

                public bool AddEntry(Utils.NodeTPos ntp)
                { 
                    return this.AddEntry(ntp.t, ntp.node);
                }
            }

            public class SplitCollection
            {
                public Dictionary<BNode, SplitInfo> splits = new Dictionary<BNode, SplitInfo>();


                public SplitCollection(
                    BLoop dstLoop,
                    List<Utils.BezierSubdivSample> delCollisions,
                    Dictionary<Utils.NodeTPos, BNode> createdSubdivs)
                { 
                    this.SetupFromCollisionData(dstLoop, delCollisions, createdSubdivs);
                }

                public SplitCollection()
                { }

                public SplitInfo GetSplitInfo(BNode bn)
                { 
                    SplitInfo ret;
                    if(this.splits.TryGetValue(bn, out ret) == false)
                    { 
                        ret = new SplitInfo(bn);
                        this.splits.Add(bn, ret);
                    }
                    return ret;
                }

                public BNode GetPreviousTo(Utils.NodeTPos ntp)
                { 
                    return this.GetPreviousTo(ntp.node, ntp.t);
                }

                public BNode GetPreviousTo(BNode node, float t)
                {
                    SplitInfo si;
                    if (this.splits.TryGetValue(node, out si) == true)
                    { 
                        for(int i = 0; i < this.splits.Count; ++i)
                        { 
                            if(t > si.splits[i].t)
                                continue;

                            if(t == si.splits[i].t)
                            { 
                                // exit this part to visit the previous node.
                                if(i == 0)
                                    break;

                                return si.splits[i-1].node;
                            }

                            return si.splits[i].node;
                        }

                        return si.splits[si.splits.Count - 1].node;
                    }

                    // Get the last of the previous

                    // If we don't have a record of it, it hasn't been split, so 
                    // we just send the previous node.
                    if(this.splits.TryGetValue(si.origPrev, out si) == false)
                        return si.origPrev;

                    // If it has been split, we want the very last one.
                    return si.splits[si.splits.Count - 1].node;
                }

                public BNode GetNextTo(Utils.NodeTPos ntp)
                { 
                    return this.GetNextTo(ntp.node, ntp.t);
                }

                public BNode GetNextTo(BNode node, float t)
                {
                    SplitInfo si;
                    if(this.splits.TryGetValue(node, out si) == true)
                    { 
                        for(int i = 0; i < si.splits.Count; ++i)
                        { 
                            if(si.splits[i].t < t)
                                continue;

                            // If the last item, we're returning the
                            // next item.
                            if(i == si.splits.Count - 1)
                                break;

                            // If we found what's past up, return 1 past that.
                            return si.splits[i + 1].node;
                        }
                    }

                    // The first entry of any split will always be the original node
                    return si.origNext;
                }

                public void SetupFromCollisionData(
                    BLoop dstLoop, 
                    List<Utils.BezierSubdivSample> delCollisions,
                    Dictionary<Utils.NodeTPos, BNode> createdSubdivs)
                {
                    foreach (Utils.BezierSubdivSample bss in delCollisions)
                    {
                        Vector2 pos = bss.nodeA.CalculatetPoint(bss.lAEst);
                        BNode newSubNode = new BNode(dstLoop, pos);
                        dstLoop.nodes.Add(newSubNode);

                        createdSubdivs.Add(bss.GetTPosA(), newSubNode);
                        createdSubdivs.Add(bss.GetTPosB(), newSubNode);

                        SplitInfo sia = this.GetSplitInfo(bss.nodeA);
                        sia.AddEntry(bss.lAEst, newSubNode);

                        SplitInfo sib = this.GetSplitInfo(bss.nodeB);
                        sib.AddEntry(bss.lBEst, newSubNode);
                    }
                }
            }

            public static void Union(BLoop dst, out BNode onIslA, params BLoop [] others)
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


                foreach(BNode islA in islandsA)
                {
                    onIslA = islA;
                    foreach (BNode islB in islandB)
                    {
                        List<BNode> islandSegsA = new List<BNode>(islA.Travel());
                        List<BNode> islandSegsB = new List<BNode>(islB.Travel());

                        op(left, islandSegsA, islandSegsB, out onIslA);
                    }
                }
                if(removeRight == true)
                    RemoveLoop(right, true);
            }

            public static BoundingMode Union(BLoop dst, List<BNode> islandSegsA, List<BNode> islandSegsB, out BNode onIslA)
            { 
                return Union(dst, islandSegsA, islandSegsB, out onIslA, true);
            }

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
                        foreach(BNode bn in islandSegsB)
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

                    if(mergeNonCol == true)
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

                        colNode.UseTanIn = bss.nodeA.IsLine() == false;
                        colNode.UseTanOut = bss.nodeB.IsLine() == false;
                        colNode.TanIn = sdiA.subIn;
                        colNode.TanOut = sdiB.subOut;

                        nA.next = colNode;
                        colNode.prev = nA;
                        nB.prev = colNode;
                        colNode.next = nB;

                        looseEnds.Add(bss.nodeB);
                        looseEnds.Add(splitCol.GetSplitInfo(bss.nodeA).origNext);

                    }
                    else
                    {
                        // A CW transition will go from the other to it.
                        BNode nA = splitCol.GetNextTo(bss.GetTPosA());
                        BNode nB = splitCol.GetPreviousTo(bss.GetTPosB());

                        nA.TanIn = sdiA.nextIn;
                        nB.TanOut = sdiB.prevOut;

                        colNode.UseTanIn = bss.nodeB.IsLine() == false;
                        colNode.UseTanOut = bss.nodeA.IsLine() == false;
                        colNode.TanIn = sdiB.subIn;
                        colNode.TanOut = sdiA.subOut;

                        nB.next = colNode;
                        colNode.prev = nB;
                        nA.prev = colNode;
                        colNode.next = nA;

                        looseEnds.Add(splitCol.GetSplitInfo(bss.nodeB).origNext);
                        looseEnds.Add(bss.nodeA);
                    }
                }

                // Figure out what internal items need to be removed by 
                // checking which nodes have unmatching connectivity.
                ClipLooseEnds(looseEnds);

                return BoundingMode.Collision;

            }

            public static void Difference(BLoop left, BLoop right, out BNode onIslA)
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

            public static BoundingMode Difference(BLoop dstloop, List<BNode> islandSegsA, List<BNode> islandSegsB, out BNode onIslA)
            {
                return Difference(dstloop, islandSegsA, islandSegsB, out onIslA, true);
            }

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
                if(leftWind > 0.0f == rightWind > 0.0f)
                    islandSegsB[0].ReverseChainOrder();


                List<Utils.BezierSubdivSample> delCollisions = new List<Utils.BezierSubdivSample>();
                GetLoopCollisionInfo(islandSegsA, islandSegsB, delCollisions);
                Utils.BezierSubdivSample.CleanIntersectionList(delCollisions);

                onIslA = null;

                if (delCollisions.Count == 0)
                { 
                    BoundingMode bm = GetLoopBoundingMode(islandSegsA, islandSegsB);

                    if(processFullOverlaps == false)
                    {
                        onIslA = islandSegsA[0];
                        return bm;
                    }

                    if (bm == BoundingMode.NoCollision)
                    {
                        onIslA = islandSegsA[0];
                        return BoundingMode.NoCollision;
                    }
                    else if(bm == BoundingMode.RightSurroundsLeft)
                    { 
                        // Everything was subtracted out
                        foreach(BNode bn in islandSegsA)
                        {
                            onIslA = bn;
                            bn.SetParent(null);
                        }

                        return BoundingMode.RightSurroundsLeft;
                    }
                    else if(bm == BoundingMode.LeftSurroundsRight)
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
                    bss.nodeB = cloneMap[bss.nodeB];
                    delCollisions[i] = bss;
                }

                Dictionary <Utils.NodeTPos, BNode.SubdivideInfo> colSlideInfo = SliceCollisionInfo(delCollisions);
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

                        colNode.UseTanIn = bss.nodeA.IsLine() == false;
                        colNode.UseTanOut = bss.nodeB.IsLine() == false;
                        colNode.TanIn = sdiA.subIn;
                        colNode.TanOut = sdiB.subOut;

                        nA.next = colNode;
                        colNode.prev = nA;
                        nB.prev = colNode;
                        colNode.next = nB;

                        looseEnds.Add(bss.nodeB);
                        looseEnds.Add(bss.nodeA.next);

                    }
                    else
                    {
                        // A CW transition will go from the other to it.
                        BNode nA = splitCol.GetNextTo(bss.GetTPosA());
                        BNode nB = splitCol.GetPreviousTo(bss.GetTPosB());

                        nA.TanIn = sdiA.nextIn;
                        nB.TanOut = sdiB.prevOut;

                        colNode.UseTanIn = bss.nodeB.IsLine() == false;
                        colNode.UseTanOut = bss.nodeA.IsLine() == false;
                        colNode.TanIn = sdiB.subIn;
                        colNode.TanOut = sdiA.subOut;

                        nB.next = colNode;
                        colNode.prev = nB;
                        nA.prev = colNode;
                        colNode.next = nA;

                        looseEnds.Add(bss.nodeB.next);
                        looseEnds.Add(bss.nodeA);
                    }
                }

                // Figure out what internal items need to be removed by 
                // checking which nodes have unmatching connectivity.
                ClipLooseEnds(looseEnds);

                return BoundingMode.Collision;
            }

            public static void Intersection(BLoop left, BLoop right, out BNode onIslA, bool removeRight)
            {
                onIslA = null;
                // Sanity check on geometry, try to union each loop with its islands
                // so there's no overlapping regions within them.

                List<BNode> rightIslands = right.GetIslands();
                if(rightIslands.Count >= 2)
                { 
                    for(int i = 1; i < rightIslands.Count; ++i)
                    {
                        List<BNode> RSegsA = new List<BNode>(rightIslands[0].Travel());
                        List<BNode> RSegsB = new List<BNode>(rightIslands[i].Travel());

                        Union(right, RSegsA, RSegsB, out onIslA, true);
                    }
                }

                List<BNode> leftIslands = left.GetIslands();
                if(leftIslands.Count >= 2)
                { 
                    for(int i = 1; i < leftIslands.Count; ++i)
                    {
                        List<BNode> LSegsA = new List<BNode>(leftIslands[0].Travel());
                        List<BNode> LSegsB = new List<BNode>(leftIslands[i].Travel());

                        Union(right, LSegsA, LSegsB, out onIslA, true);
                    }
                }

                leftIslands = left.GetIslands();
                rightIslands = right.GetIslands();

                foreach(BNode leftIsl in leftIslands)
                { 
                    List<BNode> leftIslandSegs = new List<BNode>(leftIsl.Travel());
                    foreach(BNode rightIsl in rightIslands)
                    {
                        List<BNode> rightIslandSegs = new List<BNode>(rightIsl.Travel());

                        Intersection(
                            left,
                            leftIslandSegs,
                            rightIslandSegs,
                            out onIslA);
                    }
                }

                foreach(BNode bn in leftIslands)
                    bn.RemoveIsland(false);

                foreach(BNode bn in rightIslands)
                    bn.RemoveIsland(false);
                
                // TODO: For each island left, we need to see if there's 
                // any shapes being fully contained by the other side.

                if (removeRight == true)
                {
                    right.Clear();
                    RemoveLoop(right, true);
                }
            }

            public static BoundingMode Intersection(BLoop dst, List<BNode> islandSegsA, List<BNode> islandSegsB, out BNode onIslA)
            {
                // For the intersection, if there's ANY geometry return, it's a copy.
                // It's expected after this operation that the original islands will
                // be destroyed and only the copies will be left.

                float leftWinding = BNode.CalculateWinding(islandSegsA[0].Travel());
                float rightWinding = BNode.CalculateWinding(islandSegsB[0].Travel());

                if(leftWinding > 0.0f != rightWinding > 0.0f)
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
                foreach(BNode bn in cloneMapA.Values)
                    bn.SetParent(dst);

                foreach (BNode bn in cloneMapB.Values)
                    bn.SetParent(dst);

                for (int i = 0; i < delCollisions.Count; ++i)
                {
                    Utils.BezierSubdivSample bss = delCollisions[i];

                    bss.nodeA = cloneMapA[bss.nodeA];
                    bss.nodeB = cloneMapB[bss.nodeB];


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
                foreach(BNode bn in islandSegsB)
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

                        colNode.UseTanIn = bss.nodeB.IsLine() == false;
                        colNode.UseTanOut = bss.nodeA.IsLine() == false;
                        colNode.TanIn = sdiB.subIn;
                        colNode.TanOut = sdiA.subOut;

                        nB.next = colNode;
                        colNode.prev = nB;
                        nA.prev = colNode;
                        colNode.next = nA;

                        looseEnds.Add(bss.nodeA);
                        looseEnds.Add(splitCol.GetSplitInfo(bss.nodeB).origNext);

                    }
                    else
                    {
                        BNode nA = splitCol.GetPreviousTo(bss.GetTPosA());
                        BNode nB = splitCol.GetNextTo(bss.GetTPosB());

                        nA.TanOut = sdiA.prevOut;
                        nB.TanIn = sdiB.nextIn;

                        colNode.UseTanIn = bss.nodeA.IsLine() == false;
                        colNode.UseTanOut = bss.nodeB.IsLine() == false;
                        colNode.TanIn = sdiA.subIn;
                        colNode.TanOut = sdiB.subOut;

                        nA.next = colNode;
                        colNode.prev = nA;
                        nB.prev = colNode;
                        colNode.next = nB;

                        looseEnds.Add(splitCol.GetSplitInfo(bss.nodeA).origNext);
                        looseEnds.Add(bss.nodeB);
                    }
                }

                // Figure out what internal items need to be removed by 
                // checking which nodes have unmatching connectivity.
                ClipLooseEnds(looseEnds);
                return BoundingMode.Collision;
            }

            public static void Exclusion(BLoop left, BLoop right, bool removeRight)
            {
                if (left == right || left == null || right == null)
                    return;
            }

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

            // Potential loose ends to start checking from
            public static void ClipLooseEnds(IEnumerable<BNode> ends)
            { 
                HashSet<BNode> alreadyChecked = new HashSet<BNode>();

                Queue<BNode> toProcess = new Queue<BNode>(ends);
                while(toProcess.Count > 0)
                { 
                    BNode bn = toProcess.Dequeue();
                    if(alreadyChecked.Add(bn) == false)
                        continue;

                    if(bn == null) //Sanity
                        continue;

                    bool clip = false;
                    if(bn.next != null && bn.next.prev != bn)
                        clip = true;
                    else if(bn.prev != null && bn.prev.next != bn)
                        clip = true;

                    if(clip == true)
                    { 
                        if(bn.prev != null)
                        {
                            if(alreadyChecked.Contains(bn.prev) == false)
                                toProcess.Enqueue(bn.prev);

                            bn.prev = null;
                        }

                        if(bn.next != null)
                        { 
                            if(alreadyChecked.Contains(bn.next) == false)
                                toProcess.Enqueue(bn.next);

                            bn.next = null;
                        }

                        if(bn.parent != null)
                        { 
                            bn.parent.RemoveNode(bn);
                            bn.parent = null;
                        }
                    }
                }
            }

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

            public static Dictionary<Utils.NodeTPos, BNode.SubdivideInfo> SliceCollisionInfo(List<Utils.BezierSubdivSample> collisions)
            {
                Dictionary<Utils.NodeTPos, BNode.SubdivideInfo> ret = 
                    new Dictionary<Utils.NodeTPos, BNode.SubdivideInfo>();

                Dictionary<BNode, HashSet<float>> subdivLocs = 
                    new Dictionary<BNode, HashSet<float>>();

                // Get all the unique subdivision locations for both parts of 
                // each collision.
                foreach(Utils.BezierSubdivSample bss in collisions)
                { 
                    HashSet<float> hsA;
                    if(subdivLocs.TryGetValue(bss.nodeA, out hsA) == false)
                    { 
                        hsA = new HashSet<float>();
                        subdivLocs.Add(bss.nodeA, hsA);
                    }

                    hsA.Add(bss.lAEst);

                    HashSet<float> hsB;
                    if(subdivLocs.TryGetValue(bss.nodeB, out hsB) == false)
                    { 
                        hsB = new HashSet<float>();
                        subdivLocs.Add(bss.nodeB, hsB);
                    }

                    hsB.Add(bss.lBEst);
                }

                foreach(KeyValuePair<BNode, HashSet<float>> kvp in subdivLocs)
                { 
                    BNode node = kvp.Key;
                    List<float> subs = new List<float>(kvp.Value);
                    subs.Sort();


                    if(node.UseTanOut == false && node.next.UseTanIn == false)
                    { 
                        // The scale isn't useful, but the direction is, for 
                        // winding purposes later.
                        Vector2 outTan = (node.next.Pos - node.Pos);
                        for(int i = 0; i < subs.Count; ++i)
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
                            //
                            ret.Add( new Utils.NodeTPos(node, subs[i]), si);
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
                            float realT = (curT - lm)/(1.0f - lm);

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

                            si.prevOut = subSpots[idx -2] - subSpots[idx - 3];
                            si.nextIn = subSpots[idx + 2] - subSpots[idx + 3];

                            ret.Add(new Utils.NodeTPos(node, subs[i]), si);
                        }
                    }
                }

                return ret;
            }

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
                foreach(BNode nOth in islandSegsB)
                    nOth.ProjectSegment(mptL, rayEnd, interCurve, interLine);
                
                int leftcols = 0;
                for(int i = 0; i < interLine.Count; ++i)
                {
                    float l = interLine[i];
                    if (l <= 0.0f)
                        continue;

                    float c = interCurve[i];
                    if(c < 0.0f || c > 1.0f)
                        continue;

                    ++leftcols;
                }
                // If the right loop is physically more to the right, it could still be untouching - but only if for
                // every entry, there's an exit. Meaning if there's an odd number of collisions, the left is completely
                // inside the right.
                //
                // Keep in mind this logic only works because of the constraint that left and right should not intersect.
                if(leftcols > 0)
                { 
                    if((leftcols % 2) == 1)
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
                foreach(BNode nOth in islandSegsA)
                {
                    //BNode.PathBridge pb = nOth.GetPathBridgeInfo();
                    //
                    //if(pb.pathType == BNode.PathType.Line)
                    //{ 
                    //    float s, t;
                    //    if(Utils.ProjectSegmentToSegment(mptR, rayEnd, nOth.Pos, nOth.next.Pos, out s, out t) == true)
                    //    { 
                    //        interLine.Add(s);
                    //        interCurve.Add(t);
                    //    }
                    //}
                    //else
                    //{
                    //    Vector2 pt0 = nOth.Pos;
                    //    Vector2 pt1 = nOth.Pos + pb.prevTanOut;
                    //    Vector2 pt2 = nOth.next.Pos + pb.nextTanIn;
                    //    Vector2 pt3 = nOth.next.Pos;
                    //
                    //    Utils.IntersectLine(interCurve, interLine, pt0, pt1, pt2, pt3, mptR, rayEnd, false);
                    //}

                    nOth.ProjectSegment(mptR, rayEnd, interCurve, interLine);
                }
                int rightcols = 0;
                for(int i = 0; i < interLine.Count; ++i)
                { 
                    float l = interLine[i];
                    if(l <= 0.0f)
                        continue;

                    float c = interCurve[i];
                    if(c < 0.0f || c > 1.0f)
                        continue;

                    ++rightcols;
                }

                if((rightcols % 2) == 1)
                    return BoundingMode.LeftSurroundsRight;

                return BoundingMode.NoCollision;
            }
        }
    }
}