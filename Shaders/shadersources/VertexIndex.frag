uniform vec4 color0, color1;
uniform int tri;

out vec4 out0;
flat in int vertexId;
out int out1;
void main () { 
    out0 =  (tri == vertexId / 3) ? color1 : color0;
    out1 = vertexId;
}
