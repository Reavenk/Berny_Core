﻿// MIT License
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

namespace PxPre.Berny
{
    /// <summary>
    /// A utility class to hold the vertices of a 2D triangle mesh.
    /// 
    /// The class name is short for Vector2 repository.
    /// </summary>
    public class Vector2Repo
    {
        /// <summary>
        /// The vertices of the mesh.
        /// </summary>
        List<Vector2> vectors = new List<Vector2>();

        /// <summary>
        /// A lookup of mesh points to find their indices (if they're in the mesh).
        /// </summary>
        Dictionary<Vector2, int> lookup = new Dictionary<Vector2, int>();

        /// <summary>
        /// Queries if a vector is already included.
        /// </summary>
        /// <param name="v">The vector to check.</param>
        /// <returns>True if it's already contained. Else, false.</returns>
        public bool HasVector(Vector2 v)
        { 
            return lookup.ContainsKey(v);
        }

        /// <summary>
        /// Get the index of a vector. If the vector isn't currently contained, it is added.
        /// </summary>
        /// <param name="v">The vector to get the index of.</param>
        /// <returns>The index of the vector.</returns>
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

        /// <summary>
        /// Get a vector of an index.
        /// </summary>
        /// <param name="idx">The index to retrieve.</param>
        /// <returns>The Vector at the specified index.</returns>
        public Vector2 GetVector(int idx)
        { 
            return this.vectors[idx];
        }

        /// <summary>
        /// Get the contents of the repo as a Vector2 array.
        /// </summary>
        /// <returns>The array of Vector2s in the repo.</returns>
        public Vector2 [] GetVector2Array()
        { 
            return this.vectors.ToArray();
        }

        /// <summary>
        /// Get the contents of the repo as a Vector3 array, where all z values are set 0.0.
        /// </summary>
        /// <returns>The array of Vector3s in the repo.</returns>
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

        /// <summary>
        /// Get the contents of the repo as a Vector3 array.
        /// </summary>
        /// <param name="z">The z component for the vectors.</param>
        /// <returns>The array of Vector3s in the repo.</returns>
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

        /// <summary>
        /// Scale all points in the repo.
        /// </summary>
        /// <param name="f">The amount to scale by.</param>
        public void Scale(float f)
        {
            this.lookup.Clear();
            for(int i = 0; i < this.vectors.Count; ++i)
            { 
                this.vectors[i] *= f;
                this.lookup.Add(this.vectors[i], i);
            }
        }
    }
}