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

namespace PxPre.Berny
{
    /// <summary>
    /// A utility class to hold the vertices of a 3D triangle mesh.
    /// 
    /// The class name is short for Vector3 repository.
    /// </summary>
    public class Vector3Repo
    {
        /// <summary>
        /// The vertices of the mesh.
        /// </summary>
        List<Vector3> vectors = new List<Vector3>();

        /// <summary>
        /// A lookup of mesh point to find their indices (if they're in the mesh).
        /// </summary>
        Dictionary<Vector3, int> lookup = new Dictionary<Vector3, int>();

        /// <summary>
        /// Queries if a vector is already included.
        /// </summary>
        /// <param name="v">The vector to check.</param>
        /// <returns>True if it's already contained. Else, false.</returns>
        public bool HasVector(Vector3 v)
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
            if (lookup.TryGetValue(v, out ret) == true)
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
        public Vector3 GetVector(int idx)
        {
            return this.vectors[idx];
        }

        /// <summary>
        /// Get the contents of the repo as a Vector3 array.
        /// </summary>
        /// <returns>The array of Vector3s in the repo.</returns>
        public Vector3[] GetVector3Array()
        {
            return this.vectors.ToArray();
        }
    }
}
