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

namespace PxPre 
{   
    namespace Berny
    {
        /// <summary>
        /// A node in a BLoop. The node represents a position in the path, as well as the 
        /// curve tangents at that position.
        /// 
        /// There is also linked list connectivity data.
        /// </summary>
        public class BNode
        {
            /// <summary>
            /// The type of tangent to enforce when drawing and editing the node.
            /// </summary>
            public enum TangentMode
            { 
                /// <summary>
                /// The nodes can have an input and output tangent of different magnitudes and directions.
                /// </summary>
                Disconnected,

                /// <summary>
                /// The tangents are set to have the same direction, but can have different magnitudes.
                /// </summary>
                Smooth,

                /// <summary>
                /// The tangents are set to have the same direction and magnitude. This also enforces
                /// both tangents to be on.
                /// </summary>
                Symmetric
            }

            /// <summary>
            /// A representation of the input and output tangent.
            /// </summary>
            public enum TangentType
            {
                /// <summary>
                /// An input tangent.
                /// </summary>
                Input,

                /// <summary>
                /// An output tangent.
                /// </summary>
                Output
            }

            /// <summary>
            /// An enum used to describe the result of finding the first node in a path.
            /// </summary>
            public enum EndpointResult
            { 
                /// <summary>
                /// The node that's the edge of the path was successfully found. This also
                /// means it's an open chain.
                /// </summary>
                SuccessfulEdge,

                /// <summary>
                /// The node is a closed chain - this means there is no edge node in the island.
                /// </summary>
                Cyclical
            }

            /// <summary>
            /// A result structure that holds information on what kind of island the edge node
            /// is, and the node at the edge.
            /// </summary>
            public struct EndpointQuery
            { 
                /// <summary>
                /// The edge node of the island - or if the island is closed, it's any node
                /// from the island.
                /// </summary>
                public BNode node;

                /// <summary>
                /// The type of island the node belongs to - whether it's an open chain or closed.
                /// </summary>
                public EndpointResult result;

                /// <summary>
                /// Constructor.
                /// </summary>
                /// <param name="node">The node to set the struct's value to.</param>
                /// <param name="result">The result value to set the struct's value to.</param>
                public EndpointQuery(BNode node, EndpointResult result)
                { 
                    this.node = node;
                    this.result = result;
                }
            }

            /// <summary>
            /// The type of interpolation the node is support to connect to the next path.
            /// </summary>
            public enum PathType
            { 
                /// <summary>
                /// The node is an explicit end to the path.
                /// </summary>
                /// <remarks>This is deprecated - as the official way to mark the end of a
                /// path is to just have a broken link list reference to the next node.</remarks>
                None,

                /// <summary>
                /// The node supports Bezier path features.
                /// </summary>
                BezierCurve,

                /// <summary>
                /// The node is a straight line to the next node. This means the next node's 
                /// input tangent is ignored, and the path can be a single line segment instead
                /// of a flattened curve.
                /// </summary>
                Line
            }

            /// <summary>
            /// Represents the final processed tangent curves and interpolation.
            /// </summary>
            public struct PathBridge
            { 
                /// <summary>
                /// The path type.
                /// </summary>
                public PathType pathType;

                /// <summary>
                /// The tangent exiting the current node - hermite form.
                /// </summary>
                public Vector2 prevTanOut;

                /// <summary>
                /// The tangent entering the next node - hermite form.
                /// </summary>
                public Vector2 nextTanIn;

                /// <summary>
                /// Constructor.
                /// </summary>
                /// <param name="pathType">The path type to set the object's value to.</param>
                /// <param name="prevTanOut">The current node's output tangent.</param>
                /// <param name="nextTanIn">The next node's input tangent.</param>
                public PathBridge(PathType pathType, Vector2 prevTanOut, Vector2 nextTanIn)
                { 
                    this.pathType = pathType;
                    this.prevTanOut = prevTanOut;
                    this.nextTanIn = nextTanIn;
                }
            }

            /// <summary>
            /// A structure representing a BNode's basic properties of representing a node
            /// in the path, without variable properties or linked list data.
            /// </summary>
            public struct BezierInfo
            {
                /// <summary>
                /// The position of the node.
                /// </summary>
                public Vector2 pos;

                /// <summary>
                /// The incomming tangent of the node.
                /// </summary>
                public Vector2 tanIn;

                /// <summary>
                /// The outgoing tangent of the node.
                /// </summary>
                public Vector2 tanOut;

                /// <summary>
                /// If true, the incomming tangent is used, else a zero vector should be used.
                /// </summary>
                public bool useTanIn;

                /// <summary>
                /// If true, the outgoing tangent is used, else a zero vector should be used.
                /// </summary>
                public bool useTanOut;

                /// <summary>
                /// The tangent mode of the node.
                /// </summary>
                public TangentMode tangentMode;

