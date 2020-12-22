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
        public static class Operators
        { 
            public static List<BShape> CreateText(Font.Typeface typeface, Vector2 startPos, float scale, Layer layer, string str)
            { 
                List<BShape> lst = new List<BShape>();

                return lst;
            }

            public static void Edgify(BLoop loop, float pushOut, float pullIn = 0.0f)
            {
                if(pushOut == 0.0f && pullIn == 0.0f)
                    return;

                List<BNode> islands = loop.GetIslands();

                foreach (BNode bisl in islands)
                {
                    // This will probably just give us bisl back, but it that's the case, then it should
                    // be minimal overhead - just to be safe though, and to see what kind of connectivity we're dealing with.
                    BNode.EndpointQuery eq = bisl.GetPathLeftmost();

                    List<BNode> origs = new List<BNode>();
                    List<BNode> copies = new List<BNode>();
                    List<InflationCache> inflations = new List<InflationCache>();
                    foreach (BNode it in eq.Enumerate())
                    {
                        origs.Add(it);

                        BNode cpy = new BNode(loop, it, false, true);
                        copies.Add(cpy);

                        loop.nodes.Add(cpy);

                        InflationCache ic = new InflationCache();
                        it.GetInflateDirection(out ic.selfInf, out ic.inInf, out ic.outInf);
                        inflations.Add(ic);
                    }

                    // Stitch the new chain - it should have a reverse winding.
                    //
                    // The loop is a little backwards, but basically we sub instead of add to
                    // treat the prev item in the array like the next in the chain.
                    for (int i = 1; i < copies.Count; ++i)
                    {
                        copies[i].next = copies[i - 1];
                        copies[i - 1].prev = copies[i];
                    }

                    int lastIdx = copies.Count - 1;
                    if (eq.result == BNode.EndpointResult.Cyclical)
                    {
                        // If it was cyclical, it should close in on itself and it should
                        // never touch the original outline;
                        //
                        // Remember we're treating copies in reverse.
                        copies[lastIdx].prev = copies[0];
                        copies[0].next = copies[lastIdx];
                    }
                    else
                    {
                        // Or else the opposite ends connect to each other.
                        // Remember we're treating copies in reverse.
                        origs[0].prev = copies[0];
                        copies[0].next = origs[0];

                        origs[lastIdx].next = copies[lastIdx];
                        copies[lastIdx].prev = origs[lastIdx];

                        origs[0].UseTanIn = false;
                        origs[lastIdx].UseTanOut = false;
                        copies[0].UseTanOut = false;
                        copies[lastIdx].UseTanIn = false;
                    }

                    if(pushOut != 0.0f)
                    {
                        // Now that we have copies and connectivity set up, it's time
                        // to apply the thickening
                        for (int i = 0; i < origs.Count; ++i)
                        {
                            // Push out the original
                            origs[i].Pos += pushOut * inflations[i].selfInf;
                            origs[i].TanIn += pushOut * (inflations[i].inInf - inflations[i].selfInf);
                            origs[i].TanOut += pushOut * (inflations[i].outInf - inflations[i].selfInf);

                        }
                    }

                    if(pullIn != 0.0f)
                    {
                        // We can optionally pull in the copy
                        for (int i = 0; i < copies.Count; ++i)
                        {
                            copies[i].Pos += pullIn * inflations[i].selfInf;
                            copies[i].TanIn += pullIn * (inflations[i].inInf - inflations[i].selfInf);
                            copies[i].TanOut += pullIn * (inflations[i].outInf - inflations[i].selfInf);
                        }
                    }
                }
            }
        }
    }
}