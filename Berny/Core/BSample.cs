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
        /// A BNode curve sample segment.
        /// 
        /// While the BNode represents a path, it's the parametric path. The actual explicit data
        /// of the path is represented by a linked list of BSample edges that are recreated 
        /// every time a BNode is dirty. 
        /// 
        /// It is also the same region that ear clipping is perfomed
        /// on for filling a closed island - although that is done with
        /// the FillSegment class.
        /// </summary>
        public class BSample
        {
            /// <summary>
            /// The BNode the sample segment belongs to.
            /// </summary>
            public BNode parent;

            /// <summary>
            /// The next sample in the chain.
            /// </summary>
            public BSample next;

            /// <summary>
            /// The previous sample in the chain.
            /// </summary>
            public BSample prev;

            /// <summary>
            /// The evaluated position.
            /// </summary>
            public Vector2 pos;

            // The lambda that created this point
            public float lambda;

#if DEVELOPMENT_BUILD || UNITY_EDITOR
            int debugCounter;
#endif

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="parent">The BNode being represented.</param>
            /// <param name="pos">The sampled position.</param>
            /// <param name="lambda">The t value being sampled.</param>
            public BSample(BNode parent, Vector2 pos, float lambda)
            { 
                this.parent = parent;
                this.pos = pos;
                this.lambda = lambda;

#if DEVELOPMENT_BUILD || UNITY_EDITOR
                this.debugCounter = Utils.RegisterCounter();
#endif
            }
        }
    } 
}