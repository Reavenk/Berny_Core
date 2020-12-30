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
        /// Utility library to turn Berny fonts and text into proper vectorized Berny shapes.
        /// </summary>
        public static class Text
        {
            /// <summary>
            /// Given a string and a typeface, create the vector shapes for them.
            /// </summary>
            /// <param name="l">The layer to create the glyph shapes in.</param>
            /// <param name="offset">The location where the glyphs will be created. Represents the
            /// baseline position for the start of the string.</param>
            /// <param name="font">The font to create.</param>
            /// <param name="scale">A multiplier scale on the created geometry.</param>
            /// <param name="strToGen">The string to generate.</param>
            /// <returns>A 1-1 mapping between the string chars and the generated shapes. For missing glyphs, the entry will
            /// either be an empty glyph or null.</returns>
            public static List<BShape> GenerateString(
                Layer l,
                Vector2 offset,
                Font.Typeface font,
                float scale,
                string strToGen)
            {
                Vector2 pos = offset;
                List<BShape> ret = new List<BShape>();

                const float normLineHeight = 1.0f;

                // For each shape, generate the geometry.
                for (int i = 0; i < strToGen.Length; ++i)
                {
                    char c = strToGen[i];
                    Font.Glyph g;

                    if (c == '\n')
                    {
                        pos.x = offset.x;
                        pos.y += 1.0f * scale * normLineHeight;
                        ret.Add(null);
                        continue;
                    }

                    if (font.glyphLookup.TryGetValue(c, out g) == false)
                    {
                        ret.Add(null);
                        continue;
                    }

                    BShape glyphShape = GenerateGlyph(g, l, pos, scale);
                    ret.Add(glyphShape);

                    pos.x += g.advance * scale;
                }

                return ret;
            }

            /// <summary>
            /// Given a font glyph, generate a formal path for it.
            /// </summary>
            /// <param name="glyph">The glyph to turn into a path.</param>
            /// <param name="l">The layer to create the shape in.</param>
            /// <param name="offset">The offset to translate the glyph.</param>
            /// <param name="scale">The scale of the glyph, with 1.0 being the
            /// default size.</param>
            /// <returns>The created path. If the glyph has multiple
            /// contours, they will be created as individual loops.</returns>
            public static BShape GenerateGlyph(
                Font.Glyph glyph,
                Layer l,
                Vector2 offset,
                float scale)
            {
                BShape shapeLetter = new BShape(Vector2.zero, 0.0f);
                shapeLetter.layer = l;

                if (l != null)
                    l.shapes.Add(shapeLetter);

                // Generate each contour in the glyph. When we're iterating through the glyph points,
                // we need to remember we're dealing with two possible conventions at once - TTF/OTF and
                // CFF.
                //
                // Remember TTF/OTF uses quadratic Beziers and the control flags.
                //
                // While CCF uses cubic Beziers and the point tangents and tangent flags.
                for (int j = 0; j < glyph.contours.Count; ++j)
                {
                    BLoop loopCont = new BLoop(shapeLetter);

                    //https://stackoverflow.com/questions/20733790/truetype-fonts-glyph-are-made-of-quadratic-bezier-why-do-more-than-one-consecu
                    Font.Contour cont = glyph.contours[j];

                    for (int k = 0; k < cont.points.Count - 1; ++k)
                    {
                        // If two control points are next to each other, there's an implied
                        // point in between them at their average. The easiest way to handle
                        // that is to make a representation where we manually inserted them
                        // to define them explicitly.
                        //
                        // NOTE: We should probably just directly "sanitize" this information
                        // when it's first loaded.
                        if (cont.points[k].isControl == true && cont.points[k + 1].isControl == true)
                        {
                            Font.Point pt = new Font.Point();
                            pt.isControl = false;
                            pt.position = (cont.points[k].position + cont.points[k + 1].position) * 0.5f;

                            // Things that process this data may want to know it's implied, especially
                            // diagnostic tools.
                            pt.implied = true;

                            cont.points.Insert(k + 1, pt);
                            ++k;
                        }
                    }

                    BNode firstNode = null;     // Used to know what to link the last node to when we're done looping.
                    BNode prevNode = null;      // Used to have a record of the last node when we're done looping.
                    Vector2? lastTan = null;    // The last used tangent when dealing with control points.

                    // Point are now either points, or curve controls surrounded by points - 
                    // or it's a CFF and we don't actually care about control points since we have
                    // explicitly defined tangents.
                    //
                    // The code is written to handle both without explicitly knowing which system is being used.
                    for (int k = 0; k < cont.points.Count; ++k)
                    {
                        Vector2 ptpos = cont.points[k].position * scale + offset;

                        if (cont.points[k].isControl == false)
                        {
                            BNode node = new BNode(loopCont, ptpos);
                            loopCont.nodes.Add(node);

                            if (lastTan.HasValue == true)
                            {
                                node.UseTanIn = true;
                                node.TanIn = (lastTan.Value - ptpos) * (2.0f / 3.0f) * scale;
                            }

                            lastTan = null;
                            if (prevNode != null)
                            {
                                node.prev = prevNode;
                                prevNode.next = node;
                            }

                            if (firstNode == null)
                                firstNode = node;

                            if (k != 0 && cont.points[k - 1].isControl == false && cont.points[k - 1].useTangentOut == false)
                            {
                                prevNode.UseTanOut = false;
                                node.UseTanIn = false;
                            }

                            if (cont.points[k].useTangentIn == true)
                            {
                                node.UseTanIn = true;
                                node.TanIn = cont.points[k].tangentIn;
                            }

                            if (cont.points[k].useTangentOut == true)
                            {
                                node.UseTanOut = true;
                                node.TanOut = cont.points[k].tangentOut;
                            }

                            node.FlagDirty();
                            prevNode = node;
                        }
                        else // if (cont.points[k].control == true)
                        {
                            lastTan = ptpos;

                            if (prevNode != null)
                            {
                                prevNode.UseTanOut = true;
                                prevNode.TanOut = (ptpos - prevNode.Pos) * (2.0f / 3.0f) * scale;
                            }

                        }
                    }

                    if (firstNode != null)
                    {
                        prevNode.next = firstNode;
                        firstNode.prev = prevNode;

                        if (
                            cont.points[0].isControl == false &&
                            cont.points[0].useTangentIn == false &&
                            cont.points[cont.points.Count - 1].isControl == false &&
                            cont.points[cont.points.Count - 1].useTangentOut == false)
                        {
                            firstNode.UseTanIn = false;
                            prevNode.UseTanOut = false;
                        }
                    }
                }

                return shapeLetter;
            }

            /// <summary>
            /// Given a BShape that was created from a font glyph, modify as needed so that
            /// it can be immediately filled properly.
            /// </summary>
            /// <param name="shape"></param>
            public static void BridgeGlyph(BShape shape)
            { 
                List<BNode> posIslands = new List<BNode>();
                List<BNode> negIslands = new List<BNode>();

                foreach(BLoop bl in shape.loops)
                { 
                    List<BNode> islands = bl.GetIslands();
                    foreach(BNode isl in islands)
                    { 


                        List<BNode> islSegs = new List<BNode>(isl.Travel());
                        float f = BNode.CalculateWinding(islSegs);
                        if(f < 0.0)
                            posIslands.Add(isl);
                        else
                            negIslands.Add(isl);
                    }
                }

                for(int p = 0; p < posIslands.Count; ++p)
                { 
                    for(int n = 0; n < negIslands.Count; ++n)
                    {
                        List<BNode> posIslSegs = new List<BNode>(posIslands[p].Travel());
                        List<BNode> negIslSegs = new List<BNode>(negIslands[n].Travel());

                        Boolean.BoundingMode bm = Boolean.Difference(posIslSegs[0].parent, posIslSegs, negIslSegs, false);
                        if( bm == Boolean.BoundingMode.Collision)
                        { 
                            // If there's a collision, the positive loops is modified and items will be clipped.
                            //
                            // This solution isn't completely robust because it's possible every node
                            // of the original island is clipped.
                            for(int i = 0; i < posIslSegs.Count; ++i)
                            { 
                                if(posIslSegs[i].next != null && posIslSegs[i].prev != null)
                                { 
                                    posIslands[p] = posIslSegs[i];
                                    break;
                                }
                            }
                        }
                        else if( bm == Boolean.BoundingMode.LeftSurroundsRight)
                        { 
                            // If the positive fully wraps the negative, bridge it.
                            Dictionary<BNode, BNode> dmap = BNode.CloneNodes(negIslSegs, false);
                            List<BNode> negClone = new List<BNode>();
                            foreach(BNode b in negIslSegs)
                            { 
                                BNode cl = dmap[b];
                                cl.SetParent(posIslSegs[0].parent);
                                negClone.Add(cl);
                            }

                            BNode outer;
                            BNode inner;
                            float outT;
                            float inT;
                            BNode.FindBridge(posIslSegs, negClone, out inner, out outer, out inT, out outT);
                            if(outer != null)
                                BNode.MakeBridge(inner, inT, outer, outT);
                        }
                    }
                }

                // Subtract negative nodes from any positive nodes it affects.

                // Get rid of negative nodes, we made sure their "cavity-ness" 
                // was applied in a way it will show when we tessellate the
                // shape.
                foreach(BNode bn in negIslands)
                    bn.RemoveIsland(false);

                shape.CleanEmptyLoops();
            }
        }
    }
}