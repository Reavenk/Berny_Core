using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PxPre.Berny;

[CustomEditor(typeof(BernyTest))]
public class CurveEditor : Editor
{
    /// <summary>
    /// The nodes in the document that are selected for editing.
    /// </summary>
    HashSet<BNode> selectedNodes = new HashSet<BNode>();

    /// <summary>
    /// If editing a tangent, what node does the tangent belong to?
    /// </summary>
    BNode movedTangent = null;

    /// <summary>
    /// If editing a tangent of a node, which tangent is being edited on the node?
    /// </summary>
    BNode.TangentType movedTangentType = BNode.TangentType.Output;


    public Vector2 intersectTestStart = new Vector2(-2.0f, -2.0f);
    public Vector2 intersectTextEnd = new Vector2(2.0f, 2.0f);

    public List<Vector2> intersectionPreviews = new List<Vector2>();

    /// <summary>
    /// The location to place editing widgets.
    /// </summary>
    Vector2 averagePoint = Vector2.zero;

    public float infAmt = 0.05f;

    /// <summary>
    /// Unity inspector function.
    /// </summary>
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        BernyTest t = (BernyTest)this.target;
        if (t == null || t.curveDocument == null)
            return;

        if (GUILayout.Button("Test Validity") == true)
            t.curveDocument.TestValidity();

        if(GUILayout.Button("Fill") == true)
            t.UpdateFillsForAll();

        GUILayout.BeginHorizontal();
            if(GUILayout.Button("Select All") == true)
            { 
                this.selectedNodes = new HashSet<BNode>( t.curveDocument.EnumerateNodes() );
            }
            if(GUILayout.Button("Deselect All") == true)
            { 
                this.selectedNodes.Clear();
            }
        GUILayout.EndHorizontal();

        if(GUILayout.Button("Scan Selected Intersections") == true)
        { 
            this.ScanSelectedIntersections(t.curveDocument);
        }

        GUILayout.Space(20.0f);

        if(GUILayout.Button("LOAD Ven") == true)
        {
            t.curveDocument.Clear();
            SVGSerializer.Load("Ven.svg", t.curveDocument);
        }

        if (GUILayout.Button("LOAD TriVen") == true)
        {
            t.curveDocument.Clear();
            SVGSerializer.Load("TriVen.svg", t.curveDocument);
        }

        if (GUILayout.Button("LOAD CircAnCirc") == true)
        {
            t.curveDocument.Clear();
            SVGSerializer.Load("CircAnCirc.svg", t.curveDocument);
        }

        if (GUILayout.Button("LOAD Complex") == true)
        {
            t.curveDocument.Clear();
            SVGSerializer.Load("Complex.svg", t.curveDocument);
        }

        if (GUILayout.Button("LOAD Edges") == true)
        {
            t.curveDocument.Clear();
            SVGSerializer.Load("Edges.svg", t.curveDocument);
        }

        GUILayout.Space(20.0f);

        this.infAmt = EditorGUILayout.FloatField("Inflation Amt", this.infAmt);
        if(GUILayout.Button("Inflate") == true)
        {
            foreach(Layer l in t.curveDocument.Layers())
            {
                foreach(BShape bs in l.shapes)
                { 
                    foreach(BLoop bl in bs.loops)
                        bl.Inflate(this.infAmt);
                }
            }
        }

