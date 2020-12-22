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
        /// <summary>
        /// A class used to hold information and processes for filling in the contents of a path with triangle geometry.
        /// 
        /// In order to perform tesselation, a linked list of points are needed, similar to the BNode, but that list
        /// deals with explicit segments, similar to the BNode segments. But this linked list path is modified chipped away
        /// in the process while performing ear cutting algorithm.
        /// </summary>
        public class FillSession
        {
            /// <summary>
            /// The islands to process filling geometry for. These 
            /// </summary>
            /// <remarks>XORing internals is not finished or tested.</remarks>
            public HashSet<FillIsland> islands = new HashSet<FillIsland>();

            /// <summary>
            /// Clear all contents of the fill session.
            /// </summary>
            public void Clear()
            { 
                // Forget everything
                this.islands.Clear();
            }

            public FillSession Clone()
            { 
                FillSession ret = new FillSession();

                foreach(FillIsland fi in this.islands)
                    ret.islands.Add(fi.Clone());

                return ret;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="shape"></param>
            /// <returns></returns>
            public int ExtractFillLoops(BShape shape)
            { 
                int ret = 0;

                List<BNode> islandNodes = new List<BNode>();

                // For all loops in the shape, extract a single peice of each 
                // unique circular island.
                foreach(BLoop loop in shape.loops)
                {
                    HashSet<BNode> toScan = new HashSet<BNode>(loop.nodes);

                    while (toScan.Count > 0)
                    { 
                        BNode bn = Utils.GetFirstInHash(toScan);

                        BNode.EndpointQuery eq = bn.GetPathLeftmost();

                        // If cyclical, save a single node to scan the island later
                        if(eq.result == BNode.EndpointResult.Cyclical)
                            islandNodes.Add(bn);
                        
                        // And remove every part of it from what we're going to
                        //scan in the future.
                        BNode it = bn;
                        while(true)
                        {
                            toScan.Remove(it);

                            it = it.next;
                            if(it == null || it == eq.node)
                                break;
                        }
                    }
                }

                // Extra islands from the looped nodes we've collected.
                foreach(BNode bisln in islandNodes)
                { 
                    BSample bstart = bisln.sample;
                    BSample bit = bstart;

                    FillIsland island = new FillIsland();
                    this.islands.Add(island);
                    ++ret;

                    FillSegment firstSeg = null;
                    FillSegment lastSeg = null;

                    // For now we're going to assume the samples are well formed.
                    while(true)
                    { 
                        FillSegment fs = new FillSegment();

                        // Transfer positions. We keep a local copy of positions, but by convention,
                        // the prevPos will match the prev's nextPos, and the nextPos will match
                        // the next's prevPos.
                        fs.pos = bit.pos;

                        island.segments.Add(fs);

                        if (firstSeg == null)
                            firstSeg = fs;
                        else
                        {
                            lastSeg.next = fs;
                            fs.prev = lastSeg;
                        }

                        lastSeg = fs;

                        bit = bit.next;
                        if(bit == bstart)
                            break;
                    }
                    lastSeg.next = firstSeg;
                    firstSeg.prev = lastSeg;

                    // Delme
                    if(island.TestValidity() == false)
                        throw new System.Exception("FillSession.ExtractFillLoops produced invalid island.");
                }


                return ret;

            }
             
            /// <summary>
            /// 
            /// </summary>
            /// <param name="island"></param>
            /// <returns></returns>
            public int SanatizeIslandIntersections(FillIsland island)
            {
                if(Utils.verboseDebug )
                {
                    //island.DumpDebugCSV("SanatizeIslandIntersections");
                    if (island.TestValidity() == false)
                        throw new System.Exception("Error in FillSession.SanitizeIslandIntersections: invalid island parameter.");
                }

                int ret = 0;

                HashSet<FillIsland> newIslandsToCheck = new HashSet<FillIsland>();

                FillSegment fs = island.GetAStartingPoint();
                FillSegment it = fs;
                while(true)
                { 
                    // We only need to check everything ahead of us back the beginning. Checking
                    // collisions against stuff we've already traveled over is redundant.

                    for(FillSegment itOther = it.next; itOther != it; itOther = itOther.next)
                    { 
                        // Neighboring items detect very slight collision, plus we 
                        // know they can't collide. While in theory we could start itOther one
                        // more advancement, I'll pass on that for now.
                        if(itOther.prev == it || itOther.next == it)
                            continue;

                        //island.DumpDebugCSV("SanatizeIslandIntersections_Before");
                        float s,t;
                        if( Utils.ProjectSegmentToSegment(
                                it.pos, 
                                it.next.pos, 
                                itOther.pos, 
                                itOther.next.pos, 
                                out s, 
                                out t) == false)
                        { 
                            continue;
                        }

                        if(s < 0.0f || s > 1.0f || t < 0.0f || t > 1.0f)
                            continue;

                        //island.DumpDebugCSV("SanatizeIslandIntersection_After");

                        // When a collision happens, we need to split them into two islands 
                        // at the collision point

                        Vector2 colPt = it.pos + s * (it.next.pos - it.pos);

                        // First we create the point and set some references
                        // Stitch "us"
                        FillSegment newUsStitch = new FillSegment();
                        newUsStitch.pos = colPt;
                        newUsStitch.prev = it;
                        newUsStitch.next = itOther.next;
                        //
                        // And stitch "them",
                        FillSegment newThemStitch = new FillSegment();
                        newThemStitch.pos = colPt;
                        newThemStitch.prev = itOther;
                        newThemStitch.next = it.next;

                        // And then we patch it all up
                        it.next = newUsStitch;
                        newUsStitch.next.prev = newUsStitch;

                        itOther.next = newThemStitch;
                        newThemStitch.next.prev = itOther.next;

                        island.segments.Add(newUsStitch);

                        // and move them to another island 
                        FillIsland newIsland = new FillIsland();
                        this.islands.Add(newIsland);

                        newIsland.segments.Add(itOther);
                        for(FillSegment sgIt = itOther.next;  sgIt != itOther; sgIt = sgIt.next)
                        {
                            island.segments.Remove(sgIt);
                            newIsland.segments.Add(sgIt);
                        }
                    }
                    

                    it = it.next;
                    // Once we've gone full circle, we've check everything
                    if(it ==  fs)
                        break;
                }

                foreach(FillIsland fi in newIslandsToCheck)
                    ret += SanatizeIslandIntersections(fi);

                return ret;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="triangles"></param>
            /// <param name="vectors"></param>
            /// <param name="sanatizeIsland"></param>
            /// <param name="consume"></param>
            public void GetTriangles(List<int> triangles, Vector2Repo vectors, bool sanatizeIsland, bool consume = false)
            {
                if(sanatizeIsland == true)
                { 
                    // If an island starts splitting, it will add to this.islands, so we can't
                    // iterate over it - so we make a copy.
                    HashSet<FillIsland> originalIslands = new HashSet<FillIsland>(this.islands);

                    //foreach(FillIsland fi in originalIslands)
                    //    this.SanatizeIslandIntersections(fi);
                }

                foreach(FillIsland fi in this.islands)
                    fi.GetTriangles(triangles, vectors, consume);
            }

        }
    }
}
