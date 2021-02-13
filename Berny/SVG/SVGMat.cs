using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A 2D matrix transform for element in an SVG.
/// </summary>
public struct SVGMat
{
    /// <summary>
    /// The X axis.
    /// </summary>
    public Vector2 x;

    /// <summary>
    /// The Y axis.
    /// </summary>
    public Vector2 y;

    /// <summary>
    /// The translation.
    /// </summary>
    public Vector2 t;

    /// <summary>
    /// Create an identity matrix.
    /// </summary>
    /// <returns>An identity matrix.</returns>
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

    /// <summary>
    /// Multiple this matrix by another one.
    /// </summary>
    /// <param name="v">The matrix to multiply by.</param>
    /// <returns>The product of the multiplied matrices.</returns>
    public Vector2 Mul(Vector2 v)
    { 
        return 
            v.x * this.x + 
            v.y * this.y + 
            this.t;
    }
}