                /// <summary>
                /// Constructor of a tangentless node.
                /// </summary>
                /// <param name="pos">The position of the node.</param>
                public BezierInfo(Vector2 pos)
                { 
                    this.pos        = pos;

                    // Defaults for everything else.
                    this.tanIn      = Vector2.zero;
                    this.tanOut     = Vector2.zero;
                    //
                    this.tangentMode = TangentMode.Disconnected;
                    this.useTanIn = false;
                    this.useTanOut = false;

                }

                /// <summary>
                /// Constructor of a disconnected node.
                /// </summary>
                /// <param name="pos">The position of the node.</param>
                /// <param name="tanIn">The value to set the object's incoming tangent.</param>
                /// <param name="tanOut">The value to set the object's outgoing tangent.</param>
                public BezierInfo(Vector2 pos, Vector2 tanIn, Vector2 tanOut)
                { 
                    this.pos = pos;
                    this.tanIn = tanIn;
                    this.tanOut = tanOut;

                    this.useTanIn = true;
                    this.useTanOut = true;

                    this.tangentMode = TangentMode.Disconnected;
                }

                /// <summary>
                /// Constructor.
                /// </summary>
                /// <param name="pos">The position of the node.</param>
                /// <param name="tanIn">The value to set the object's incoming tangent.</param>
                /// <param name="tanOut">The value to set the object's outgoing tangent.</param>
                /// <param name="leftTan">True if the left tangent is used.</param>
                /// <param name="rightTan">True if the right tangent is used.</param>
                /// <param name="tanMode">The tangent mode of the object.</param>
                public BezierInfo(
                    Vector2 pos, 
                    Vector2 tanIn, 
                    Vector2 tanOut, 
                    bool leftTan, 
                    bool rightTan, 
                    TangentMode tanMode)
                {
                    this.pos = pos;
                    this.tanIn = tanIn;
                    this.tanOut = tanOut;

                    this.useTanIn = leftTan;
                    this.useTanOut = rightTan;

                    this.tangentMode = TangentMode.Disconnected;
                }
            }

            /// <summary>
            /// The node's parent. If this is set to reference a parent, the parent loop should also
            /// contain a reference to the child node in its BLoop.nodes variable.
            /// </summary>
            public BLoop parent;

            /// <summary>
            /// Link list reference to the next node in the island chain.
            /// </summary>
            /// <remarks>If set, the next's prev should point to this node.</remarks>
            public BNode next = null;

            /// <summary>
            /// Link list reference to the previous node in the island chain.
            /// </summary>
            /// <remarks>If set, the prev's next should point to this node.</remarks>
            public BNode prev = null;

            /// <summary>
            /// The segment sample starting at the node's position.
            /// Used to create the flattened preview node, as well as the base template
            /// for filling in the node.
            /// </summary>
            public BSample sample = null;

            /// <summary>
            /// The node's position.
            /// </summary>
            Vector2 pos;

            /// <summary>
            /// The node's incoming tangent value.
            /// </summary>
            Vector2 tanIn;

            /// <summary>
            /// The node's outgoing tangent value.l
            /// </summary>
            Vector2 tanOut;

            /// <summary>
            /// A struct used to represent a point on the curve
            /// for bisection algorithms.
            /// </summary>
            public struct PointOnCurve
            {
                /// <summary>
                /// The node being sampled.
                /// </summary>
                public BNode node;

                /// <summary>
                /// The lambda into the node's curve.
                /// </summary>
                public float lambda;

                /// <summary>
                /// True if the point represents the very edge of a node chain path.
                /// </summary>
                public bool inrange;
            }

            /// <summary>
            /// The property for the node's position. 
            /// 
            /// Ensures the dirty flag is set if somethign outside modifies it.
            /// </summary>
            public Vector2 Pos 
            {
                get => this.pos;
                set 
                {
                    this.pos = value; 
                    this.FlagDirty(); 
                } 
            }

            /// <summary>
            /// The property for the node's incoming tangent.
            /// 
            /// Ensures the dirty flag is set if something outside modifies it.
            /// Also handles enforcement of the tangentMode setting.
            /// </summary>
            public Vector2 TanIn
            { 
                get => this.tanIn;
                set 
                {
                    this.tanIn = value; 

                    switch(this.tangentMode)
                    { 
                        case TangentMode.Smooth:
                            this.tanOut = -value.normalized * this.tanOut.magnitude;
                            break;

                        case TangentMode.Symmetric:
                            this.tanOut = -value;
                            break;
                    }

                    this.FlagDirty(); 
                }
            }

            /// <summary>
            /// The propery for the node's output tangent.
            /// 
            /// Ensures the dirty flag is set if something outside modifies it.
            /// /// Also handles enforcement of the tangentMode setting.
            /// </summary>
            public Vector2 TanOut
            { 
                get => this.tanOut;
                set
                { 
                    this.tanOut = value;

                    switch(this.tangentMode)
                    { 
                        case TangentMode.Smooth:
                            this.tanIn = -value.normalized * this.tanIn.magnitude;
                            break;

                        case TangentMode.Symmetric:
                            this.tanIn = -value;
                            break;
                    }

                    this.FlagDirty();
                }
            }

