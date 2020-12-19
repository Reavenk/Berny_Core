using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct SVGMat
{
    public Vector2 x;
    public Vector2 y;
    public Vector2 t;

    public static SVGMat Identity()
    {
        SVGMat ret = new SVGMat();

        ret.x.x = 1.0f;
        ret.x.y = 0.0f;

        ret.y.x = 0.0f;
        ret.y.y = 1.0f;

        ret.t.x = 0.0f;
        ret.t.y = 0.0f;

        return ret;
    }

    public Vector2 Mul(Vector2 v)
    { 
        return 
            v.x * this.x + 
            v.y * this.y + 
            this.t;
    }
}
