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
        public class Vector2Repo
        {
            List<Vector2> vectors = new List<Vector2>();
            Dictionary<Vector2, int> lookup = new Dictionary<Vector2, int>();

            public bool HasVector(Vector2 v)
            { 
                return lookup.ContainsKey(v);
            }

            public int GetVectorID(Vector3 v)
            { 
                int ret;
                if(lookup.TryGetValue(v, out ret) == true)
                    return ret;

                int idx = lookup.Count;
                vectors.Add(v);
                lookup.Add(v, idx);

                return idx;
            }

            public Vector2 GetVector(int idx)
            { 
                return this.vectors[idx];
            }

            public Vector2 [] GetVector2Array()
            { 
                return this.vectors.ToArray();
            }

            public Vector3 [] GetVector3Array()
            { 
                Vector3 [] ret = new Vector3 [this.vectors.Count];

                int ct = this.vectors.Count;
                for (int i = 0; i < ct; ++i)
                {
                    ret[i] = this.vectors[i];
                }

                return ret;
            }

            public Vector3 [] GetVector3Array(float z)
            {
                Vector3[] ret = new Vector3[this.vectors.Count];

                int ct = this.vectors.Count;
                for (int i = 0; i < ct; ++i)
                {
                    ret[i] = 
                        new Vector3(
                            this.vectors[i].x,
                            this.vectors[i].y,
                            z);
                }

                return ret;
            }
        }
    }
}