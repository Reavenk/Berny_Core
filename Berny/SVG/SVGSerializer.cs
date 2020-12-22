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
        public static class SVGSerializer
        { 
            public static bool Save(string filename, Document doc)
            { 
                System.Xml.XmlDocument xmlDoc = new System.Xml.XmlDocument();
                System.Xml.XmlElement root = xmlDoc.CreateElement("svg");
                xmlDoc.AppendChild(root);

                if(ConvertToXML(doc, xmlDoc, root) == false)
                    return false;

                xmlDoc.Save(filename);

                return true;
            }

            public static bool ConvertToXML(Document doc, System.Xml.XmlDocument xmldoc, System.Xml.XmlElement xmlroot)
            { 
                xmlroot.SetAttribute("version", "PxPreVector", "0.0.1");
                xmlroot.SetAttribute("xmlns:dc", "http://purl.org/dc/elements/1.1/");
                xmlroot.SetAttribute("xmlns:cc", "http://creativecommons.org/ns/#");
                xmlroot.SetAttribute("xmlns:rdf", "http://w3.org/1999/02/22-rdf-syntax-ns#");
                xmlroot.SetAttribute("xmlns:svg", "http://www.w3.org/2000/svg");
                xmlroot.SetAttribute("xmlns", "http://www.w3.org/2000/svg");
                xmlroot.SetAttribute("xmlns:sodipodi", "http://sodipodi.sourceforge.net/DTD/sodipodi-0.dtd");
                xmlroot.SetAttribute("xmlns:inkscape", "http://www.inkscape.org/namespaces/inkscape");

                xmlroot.SetAttribute("width", (doc.documentSize.x * 1000.0f).ToString() + "mm");
                xmlroot.SetAttribute("height", (doc.documentSize.y * 1000.0f).ToString() + "mm");
                xmlroot.SetAttribute("viewBox", $"0 0 {doc.documentSize.x} {doc.documentSize.y}");
                xmlroot.SetAttribute("version", "1.1");
                xmlroot.SetAttribute("id", "svg8");

                System.Xml.XmlElement eleDefs = xmldoc.CreateElement("defs"); // Empty for now
                xmlroot.AppendChild(eleDefs);

                System.Xml.XmlElement eleMetadata = xmldoc.CreateElement("metadata");
                xmlroot.AppendChild(eleMetadata);
                eleMetadata.SetAttribute("id", "metadata5");
                { 
                    System.Xml.XmlElement eleRDF = xmldoc.CreateElement("RDF", "rdf");
                    eleMetadata.AppendChild(eleRDF);
                    { 
                        System.Xml.XmlElement eleWork = xmldoc.CreateElement("Work", "cc");
                        eleRDF.AppendChild(eleWork);
                        eleWork.SetAttribute("rdf:about", "");
                        { 
                            System.Xml.XmlElement eleDCFormat = xmldoc.CreateElement("format", "dc");
                            eleWork.AppendChild(eleDCFormat);
                            eleDCFormat.InnerText = "image/svg+xml";

                            System.Xml.XmlElement eleDCType = xmldoc.CreateElement("type", "dc");
                            eleWork.AppendChild(eleDCType);
                            eleDCType.SetAttribute("rdf:resource", "http://purl.org/dc/dcmitype/StillImage");

                            System.Xml.XmlElement eleDCTitle = xmldoc.CreateElement("title", "dc");
                            eleWork.AppendChild(eleDCTitle);
                        }
                    }
                }

                int layerOrder = 1;
                foreach(Layer layer in doc.Layers())
                { 
                    // We're jst going to do 
                    System.Xml.XmlElement xmlLayer = xmldoc.CreateElement("g");
                    xmlroot.AppendChild(xmlLayer);

                    xmlLayer.SetAttribute("label", "inkscape", layer.name);
                    xmlLayer.SetAttribute("groupmode", "inkscape", "layer");
                    xmlLayer.SetAttribute("id", "layer" + layerOrder.ToString());
                    xmlLayer.SetAttribute("style", layer.Visible ? "display:inline" : "display:none");

                    if(layer.Locked == true)
                        xmlLayer.SetAttribute("sodipodi:insensitive", "true");

                    foreach(BShape shape in layer.shapes)
                    {
                        string shapetype = "path";
                        if(shape.shapeGenerator != null)
                            shapetype = shape.shapeGenerator.GetSVGXMLName;

                        System.Xml.XmlElement eleShape = xmldoc.CreateElement(shapetype);
                        xmlLayer.AppendChild(eleShape);
                        {
                            string styleAttrib = "";
                            if(shape.fill == true)
                            { 
                                styleAttrib += "fill: #" + Utils.ConvertColorToHex6(shape.fillColor) + "; ";
                                styleAttrib += "fill-opacity: " + shape.fillColor.a.ToString() + "; ";
                            }
                            else
                            {
                                styleAttrib += "fill: none; ";
                            }

                            if(shape.stroke == true)
                            {
                                styleAttrib += "stroke: #" + Utils.ConvertColorToHex6(shape.strokeColor) + "; ";
                                styleAttrib += "stroke-opacity: " + shape.strokeColor.a.ToString() + "; ";
                                styleAttrib += "stroke-width: " + shape.strokeWidth.ToString() + "; ";

                                // TODO: The order of this stuff probably dictates the draw order attribute.
                                styleAttrib += "stroke-linecap: " + BShape.CapToString(shape.cap) + "; ";
                                styleAttrib += "stroke-miterlimit: " + shape.maxMitreLength.ToString() + "; ";
                                styleAttrib += "stroke-linejoin: " + BShape.CornerToString(shape.corner) + "; ";
                                styleAttrib += "stroke-dasharray: none; ";
                            }
                            else
                            { 
                                styleAttrib += "stroke: none; ";
                            }
                            eleShape.SetAttribute("style", styleAttrib);

                            // If we're a generated shape, ignore our explicit path data and just let the generate
                            // save the SVG stuff - if not, it's a path and we'll fallback on saving the explicit path.
                            if(shape.shapeGenerator != null)
                            { 
                                shape.shapeGenerator.SaveToSVGXML(eleShape);
                                continue;
                            }

                            // https://developer.mozilla.org/en-US/docs/Web/SVG/Attribute/d
                            // SVG defines 6 types of path commands, for a total of 20 commands:
                            // MoveTo: M, m
                            // LineTo: L, l, H, h, V, v
                            // Cubic Bézier Curve: C, c, S, s
                            // Quadratic Bézier Curve: Q, q, T, t
                            // Elliptical Arc Curve: A, a
                            // ClosePath: Z, z

                            string drawAttrib = "";
                            string lastSymbol = "";
                            foreach(BLoop loop in shape.loops)
                            { 
                                HashSet<BNode> nodesLeft = new HashSet<BNode>(loop.nodes);
                                while(nodesLeft.Count > 0)
                                { 
                                    BNode.EndpointQuery eq = Utils.GetFirstInHash(nodesLeft).GetPathLeftmost();
                                    BNode it = eq.node;

                                    drawAttrib += $"M {eq.node.Pos.x}, {eq.node.Pos.y}";
                                    while(true)
                                    { 
                                        if(it == null)
                                            break;

                                        nodesLeft.Remove(it);
                                        if(it.next == null)
                                            break;

                                        if(it.UseTanOut == false && it.UseTanIn == false)
                                        { 
                                            if(lastSymbol != "L")
                                            {
                                                drawAttrib += " L";
                                                lastSymbol = "L";
                                            }

                                            drawAttrib += $"  {it.next.Pos.x.ToString()}, {it.next.Pos.y.ToString()}";
                                        }
                                        else
                                        { 
                                            Vector2 curOut = Vector2.zero;
                                            Vector2 nextIn = Vector2.zero;

                                            if(it.UseTanOut == true)
                                                curOut = it.TanOut;

                                            if(it.next != null && it.next.UseTanIn == true)
                                                nextIn = it.next.TanIn;

                                            if(lastSymbol != "C")
                                            {
                                                drawAttrib += " C";
                                                lastSymbol = "C";
                                            }

                                            drawAttrib += $" {it.Pos.x + curOut.x},{it.Pos.y + curOut.y} {it.next.Pos.x + nextIn.x},{it.next.Pos.y + nextIn.y} {it.next.Pos.x},{it.next.Pos.y}";

                                        }

                                        it = it.next;

                                        if (it == eq.node)
                                        {
                                            drawAttrib += " Z";
                                            break;
                                        }

                                    }
                                }
                            }

                            elePath.SetAttribute("d", drawAttrib);

                            elePath.SetAttribute("id", shape.name);
                        }
                    }
                }

                return true;
            }

            public static bool Load(string filename, Document doc)
            { 
                System.Xml.XmlDocument xmlDoc = new System.Xml.XmlDocument();
                try
                {
                    xmlDoc.Load(filename);
                }
                catch(System.Exception ex)
                { 
                    Debug.Log("Error loading SVG: " + ex.Message);
                    return false;
                }
                return ConvertFromXML(xmlDoc, doc);
            }

            public static bool ConvertFromXML(System.Xml.XmlDocument xmlDoc, Document doc)
            { 
                System.Xml.XmlElement root = xmlDoc.DocumentElement;
                if(root.Name != "svg")
                    return false;

                Vector2 docSz = doc.documentSize;

                System.Xml.XmlAttribute attrWidth = root.Attributes["width"];
                if(attrWidth != null)
                { 
                    float num;
                    Utils.LengthUnit lu;
                    //
                    if( Utils.ExtractLengthString(attrWidth.Value, out num, out lu) == true)
                        docSz.x = Utils.ConvertUnitsToMeters(num, lu);
                }

                System.Xml.XmlAttribute attrHeight = root.Attributes["height"];
                if(attrHeight != null)
                { 
                    float num;
                    Utils.LengthUnit lu;
                    //
                    if( Utils.ExtractLengthString(attrHeight.Value, out num, out lu) == true)
                        docSz.y = Utils.ConvertMetersToUnit(num, lu);
                }

                System.Xml.XmlAttribute attrView = root.Attributes["viewBox"];
                if(attrView != null && string.IsNullOrEmpty(attrView.Value) == false)
                { 
                    string [] strs = attrView.Value.Trim().Split(new char[]{' ' }, System.StringSplitOptions.RemoveEmptyEntries);
                    if(strs.Length == 4)
                    { 
                        // TODO: Do something with X and Y ([0] and [1]).
                        float.TryParse( strs[2], out docSz.x);
                        float.TryParse( strs[3], out docSz.y);
                        doc.documentSize = docSz;
                    }
                }

                doc.documentSize = docSz;

                List<System.Xml.XmlElement> graphicEles = new List<System.Xml.XmlElement>();

                foreach (System.Xml.XmlElement ele in EnumerateChildElements(root))
                {
                    if (ele.Name == "defs")
                    {}
                    else if (ele.Name == "g")
                        graphicEles.Add(ele);
                    else
                    {
                        // It could just be a shape at the root level.
                        // If not, CreateShapeFromXML will silently and gracefully do nothing.
                        // but the return value can be checked if it's non-null.
                        CreateShapeFromXML(ele, doc, null);
                    }
                }

                foreach(System.Xml.XmlElement eleLayer in graphicEles)
                {
                    Layer layer = doc.AddLayer();

                    System.Xml.XmlAttribute attribStyle = GetAttributeFromXMLElement(eleLayer, "label", "inkscape");
                    if (attribStyle != null)
                        layer.name = attribStyle.Value;

                    System.Xml.XmlAttribute attribMode = GetAttributeFromXMLElement(eleLayer, "groupmode", "inkscape");
                    if(attribMode != null)
                        if(attribMode.Value != "layer")
                        { }

                    System.Xml.XmlAttribute attribId = eleLayer.Attributes["id"];
                    if(attribId != null)
                    { 
                        string layerNum = attribId.Value;
                        // TODO:
                    }

                    System.Xml.XmlAttribute attriStyle = eleLayer.Attributes["style"];
                    if(attriStyle != null)
                    { 
                        Dictionary<string,string> d = Utils.SplitProperties(attriStyle.Value);
                        string styleValue;
                        if(d.TryGetValue("display", out styleValue) == true)
                        { 
                            if(styleValue == "inline")
                                layer.Visible = true;
                            else if(styleValue == "none")
                                layer.Visible = false;
                        }
                    }

                    //"stroke"
                    //"fill"
                    //"stroke-width"
                    //"opacity"

                    System.Xml.XmlAttribute attriLock = GetAttributeFromXMLElement(eleLayer, "insensitive", "sodipodi");
                    if(attriLock != null && attriLock.Value == "true")
                        layer.Locked = true;

                    foreach(System.Xml.XmlElement eleLE in EnumerateChildElements(eleLayer))
                        CreateShapeFromXML(eleLE, doc, layer);
                }

                return true;
            }

            public static Layer ResolveUsableLayer(Document doc, Layer layer)
            { 
                if(layer == null)
                    return doc.GetFirstLayer();

                return layer;
            }

            public static BShape CreateShapeFromXML(System.Xml.XmlElement ele, Document doc, Layer layer)
            {
                BShape bs = null;
                if (ele.Name == "path")
                {
                    SVGMat mat;
                    bs = CreateTemplateShapeFromXML(ResolveUsableLayer(doc,layer), ele, out mat);

                    System.Xml.XmlAttribute attrPathDraw = ele.Attributes["d"];
                    if (attrPathDraw != null)
                        ProcessPathDrawAttrib(bs, attrPathDraw.Value, mat);
                }
                else if (ele.Name == "rect")
                {
                    SVGMat mat;
                    bs = CreateTemplateShapeFromXML(ResolveUsableLayer(doc, layer), ele, out mat);

                    BShapeGenRect gen = new BShapeGenRect(bs, Vector2.zero, Vector2.one);
                    gen.LoadFromSVGXML(ele);
                    bs.shapeGenerator = gen;

                }
                else if (ele.Name == "ellipse")
                {
                    SVGMat mat;
                    bs = CreateTemplateShapeFromXML(ResolveUsableLayer(doc, layer), ele, out mat);

                    BShapeGenEllipse gen = new BShapeGenEllipse(bs, Vector2.zero, Vector2.one);
                    gen.LoadFromSVGXML(ele);
                    bs.shapeGenerator = gen;
                }
                else if (ele.Name == "circle")
                {
                    SVGMat mat;
                    bs = CreateTemplateShapeFromXML(ResolveUsableLayer(doc, layer), ele, out mat);

                    BShapeGenCircle gen = new BShapeGenCircle(bs, Vector2.zero, 1.0f);
                    gen.LoadFromSVGXML(ele);
                    bs.shapeGenerator = gen;
                }
                else if (ele.Name == "polyline")
                {
                    SVGMat mat;
                    bs = CreateTemplateShapeFromXML(ResolveUsableLayer(doc, layer), ele, out mat);

                    BShapeGenPolyline gen = new BShapeGenPolyline(bs);
                    gen.LoadFromSVGXML(ele);
                    bs.shapeGenerator = gen;
                }
                else if (ele.Name == "polygon")
                {
                    SVGMat mat;
                    bs = CreateTemplateShapeFromXML(ResolveUsableLayer(doc, layer), ele, out mat);

                    BShapeGenPolygon gen = new BShapeGenPolygon(bs);
                    gen.LoadFromSVGXML(ele);
                    bs.shapeGenerator = gen;
                }
                else if (ele.Name == "line")
                {
                    SVGMat mat;
                    bs = CreateTemplateShapeFromXML(ResolveUsableLayer(doc, layer), ele, out mat);

                    BShapeGenLine gen = new BShapeGenLine(bs, Vector2.zero, Vector2.zero);
                    gen.LoadFromSVGXML(ele);
                    bs.shapeGenerator = gen;
                }

                if (bs != null)
                    bs.FlagDirty();

                return bs;
            }

            public static BShape CreateTemplateShapeFromXML(Layer layer, System.Xml.XmlElement ele, out SVGMat mat)
            {
                BShape bs = new BShape(Vector2.zero, 0.0f);
                layer.shapes.Add(bs);
                bs.layer = layer;

                LoadShapeInfo(bs, ele, out mat);
                return bs;
            }

            public static List<string> SplitDrawCommand(string drawCmd)
            { 
                int idx = 0;
                List<string> ret = new List<string>();
                while(idx < drawCmd.Length)
                { 
                    if(idx >= drawCmd.Length)
                        break;

                    if(ConsumeWhitespace(drawCmd, ref idx) == false)
                        break;

                    if(drawCmd[idx] == ',')
                    {
                        ret.Add(",");
                        ++idx;
                    }
                    else
                    { 
                        // Get the length up to a whitespace or comma
                        int orig = idx;

                        if (idx >= drawCmd.Length)
                            break;

                        while (true)
                        {
                            ++idx;
                            if (idx >= drawCmd.Length)
                                break;

                            if (char.IsWhiteSpace(drawCmd[idx]) == true || drawCmd[idx] == ',')
                                break;
                        }

                        // If consuming content didn't move the read cursor, we have a situation
                        // we're aborting from. Most likely the readhead was at the end.
                        if (orig == idx)
                            break;

                        ret.Add( drawCmd.Substring(orig, idx - orig));
                    }
                }
                return ret;
            }

            public static List<Vector2> SplitPointsString(string pointsString)
            { 
                List<Vector2> ret = new List<Vector2>();

                // Find a command, find a whitespace, 
                // parse in between, don't worry about any other
                // whitespace

                int idx = 0;
                while(idx < pointsString.Length)
                { 
                    int comma = pointsString.IndexOf(',', idx);

                    if(comma == -1)
                        break;


                    int end = comma + 1;

                    // Eat any whitespace
                    while( true)
                    { 
                        if(end >= pointsString.Length)
                            break;

                        if(char.IsWhiteSpace(pointsString[end]) == false)
                            break;

                        ++end;
                    }

                    if(end >= pointsString.Length)
                        break;
                    
                    // Eat to next whitespace or end
                    while(true)
                    { 
                        if(end >= pointsString.Length)
                            break;

                        if(char.IsWhiteSpace(pointsString[end]) == true)
                            break;

                        ++end;
                    }

                    // If nothing was eaten for the Y, we have a degenerate point
                    if(end == comma + 1)
                        break;


                    string strX = pointsString.Substring(idx, comma - idx).Trim();
                    string strY = pointsString.Substring(comma + 1, end - comma - 1).Trim();

                    Vector2 newV = new Vector2();
                    if(float.TryParse(strX, out newV.x) == false)
                        break;

                    if(float.TryParse(strY, out newV.y) == false)
                        break;

                    ret.Add(newV);

                    idx = end;
                }

                return ret;
            }

            public static string PointsToPointsString(IEnumerable<Vector2> ieV2)
            { 
                List<string> strs = new List<string>();
                foreach(Vector2 v2 in ieV2)
                    strs.Add(v2.x.ToString() + "," + v2.y.ToString());

                return string.Join(" ", strs);
            }

            public static void LoadShapeInfo(BShape shape, System.Xml.XmlElement ele, out SVGMat matrix)
            {
                System.Xml.XmlAttribute attrPathID = ele.Attributes["id"];
                if (attrPathID != null)
                    ProcessShapeIdAttrib(shape, attrPathID.Value);

                System.Xml.XmlAttribute attrPathStyle = ele.Attributes["style"];
                if (attrPathStyle != null)
                    ProcessShapeStyleAttrib(shape, attrPathStyle.Value);


                System.Xml.XmlAttribute attrTrans = ele.Attributes["transform"];
                matrix = (attrTrans != null) ? ProcessMatrixAttribute(attrTrans.Value) : SVGMat.Identity();
            }

            public static void ProcessPathDrawAttrib(BShape shape, string attrib, SVGMat mat)
            {
                BLoop curLoop = null;
                BNode prevNode = null;
                BNode firstNode = null;
                Vector2 lastPos = Vector2.zero;

                // https://www.w3schools.com/graphics/svg_path.asp

                List<string> parts = SplitDrawCommand(attrib);

                int i = 0;
                char lastCmd = (char)0;
                while (i < parts.Count)
                {
                    // Did we parse the command on this loop iter?
                    bool parsedLetter = false;
                    if (parts[i].Length == 1 && char.IsLetter(parts[i][0]) == true)
                    {
                        lastCmd = parts[i][0];
                        parsedLetter = true;
                        ++i;
                    }

                    if (lastCmd == 'm') // Relative Move To
                    {
                        Vector2 v;
                        if (ConsumeVector2(parts, ref i, out v) == false)
                            break; // Aborting

                        v = lastPos + v;

                        if (parsedLetter == false)
                        {
                            EnsureLoopAndNode(shape, ref curLoop, lastPos, ref mat, ref firstNode, ref prevNode);
                            BNode node = new BNode(curLoop, mat.Mul(v));
                            prevNode.next = node;
                            node.prev = prevNode;
                            node.UseTanOut = false;
                            node.UseTanIn = false;
                            curLoop.nodes.Add(node);
                            prevNode = node;
                        }

                        lastPos = v;


                    }
                    else if (lastCmd == 'M') // Global Move To
                    {
                        Vector2 v;
                        if (ConsumeVector2(parts, ref i, out v) == false)
                            break; //Aborting

                        if (parsedLetter == false)
                        {
                            EnsureLoopAndNode(shape, ref curLoop, lastPos, ref mat, ref firstNode, ref prevNode);
                            BNode node = new BNode(curLoop, mat.Mul(v));
                            prevNode.next = node;
                            node.prev = prevNode;
                            node.UseTanOut = false;
                            node.UseTanIn = false;
                            curLoop.nodes.Add(node);
                            prevNode = node;
                        }

                        lastPos = v;
                    }
                    else if (lastCmd == 'l' || lastCmd == 'L') // Relative and Global Line To
                    {
                        Vector2 v;
                        if (ConsumeVector2(parts, ref i, out v) == false)
                            break; //Aborting

                        EnsureLoopAndNode(shape, ref curLoop, lastPos, ref mat, ref firstNode, ref prevNode);

                        if (lastCmd == 'l') // if relative
                            v += lastPos;

                        BNode node = new BNode(curLoop, mat.Mul(v));
                        curLoop.nodes.Add(node);
                        node.prev = prevNode;
                        prevNode.next = node;
                        prevNode.UseTanOut = false;
                        node.UseTanIn = false;

                        prevNode = node;
                        lastPos = v;
                    }
                    else if (lastCmd == 'h' || lastCmd == 'H') // Relative or Global Horizontal Line
                    {
                        float f;
                        if (ConsumeFloat(parts, ref i, out f) == false)
                            break; // Aborting

                        EnsureLoopAndNode(shape, ref curLoop, lastPos, ref mat, ref firstNode, ref prevNode);

                        Vector2 v = lastPos;
                        if (lastCmd == 'h')
                            v.x += f; // Relative
                        else
                            v.x = f; // Global

                        BNode node = new BNode(curLoop, mat.Mul(v));
                        curLoop.nodes.Add(node);
                        node.prev = prevNode;
                        prevNode.next = node;
                        prevNode.UseTanOut = false;
                        node.UseTanIn = false;

                        prevNode = node;
                        lastPos = v;
                    }
                    else if (lastCmd == 'v' | lastCmd == 'V') // Relative or Global Vertical Line
                    {
                        float f;
                        if (ConsumeFloat(parts, ref i, out f) == false)
                            break;

                        EnsureLoopAndNode(shape, ref curLoop, lastPos, ref mat, ref firstNode, ref prevNode);

                        Vector2 v = lastPos;
                        if (lastCmd == 'v')
                            v.y += f; // Relative
                        else
                            v.y = f; // Global

                        BNode node = new BNode(curLoop, mat.Mul(v));
                        curLoop.nodes.Add(node);
                        node.prev = prevNode;
                        prevNode.next = node;
                        prevNode.UseTanOut = false;
                        node.UseTanIn = false;

                        prevNode = node;
                        lastPos = v;
                    }
                    else if (lastCmd == 'c' || lastCmd == 'C') // Relative or Global Cubic
                    {
                        Vector2 tcurout, tnxtin, v;
                        if (
                            ConsumeVector2(parts, ref i, out tcurout) == false ||
                            ConsumeVector2(parts, ref i, out tnxtin) == false ||
                            ConsumeVector2(parts, ref i, out v) == false)
                        {
                            break; //Aborting
                        }

                        EnsureLoopAndNode(shape, ref curLoop, lastPos, ref mat, ref firstNode, ref prevNode);

                        // Relative
                        if (lastCmd == 'c')
                        {
                            tcurout += lastPos;
                            tnxtin += lastPos;
                            v += lastPos;
                        }

                        BNode node = new BNode(curLoop, mat.Mul(v));
                        curLoop.nodes.Add(node);
                        //
                        node.prev = prevNode;
                        prevNode.next = node;
                        //
                        prevNode.UseTanOut = true;
                        prevNode.TanOut = mat.Mul(tcurout) - prevNode.Pos;
                        node.UseTanIn = true;
                        node.TanIn = mat.Mul(tnxtin) - node.Pos;

                        prevNode = node;
                        lastPos = v;
                    }
                    else if (lastCmd == 's' || lastCmd == 'S') // Relative Cubic Smooth
                    {
                        Vector2 t, v;
                        if (
                            ConsumeVector2(parts, ref i, out t) == false ||
                            ConsumeVector2(parts, ref i, out v) == false)
                        {
                            break; // Aborting
                        }

                        EnsureLoopAndNode(shape, ref curLoop, lastPos, ref mat, ref firstNode, ref prevNode);

                        // Relative
                        if (lastCmd == 'S')
                        {
                            t += lastPos;
                            v += lastPos;
                        }

                        BNode node = new BNode(curLoop, mat.Mul(v));
                        curLoop.nodes.Add(node);
                        node.prev = prevNode;
                        prevNode.next = node;
                        prevNode.UseTanOut = true;
                        prevNode.TanOut = mat.Mul(t) - prevNode.Pos;
                        node.UseTanOut = true;
                        node.TanIn = mat.Mul(t) - node.Pos;

                        prevNode = node;
                        lastPos = v;
                    }
                    else if (lastCmd == 'q') // Relative Quadratic
                    {
                        // UNIMPLEMENTED:
                    }
                    else if (lastCmd == 'Q') // Global Quadratic
                    {
                        // UNIMPLEMENTED:
                    }
                    else if (lastCmd == 't') // Relative Quadratic Smooth
                    {
                        // UNIMPLEMENTED:
                    }
                    else if (lastCmd == 'T') // Global Quadratic Smooth
                    {
                        // UNIMPLEMENTED:
                    }
                    else if (lastCmd == 'a') // Relative elliptical arc curve
                    {
                        // UNIMPLEMENTED:
                    }
                    else if (lastCmd == 'A') // Global elliptical arc curve
                    {
                        // UNIMPLEMENTED:
                    }
                    else if (lastCmd == 'z' || lastCmd == 'Z')
                    {
                        // Close and add loop
                        if (curLoop != null)
                        {
                            if (firstNode != null && prevNode != null)
                            {
                                // Oh geeze! Technically this is what it should be, but I don't think the
                                // code is currently robust enough to handle a 1 point curve.
                                if (firstNode == prevNode)
                                {
                                    firstNode.next = firstNode;
                                    firstNode.prev = firstNode;
                                }
                                else if (firstNode.Pos == prevNode.Pos)
                                {
                                    // If they're positioned on the exact same spot, we turn
                                    // it into a single point
                                    curLoop.nodes.Remove(prevNode);
                                    firstNode.prev = prevNode.prev; // Disconnect the prev node from the chain and form a loop without it
                                    prevNode.prev.next = firstNode;


                                    firstNode.UseTanIn = prevNode.UseTanIn;
                                    firstNode.TanIn = prevNode.TanIn;
                                }
                                else
                                {
                                    // If they're not the same spot, we connect them with a line
                                    firstNode.prev = prevNode;
                                    firstNode.TanIn = Vector2.zero;
                                    firstNode.UseTanIn = false;

                                    prevNode.next = firstNode;
                                    prevNode.TanOut = Vector2.zero;
                                    prevNode.UseTanOut = false;
                                }
                            }

                            shape.AddLoop(curLoop);
                            curLoop = null;
                            prevNode = null;
                            firstNode = null;

                            lastCmd = (char)0;
                        }
                    }
                    else
                    {
                        break;  // TODO: ERROR
                    }
                }


                if (curLoop != null)
                {
                    shape.AddLoop(curLoop);
                    curLoop = null;
                    prevNode = null;
                    firstNode = null;
                }
            }

            public static SVGMat ProcessMatrixAttribute(string attrib)
            {
                SVGMat ret = SVGMat.Identity();

                if (attrib.StartsWith("matrix(") == true)
                {
                    int matlen = "matrix(".Length;
                    // Not exactly robust parsing - an extra -1 for the closing parenthesis
                    string matValues = attrib.Substring(matlen, attrib.Length - matlen - 1);
                    string[] vals = matValues.Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);

                    float.TryParse(vals[0].Trim(), out ret.x.x);
                    float.TryParse(vals[1].Trim(), out ret.x.y);
                    float.TryParse(vals[2].Trim(), out ret.y.x);
                    float.TryParse(vals[3].Trim(), out ret.y.y);
                    float.TryParse(vals[4].Trim(), out ret.t.x);
                    float.TryParse(vals[5].Trim(), out ret.t.y);
                }

                return ret;
            }

            public static void ProcessShapeStyleAttrib(BShape shape, string attrib)
            {
                Dictionary<string, string> styles = Utils.SplitProperties(attrib);
                string fill;
                if (styles.TryGetValue("fill", out fill) == true)
                {
                    if (fill == "none")
                        shape.fill = false;
                    else
                    {
                        shape.fill = true;
                        shape.fillColor = Utils.ConvertSVGStringToColor(fill);
                    }
                }

                string fillopacity;
                if (styles.TryGetValue("fill-opacity", out fillopacity) == true)
                {
                    float opacity;
                    if (float.TryParse(fillopacity, out opacity) == true)
                        shape.fillColor.a = opacity;
                }

                string stroke;
                if (styles.TryGetValue("stroke", out stroke) == true)
                {
                    if (stroke == "none")
                        shape.stroke = false;
                    else
                    {
                        shape.stroke = true;
                        shape.strokeColor = Utils.ConvertSVGStringToColor(fill);
                    }
                }

                string strokeopacity;
                if (styles.TryGetValue("stroke-opacity", out strokeopacity) == true)
                {
                    float opacity;
                    if (float.TryParse(strokeopacity, out opacity) == true)
                        shape.strokeColor.a = opacity;
                }

                string strokewidth;
                if (styles.TryGetValue("stroke-width", out strokewidth) == true)
                {
                    float num;
                    Utils.LengthUnit unit;
                    if (Utils.ExtractLengthString(strokewidth, out num, out unit) == true)
                        shape.strokeWidth = Utils.ConvertUnitsToMeters(num, unit);
                }


                string strokecap;
                if (styles.TryGetValue("stroke-linecap", out strokecap) == true)
                    shape.cap = BShape.StringToCap(strokecap);

                string strokejoin;
                if (styles.TryGetValue("stroke-linejoin", out strokejoin) == true)
                    shape.corner = BShape.StringToCorner(strokejoin);
            }

            public static void ProcessShapeIdAttrib(BShape shape, string attrib)
            { 
                shape.name = attrib;
            }

            public static bool ConsumeWhitespace(string str, ref int idx)
            { 
                if(idx >= str.Length)
                    return false;

                while(true)
                {
                    if (char.IsWhiteSpace(str[idx]) == false)
                        break;

                    ++idx;
                    if(idx >= str.Length)
                        return false;
                }
                return true;
            }

            public static bool ConsumeVector2(List<string> lst, ref int idx, out Vector2 vecOut)
            { 
                if(lst.Count < idx + 2 || lst[idx + 1] != ",")
                {
                    vecOut = Vector2.zero;
                    return false;
                }

                string strX = lst[idx + 0];
                string strY = lst[idx + 2];

                vecOut = new Vector2();

                if(float.TryParse(strX, out vecOut.x) == false)
                    return false;

                if (float.TryParse(strY, out vecOut.y) == false)
                    return false;

                idx += 3;
                return true;
            }

            public static bool ConsumeFloat(List<string> lst, ref int idx, out float f)
            { 
                if(idx >= lst.Count)
                { 
                    f = 0.0f;
                    return false;
                }

                bool ret = float.TryParse(lst[idx], out f);
                ++idx;
                return ret;
            }

            public static void EnsureLoopAndNode(BShape shape, ref BLoop loop, Vector2 lastPos, ref SVGMat mat, ref BNode first, ref BNode prev)
            { 
                if(loop == null)
                { 
                    loop = new BLoop(shape);

                    first = null;
                    prev = null;
                }

                if(prev == null)
                {
                    first = new BNode(loop, mat.Mul(lastPos));
                    loop.nodes.Add(first);
                    prev = first;

                    // Anything that needs the tangents can set them later.
                    first.UseTanIn = false;
                    first.UseTanOut = false;

                    first.TanIn = Vector2.zero;
                    first.TanOut = Vector2.zero;
                }
            }

            public static System.Xml.XmlAttribute GetAttributeFromXMLElement(System.Xml.XmlElement ele, string attr, string xmlns)
            {
                if(string.IsNullOrEmpty(xmlns) == true)
                    return ele.GetAttributeNode(attr);

                System.Xml.XmlAttribute xmlAttr = ele.GetAttributeNode(attr, xmlns);
                if(xmlAttr != null)
                    return xmlAttr;

                return ele.GetAttributeNode($"{ xmlns}:{ attr}");
            }

            public static bool AttribToFloat(System.Xml.XmlAttribute attr, ref float f)
            { 
                if(attr == null || string.IsNullOrEmpty(attr.Value) == true)
                    return false;

                float pf;
                if(float.TryParse(attr.Value, out pf) == true)
                { 
                    f = pf;
                    return true;
                }
                return false;
            }

            public static IEnumerable<System.Xml.XmlElement> EnumerateChildElements(System.Xml.XmlElement ele)
            { 
                foreach(System.Xml.XmlNode node in ele)
                { 
                    if(node.NodeType == System.Xml.XmlNodeType.Element == false)
                        continue;

                    System.Xml.XmlElement childEle = node as System.Xml.XmlElement;
                    yield return childEle;
                }
            }
        }
    }
}