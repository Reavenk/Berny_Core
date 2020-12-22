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
        public enum IslandTypeRequest
        { 
            Any,
            Open,
            Closed
        }

        /// <summary>
        /// Cached values for how much a node's part will move (per-unit)
        /// during inflation.
        /// </summary>
        public struct InflationCache
        { 
            /// <summary>
            /// Influence on the BNode's point.
            /// </summary>
            public Vector2 selfInf;

            /// <summary>
            /// Influence on the BNode's p1 tangent.
            /// This is also known as the previous' output tangent.
            /// </summary>
            public Vector2 inInf;

            /// <summary>
            /// Influence on the BNode's p2 tangent.
            /// This is also known as the next's input tangent.
            /// </summary>
            public Vector2 outInf;
        }

        /// <summary>
        /// Represents a bezier path inside a shape. 
        /// 
        /// This class focuses explicitly on defining the path geometry and leaves
        /// the rest of the visuals to the parent shape.
        /// </summary>
        public class BLoop
        {
            /// <summary>
            /// The parent shape of the loop. If the loop belongs to a shape, the shape should
            /// also reference this loop in its BShape.loop variable.
            /// </summary>
            public BShape shape;

            /// <summary>
            /// The nodes contained in the loop. If the loop
            /// </summary>
            public List<BNode> nodes = new List<BNode>();

            /// <summary>
            /// True if the loop has been changed since the last time the object was prepared
            /// for presentation.
            /// </summary>
            public bool dirty = true;

#if DEVELOPMENT_BUILD || UNITY_EDITOR
            /// <summary>
            /// Debug ID. Each of this object created will have a unique ID that will be assigned the same way
            /// if each app session runs deterministically the same. Used for identifying objects when
            /// debugging.
            /// </summary>
            public int debugCounter;
#endif

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="shape">The parent shape. If not null, it will automatically add the created loop
            /// to the shape.
            /// </param>
            /// <param name="initialInfo">Information on initial nodes to create.</param>
            public BLoop(BShape shape, params BNode.BezierInfo [] initialInfo)
            { 
                if(shape != null)
                    shape.AddLoop(this);

                foreach(BNode.BezierInfo bi in initialInfo)
                {
                    BNode bn = new BNode(this, bi);
                    bn.FlagDirty();
                    nodes.Add(bn);
                }

                if(initialInfo.Length == 0)
                { } // Do nothing
                else if(initialInfo.Length == 1)
                { } // Also do nothing
                if(initialInfo.Length == 2)
                {
                    this.nodes[0].next = this.nodes[1];
                    this.nodes[1].prev = this.nodes[0];
                }
                else
                { 
                    int lastIdx = this.nodes.Count - 1;

                    for(int i = 0; i < lastIdx; ++i)
                    { 
                        BNode bnprv = this.nodes[i];
                        BNode bnnxt = this.nodes[i + 1];

                        bnprv.next = bnnxt;
                        bnnxt.prev = bnprv;
                    }

                    // Close the shape.
                    if(this.nodes.Count > 0)
                    {
                        BNode bnfirst = this.nodes[0];
                        BNode bnlast = this.nodes[lastIdx];
                        bnfirst.prev = bnlast;
                        bnlast.next = bnfirst;
                    }
                }

#if DEVELOPMENT_BUILD || UNITY_EDITOR
                this.debugCounter = Utils.RegisterCounter();
#endif
            }

            public BLoop(BShape shape, bool closed, params Vector2 [] linePoints)
            {
                // Arguably, we could have made the style of creation between this and the 
                // "params BNode.BezierInfo []" constructor the same style of either
                // storing in an array and connecting afterwards (the other) or making 
                // connections as we go and storing the last (this one).
                if (shape != null)
                    shape.AddLoop(this);

                if(linePoints.Length == 0)
                { }
                else if(linePoints.Length == 1)
                { 
                    BNode bn = new BNode(this, linePoints[0]);
                    this.nodes.Add(bn);
                }
                else if(linePoints.Length == 2)
                {
                    BNode bnA = new BNode(this, linePoints[0]);
                    this.nodes.Add(bnA);

                    BNode bnB = new BNode(this, linePoints[1]);
                    this.nodes.Add(bnB);

                    bnA.next = bnB;
                    bnB.prev = bnA;
                }
                else
                { 
                    BNode first = null;
                    BNode prev = null;

                    foreach(Vector2 v2 in linePoints)
                    { 
                        BNode bn = new BNode(this, v2);
                        this.nodes.Add(bn);

                        if(prev == null)
                        { 
                            first = bn;
                        }
                        else
                        { 
                            prev.next = bn;
                            bn.prev = prev;
                        }
                        prev = bn;
                    }

                    if(closed == true)
                    {
                        // At this point, prev will point to the last item.
                        first.prev = prev;
                        prev.next = first;
                    }
                }

#if DEVELOPMENT_BUILD || UNITY_EDITOR
                this.debugCounter = Utils.RegisterCounter();
#endif
            }

            /// <summary>
            /// Clear out all flattened samples in the containing nodes.
            /// </summary>
            public void DissassembleSampleLoop()
            { 
                foreach(BNode bn in nodes)
                    bn.sample = null;
            }

            /// <summary>
            /// Clear out all the nodes in the loop.
            /// </summary>
            public void Clear()
            { 
                this.DissassembleSampleLoop();

                this.nodes.Clear();
            }

            /// <summary>
            /// Get the number of nodes in the loop.
            /// </summary>
            /// <returns>The number of nodes in the loop.</returns>
            public int NodeCount()
            { 
                return this.nodes.Count;
            }

            /// <summary>
            /// If true, has at least one node. Else false.
            /// </summary>
            /// <returns>If true, the loop has at least one node.</returns>
            public bool HasNodes()
            {
                return this.nodes.Count > 0;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="bn"></param>
            /// <returns></returns>
            public bool RemoveNode(BNode bn)
            { 
                int idx = this.nodes.IndexOf(bn);
                if(idx == -1)
                    return false;

                this.nodes.RemoveAt(idx);

                if(bn.prev != null && bn.next != null)
                { 
                    // A full stitch job is needed here.
                    bn.prev.next = bn.next;
                    bn.next.prev = bn.prev;
                    bn.prev.FlagDirty();
                    bn.next.FlagDirty();

                    // Cut off the arc from the counter to us.
                    if(bn.prev.sample != null)
                        bn.prev.sample.next = null;

                    // Cut off the arc from us to our clockwise.
                    if(bn.prev.sample != null)
                        bn.prev.sample.prev = null;

                }
                else if(bn.prev != null)
                { 
                    // If the counterclock item exists, not only do they need to
                    // forget about us, we need to do some trimming for them since
                    // we're their clockwise.
                    bn.prev.next = null;
                    bn.prev.FlagDirty();

                    if(bn.prev.sample != null)
                    {
                        if(bn.prev.sample.next != null)
                            bn.prev.sample.next = null;

                        bn.prev.sample = null;
                    }
                }
                else if(bn.next != null)
                { 
                    // If the clockwise item exists, just have them forget about us.
                    bn.next.prev = null;
                }
                this.FlagDirty();
                return true;
            }

            /// <summary>
            /// Flag the loop as dirty, meaning it has been changed since the last time
            /// it was prepared for presentation.
            /// 
            /// This will also set parent objects as dirty (i.e., the shape, layer, and document).
            /// </summary>
            public void FlagDirty()
            {
                this.dirty = true;

                if(this.shape != null)
                    this.shape.FlagDirty();
            }

            /// <summary>
            /// Check if the loop's dirty flag is set.
            /// </summary>
            /// <returns>If true, the loop is dirty.</returns>
            public bool IsDirty()
            { 
                return this.dirty;
            }

            /// <summary>
            /// Prepare the loop for rendering if it is dirty.
            /// Also clears the dirty flag afterwards.
            /// </summary>
            public void FlushDirty()
            { 
                foreach(BNode bn in this.nodes)
                    bn.HandleDirty();

                this.dirty = false;
            }

            /// <summary>
            /// Counts how many islands are opened and closed.
            /// </summary>
            /// <param name="open">The number of open islands found in the loop object.</param>
            /// <param name="closed">The number of closed islands in the loop object.</param>
            /// <returns></returns>
            public void CountOpenAndClosed(out int open, out int closed)
            { 
                open = 0;
                closed = 0;

                HashSet<BNode> nodesLeft = new HashSet<BNode>(this.nodes);

                while(nodesLeft.Count > 0)
                { 
                    BNode n = Utils.GetFirstInHash<BNode>(nodesLeft);
                    BNode.EndpointQuery eq = n.GetPathLeftmost();

                    if(eq.result == BNode.EndpointResult.Cyclical)
                        ++closed;
                    else
                        ++open;

                    BNode it = eq.node;
                    while(it != null)
                    { 
                        nodesLeft.Remove(it);

                        it = it.next;
                        if(it == eq.node)
                            break;
                    }
                }
            }

            /// <summary>
            /// Returns a node from each island found.
            /// </summary>
            /// <returns>A node from each island found in the loop.</returns>
            public List<BNode> GetIslands(IslandTypeRequest req = IslandTypeRequest.Any)
            { 
                List<BNode> ret = new List<BNode>();
                HashSet<BNode> nodesLeft = new HashSet<BNode>(this.nodes);

                while(nodesLeft.Count > 0)
                {
                    BNode n = Utils.GetFirstInHash<BNode>(nodesLeft);
                    BNode.EndpointQuery eq = n.GetPathLeftmost();

                    switch(req)
                    { 
                        case IslandTypeRequest.Any:
                            ret.Add(eq.node);
                            break;

                        case IslandTypeRequest.Closed:
                            if(eq.result == BNode.EndpointResult.Cyclical)
                                ret.Add(eq.node);
                            break;

                        case IslandTypeRequest.Open:
                            if(eq.result == BNode.EndpointResult.SuccessfulEdge)
                                ret.Add(eq.node);
                            break;
                    }

                    BNode it = eq.node;
                    while (it != null)
                    {
                        nodesLeft.Remove(it);

                        it = it.next;
                        if (it == eq.node)
                            break;
                    }
                }

                return ret;
            }

            /// <summary>
            /// Get a list of all the islands and the type of loop they are, whether they're
            /// opened or closed.
            /// </summary>
            /// <returns>A list of endpoint queries containing a reference to all the islands.</returns>
            public List<BNode.EndpointQuery> GetIslandsDescriptive()
            { 
                List<BNode.EndpointQuery> ret = new List<BNode.EndpointQuery>();
                HashSet<BNode> nodesLeft = new HashSet<BNode>(this.nodes);

                while (nodesLeft.Count > 0)
                {
                    BNode n = Utils.GetFirstInHash<BNode>(nodesLeft);
                    BNode.EndpointQuery eq = n.GetPathLeftmost();

                    ret.Add(eq);

                    BNode it = eq.node;
                    while (it != null)
                    {
                        nodesLeft.Remove(it);

                        it = it.next;
                        if (it == eq.node)
                            break;
                    }
                }

                return ret;
            }

            ///// <summary>
            ///// Combine multiple loops into a single loop.
            ///// </summary>
            ///// <param name="loops"></param>
            ///// <returns></returns>
            //public static BLoop Union(params BLoop [] loops)
            //{ 
            //    // TODO:
            //    return null;
            //}
            //
            ///// <summary>
            ///// Subtract one loop from another.
            ///// </summary>
            ///// <param name="left"></param>
            ///// <param name="right"></param>
            ///// <returns></returns>
            //public static BLoop Difference(BLoop left, BLoop right)
            //{
            //    // TODO: 
            //    return null;
            //}
            //
            ///// <summary>
            ///// 
            ///// </summary>
            ///// <param name="loops"></param>
            ///// <returns></returns>
            //public static BLoop Intersect(params BLoop [] loops)
            //{ 
            //    // TODO:
            //    return null;
            //}
            //
            ///// <summary>
            ///// 
            ///// </summary>
            ///// <param name="loops"></param>
            ///// <returns></returns>
            //public static BLoop Exclusion(params BLoop [] loops)
            //{
            //    // TODO:
            //    return null;
            //}
    
            /// <summary>
            /// Test the properties contained in the datastructure to make sure
            /// there are no errors.
            /// </summary>
            public void TestValidity()
            { 
                foreach(BNode node in this.nodes)
                { 
                    if(node.parent != this)
                        Debug.Log("Validity Error: Mismatch between a loop's node and that node's reference to the parent loop.");

                    if(node.prev != null)
                    { 
                        if(node.prev.next != node)
                            Debug.Log("Validity Error: Node's previous doesn't reference node as next.");
                    }

                    if(node.next != null)
                    { 
                        if(node.next.prev != node)
                            Debug.Log("Validity Error: Node's next doesn't reference node as previous.");
                    }
                }
            }

            /// <summary>
            /// Approximate the arclength by summing the length of the flattened curve segments.
            /// </summary>
            /// <returns>The approximated arclength of all curve segments in the loop.</returns>
            public float CalculateSampleLens()
            { 
                float ret = 0.0f;

                foreach(BNode bn in this.nodes)
                    ret += bn.CalculateSampleLens();
        
                return ret;
            }

            /// <summary>
            /// Approximate the arclength by flattening the node and accumulated the line lengths.
            /// 
            /// This generates lines for measurement with an arbitrary subdivision amount and does
            /// not in any way touch the node segments.
            /// </summary>
            /// <param name="subdivs">The subdivision amount.</param>
            /// <returns>The approximated arclength of all curve segments in the loop.</returns>
            public float CalculateArclen(int subdivs = 30)
            {
                float ret = 0.0f;

                if(subdivs < 2)
                    subdivs = 2;

                foreach(BNode bn in this.nodes)
                    ret += bn.CalculateArcLen(subdivs);

                return ret;
            }

            /// <summary>
            /// Count how many separate islands are in the loop. 
            /// 
            /// An island is a chain of nodes that are unconnected to another chain.
            /// </summary>
            /// <returns>The number of islands found.</returns>
            public int CalculateIslands()
            {
                HashSet<BNode> nl = new HashSet<BNode>(this.nodes);
                int ret = 0;

                // If anything's left, then there's an island
                while(nl.Count > 0)
                { 
                    // Count the island
                    ++ret;

                    // Get any point
                    BNode bn = Utils.GetFirstInHash(nl);
                    // Find a starting point to remove the island from our record
                    BNode.EndpointQuery eq = bn.GetPathLeftmost();

                    // Remove the island from our record
                    nl.Remove(eq.node);
                    for(BNode it = eq.node.next; it != null && it != eq.node; it = it.next)
                        nl.Remove(it);
                }

                return ret;
            }

            /// <summary>
            /// Subdivide a child node imto multiple parts.
            /// </summary>
            /// <remarks>Not reliable, to be replaced later with De Casteljau's algorithm.</remarks>
            /// <param name="targ"></param>
            /// <param name="lambda"></param>
            /// <returns></returns>
            public BNode Subdivide(BNode targ, float lambda = 0.5f)
            {
                if (targ.parent != this)
                    return null;

                BNode.PathBridge pb = targ.GetPathBridgeInfo();

                if (pb.pathType == BNode.PathType.None)
                    return null;

                BNode bn = null;
                if (pb.pathType == BNode.PathType.Line)
                {
                    bn = 
                        new BNode(
                            this, 
                            Vector2.Lerp(targ.Pos,  targ.next.Pos, lambda));

                    bn.UseTanIn = false;
                    bn.UseTanOut = false;
                }
                else if(pb.pathType == BNode.PathType.BezierCurve)
                {
                    BNode.SubdivideInfo sdi = targ.GetSubdivideInfo(lambda);
                    bn = new BNode(
                            this,
                            sdi.subPos,
                            sdi.subIn,
                            sdi.subOut);

                    targ.next.SetTangentDisconnected();
                    targ.SetTangentDisconnected();

                    bn.UseTanIn = true;
                    bn.UseTanOut = true;
                    targ.TanOut = sdi.prevOut;              
                    targ.next.TanIn = sdi.nextIn;

                }

                if(bn != null)
                {
                    bn.next = targ.next;
                    bn.prev = targ;
                    bn.next.prev = bn;
                    bn.prev.next = bn;

                    //
                    this.nodes.Add(bn);

                    bn.FlagDirty();
                    targ.FlagDirty();

                    return bn;
                }
                return null;
            }

            /// <summary>
            /// Connect multiple child nodes together. If input nodes are not both a part of the loop,
            /// the request is ignored.
            /// 
            /// For a connection to be successful, it must be two edge nodes.
            /// </summary>
            /// <param name="a">The node to connect to b.</param>
            /// <param name="b">The node to connect to a.</param>
            /// <returns>True if the connection succeeded, else false.</returns>
            public bool ConnectNodes(BNode a, BNode b)
            { 
                // We're only responsible for our nodes.
                if(a.parent != this || b.parent != this)
                    return false;

                if(a.next != null && a.prev != null)
                    return false;

                if(b.next != null && b.prev != null)
                    return false;

                if(a.next != null && b.next == null)
                {
                    BNode tmp = a;
                    a = b;
                    b = tmp;
                }

                if(a.next != null)
                    a.InvertChainOrder();

                // This should only happen if they're different strips
                if(b.prev != null)
                    b.InvertChainOrder();

                a.next = b;
                b.prev = a;

                a.FlagDirty();
                b.FlagDirty();
                return true;
            }

            /// <summary>
            /// Given a node this in the loop, extract its entire island, and move it
            /// into a newly created loop.
            /// </summary>
            /// <param name="member">A node in the loop that should be moved into its own
            /// new loop.</param>
            /// <param name="createInParent">If true, the created loop will be added to the
            /// parent shape. Else, it will be created as an orphan.</param>
            /// <returns>The newly created loop, or null if the operation fails.</returns>
            public BLoop ExtractIsland(BNode member, bool createInParent = true)
            { 
                if(member.parent != this)
                    return null;

                BLoop bl = new BLoop(createInParent ? this.shape : null);

                BNode.EndpointQuery eq = member.GetPathLeftmost();

                eq.node.parent = bl;
                this.nodes.Remove(eq.node);
                bl.nodes.Add( eq.node);

                for(BNode bn = eq.node.next; bn != null && bn != eq.node; bn = bn.next)
                { 
                    bn.parent = bl;
                    this.nodes.Remove(bn);
                    bl.nodes.Add(bn);
                }

                bl.FlagDirty();
                this.FlagDirty();

                return bl;
            }

            /// <summary>
            /// Calculate the shape's winding based on the nodes and tangents.
            /// This can be used for closed loops to determine if an island is filled or hollow.
            /// </summary>
            /// <param name="node">A node in the island to calculate the winding for. The node must
            /// be a child of the loop.</param>
            /// <param name="rewindMember"></param>
            /// <returns>The winding value of the island. A negative value is filled, while a positive value is hollow.</returns>
            public float CalculateWindingSimple(BNode node, bool rewindMember = true)
            { 
                if(rewindMember == true)
                { 
                    BNode.EndpointQuery eq = node.GetPathLeftmost();
                    node = eq.node;
                }

                float ret = node.CalculateWindingSimple();
                for(BNode it = node.next; it != null && it != node; it = it.next)
                    ret += it.CalculateWindingSimple();

                return ret;
            }

            /// <summary>
            /// Calculate the shape's winding based on the child nodes' segments.
            /// This can be used for closed loops to determine if an island is filled or hollow.
            /// </summary>
            /// <param name="node">A node in the island to calculate the winding for. The node must
            /// be a child of the loop.</param>
            /// <param name="rewindMember">If true, rewind the node to the start of the chain before calculating.</param>
            /// <returns>The winding value of the island. A negative value is filled, while a positive value is hollow.</returns>
            public float CalculateWindingSamples(BNode node, bool rewindMember = true)
            {
                if (rewindMember == true)
                {
                    BNode.EndpointQuery eq = node.GetPathLeftmost();
                    node = eq.node;
                }

                float ret = node.CalculateWindingSamples();
                for (BNode it = node.next; it != null && it != node; it = it.next)
                    ret += it.CalculateWindingSamples();

                return ret;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="v2"></param>
            /// <returns></returns>
            public static Vector2 RotateEdge90CCW(Vector2 v2)
            { 
                return new Vector2(-v2.y, v2.x);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="amt"></param>
            public void Inflate(float amt)
            { 
                Dictionary<BNode, InflationCache> cachedInf = 
                    new Dictionary<BNode, InflationCache>();

                // Go through all nodes and get their influences. We can't do this
                // on the same pass we update them, or else we would be modifying
                // values that would be evaluated later as dirty neighbors.
                foreach(BNode bn in this.nodes)
                { 
                    InflationCache ic = new InflationCache();
                    bn.GetInflateDirection(out ic.selfInf, out ic.inInf, out ic.outInf);

                    cachedInf.Add(bn, ic);
                }

                foreach(KeyValuePair<BNode, InflationCache> kvp in cachedInf)
                { 
                    BNode bn = kvp.Key;
                    // This is just us being lazy for sanity. Sure we could try to 
                    // inflate while keeping smooth or symmetry, or it might just
                    // naturally work itself out if we leave it alone - but I'd rather
                    // take the easy way out on this for now.
                    // (wleu)
                    if(bn.tangentMode != BNode.TangentMode.Disconnected)
                        bn.SetTangentDisconnected();

                    bn.Pos += amt * kvp.Value.selfInf;
                    bn.TanIn += amt * (kvp.Value.inInf - kvp.Value.selfInf);
                    bn.TanOut += amt * (kvp.Value.outInf - kvp.Value.selfInf);
                }
            }

            /// <summary>
            /// 
            /// </summary>
            public void Reverse()
            { 
                foreach(BNode bn in this.nodes)
                    bn._Invert();
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="dst"></param>
            public void DumpInto(BLoop dst)
            { 
                if(dst == this)
                    return;

                foreach(BNode bn in this.nodes)
                { 
                    bn.parent = dst;
                    dst.nodes.Add(bn);
                }

                this.nodes.Clear();
            }

            /// <summary>
            /// Call BNode.Deinflect() for all nodes in the loop.
            /// </summary>
            public void Deinflect()
            { 
                List<BNode> nodeCpy = new List<BNode>( this.nodes);
                foreach(BNode bn in nodeCpy)
                    bn.Deinflect();
            }
        }
    } 
}