            /// <summary>
            /// If true, the node should use the incoming tangent.
            /// </summary>
            bool useTanIn = false;

            /// <summary>
            /// If true, the node should use the outgoing tangent.
            /// </summary>
            bool useTanOut = false;

            /// <summary>
            /// The public property for using the incomming tangent value.
            /// 
            /// Ensures the dirty flag is set if something outside modifies it.
            /// </summary>
            public bool UseTanIn 
            {
                get => useTanIn;
                set
                { 
                    if(this.useTanIn == value)
                        return;

                    this.useTanIn = value;
                    this.tangentMode = TangentMode.Disconnected;

                    this.FlagDirty();
                }
            }

            /// <summary>
            /// The public property for using the outgoing tangent value.
            /// 
            /// Ensures the dirty flag is set if something outside modifies it.
            /// </summary>
            public bool UseTanOut
            { 
                get => useTanOut;
                set
                { 
                    if(this.useTanOut == value)
                        return;

                    this.useTanOut = value;
                    this.tangentMode = TangentMode.Disconnected;

                    this.FlagDirty();
                }
            }

            /// <summary>
            /// The tangent mode.
            /// </summary>
            // TODO: public access to this needs to be wrapped into a property.
            public TangentMode tangentMode = TangentMode.Disconnected;

            /// <summary>
            /// The dirty state of the. If true, the node is dirty which means it has been
            /// modified since the last time it was prepared for presentation.
            /// </summary>
            bool dirty = false;

#if DEVELOPMENT_BUILD || UNITY_EDITOR
            /// <summary>
            /// Debug ID. Each of this object created will have a unique ID that will be assigned the same way
            /// if each app session runs deterministicly the same. Used for identifying objects when
            /// debugging.
            /// </summary>
            public int debugCounter;
#endif

            /// <summary>
            /// Constructor for a node without tangents.
            /// </summary>
            /// <param name="parent">The parent loop.</param>
            /// <param name="pos">The position of the node.</param>
            public BNode(BLoop parent, Vector2 pos)
            { 
                this.pos = pos;
                this.parent = parent;

                this.tanIn = Vector2.zero;
                this.tanOut = Vector2.zero;

#if DEVELOPMENT_BUILD || UNITY_EDITOR
                this.debugCounter = Utils.RegisterCounter();
#endif
            }

            /// <summary>
            /// Constructor for copying the values of a BezierInfo.
            /// </summary>
            /// <param name="parent">The parent loop.</param>
            /// <param name="bi">The node info to copy from.</param>
            public BNode(BLoop parent, BezierInfo bi)
            { 
                this.pos = bi.pos;
                this.parent = parent;

                this.tanIn = bi.tanIn;
                this.tanOut = bi.tanOut;

                this.useTanIn = bi.useTanIn;
                this.useTanOut = bi.useTanOut;

                this.tangentMode = bi.tangentMode;

#if DEVELOPMENT_BUILD || UNITY_EDITOR
                this.debugCounter = Utils.RegisterCounter();
#endif
            }

            /// <summary>
            /// Constructor for a node with disconnected tangents..
            /// </summary>
            /// <param name="parent">The parent loop.</param>
            /// <param name="pos">The node's position.</param>
            /// <param name="tanIn">The incomming tangent.</param>
            /// <param name="tanOut">The outgoing tangent.</param>
            public BNode(BLoop parent, Vector2 pos, Vector2 tanIn, Vector2 tanOut)
            { 
                this.pos = pos;
                this.parent = parent;

                this.tanIn = tanIn;
                this.tanOut = tanOut;

                this.useTanIn = tanIn.sqrMagnitude >= Mathf.Epsilon;
                this.useTanOut = tanOut.sqrMagnitude >= Mathf.Epsilon;

                this.tangentMode = TangentMode.Disconnected;

#if DEVELOPMENT_BUILD || UNITY_EDITOR
                this.debugCounter = Utils.RegisterCounter();
#endif
            }

            /// <summary>
            /// Sets the tangent value based on a specified TangentType for
            /// which tangent (incomming or outgoing) to modify.
            /// </summary>
            /// <param name="tt">The tangent type to set.</param>
            /// <param name="value">The tangent value.</param>
            public void SetTangent(TangentType tt, Vector2 value)
            { 
                if(tt == TangentType.Output)
                    this.TanIn = value;
                else
                    this.TanOut = value;

                this.FlagDirty();
            }

            /// <summary>
            /// Sets an input and output tangent. 
            /// 
            /// Forces the tangent to become a disconnected continuity.
            /// </summary>
            /// <param name="input">The incomming tangent value.</param>
            /// <param name="output">The outgoing tangent value.</param>
            public void SetTangents(Vector2 input, Vector2 output)
            { 
                this.tangentMode = TangentMode.Disconnected;
                this.tanIn = input;
                this.tanOut = output;

                this.FlagDirty();
            }

