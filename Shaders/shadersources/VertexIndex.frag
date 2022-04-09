#pragma debug(on)

uniform vec4 color;

out vec4 out0;
flat in int vertexId;
out int out1;
void main () { 
    out0 = color;
    out1 = vertexId;
}
