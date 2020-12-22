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
        public static class Text
        { 
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

                    BShape shapeLetter = new BShape(Vector2.zero, 0.0f);
                    shapeLetter.layer = l;
                    ret.Add(shapeLetter);
                    l.shapes.Add(shapeLetter);

                    for (int j = 0; j < g.contours.Count; ++j)
                    {
                        BLoop loopCont = new BLoop(shapeLetter);

                        //https://stackoverflow.com/questions/20733790/truetype-fonts-glyph-are-made-of-quadratic-bezier-why-do-more-than-one-consecu
                        Font.Contour cont = g.contours[j];

                        for (int k = 0; k < cont.points.Count - 1; ++k)
                        {
                            // If two control points are next to each other, there's an implied
                            // point in between them at their average. The easiest way to handle
                            // that is to make a representation where we manually inserted them
                            // to define them explicitly.
                            //
                            // NOTE: We should probably just directly "sanitize" this information
                            // when it's first loaded.
                            if (cont.points[k].control == true && cont.points[k + 1].control == true)
                            {
                                Font.Point pt = new Font.Point();
                                pt.control = false;
                                pt.position = (cont.points[k].position + cont.points[k + 1].position) * 0.5f;

                                cont.points.Insert(k + 1, pt);
                                ++k;
                            }
                        }

                        BNode firstNode = null;
                        BNode prevNode = null;
                        Vector2? lastTan = null;

                        // Point are now either points, or curve controls surrounded by points.
                        for (int k = 0; k < cont.points.Count; ++k)
                        {
                            Vector2 ptpos = cont.points[k].position * scale + pos;

                            if (cont.points[k].control == false)
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

                                if (k != 0 && cont.points[k - 1].control == false)
                                {
                                    prevNode.UseTanOut = false;
                                    node.UseTanIn = false;
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
                                cont.points[0].control == false &&
                                cont.points[cont.points.Count - 1].control == false)
                            {
                                firstNode.UseTanIn = false;
                                prevNode.UseTanOut = false;
                            }
                        }
                    }

                    pos.x += g.advance * scale;
                }

                return ret;
            }
        }
    }
}