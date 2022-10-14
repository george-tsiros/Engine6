namespace Engine6;
using Common;
using System.Numerics;

public abstract class ExampleBase:GlWindow {
    /*
       7-----6
      /     /
     /     / |
    3-----2  |
    |  4  |  |5
    |     | /
    |     |/
    0-----1

        new(-1, -1, +1, 1), // 0
        new(+1, -1, +1, 1), // 1
        new(+1, +1, +1, 1), // 2
        new(-1, +1, +1, 1), // 3
        new(-1, -1, -1, 1), // 4
        new(+1, -1, -1, 1), // 5
        new(+1, +1, -1, 1), // 6
        new(-1, +1, -1, 1), // 7

*/
    protected static readonly Vector4[] ThreeFaces = {
        new(+1, -1, +1, 1), // 1
        new(+1, -1, -1, 1), // 5
        new(+1, +1, -1, 1), // 6
        new(+1, -1, +1, 1), // 1
        new(+1, +1, -1, 1), // 6
        new(+1, +1, +1, 1), // 2

        new(-1, +1, +1, 1), // 3
        new(+1, +1, +1, 1), // 2
        new(+1, +1, -1, 1), // 6
        new(-1, +1, +1, 1), // 3
        new(+1, +1, -1, 1), // 6
        new(-1, +1, -1, 1), // 7

        new(-1, -1, +1, 1), // 0
        new(+1, -1, +1, 1), // 1
        new(+1, +1, +1, 1), // 2
        new(-1, -1, +1, 1), // 0
        new(+1, +1, +1, 1), // 2
        new(-1, +1, +1, 1), // 3

    };

    protected static readonly Vector3[] ThreeFacesNormals = {
        Vector3.UnitX, Vector3.UnitX, Vector3.UnitX, Vector3.UnitX, Vector3.UnitX, Vector3.UnitX,
        Vector3.UnitY, Vector3.UnitY, Vector3.UnitY, Vector3.UnitY, Vector3.UnitY, Vector3.UnitY,
        Vector3.UnitZ, Vector3.UnitZ, Vector3.UnitZ, Vector3.UnitZ, Vector3.UnitZ, Vector3.UnitZ,
    };

    protected static readonly Vector4[] Colors = {
        new(0.19f, 0.02f, 0.68f, 1), new(0.25f, 0.02f, 0.99f, 1), new(0.44f, 0f, 0.98f, 1),
        new(0.62f, 0f, 0.99f, 1), new(0.76f, 0.87f, 0.98f, 1), new(0.8f, 0f, 0.93f, 1),
        new(1f, 0f, 0.29f, 1), new(0.59f, 0.62f, 0.64f, 1), new(1f, 0.74f, 0.82f, 1),
    };

}