        if(GUILayout.Button("Edgeify") == true)
        {
            foreach (Layer l in t.curveDocument.Layers())
            {
                foreach (BShape bs in l.shapes)
                {
                    foreach (BLoop bl in bs.loops)
                    {
                        List<BNode> islands = bl.GetIslands();

                        foreach(BNode bisl in islands)
                        {
                            // This will probably just give us bisl back, but it that's the case, then it should
                            // be minimal overhead - just to be safe though, and to see what kind of connectivity we're dealing with.
                            BNode.EndpointQuery eq = bisl.GetPathLeftmost();

                            List<BNode> origs = new List<BNode>();
                            List<BNode> copies = new List<BNode>();
                            List<InflationCache> inflations = new List<InflationCache>();
                            foreach(BNode it in eq.Enumerate())
                            {
                                origs.Add(it);

                                BNode cpy = new BNode(bl, it, false, true);
                                copies.Add(cpy);

                                bl.nodes.Add(cpy);

                                InflationCache ic = new InflationCache();
                                it.GetInflateDirection(out ic.selfInf, out ic.inInf, out ic.outInf);
                                inflations.Add(ic);
                            }

                            // Stitch the new chain - it should have a reverse winding.
                            //
                            // The loop is a little backwards, but basically we sub instead of add to
                            // treat the prev item in the array like the next in the chain.
                            for(int i = 1; i < copies.Count; ++i)
                            { 
                                copies[i].next = copies[i - 1];
                                copies[i - 1].prev = copies[i];
                            }

                            int lastIdx = copies.Count - 1;
                            if(eq.result == BNode.EndpointResult.Cyclical)
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

                            // Now that we have copies and connectivity set up, it's time
                            // to apply the thickening
                            for(int i = 0; i < copies.Count; ++i)
                            { 
                                // Push out the original
                                origs[i].Pos += this.infAmt * inflations[i].selfInf;
                                origs[i].TanIn += this.infAmt * (inflations[i].inInf - inflations[i].selfInf);
                                origs[i].TanOut += this.infAmt * (inflations[i].outInf - inflations[i].selfInf);

                                // We can optinally pull in the copy
                            }
                        }
                    }
                }
            }
        }


        GUILayout.Space(20.0f);

        if(GUILayout.Button("Clear") == true)
        { 
            t.curveDocument.Clear();
        }

        if(GUILayout.Button("Save SVG") == true)
        { 
            SVGSerializer.Save("TestSave.svg", t.curveDocument);
        }

        if(GUILayout.Button("Load SVG") == true)
        { 
            t.curveDocument.Clear();
            SVGSerializer.Load("TestSave.svg", t.curveDocument);
        }

        GUILayout.Space(20.0f);

