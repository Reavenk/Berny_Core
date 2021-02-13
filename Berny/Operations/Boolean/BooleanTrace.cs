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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PxPre.Berny
{
    /// <summary>
    /// Utility class to perform boolean operations between closed shapes. 
    /// 
    /// This file focuses on algorithms that perform boolean operations by
    /// tracing new shapes.
    /// </summary>
    public static partial class Boolean
    {
        /// <summary>
        /// Gather intersection data commonly used in boolean trace operations.
        /// </summary>
        /// <param name="islA">A node on a closed island used as the left path parameter.</param>
        /// <param name="islB">A node on a closed island used as the right path parameter.</param>
        /// <param name="allNodes">All the nodes found in the loop for islA and islB.</param>
        /// <param name="outList">
        /// A collection of all the collisions found between islA and islB. This also includes self 
        /// intersections (such as collisions between islA and other parts of islA).</param>
        /// <param name="dictCol">A collection of collisions sorted per-node, and ordered by the
        /// point on the beziers they occured.</param>
        /// <remarks>The significance of the left and right paths depends on the operation this
        /// function is being used for.</remarks>
        public static void GatherTraceData(
            BNode islA,
            BNode islB,
            List<BNode> allNodes,
            List<Utils.BezierSubdivSample> outList,
            Dictionary<BNode, List<Utils.BezierSubdivSample>> dictCol)
        {
            foreach (BNode bnit in islA.Travel())
                allNodes.Add(bnit); // Also collect for allNodes

            foreach (BNode bnit in islB.Travel())
                allNodes.Add(bnit); // Also collect for allNodes

            //  SCAN AND LOG ALL COLLISIONS
            //
            //////////////////////////////////////////////////

            for (int i = 0; i < allNodes.Count - 1; ++i)
            {
                BNode bni = allNodes[i];
                // This check isn't as robust as it could be. While rare, a segment can create
                // a loop that causes an intersection with itself. But, these loops are set up
                // assuming loops can never do that.
                for (int j = i + 1; j < allNodes.Count; ++j)
                {
                    BNode bnj = allNodes[j];

                    // Make an output list that checks and cleans for each individual
                    // collision.
                    List<Utils.BezierSubdivSample> ol = new List<Utils.BezierSubdivSample>();
                    Utils.NodeIntersections(bni, bnj, 20, 0.00001f, ol);
                    Utils.BezierSubdivSample.CleanIntersectionList(ol);

                    foreach (Utils.BezierSubdivSample bss in ol)
                    {
                        // This part is trick and isn't as robust as it could be. Neighboring nodes 
                        // are prone to creating false positives for collising at the ends where 
                        // they are attached. We can't be too aggressive on culling these detections
                        // out though because it is possible that there could be tiny loops that
                        // back-track and legitimately collide with their segment neighbors 
                        // *near* the ends. To do this properly we need to bounds check at the
                        // (cubic) curve roots instead of at extreemly low (0.001) and high (0.99)
                        // interpolation points.
                        if (
                            (bss.a.lEst < 0.001f && bss.a.node.prev == bss.b.node) ||
                            (bss.a.lEst > 0.99f && bss.a.node.next == bss.b.node))
                        {
                            continue;
                        }

                        outList.Add(bss);
                    }
                }
            }

            //  ORGANIZE COLLISIONS TO BE USABLE
            //
            //////////////////////////////////////////////////

            // Each node that has a collision has a ordered list of where collisions
            // happened. This makes the collision data FAR less unwieldy and more 
            // usable. Note that because a collision involves two segments colliding
            // (ignoring self collisions) each collision will be added to two nodes;
            // where the list it's added to will be the "a" segment. If it's originally 
            // labeled the "b" segment in the collision data, a and b are swapped.

            foreach (Utils.BezierSubdivSample bss in outList)
            {
                List<Utils.BezierSubdivSample> lstA;
                List<Utils.BezierSubdivSample> lstB;

                // Add a directly.
                if (dictCol.TryGetValue(bss.a.node, out lstA) == false)
                {
                    lstA = new List<Utils.BezierSubdivSample>();
                    dictCol.Add(bss.a.node, lstA);
                }
                lstA.Add(bss);

                // For b, swap b and a before adding so that it's also
                // a. This makes it so we don't need to branch to figure
                // out which NodeSubRgn (a or b) to reference for a given
                // key.
                Utils.BezierSubdivSample bssRec = bss.Reciprocal();
                if (dictCol.TryGetValue(bss.b.node, out lstB) == false)
                {
                    lstB = new List<Utils.BezierSubdivSample>();
                    dictCol.Add(bss.b.node, lstB);
                }
                lstB.Add(bssRec);
            }

            foreach (List<Utils.BezierSubdivSample> lst in dictCol.Values)
                lst.Sort(_LexicalSubdivSort);
        }

        /// <summary>
        /// Sorting function for the entries in the output parameter outList used for 
        /// function GatherTraceData().
        /// </summary>
        /// <param name="x">The left item being sorted.</param>
        /// <param name="y">The right item being sorted.</param>
        /// <returns>The comparison return.</returns>
        public static int _LexicalSubdivSort(Utils.BezierSubdivSample x, Utils.BezierSubdivSample y)
        {
            // We only care about the "a" SubdivSample.
            if (x.a.lEst == y.a.lEst)
                return 0;

            if (x.a.lEst < y.a.lEst)
                return -1;

            return 1;
        }

        /// <summary>
        /// Union boolean operation using a tracing strategy.
        /// </summary>
        /// <param name="islA">A node, on an island, to be unioned.</param>
        /// <param name="islB">A node, on another island, that is to be unioned.</param>
        /// <param name="loopInto">The destination loop, where the newly traced contents
        /// will be parented to.</param>
        /// <param name="onIsle">Output parameter, returns a node on the traced island.</param>
        /// <param name="removeInputs">If true, remove the island nodes of islA and islB from
        /// their parents after the operation is finished. This only happens if there is a 
        /// collision to process.</param>
        /// <returns>True if the parameter islands touch and a traced result was created. Else, false.</returns>
        public static bool TraceUnion(
            BNode islA, 
            BNode islB, 
            BLoop loopInto, 
            out BNode onIsle, 
            bool removeInputs = true)
        {
            // Every node for both islands
            List<BNode> allNodes = new List<BNode>();
            List<Utils.BezierSubdivSample> outList = new List<Utils.BezierSubdivSample>();
            Dictionary<BNode, List<Utils.BezierSubdivSample>> dictCol = new Dictionary<BNode, List<Utils.BezierSubdivSample>>();

            GatherTraceData(islA, islB, allNodes, outList, dictCol);

            if (outList.Count == 0)
            {
                onIsle = null;
                return false;
            }

            //
            //  FIND THE MOST EXTREME POINT
            //
            //////////////////////////////////////////////////
            // Rightmost node. We need to find a point on every segment that's on the most
            // extreme somewhere (i.e., leftmost, topmost, rightmost, bottom-most). That way
            // we know it's not inside the shape union.
            BNode rtmst = islA;
            Vector2 rt = rtmst.Pos;     // Point of rightmost
            float lam = 0.0f;           // Lambda of rightmost

            foreach (BNode bn in allNodes)
            {
                if (bn.GetMaxPoint(ref rt, ref lam, 0) == true)
                    rtmst = bn;
            }

            //  FIND THE ACTUAL STARTING POSITIONS
            //
            //////////////////////////////////////////////////
            // Given the rightmost postion we found earlier, we can't use that directly
            // because that's not a good endpoint for how we trace. A valid starting 
            // point needs to also be a good end point for us to know when to stop without
            // creating awkward edge cases in the tracing code. 
            //
            // Valid positions at the endpoints of a node, or the closest previous collision
            // point on the segment.
            List<Utils.BezierSubdivSample> lstBss;
            if (dictCol.TryGetValue(rtmst, out lstBss) == false)
                lam = 0.0f;
            else if (lam > lstBss[0].a.lEst)
                lam = 0.0f;
            else
            {
                for (int i = lstBss.Count - 1; i >= 0; --i)
                {
                    if (lam <= lstBss[i].a.lEst)
                    {
                        lam = lstBss[i].a.lEst;
                        break;
                    }
                }
            }

            //  PERFORM THE TRACING
            //
            //////////////////////////////////////////////////
            List<BNode> newPath = new List<BNode>();

            BNode.SubdivideInfo sdiFirst = rtmst.GetSubdivideInfo(lam);
            BNode bnPrevNew = new BNode(null, sdiFirst.subPos);
            bnPrevNew.TanIn = sdiFirst.subIn;
            bnPrevNew.UseTanIn = sdiFirst.subIn != Vector2.zero;
            bnPrevNew.TanOut = sdiFirst.subOut;
            bnPrevNew.UseTanOut = sdiFirst.subOut != Vector2.zero;
            newPath.Add(bnPrevNew);

            BNode bnIt = rtmst;
            float itLam = lam;

            BNode.SubdivideInfo sdiL;
            BNode.SubdivideInfo sdiR;

            while (true)
            {
                BNode bnPrev = bnIt;
                if (dictCol.TryGetValue(bnIt, out lstBss) == false)
                {
                    // If there are no collisions in the segment, just
                    // add the whole segment
                    itLam = 0.0f;
                    bnIt = bnIt.next;

                    if (bnIt == rtmst && itLam == lam) // Are we back where we started?
                    {
                        // close the shape and we're done
                        bnPrevNew.TanIn = bnIt.TanIn;
                        bnPrevNew.UseTanIn = true;

                        BNode prevToFirst = newPath[newPath.Count - 1];
                        prevToFirst.TanOut = bnPrev.TanOut;
                        prevToFirst.UseTanOut = true;
                        break;
                    }
                    else
                    {
                        // Full copy. TanOut can be modified later.
                        BNode bnDirAdd = new BNode(null, bnIt.Pos);
                        bnDirAdd.TanOut = bnIt.TanOut;
                        bnDirAdd.TanIn = bnIt.TanIn;
                        bnDirAdd.UseTanIn = bnIt.UseTanIn;
                        bnDirAdd.UseTanOut = bnIt.UseTanOut;
                        newPath.Add(bnDirAdd);

                        BNode prevToFirst = newPath[newPath.Count - 2];
                        prevToFirst.TanOut = bnPrev.TanOut;
                        prevToFirst.UseTanOut = true;
                    }
                    continue;
                }

                // If there are collision points, trace only the segment
                // and swap the segment chain we're tracing with the one
                // we collided with.
                // 
                // Where we've moved, in relationship to bnPrev.
                float itStart = itLam;
                float itEnd = 1.0f;

                bool nextProc = false;
                for (int i = 0; i < lstBss.Count - 1; ++i)
                {
                    if (lstBss[i].a.lEst == itLam && i != lstBss.Count - 1)
                    {
                        itEnd = lstBss[i + 1].a.lEst;

                        // Figure out where we continue after jumping to the next item
                        bnIt = lstBss[i + 1].b.node;
                        itLam = lstBss[i + 1].b.lEst;
                        nextProc = true;
                        break;
                    }
                }

                if (nextProc == false)
                {
                    if (itLam == 0.0f)
                    {
                        // The first collision in the segment
                        itStart = 0.0f;
                        itEnd = lstBss[0].a.lEst;

                        // Swap as we were in the loop directly above
                        bnIt = lstBss[0].b.node;
                        itLam = lstBss[0].b.lEst;

                    }
                    else
                    {
                        // The last collision to the end
                        itStart = lstBss[lstBss.Count - 1].a.lEst;
                        itEnd = 1.0f;

                        // Move on to the next node in the chain.
                        bnIt = bnIt.next;
                        itLam = 0.0f;
                    }
                }

                bnPrev.GetSubdivideInfo(itStart, out sdiL, itEnd, out sdiR);

                if (bnIt == rtmst && itLam == lam) // Are we back where we started?
                {
                    // close the shape and we're done
                    bnPrevNew.TanIn = sdiR.subIn;
                    bnPrevNew.UseTanIn = true;

                    newPath[newPath.Count - 1].TanOut = sdiL.subOut;
                    newPath[newPath.Count - 1].UseTanOut = true;
                    break;
                }
                else
                {
                    // Add additional traced point.
                    BNode bnNew = new BNode(null, sdiR.subPos);
                    bnNew.TanIn = sdiR.subIn;
                    bnNew.UseTanIn = true;

                    newPath[newPath.Count - 1].TanOut = sdiL.subOut;
                    newPath[newPath.Count - 1].UseTanOut = true;

                    newPath.Add(bnNew);
                }
            }


            for (int i = 0; i < newPath.Count; ++i)
            {
                newPath[i].parent = loopInto;

                if (loopInto != null)
                    loopInto.nodes.Add(newPath[i]);

                newPath[i].next = newPath[(i + 1) % newPath.Count];
                newPath[i].prev = newPath[((i - 1) + newPath.Count) % newPath.Count];
            }

            if (newPath.Count > 0)
                onIsle = newPath[0];
            else
                onIsle = null;

            if (removeInputs == true)
            {
                foreach (BNode bn in allNodes)
                {
                    if (bn.parent != null)
                        bn.parent.RemoveNode(bn);
                }
            }

            return true;
        }

        /// <summary>
        /// Intersection boolean operation using a tracing strategy.
        /// </summary>
        /// <param name="islA">A node, on an island, to be intersectioned.</param>
        /// <param name="islB">A node, on another island, that is to be intersectioned.</param>
        /// <param name="loopInto">The destination loop, where newly traced contents
        /// will be parented to.</param>
        /// <param name="onIsle">Output parameter. If the return value is true, it returns a node on 
        /// the newly created traced island.
        /// </param>
        /// <param name="removeInputs">If true, the contents in the islands of islA and islB will be
        /// removed from their parents if a traced path is created.</param>
        /// <returns>True if the parameter islands touch and a traced result was created. Else, false.</returns>
        public static bool TraceIntersection(
            BNode islA, 
            BNode islB, 
            BLoop loopInto, 
            out BNode onIsle, 
            bool removeInputs = true)
        {
            List<BNode> allNodes = new List<BNode>();
            List<Utils.BezierSubdivSample> outList = new List<Utils.BezierSubdivSample>();
            Dictionary<BNode, List<Utils.BezierSubdivSample>> dictCol = new Dictionary<BNode, List<Utils.BezierSubdivSample>>();

            GatherTraceData(islA, islB, allNodes, outList, dictCol);

            onIsle = null;
            if (outList.Count == 0)
            {
                return false;
            }

            while (outList.Count > 0)
            {
                BNode startnode = null;
                Vector2 rt = Vector2.zero;  // Point of rightmost
                float lam = 0.0f;           // Lambda of rightmost

                BNode.SubdivideInfo sdiStartA = outList[0].a.node.GetSubdivideInfo(outList[0].a.lEst);
                BNode.SubdivideInfo sdiStartB = outList[0].b.node.GetSubdivideInfo(outList[0].b.lEst);
                float wind = Utils.Vector2Cross(sdiStartA.windTangent, sdiStartB.windTangent);

                List<BNode> newPath = new List<BNode>();
                BNode firstNew = null;

                if (wind > 0.0f)
                {
                    startnode = outList[0].a.node;
                    lam = outList[0].a.lEst;

                    firstNew = new BNode(null, sdiStartA.subPos);
                    firstNew.TanIn = sdiStartA.subIn;
                    firstNew.UseTanIn = sdiStartA.subIn != Vector2.zero;
                    firstNew.TanOut = sdiStartA.subOut;
                    firstNew.UseTanOut = sdiStartA.subOut != Vector2.zero;
                }
                else
                {
                    startnode = outList[0].b.node;
                    lam = outList[0].b.lEst;

                    firstNew = new BNode(null, sdiStartB.subPos);
                    firstNew.TanIn = sdiStartB.subIn;
                    firstNew.UseTanIn = sdiStartB.subIn != Vector2.zero;
                    firstNew.TanOut = sdiStartB.subOut;
                    firstNew.UseTanOut = sdiStartB.subOut != Vector2.zero;
                }
                outList.RemoveAt(0);
                newPath.Add(firstNew);


                BNode bnIt = startnode;
                float itLam = lam;

                BNode.SubdivideInfo sdiL;
                BNode.SubdivideInfo sdiR;

                while (true)
                {
                    BNode bnPrev = bnIt;
                    List<Utils.BezierSubdivSample> lstBss;
                    if (dictCol.TryGetValue(bnIt, out lstBss) == false)
                    {
                        // If there are no collisions in the segment, just
                        // add the whole segment
                        itLam = 0.0f;
                        bnIt = bnIt.next;

                        if (bnIt == startnode && itLam == lam) // Are we back where we started?
                        {
                            // close the shape and we're done
                            firstNew.TanIn = bnIt.TanIn;
                            firstNew.UseTanIn = true;

                            BNode prevToFirst = newPath[newPath.Count - 1];
                            prevToFirst.TanOut = bnPrev.TanOut;
                            prevToFirst.UseTanOut = true;
                            break;
                        }
                        else
                        {
                            // Full copy. TanOut can be modified later.
                            BNode bnDirAdd = new BNode(null, bnIt.Pos);
                            bnDirAdd.TanOut = bnIt.TanOut;
                            bnDirAdd.TanIn = bnIt.TanIn;
                            bnDirAdd.UseTanIn = bnIt.UseTanIn;
                            bnDirAdd.UseTanOut = bnIt.UseTanOut;
                            newPath.Add(bnDirAdd);

                            BNode prevToFirst = newPath[newPath.Count - 2];
                            prevToFirst.TanOut = bnPrev.TanOut;
                            prevToFirst.UseTanOut = true;
                        }
                        continue;
                    }

                    // If there are collision points, trace only the segment
                    // and swap the segment chain we're tracing with the one
                    // we collided with.
                    // 
                    // Where we've moved, in relationship to bnPrev.
                    float itStart = itLam;
                    float itEnd = 1.0f;

                    bool nextProc = false;
                    for (int i = 0; i < lstBss.Count - 1; ++i)
                    {
                        if (lstBss[i].a.lEst == itLam && i != lstBss.Count - 1)
                        {
                            itEnd = lstBss[i + 1].a.lEst;

                            // Figure out where we continue after jumping to the next item
                            bnIt = lstBss[i + 1].b.node;
                            itLam = lstBss[i + 1].b.lEst;
                            nextProc = true;

                            if (outList.Remove(lstBss[i]) == false)
                                outList.Remove(lstBss[i].Reciprocal());

                            break;
                        }
                    }

                    if (nextProc == false)
                    {
                        if (itLam == 0.0f)
                        {
                            // The first collision in the segment
                            itStart = 0.0f;
                            itEnd = lstBss[0].a.lEst;

                            // Swap as we were in the loop directly above
                            bnIt = lstBss[0].b.node;
                            itLam = lstBss[0].b.lEst;

                        }
                        else
                        {
                            // The last collision to the end
                            itStart = lstBss[lstBss.Count - 1].a.lEst;
                            itEnd = 1.0f;

                            if (outList.Remove(lstBss[lstBss.Count - 1]) == false)
                                outList.Remove(lstBss[lstBss.Count - 1].Reciprocal());

                            // Move on to the next node in the chain.
                            bnIt = bnIt.next;
                            itLam = 0.0f;
                        }
                    }

                    bnPrev.GetSubdivideInfo(itStart, out sdiL, itEnd, out sdiR);

                    if (bnIt == startnode && itLam == lam) // Are we back where we started?
                    {
                        // close the shape and we're done
                        firstNew.TanIn = sdiR.subIn;
                        firstNew.UseTanIn = true;

                        newPath[newPath.Count - 1].TanOut = sdiL.subOut;
                        newPath[newPath.Count - 1].UseTanOut = true;
                        break;
                    }
                    else
                    {
                        // Add additional traced point.
                        BNode bnNew = new BNode(null, sdiR.subPos);
                        bnNew.TanIn = sdiR.subIn;
                        bnNew.UseTanIn = true;

                        newPath[newPath.Count - 1].TanOut = sdiL.subOut;
                        newPath[newPath.Count - 1].UseTanOut = true;

                        newPath.Add(bnNew);
                    }
                }

                for (int i = 0; i < newPath.Count; ++i)
                {
                    newPath[i].parent = loopInto;

                    if (loopInto != null)
                        loopInto.nodes.Add(newPath[i]);

                    newPath[i].next = newPath[(i + 1) % newPath.Count];
                    newPath[i].prev = newPath[((i - 1) + newPath.Count) % newPath.Count];
                }

                if (onIsle == null && newPath.Count > 0)
                    onIsle = newPath[0];
            }

            if (removeInputs == true)
            {
                foreach (BNode bn in allNodes)
                {
                    if (bn.parent != null)
                        bn.parent.RemoveNode(bn);
                }
            }

            return true;
        }

        /// <summary>
        /// Difference boolean operation using a tracing strategy.
        /// </summary>
        /// <param name="islA">A node, on an island, to be differenced (as the left side).</param>
        /// <param name="islB">A node, on another island, that is to be intersectioned.</param>
        /// <param name="loopInto">The destination loop, where newly traced contents 
        /// will be parented to.</param>
        /// <param name="onIsle">Output parameter. If the return value is true, it returns a node on 
        /// the newly created traced island.</param>
        /// <param name="removeInputs">If true, the contents in the islands of islA and islB will be
        /// removed from their parents if a traced path is created.</param>
        /// <returns>True if the parameter islands touch and a traced result was created. Else, false.</returns>
        public static bool TraceDifference(
            BNode islA, 
            BNode islB, 
            BLoop loopInto, 
            out BNode onIsle, 
            bool removeInputs = true)
        {
            List<BNode> allNodes = new List<BNode>();
            List<Utils.BezierSubdivSample> outList = new List<Utils.BezierSubdivSample>();
            Dictionary<BNode, List<Utils.BezierSubdivSample>> dictCol = new Dictionary<BNode, List<Utils.BezierSubdivSample>>();

            HashSet<BNode> islANodes = new HashSet<BNode>(islA.Travel());

            GatherTraceData(islA, islB, allNodes, outList, dictCol);

            onIsle = null;
            if (outList.Count == 0)
            {
                return false;
            }

            while (outList.Count > 0)
            {
                BNode startnode = null;
                Vector2 rt = Vector2.zero;  // Point of rightmost
                float lam = 0.0f;           // Lambda of rightmost

                BNode.SubdivideInfo sdiStartA = outList[0].a.node.GetSubdivideInfo(outList[0].a.lEst);
                BNode.SubdivideInfo sdiStartB = outList[0].b.node.GetSubdivideInfo(outList[0].b.lEst);
                float wind;
                if (islANodes.Contains(outList[0].a.node) == true)
                    wind = Utils.Vector2Cross(sdiStartA.windTangent, sdiStartB.windTangent);
                else
                    wind = Utils.Vector2Cross(sdiStartB.windTangent, sdiStartA.windTangent);

                List<BNode> newPath = new List<BNode>();
                BNode firstNew = null;

                if (wind > 0.0f)
                {
                    startnode = outList[0].a.node;
                    lam = outList[0].a.lEst;

                    firstNew = new BNode(null, sdiStartA.subPos);
                    firstNew.TanIn = sdiStartA.subIn;
                    firstNew.UseTanIn = sdiStartA.subIn != Vector2.zero;
                    firstNew.TanOut = sdiStartA.subOut;
                    firstNew.UseTanOut = sdiStartA.subOut != Vector2.zero;
                }
                else
                {
                    startnode = outList[0].b.node;
                    lam = outList[0].b.lEst;

                    firstNew = new BNode(null, sdiStartB.subPos);
                    firstNew.TanIn = sdiStartB.subIn;
                    firstNew.UseTanIn = sdiStartB.subIn != Vector2.zero;
                    firstNew.TanOut = sdiStartB.subOut;
                    firstNew.UseTanOut = sdiStartB.subOut != Vector2.zero;
                }
                outList.RemoveAt(0);
                newPath.Add(firstNew);


                BNode bnIt = startnode;
                float itLam = lam;

                BNode.SubdivideInfo sdiL;
                BNode.SubdivideInfo sdiR;

                while (true)
                {
                    BNode bnPrev = bnIt;
                    List<Utils.BezierSubdivSample> lstBss;
                    if (dictCol.TryGetValue(bnIt, out lstBss) == false)
                    {
                        // If there are no collisions in the segment, just
                        // add the whole segment
                        itLam = 0.0f;
                        bnIt = bnIt.next;

                        if (bnIt == startnode && itLam == lam) // Are we back where we started?
                        {
                            // close the shape and we're done
                            firstNew.TanIn = bnIt.TanIn;
                            firstNew.UseTanIn = true;

                            BNode prevToFirst = newPath[newPath.Count - 1];
                            prevToFirst.TanOut = bnPrev.TanOut;
                            prevToFirst.UseTanOut = true;
                            break;
                        }
                        else
                        {
                            // Full copy. TanOut can be modified later.
                            BNode bnDirAdd = new BNode(null, bnIt.Pos);
                            bnDirAdd.TanOut = bnIt.TanOut;
                            bnDirAdd.TanIn = bnIt.TanIn;
                            bnDirAdd.UseTanIn = bnIt.UseTanIn;
                            bnDirAdd.UseTanOut = bnIt.UseTanOut;
                            newPath.Add(bnDirAdd);

                            BNode prevToFirst = newPath[newPath.Count - 2];
                            prevToFirst.TanOut = bnPrev.TanOut;
                            prevToFirst.UseTanOut = true;
                        }
                        continue;
                    }

                    // If there are collision points, trace only the segment
                    // and swap the segment chain we're tracing with the one
                    // we collided with.
                    // 
                    // Where we've moved, in relationship to bnPrev.
                    float itStart = itLam;
                    float itEnd = 1.0f;

                    bool nextProc = false;
                    for (int i = 0; i < lstBss.Count - 1; ++i)
                    {
                        if (lstBss[i].a.lEst == itLam && i != lstBss.Count - 1)
                        {
                            itEnd = lstBss[i + 1].a.lEst;

                            // Figure out where we continue after jumping to the next item
                            bnIt = lstBss[i + 1].b.node;
                            itLam = lstBss[i + 1].b.lEst;
                            nextProc = true;

                            if (outList.Remove(lstBss[i]) == false)
                                outList.Remove(lstBss[i].Reciprocal());

                            break;
                        }
                    }

                    if (nextProc == false)
                    {
                        if (itLam == 0.0f)
                        {
                            // The first collision in the segment
                            itStart = 0.0f;
                            itEnd = lstBss[0].a.lEst;

                            // Swap as we were in the loop directly above
                            bnIt = lstBss[0].b.node;
                            itLam = lstBss[0].b.lEst;

                        }
                        else
                        {
                            // The last collision to the end
                            itStart = lstBss[lstBss.Count - 1].a.lEst;
                            itEnd = 1.0f;

                            if (outList.Remove(lstBss[lstBss.Count - 1]) == false)
                                outList.Remove(lstBss[lstBss.Count - 1].Reciprocal());

                            // Move on to the next node in the chain.
                            bnIt = bnIt.next;
                            itLam = 0.0f;
                        }
                    }

                    bnPrev.GetSubdivideInfo(itStart, out sdiL, itEnd, out sdiR);

                    if (bnIt == startnode && itLam == lam) // Are we back where we started?
                    {
                        // close the shape and we're done
                        firstNew.TanIn = sdiR.subIn;
                        firstNew.UseTanIn = true;

                        newPath[newPath.Count - 1].TanOut = sdiL.subOut;
                        newPath[newPath.Count - 1].UseTanOut = true;
                        break;
                    }
                    else
                    {
                        // Add additional traced point.
                        BNode bnNew = new BNode(null, sdiR.subPos);
                        bnNew.TanIn = sdiR.subIn;
                        bnNew.UseTanIn = true;

                        newPath[newPath.Count - 1].TanOut = sdiL.subOut;
                        newPath[newPath.Count - 1].UseTanOut = true;

                        newPath.Add(bnNew);
                    }
                }

                for (int i = 0; i < newPath.Count; ++i)
                {
                    newPath[i].parent = loopInto;

                    if (loopInto != null)
                        loopInto.nodes.Add(newPath[i]);

                    newPath[i].next = newPath[(i + 1) % newPath.Count];
                    newPath[i].prev = newPath[((i - 1) + newPath.Count) % newPath.Count];
                }

                if (onIsle == null && newPath.Count > 0)
                    onIsle = newPath[0];
            }

            if (removeInputs == true)
            {
                foreach (BNode bn in allNodes)
                {
                    if (bn.parent != null)
                        bn.parent.RemoveNode(bn);
                }
            }

            return true;
        }
    }
}
