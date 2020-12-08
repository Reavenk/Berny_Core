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
        public class Layer
        {
            public Document document;
            public List<BShape> shapes = new List<BShape>();

            public string name = "";

            bool locked = false;
            bool visible = true;

            public bool Locked 
            {
                get => this.locked;
                set { this.locked = value; }
            }

            public bool Visible
            { 
                get => this.visible;
                set 
                { 
                    if(this.visible == value)
                        return;

                    this.visible = value;
                    this.FlagDirty();
                }
            }


            bool dirty = false;

            public Layer(Document doc)
            { 
                this.document = doc;
            }

            public void FlagDirty()
            {
                this.dirty = true;

                if(this.document != null)
                    this.document.FlagDirty();
            }

            public bool IsDirty()
            { 
                return this.dirty;
            }

            public void FlushDirty()
            {
                foreach (BShape bs in this.shapes)
                {
                    foreach (BLoop bl in bs.loops)
                        bl.FlushDirty();
                }

                this.dirty = false;
            }

            public IEnumerable<BShape> Shapes()
            { 
                return this.shapes;
            }

            public void TestValidity()
            {
                foreach (BShape bs in this.shapes)
                {
                    if (bs.layer != this)
                        Debug.LogError("Validity Error: Document layer referencing incorrect parent document.");

                    bs.TestValidity();
                }

                Debug.Log("Finished validity check.");
            }

            public BShape CreateEmptyShape()
            { 
                BShape bs = new BShape(Vector2.zero, 0.0f);
                bs.layer = this;
                this.shapes.Add(bs);
                return bs;
            }

            public Layer Clone()
            { 
                return new Layer(this.document);
            }

            public Layer Clone(Document doc)
            {
                Layer ret = null;
                if(doc != null)
                    ret = doc.AddLayer();
                else
                    ret = new Layer(null);

                ret.locked = this.locked;
                ret.visible = this.visible;
                ret.name = this.name;

                foreach(BShape shape in this.shapes)
                    this.shapes.Add(shape.Clone(this));

                return new Layer(this.document);
            }
        }
    }
}