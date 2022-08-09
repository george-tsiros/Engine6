uniform ivec2 renderSize;
uniform ivec2 offset;
in ivec2 vertexPosition;

void main () {
    vec2 a = 2 / (renderSize - vec2(1.0,1.0));
    gl_Position = vec4((vertexPosition + offset) * a - vec2(1.0, 1.0), 0.0, 1.0); 
}
