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

namespace PxPre
{
    namespace Berny
    {
        public static partial class Boolean
        {
            /// <summary>
            /// Utility class to manage the SplitInfo of a collision session.
            /// </summary>
            /// <remarks>This is used to subdivide a node path where needed to perform
            /// a boolean operation. The starting node path before these subdivisions occured
            /// are referred to as "original" paths.</remarks>
            public class SplitCollection
            {
                /// <summary>
                /// The collision and dicing information for each node.
                /// </summary>
                public Dictionary<BNode, SplitInfo> splits = new Dictionary<BNode, SplitInfo>();

                /// <summary>
                /// Constructor.
                /// </summary>
                /// <param name="dstLoop">The loop to add nodes from requested insertions.</param>
                /// <param name="delCollisions">The segment intersections detected between two islands.</param>
                /// <param name="createdSubdivs">A dictionary that gets filled in with reorganized data
                /// from delCollisions.</param>
                public SplitCollection(
                    BLoop dstLoop,
                    List<Utils.BezierSubdivSample> delCollisions,
                    Dictionary<Utils.NodeTPos, BNode> createdSubdivs)
                {
                    this.SetupFromCollisionData(dstLoop, delCollisions, createdSubdivs);
                }

                /// <summary>
                /// Constructor.
                /// </summary>
                public SplitCollection()
                { }

                /// <summary>
                /// Get the split (dicing) information for a specified original node.
                /// </summary>
                /// <param name="bn"></param>
                /// <returns></returns>
                public SplitInfo GetSplitInfo(BNode bn)
                {
                    SplitInfo ret;
                    if (this.splits.TryGetValue(bn, out ret) == false)
                    {
                        ret = new SplitInfo(bn);
                        this.splits.Add(bn, ret);
                    }
                    return ret;
                }

                /// <summary>
                /// Get the previous dice segment to a specified segment.
                /// </summary>
                /// <param name="ntp">The segment to get the previous neighbor in respect to.</param>
                /// <returns>The previous diced segment neighbor in respect to ntp.</returns>
                public BNode GetPreviousTo(Utils.NodeTPos ntp)
                {
                    return this.GetPreviousTo(ntp.node, ntp.t);
                }

                /// <summary>
                /// Get the previous dice segment to an original node and location.
                /// </summary>
                /// <param name="node">The original node.</param>
                /// <param name="t">The location to query.</param>
                /// <returns>The diced node previous to the specified original node and location in it.</returns>
                public BNode GetPreviousTo(BNode node, float t)
                {
                    SplitInfo si;
                    if (this.splits.TryGetValue(node, out si) == true)
                    {
                        for (int i = 0; i < this.splits.Count; ++i)
                        {
                            if (t > si.splits[i].t)
                                continue;

                            if (t == si.splits[i].t)
                            {
                                // exit this part to visit the previous node.
                                if (i == 0)
                                    break;

                                return si.splits[i - 1].node;
                            }

                            return si.splits[i].node;
                        }

                        return si.splits[si.splits.Count - 1].node;
                    }

                    // Get the last of the previous

                    // If we don't have a record of it, it hasn't been split, so 
                    // we just send the previous node.
                    if (this.splits.TryGetValue(si.origPrev, out si) == false)
                        return si.origPrev;

                    // If it has been split, we want the very last one.
                    return si.splits[si.splits.Count - 1].node;
                }

                /// <summary>
                /// Get the dice segment after a specified segment.
                /// </summary>
                /// <param name="ntp">The segment to get the neighbor after.</param>
                /// <returns>The diced node after the specified original node and location in it.</returns>
                public BNode GetNextTo(Utils.NodeTPos ntp)
                {
                    return this.GetNextTo(ntp.node, ntp.t);
                }

                /// <summary>
                /// Get the dice segment after the segment of a specified original node and location in it.
                /// </summary>
                /// <param name="node">The original node</param>
                /// <param name="t">The location to query.</param>
                /// <returns>The diced node after the specified original node and location in it.</returns>
                public BNode GetNextTo(BNode node, float t)
                {
                    SplitInfo si;
                    if (this.splits.TryGetValue(node, out si) == true)
                    {
                        for (int i = 0; i < si.splits.Count; ++i)
                        {
                            if (si.splits[i].t < t)
                                continue;

                            // If the last item, we're returning the
                            // next item.
                            if (i == si.splits.Count - 1)
                                break;

                            // If we found what's past up, return 1 past that.
                            return si.splits[i + 1].node;
                        }
                    }

                    // The first entry of any split will always be the original node
                    return si.origNext;
                }

                /// <summary>
                /// Initialize data for the SplitCollection and its use.
                /// </summary>
                /// <param name="dstLoop">The loop to create new diced nodes in.</param>
                /// <param name="delCollisions">The segment intersections detected between two islands.</param>
                /// <param name="createdSubdivs">A dictionary that gets filled in with reorganized data
                /// from delCollisions.</param>
                public void SetupFromCollisionData(
                    BLoop dstLoop,
                    List<Utils.BezierSubdivSample> delCollisions,
                    Dictionary<Utils.NodeTPos, BNode> createdSubdivs)
                {
                    foreach (Utils.BezierSubdivSample bss in delCollisions)
                    {
                        Vector2 pos = bss.a.node.CalculatetPoint(bss.a.lEst);
                        BNode newSubNode = new BNode(dstLoop, pos);
                        dstLoop.nodes.Add(newSubNode);

                        createdSubdivs.Add(bss.GetTPosA(), newSubNode);
                        createdSubdivs.Add(bss.GetTPosB(), newSubNode);

                        SplitInfo sia = this.GetSplitInfo(bss.a.node);
                        sia.AddEntry(bss.a.lEst, newSubNode);

                        SplitInfo sib = this.GetSplitInfo(bss.b.node);
                        sib.AddEntry(bss.b.lEst, newSubNode);
                    }
                }
            }
        }
    }
}
