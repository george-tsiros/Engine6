namespace Gl;

public enum Primitive {
    ///<summary></summary>
    Points = Const.POINTS,
    ///<summary>The adjacent vertices are considered lines. Thus, if you pass n vertices, you will get n-1 lines. If the user only specifies 1 vertex, the drawing command is ignored.</summary>
    LineStrip = Const.LINE_STRIP,
    ///<summary>As line strips, except that the first and last vertices are also used as a line. Thus, you get n lines for n input vertices. If the user only specifies 1 vertex, the drawing command is ignored. The line between the first and last vertices happens after all of the previous lines in the sequence.</summary>
    LineLoop = Const.LINE_LOOP,
    ///<summary>Vertices 0 and 1 are considered a line. Vertices 2 and 3 are considered a line. And so on. If the user specifies a non-even number of vertices, then the extra vertex is ignored.</summary>
    Lines = Const.LINES,
    ///<summary></summary>
    LineStripAdjacency = Const.LINE_STRIP_ADJACENCY,
    ///<summary></summary>
    LinesAdjacency = Const.LINES_ADJACENCY,
    ///<summary></summary>
    TriangleStrip = Const.TRIANGLE_STRIP,
    ///<summary></summary>
    TriangleFan = Const.TRIANGLE_FAN,
    ///<summary></summary>
    Triangles = Const.TRIANGLES,
    ///<summary></summary>
    TrianglesAdjacency = Const.TRIANGLES_ADJACENCY,
    ///<summary></summary>
    TriangleStripAdjacency = Const.TRIANGLE_STRIP_ADJACENCY,
}
