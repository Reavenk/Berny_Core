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
        public static class Boolean
        { 
            public struct NodeTPos
            { 
                public BNode node;
                public float t;

                public NodeTPos(BNode node, float t)
                { 
                    this.node = node;
                    this.t = t;
                }
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

                public readonly BNode node;
                public List<NodeTPos> splits = new List<NodeTPos>();

                public SplitInfo(BNode node)
                { 
                    this.node = node;

                    splits.Add(new NodeTPos(node, 0.0f));
                }

                public SplitResult GetNode(float t, out NodeTPos left, out NodeTPos right)
                { 
                    if(t < 0.0f || t > 1.0f)
                    { 
                        left = new NodeTPos(null, t);
                        right = new NodeTPos(null, t);
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

                    for(int i = 0; i < this.splits.Count; ++i)
                    { 
                        // Collisions not allowed!
                        if(this.splits[i].t == t)
                            return false;

                        if(t > this.splits[i].t)
                        { 
                            this.splits.Insert(i, new NodeTPos(node, t));
                            return true;
                        }
                    }

                    this.splits.Add(new NodeTPos(node, t));
                    return true;
                }
            }

            public static void Union(BLoop dst, bool removeRight, params BLoop [] others)
            {
                foreach(BLoop bl in others)
                    Union(dst, bl, removeRight);
            }

            public static void Union(BLoop left, BLoop right, bool removeRight)
            { 
                if(left == right || left == null || right == null)
                    return;

                List<BNode> leftIsls = left.GetIslands();
                List<BNode> rightIsls = right.GetIslands();

                // For all island permutations between L and R
                foreach (BNode islL in leftIsls)
                {
                    BNode.EndpointQuery eqL = islL.GetPathLeftmost();
                    // Only closed loops count
                    if (eqL.result == BNode.EndpointResult.SuccessfulEdge)
                        continue;

                    // For each other island
                    foreach (BNode islR in rightIsls)
                    {
                        BNode.EndpointQuery eqR = islR.GetPathLeftmost();
                        // Only closed loops count
                        if(eqR.result == BNode.EndpointResult.SuccessfulEdge)
                            continue;

                        // We'll wait for the end to figure out what internals to clip
                        HashSet<BNode> looseEnds = new HashSet<BNode>();

                        // Test each BNode of ours, intersecting with theirs.
                        // We need to get the left for every island, every time
                        // because left is modified for every loop
                        List<BNode> leftSegs = new List<BNode>(eqL.Enumerate());
                        List<BNode> rightSegs = new List<BNode>(eqR.Enumerate());

                        // Find all intersections and log them.
                        Dictionary<BNode, List<Utils.BezierSubdivSample>> segIntersections =
                            new Dictionary<BNode, List<Utils.BezierSubdivSample>>();

                        foreach (BNode nseg in leftSegs)
                        {
                            Utils.BezierSubdivRgn sdRgn = Utils.BezierSubdivRgn.FromNode(nseg);
                            foreach (BNode noth in rightSegs)
                            {
                                Utils.BezierSubdivRgn sdOtherRgn = Utils.BezierSubdivRgn.FromNode(noth);

                                List<Utils.BezierSubdivSample> lstInter = new List<Utils.BezierSubdivSample>();
                                Utils.SubdivideSample(sdRgn, sdOtherRgn, 20, Mathf.Epsilon, lstInter);
                                Utils.BezierSubdivSample.CleanIntersectionList(lstInter);

                                if (lstInter.Count > 0)
                                {
                                    List<Utils.BezierSubdivSample> outLst;
                                    if (segIntersections.TryGetValue(nseg, out outLst) == false)
                                    {
                                        outLst = new List<Utils.BezierSubdivSample>();
                                        segIntersections.Add(nseg, outLst);
                                    }
                                    foreach (Utils.BezierSubdivSample bss in lstInter)
                                        outLst.Add(bss);
                                }
                            }
                        }

                        // For all intersections, now process them
                        foreach (KeyValuePair<BNode, List<Utils.BezierSubdivSample>> kvp in segIntersections)
                        {
                            List<Utils.BezierSubdivSample> cols = kvp.Value;
                            cols.Sort(
                                (x, y) =>
                                {
                                    if (x.lAEst < y.lAEst)
                                        return -1;
                                    else if (x.lAEst > y.lAEst)
                                        return 1;
                                    return 0;
                                });

                            for (int ii = 0; ii < cols.Count; ++ii)
                            {
                                Utils.BezierSubdivSample bss = cols[ii];

                                BNode.SubdivideInfo sdiIt = cols[ii].nodeA.GetSubdivideInfo(cols[ii].lAEst);
                                BNode.SubdivideInfo sdiOth = cols[ii].nodeB.GetSubdivideInfo(cols[ii].lBEst);

                                Vector2 subPt;
                                if(cols[ii].linearA == true)
                                    subPt = Vector2.Lerp(cols[ii].nodeA.Pos, cols[ii].nodeA.next.Pos, cols[ii].lAEst);
                                else
                                    subPt = sdiIt.subPos;

                                float wind =
                                    Utils.Vector2Cross(
                                        sdiIt.subOut,
                                        sdiOth.subOut);

                                BNode mid = new BNode(left, subPt);
                                left.nodes.Add(mid);
                                mid.tangentMode = BNode.TangentMode.Disconnected;
                                mid.UseTanIn = true;
                                mid.UseTanOut = true;

                                if (wind <= 0.0f)
                                {
                                    // A CCW transition will go from the it to the other.
                                    mid.TanIn = sdiIt.subIn;
                                    mid.TanOut = sdiOth.subOut;

                                    mid.prev = cols[ii].nodeA;
                                    // 
                                    // Create a loose end and record it
                                    looseEnds.Add(cols[ii].nodeA.next);
                                    //
                                    cols[ii].nodeA.next = mid;
                                    cols[ii].nodeA.tangentMode = BNode.TangentMode.Disconnected;
                                    cols[ii].nodeA.UseTanOut = (bss.linearA == false);
                                    cols[ii].nodeA.TanOut = sdiIt.prevOut;

                                    BNode redirPrev = cols[ii].nodeB.next;
                                    //
                                    // Create a loose end and record it
                                    looseEnds.Add(redirPrev.prev);
                                    //
                                    mid.next = redirPrev;
                                    redirPrev.prev = mid;
                                    redirPrev.tangentMode = BNode.TangentMode.Disconnected;
                                    redirPrev.UseTanIn = (bss.linearB == false);
                                    redirPrev.TanIn = sdiOth.nextIn;


                                }
                                else
                                {
                                    // A CW transition will go from the other to it.
                                    mid.TanIn = sdiOth.subIn;
                                    mid.TanOut = sdiIt.subOut;

                                    mid.prev = cols[ii].nodeB;
                                    //
                                    looseEnds.Add(cols[ii].nodeB.next);
                                    //
                                    cols[ii].nodeB.next = mid;
                                    cols[ii].nodeB.tangentMode = BNode.TangentMode.Disconnected;
                                    cols[ii].nodeB.UseTanOut = true;
                                    cols[ii].nodeB.TanOut = sdiOth.prevOut;

                                    BNode redirNext = cols[ii].nodeA.next;
                                    //
                                    looseEnds.Add(cols[ii].nodeA);
                                    //
                                    mid.next = redirNext;
                                    redirNext.prev = mid;
                                    redirNext.tangentMode = BNode.TangentMode.Disconnected;
                                    redirNext.UseTanOut = true;
                                    redirNext.TanIn = sdiIt.nextIn;
                                }

                                // Since we subdivided the edge, the reference to nodeA or nodeB, and the
                                // intersection parameters will no longer be correct. Modify to correct
                                // as needed.
                                for (int jj = ii + 1; jj < cols.Count; ++jj)
                                {
                                    Utils.BezierSubdivSample bssMod = cols[jj];

                                    // Fixup references to nodeA and lAEst.
                                    if (bssMod.lAEst >= cols[ii].lAEst)
                                    {
                                        bssMod.lAEst = Mathf.InverseLerp(cols[ii].lAEst, 1.0f, bssMod.lAEst);
                                        bssMod.nodeA = mid;
                                    }
                                    else
                                    {
                                        bssMod.lAEst = Mathf.InverseLerp(0.0f, cols[ii].lAEst, bssMod.lAEst);
                                    }

                                    // Fixup references to nodeB and lBEst.
                                    if (bssMod.lBEst >= cols[ii].lBEst)
                                    {
                                        bssMod.lBEst = Mathf.InverseLerp(cols[ii].lBEst, 1.0f, bssMod.lBEst);
                                        bssMod.nodeB = mid;
                                    }
                                    else
                                    {
                                        bssMod.lBEst = Mathf.InverseLerp(0.0f, cols[ii].lBEst, bssMod.lBEst);
                                    }

                                    cols[jj] = bssMod;
                                }
                            }
                        }

                        // Figure out what internal items need to be removed by 
                        // checking which nodes have unmatching connectivity.
                        ClipLooseEnds(looseEnds);

                        // Move everything in from the other loop
                        foreach (BNode bn in rightSegs)
                        {
                            // If it's null, it was clipped
                            if (bn.parent == null)
                                continue;

                            // Everything still remaining gets moved
                            // to the endLoop.
                            if (bn.parent != left)
                                bn.parent.nodes.Remove(bn);

                            bn.parent = left;
                            bn.parent.nodes.Add(bn);
                        }
                    }
                }

                if(Utils.verboseDebug == true)
                {
                    if(right.nodes.Count != 0)
                        Debug.Log("Boolean union didn't end up with an empty right loop as expected");
                }

                if(removeRight == true)
                    RemoveLoop(right, true);
            }

            public static void Difference(BLoop left, BLoop right, bool removeRight)
            {
                if (left == right || left == null || right == null)
                    return;

                right.Reverse();

                List<BNode> leftIsls = left.GetIslands();
                List<BNode> rightIsls = right.GetIslands();

                // For all island permutations between L and R
                foreach (BNode islL in leftIsls)
                {
                    BNode.EndpointQuery eqL = islL.GetPathLeftmost();
                    // Only closed loops count
                    if (eqL.result == BNode.EndpointResult.SuccessfulEdge)
                        continue;

                    // For each other island
                    foreach (BNode islR in rightIsls)
                    {
                        BNode.EndpointQuery eqR = islR.GetPathLeftmost();
                        // Only closed loops count
                        if (eqR.result == BNode.EndpointResult.SuccessfulEdge)
                            continue;

                        // We'll wait for the end to figure out what internals to clip
                        HashSet<BNode> looseEnds = new HashSet<BNode>();

                        // Test each BNode of ours, intersecting with theirs.
                        // We need to get the left for every island, every time
                        // because left is modified for every loop
                        List<BNode> leftSegs = new List<BNode>(eqL.Enumerate());
                        List<BNode> rightSegs = new List<BNode>(eqR.Enumerate());

                        // Find all intersections and log them.
                        Dictionary<BNode, List<Utils.BezierSubdivSample>> segIntersections =
                            new Dictionary<BNode, List<Utils.BezierSubdivSample>>();

                        foreach (BNode nseg in leftSegs)
                        {
                            Utils.BezierSubdivRgn sdRgn = Utils.BezierSubdivRgn.FromNode(nseg);
                            foreach (BNode noth in rightSegs)
                            {
                                Utils.BezierSubdivRgn sdOtherRgn = Utils.BezierSubdivRgn.FromNode(noth);

                                List<Utils.BezierSubdivSample> lstInter = new List<Utils.BezierSubdivSample>();
                                Utils.SubdivideSample(sdRgn, sdOtherRgn, 20, Mathf.Epsilon, lstInter);
                                Utils.BezierSubdivSample.CleanIntersectionList(lstInter);

                                if (lstInter.Count > 0)
                                {
                                    List<Utils.BezierSubdivSample> outLst;
                                    if (segIntersections.TryGetValue(nseg, out outLst) == false)
                                    {
                                        outLst = new List<Utils.BezierSubdivSample>();
                                        segIntersections.Add(nseg, outLst);
                                    }
                                    foreach (Utils.BezierSubdivSample bss in lstInter)
                                        outLst.Add(bss);
                                }
                            }
                        }

                        // For all intersections, now process them
                        foreach (KeyValuePair<BNode, List<Utils.BezierSubdivSample>> kvp in segIntersections)
                        {
                            List<Utils.BezierSubdivSample> cols = kvp.Value;
                            cols.Sort(
                                (x, y) =>
                                {
                                    if (x.lAEst < y.lAEst)
                                        return -1;
                                    else if (x.lAEst > y.lAEst)
                                        return 1;
                                    return 0;
                                });

                            for (int ii = 0; ii < cols.Count; ++ii)
                            {
                                Utils.BezierSubdivSample bss = cols[ii];

                                BNode.SubdivideInfo sdiIt = cols[ii].nodeA.GetSubdivideInfo(cols[ii].lAEst);
                                BNode.SubdivideInfo sdiOth = cols[ii].nodeB.GetSubdivideInfo(cols[ii].lBEst);

                                Vector2 subPt;
                                if (cols[ii].linearA == true)
                                    subPt = Vector2.Lerp(cols[ii].nodeA.Pos, cols[ii].nodeA.next.Pos, cols[ii].lAEst);
                                else
                                    subPt = sdiIt.subPos;

                                float wind =
                                    Utils.Vector2Cross(
                                        sdiIt.subOut,
                                        sdiOth.subOut);

                                BNode mid = new BNode(left, subPt);
                                left.nodes.Add(mid);
                                mid.tangentMode = BNode.TangentMode.Disconnected;
                                mid.UseTanIn = true;
                                mid.UseTanOut = true;

                                if (wind >= 0.0f)
                                {
                                    // A CCW transition will go from the it to the other.
                                    mid.TanIn = sdiIt.subIn;
                                    mid.TanOut = sdiOth.subOut;

                                    mid.prev = cols[ii].nodeA;
                                    // 
                                    // Create a loose end and record it
                                    looseEnds.Add(cols[ii].nodeA.next);
                                    //
                                    cols[ii].nodeA.next = mid;
                                    cols[ii].nodeA.tangentMode = BNode.TangentMode.Disconnected;
                                    cols[ii].nodeA.UseTanOut = (cols[ii].linearA == false);
                                    cols[ii].nodeA.TanOut = sdiIt.prevOut;

                                    BNode redirPrev = cols[ii].nodeB.next;
                                    //
                                    // Create a loose end and record it
                                    looseEnds.Add(redirPrev.prev);
                                    //
                                    mid.next = redirPrev;
                                    redirPrev.prev = mid;
                                    redirPrev.tangentMode = BNode.TangentMode.Disconnected;
                                    redirPrev.UseTanIn = (cols[ii].linearB == false);
                                    redirPrev.TanIn = sdiOth.nextIn;
                                }
                                else
                                {
                                    // A CW transition will go from the other to it.
                                    mid.TanIn = sdiOth.subIn;
                                    mid.TanOut = sdiIt.subOut;

                                    mid.prev = cols[ii].nodeB;
                                    //
                                    looseEnds.Add(cols[ii].nodeB);
                                    //
                                    cols[ii].nodeB.next = mid;
                                    cols[ii].nodeB.tangentMode = BNode.TangentMode.Disconnected;
                                    cols[ii].nodeB.UseTanOut = (cols[ii].linearB == false);
                                    cols[ii].nodeB.TanOut = sdiOth.prevOut;

                                    BNode redirNext = cols[ii].nodeA.next;
                                    //
                                    looseEnds.Add(cols[ii].nodeA);
                                    //
                                    mid.next = redirNext;
                                    redirNext.prev = mid;
                                    redirNext.tangentMode = BNode.TangentMode.Disconnected;
                                    redirNext.UseTanOut = (cols[ii].linearA == false);
                                    redirNext.TanIn = sdiIt.nextIn;
                                }

                                // Since we subdivided the edge, the reference to nodeA or nodeB, and the
                                // intersection parameters will no longer be correct. Modify to correct
                                // as needed.
                                for (int jj = ii + 1; jj < cols.Count; ++jj)
                                {
                                    Utils.BezierSubdivSample bssMod = cols[jj];

                                    // Fixup references to nodeA and lAEst.
                                    if (bssMod.lAEst >= cols[ii].lAEst)
                                    {
                                        bssMod.lAEst = Mathf.InverseLerp(cols[ii].lAEst, 1.0f, bssMod.lAEst);
                                        bssMod.nodeA = mid;
                                    }
                                    else
                                    {
                                        bssMod.lAEst = Mathf.InverseLerp(0.0f, cols[ii].lAEst, bssMod.lAEst);
                                    }

                                    // Fixup references to nodeB and lBEst.
                                    if (bssMod.lBEst >= cols[ii].lBEst)
                                    {
                                        bssMod.lBEst = Mathf.InverseLerp(cols[ii].lBEst, 1.0f, bssMod.lBEst);
                                        bssMod.nodeB = mid;
                                    }
                                    else
                                    {
                                        bssMod.lBEst = Mathf.InverseLerp(0.0f, cols[ii].lBEst, bssMod.lBEst);
                                    }

                                    cols[jj] = bssMod;
                                }
                            }
                        }

                        // Figure out what internal items need to be removed by 
                        // checking which nodes have unmatching connectivity.
                        ClipLooseEnds(looseEnds);

                        // Move everything in from the other loop
                        foreach (BNode bn in rightSegs)
                        {
                            // If it's null, it was clipped
                            if (bn.parent == null)
                                continue;

                            // Everything still remaining gets moved
                            // to the endLoop.
                            if (bn.parent != left)
                                bn.parent.nodes.Remove(bn);

                            bn.parent = left;
                            bn.parent.nodes.Add(bn);
                        }
                    }
                }

                if (Utils.verboseDebug == true)
                {
                    if (right.nodes.Count != 0)
                        Debug.Log("Boolean union didn't end up with an empty right loop as expected");
                }

                if (removeRight == true)
                    RemoveLoop(right, true);
            }

            public static void Intersection(BLoop left, BLoop right, bool removeRight)
            {
                if (left == right || left == null || right == null)
                    return;

                List<BNode> leftIsls = left.GetIslands();
                List<BNode> rightIsls = right.GetIslands();

                // For all island permutations between L and R
                foreach (BNode islL in leftIsls)
                {
                    BNode.EndpointQuery eqL = islL.GetPathLeftmost();
                    // Only closed loops count
                    if (eqL.result == BNode.EndpointResult.SuccessfulEdge)
                        continue;

                    // For each other island
                    foreach (BNode islR in rightIsls)
                    {
                        BNode.EndpointQuery eqR = islR.GetPathLeftmost();
                        // Only closed loops count
                        if (eqR.result == BNode.EndpointResult.SuccessfulEdge)
                            continue;

                        // We'll wait for the end to figure out what internals to clip
                        HashSet<BNode> looseEnds = new HashSet<BNode>();

                        // Test each BNode of ours, intersecting with theirs.
                        // We need to get the left for every island, every time
                        // because left is modified for every loop
                        List<BNode> leftSegs = new List<BNode>(eqL.Enumerate());
                        List<BNode> rightSegs = new List<BNode>(eqR.Enumerate());

                        // Find all intersections and log them.
                        Dictionary<BNode, List<Utils.BezierSubdivSample>> segIntersections =
                            new Dictionary<BNode, List<Utils.BezierSubdivSample>>();

                        foreach (BNode nseg in leftSegs)
                        {
                            Utils.BezierSubdivRgn sdRgn = Utils.BezierSubdivRgn.FromNode(nseg);
                            foreach (BNode noth in rightSegs)
                            {
                                Utils.BezierSubdivRgn sdOtherRgn = Utils.BezierSubdivRgn.FromNode(noth);

                                List<Utils.BezierSubdivSample> lstInter = new List<Utils.BezierSubdivSample>();
                                Utils.SubdivideSample(sdRgn, sdOtherRgn, 20, Mathf.Epsilon, lstInter);
                                Utils.BezierSubdivSample.CleanIntersectionList(lstInter);

                                if (lstInter.Count > 0)
                                {
                                    List<Utils.BezierSubdivSample> outLst;
                                    if (segIntersections.TryGetValue(nseg, out outLst) == false)
                                    {
                                        outLst = new List<Utils.BezierSubdivSample>();
                                        segIntersections.Add(nseg, outLst);
                                    }
                                    foreach (Utils.BezierSubdivSample bss in lstInter)
                                        outLst.Add(bss);
                                }
                            }
                        }

                        // For all intersections, now process them
                        foreach (KeyValuePair<BNode, List<Utils.BezierSubdivSample>> kvp in segIntersections)
                        {
                            List<Utils.BezierSubdivSample> cols = kvp.Value;
                            cols.Sort(
                                (x, y) =>
                                {
                                    if (x.lAEst < y.lAEst)
                                        return -1;
                                    else if (x.lAEst > y.lAEst)
                                        return 1;
                                    return 0;
                                });

                            for (int ii = 0; ii < cols.Count; ++ii)
                            {
                                Utils.BezierSubdivSample bss = cols[ii];

                                BNode.SubdivideInfo sdiIt = cols[ii].nodeA.GetSubdivideInfo(cols[ii].lAEst);
                                BNode.SubdivideInfo sdiOth = cols[ii].nodeB.GetSubdivideInfo(cols[ii].lBEst);

                                Vector2 subPt;
                                if (cols[ii].linearA == true)
                                    subPt = Vector2.Lerp(cols[ii].nodeA.Pos, cols[ii].nodeA.next.Pos, cols[ii].lAEst);
                                else
                                    subPt = sdiIt.subPos;

                                float wind =
                                    Utils.Vector2Cross(
                                        sdiIt.subOut,
                                        sdiOth.subOut);

                                BNode mid = new BNode(left, subPt);
                                left.nodes.Add(mid);
                                mid.tangentMode = BNode.TangentMode.Disconnected;
                                mid.UseTanIn = true;
                                mid.UseTanOut = true;

                                if (wind <= 0.0f)
                                {
                                    // A CCW transition will go from the it to the other.
                                    mid.TanIn = sdiOth.subIn;
                                    mid.TanOut = sdiIt.subOut;

                                    mid.prev = cols[ii].nodeB;
                                    // 
                                    // Create a loose end and record it
                                    looseEnds.Add(cols[ii].nodeB.next);
                                    //
                                    cols[ii].nodeB.next = mid;
                                    cols[ii].nodeB.tangentMode = BNode.TangentMode.Disconnected;
                                    cols[ii].nodeB.UseTanOut = (bss.linearB == false);
                                    cols[ii].nodeB.TanOut = sdiOth.prevOut;

                                    BNode redirNext = cols[ii].nodeA.next;
                                    //
                                    // Create a loose end and record it
                                    looseEnds.Add(cols[ii].nodeA);
                                    //
                                    mid.next = redirNext;
                                    redirNext.prev = mid;
                                    redirNext.tangentMode = BNode.TangentMode.Disconnected;
                                    redirNext.UseTanIn = (bss.linearA == false);
                                    redirNext.TanIn = sdiIt.nextIn;
                                }
                                else
                                {
                                    // A CW transition will go from the other to it.
                                    mid.TanIn = sdiIt.subIn;
                                    mid.TanOut = sdiOth.subOut;

                                    mid.prev = cols[ii].nodeA;
                                    //
                                    looseEnds.Add(cols[ii].nodeA.next);
                                    //
                                    cols[ii].nodeA.next = mid;
                                    cols[ii].nodeA.tangentMode = BNode.TangentMode.Disconnected;
                                    cols[ii].nodeA.UseTanOut = true;
                                    cols[ii].nodeA.TanOut = sdiIt.prevOut;

                                    BNode redirNext = cols[ii].nodeB.next;
                                    //
                                    looseEnds.Add(cols[ii].nodeB);
                                    //
                                    mid.next = redirNext;
                                    redirNext.prev = mid;
                                    redirNext.tangentMode = BNode.TangentMode.Disconnected;
                                    redirNext.UseTanOut = true;
                                    redirNext.TanIn = sdiOth.nextIn;
                                }

                                // Since we subdivided the edge, the reference to nodeA or nodeB, and the
                                // intersection parameters will no longer be correct. Modify to correct
                                // as needed.
                                for (int jj = ii + 1; jj < cols.Count; ++jj)
                                {
                                    Utils.BezierSubdivSample bssMod = cols[jj];

                                    // Fixup references to nodeA and lAEst.
                                    if (bssMod.lAEst >= cols[ii].lAEst)
                                    {
                                        bssMod.lAEst = Mathf.InverseLerp(cols[ii].lAEst, 1.0f, bssMod.lAEst);
                                        bssMod.nodeA = mid;
                                    }
                                    else
                                    {
                                        bssMod.lAEst = Mathf.InverseLerp(0.0f, cols[ii].lAEst, bssMod.lAEst);
                                    }

                                    // Fixup references to nodeB and lBEst.
                                    if (bssMod.lBEst >= cols[ii].lBEst)
                                    {
                                        bssMod.lBEst = Mathf.InverseLerp(cols[ii].lBEst, 1.0f, bssMod.lBEst);
                                        bssMod.nodeB = mid;
                                    }
                                    else
                                    {
                                        bssMod.lBEst = Mathf.InverseLerp(0.0f, cols[ii].lBEst, bssMod.lBEst);
                                    }

                                    cols[jj] = bssMod;
                                }
                            }
                        }

                        // Figure out what internal items need to be removed by 
                        // checking which nodes have unmatching connectivity.
                        ClipLooseEnds(looseEnds);

                        // Move everything in from the other loop
                        foreach (BNode bn in rightSegs)
                        {
                            // If it's null, it was clipped
                            if (bn.parent == null)
                                continue;

                            // Everything still remaining gets moved
                            // to the endLoop.
                            if (bn.parent != left)
                                bn.parent.nodes.Remove(bn);

                            bn.parent = left;
                            bn.parent.nodes.Add(bn);
                        }
                    }
                }

                if (Utils.verboseDebug == true)
                {
                    if (right.nodes.Count != 0)
                        Debug.Log("Boolean union didn't end up with an empty right loop as expected");
                }

                if (removeRight == true)
                    RemoveLoop(right, true);
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
                List<BNode> islsA = loopA.GetIslands();
                List<BNode> islsB = loopB.GetIslands();

                List<Utils.BezierSubdivSample> collisions = new List<Utils.BezierSubdivSample>();
                
                foreach (BNode isA in islsA)
                { 
                    BNode.EndpointQuery eqA = isA.GetPathLeftmost();
                    // Only closed loops count
                    if (eqA.result == BNode.EndpointResult.SuccessfulEdge)
                        continue;

                    List<BNode> segsA = new List<BNode>(eqA.Enumerate());
                    foreach (BNode isB in islsB)
                    { 
                        BNode.EndpointQuery eqB = isB.GetPathLeftmost();
                        // Only closed loops count
                        if(eqB.result == BNode.EndpointResult.SuccessfulEdge)
                            continue;

                        List<BNode> segsB = new List<BNode>(eqB.Enumerate());

                        foreach (BNode na in segsA)
                        { 
                            foreach(BNode nb in segsB)
                            {
                                Utils.NodeIntersections(na, nb, 20, Mathf.Epsilon, collisions);
                            }
                        }
                    }
                }

                Utils.BezierSubdivSample.CleanIntersectionList(collisions);
                return collisions;
            }

            public static Dictionary<NodeTPos, BNode.SubdivideInfo> SliceCollisionInfo(List<Utils.BezierSubdivSample> collisions)
            {
                Dictionary<NodeTPos, BNode.SubdivideInfo> ret = 
                    new Dictionary<NodeTPos, BNode.SubdivideInfo>();

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
                            ret.Add( new NodeTPos(node, subs[i]), si);
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
                            float realT = (curT = lm)/(1.0f - lm);

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
                        subSpots.Add(pt2);
                        subSpots.Add(pt3);

                        for (int i = 0; i < subs.Count; ++i)
                        {
                            int idx = 3 + i * 3;
                            BNode.SubdivideInfo si = new BNode.SubdivideInfo();
                            si.subPos = subSpots[idx];
                            si.subIn = subSpots[idx - 1] - si.subPos;
                            si.subOut = subSpots[idx + 1] - si.subPos;

                            si.prevOut = subSpots[idx -3] - subSpots[idx - 2];
                            si.nextIn = subSpots[idx + 3] - subSpots[idx + 2];
                        }
                    }
                }

                return ret;
            }
        }
    }
}