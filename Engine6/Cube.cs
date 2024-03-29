namespace Engine6;

using System;
using System.Collections.Generic;
using System.Numerics;

static class Cube {
    /*
       4-----5
      /     /
     /     / |
    7-----6  |
    |  0  |  |1
    |     | /
    |     |/
    3-----2
    -------------------------------------------
    |0        |1 top    |2        |3        |4
    --------------------------------------------
    |5 left   |6 near   |7 right  |8  far   |9
    --------------------------------------------
    |10       |11 bottom|12       |13       |14
    --------------------------------------------
    |15       |16       |17       |18       |19
    --------------------------------------------

    */
    internal static readonly IReadOnlyList<Vector4> Vertices = new Vector4[] { new(0, 0, 0, 1), new(1, 0, 0, 1), new(1, 0, 1, 1), new(0, 0, 1, 1), new(0, 1, 0, 1), new(1, 1, 0, 1), new(1, 1, 1, 1), new(0, 1, 1, 1), };
    internal static readonly IReadOnlyList<Vector2> UvVectors = new Vector2[] {
        new(0.00f, 0.00f),
        new(0.25f, 0.00f),
        new(0.50f, 0.00f),
        new(0.75f, 0.00f),
        new(1.00f, 0.00f),

        new(0.00f, 0.25f),
        new(0.25f, 0.25f),
        new(0.50f, 0.25f),
        new(0.75f, 0.25f),
        new(1.00f, 0.25f),

        new(0.00f, 0.50f),
        new(0.25f, 0.50f),
        new(0.50f, 0.50f),
        new(0.75f, 0.50f),
        new(1.00f, 0.50f),

        new(0.00f, 0.75f),
        new(0.25f, 0.75f),
        new(0.50f, 0.75f),
        new(0.75f, 0.75f),
        new(1.00f, 0.75f),

        new(0.00f, 1.00f),
        new(0.25f, 1.00f),
        new(0.50f, 1.00f),
        new(0.75f, 1.00f),
        new(1.00f, 1.00f),

    };
    internal static readonly IReadOnlyList<int> Indices = new int[] {
        1, 5, 6, 6, 2, 1, // right
        0, 3, 7, 7, 4, 0, // left
        4, 7, 6, 6, 5, 4, // top
        0, 1, 2, 2, 3, 0, // bottom
        2, 6, 7, 7, 3, 2, // near
        0, 4, 5, 5, 1, 0, // far
    };
    internal static readonly IReadOnlyList<int> UvIndices = new int[] {
        13, 8, 7, 7 , 12, 13,
        10, 11, 6, 6, 5, 10,
        6, 7, 2, 2, 1, 6,
        16, 17, 12, 12, 11, 16,
        12, 7, 6, 6, 11, 12,
        14, 9, 8, 8, 13, 14,
    };
    internal static readonly IReadOnlyList<Vector3> Normals = new Vector3[] { Vector3.UnitX, -Vector3.UnitX, Vector3.UnitY, -Vector3.UnitY, Vector3.UnitZ, -Vector3.UnitZ, };
    internal static readonly IReadOnlyList<int> NormalIndices = new int[] { 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 2, 2, 2, 2, 2, 2, 3, 3, 3, 3, 3, 3, 4, 4, 4, 4, 4, 4, 5, 5, 5, 5, 5, 5, };

}