            /// <summary>
            /// Gets the tangent value based on a specified TangentType.
            /// </summary>
            /// <param name="tt">The tangent type to retrieve.</param>
            /// <returns>The requested tangent.</returns>
            /// <remarks>Gives direct access to the tangent, ignores tangent usage rules.</remarks>
            public Vector2 GetTangent(TangentType tt)
            {
                if(tt == TangentType.Input)
                    return this.tanIn;
                else
                    return this.tanOut;
            }

            /// <summary>
            /// Set the node's data from a BezierInfo.
            /// </summary>
            /// <param name="binfo">the info to set.</param>
            /// <param name="checkDirty">If true, ignore setting the value if it matches the node. 
            /// While this can create overhead, it can also remove overhead avoiding unneccessarily 
            /// flagging the node and its parent as dirty.</param>
            public void SetFromInfo(BezierInfo binfo, bool checkDirty = false)
            { 
                if(checkDirty == true)
                { 
                    if(
                            this.pos            == binfo.pos        &&
                            this.tanIn          == binfo.tanIn      &&
                            this.tanOut         == binfo.tanOut     &&
                            this.useTanIn       == binfo.useTanIn &&
                            this.useTanOut      == binfo.useTanOut &&
                            this.tangentMode    == binfo.tangentMode)
                    { 
                        return;
                    }
                }

                this.pos        = binfo.pos;

                this.tanIn      = binfo.tanIn;
                this.tanOut     = binfo.tanOut;

                this.useTanIn   = binfo.useTanIn;
                this.useTanOut  = binfo.useTanOut;

                this.tangentMode = binfo.tangentMode;

                this.dirty = true;
            }

            /// <summary>
            /// Returns a BezierInfo of the node's contents.
            /// </summary>
            /// <returns>The node's contents as a BezierInfo.</returns>
            public BezierInfo GetInfo()
            {
                BezierInfo ret = new BezierInfo();

                ret.pos         = this.pos;
                ret.tanIn       = this.tanIn;
                ret.tanOut      = this.tanOut;
                ret.useTanIn    = this.useTanIn;
                ret.useTanOut   = this.useTanOut;
                ret.tangentMode = this.tangentMode;

                return ret;

            }

            /// <summary>
            /// Set the tangent mode to be symmetrical.
            /// </summary>
            /// <param name="inToOut">If true, the output will be the
            /// mirrored version of the input - if false, the input will
            /// be the mirrored version of the output.</param>
            public void SymmetryHandle(bool inToOut = true)
            { 
                if(inToOut == true)
                    this.tanOut = this.tanIn;
                else
                    this.tanIn = this.tanOut;
            }

            /// <summary>
            /// Flag the node as dirty, marking it as being modified
            /// after the last time it was prepared for presentation.
            /// </summary>
            public void FlagDirty()
            { 
                this.dirty = true;

                if(this.parent != null)
                    this.parent.FlagDirty();
            }

            /// <summary>
            /// If true, the node is dirty. 
            /// 
            /// This means the node has been modified since the last time it was
            /// prepared for presentation. To clear the dirty flag, call
            /// HandleDirty().
            /// </summary>
            /// <returns></returns>
            public bool IsDirty()
            { 
                return this.dirty;
            }

            /// <summary>
            /// Prepare the node for presentation if dirty. This mostly involves
            /// reconstructing the sample segments.
            /// 
            /// The function clears the node's dirty flag afterwards.
            /// </summary>
            /// <param name="force">If true, prepare the node even if the
            /// node isn't flagged as dirty.</param>
            public void HandleDirty(bool force = false)
            { 
                if(force == true && this.dirty == false)
                    return;

                const int minSubAmt = 20;

                this.EnsureSyncedSelfSample();
                this.sample.pos = this.pos;


                PathBridge pb = GetPathBridgeInfo();
                // Is there anything to even connect to?
                if (pb.pathType == PathType.None)
                    this.sample.next = null;
                // Can we just make a straight line instead of a curve if there aren't
                // tangents?
                else if(pb.pathType == PathType.Line)
                {
                    this.next.EnsureSyncedSelfSample();

                    //Direct connection, no curve middlemen
                    this.sample.next = this.next.sample;
                    this.next.sample.prev = this.sample;
                }
                // Everything else is a Bezier curve
                else
                { 
                    this.next.EnsureSyncedSelfSample();

                    BSample bsPrev = this.sample;
                    for(int i = 1; i < minSubAmt; ++i)
                    { 
                        float lambda = (float)i / (float)(minSubAmt );
                        float A, B, C, D;
                        Utils.GetBezierWeights(lambda, out A, out B, out C, out D);

                        Vector2 subPt = 
                            A * this.pos + 
                            B * (this.pos + pb.prevTanOut) +
                            C * (this.next.pos + pb.nextTanIn) +
                            D * this.next.pos;

                        BSample bs = new BSample(this, subPt ,lambda);
                        bsPrev.next = bs;
                        bs.prev = bsPrev;
                        bsPrev = bs;
                    }

                    bsPrev.next = this.next.sample;
                    this.next.sample.prev = bsPrev;
                }

                this.dirty = false;
            }

