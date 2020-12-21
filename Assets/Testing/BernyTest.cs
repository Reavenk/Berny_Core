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

using PxPre.Berny;

/// <summary>
/// A class to test the functionality of Berny library. 
/// 
/// While it has some testing features, most of the features are in its inspector (CurveEditor).
/// </summary>
public class BernyTest : MonoBehaviour
{
    public Document curveDocument;

    public PxPre.Berny.Font.Typeface typeface;

    public struct FillEntry
    { 
        public BShape shape;
        public GameObject go;
        public MeshFilter mf;
        public MeshRenderer mr;
        public Mesh mesh;
    }

    public enum FillType
    { 
        Filled,
        Outlined,
        FilledAndOutlined
    }

    Dictionary<BShape, FillEntry> fillEntries = new Dictionary<BShape, FillEntry>();

    public Shader standardShader;

    // Start is called before the first frame update
    void Start()
    {
        PxPre.Berny.TTF.Loader loader = new PxPre.Berny.TTF.Loader();
        this.typeface = loader.ReadTTF("Assets\\Testing\\Nerko\\NerkoOne-Regular.ttf");

        this.curveDocument = new Document();

        BShape shapeRect = this.curveDocument.AddRectangle(Vector2.zero, new Vector2(1.0f, 1.0f));

        foreach(BLoop bl in shapeRect.loops)
        {
            foreach (BNode bn in bl.nodes)
                bn.Round();
        }

        this.curveDocument.FlushDirty();
    }

    public void UpdateForFill(BShape bs, FillType ft, float width)
    {
        FillEntry fe;
        if (this.fillEntries.TryGetValue(bs, out fe)  == false)
        { 
            GameObject go = new GameObject("ShapeFill");
            MeshFilter mf = go.AddComponent<MeshFilter>();
            MeshRenderer mr = go.AddComponent<MeshRenderer>();

            fe.go = go;
            fe.mf = mf;
            fe.mr = mr;

            this.fillEntries.Add(bs, fe);
        }

        Mesh m = new Mesh();
        fe.mf.mesh = m;
        fe.mesh = m;

        if (ft == FillType.Filled)
        {
            List<int> triangles = new List<int>();
            Vector2Repo vectorRepo = new Vector2Repo();

            FillSession session = new FillSession();
            session.ExtractFillLoops(bs);
            session.GetTriangles(triangles, vectorRepo, true, true);

            fe.mesh.SetVertices(vectorRepo.GetVector3Array());
            fe.mesh.SetIndices(triangles, MeshTopology.Triangles, 0);

            Material matFill = new Material(this.standardShader);
            matFill.color = Color.white;

            fe.mr.sharedMaterial = matFill;
        }
        else if(ft == FillType.Outlined)
        {
            List<int> triangles = new List<int>();
            Vector2Repo vectorRepo = new Vector2Repo();

            FillSession session = new FillSession();
            session.ExtractFillLoops(bs);
            foreach(FillIsland fi in session.islands)
                fi.MakeOutlineBridged(width);
            
            session.GetTriangles(triangles, vectorRepo, true, true);

            fe.mesh.SetVertices(vectorRepo.GetVector3Array());
            fe.mesh.SetIndices(triangles, MeshTopology.Triangles, 0);

            Material matStroke = new Material(this.standardShader);
            matStroke.color = Color.black;

            fe.mr.sharedMaterial = matStroke;
        }
        else if(ft == FillType.FilledAndOutlined)
        {
            List<int> triangles = new List<int>();
            List<int> strokeTris = new List<int>();
            Vector2Repo vectorRepo = new Vector2Repo();

            FillSession session = new FillSession();
            session.ExtractFillLoops(bs);
            FillSession outSession = session.Clone();

            session.GetTriangles(triangles, vectorRepo, true, true);

            foreach (FillIsland fi in outSession.islands)
                fi.MakeOutlineBridged(width);

            outSession.GetTriangles(strokeTris, vectorRepo, true, true);

            fe.mesh.subMeshCount = 2;
            fe.mesh.SetVertices(vectorRepo.GetVector3Array());
            fe.mesh.SetIndices(triangles, MeshTopology.Triangles, 0);
            fe.mesh.SetIndices(strokeTris, MeshTopology.Triangles, 1);

            Material matFill = new Material(this.standardShader);
            matFill.color = Color.white;
            Material matStroke = new Material(this.standardShader);
            matStroke.color = Color.black;

            fe.mr.sharedMaterials = new Material[]{ matFill, matStroke };
        }
    }

    public void UpdateFillsForAll(FillType ft, float stroke)
    { 
        foreach(Layer layer in this.curveDocument.Layers())
        { 
            foreach(BShape shape in layer.shapes)
            { 
                this.UpdateForFill(shape, ft, stroke);
            }
        }
    }

    public void ClearFills()
    {
        foreach(KeyValuePair<BShape, FillEntry > kvp in this.fillEntries)
        { 
            FillEntry fe = kvp.Value;
            GameObject.Destroy(fe.go);
        }
        this.fillEntries.Clear();
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

    private void OnDrawGizmos()
    {
        if(this.typeface == null)
            return;

        Gizmos.color = Color.magenta;

        float x = 0.0f;
        foreach(PxPre.Berny.Font.Glyph g in this.typeface.glyphs)
        { 
            for(int i = 0; i < g.contours.Count; ++i)
            {
                PxPre.Berny.Font.Contour c = g.contours[i];
                for (int j = 0; j < c.points.Count - 1; ++j)
                {
                    Gizmos.DrawLine(
                        new Vector2( x, 0.0f) + c.points[j].position,
                        new Vector2(x, 0.0f) + c.points[j + 1].position);
                }

                Gizmos.DrawLine(
                    new Vector2( x, 0.0f) + c.points[0].position,
                    new Vector2(x, 0.0f) + c.points[c.points.Count - 1].position);
            }

            x += g.advance;
        }
    }
}
