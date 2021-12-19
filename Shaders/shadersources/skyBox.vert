#pragma debug(on)

in vec4 vertexPosition;
in vec2 vertexUV;

uniform mat4 view, projection;

out vec2 uv;

void main () {
    uv = vertexUV;
    vec4 p = projection * view * vertexPosition;
    gl_Position = p.xyww; 
}