            /// <summary>
            /// 
            /// </summary>
            public void Round()
            { 
                Vector2 avg = Vector2.zero;

                if(this.next != null)
                    avg -= this.next.pos - this.pos;

                if(this.prev != null)
                    avg -= this.pos - this.prev.pos;

                this.tanIn = avg / 4.0f;
                this.tanOut = -avg / 4.0f;

                this.useTanIn = true;
                this.useTanOut = true;

                this.tangentMode = TangentMode.Symmetric;

                this.FlagDirty();
            }

            /// <summary>
            /// 
            /// </summary>
            public void EnsureSyncedSelfSample()
            {
                if (this.sample == null)
                    this.sample = new BSample(this, this.pos, 0.0f);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="force"></param>
            public void SetTangentSmooth(bool force = false)
            { 
                if(force == false && this.tangentMode == TangentMode.Smooth)
                    return;

                this.tangentMode    = TangentMode.Smooth;
                this.useTanIn       = true;
                this.useTanOut      = true;

                float tanInMag = this.tanIn.magnitude;
                float tanOutMag = this.tanOut.magnitude;

                if(tanInMag + tanOutMag < Mathf.Epsilon)
                    this.Round(); // Flags dirty
                else
                { 
                    Vector2 dir = (this.tanIn - this.tanOut).normalized;

                    this.tanIn = dir * tanInMag;
                    this.tanOut = -dir * tanOutMag;

                    this.FlagDirty();
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="force"></param>
            public void SetTangentsSymmetry(bool force = false)
            {
                if(force == false && this.tangentMode == TangentMode.Symmetric)
                    return;

                this.tangentMode = TangentMode.Symmetric;
                this.useTanIn = true;
                this.useTanOut = true;

                Vector2 avg = (this.tanIn - this.tanOut) * 0.5f;
                if(avg.sqrMagnitude < Mathf.Epsilon)
                    this.Round(); // Flags dirty
                else
                {
                    this.tanIn = avg;
                    this.tanOut = -avg;

                    this.FlagDirty();
                }
            }

            /// <summary>
            /// Set the node to have disconnected tangents.
            /// </summary>
            /// <param name="force">If true, set again, even if already using 
            /// disconnected tangents.</param>
            public void SetTangentDisconnected(bool force = false)
            {
        
                if(force == false && this.tangentMode == TangentMode.Disconnected)
                    return;

                this.tangentMode = TangentMode.Disconnected;
                this.FlagDirty();
            }

            /// <summary>
            /// Try to find the leftmost node in the node's island.
            /// </summary>
            /// <returns>The query's result with the leftmost node.</returns>
            public EndpointQuery GetPathLeftmost()
            { 
                if(this.prev == null)
                    return new EndpointQuery(this, EndpointResult.SuccessfulEdge);

                BNode it = this.prev;
                while(true)
                { 
                    if(it == this)
                        return new EndpointQuery(this, EndpointResult.Cyclical);

                    if(it.prev == null)
                        return new EndpointQuery(it, EndpointResult.SuccessfulEdge);

                    it = it.prev;
                }
            }

            /// <summary>
            /// Try to find the rightmost node in the node's island.
            /// </summary>
            /// <returns>The query's result with the rightmost node.</returns>
            public EndpointQuery GetPathRightmost()
            { 
                if(this.next == null)
                    return new EndpointQuery(this, EndpointResult.SuccessfulEdge);

                BNode it = this.next;
                while(true)
                { 
                    if(it == this)
                        return new EndpointQuery(this, EndpointResult.Cyclical);

                    if(it.next == null)
                        return new EndpointQuery(it, EndpointResult.SuccessfulEdge);

                    it = it.next;
                }
            }

            /// <summary>
            /// Approxiate the arclength by calculating the summed lengths
            /// of the node's segment samples.
            /// </summary>
            /// <returns>The approximate arclength.</returns>
            /// <remarks>The function depends on the segment samples being 
            /// correct - best way to do that is to make sure the node isn't
            /// dirty when the function is called.</remarks>
            public float CalculateSampleLens()
            { 
                if(this.sample == null)
                    return 0.0f;

                float ret = 0.0f;

                for(
                    BSample bs = this.sample; 
                    bs != null && bs.next != null && bs.parent == this; 
                    bs = bs.next)
                { 
                    ret += (bs.pos - bs.next.pos).magnitude;
                }

                return ret;
            }

            /// <summary>
            /// Approximate the arclength by flatting the curve and summing 
            /// the lengths of those segments.
            /// 
            /// Does not affect sample segments.
            /// </summary>
            /// <param name="subdivs">The number of times to subdivide.</param>
            /// <returns>The approximate arclength.</returns>
            public float CalculateArcLen(int subdivs = 30)
            { 
                PathBridge pb = GetPathBridgeInfo();

                if(pb.pathType == PathType.None)
                    return 0.0f;

                if(pb.pathType == PathType.Line)
                    return (this.pos - this.next.pos).magnitude;

                float ret = 0.0f;
                for (int i = 0; i < subdivs; ++i)
                { 
                    float l1 = (float)(i + 0) / (float)(subdivs - 1);
                    float l2 = (float)(i + 1) / (float)(subdivs - 1);

                    float A, B, C, D;
                    Utils.GetBezierWeights(l1, out A, out B, out C, out D);

                    Vector2 prvPos =
                        A * this.pos +
                        B * (this.pos + pb.prevTanOut) +
                        C * (this.next.pos + pb.nextTanIn) +
                        D * this.next.pos;

                    Utils.GetBezierWeights(l2, out A, out B, out C, out D);

                    Vector2 nxtPos =
                        A * this.pos +
                        B * (this.pos + pb.prevTanOut) +
                        C * (this.next.pos + pb.nextTanIn) +
                        D * this.next.pos;

                    ret += (prvPos - nxtPos).magnitude;
                }
                return ret;
            }

            /// <summary>
            /// Get the PathBridge evaluated at the node. The info will contain the
            /// final tangents, as well as the path connection method to use to connect its
            /// position to the next node.
            /// </summary>
            /// <returns>The PathBridge evaluated at the node.</returns>
            public PathBridge GetPathBridgeInfo()
            { 
                PathBridge pb = new PathBridge();

                if(this.next == null)
                {
                    pb.pathType = PathType.None;
                    return pb;
                }

                bool leftTan = false;
                bool rightTan = false;

                if(this.useTanOut == true && this.tanOut.sqrMagnitude > Mathf.Epsilon)
                {
                    pb.prevTanOut = this.tanOut;
                    leftTan = true;
                }

                if(this.next.useTanIn == true && this.next.tanIn.sqrMagnitude > Mathf.Epsilon)
                {
                    pb.nextTanIn = this.next.tanIn;
                    rightTan = true;
                }

                if(leftTan == false && rightTan == false)
                    pb.pathType = PathType.Line;
                else
                    pb.pathType = PathType.BezierCurve;

                return pb;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="lambda">The t value to subdivide.</param>
            /// <returns></returns>
            // TODO: Has been tested to be inaccurate in certain sitations.
            // This implementation is deprecated and is going to be replaced
            // with De Casteljau's algorithm for solving the tangents.
            public BNode Subdivide(float lambda = 0.5f)
            {
                if(this.parent == null)
                    return null;

                // We need to have our parent subdivide us to make
                // sure the loop can integrate it properly  with encapsulation.
                return this.parent.Subdivide(this, lambda);
            }

            /// <summary>
            /// Checks if the node is the only member in its island.
            /// </summary>
            /// <returns>True if the node is the only member in its island.</returns>
            public bool IsStrayPoint()
            { 
                return 
                    this.next == null && 
                    this.prev == null;
            }

            /// <summary>
            /// Disconnect the node from the link list.
            /// </summary>
            /// <param name="removeIfStray">If true, remove if the disconnect leaves
            /// the point stray.</param>
            /// <returns>True of the disconnect was successful.</returns>
            public bool Disconnect(bool removeIfStray = true)
            { 
                if(this.next == null)
                    return false;

                this.next.prev = null;
                this.next = null;

                if(removeIfStray == true && this.IsStrayPoint() == true)
                    this.parent.RemoveNode(this);

                this.FlagDirty();
                return true;
            }

            /// <summary>
            /// Make a cut at the chain where the node is.
            /// </summary>
            /// <returns>True if the detachment was successful.</returns>
            public bool Detach()
            { 
                // It doesn't make sense to detach if we're an edge
                if(this.prev == null || this.next == null)
                    return false;

                // To detach, we not only need to not reference our previous, but our 
                // previous need to stop referencing us. To to that without being a 
                // disconnect, we need to create a copy that the previous can
                // reference instead of us.
                BNode bnNew = this.Clone(this.parent);
                if(this.parent != null)
                    this.parent.nodes.Add(bnNew);

                this.prev = null;
                bnNew.prev.next = bnNew;
                bnNew.next = null;

                this.FlagDirty();
                return true;
            }

            /// <summary>
            /// Reverse the order of the island. The outline will look the same, 
            /// the chain direction and its winding should be inverted.
            /// </summary>
            public void InvertChainOrder()
            { 
                EndpointQuery eq = this.GetPathLeftmost();

                BNode it = eq.node.next;

                BNode tmp = eq.node.prev;
                eq.node.prev = eq.node.next;
                eq.node.next = tmp;
                eq.node.SwapTangents();
       
                while(it != null && it != eq.node)
                { 
                    BNode next = it.next;

                    tmp = it.prev;
                    it.prev = it.next;
                    it.next = tmp;
                    it.SwapTangents();

                    it = next;
                }
            }

            /// <summary>
            /// Swap the input and output tangents.
            /// </summary>
            public void SwapTangents()
            { 
                bool oldUseIn = this.useTanIn;
                this.useTanIn = this.useTanOut;
                this.useTanOut = oldUseIn;

                Vector2 oldIn = this.tanIn;
                this.tanIn = this.tanOut;
                this.tanOut = oldIn;

                this.FlagDirty();
            }

            /// <summary>
            /// Find the closest point on the chain.
            /// 
            /// The solution attempts to perform a numerical segmentation and bisection to approximate
            /// this value.
            /// </summary>
            /// <param name="pos">The position to find the closest location in respect to.</param>
            /// <param name="initSubdivs">The initial number of times to subdivide before 
            /// doing the bisection phase.</param>
            /// <param name="maxBisections">The number of bisection iterations.</param>
            /// <param name="eps">The epsillon for when to quit if bisected areas get too small.</param>
            /// <returns>The estimated closes point on the curve.</returns>
            /// <remarks>This implementation is depresecated for a more accurate analytical method.</remarks>
            //
            // A better implementation exists, but only in peices for a BNode segment and not an entire chain.
            // It is planned to be added in and this will be phased out.
            public PointOnCurve FindClosestToPointChainBySub(Vector2 pos, int initSubdivs, int maxBisections, float eps)
            {
                BNode closest = null;
                float closestDistSqr = float.PositiveInfinity;
                float secondClosestSqr = float.PositiveInfinity;
                int closestIndex = -1;
                int secondClosestIndex = -1;

                // Find the segment with the closest subdivided point. We don't
                // care exactly where right now, only which.
                BNode bn = this;
                while(bn != null)
                {
                    float sampClosestDistSqr;
                    float sampSecondClosestSqr;
                    int sampClosestIndex;
                    int sampSecondClosestIndex;

                    bool sampRet = 
                        bn._FindClosestToPointSub(
                            pos, 
                            initSubdivs, 
                            out sampClosestDistSqr, 
                            out sampSecondClosestSqr, 
                            out sampClosestIndex, 
                            out sampSecondClosestIndex);

                    if (sampRet == false)
                        break;

                    if(
                        closest == null ||
                        sampClosestDistSqr < closestDistSqr || 
                        (sampClosestDistSqr == closestDistSqr && sampSecondClosestSqr < secondClosestSqr))
                    { 
                        closest = bn;
                        closestDistSqr = sampClosestDistSqr;
                        secondClosestSqr = sampSecondClosestSqr;
                        closestIndex = sampClosestIndex;
                        secondClosestIndex = sampSecondClosestIndex;
                    }

                    if(bn.next == this)
                        break;

                    bn = bn.next;
                }

                PointOnCurve ret = new PointOnCurve();
                if(closest == null)
                    return ret;

                float lamL;
                float lamR;
                float distL;
                float distR;

                // Set up a window between the closest and second. With any luck, they'll
                // be right next to each other.
                if (closestIndex < secondClosestIndex)
                { 
                    lamL = (float)closestIndex / (float)(initSubdivs - 1);
                    lamR = (float)secondClosestIndex / (float)(initSubdivs - 1);
                    distL = closestDistSqr;
                    distR = secondClosestSqr;
                }
                else
                {
                    lamL = (float)secondClosestIndex / (float)(initSubdivs - 1);
                    lamR = (float)closestIndex / (float)(initSubdivs - 1);
                    distL = secondClosestSqr;
                    distR = closestDistSqr;
                }

                PathBridge pb = closest.GetPathBridgeInfo();
                for (int i = 0; i < maxBisections; ++i)
                { 
                    float midL = (lamL + lamR) * 0.5f;
                    float A, B, C, D;
                    Utils.GetBezierWeights(midL, out A, out B, out C, out D);

                    Vector2 sample =
                        A * closest.pos +
                        B * (closest.pos + pb.prevTanOut) +
                        C * (closest.next.pos + pb.nextTanIn) +
                        D * closest.next.pos;

                    float midDstSqr = (sample - pos).sqrMagnitude;

                    if(distL < distR)
                    { 
                        distR = midDstSqr;
                        lamR = midL;
                    }
                    else
                    { 
                        distL = midDstSqr;
                        lamL = midL;
                    }
                }

                ret.lambda = (lamR + lamR) * 0.5f;
                ret.node = closest;
                ret.inrange = (lamL == 0.0f && closest.prev == null) || (lamR == 1.0f && closest.next == null);
                return ret;
            }

            /// <summary>
            /// Utility function for BNode.FindClosestToPointChainBySub().
            /// Does this with numerical bisection.
            /// </summary>
            /// <param name="pos">The position to find the closest point in respect to.</param>
            /// <param name="initSubdivs">The number of times to subdivide the curve before 
            /// going through the bisection process.</param>
            /// <param name="closestDistSqr">The closest distance squared.</param>
            /// <param name="secondClosestSqr">The second closest distance squared</param>
            /// <param name="closestIdx">The index of the closest ID.</param>
            /// <param name="secondsClosestIdx">The index of the second closest ID.</param>
            /// <returns>True if a value could be calculated for the node, else false.</returns>
            /// <remarks>This function is deprecated and will eventually be removed for a more accurate 
            /// analytical function.</remarks>
            bool _FindClosestToPointSub(
                Vector2 pos, 
                int initSubdivs, 
                out float closestDistSqr, 
                out float secondClosestSqr, 
                out int closestIdx, 
                out int secondsClosestIdx)
            {
                closestDistSqr = float.PositiveInfinity;
                secondClosestSqr = float.PositiveInfinity;
                closestIdx = -1;
                secondsClosestIdx = -1;

                PathBridge pb = this.GetPathBridgeInfo();
                if (pb.pathType == PathType.None)
                    return false;

                for (int i = 0; i < initSubdivs; ++i)
                {
                    float lambda = (float)i / (float)(initSubdivs - 1);

                    float A, B, C, D;
                    Utils.GetBezierWeights(lambda, out A, out B, out C, out D);

                    Vector2 sample =
                        A * this.pos +
                        B * (this.pos + pb.prevTanOut) +
                        C * (this.next.pos + pb.nextTanIn) +
                        D * this.next.pos;

                    float sdistSqr = (sample - pos).sqrMagnitude;
                    if(sdistSqr < closestDistSqr)
                    { 
                        secondClosestSqr = closestDistSqr;
                        secondsClosestIdx = closestIdx;

                        closestDistSqr = sdistSqr;
                        closestIdx = i;
                    }
                    else if(sdistSqr < secondClosestSqr)
                    {
                        secondClosestSqr = closestDistSqr;
                        secondsClosestIdx = closestIdx;
                    }
                }

                return true;
            }

            /// <summary>
            /// Calculate the winding angle of the node, ignoring the tangents and pretending
            /// it's part of a path with straight line segments.
            /// </summary>
            /// <returns>The calculated winding angle.</returns>
            public float CalculateWindingSimple()
            { 
                if(this.prev == null || this.next == null)
                    return 0.0f;

                Vector2 towa = this.pos - this.prev.pos;
                Vector2 away = this.next.pos - this.pos;

                return towa.x * away.y - towa.y * away.x;
            }

            /// <summary>
            /// Calculate the winding angle of the node by calculating 
            /// the winding of the samples.
            /// </summary>
            /// <returns>The accumulated winding angle of all its sample segments.</returns>
            /// <remarks>In order for this to work correctly, the sample segments need
            /// to be set up and up to date. The best way to assure this is to make
            /// sure the node isn't dirty before calling this function.</remarks>
            public float CalculateWindingSamples()
            { 
                if(this.sample == null)
                    return 0.0f;

                float ret = 0.0f;

                BSample bs = this.sample;
                while(
                    bs.parent == this && 
                    bs != null && 
                    bs.next != null)
                { 
                    if(bs.prev == null)
                        continue;

                    Vector2 towa = this.pos - this.prev.pos;
                    Vector2 away = this.next.pos - this.pos;

                    ret += towa.x * away.y - towa.y * away.x;

                    bs = bs.next;
                }

                return ret;
            }

            /// <summary>
            /// Createa deep copy of the node and its contents.
            /// </summary>
            /// <param name="loop">The loop to add the node to.</param>
            /// <returns>The newly created duplicate node.</returns>
            /// <remarks>The node will also contain the same link list references, but those
            /// will not be properly hooked up. They're set so that mapping functions can
            /// translate them with their equivalent nodes when they're confirmed to be created, 
            /// assuming </remarks>
            public BNode Clone(BLoop loop)
            { 
                BNode newNode = new BNode(loop, this.GetInfo());
                newNode.prev = this.prev;
                newNode.next = this.next;
                return newNode;
            }

            /// <summary>
            /// Get the axis aligned bounding box of the bezier curve.
            /// </summary>
            /// <returns>The bounds of the node.</returns>
            /// <remarks>The return result is only corrct if the node isn't an ending point.</remarks>
            public BoundsMM2 GetBounds()
            {
                if(this.next == null)
                   return BoundsMM2.GetInifiniteRegion();

                PathBridge pb = this.GetPathBridgeInfo();

                Vector2 p0 = this.pos;
                Vector2 p1 = this.pos + pb.prevTanOut;
                Vector2 p2 = this.next.pos + pb.nextTanIn;
                Vector2 p3 = this.next.pos;

                return Utils.GetBoundingBoxCubic(p0, p1, p2, p3);
            }
        }
    }
}