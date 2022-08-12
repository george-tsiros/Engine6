uniform int fontWidth;
uniform ivec2 offset;
in ivec2 vertexPosition;
out vec2 uv;
void main () {
    uv = vec2(1);
    gl_Position = vec4(vertexPosition.xy,0.0, 1.0); 
}
