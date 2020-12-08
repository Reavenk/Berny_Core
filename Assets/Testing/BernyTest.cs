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

using PxPre.Berny;

/// <summary>
/// A class to test the functionality of Berny library. 
/// 
/// While it has some testing features, most of the features are in its inspector (CurveEditor).
/// </summary>
public class BernyTest : MonoBehaviour
{
    public Document curveDocument;

    public struct FillEntry
    { 
        public BShape shape;
        public GameObject go;
        public MeshFilter mf;
        public MeshRenderer mr;
        public Mesh mesh;
    }

    Dictionary<BShape, FillEntry> fillEntries = new Dictionary<BShape, FillEntry>();

    // Start is called before the first frame update
    void Start()
    {
        this.curveDocument = new Document();

        BShape shapeRect = this.curveDocument.AddRectangle(Vector2.zero, new Vector2(1.0f, 1.0f));

        foreach(BLoop bl in shapeRect.loops)
        {
            foreach (BNode bn in bl.nodes)
                bn.Round();
        }

        this.curveDocument.FlushDirty();
    }

    public void UpdateForFill(BShape bs)
    {
        FillEntry fe;
        if (this.fillEntries.TryGetValue(bs, out fe)  == false)
        { 
            GameObject go = new GameObject("ShapeFill");
            MeshFilter mf = go.AddComponent<MeshFilter>();
            MeshRenderer mr = go.AddComponent<MeshRenderer>();
            Mesh m = new Mesh();

            fe.go = go;
            fe.mf = mf;
            fe.mr = mr;
            fe.mesh = m;

            fe.mf.mesh = fe.mesh;

            this.fillEntries.Add(bs, fe);
        }

        List<int> triangles = new List<int>();
        Vector2Repo vectorRepo = new Vector2Repo();

        FillSession session = new FillSession();
        session.ExtractFillLoops(bs);
        session.GetTriangles(triangles, vectorRepo, true, true);

        fe.mesh.SetVertices(vectorRepo.GetVector3Array());
        fe.mesh.SetIndices(triangles, MeshTopology.Triangles, 0);
    }

    public void UpdateFillsForAll()
    { 
        foreach(Layer layer in this.curveDocument.Layers())
        { 
            foreach(BShape shape in layer.shapes)
            { 
                this.UpdateForFill(shape);
            }
        }
    }

    GameObject cursor = null;
    void Update()
    {
        if(cursor == null)
            cursor = new GameObject("Cursor");

        if(Input.GetKeyDown(KeyCode.D) == true)
        { 
            float dist = float.PositiveInfinity;

            Vector2 mp = cursor.transform.position;

            foreach (BNode node in this.curveDocument.EnumerateNodes())
            {
                if(node.next == null)
                    continue;

                float l;
                float nodeDst = 
                    Utils.GetDistanceFromCubicBezier(
                        mp, 
                        node.Pos, 
                        node.Pos + node.TanOut, 
                        node.next.Pos + node.next.TanIn,
                        node.next.Pos,
                        out l);

                dist = Mathf.Min(dist, nodeDst);
            }

            Debug.Log($"Closest distance was at {dist}");
        }
    }

    //private void OnDrawGizmos()
    //{
    //    
    //}
}