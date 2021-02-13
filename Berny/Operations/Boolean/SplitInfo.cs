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

namespace PxPre
{
    namespace Berny
    { 
        public static partial class Boolean
        {
            /// <summary>
            /// A container of subdivisions used for Boolean Reflow operations.
            /// 
            /// When performing a reflow operation, nodes are "diced" at collision
            /// locations with new nodes inserted that can have their topology 
            /// redirected to create the new path. This is a collection of that
            /// dicing and cached history of what the topology looked like before
            /// the dicing.
            /// </summary>
            public class SplitInfo
            {
                /// <summary>
                /// The type of split being referenced at a queried location.
                /// </summary>
                public enum SplitResult
                {
                    /// <summary>
                    /// No dicing occured.
                    /// </summary>
                    None,

                    /// <summary>
                    /// The split happened exactly where queried.
                    /// </summary>
                    OnBoundary,

                    /// <summary>
                    /// The queried location is past the split.
                    /// </summary>
                    End,

                    /// <summary>
                    /// Only one split occured.
                    /// </summary>
                    OnlyOne,

                    /// <summary>
                    /// The location is between two diced locations.
                    /// </summary>
                    Between
                }

                /// <summary>
                /// The original previous reference of node, before being diced.
                /// </summary>
                public readonly BNode origPrev;

                /// <summary>
                /// The original next reference of node, before being diced.
                /// </summary>
                public readonly BNode origNext;

                /// <summary>
                /// The original node that the SplitInfo is referring to.
                /// </summary>
                public readonly BNode node;

                /// <summary>
                /// The splits for the dicing in the middle.
                /// </summary>
                public List<Utils.NodeTPos> splits = new List<Utils.NodeTPos>();

                /// <summary>
                /// Constructor.
                /// </summary>
                /// <param name="node">The original node.</param>
                public SplitInfo(BNode node)
                {
                    this.node = node;

                    splits.Add(new Utils.NodeTPos(node, 0.0f));
                    this.origPrev = node.prev;
                    this.origNext = node.next;
                }

                /// <summary>
                /// Get the node relevant for a specific location on the line 
                /// segment's original path.
                /// </summary>
                /// <param name="t">The location on the original path to query.</param>
                /// <param name="left">The diced node left of the location queried.</param>
                /// <param name="right">The diced node right of the location queried.</param>
                /// <returns>Information on the diced state and the output parameters.</returns>
                public SplitResult GetNode(float t, out Utils.NodeTPos left, out Utils.NodeTPos right)
                {
                    if (t < 0.0f || t > 1.0f)
                    {
                        left = new Utils.NodeTPos(null, t);
                        right = new Utils.NodeTPos(null, t);
                        return SplitResult.OnBoundary;
                    }

                    if (splits.Count < 2)
                    {
                        left = this.splits[0];
                        right = this.splits[0];
                        return SplitResult.OnlyOne;
                    }

                    for (int i = 0; i < splits.Count; ++i)
                    {
                        if (splits[i].t == t)
                        {
                            left = this.splits[i];
                            right = this.splits[i];
                            return SplitResult.OnBoundary;
                        }
                        else if (this.splits[i].t > t)
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

                /// <summary>
                /// Add a diced entry.
                /// </summary>
                /// <param name="t">The location to add the entry.</param>
                /// <param name="node">The node to add for the entry.</param>
                /// <returns>True if successful. Else, false. Adding an entry will fail
                /// if the t location is out of bounds or already claimed.</returns>
                public bool AddEntry(float t, BNode node)
                {
                    if (t < 0.0f || t > 1.0f)
                        return false;

                    // It needs to be an ordered insertion.
                    for (int i = 0; i < this.splits.Count; ++i)
                    {
                        // Collisions not allowed!
                        if (this.splits[i].t == t)
                            return false;

                        if (this.splits[i].t < t)
                            continue;

                        this.splits.Insert(i, new Utils.NodeTPos(node, t));
                        return true;
                    }

                    this.splits.Add(new Utils.NodeTPos(node, t));
                    return true;
                }

                /// <summary>
                /// Add a diced entry.
                /// </summary>
                /// <param name="ntp">The entry information.</param>
                /// <returns>True if successful. Else, false. Adding an entry will fail
                /// if the t location is out of bounds or already claimed.</returns>
                public bool AddEntry(Utils.NodeTPos ntp)
                {
                    return this.AddEntry(ntp.t, ntp.node);
                }
            }
        }
    }
}