        if(this.selectedNodes.Count > 0)
        { 
            GUILayout.BeginHorizontal();
                if(GUILayout.Button("Delete Selected") == true)
                { 
                    foreach(BNode selNode in this.selectedNodes)
                        selNode.parent.RemoveNode(selNode);

                    this.selectedNodes.Clear();
                    this.movedTangent = null;
                }

                if(GUILayout.Button("Disconnect Selected") == true)
                {
                    foreach (BNode selNode in this.selectedNodes)
                        selNode.Disconnect(true);
                }

                if(GUILayout.Button("Detach Selected") == true)
                { 
                    foreach(BNode selNode in this.selectedNodes)
                        selNode.Detach();
                }
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Connect") == true)
            {
                if (this.selectedNodes.Count != 2)
                    return;

                List<BNode> selList = new List<BNode>(this.selectedNodes);
                selList[0].parent.ConnectNodes(selList[0], selList[1]);
            }

            if (GUILayout.Button("Subdivide Selected") == true)
            {
                HashSet<BNode> subbed = new HashSet<BNode>();
                foreach (BNode selNode in this.selectedNodes)
                {
                    BNode bsubed = selNode.Subdivide(0.5f);

                    if(bsubed != null)
                        subbed.Add(bsubed);
                }

                if(subbed.Count > 0)
                { 
                    foreach(BNode bn in subbed)
                        this.selectedNodes.Add(bn);

                    this.RecalculateSelectionCentroid();
                }
            }


            GUILayout.BeginHorizontal();
                if(GUILayout.Button("Round Selected") == true)
                {
                    foreach (BNode selNode in this.selectedNodes)
                        selNode.Round();
                }

                if(GUILayout.Button("Smooth") == true)
                {
                    foreach (BNode selNode in this.selectedNodes)
                        selNode.SetTangentSmooth();
                }

                if(GUILayout.Button("Symmetrize") == true)
                {
                    foreach (BNode selNode in this.selectedNodes)
                        selNode.SetTangentsSymmetry();
                }

                if(GUILayout.Button("Disconnect") == true)
                {
                    foreach (BNode selNode in this.selectedNodes)
                        selNode.SetTangentDisconnected();
                }

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
                if(GUILayout.Button("Enable Inputs") == true)
                {
                    foreach (BNode selNode in this.selectedNodes)
                        selNode.UseTanIn = true;
                }
                if(GUILayout.Button("Enable Outputs") == true)
                {
                    foreach (BNode selNode in this.selectedNodes)
                        selNode.UseTanOut = true;
                }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
                if(GUILayout.Button("Disable Inputs") == true)
                {
                    foreach (BNode selNode in this.selectedNodes)
                        selNode.UseTanIn = false;
                }
                if(GUILayout.Button("Disable Outputs") == true)
                {
                    foreach (BNode selNode in this.selectedNodes)
                        selNode.UseTanOut = false;
                }
            GUILayout.EndHorizontal();

            if(GUILayout.Button("Islands")  == true)
            {
                HashSet<BLoop> foundLoops = new HashSet<BLoop>();
                foreach(BNode bn in this.selectedNodes)
                    foundLoops.Add(bn.parent);

                foreach(BLoop bl in foundLoops)
                { 
                    int island = bl.CalculateIslands();
                    for(int i = 0; i < island - 1; ++i)
                    { 
                        bl.ExtractIsland(bl.nodes[0]);
                    }
                }
            }

            GUILayout.BeginHorizontal();
                if(GUILayout.Button("Winding Simple") == true)
                { 
                    if(this.selectedNodes.Count > 0)
                    {
                        BNode bn = Utils.GetFirstInHash(this.selectedNodes);
                        float w = bn.parent.CalculateWindingSimple(bn, true);
                        Debug.Log("Simple winding of " + w.ToString());
                    }
                }
                if(GUILayout.Button("Winding Samples") == true)
                {
                    BNode bn = Utils.GetFirstInHash(this.selectedNodes);
                    float w = bn.parent.CalculateWindingSamples(bn, true);
                    Debug.Log("Simple winding of " + w.ToString());
                }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Calculate ArcLength") == true)
            { 
                HashSet<BLoop> loops = new HashSet<BLoop>();
                foreach(BNode bn in this.selectedNodes)
                    loops.Add(bn.parent);

                foreach(BLoop loop in loops)
                { 
                    float len = loop.CalculateArclen();
                    Debug.Log("Calculated arclen of " + len.ToString());
                }
            }

            if (GUILayout.Button("Calculate SampleLen") == true)
            {
                HashSet<BLoop> loops = new HashSet<BLoop>();
                foreach (BNode bn in this.selectedNodes)
                    loops.Add(bn.parent);

                foreach (BLoop loop in loops)
                {
                    float len = loop.CalculateSampleLens();
                    Debug.Log("Calculated sample len of " + len.ToString());
                }
            }

            GUILayout.EndHorizontal();

            if (GUILayout.Button("Calculate BBoxes") == true)
            {
                BoundsMM2 total = BoundsMM2.GetInifiniteRegion();

                foreach (BNode bn in this.selectedNodes)
                {
                    if (bn.next == null)
                        continue;

                    BoundsMM2 b2 = bn.GetBounds();
                    Debug.Log($"Node found to have bounds of region min {{{b2.min.x}, {b2.min.y}}} - and max {{{b2.max.x}, {b2.max.y} }}");

                    total.Union(b2);
                }

                Debug.Log($"Total selected bounds of region min {{{total.min.x}, {total.min.y}}} - and max {{{total.max.x}, {total.max.y} }}");
            }

            if(GUILayout.Button("Split into Thirds") == true)
            { 
                Vector2 pt0, pt1, pt2, pt3;

                foreach(BNode bn in this.selectedNodes)
                {
                    if(bn.next == null)
                        continue;

                    Utils.SubdivideBezier(bn.Pos, bn.Pos + bn.TanOut, bn.next.Pos + bn.next.TanIn, bn.next.Pos, out pt0, out pt1, out pt2, out pt3, 0.1f, 0.9f);
                    bn.Pos = pt0;
                    bn.TanOut = (pt1 - pt0);
                    bn.next.TanIn = (pt2 - pt3);
                    bn.next.Pos = pt3;
                }
            }

            GUILayout.Space(20.0f);

            this.intersectTestStart = EditorGUILayout.Vector2Field("Intersection Start", this.intersectTestStart);
            this.intersectTextEnd = EditorGUILayout.Vector2Field("Intersection End", this.intersectTextEnd);
            if(GUILayout.Button("Line Intersection Test") == true)
            { 

                this.intersectionPreviews.Clear();
                foreach(BNode node in this.selectedNodes)
                {
                    List<float> curveOuts = new List<float>();

                    if(node.next == null)
                        continue;

                    BNode.PathBridge pb = node.GetPathBridgeInfo(); 

                    Vector2 pt0 = node.Pos;
                    Vector2 pt1 = node.Pos + pb.prevTanOut;
                    Vector2 pt2 = node.next.Pos + pb.nextTanIn;
                    Vector2 pt3 = node.next.Pos;
                    int cols = 
                        Utils.IntersectLine(
                            curveOuts, 
                            null, 
                            pt0, 
                            pt1,
                            pt2,
                            pt3,
                            this.intersectTestStart,
                            this.intersectTextEnd);

                    for(int i = 0; i < cols; ++i)
                    {
                        float intLam = curveOuts[i];
                        float a, b, c, d;
                        Utils.GetBezierWeights(intLam, out a, out b, out c, out d);
                        this.intersectionPreviews.Add(a * pt0 + b * pt1 + c * pt2 + d * pt3);
                    }
                }

                if(this.intersectionPreviews.Count == 0)
                    Debug.Log("No collisions found");
                else
                    Debug.Log($"{this.intersectionPreviews.Count} Collisions found");
            }
        }

