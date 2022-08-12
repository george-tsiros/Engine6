uniform int fontWidth;
uniform ivec2 offset;
in ivec2 vertexPosition;

void main () {

    gl_Position = vec4(vertexPosition.xy,0.0, 1.0); 
}
