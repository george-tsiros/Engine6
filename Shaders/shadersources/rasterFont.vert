#version 460 core
#pragma debug(on)
in vec2 char, offset, vertex;
uniform vec2 screenSize, fontSize;
out vec2 uv;
void main () {
    uv = (char + vec2(vertex.x, 1 - vertex.y)) / 16.0;
    vec2 po =vec2(offset + (vec2(gl_InstanceID, 0) + vertex) * fontSize);
    gl_Position = vec4(2 * po / vec2(screenSize) - vec2(1),0,1); 
}