        if (t.curveDocument.IsDirty() == true)
            t.curveDocument.FlushDirty();
    }

    /// <summary>
    /// Render the tree data of the document, and render the outline and preview to the Scene view.
    /// </summary>
    private void OnSceneGUI()
    {
        BernyTest t = (BernyTest)this.target;
        if(t == null || t.curveDocument == null)
            return;

        Handles.BeginGUI();
            foreach(BNode selbn in this.selectedNodes)
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            GUILayout.Label($"Selected: {selbn.debugCounter.ToString()} - {selbn.tangentMode.ToString()} - {selbn.Pos.x} : {selbn.Pos.y}");
#else
            GUILayout.Label($"Selected: {selbn.tangentMode.ToSTring()} - {selbn.Pos.x} : {selbn.Pos.y}");
#endif

            this.DoUIDocument(t.curveDocument, 0.0f);
        Handles.EndGUI();



        bool recalculateAverage = false;

        for(int i = 0; i < this.intersectionPreviews.Count; ++i)
        { 
            Handles.Button(
                this.intersectionPreviews[i], 
                Quaternion.identity, 
                0.1f, 
                0.1f, 
                Handles.SphereHandleCap);
        }
        Handles.DrawLine(this.intersectTestStart, this.intersectTextEnd);

        // Draw all curves and show handles for them.
        Handles.color = Color.white;
        foreach (BNode bn in t.curveDocument.EnumerateNodes())
        {
            bool sel = this.selectedNodes.Contains(bn);
            if (sel == true)
                Handles.color = Color.green;
            else
                Handles.color = Color.white;

            bool inter = Handles.Button(bn.Pos, Quaternion.identity, 0.1f, 0.1f, Handles.CubeHandleCap);

            Handles.color = Color.white;
            if(inter == true)
            {
                recalculateAverage = true;

                if (Event.current.shift == true)
                { 
                    if(sel == true)
                        this.selectedNodes.Remove(bn);
                    else
                        this.selectedNodes.Add(bn);
                }
                else
                {
                    this.selectedNodes.Clear();
                    this.selectedNodes.Add(bn);
                }

                this.movedTangent = null;
            }

            BSample bit = bn.sample;
            while(bit.parent == bn && bit != null && bit.next != null)
            {
                Handles.DrawLine(bit.pos, bit.next.pos);
                bit = bit.next;
            }
        }
    
        // For selected items, show handles for their tangents.
        foreach(BNode bn in this.selectedNodes)
        {
            if(bn.UseTanIn == true)
            {
                Handles.color = Color.blue;
                if(Handles.Button(bn.Pos + bn.TanIn, Quaternion.identity, 0.05f, 0.05f, Handles.CubeHandleCap) == true)
                {
                    this.movedTangent = bn;
                    this.movedTangentType = BNode.TangentType.Input;
                }
                Handles.color = Color.white;
                Handles.DrawDottedLine(bn.Pos + bn.TanIn, bn.Pos, 1.0f);
            }

            //
            if(bn.UseTanOut == true)
            {
                Handles.color = Color.blue;
                if(Handles.Button(bn.Pos + bn.TanOut, Quaternion.identity, 0.05f, 0.05f, Handles.CubeHandleCap) == true)
                {
                    this.movedTangent = bn;
                    this.movedTangentType = BNode.TangentType.Output;
                }
                Handles.color = Color.white;
                Handles.DrawDottedLine(bn.Pos + bn.TanOut, bn.Pos, 1.0f);
            }
        }

        if(this.movedTangent != null)
        { 
            Vector3 oriPos = 
                (this.movedTangentType == BNode.TangentType.Input) ? 
                    this.movedTangent.TanIn : 
                    this.movedTangent.TanOut;

            oriPos += (Vector3)this.movedTangent.Pos;

            Vector3 tanPos = Handles.PositionHandle(oriPos, Quaternion.identity);

            if(tanPos != oriPos)
            { 
                Vector2 newTan = (Vector2)tanPos - this.movedTangent.Pos;
                if (this.movedTangentType == BNode.TangentType.Input)
                    this.movedTangent.TanIn = newTan;
                else
                    this.movedTangent.TanOut = newTan;

                this.movedTangent.FlagDirty();
            }
        }
        
        if(recalculateAverage == true)
            this.RecalculateSelectionCentroid();

        if(this.selectedNodes.Count > 0)
        { 
            Vector3 origAvg = this.averagePoint;
            Vector3 mod = Handles.PositionHandle(origAvg, Quaternion.identity);

            if(origAvg != mod)
            {
                Vector2 diff = mod - origAvg;
                this.averagePoint = mod;

                foreach(BNode bn in this.selectedNodes)
                { 
                    bn.Pos += diff;
                    bn.FlagDirty();
                }
            }
        }

        if (t.curveDocument.IsDirty() == true)
            t.curveDocument.FlushDirty();
    }

    /// <summary>
    /// Recalculate the average point of all selected nodes positions.
    /// </summary>
    void RecalculateSelectionCentroid()
    {
        float ct = 0.0f;
        this.averagePoint = Vector2.zero;

        foreach (BNode bn in this.selectedNodes)
        {
            ct += 1.0f;
            this.averagePoint += bn.Pos;
        }

        if (ct > 0.0f)
            this.averagePoint /= ct;
    }

    // Testing intersection functionality - logic going to be moved to relevant core classes.
    void ScanSelectedIntersections(Document doc)
    { 
        List<BNode> nodes = new List<BNode>(doc.EnumerateNodes());

        // Get rid of nodes that aren't line segments
        for(int i = nodes.Count - 1; i >= 0; --i)
        { 
            if(nodes[i].next == null)
                nodes.RemoveAt(i);
        }

        List<Utils.BezierSubdivSample> inter = new List<Utils.BezierSubdivSample>();

        // Check for self intersection
        foreach(BNode bn in nodes)
        {
            // TODO: Check path tangent function
            Vector2 p0 = bn.Pos;
            Vector2 p1 = bn.Pos + bn.TanOut;
            Vector2 p2 = bn.next.Pos + bn.next.TanIn;
            Vector2 p3 = bn.next.Pos;

            float r1, r2;

            List<float> roots = new List<float>();

            // For both the x and the y components, we're interested in in the roots, because
            // that's where a curve will turn back in on itself, so that marks a dividing line
            // where an intersection with itself can occur.

            // Check the roots for the X component
            int rs = Utils.GetRoots1DCubic(p0.x, p1.x, p2.x, p3.x, out r1, out r2);
            if(rs == 1)
            {
                roots.Add(r1);
            }
            else if(rs == 2)
            {
                roots.Add(r1);
                roots.Add(r2);
            }
            // Check the roots for the Y component
            rs = Utils.GetRoots1DCubic(p0.y, p1.y, p2.y, p3.y, out r1, out r2);
            if (rs == 1)
            {
                roots.Add(r1);
            }
            else if (rs == 2)
            {
                roots.Add(r1);
                roots.Add(r2);
            }

            // I don't feel like using linq in the document and polluting the intellisense
            // namespace, so we're going to do this the long way.
            roots = new List<float>(System.Linq.Enumerable.Distinct<float>(roots));
            roots.Sort();

            // Sanity check to get rid of stuff out of bounds. If I'm being honest, this should
            // already be filtered by the root finding functions.
            while(roots.Count > 0 && roots[0] <= 0.0f)
                roots.RemoveAt(0);
            while(roots.Count > 0 && roots[roots.Count - 1] >= 1.0f)
                roots.RemoveAt(roots.Count - 1);

            if(roots.Count > 0)
            { 
                // Add exactly one 0.0 and 1.0 in, while keeping the list ordered. This
                // just makes the process of looping through more elegant.
                roots.Insert(0, 0.0f);
                roots.Add(1.0f);

                for(int i = 0; i < roots.Count - 2; ++i)
                { 
                    for(int j = i + 1; j < roots.Count - 1; ++j)
                    {
                        Utils.BezierSubdivRgn rgnA = new Utils.BezierSubdivRgn();
                        rgnA.node = bn;
                        rgnA.lambda0 = roots[i];
                        rgnA.lambda1 = roots[i + 1];
                        rgnA.pt0 = bn.Pos;
                        rgnA.pt1 = bn.Pos + bn.TanOut;
                        rgnA.pt2 = bn.next.Pos + bn.next.TanIn;
                        rgnA.pt3 = bn.next.Pos;
                        rgnA.CalculateBounds();

                        Utils.BezierSubdivRgn rgnB = new Utils.BezierSubdivRgn();
                        rgnB.node = bn;
                        rgnB.lambda0 = roots[j];
                        rgnB.lambda1 = roots[j+1];
                        rgnB.pt0 = bn.Pos;
                        rgnB.pt1 = bn.Pos + bn.TanOut;
                        rgnB.pt2 = bn.next.Pos + bn.next.TanIn;
                        rgnB.pt3 = bn.next.Pos;
                        rgnB.CalculateBounds();

                        Utils.SubdivideSample(rgnA, rgnB, 20, Mathf.Epsilon, inter);
                    }
                }
            }
        }

        // Check for intersections with other nodes.
        //
        // Scan each node segment with each other. Probably won't be too big of a deal
        // because in a normal situation, the first bounding box check will bounce.
        for (int i = 0; i < nodes.Count; ++i)
        { 
            // TODO: Check path tangent function
            BNode bni = nodes[i];
            for(int j = i + 1; j < nodes.Count; ++j)
            {
                // TODO: Check path tangent function
                BNode bnj = nodes[j];

                Utils.BezierSubdivRgn rgnA = new Utils.BezierSubdivRgn();
                rgnA.node = bni;
                rgnA.lambda0 = 0.0f;
                rgnA.lambda1 = 1.0f;
                rgnA.pt0 = bni.Pos;
                rgnA.pt1 = bni.Pos + bni.TanOut;
                rgnA.pt2 = bni.next.Pos + bni.next.TanIn;
                rgnA.pt3 = bni.next.Pos;
                rgnA.CalculateBounds();

                Utils.BezierSubdivRgn rgnB = new Utils.BezierSubdivRgn();
                rgnB.node = bnj;
                rgnB.lambda0 = 0.0f;
                rgnB.lambda1 = 1.0f;
                rgnB.pt0 = bnj.Pos;
                rgnB.pt1 = bnj.Pos + bnj.TanOut;
                rgnB.pt2 = bnj.next.Pos + bnj.next.TanIn;
                rgnB.pt3 = bnj.next.Pos;
                rgnB.CalculateBounds();

                Utils.SubdivideSample(rgnA, rgnB, 20, Mathf.Epsilon, inter);
            }
        }

        // Get rid of similar ones, they're probably from numerical errors
        for(int i = 0; i < inter.Count; ++i)
        { 
            // Note how we're starting from the end going back next to i
            for(int j = inter.Count - 1; j > i; --j)
            { 
                // If they're different "enough" somehow, let it pass
                // and move on.
                if(inter[i].nodeA != inter[j].nodeA || inter[i].nodeB != inter[j].nodeB)
                    continue;

                if(inter[i].lAEst - inter[j].lAEst > 0.00001f)
                    continue;

                if (inter[i].lBEst - inter[j].lBEst > 0.00001f)
                    continue;

                // Or else, they're too similar
                inter.RemoveAt(j);
            }
        }

        foreach(Utils.BezierSubdivSample interS in inter)
        {
            if(interS.nodeA == interS.nodeB)
            {
                Debug.Log($"SELF Collision detected at range {interS.lA0} - {interS.lA1} for object 1 and {interS.lB0} - {interS.lB1} for object 2.");
            }
            else
            {
                Debug.Log($"Collision detected at range {interS.lA0} - {interS.lA1} for object 1 and {interS.lB0} - {interS.lB1} for object 2.");
            }
        }
    }

    /// <summary>
    /// UI function for drawing Document data to the Scene Window in a tree form.
    /// </summary>
    /// <param name="doc">The document to show content for.</param>
    /// <param name="indent">The acount to indent the tree information.</param>
    void DoUIDocument(Document doc, float indent)
    { 
        GUILayout.BeginHorizontal();
        GUILayout.Space(indent);
        GUILayout.Box("DOCUMENT");
        GUILayout.EndHorizontal();

        foreach(Layer layer in doc.Layers())
            this.DoUILayer(layer, indent + 20.0f);
    }

    /// <summary>
    /// UI function for drawing Layer data to the Scene window in a tree form.
    /// </summary>
    /// <param name="layer">The layer to show content for.</param>
    /// <param name="indent">The amount to indent the tree information.</param>
    void DoUILayer(Layer layer, float indent)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Space(indent);
        GUILayout.Box("Layer");
        layer.name = GUILayout.TextField( layer.name, GUILayout.Width(200.0f));
        GUILayout.EndHorizontal();

        foreach (BShape shape in layer.shapes)
            this.DoUIShape(shape, indent + 20.0f);
    }

    /// <summary>
    /// UI function for drawing Shape data to the Scene window in a tree form.
    /// </summary>
    /// <param name="shape">The shape to show content for.</param>
    /// <param name="indent">The amount to indent the tree information.</param>
    void DoUIShape(BShape shape, float indent)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Space(indent);
        GUILayout.Box("SHAPE");
        shape.name = GUILayout.TextField(shape.name, GUILayout.Width(200.0f));
        GUILayout.EndHorizontal();

        foreach(BLoop loop in shape.loops)
            this.DoUILoop(loop, indent + 20.0f);
    }

    /// <summary>
    /// UI function for drawing Loop data to the Scene window in a tree form.
    /// </summary>
    /// <param name="loop">The loop to show content for.</param>
    /// <param name="indent">The amount to indent the tree information.</param>
    void DoUILoop(BLoop loop, float indent)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Space(indent);
        if(GUILayout.Button($"LOOP({loop.nodes.Count})", GUILayout.ExpandWidth(false)) == true)
        { 
            if(Event.current.shift == false)
                this.selectedNodes.Clear();
            
            foreach(BNode bn in loop.nodes)
                this.selectedNodes.Add(bn);

            this.RecalculateSelectionCentroid();
            
        }
        GUILayout.EndHorizontal();
    }
}
