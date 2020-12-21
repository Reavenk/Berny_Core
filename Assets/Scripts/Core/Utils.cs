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
        /// Misc util functions for math and other shared functionality.
        /// </summary>
        public static class Utils
        {
            /// <summary>
            /// Currently unused, the blend types supported in SVG.
            /// </summary>
            public enum BlendMode
            { 
                Normal,
                Multiply,
                Screen,
                Darken,
                Lighten,
                Overlay,
                ColorDodge,
                ColorBurn,
                HardLight,
                SoftLight,
                Difference,
                Exclusion,
                Hue,
                Saturation,
                Color,
                Luminosity
            }

            /// <summary>
            /// The unit type supported in SVG
            /// </summary>
            public enum LengthUnit
            { 
                Unlabled,
                Meters, // Might not actually be a supported type
                Centimeters,
                Millimeters,
                Inches,
                Points,
                Picas,
                Pixels
            }

            /// <summary>
            /// A pairing of a BNode and an interpolation point in that node.
            /// </summary>
            public struct NodeTPos
            {
                /// <summary>
                /// The node.
                /// </summary>
                public BNode node;

                /// <summary>
                /// The interpolation point on the node.
                /// </summary>
                public float t;

                /// <summary>
                /// Constructor.
                /// </summary>
                /// <param name="node">The node.</param>
                /// <param name="t">The interpolation point on the node.</param>
                public NodeTPos(BNode node, float t)
                {
                    this.node = node;
                    this.t = t;
                }
            }

            /// <summary>
            /// Return value recording an found intersection of two Bezier path segments from iterative subdivision
            /// </summary>
            public struct BezierSubdivSample
            {
                // A node who's a part of the intersection.
                public BNode nodeA;
            #if BWINDOW
                public float lA0;       // Lower t of A of the final subdivided window.
                public float lA1;       // Higher t of A of the final subdivided window.
            #endif
                public float lAEst;     // The final estimated t value of A for the intersection.
                public bool linearA;

                // Another node who's part of the intersection
                public BNode nodeB;     
            #if BWINDOW
                public float lB0;       // Lower t of B of the final subdivided window.
                public float lB1;       // Higher t of B of the final subdivided window.
            #endif
                public float lBEst;     // The final estimated t value of B for the intersection.
                public bool linearB;

                /// <summary>
                /// Constructor
                /// </summary>
                /// <param name="A"></param>
                /// <param name="AEst"></param>
                /// <param name="B"></param>
                /// <param name="bEst"></param>
                public BezierSubdivSample(BezierSubdivRgn A, float AEst, BezierSubdivRgn B, float bEst)
                { 
                    this.nodeA = A.node;
#if BWINDOW
                    this.lA0 = A.lambda0;
                    this.lA1 = A.lambda1;
#endif
                    this.lAEst = AEst;
                    this.linearA = false;


                    this.nodeB = B.node;
#if BWINDOW
                    this.lB0 = B.lambda0;
                    this.lB1 = B.lambda1;
#endif
                    this.lBEst = bEst;
                    this.linearB = false;
                }

                /// <summary>
                /// 
                /// </summary>
                /// <param name="lst"></param>
                /// <param name="eps"></param>
                public static void CleanIntersectionList(List<Utils.BezierSubdivSample> lst, float eps = 0.00001f)
                {
                    for (int i = 0; i < lst.Count; ++i)
                    {
                        // Note how we're starting from the end going back next to i
                        for (int j = lst.Count - 1; j > i; --j)
                        {
                            // If they're different "enough" somehow, let it pass
                            // and move on.
                            if (lst[i].nodeA != lst[j].nodeA || lst[i].nodeB != lst[j].nodeB)
                                continue;

                            if (Mathf.Abs(lst[i].lAEst - lst[j].lAEst) > eps)
                                continue;

                            if (Mathf.Abs(lst[i].lBEst - lst[j].lBEst) > eps)
                                continue;

                            // Or else, they're too similar
                            lst.RemoveAt(j);
                        }
                    }
                }

                /// <summary>
                /// Get the A side node and interpolation as a NodeTPos.
                /// </summary>
                /// <returns>The A side as a NodeTPos.</returns>
                public NodeTPos GetTPosA() => new NodeTPos(this.nodeA, this.lAEst);

                /// <summary>
                /// Get the B side node and interpolation as a NodeTPos.
                /// </summary>
                /// <returns>The B side as a NodeTPos.</returns>
                public NodeTPos GetTPosB() => new NodeTPos(this.nodeB, this.lBEst);
            }

            /// <summary>
            /// The window when subdividing a Bezier for intersection.
            /// </summary>
            public struct BezierSubdivRgn
            { 
                /// <summary>
                /// 
                /// </summary>
                public BNode node;

                /// <summary>
                /// The starting point.
                /// </summary>
                public Vector2 pt0;

                /// <summary>
                /// The starting point's control.
                /// </summary>
                public Vector2 pt1;

                /// <summary>
                /// The ending point's control
                /// </summary>
                public Vector2 pt2;

                /// <summary>
                /// The ending point.
                /// </summary>
                public Vector2 pt3;

                /// <summary>
                /// The lower lambda of the window.
                /// </summary>
                public float lambda0;

                /// <summary>
                /// The upper lambda of the window.
                /// </summary>
                public float lambda1;

                /// <summary>
                /// The bounds of the window.
                /// </summary>
                public BoundsMM2 bounds;

                /// <summary>
                /// Split the window by taking the mid interpolation point (lambda*).
                /// </summary>
                /// <param name="l">The left side of the subdivide.</param>
                /// <param name="r">The right side of the subdivide.</param>
                public void Split(out BezierSubdivRgn l, out BezierSubdivRgn r)
                { 
                    float lambdaM = (lambda0 + lambda1) * 0.5f;

                    l = new BezierSubdivRgn();
                    l.node = this.node;
                    l.pt0 = this.pt0;
                    l.pt1 = this.pt1;
                    l.pt2 = this.pt2;
                    l.pt3 = this.pt3;
                    l.lambda0 = this.lambda0;
                    l.lambda1 = lambdaM;
                    l.CalculateBounds();

                    r = new BezierSubdivRgn();
                    r.node = this.node;
                    r.pt0 = this.pt0;
                    r.pt1 = this.pt1;
                    r.pt2 = this.pt2;
                    r.pt3 = this.pt3;
                    r.lambda0 = lambdaM;
                    r.lambda1 = this.lambda1;
                    r.CalculateBounds();
                }

                /// <summary>
                /// Calculate the bound of the window.
                /// </summary>
                public void CalculateBounds()
                {
                    Vector2 sdpt0, sdpt1, sdpt2, sdpt3;
                    
                    Utils.SubdivideBezier(
                        this.pt0, this.pt1, this.pt2, this.pt3, 
                        out sdpt0, out sdpt1, out sdpt2, out sdpt3, 
                        this.lambda0, this.lambda1);
                    
                    this.bounds = GetBoundingBoxCubic(sdpt0, sdpt1, sdpt2, sdpt3);
                }

                /// <summary>
                /// 
                /// </summary>
                /// <param name="node"></param>
                /// <returns></returns>
                public static BezierSubdivRgn FromNode(BNode node)
                {
                    Utils.BezierSubdivRgn ret = new Utils.BezierSubdivRgn();
                    ret.node = node;
                    ret.lambda0 = 0.0f;
                    ret.lambda1 = 1.0f;
                    ret.pt0 = node.Pos;
                    ret.pt1 = node.Pos + node.TanOut;
                    ret.pt2 = node.next.Pos + node.next.TanIn;
                    ret.pt3 = node.next.Pos;
                    ret.CalculateBounds();

                    return ret;
                }

                /// <summary>
                /// 
                /// </summary>
                /// <param name="node"></param>
                /// <param name="leftLam"></param>
                /// <param name="rightLam"></param>
                /// <returns></returns>
                public static BezierSubdivRgn FromNode(BNode node, float leftLam, float rightLam)
                {
                    Utils.BezierSubdivRgn ret = new Utils.BezierSubdivRgn();
                    ret.node = node;
                    ret.lambda0 = leftLam;
                    ret.lambda1 = rightLam;
                    ret.pt0 = node.Pos;
                    ret.pt1 = node.Pos + node.TanOut;
                    ret.pt2 = node.next.Pos + node.next.TanIn;
                    ret.pt3 = node.next.Pos;
                    ret.CalculateBounds();

                    return ret;
                }
            }

            // For some things, we may want to check against a bool if debugging utilities should be turned
            // on instead of relying on preprocessors. If the compiler is savvy enough - which is most 
            // definitely should be, it will optimized out if statements with a const false value.
            public const bool verboseDebug =
#if DEVELOPMENT_BUILD || UNITY_EDITOR
                true;
#else
                false;
#endif

            /// <summary>
            /// Given a t value, figure out the Bezier weight at that interpolation point.
            /// Weights were taken from https://blackpawn.com/texts/splines/
            /// </summary>
            /// <param name="t">Interpolation location, a value between [0.0, 1.0]</param>
            /// <param name="A">The linear combination weight for the starting point.</param>
            /// <param name="B">The linear combination weight for the starting point's control.</param>
            /// <param name="C">The linear combination weight for the end point's control.</param>
            /// <param name="D">The linear combination weight for the end point.</param>
            public static void GetBezierWeights(float t, out float A, out float B, out float C, out float D)
            { 
                float t2 = t * t;
                float t3 = t2 * t;
                A = -t3 + 3.0f * t2 - 3.0f * t + 1.0f;
                B = 3.0f * t3 - 6.0f * t2 + 3.0f * t;
                C = -3.0f * t3 + 3.0f * t2;
                D = t3;
            }

            /// <summary>
            /// The derivatives of the Bezier matrix.
            /// 
            /// It's basically the power rule applied to the values for GetBezierWeights().
            /// </summary>
            /// <param name="t">Interpolation location, a value between [0.0, 1.0]</param>
            /// <param name="A">The linear combination weight for the starting point.</param>
            /// <param name="B">The linear combination weight for the starting pont's tangent</param>
            /// <param name="C">The linear combination weight for the end point's tangent.</param>
            /// <param name="D">The linear combination weight for the end point.</param>
            public static void GetBezierDerivativeWeights(float t, out float A, out float B, out float C, out float D)
            { 
                float t2 = t* t;
                A = (-3 * t2 + 6 * t - 3)/6.0f;
                B = (9 * t2 - 12 * t + 3)/6.0f;
                C = (-9 * t2 + 6 * t)/6.0f;
                D = (3 * t2)/6.0f;
            }

            /// <summary>
            /// Bezier subdivision. 
            /// If all the features of the functions are used, the Bezier segment 
            /// can be cut into 3 peices, with the middle one being return back.
            /// </summary>
            /// <param name="p0">Original start position.</param>
            /// <param name="p1">Original start position control.</param>
            /// <param name="p2">Original end position control.</param>
            /// <param name="p3">Original end position.</param>
            /// <param name="op0">The new start pos.</param>
            /// <param name="op1">The new start control pos.</param>
            /// <param name="op2">The new end control pos.</param>
            /// <param name="op3">The new end pos.</param>
            /// <param name="lleft">The cut point from the start. Use 0.0 to keep the start.</param>
            /// <param name="lright">The cutt off point from the end. Use 1.0 to keep the end.</param>
            /// <remarks>Chances are the functionality is too much for normal use - eventually
            /// a specialized left subdivide and right subdivide should be created.</remarks>
            public static void SubdivideBezier(
                Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, 
                out Vector2 op0, out Vector2 op1, out Vector2 op2, out Vector2 op3,
                float lleft, float lright)
            {
                // https://en.wikipedia.org/wiki/De_Casteljau%27s_algorithm
                Vector2 l11 = Vector2.Lerp(p0, p1, lleft);
                Vector2 l12 = Vector2.Lerp(p1, p2, lleft);
                Vector2 l13 = Vector2.Lerp(p2, p3, lleft);
                Vector2 l21 = Vector2.Lerp(l11, l12, lleft);
                Vector2 l22 = Vector2.Lerp(l12, l13, lleft);
                Vector2 l3 = Vector2.Lerp(l21, l22, lleft);
                op0 = l3;
                op1 = l22;
                op2 = l13;

                // Go from the other side, so we're going to invert
                // The left side changed some values, including the ratio of how much the
                // right is compared to the enture curve of what's left.
                float invr = (1.0f - lright) / (1.0f - lleft);
                Vector2 r11 = Vector2.Lerp(p3, op2, invr);// Reversed from left version
                Vector2 r12 = Vector2.Lerp(op2, l22, invr);// Reversed from left version
                Vector2 r13 = Vector2.Lerp(l22, l3, invr);// Reversed from left version
                Vector2 r21 = Vector2.Lerp(r11, r12, invr);
                Vector2 r22 = Vector2.Lerp(r12, r13, invr);
                Vector2 r3 = Vector2.Lerp(r21, r22, invr);
                op3 = r3;
                op2 = r22;
                op1 = r13;
            }

            /// <summary>
            /// Used for debugging, every time a counted object needs an ID, they can
            /// pull from this universal counter.
            /// </summary>
            /// <returns></returns>
            public static int RegisterCounter()
            { 
                ++ctr;
                return ctr;
            }
            static int ctr = 0;

            /// <summary>
            /// Find all nodes in a set of nodes that aren't the end of
            /// a node chain.
            /// </summary>
            /// <param name="set">The collection of nodes to scan through.</param>
            /// <returns>An enumerator of nodes that have a path segment.</returns>
            public static IEnumerable<BNode> FindSelectedEdges(HashSet<BNode> set)
            { 
                foreach(BNode bn in set)
                { 
                    if(bn.next == null)
                        continue;

                    if(set.Contains(bn.next) == true)
                        yield return bn;
                }
            }

            /// <summary>
            /// Get the first value in a HashSet.
            /// 
            /// The set doesn't natively have that functionality, only enumerating.
            /// So we enumerate and instantly return the value we find.
            /// 
            /// There is a LINQ equivalent, but no reason to bring in LINQ for
            /// such trivilities.
            /// </summary>
            /// <typeparam name="ty">The type of the HashSet.</typeparam>
            /// <param name="hs">The HashSet to pull a value from.</param>
            /// <returns>The first item found in the HashSet.</returns>
            public static ty GetFirstInHash<ty>(HashSet<ty> hs) where ty : class
            { 
                foreach(ty t in hs)
                    return t;

                return null;
            }

            /// <summary>
            /// The point in the triangle to test.
            /// 
            /// Something to note about this implementation is that triangle winding
            /// doesn't matter.
            /// </summary>
            /// <param name="pt">The point to test against.</param>
            /// <param name="a">A point of the triangle.</param>
            /// <param name="b">Another point of the triangle.</param>
            /// <param name="c">Another other point of the triangle.</param>
            /// <returns></returns>
            /// <remarks>Function taken from the book "Realtime Collision Detection."</remarks>
            public static bool PointInTriangle(Vector2 pt, Vector2 a, Vector2 b, Vector2 c)
            { 
                float pab = Vector2Cross( pt - a, b - a);
                float pbc = Vector2Cross(pt - b, c - b);

                if(Mathf.Sign(pab) != Mathf.Sign(pbc))
                    return false;

                float pca = Vector2Cross(pt - c, a - c);

                if(Mathf.Sign(pab) != Mathf.Sign(pca))
                    return false;

                return true;
            }

            /// <summary>
            /// 2D cross section.
            /// </summary>
            /// <param name="a"></param>
            /// <param name="b"></param>
            /// <returns></returns>
            /// <remarks>A positive value will be a clockwise winding.</remarks>
            public static float Vector2Cross(Vector2 a, Vector2 b)
            { 
                // It's the equivalent of pretending a and b are 3D vectors and 
                // only returning the Z value - since the X and Y would be 0.
                //
                // It's basically a value on the Z, but at what magnitude and sign?
                return a.y * b.x - a.x * b.y;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="p1">First point of first segment.</param>
            /// <param name="q1">Second point of first segment.</param>
            /// <param name="p2">First point of second segment.</param>
            /// <param name="q2">Second point of second segment.</param>
            /// <param name="s">t interpolation value of p1-q1 where intersection happened.</param>
            /// <param name="t">t interpolation value of p2-q2 where intersection happened.</param>
            /// <returns></returns>
            /// <remarks>
            /// Taken from ClosestPtSegmentSegment from the book "Realtime Collision Detection."
            /// We're heavily modifying the function to our needs of detecting line intersection.
            /// The biggest change is that s and t are unbounded.
            /// </remarks>
            public static bool ProjectSegmentToSegment(
                Vector2 p1, Vector2 q1, 
                Vector2 p2, Vector2 q2, 
                out float s, out float t)
            { 
                Vector2 d1 = q1 - p1;
                Vector2 d2 = q2 - p2;
                Vector2 r = p1 - p2;
                float a = Vector2.Dot(d1, d1);
                float e = Vector2.Dot(d2, d2);
                float f = Vector2.Dot(d2, r);

                // Check if either or both segments degenerate into points.
                if(a <= Mathf.Epsilon && e <= Mathf.Epsilon)
                { 
                    s = float.PositiveInfinity;
                    t = float.PositiveInfinity;
                    return false;
                }

                if(a <= Mathf.Epsilon)
                { 
                    // First segment degenerates into a point.
                    s = float.PositiveInfinity;
                    t = f / e;
                    return false;
                }
            
                float c = Vector2.Dot(d1, r);
                if(e <= Mathf.Epsilon)
                { 
                    // Second segment degenerates into a point.
                    t = float.PositiveInfinity;
                    s = -c/a;
                    return false;
                }

                float b =  Vector2.Dot(d1, d2);
                float denom = a * e - b * b;

                // If segments not parallel, compute closest point on L1 and L2.
                if(Mathf.Abs(denom) > Mathf.Epsilon)
                    s = (b * f - c * e)/denom;
                else 
                {
                    // Parallel
                    s = float.PositiveInfinity;
                    t = float.PositiveInfinity;
                    return false;
                }

                t = (b * s + f)/e;

                if(t < 0.0f)
                    s = -c/a;
                else if(t > 1.0f)
                    s = (b-c)/a;

                return true;
            }

            /// <summary>
            /// Convert a color to an opaque 6 character hex.
            /// </summary>
            /// <param name="c">The color to convert.</param>
            /// <returns>A 6 character hex representing the color.</returns>
            /// <remarks>If a "#" is needed, note that it is not a part of the output. That
            /// should be additionally prefixed to the result.</remarks>
            public static string ConvertColorToHex6(Color c)
            { 
                return 
                    ((int)Mathf.Clamp(c.r * 255.0f, 0.0f, 255.0f)).ToString("x2") +
                    ((int)Mathf.Clamp(c.g * 255.0f, 0.0f, 255.0f)).ToString("x2") +
                    ((int)Mathf.Clamp(c.b * 255.0f, 0.0f, 255.0f)).ToString("x2");
            }

            /// <summary>
            /// Convert a color to an opaque 8 character hex (to include alpha).
            /// </summary>
            /// <param name="c">The color to convert.</param>
            /// <returns>An 8 character hex representing the color.</returns>
            /// <remarks>If a "#" is needed, note that it is not a part of the output. That
            /// should be additionally prefixed to the result.</remarks>
            public static string ConvertColorToHex8(Color c)
            {
                return
                    ((int)Mathf.Clamp(c.r * 255.0f, 0.0f, 255.0f)).ToString("x2") +
                    ((int)Mathf.Clamp(c.g * 255.0f, 0.0f, 255.0f)).ToString("x2") +
                    ((int)Mathf.Clamp(c.b * 255.0f, 0.0f, 255.0f)).ToString("x2") +
                    ((int)Mathf.Clamp(c.a * 255.0f, 0.0f, 255.0f)).ToString("x2");
            }

            public static void ColorNamesHexMap(
                out Dictionary<string,string> outNameToHex,
                out Dictionary<string, string> outHexToName )
            { 
                https://www.december.com/html/spec/colorsvghex.html
                outNameToHex = new Dictionary<string, string>();
                outHexToName = new Dictionary<string, string>();

                outNameToHex.Add("black",                   "#000000");
                outNameToHex.Add("navy",                    "#000080");
                outNameToHex.Add("darkblue",                "#00008B");
                outNameToHex.Add("mediumblue",              "#0000CD");
 	            outNameToHex.Add("blue",                    "#0000FF");
                outNameToHex.Add("darkgreen",               "#006400");
                outNameToHex.Add("green",                   "#008000");
 	            outNameToHex.Add("teal",                    "#008080");
                outNameToHex.Add("darkcyan",                "#008B8B");
                outNameToHex.Add("deepskyblue",             "#00BFFF");
 	            outNameToHex.Add("darkturquoise",           "#00CED1");
                outNameToHex.Add("mediumspringgreen",       "#00FA9A");	
                outNameToHex.Add("lime",                    "#00FF00");	
 	            outNameToHex.Add("springgreen",             "#00FF7F");		
                outNameToHex.Add("cyan",                    "#00FFFF");	
                outNameToHex.Add("aqua",                    "#00FFFF");	
 	            outNameToHex.Add("midnightblue",            "#191970");		
                outNameToHex.Add("dodgerblue",              "#1E90FF");	
                outNameToHex.Add("lightseagreen",           "#20B2AA");	
 	            outNameToHex.Add("forestgreen",             "#228B22");		
                outNameToHex.Add("seagreen",                "#2E8B57");	
                outNameToHex.Add("darkslategray",           "#2F4F4F");	
 	            outNameToHex.Add("darkslategrey",           "#2F4F4F");		
                outNameToHex.Add("limegreen",               "#32CD32");	
                outNameToHex.Add("mediumseagreen",          "#3CB371");	
 	            outNameToHex.Add("turquoise",               "#40E0D0");		
                outNameToHex.Add("royalblue",               "#4169E1");	
                outNameToHex.Add("steelblue",               "#4682B4");	
 	            outNameToHex.Add("darkslateblue",           "#483D8B");		
                outNameToHex.Add("mediumturquoise",         "#48D1CC");	
                outNameToHex.Add("indigo",                  "#4B0082");	
 	            outNameToHex.Add("darkolivegreen",          "#556B2F");		
                outNameToHex.Add("cadetblue",               "#5F9EA0");	
                outNameToHex.Add("cornflowerblue",          "#6495ED");	
 	            outNameToHex.Add("mediumaquamarine",        "#66CDAA");		
                outNameToHex.Add("dimgrey",                 "#696969");	
                outNameToHex.Add("dimgray",                 "#696969");	
 	            outNameToHex.Add("slateblue",               "#6A5ACD");		
                outNameToHex.Add("olivedrab",               "#6B8E23");	
                outNameToHex.Add("slategrey",               "#708090");	
 	            outNameToHex.Add("slategray",               "#708090");		
                outNameToHex.Add("lightslategray",          "#778899");	
                outNameToHex.Add("lightslategrey",          "#778899");	
 	            outNameToHex.Add("mediumslateblue",         "#7B68EE");		
                outNameToHex.Add("lawngreen",               "#7CFC00");	
                outNameToHex.Add("chartreuse",              "#7FFF00");	
 	            outNameToHex.Add("aquamarine",              "#7FFFD4");		
                outNameToHex.Add("maroon",                  "#800000");	
                outNameToHex.Add("purple",                  "#800080");	
 	            outNameToHex.Add("olive",                   "#808000");		
                outNameToHex.Add("gray",                    "#808080");	
                outNameToHex.Add("grey",                    "#808080");	
 	            outNameToHex.Add("skyblue",                 "#87CEEB");		
                outNameToHex.Add("lightskyblue",            "#87CEFA");	
                outNameToHex.Add("blueviolet",              "#8A2BE2");	
 	            outNameToHex.Add("darkred",                 "#8B0000");
                outNameToHex.Add("darkmagenta",             "#8B008B");
                outNameToHex.Add("saddlebrown",             "#8B4513");
 	            outNameToHex.Add("darkseagreen",            "#8FBC8F");
                outNameToHex.Add("lightgreen",              "#90EE90");
                outNameToHex.Add("mediumpurple",            "#9370DB");
 	            outNameToHex.Add("darkviolet",              "#9400D3");
                outNameToHex.Add("palegreen",               "#98FB98");
                outNameToHex.Add("darkorchid",              "#9932CC");
 	            outNameToHex.Add("yellowgreen",             "#9ACD32");
                outNameToHex.Add("sienna",                  "#A0522D");
                outNameToHex.Add("brown",                   "#A52A2A");
 	            outNameToHex.Add("darkgray",                "#A9A9A9");
                outNameToHex.Add("darkgrey",                "#A9A9A9");
                outNameToHex.Add("lightblue",               "#ADD8E6");
 	            outNameToHex.Add("greenyellow",             "#ADFF2F");
                outNameToHex.Add("paleturquoise",           "#AFEEEE");
                outNameToHex.Add("lightsteelblue",          "#B0C4DE");
 	            outNameToHex.Add("powderblue",              "#B0E0E6");
                outNameToHex.Add("firebrick",               "#B22222");
                outNameToHex.Add("darkgoldenrod",           "#B8860B");
 	            outNameToHex.Add("mediumorchid",            "#BA55D3");	
                outNameToHex.Add("rosybrown",               "#BC8F8F");
                outNameToHex.Add("darkkhaki",               "#BDB76B");
 	            outNameToHex.Add("silver",                  "#C0C0C0");	
                outNameToHex.Add("mediumvioletred",         "#C71585");
                outNameToHex.Add("indianred",               "#CD5C5C");
 	            outNameToHex.Add("peru",                    "#CD853F");	
                outNameToHex.Add("chocolate",               "#D2691E");
                outNameToHex.Add("tan",                     "#D2B48C");
 	            outNameToHex.Add("lightgray",               "#D3D3D3");	
                outNameToHex.Add("lightgrey",               "#D3D3D3");
                outNameToHex.Add("thistle",                 "#D8BFD8");
 	            outNameToHex.Add("orchid",                  "#DA70D6");	
                outNameToHex.Add("goldenrod",               "#DAA520");
                outNameToHex.Add("palevioletred",           "#DB7093");
 	            outNameToHex.Add("crimson",                 "#DC143C");	
                outNameToHex.Add("gainsboro",               "#DCDCDC");
                outNameToHex.Add("plum",                    "#DDA0DD");
 	            outNameToHex.Add("burlywood",               "#DEB887");	
                outNameToHex.Add("lightcyan",               "#E0FFFF");
                outNameToHex.Add("lavender",                "#E6E6FA");
 	            outNameToHex.Add("darksalmon",              "#E9967A");	
                outNameToHex.Add("violet",                  "#EE82EE");
                outNameToHex.Add("palegoldenrod",           "#EEE8AA");
 	            outNameToHex.Add("lightcoral",              "#F08080");	
                outNameToHex.Add("khaki",                   "#F0E68C");
                outNameToHex.Add("aliceblue",               "#F0F8FF");
 	            outNameToHex.Add("honeydew",                "#F0FFF0");	
                outNameToHex.Add("azure",                   "#F0FFFF");
                outNameToHex.Add("sandybrown",              "#F4A460");
 	            outNameToHex.Add("wheat",                   "#F5DEB3");		
                outNameToHex.Add("beige",                   "#F5F5DC");	
                outNameToHex.Add("whitesmoke",              "#F5F5F5");	
 	            outNameToHex.Add("mintcream",               "#F5FFFA");		
                outNameToHex.Add("ghostwhite",              "#F8F8FF");	
                outNameToHex.Add("salmon",                  "#FA8072");	
 	            outNameToHex.Add("antiquewhite",            "#FAEBD7");		
                outNameToHex.Add("linen",                   "#FAF0E6");	
                outNameToHex.Add("lightgoldenrodyellow",    "#FAFAD2");	
 	            outNameToHex.Add("oldlace",                 "#FDF5E6");		
                outNameToHex.Add("red",                     "#FF0000");	
                outNameToHex.Add("fuchsia",                 "#FF00FF");	
 	            outNameToHex.Add("magenta",                 "#FF00FF");		
                outNameToHex.Add("deeppink",                "#FF1493");	
                outNameToHex.Add("orangered",               "#FF4500");	
 	            outNameToHex.Add("tomato",                  "#FF6347");		
                outNameToHex.Add("hotpink",                 "#FF69B4");	
                outNameToHex.Add("coral",                   "#FF7F50");	
 	            outNameToHex.Add("darkorange",              "#FF8C00");		
                outNameToHex.Add("lightsalmon",             "#FFA07A");	
                outNameToHex.Add("orange",                  "#FFA500");	
 	            outNameToHex.Add("lightpink",               "#FFB6C1");		
                outNameToHex.Add("pink",                    "#FFC0CB");	
                outNameToHex.Add("gold",                    "#FFD700");	
 	            outNameToHex.Add("peachpuff",               "#FFDAB9");		
                outNameToHex.Add("navajowhite",             "#FFDEAD");	
                outNameToHex.Add("moccasin",                "#FFE4B5");	
 	            outNameToHex.Add("bisque",                  "#FFE4C4");		
                outNameToHex.Add("mistyrose",               "#FFE4E1");	
                outNameToHex.Add("blanchedalmond",          "#FFEBCD");	
 	            outNameToHex.Add("papayawhip",              "#FFEFD5");		
                outNameToHex.Add("lavenderblush",           "#FFF0F5");	
                outNameToHex.Add("seashell",                "#FFF5EE");	
 	            outNameToHex.Add("cornsilk",                "#FFF8DC");		
                outNameToHex.Add("lemonchiffon",            "#FFFACD");	
                outNameToHex.Add("floralwhite",             "#FFFAF0");	
 	            outNameToHex.Add("snow",                    "#FFFAFA");		
                outNameToHex.Add("yellow",                  "#FFFF00");	
                outNameToHex.Add("lightyellow",             "#FFFFE0");	
 	            outNameToHex.Add("ivory",                   "#FFFFF0");		
                outNameToHex.Add("white",                   "#FFFFFF");

                foreach (KeyValuePair<string, string> kvp in outNameToHex)
                {
                    if(outHexToName.ContainsKey(kvp.Value) == true)
                        continue;

                    outHexToName.Add(kvp.Value, kvp.Key);
                }
            }

            private static Dictionary<string,string> _colorNameToHex = null;
            private static Dictionary<string,string> _colorHexToName = null;

            public static Dictionary<string, string> GetNamesToHexColorMap()
            { 
                if(_colorNameToHex == null)
                    ColorNamesHexMap(out _colorNameToHex, out _colorHexToName);

                return _colorNameToHex;
            }

            public static Dictionary<string, string> GetHexToNamesColorMap()
            {
                if (_colorHexToName == null)
                    ColorNamesHexMap(out _colorNameToHex, out _colorHexToName);

                return _colorHexToName;
            }

            public static Color ConvertSVGStringToColor(string str)
            {
                if(string.IsNullOrEmpty(str) == true)
                    return Color.black;

                Dictionary<string,string> map = GetNamesToHexColorMap();
                string conv;
                if(map.TryGetValue(str, out conv) == true)
                    str = conv;

                if (str[0] == '#')
                    return Utils.ConvertHexStringToColor(str);

                return Color.black;
            }

            /// <summary>
            /// Convert a string in form "#XXXXXX" or "#XXXXXXXX" to a Unity color,
            /// where X is a hex character.
            /// </summary>
            /// <param name="str">The string to convert.</param>
            /// <returns>The generated color.</returns>
            public static Color ConvertHexStringToColor(string str)
            { 
                Color c = Color.black;
                ConvertHexStringToColor(str, ref c);
                return c;
            }

            /// <summary>
            /// Convert a string in form "#XXXXXX" or "#XXXXXXXX" to a Unity color,
            /// where X is a hex character.
            /// </summary>
            /// <param name="str">The string to convert.</param>
            /// <param name="c">The color to modify.</param>
            public static void ConvertHexStringToColor(string str, ref Color c)
            { 
                if(str.Length == 0)
                    return;

                if(str[0] == '#')
                    str = str.Substring(1);

                if(str.Length >= 2)
                {
                    int ir = int.Parse(str.Substring(0,2), System.Globalization.NumberStyles.HexNumber);
                    c.r = (float)ir/ 255.0f;
                }

                if(str.Length >= 4)
                { 
                    int ig = int.Parse(str.Substring(2,2), System.Globalization.NumberStyles.HexNumber);
                    c.g = (float)ig/255.0f;
                }

                if(str.Length >= 6)
                {
                    int ib = int.Parse(str.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
                    c.b = (float)ib / 255.0f;
                }

                if(str.Length >= 8)
                {
                    int ia = int.Parse(str.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
                    c.a = (float)ia / 255.0f;
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="str"></param>
            /// <param name="defaultToMeters"></param>
            /// <returns></returns>
            public static LengthUnit ConvertStringToLengthUnit(string str, bool defaultToMeters = false)
            {
                if(str == "m")
                    return LengthUnit.Meters;

                if(str == "cm")
                    return LengthUnit.Centimeters;

                if(str == "in")
                    return LengthUnit.Inches;

                if(str == "mm")
                    return LengthUnit.Millimeters;

                if(str == "pc")
                    return LengthUnit.Picas;

                if(str == "px")
                    return LengthUnit.Pixels;

                if(str == "pt")
                    return LengthUnit.Points;

                if(defaultToMeters == true)
                    return LengthUnit.Meters;

                return LengthUnit.Unlabled;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="lu"></param>
            /// <returns></returns>
            public static string ConvertLengthUnitToString(LengthUnit lu)
            { 
                switch(lu)
                { 
                    case LengthUnit.Centimeters:
                        return "cm";

                    case LengthUnit.Inches:
                        return "in";

                    case LengthUnit.Millimeters:
                        return "mm";

                    case LengthUnit.Picas:
                        return "pc";

                    case LengthUnit.Pixels:
                        return "px";

                    case LengthUnit.Points:
                        return "pt";
                }

                return "m";
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="str"></param>
            /// <param name="value"></param>
            /// <param name="unit"></param>
            /// <returns></returns>
            public static bool ExtractLengthString(string str, out float value, out LengthUnit unit)
            { 
                str = str.Trim();

                if(str.Length == 0)
                {
                    value = float.NaN;
                    unit = LengthUnit.Unlabled;
                    return false;
                }

                int inw = 0;
                for(inw = 0; inw < str.Length; ++inw)
                { 
                    if(char.IsLetter(str[inw]) && str[inw] != 'e')
                        break;
                }

                if(inw == str.Length)
                { 
                    unit = LengthUnit.Unlabled;
                    return float.TryParse(str, out value);
                }

                string num = str.Substring(0, inw);
                string un = str.Substring(inw);

                unit = ConvertStringToLengthUnit(un.Trim(), false);
                return float.TryParse(num.Trim(), out value);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="value"></param>
            /// <param name="unit"></param>
            /// <returns></returns>
            public static float ConvertUnitsToMeters(float value, LengthUnit unit)
            {
                switch(unit)
                { 
                    default:
                    case LengthUnit.Meters:
                    case LengthUnit.Unlabled:
                        return value;

                    case LengthUnit.Centimeters:
                        return value / 100.0f;

                    case LengthUnit.Millimeters:
                        return value / 1000.0f;

                    case LengthUnit.Picas:
                        return value * (0.0254f / 16.0f);

                    case LengthUnit.Points:
                        return value * (0.0254f / 72.0f);

                    case LengthUnit.Inches:
                        return value * 0.0254f;

                    case LengthUnit.Pixels:
                        return value * (0.0254f / 96.0f);
                }
            }

            /// <summary>
            /// Converts meters to SVG units.
            /// </summary>
            /// <param name="meters">The amount of meters to convert.</param>
            /// <param name="unit">The unit to convert to.</param>
            /// <returns>The distance in converted units.</returns>
            public static float ConvertMetersToUnit(float meters, LengthUnit unit)
            { 
                switch(unit)
                { 
                    default:
                    case LengthUnit.Unlabled:
                    case LengthUnit.Meters:
                        return meters;

                    case LengthUnit.Centimeters:
                        return meters * 100.0f;

                    case LengthUnit.Millimeters:
                        return meters * 1000.0f;

                    case LengthUnit.Picas:
                        return meters / (0.0254f / 16.0f);

                    case LengthUnit.Points:
                        return meters / (0.0254f / 72.0f);

                    case LengthUnit.Inches:
                        return meters / 0.0254f;

                    case LengthUnit.Pixels:
                        return meters / (0.0254f / 96.0f);
                }
            }

            /// <summary>
            /// A common pattern in a SVG XML file as an attribute is "key:value; nextkey:value; etcKey:etcValue"
            /// This will take that pattern and convert it into a dictionary.
            /// </summary>
            /// <param name="str">The string to process.</param>
            /// <returns>A dictionary matching the same mapping as the input string.</returns>
            public static Dictionary<string,string> SplitProperties(string str)
            { 
                Dictionary<string,string> ret = new Dictionary<string, string>();

                str = str.Trim();
                string [] sections = str.Split(new char[]{';' }, System.StringSplitOptions.RemoveEmptyEntries);

                foreach(string sec in sections)
                { 
                    string [] splits = sec.Split( new char[]{':' });
                    if(splits.Length == 0)
                        continue;

                    if(splits.Length == 1)
                        ret[splits[0].Trim()] = "";
                    else if(splits.Length == 2)
                        ret[splits[0].Trim()] = splits[1].Trim();
                }

                return ret;
            }

            /// <summary>
            /// Get the roots of a 1D quadratic Bezier between [0, 1].
            /// 
            /// https://iquilezles.org/www/articles/bezierbbox/bezierbbox.htm
            /// Since finding the roots of a set of vectors is orthogonal between the 
            /// components, it's simpler to break it down to a 1D problem.
            /// </summary>
            /// <param name="f0">The left value.</param>
            /// <param name="f1">The shared tangent.</param>
            /// <param name="f2">The right value.</param>
            /// <param name="min">The minimum bounds.</param>
            /// <param name="max">The maximum bounds.</param>
            /// <returns></returns>
            public static bool GetBounds1DQuad(float f0, float f1, float f2, out float min, out float max)
            {
                min = Mathf.Min(f0, f2);
                max = Mathf.Max(f0, f2);

                if (f1 < min || f1 > max || f1 < min || f1 > max)
                {
                    float t = Mathf.Clamp01((f0 - f1) / (f0 - 2.0f * f1 + f2));
                    float s = 1.0f - t;
                    float q = s * s * f0 + 2.0f * s * t * f1 + t * t * f2;
                    min = Mathf.Min(min, q);
                    max = Mathf.Max(max, q);
                }
                return true;
            }

            /// <summary>
            /// Get the bounds of a 1D quadratic Bezier between [0, 1].
            /// 
            /// https://iquilezles.org/www/articles/bezierbbox/bezierbbox.htm
            /// Since finding the roots of a set of vectors is orthogonal between the 
            /// components, it's simpler to break it down to a 1D problem.
            /// </summary>
            /// <param name="f0">The left value.</param>
            /// <param name="f1">The left value's control.</param>
            /// <param name="f2">The right value's control.</param>
            /// <param name="ret">The right value.</param>
            /// <returns></returns>
            public static bool GetRoot1DQuat(float f0, float f1, float f2, out float ret)
            {
                ret = (f0 - f1) / (f0 - 2.0f * f1 + f2);
                return ret >= 0.0f && ret <= 1.0f;
            }

            /// <summary>
            /// Get the bounds of a 1D cubic Bezier between a t domain of [0,1].
            /// 
            /// https://iquilezles.org/www/articles/bezierbbox/bezierbbox.htm
            /// Since finding the roots of a set of vectors is orthogonal between the 
            /// components, it's simpler to break it down to a 1D problem.
            /// </summary>
            /// <param name="f0">The left value.</param>
            /// <param name="f1">The left value's control.</param>
            /// <param name="f2">The right value's control.</param>
            /// <param name="f3">The right value.</param>
            /// <param name="min">The minimum bounds of the function.</param>
            /// <param name="max">The maximumg bounds of the function.</param>
            /// <returns></returns>
            public static bool GetBounds1DCubic(float f0, float f1, float f2, float f3, out float min, out float max)
            { 
                if(Mathf.Abs(f1 - f2) <= Mathf.Epsilon)
                {
                    // If it's essentially a quadradic curve, we need to use the quadradic formula
                    // or else we get a degenerate error.
                    return GetBounds1DQuad(f0, f1, f2, out min, out max);
                }

                min = Mathf.Min(f0, f3);
                max = Mathf.Max(f0, f3);

                float c = -1.0f * f0 + 1.0f * f1;
                float b = 1.0f * f0 - 2.0f * f1 + 1.0f * f2;
                float a = -1.0f * f0 + 3.0f * f1 - 3.0f * f2 + 1.0f * f3;

                float h = b * b - a * c;

                if (h > 0.0)
                {
                    h = Mathf.Sqrt(h);
                    float t = (-b - h) / a;
                    if (t > 0.0f && t < 1.0f)
                    {
                        float s = 1.0f - t;
                        float q = s * s * s * f0 + 3.0f * s * s * t * f1 + 3.0f * s * t * t * f2 + t * t * t * f3;
                        min = Mathf.Min(min, q);
                        max = Mathf.Max(max, q);
                    }
                    t = (-b + h) / a;
                    if (t > 0.0 && t < 1.0)
                    {
                        float s = 1.0f - t;
                        float q = s * s * s * f0 + 3.0f * s * s * t * f1 + 3.0f * s * t * t * f2 + t * t * t * f3;
                        min = Mathf.Min(min, q);
                        max = Mathf.Max(max, q);
                    }
                }

                return true;
            }

            /// <summary>
            /// Get the roots of a 1D cubic Bezier between the range [0, 1].
            /// 
            /// https://iquilezles.org/www/articles/bezierbbox/bezierbbox.htm
            /// Since finding the roots of a set of vectors is orthogonal between the 
            /// components, it's simpler to break it down to a 1D problem.
            /// </summary>
            /// <param name="f0">The left value.</param>
            /// <param name="f1">The left value's control.</param>
            /// <param name="f2">The right value's control.</param>
            /// <param name="f3">The right value.</param>
            /// <param name="ret1">The first found root, if the return value >= 1.</param>
            /// <param name="ret2">The second found root, if the return value == 2</param>
            /// <returns>The number of roots found.</returns>
            public static int GetRoots1DCubic(float f0, float f1, float f2, float f3, out float ret1, out float ret2)
            {
                if (Mathf.Abs(f1 - f2) <= Mathf.Epsilon)
                {
                    ret2 = 0.0f;

                    if (GetRoot1DQuat(f0, f1, f3, out ret1) == true)
                        return 1;

                    return 0;
                }
                float c = -1.0f * f0 + 1.0f * f1;
                float b = 1.0f * f0 - 2.0f * f1 + 1.0f * f2;
                float a = -1.0f * f0 + 3.0f * f1 - 3.0f * f2 + 1.0f * f3;

                float h = b * b - a * c;

                int ret = 0;
                ret1 = 0.0f;
                ret2 = 0.0f;

                // Any roots?
                if (h > 0.0)
                {
                    h = Mathf.Sqrt(h);
                    float t = (-b - h) / a;
                    if (t > 0.0f && t < 1.0f)
                    {
                        ret1 = t;
                        ++ret;
                    }

                    t = (-b + h) / a;
                    if (t > 0.0 && t < 1.0)
                    {
                        if(ret == 0)
                            ret1 = t;
                        else
                            ret2 = t;

                        ++ret;
                    }
                }

                return ret;
            }

            /// <summary>
            /// Get the tight bounding box of a quadratic Bezier.
            /// https://iquilezles.org/www/articles/bezierbbox/bezierbbox.htm
            /// </summary>
            /// <param name="p0">Starting point.</param>
            /// <param name="p1">Shared middle tangent point.</param>
            /// <param name="p2">End point.</param>
            /// <returns></returns>
            public static BoundsMM2 GetBoundBoxQuad(Vector2 p0, Vector2 p1, Vector2 p2)
            {
                BoundsMM2 ret = BoundsMM2.GetBoundsAroundPoints(p0, p2);

                GetBounds1DQuad(p0.x, p1.x, p2.x, out ret.min.x, out ret.max.x);
                GetBounds1DQuad(p0.y, p1.y, p2.y, out ret.min.y, out ret.max.y);

                return ret;
            }

            /// <summary>
            /// Get the tight bounding box of a cubic Bezier.
            /// https://iquilezles.org/www/articles/bezierbbox/bezierbbox.htm
            /// </summary>
            /// <param name="p0">Starting point.</param>
            /// <param name="p1">Starting point tangent.</param>
            /// <param name="p2">End point tangent.</param>
            /// <param name="p3">End point.</param>
            /// <returns></returns>
            public static BoundsMM2 GetBoundingBoxCubic(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
            {
                //https://iquilezles.org/www/articles/bezierbbox/bezierbbox.htm

                BoundsMM2 ret = new BoundsMM2();

                GetBounds1DCubic(p0.x, p1.x, p2.x, p3.x, out ret.min.x, out ret.max.x);
                GetBounds1DCubic(p0.y, p1.y, p2.y, p3.y, out ret.min.y, out ret.max.y);

                return ret;
            }

            // When are numbers getting too small? Unity's Epsilon value was a little *too* small.
            // The number we'll use for early exit
            const float segEps = 0.0000001f;
            // The number we'll use for slack when checking time intersection in bounds between [0,1].
            // It needs to be a lower precision than segEps.
            const float segEpsLamSlack = 0.00001f;

            /// <summary>
            /// Bezier intersection with Bezier clipping - iterative subdivision.
            /// https://stackoverflow.com/a/4041286
            /// 
            /// For a Bezier curve window (usually an entire segment - although may be smaller
            /// if testing self intersection) split the sections in half and get their bounding
            /// boxes, reject pairs that don't have overlapping bounding boxes. Then keep subdividing
            /// and testing collisions until we reach and end point. Then for the tiny windows, do
            /// a line intersection test.
            /// 
            /// Some massaging is needed on the line intersection - seems floating point error is
            /// getting the best of this implementation.
            /// </summary>
            /// <param name="rgnA">The region with the window to check bounding box overlap.</param>
            /// <param name="rgnB">Another region with a window to check bounding box overlap.</param>
            /// <param name="iterLeft">The number of times left to allow iteration.</param>
            /// <param name="minDst">An epsilon on the X and Y axis of the bounding boxes to stop 
            /// iterating if the bounding boxes are getting too small.</param>
            /// <param name="outList">The initial list of collisions.</param>
            /// <remarks>It is recommended that the eventual outList be processed to condense
            /// entries that are too similar.</remarks>
            public static void SubdivideSample(BezierSubdivRgn rgnA, BezierSubdivRgn rgnB, int iterLeft, float minDst, List<BezierSubdivSample> outList)
            { 
                if(rgnA.bounds.Intersects(rgnB.bounds) == false)
                    return;
                
                if(
                    iterLeft <= 0 || 
                    Mathf.Abs(rgnA.lambda0 - rgnA.lambda1) * 0.5f < segEps || 
                    Mathf.Abs(rgnB.lambda0 - rgnB.lambda1) * 0.5f < segEps)
                {
                    // When we've narrowed down far enough, it's time to do a 
                    // direct line test.


                    float a, b, c, d;
                    //
                    Utils.GetBezierWeights(rgnA.lambda0, out a, out b, out c, out d);
                    Vector2 A0 = a * rgnA.pt0 + b * rgnA.pt1 + c * rgnA.pt2 + d * rgnA.pt3;
                    Utils.GetBezierWeights(rgnA.lambda1, out a, out b, out b, out d);
                    Vector2 A1 = a * rgnA.pt0 + b * rgnA.pt1 + c * rgnA.pt2 + d * rgnA.pt3;
                    //
                    Utils.GetBezierWeights(rgnB.lambda0, out a, out b, out c, out d);
                    Vector2 B0 = a * rgnB.pt0 + b * rgnB.pt1 + c * rgnB.pt2 + d * rgnB.pt3;
                    Utils.GetBezierWeights(rgnB.lambda1, out a, out b, out b, out d);
                    Vector2 B1 = a * rgnB.pt0 + b * rgnB.pt1 + c * rgnB.pt2 + d * rgnB.pt3;

                    float s, t;
                    if(Utils.ProjectSegmentToSegment(A0, A1, B0, B1, out s, out t) == true)
                    {
                        if(rgnA.node == rgnB.node)
                        {
                            // TODO: If they're the same node, reject piecewise connections

                            //
                            // The on-segment testing is canceled, because it we've gotten this far,
                            // we're going to assume it's in. Floating point imprecision has been
                            // a thorny problem when evaluating s and t at these small distances.
                            //
                            //if(
                            //    (rgnA.lambda0 < rgnB.lambda0 && rgnA.lambda1 + segEps < rgnB.lambda0) ||
                            //    (rgnB.lambda0 < rgnA.lambda0 && rgnB.lambda1 + segEps < rgnA.lambda0))
                            //{

                                //if (s >= 0.0f && s <= 1.0f && t >= 0.0 && t <= 1.0f)
                                //{
                                    outList.Add(
                                        new BezierSubdivSample(
                                            rgnA,
                                            Mathf.Lerp(
                                                rgnA.lambda0,
                                                rgnA.lambda1,
                                                s),
                                            rgnB,
                                            Mathf.Lerp(
                                                rgnB.lambda0,
                                                rgnB.lambda1,
                                                t)));
                                //}
                            //}
                        }
                        else if(rgnA.node.next == rgnB.node || rgnB.node.next == rgnA.node)
                        {
                            // Else if they're peicewise node connections, reject very end connections
                            //
                            // The on-segment testing is canceled, because it we've gotten this far,
                            // we're going to assume it's in. Floating point imprecision has been
                            // a thorny problem when evaluating s and t at these small distances.
                            //
                            //if(
                            //    (rgnA.node.next == rgnB.node && rgnA.lambda1 < 1.0f - segEps) ||
                            //    (rgnA.node.prev == rgnB.node && rgnA.lambda0 > segEps))
                            //{
                            // Note we don't have the -or-equal-to comparisons
                            //if (s > 0.0f && s < 1.0f && t > 0.0 && t < 1.0f)
                            //{
                            outList.Add(
                                        new BezierSubdivSample(
                                            rgnA,
                                            Mathf.Lerp(
                                                rgnA.lambda0,
                                                rgnA.lambda1,
                                                s),
                                            rgnB,
                                            Mathf.Lerp(
                                                rgnB.lambda0,
                                                rgnB.lambda1,
                                                t)));
                                //}
                            //}
                        }
                        else
                        {
                            // Two nodes actually intersecting and not numerical error

                            // The on-segment testing is canceled, because it we've gotten this far,
                            // we're going to assume it's in. Floating point imprecision has been
                            // a thorny problem when evaluating s and t at these small distances.
                            //
                            //if(s >= -segEpsLamSlack && s <= 1.0f + segEpsLamSlack && t >= -segEpsLamSlack && t <= 1.0f + segEpsLamSlack)
                            //{ 
                            outList.Add(
                                    new BezierSubdivSample(
                                        rgnA, 
                                        Mathf.Lerp(
                                            rgnA.lambda0, 
                                            rgnA.lambda1, 
                                            s), 
                                        rgnB, 
                                        Mathf.Lerp(
                                            rgnB.lambda0,
                                            rgnB.lambda1, 
                                            t)));
                            //}
                        }
                    }

                    return;
                }

                

                // Subdivide out candidates
                BezierSubdivRgn rgnAA, rgnAB;
                rgnA.Split(out rgnAA, out rgnAB);

                BezierSubdivRgn rgnBA, rgnBB;
                rgnB.Split(out rgnBA, out rgnBB);

                SubdivideSample(rgnAA, rgnBA, iterLeft -1, minDst, outList);
                SubdivideSample(rgnAA, rgnBB, iterLeft - 1, minDst, outList);
                SubdivideSample(rgnAB, rgnBA, iterLeft - 1, minDst, outList);
                SubdivideSample(rgnAB, rgnBB, iterLeft - 1, minDst, outList);
            }

            public static void NodeIntersections(BNode nodeA, BNode nodeB, int iterLeft, float minDst, List<BezierSubdivSample> outList)
            { 
                // If we don't have two segments, then we can't have any kind of segment intersection
                if(nodeA.next == null || nodeB.next == null)
                    return;


                bool isLineA = nodeA.UseTanOut == false && nodeA.next.UseTanIn == false;
                bool isLineB = nodeB.UseTanOut == false && nodeB.next.UseTanIn == false;

                // If the entire segment isn't using tangents, we have two straight
                // lines, so we can just do a simple line segment test and be done with
                // it without complex subdivision iterations
                //
                if (isLineA == true && isLineB == true)
                { 
                    float s, t;
                    if(Utils.ProjectSegmentToSegment(
                        nodeA.Pos,
                        nodeA.next.Pos,
                        nodeB.Pos,
                        nodeB.next.Pos,
                        out s,
                        out t) == true)
                    {
                        if(s >= 0.0f && s <= 1.0f && t >= 0.0f && t <= 1.0f)
                        {
                            BezierSubdivSample bss = new BezierSubdivSample();
                            //
                            bss.linearA = true;
                            bss.nodeA = nodeA;
                            bss.lAEst = s;
                            //
                            bss.linearB = true;
                            bss.nodeB = nodeB;
                            bss.lBEst = t;
                            outList.Add(bss);
                        }
                    }
                }
                else if(isLineA == true || isLineB == true)
                { 
                    // We have two options here, we could have mirrored
                    // lineA line and lineB line branches that are duplicate,
                    // or a unified implementation that does branching. For
                    // now we do the latter - one unified implementation for
                    // curve-to-line with multiple branches inside.
                    BNode curve;
                    BNode line;

                    if (isLineA == true)
                    { 
                        line = nodeA;
                        curve = nodeB;
                    }
                    else
                    { 
                        line = nodeB;
                        curve = nodeA;
                    }
                    BNode.PathBridge pb = curve.GetPathBridgeInfo();

                    List<float> intersectCurve = new List<float>();
                    List<float> intersectLine = new List<float>();
                    Utils.IntersectLine(
                        intersectCurve, 
                        intersectLine,
                        curve.Pos,
                        curve.Pos + pb.prevTanOut,
                        curve.next.Pos + pb.nextTanIn,
                        curve.next.Pos,
                        line.Pos,
                        line.next.Pos);

                    for(int i = 0; i < intersectCurve.Count; ++i)
                    {
                        BezierSubdivSample bss = new BezierSubdivSample();
                        bss.nodeA = nodeA;
                        bss.nodeB = nodeB;
                        if(isLineA)
                        { 
                            bss.lAEst = intersectLine[i];
                            bss.linearA = true;
                            bss.lBEst = intersectCurve[i];
                            bss.linearB = false;
                        }
                        else
                        {
                            bss.lAEst = intersectCurve[i];
                            bss.linearA = false;
                            bss.lBEst = intersectLine[i];
                            bss.linearB = true;
                        }
                        outList.Add(bss);
                    }
                }
                else
                {
                    SubdivideSample(
                        BezierSubdivRgn.FromNode(nodeA),
                        BezierSubdivRgn.FromNode(nodeB),
                        iterLeft, 
                        minDst,
                        outList);
                }
            }

            /// <summary>
            /// The upward sloping basis of a Bezier hermite.
            /// </summary>
            /// <param name="t">The interpolation value.</param>
            /// <returns></returns>
            public static float HermiteS(float t)
            { 
                return -2.0f *t*t*t + 3.0f*t*t;
            }

            /// <summary>
            /// Given a value pt, find the parameter t that would go
            /// into HermiteS to make it return back to us pt;
            /// </summary>
            /// <param name="pt">The target value to receive from HermiteS.</param>
            /// <returns>The value needed to put into HermiteS for it to return pt.</returns>
            public static float InvertHermiteS(float pt)
            {
                // https://www.wolframalpha.com/input/?i=invert+y+%3D+-2x%5E3+%2B+3x%5E2

                float sqrPart = Mathf.Sqrt(pt * pt - pt);
                float cubePart = Mathf.Pow(2.0f * sqrPart - 2.0f * pt + 1.0f, 1.0f / 3.0f);

                return 0.5f * (cubePart + 1.0f/cubePart + 1.0f);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="a0"></param>
            /// <param name="a1"></param>
            /// <param name="a2"></param>
            /// <param name="a3"></param>
            /// <param name="a4"></param>
            /// <param name="x"></param>
            /// <returns></returns>
            public static float HalleyIteration5(float a0, float a1, float a2, float a3, float a4, float x)
            {
                // https://www.shadertoy.com/view/4sKyzW
                //
                //halley's method
                //basically a variant of newton raphson which converges quicker and has bigger basins of convergence
                //see http://mathworld.wolfram.com/HalleysMethod.html
                //or https://en.wikipedia.org/wiki/Halley%27s_method

                float f = ((((x + a4) * x + a3) * x + a2) * x + a1) * x + a0;
                float f1 = (((5.0f * x + 4.0f * a4) * x + 3.0f * a3) * x + 2.0f * a2) * x + a1;
                float f2 = ((20.0f * x + 12.0f * a4) * x + 6.0f * a3) * x + 2.0f * a2;

                return x - (2.0f * f * f1) / (2.0f * f1 * f1 - f * f2);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="coeffs"></param>
            /// <param name="x"></param>
            /// <returns></returns>
            public static float HalleyIteration4(Vector4 coeffs, float x)
            {
                // https://www.shadertoy.com/view/4sKyzW
                float f = (((x + coeffs[3]) * x + coeffs[2]) * x + coeffs[1]) * x + coeffs[0];
                float f1 = ((4.0f * x + 3.0f * coeffs[3]) * x + 2.0f * coeffs[2]) * x + coeffs[1];
                float f2 = (12.0f * x + 6.0f * coeffs[3]) * x + 2.0f * coeffs[2];

                return x - (2.0f * f * f1) / (2.0f * f1 * f1 - f * f2);
            }

            /// <summary>
            /// Coppied from the shader toy sample.
            /// // https://www.shadertoy.com/view/4sKyzW
            /// </summary>
            const int halley_iterations = 8;

            /// <summary>
            /// Solve distance epsilon - an eps used specifically for the distance solving stuff. Other
            /// code for other things may use other epsilon values.
            /// </summary>
            const float sdeps = .000005f;

            /// <summary>
            /// Given a 2D position and the point to define a cubic Bezier segment, find the 
            /// closest location.
            /// 
            /// Analytical function, finds the actual closest without numerical iteration.
            /// https://www.shadertoy.com/view/4sKyzW
            /// </summary>
            /// <param name="samp">The sample point to measure against.</param>
            /// <param name="p0">Bezier start.</param>
            /// <param name="p1">Bezier start tangent.</param>
            /// <param name="p2">Bezier end tangent.</param>
            /// <param name="p3">Bezier end.</param>
            /// <param name="closestLambda">The lambda that evaulates to the closest point to samp.</param>
            /// <returns>The distance of the closest point found.</returns>
            /// <remarks>Given the background to https://www.shadertoy.com/view/XdVBWd, whatever distance function is 
            /// driving the background might be viable and it's FAR less complex and FAR less code to port.</remarks>
            public static float GetDistanceFromCubicBezier(Vector2 samp, Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, out float closestLambda)
            {
                //switch points when near to end point to minimize numerical error
                //only needed when control point(s) very far away
                Vector2 a3 = -p0 + 3.0f * p1 - 3.0f * p2 + p3;
                Vector2 a2 = p0 * 3.0f  - p1 * 6.0f + 3.0f * p2;
                Vector2 a1 = -3.0f * p0 + 3.0f * p1;
                Vector2 a0 = p0 - samp;

                //compute polynomial describing distance to current pixel dependent on a parameter t
                float bc6 = Vector2.Dot(a3, a3);
                float bc5 = 2.0f * Vector2.Dot(a3, a2);
                float bc4 = Vector2.Dot(a2, a2) + 2.0f * Vector2.Dot(a1, a3);
                float bc3 = 2.0f * (Vector2.Dot(a1, a2) + Vector2.Dot(a0, a3));
                float bc2 = Vector2.Dot(a1, a1) + 2.0f * Vector2.Dot(a0, a2);
                float bc1 = 2.0f * Vector2.Dot(a0, a1);
                float bc0 = Vector2.Dot(a0, a0);

                bc5 /= bc6;
                bc4 /= bc6;
                bc3 /= bc6;
                bc2 /= bc6;
                bc1 /= bc6;
                bc0 /= bc6;

                //compute derivatives of this polynomial

                float b0 = bc1 / 6.0f;
                float b1 = 2.0f * bc2 / 6.0f;
                float b2 = 3.0f * bc3 / 6.0f;
                float b3 = 4.0f * bc4 / 6.0f;
                float b4 = 5.0f * bc5 / 6.0f;

                Vector4 c1 = new Vector4(b1, 2.0f* b2, 3.0f * b3, 4.0f * b4) / 5.0f;
                Vector3 c2 = new Vector3(c1[1], 2.0f * c1[2], 3.0f * c1[3]) / 4.0f;
                Vector2 c3 = new Vector2(c2[1], 2.0f * c2[2]) / 3.0f;
                float c4 = c3[1] / 2.0f;

                Vector4 roots_drv = new Vector4(1e38F, 1e38F, 1e38F, 1e38F);

                int num_roots_drv = SolveQuartic(c1, ref roots_drv);
                SortVector4(ref roots_drv);

                float ub = UpperBoundLagrange5(b0, b1, b2, b3, b4);
                float lb = LowerBoundLagrange5(b0, b1, b2, b3, b4);

                Vector3 a = new Vector3(1e38f, 1e38f, 1e38f);
                Vector3 b = new Vector3(1e38f, 1e38f, 1e38f);

                Vector3 roots = new Vector3(1e38f, 1e38f, 1e38f);

                int num_roots = 0;

                //compute root isolating intervals by roots of derivative and outer root bounds
                //only roots going form - to + considered, because only those result in a minimum
                if (num_roots_drv == 4)
                {
                    if (EvalPoly5(b0, b1, b2, b3, b4, roots_drv[0]) > 0.0f)
                    {
                        a[0] = lb;
                        b[0] = roots_drv[0];
                        num_roots = 1;
                    }

                    if (Mathf.Sign(EvalPoly5(b0, b1, b2, b3, b4, roots_drv[1])) != Mathf.Sign(EvalPoly5(b0, b1, b2, b3, b4, roots_drv[2])))
                    {
                        if (num_roots == 0)
                        {
                            a[0] = roots_drv[1];
                            b[0] = roots_drv[2];
                            num_roots = 1;
                        }
                        else
                        {
                            a[1] = roots_drv[1];
                            b[1] = roots_drv[2];
                            num_roots = 2;
                        }
                    }

                    if (EvalPoly5(b0, b1, b2, b3, b4, roots_drv[3]) < 0.0f)
                    {
                        if (num_roots == 0)
                        {
                            a[0] = roots_drv[3];
                            b[0] = ub;
                            num_roots = 1;
                        }
                        else if (num_roots == 1)
                        {
                            a[1] = roots_drv[3];
                            b[1] = ub;
                            num_roots = 2;
                        }
                        else
                        {
                            a[2] = roots_drv[3];
                            b[2] = ub;
                            num_roots = 3;
                        }
                    }
                }
                else
                {
                    if (num_roots_drv == 2)
                    {
                        if (EvalPoly5(b0, b1, b2, b3, b4, roots_drv[0]) < 0.0f)
                        {
                            num_roots = 1;
                            a[0] = roots_drv[1];
                            b[0] = ub;
                        }
                        else if (EvalPoly5(b0, b1, b2, b3, b4, roots_drv[1]) > 0.0f)
                        {
                            num_roots = 1;
                            a[0] = lb;
                            b[0] = roots_drv[0];
                        }
                        else
                        {
                            num_roots = 2;

                            a[0] = lb;
                            b[0] = roots_drv[0];

                            a[1] = roots_drv[1];
                            b[1] = ub;
                        }

                    }
                    else
                    {
                        // Wut in the world is going on here!?

                        //Vector3 roots_snd_drv2 = new Vector3(1e38f, 1e38f, 1e38f);
                        //int num_roots_snd_drv2 = SolveCubic(c2, ref roots_snd_drv2);
                        //
                        //Vector2 roots_trd_drv2 = new Vector2(1e38f, 1e38f);
                        //int num_roots_trd_drv2 = SolveQuadric(c3, ref roots_trd_drv2);
                        num_roots = 1;

                        a[0] = lb;
                        b[0] = ub;
                    }

                    //further subdivide intervals to guarantee convergence of halley's method
                    //by using roots of further derivatives
                    Vector3 roots_snd_drv = new Vector3(1e38f, 1e38f, 1e38f);
                    int num_roots_snd_drv = SolveCubic(c2, ref roots_snd_drv);
                    SortVector3(ref roots_snd_drv);

                    int num_roots_trd_drv = 0;
                    Vector2 roots_trd_drv = new Vector2(1e38f, 1e38f);

                    if (num_roots_snd_drv != 3)
                    {
                        num_roots_trd_drv = SolveQuadric(c3, ref roots_trd_drv);
                    }

                    for (int i = 0; i < 3; i++)
                    {
                        if (i < num_roots)
                        {
                            for (int j = 0; j < 3; j += 2)
                            {
                                if (j < num_roots_snd_drv)
                                {
                                    if (a[i] < roots_snd_drv[j] && b[i] > roots_snd_drv[j])
                                    {
                                        if (EvalPoly5(b0, b1, b2, b3, b4, roots_snd_drv[j]) > 0.0f)
                                        {
                                            b[i] = roots_snd_drv[j];
                                        }
                                        else
                                        {
                                            a[i] = roots_snd_drv[j];
                                        }
                                    }
                                }
                            }
                            for (int j = 0; j < 2; j++)
                            {
                                if (j < num_roots_trd_drv)
                                {
                                    if (a[i] < roots_trd_drv[j] && b[i] > roots_trd_drv[j])
                                    {
                                        if (EvalPoly5(b0, b1, b2, b3, b4, roots_trd_drv[j]) > 0.0f)
                                        {
                                            b[i] = roots_trd_drv[j];
                                        }
                                        else
                                        {
                                            a[i] = roots_trd_drv[j];
                                        }
                                    }
                                }
                            }
                        }
                    }
                }


                //compute roots with halley's method
                float d0 = float.MaxValue;
                closestLambda = float.MaxValue;
                for (int i = 0; i < 3; i++)
                {
                    if (i < num_roots)
                    {
                        roots[i] = 0.5f * (a[i] + b[i]);

                        for (int j = 0; j < halley_iterations; j++)
                            roots[i] = HalleyIteration5(b0, b1, b2, b3, b4, roots[i]);

                        //compute squared distance to nearest point on curve
                        roots[i] = Mathf.Clamp01(roots[i]);

                        float bzA, bzB, bzC, bzD;
                        GetBezierWeights(roots[i], out bzA, out bzB, out bzC, out bzD);
                        Vector2 to_curve = samp - (bzA * p0 + bzB * p1 + bzC * p2 + bzD * p3);

                        float secDist = Mathf.Min(d0, Vector2.Dot(to_curve, to_curve));
                        if(secDist < d0)
                        {
                            d0 = secDist;
                            closestLambda = roots[i];
                        }
                    }
                }

                return Mathf.Sqrt(d0);
            }

            /// <summary>
            /// https://www.shadertoy.com/view/4sKyzW
            /// </summary>
            /// <param name="a0"></param>
            /// <param name="a1"></param>
            /// <param name="a2"></param>
            /// <param name="a3"></param>
            /// <param name="a4"></param>
            /// <returns></returns>
            /// <remarks>
            /// Original source comments:
            /// lagrange positive real root upper bound
            /// see for example: https://doi.org/10.1016/j.jsc.2014.09.038
            /// </remarks>
            public static float UpperBoundLagrange5(float a0, float a1, float a2, float a3, float a4)
            {

                Vector4 coeffs1 = new Vector4(a0, a1, a2, a3);

                Vector4 neg1 = Vector4.Max(-coeffs1, Vector4.zero);
                float neg2 = Mathf.Max(-a4, 0.0f);

                Vector4 indizes1 = new Vector4(0, 1, 2, 3);
                const float indizes2 = 4.0f;

                Vector4 bounds1 = 
                    new Vector4(
                        Mathf.Pow(neg1.x, 1.0f/  (5.0f - indizes1.x)),
                        Mathf.Pow(neg1.y, 1.0f / (5.0f - indizes1.y)),
                        Mathf.Pow(neg1.z, 1.0f / (5.0f - indizes1.z)),
                        Mathf.Pow(neg1.w, 1.0f / (5.0f - indizes1.w)));

                float bounds2 = Mathf.Pow(neg2, 1.0f/ (5.0f - indizes2));

                Vector2 min1_2 = 
                    new Vector2(
                        Mathf.Min(bounds1.x, bounds1.y),
                        Mathf.Min(bounds1.z, bounds1.w));

                Vector2 max1_2 = 
                    new Vector2(
                        Mathf.Max(bounds1.x, bounds1.y),
                        Mathf.Max(bounds1.z, bounds1.w));

                float maxmin = Mathf.Max(min1_2.x, min1_2.y);
                float minmax = Mathf.Min(max1_2.x, max1_2.y);

                float max3 = Mathf.Max(max1_2.x, max1_2.y);

                float max_max = Mathf.Max(max3, bounds2);
                float max_max2 = Mathf.Max(Mathf.Min(max3, bounds2), Mathf.Max(minmax, maxmin));

                return max_max + max_max2;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="a0"></param>
            /// <param name="a1"></param>
            /// <param name="a2"></param>
            /// <param name="a3"></param>
            /// <param name="a4"></param>
            /// <returns></returns>
            /// <remarks>
            /// Original source comments:
            /// lagrange upper bound applied to f(-x) to get lower bound.
            /// </remarks>
            public static float LowerBoundLagrange5(float a0, float a1, float a2, float a3, float a4)
            {

                Vector4 coeffs1 = new Vector4(-a0, a1, -a2, a3);

                Vector4 neg1 = Vector4.Max(-coeffs1, Vector4.zero);
                float neg2 = Mathf.Max(-a4, 0.0f);

                Vector4 indizes1 = new Vector4(0, 1, 2, 3);
                const float indizes2 = 4.0f;

                Vector4 bounds1 = 
                    new Vector4(
                        Mathf.Pow(neg1.x, 1.0f/  (5.0f - indizes1.x)),
                        Mathf.Pow(neg1.y, 1.0f / (5.0f - indizes1.y)),
                        Mathf.Pow(neg1.z, 1.0f / (5.0f - indizes1.z)),
                        Mathf.Pow(neg1.w, 1.0f / (5.0f - indizes1.w)));

                float bounds2 = Mathf.Pow(neg2, 1.0f/ (5.0f - indizes2));

                Vector2 min1_2 = 
                    new Vector2(
                        Mathf.Min(bounds1.x, bounds1.y),
                        Mathf.Min(bounds1.z, bounds1.w));

                Vector2 max1_2 = 
                    new Vector2(
                        Mathf.Max(bounds1.x, bounds1.y),
                        Mathf.Max(bounds1.z, bounds1.w));

                float maxmin = Mathf.Max(min1_2.x, min1_2.y);
                float minmax = Mathf.Min(max1_2.x, max1_2.y);

                float max3 = Mathf.Max(max1_2.x, max1_2.y);

                float max_max = Mathf.Max(max3, bounds2);
                float max_max2 = Mathf.Max(Mathf.Min(max3, bounds2), Mathf.Max(minmax, maxmin));

                return -max_max - max_max2;
            }

            /// <summary>
            /// Evaluates a 2D Bezier position.
            /// </summary>
            /// <param name="t">The t interpolation value to evaluate for.</param>
            /// <param name="p0">The starting point.</param>
            /// <param name="p1">The starting point tangent.</param>
            /// <param name="p2">The end point tangent.</param>
            /// <param name="p3">The end point.</param>
            /// <returns>The evaluated point on the Bezier curve.</returns>
            public static Vector2 ParametricCubeBezier(float t, Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
            {
                // Multiple positions and basis
                Vector2 a0 = (-p0 + 3.0f * p1 - 3.0f * p2 + p3);
                Vector2 a1 = (3.0f * p0 - 6.0f * p1 + 3.0f * p2);
                Vector2 a2 = (-3.0f * p0 + 3.0f * p1);
                Vector2 a3 = p0;

                return (((a0 * t) + a1) * t + a2) * t + a3;

            }

            /// <summary>
            /// Given the 4 points of a 1D Bezier curve it returns the values
            /// that should be multiplied by t^3, t^2, t, and 1,
            /// as a linear combination, to evaluate the Bezier at time t.
            /// 
            /// See ParametricCubeBezier for more information from similar usage.
            /// </summary>
            /// <param name="p0"></param>
            /// <param name="p1"></param>
            /// <param name="p2"></param>
            /// <param name="p3"></param>
            /// <returns></returns>
            public static Vector4 BezierCoefficients(float p0, float p1, float p2, float p3)
            { 
                return 
                    new Vector4(
                        -p0 + 3.0f * p1 - 3.0f * p2 + p3,
                        3.0f * p0 - 6.0f * p1 + 3.0f * p2,
                        -3.0f * p0 + 3.0f * p1,
                        p0);
            }

            /// <summary>
            /// Given a vector, reorder the elements to advance in numerical order.
            /// 
            /// Original function https://www.shadertoy.com/view/4sKyzW, used to sort 
            /// roots of a function.
            /// </summary>
            /// <param name="roots">The vector to sort.</param>
            public static void SortVector3(ref Vector3 roots)
            {
                Vector3 tmp = 
                    new Vector3(
                        Mathf.Min(roots[0], Mathf.Min(roots[1], roots[2])),
                        Mathf.Max(roots[0], Mathf.Min(roots[1], roots[2])),
                        Mathf.Max(roots[0], Mathf.Max(roots[1], roots[2])));

                roots = tmp;
            }

            /// <summary>
            /// Given a vector, reorder the elements to advance in numerical order.
            /// 
            /// Original function https://www.shadertoy.com/view/4sKyzW, used to sort
            /// oroots of a function.
            /// </summary>
            /// <param name="roots">The vector to sort.</param>
            public static void SortVector4(ref Vector4 roots)
            {
                Vector2 min1_2 = 
                    new Vector2(
                        Mathf.Min(roots.x, roots.y),
                        Mathf.Min(roots.z, roots.w));

                Vector2 max1_2 = 
                    new Vector2(
                        Mathf.Max(roots.x, roots.y),
                        Mathf.Max(roots.z, roots.w));

                float maxmin = Mathf.Max(min1_2.x, min1_2.y);
                float minmax = Mathf.Min(max1_2.x, max1_2.y);

                Vector4 tmp = 
                    new Vector4(
                        Mathf.Min(min1_2.x, min1_2.y),
                        Mathf.Min(maxmin, minmax),
                        Mathf.Max(minmax, maxmin),
                        Mathf.Max(max1_2.x, max1_2.y));

                roots = tmp;
            }

            public static float EvalPoly5(float a0, float a1, float a2, float a3, float a4, float x)
            {
                float f = ((((x + a4) * x + a3) * x + a2) * x + a1) * x + a0;
                return f;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="coeffs"></param>
            /// <param name="roots">The output roots.</param>
            /// <returns></returns>
            /// <remarks>
            /// Original source comments:
            /// Modified from http://tog.acm.org/resources/GraphicsGems/gems/Roots3And4.c
            /// Credits to Doublefresh for hinting there
            /// </remarks>
            public static int SolveQuadric(Vector2 coeffs, ref Vector2 roots)
            {
                // normal form: x^2 + px + q = 0
                float p = coeffs[1] / 2.0f;
                float q = coeffs[0];

                float D = p * p - q;

                if (D < 0.0f)
                {
                    return 0;
                }
                else if (D > 0.0f)
                {
                    roots[0] = -Mathf.Sqrt(D) - p;
                    roots[1] = Mathf.Sqrt(D) - p;

                    return 2;
                }

                // The original shader code we took this from doesn't handle
                // an else case - so I'm guessing...
                return 0;
            }

            //From Trisomie21
            //But instead of his cancellation fix i'm using a newton iteration
            public static int SolveCubic(Vector3 coeffs, ref Vector3 r)
            {
                float a = coeffs[2];
                float b = coeffs[1];
                float c = coeffs[0];

                float p = b - a * a / 3.0f;
                float q = a * (2.0f * a * a - 9.0f * b) / 27.0f + c;
                float p3 = p * p * p;
                float d = q * q + 4.0f * p3 / 27.0f;
                float offset = -a / 3.0f;
                if (d >= 0.0)
                { // Single solution
                    float z = Mathf.Sqrt(d);
                    float u = (-q + z) / 2.0f;
                    float v = (-q - z) / 2.0f;
                    u = Mathf.Sign(u) * Mathf.Pow(Mathf.Abs(u), 1.0f / 3.0f);
                    v = Mathf.Sign(v) * Mathf.Pow(Mathf.Abs(v), 1.0f / 3.0f);
                    r[0] = offset + u + v;

                    //Single newton iteration to account for cancellation
                    float f = ((r[0] + a) * r[0] + b) * r[0] + c;
                    float f1 = (3.0f * r[0] + 2.0f * a) * r[0] + b;

                    r[0] -= f / f1;

                    return 1;
                }
                else
                {
                    float u = Mathf.Sqrt(-p / 3.0f);
                    float v = Mathf.Acos(-Mathf.Sqrt(-27.0f / p3) * q / 2.0f) / 3.0f;
                    float m = Mathf.Cos(v), n = Mathf.Sin(v) * 1.732050808f;

                    //Single newton iteration to account for cancellation
                    //(once for every root)
                    r[0] = offset + u * (m + m);
                    r[1] = offset - u * (n + m);
                    r[2] = offset + u * (n - m);

                    Vector3 f = 
                        new Vector3(
                            ((r.x + a) * r.x + b) * r.x + c,
                            ((r.y + a) * r.y + b) * r.y + c,
                            ((r.z + a) * r.z + b) * r.z + c);

                    Vector3 f1 = 
                        new Vector3(
                                (3.0f * r.x + 2.0f * a) * r.x + b,
                                (3.0f * r.y + 2.0f * a) * r.y + b,
                                (3.0f * r.z + 2.0f * a) * r.z + b);

                    r -= new Vector3(f.x / f1.y, f.y / f1.y, f.z / f1.z);

                    return 3;
                }
            }

            /// <summary>
            /// Solves the Cubic formula, 
            /// 
            /// From Wolfram (https://mathworld.wolfram.com/CubicFormula.html), 
            /// the formula matches as a3 * z^3 + a2 * z^2 + a1 * z + a0 == 0
            /// This will be what the parameter definitions will reference.
            /// </summary>
            /// <param name="p0">The value of a3</param>
            /// <param name="p1">The value of a2</param>
            /// <param name="p2">The value of a1</param>
            /// <param name="p3">The value of a0</param>
            /// <param name="t">An ordered collection of solutions. To know how many values in
            /// the vector are relevant, see the return value.</param>
            /// <returns>
            /// The number of solution found. This is also the number of entries in the 
            /// out parameter t that were filled.
            /// </returns>
            /// <remarks>
            /// While the function is originally added to solve for cubic Bezier segments
            /// against line segments, it was decided best to keep the function generic. The biggest
            /// consequence of that is the function is unbounded. So if any bounding (between [0.0, 1.0])
            /// needs to be checked, that should be done by the caller.
            /// </remarks>
            /// <remarks>
            /// Results can be validated with this online solver: 
            /// https://www.calculatorsoup.com/calculators/algebra/cubicequation.php
            /// </remarks>
            public static int SolveCubicFormula(float p0, float p1, float p2, float p3, out Vector3 t)
            {
                // https://mathworld.wolfram.com/CubicFormula.html
                // https://www.particleincell.com/2013/cubic-line-intersection/
                float a = p0;
                float b = p1;
                float c = p2;
                float d = p3;

                float A = b / a;
                float B = c / a;
                float C = d / a;

                float Q = (3.0f * B - Mathf.Pow(A, 2.0f)) / 9.0f;
                float R = (9.0f * A * B - 27.0f * C - 2.0f * Mathf.Pow(A, 3.0f)) / 54.0f;
                float D = Mathf.Pow(Q, 3.0f) + Mathf.Pow(R, 2.0f);    // polynomial discriminant

                t = new Vector3();

                if (D >= 0)                         // complex or duplicate roots
                {
                    float S = Mathf.Sign(R + Mathf.Sqrt(D)) * Mathf.Pow(Mathf.Abs(R + Mathf.Sqrt(D)), (1.0f / 3.0f));
                    float T = Mathf.Sign(R - Mathf.Sqrt(D)) * Mathf.Pow(Mathf.Abs(R - Mathf.Sqrt(D)), (1.0f / 3.0f));

                    t[0] = -A / 3 + (S + T);                                // real root
                    float Im = Mathf.Abs(Mathf.Sqrt(3) * (S - T) / 2);      // complex part of root pair   
                    if (Im != 0)
                        return 1;
                    
                    t[1] = -A / 3 - (S + T) / 2;  // real part of complex root
                    t[2] = -A / 3 - (S + T) / 2;  // real part of complex root
                    // 3 real roots

                }
                else                                          // distinct real roots
                {
                    float th = Mathf.Acos(R / Mathf.Sqrt(-Mathf.Pow(Q, 3.0f)));

                    t[0] = 2.0f * Mathf.Sqrt(-Q) * Mathf.Cos(th / 3) - A / 3;
                    t[1] = 2.0f * Mathf.Sqrt(-Q) * Mathf.Cos((th + 2 * Mathf.PI) / 3.0f) - A / 3.0f;
                    t[2] = 2.0f * Mathf.Sqrt(-Q) * Mathf.Cos((th + 4 * Mathf.PI) / 3.0f) - A / 3.0f;
                    // 3 real roots
                }

                //SortVector3(ref t);
                // We don't do [0,1] bounds checking here, we leave that for 
                return 3;
            }

            /// <summary>
            /// Solve a quartic 
            /// </summary>
            /// <param name="coeffs"></param>
            /// <param name="s"></param>
            /// <returns></returns>
            /// <remarks>
            /// Original source comments:
            /// Modified from http://tog.acm.org/resources/GraphicsGems/gems/Roots3And4.c
            /// Credits to Doublefresh for hinting there
            ///</remarks>
            public static int SolveQuartic(Vector4 coeffs, ref Vector4 s)
            {

                float a = coeffs[3];
                float b = coeffs[2];
                float c = coeffs[1];
                float d = coeffs[0];

                /*  substitute x = y - A/4 to eliminate cubic term:
                x^4 + px^2 + qx + r = 0 */

                float sq_a = a * a;
                float p = -3.0f/ 8.0f * sq_a + b;
                float q = 1.0f/ 8.0f * sq_a * a - 1.0f/ 2.0f * a * b + c;
                float r = -3.0f/ 256.0f* sq_a * sq_a + 1.0f/ 16.0f* sq_a * b - 1.0f/ 4.0f* a * c + d;

                int num;

                Vector3 cubic_coeffs = 
                    new Vector3(
                        1.0f / 2.0f * r * p - 1.0f / 8.0f * q * q,
                        -r,
                        -1.0f / 2.0f * p);

                Vector3 s3 = s;
                SolveCubic(cubic_coeffs, ref s3);
                s.x = s3.x;
                s.y = s3.y;
                s.z = s3.z;

                /* ... and take the one real solution ... */

                float z = s[0];

                /* ... to build two quadric equations */

                float u = z * z - r;
                float v = 2.0f * z - p;

                if (u > -sdeps)
                    u = Mathf.Sqrt(Mathf.Abs(u));
                else
                    return 0;

                if (v > -sdeps)
                    v = Mathf.Sqrt(Mathf.Abs(v));
                else
                    return 0;

                Vector2 quad_coeffs = 
                    new Vector2(
                        z - u,
                        q < 0.0f ? -v : v);

                Vector2 s2 = s;
                num = SolveQuadric(quad_coeffs, ref s2);
                s.x = s2.x; s.y = s2.y;

                quad_coeffs[0] = z + u;
                quad_coeffs[1] = q < 0.0f ? v : -v;

                Vector2 tmp = new Vector2(1e38f, 1e38f);
                int old_num = num;

                num += SolveQuadric(quad_coeffs, ref tmp);
                if (old_num != num)
                {
                    if (old_num == 0)
                    {
                        s[0] = tmp[0];
                        s[1] = tmp[1];
                    }
                    else
                    {//old_num == 2
                        s[2] = tmp[0];
                        s[3] = tmp[1];
                    }
                }

                /* resubstitute */

                float sub = 1.0f/ 4.0f * a;

                /* single halley iteration to fix cancellation */
                for (int i = 0; i < 4; i += 2)
                {
                    if (i < num)
                    {
                        s[i] -= sub;
                        s[i] = HalleyIteration4(coeffs, s[i]);

                        s[i + 1] -= sub;
                        s[i + 1] = HalleyIteration4(coeffs, s[i + 1]);
                    }
                }

                return num;
            }

            /// <summary>
            /// Calculate the intersection of a unbounded-or-unbounded line against a cubic
            /// Bezier segment.
            /// </summary>
            /// <param name="intersectCurveTs">The t values of intersections for the Bezier.</param>
            /// <param name="intersectLineTs">The t values of the intersections for the line.</param>
            /// <param name="pt0">The starting point (p0) of the cubic Bezier segment.</param>
            /// <param name="pt1">The starting control (p1) of the cubic Bezier segment.</param>
            /// <param name="pt2">The ending control (p2) of the cubic Bezier segment.</param>
            /// <param name="pt3">The ending point (p3) of the cubic Bezier segment.</param>
            /// <param name="l0">A point on the line. If lineBounds is true, this also acts as the left testing bounds.</param>
            /// <param name="l1">Another point on the line. If lineBounds is true, this also acts as a testing bounds.</param>
            /// <param name="lineBounds">
            /// If true, only things inside the line bounds will be returned. 
            /// If false, everything colliding will be returned, even if false.
            /// </param>
            /// <returns>The number of intersections added.</returns>
            /// <remarks>
            /// intersectCurveTs and intersectLineTs are mapped, so that an index of n
            /// for one array will be the collision of for index n of the other.
            /// </remarks>
            public static int IntersectLine(
                List<float> intersectCurveTs,
                List<float> intersectLineTs,
                Vector2 pt0,
                Vector2 pt1,
                Vector2 pt2,
                Vector2 pt3,
                Vector2 l0,     // A point on the line
                Vector2 l1,     // Another point on the line
                bool lineBounds = true)     
            {
                // https://www.particleincell.com/2013/cubic-line-intersection/

                // Convert explicit line to parametric form
                // Ax + By + C0
                float A = l1.y - l0.y;
                float B = l0.x - l1.x;

                if(Mathf.Abs(A) <= Mathf.Epsilon || Mathf.Abs(B) <= Mathf.Epsilon)
                { 
                    // If the 
                }

                float C =
                    l0.x * (l0.y - l1.y) +
                    l0.y * (l1.x - l0.x);

                Vector4 bx = Utils.BezierCoefficients(pt0.x, pt1.x, pt2.x, pt3.x);
                Vector4 by = Utils.BezierCoefficients(pt0.y, pt1.y, pt2.y, pt3.y);

                // Setup the cubic formula to represent the Bezier segment
                // intersecting with the line
                Vector4 P =
                    new Vector4(
                        A * bx.x + B * by.x,        // t^3
                        A * bx.y + B * by.y,        // t^2
                        A * bx.z + B * by.z,        // t^
                        A * bx.w + B * by.w + C);       // 1

                // Solve for collisions
                Vector3 r;
                int roots = SolveCubicFormula(P.x, P.y, P.z, P.w, out r);

                int ret = 0;
                for (int i = 0; i < roots; ++i)
                {
                    float t = r[i];
                    if(float.IsNaN(t) == true)
                        continue;

                    float t2 = t*t;
                    float t3 = t2 * t;

                    Vector2 X =
                        new Vector2(
                            bx.x * t3 + bx.y * t2 + bx.z * t + bx.w,
                            by.x * t3 + by.y * t2 + by.z * t + by.w);

                    float s;
                    if (Mathf.Abs(l0.x - l1.x) > Mathf.Epsilon) // If not vertical line
                        s = (X[0] - l0.x) / (l1.x - l0.x);
                    else
                        s = (X[1] - l0.y) / (l1.y - l0.y);

                    if(float.IsNaN(s) == true)
                        continue;

                    // Make sure the collision point is in bounds of our
                    // line and path segment
                    //
                    // For testing if it's in bounds of the line, we have a 
                    // parameter to decide if we want to check for that, if not, the
                    // line rejection turns off and anything on the unbounded line
                    // will be added.
                        if (t < 0.0f || t > 1.0f || (lineBounds == true && (s < 0.0f || s > 1.0f)))
                        continue;

                    if(intersectCurveTs != null)
                        intersectCurveTs.Add(t);

                    if(intersectLineTs != null)
                        intersectLineTs.Add(s);

                    ++ret;
                }
                return ret;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="a"></param>
            /// <param name="b"></param>
            /// <param name="clean"></param>
            public static void TestIntersection(BNode a, BNode b, bool clean)
            {
                List<Utils.BezierSubdivSample> inter = new List<Utils.BezierSubdivSample>();
            }

            
            public static void Swap<ty>(ref ty x, ref ty y)
            { 
                ty tmp = x;
                x = y;
                y = tmp;
            }
        }
    }
}