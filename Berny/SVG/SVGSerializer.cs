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
    /// Utility class to save and load a Berny document to/from SVG.
    /// </summary>
    /// <remarks>This is not a comprehensive utility. It's just the basic for simple
    /// stuff, and for quickly loading test scenes.</remarks>
    public static class SVGSerializer
    { 
        /// <summary>
        /// Save a Berny document as an SVG.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <param name="doc">The document to save.</param>
        /// <param name="invertY">If true, the document is vertically inverted when saved.</param>
        /// <returns>If true, the docuement successfuly saved. Else, false.</returns>
        public static bool Save(string filename, Document doc, bool invertY = true)
        { 
            System.Xml.XmlDocument xmlDoc = new System.Xml.XmlDocument();
            System.Xml.XmlElement root = xmlDoc.CreateElement("svg");
            xmlDoc.AppendChild(root);

            if(ConvertToXML(doc, xmlDoc, root, invertY) == false)
                return false;

            xmlDoc.Save(filename);

            return true;
        }

        /// <summary>
        /// Convert the document into XML form.
        /// 
        /// This function is used for saving the document as an SVG.
        /// </summary>
        /// <param name="doc">The document to convert.</param>
        /// <param name="xmldoc">The XML Document used to generate elements for the SVG.</param>
        /// <param name="xmlroot">The XML element to insert the child elements into.</param>
        /// <param name="invertY">If true, the document is vertically inverted when saved.</param>
        /// <returns>If true, the document successfully converted. Else, false.</returns>
        public static bool ConvertToXML(Document doc, System.Xml.XmlDocument xmldoc, System.Xml.XmlElement xmlroot, bool invertY)
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
                            shape.shapeGenerator.SaveToSVGXML(eleShape, invertY);
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
                            if(string.IsNullOrEmpty(drawAttrib) == false)
                                drawAttrib += " ";

                            HashSet<BNode> nodesLeft = new HashSet<BNode>(loop.nodes);
                            while(nodesLeft.Count > 0)
                            { 
                                BNode.EndpointQuery eq = Utils.GetFirstInHash(nodesLeft).GetPathLeftmost();
                                BNode it = eq.node;

                                drawAttrib += $"M {eq.node.Pos.x}, {InvertBranch(eq.node.Pos.y, invertY)}";
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

                                        drawAttrib += $"  {it.next.Pos.x.ToString()}, {InvertBranch(it.next.Pos.y, invertY).ToString()}";
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

                                        drawAttrib += $" {it.Pos.x + curOut.x},{InvertBranch(it.Pos.y + curOut.y, invertY)} {it.next.Pos.x + nextIn.x},{InvertBranch(it.next.Pos.y + nextIn.y, invertY)} {it.next.Pos.x},{InvertBranch(it.next.Pos.y, invertY)}";

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

                        eleShape.SetAttribute("d", drawAttrib);

                        eleShape.SetAttribute("id", shape.name);
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Load the contents of an SVG document into a Berny document.
        /// </summary>
        /// <param name="filename">The path of the file to load.</param>
        /// <param name="doc">The document to load the contents into.</param>
        /// <param name="invertY">If true, invery the Y when the file is loaded into the document. Else, false.</param>
        /// <returns>True, if the document is successfully loaded. Else, false.</returns>
        public static bool Load(string filename, Document doc, bool invertY = true)
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
            return ConvertFromXML(xmlDoc, doc, invertY);
        }

        /// <summary>
        /// Load the contents of an SVG document into a Berny document.
        /// </summary>
        /// <param name="xmlDoc">The XML Document to load SVG content from.</param>
        /// <param name="doc">The Berny document to load content into.</param>
        /// <param name="invertY">If true, invery the Y when geometry content is loaded.</param>
        /// <returns>True, if the document is successfully loaded. Else, false.</returns>
        public static bool ConvertFromXML(System.Xml.XmlDocument xmlDoc, Document doc, bool invertY)
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
                    CreateShapeFromXML(ele, doc, null, invertY);
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
                    CreateShapeFromXML(eleLE, doc, layer, invertY);
            }

            return true;
        }

        /// <summary>
        /// Get the default layer that content can be loaded into. If no layers exists,
        /// a layer is created.
        /// </summary>
        /// <param name="doc">The document to load content into.</param>
        /// <param name="layer">If the layer is valid, relay it back for use.</param>
        /// <returns>The layer that content should be placed into.</returns>
        public static Layer ResolveUsableLayer(Document doc, Layer layer)
        { 
            if(layer == null)
                return doc.GetFirstLayer();

            return layer;
        }

        /// <summary>
        /// Create a Berny shape from an SVG path.
        /// </summary>
        /// <param name="ele">The SVG element with instructions on how to create the shape.</param>
        /// <param name="doc">The Berny document to create the shape into.</param>
        /// <param name="layer">The layer to create the shape into.</param>
        /// <param name="invertY">If true, invert the Y of loaded geometry content.</param>
        /// <returns>The created shape.</returns>
        public static BShape CreateShapeFromXML(System.Xml.XmlElement ele, Document doc, Layer layer, bool invertY)
        {
            BShape bs = null;
            if (ele.Name == "path")
            {
                SVGMat mat;
                bs = CreateTemplateShapeFromXML(ResolveUsableLayer(doc,layer), ele, out mat, invertY);

                System.Xml.XmlAttribute attrPathDraw = ele.Attributes["d"];
                if (attrPathDraw != null)
                    ProcessPathDrawAttrib(bs, attrPathDraw.Value, mat, invertY);
            }
            else if (ele.Name == "rect")
            {
                SVGMat mat;
                bs = CreateTemplateShapeFromXML(ResolveUsableLayer(doc, layer), ele, out mat, invertY);

                BShapeGenRect gen = new BShapeGenRect(bs, Vector2.zero, Vector2.one);
                gen.LoadFromSVGXML(ele, invertY);
                bs.shapeGenerator = gen;

            }
            else if (ele.Name == "ellipse")
            {
                SVGMat mat;
                bs = CreateTemplateShapeFromXML(ResolveUsableLayer(doc, layer), ele, out mat, invertY);

                BShapeGenEllipse gen = new BShapeGenEllipse(bs, Vector2.zero, Vector2.one);
                gen.LoadFromSVGXML(ele, invertY);
                bs.shapeGenerator = gen;
            }
            else if (ele.Name == "circle")
            {
                SVGMat mat;
                bs = CreateTemplateShapeFromXML(ResolveUsableLayer(doc, layer), ele, out mat, invertY);

                BShapeGenCircle gen = new BShapeGenCircle(bs, Vector2.zero, 1.0f);
                gen.LoadFromSVGXML(ele, invertY);
                bs.shapeGenerator = gen;
            }
            else if (ele.Name == "polyline")
            {
                SVGMat mat;
                bs = CreateTemplateShapeFromXML(ResolveUsableLayer(doc, layer), ele, out mat, invertY);

                BShapeGenPolyline gen = new BShapeGenPolyline(bs);
                gen.LoadFromSVGXML(ele, invertY);
                bs.shapeGenerator = gen;
            }
            else if (ele.Name == "polygon")
            {
                SVGMat mat;
                bs = CreateTemplateShapeFromXML(ResolveUsableLayer(doc, layer), ele, out mat, invertY);

                BShapeGenPolygon gen = new BShapeGenPolygon(bs);
                gen.LoadFromSVGXML(ele, invertY);
                bs.shapeGenerator = gen;
            }
            else if (ele.Name == "line")
            {
                SVGMat mat;
                bs = CreateTemplateShapeFromXML(ResolveUsableLayer(doc, layer), ele, out mat, invertY);

                BShapeGenLine gen = new BShapeGenLine(bs, Vector2.zero, Vector2.zero);
                gen.LoadFromSVGXML(ele, invertY);
                bs.shapeGenerator = gen;
            }

            if (bs != null)
                bs.FlagDirty();

            return bs;
        }

        /// <summary>
        /// Load SVG shapes from an XML element..
        /// </summary>
        /// <param name="layer">The parent layer to create the shape into.</param>
        /// <param name="ele">The element to load information from.</param>
        /// <param name="mat">The matrix.</param>
        /// <param name="invertY">If true, invert the Y of loaded geometry content.</param>
        /// <returns>The created shape.</returns>
        public static BShape CreateTemplateShapeFromXML(Layer layer, System.Xml.XmlElement ele, out SVGMat mat, bool invertY)
        {
            BShape bs = new BShape(Vector2.zero, 0.0f);
            layer.shapes.Add(bs);
            bs.layer = layer;

            LoadShapeInfo(bs, ele, out mat, invertY);
            return bs;
        }

        /// <summary>
        /// Split a path's draw command into its individual tokens.
        /// </summary>
        /// <param name="drawCmd">The command the parse.</param>
        /// <returns>A list of individual tokens from the draw command.</returns>
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

        /// <summary>
        /// Given a point string, parse it into a series of usables point.
        /// </summary>
        /// <param name="pointsString">The string with concatenated points.</param>
        /// <param name="invertY">If true, the Y coordinates are inverted after parsing. Else, the
        /// Y coordinates are left unmodified.</param>
        /// <returns>The list of parsed float vectors.</returns>
        public static List<Vector2> SplitPointsString(string pointsString, bool invertY)
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

                if(invertY == true)
                    newV.y = -newV.y;

                ret.Add(newV);

                idx = end;
            }

            return ret;
        }

        /// <summary>
        /// Given a collection of Vector2s, get the SVG string representation of that collection.
        /// </summary>
        /// <param name="ieV2">The Vector2s to convert to a string.</param>
        /// <param name="invertY">If true, the vertices are converted with an inverted Y.</param>
        /// <returns>The string representing the parameter vectors.</returns>
        public static string PointsToPointsString(IEnumerable<Vector2> ieV2, bool invertY)
        { 
            List<string> strs = new List<string>();
            foreach(Vector2 v2 in ieV2)
                strs.Add(v2.x.ToString() + "," + InvertBranch(v2.y, invertY).ToString());

            return string.Join(" ", strs);
        }

        /// <summary>
        /// Load basic information of a shape from XML. This function is only concerned with
        /// the properties that all SVG geometries have in common - and nothing type-specific.
        /// </summary>
        /// <param name="shape">The shape the load content into.</param>
        /// <param name="ele">The XML element with the SVG shape's information.</param>
        /// <param name="matrix">Output parameter of the matrix that should be used for the shape, parsed from the XML parameter.</param>
        /// <param name="invertY">If true, invert geometry Y values.</param>
        public static void LoadShapeInfo(BShape shape, System.Xml.XmlElement ele, out SVGMat matrix, bool invertY)
        {
            System.Xml.XmlAttribute attrPathID = ele.Attributes["id"];
            if (attrPathID != null)
                ProcessShapeIdAttrib(shape, attrPathID.Value);

            System.Xml.XmlAttribute attrPathStyle = ele.Attributes["style"];
            if (attrPathStyle != null)
                ProcessShapeStyleAttrib(shape, attrPathStyle.Value);


            System.Xml.XmlAttribute attrTrans = ele.Attributes["transform"];
            matrix = (attrTrans != null) ? ProcessMatrixAttribute(attrTrans.Value, invertY) : SVGMat.Identity();
        }

        /// <summary>
        /// Process a SVG path draw command and create the path geometry that was parsed into
        /// a Berny shape.
        /// </summary>
        /// <param name="shape">The shape to create geometry into.</param>
        /// <param name="attrib">The path string.</param>
        /// <param name="mat">The shape's matrix.</param>
        /// <param name="invertY">If true, inverts the Y coordinate values parsed.</param>
        public static void ProcessPathDrawAttrib(BShape shape, string attrib, SVGMat mat, bool invertY)
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
                    if (ConsumeVector2(parts, ref i, out v, invertY) == false)
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
                    if (ConsumeVector2(parts, ref i, out v, invertY) == false)
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
                    if (ConsumeVector2(parts, ref i, out v, invertY) == false)
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
                    if (ConsumeFloat(parts, ref i, out f, false) == false)
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
                    if (ConsumeFloat(parts, ref i, out f, invertY) == false)
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
                        ConsumeVector2(parts, ref i, out tcurout, invertY) == false ||
                        ConsumeVector2(parts, ref i, out tnxtin, invertY) == false ||
                        ConsumeVector2(parts, ref i, out v, invertY) == false)
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
                        ConsumeVector2(parts, ref i, out t, invertY) == false ||
                        ConsumeVector2(parts, ref i, out v, invertY) == false)
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

        /// <summary>
        /// Process a matrix's XML attribute value and return the converted matrix.
        /// </summary>
        /// <param name="attrib">The string value of a matrix from an XML attribute.</param>
        /// <param name="invertY">If true, y components are inverted.</param>
        /// <returns>The parsed matrix.</returns>
        public static SVGMat ProcessMatrixAttribute(string attrib, bool invertY)
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

                if(invertY == true)
                    ret.t.y *= -1; // Invert the translation Y 
            }

            return ret;
        }

        /// <summary>
        /// Parse an XML attribute value for the SVG attributes of a shape, and apply
        /// them to a Berny shape.
        /// </summary>
        /// <param name="shape">The shape to apply the parsed states into.</param>
        /// <param name="attrib">The XML attribute value to parse.</param>
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

        /// <summary>
        /// Get the name of a shape from an SVG's shape name attribute.
        /// </summary>
        /// <param name="shape">The shape being named.</param>
        /// <param name="attrib">The value of the shape name attribute.</param>
        public static void ProcessShapeIdAttrib(BShape shape, string attrib)
        { 
            shape.name = attrib;
        }

        /// <summary>
        /// Given a index in a string, move the index to the next non-whitespace character.
        /// </summary>
        /// <param name="str">The string being parsed.</param>
        /// <param name="idx">The index of the string.</param>
        /// <returns>True if there is anything left in str to parse after index idx.</returns>
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

        /// <summary>
        /// Given a tokenized list from a string whos contents represents a list of vectors, 
        /// parse out a vector from a specific index in token's list.
        /// 
        /// If the operation is successful, the index is moved right after the part of the tokens 
        /// list representing the vector that was parsed.
        /// </summary>
        /// <param name="lst">The list of tokens from a string that representing a vector list.</param>
        /// <param name="idx">The index into the lst parameter being operated on.</param>
        /// <param name="vecOut">The parsed vector.</param>
        /// <param name="invertY">If true, the Y coordinate is inverted for the result in vecOut.</param>
        /// <returns>True, if a vector was successfully parsed. Else, false.</returns>
        public static bool ConsumeVector2(List<string> lst, ref int idx, out Vector2 vecOut, bool invertY)
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

            if(invertY == true)
                vecOut.y = -vecOut.y;

            idx += 3;
            return true;
        }

        /// <summary>
        /// Utility function to convert a string (gathered from accessing a specified index in a list
        /// of strings) into a float. Afterwards, the index is incremented to point to the next element.
        /// </summary>
        /// <param name="lst">The list of strings to parse from. This is expected to come from a tokenized 
        /// (i.e., Split) string that contained multiple float strings.</param>
        /// <param name="idx">The index to parse from. If the function returns true, this index will also
        /// be incremented.</param>
        /// <param name="f">The output parameter for the parsed value. Only valid if the function returns true.</param>
        /// <param name="invertY">If true, invert the parsed value returned from the f parameter.</param>
        /// <returns>True, if an element was successfully parsed. Else, false.</returns>
        public static bool ConsumeFloat(List<string> lst, ref int idx, out float f, bool invertY)
        { 
            if(idx >= lst.Count)
            { 
                f = 0.0f;
                return false;
            }

            bool ret = float.TryParse(lst[idx], out f);
            if(invertY == true)
                f = -f;

            ++idx;
            return ret;
        }

        /// <summary>
        /// Utility function used when processing the first instruction of a draw command.
        /// </summary>
        /// <param name="shape">The shape being operated on.</param>
        /// <param name="loop">The loop that's set up for inserting new geometry content into.</param>
        /// <param name="lastPos">The last position set from the last draw command.</param>
        /// <param name="mat">The SVG matrix of the shape.</param>
        /// <param name="first">The first node in the shape.</param>
        /// <param name="prev">The reference to the previous node.</param>
        /// <remarks>Some of these parameter descriptions are not too useful without seeing how the
        /// function is used in context in the function ProcessPathDrawAttrib().</remarks>
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

        /// <summary>
        /// Get an attribute from an element.
        /// </summary>
        /// <param name="ele">The XML element to pull the attribute from.</param>
        /// <param name="attr">The attribute name.</param>
        /// <param name="xmlns">The attribute namespace. If no namespace is used, this can be set to null.</param>
        /// <returns>The attribute found matching the attribute name and namespace. </returns>
        public static System.Xml.XmlAttribute GetAttributeFromXMLElement(System.Xml.XmlElement ele, string attr, string xmlns)
        {
            if(string.IsNullOrEmpty(xmlns) == true)
                return ele.GetAttributeNode(attr);

            System.Xml.XmlAttribute xmlAttr = ele.GetAttributeNode(attr, xmlns);
            if(xmlAttr != null)
                return xmlAttr;

            return ele.GetAttributeNode($"{ xmlns}:{ attr}");
        }

        /// <summary>
        /// Given an attribute, convert it to a float.
        /// </summary>
        /// <param name="attr">The attribute to pull the number data from.</param>
        /// <param name="f">The </param>
        /// <param name="invert">If true, invert the value.</param>
        /// <returns></returns>
        public static bool AttribToFloat(System.Xml.XmlAttribute attr, ref float f, bool invert = false)
        { 
            if(attr == null || string.IsNullOrEmpty(attr.Value) == true)
                return false;

            float pf;
            if(float.TryParse(attr.Value, out pf) == true)
            { 
                if(invert == true)
                    f = -pf;
                else
                    f = pf;

                return true;
            }
            return false;
        }

        /// <summary>
        /// Invert a float based on a bool parameter.
        /// 
        /// When loading and saving SVGs, the SVGSerializer systems allow inverting the Y to convert between
        /// coordinate systems
        /// </summary>
        /// <param name="val">The float to invert.</param>
        /// <param name="invert">If true, the val parameter is inverted. Else, it's left alone.</param>
        /// <returns></returns>
        public static float InvertBranch(float val, bool invert)
        { 
            return (invert == true) ? -val : val;
        }

        /// <summary>
        /// Enumerate through child elements of a parent element.
        /// 
        /// This is done because an element's children from iteration
        /// can be more than just element - and we want to ignore the 
        /// non-element children.
        /// </summary>
        /// <param name="ele">The XML element whos children are being iterated.</param>
        /// <returns>The iterator through the parameter's children XML elements.</returns>
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