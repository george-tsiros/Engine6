#version 460 core

in vec4 vertexPosition;

void main() {
    gl_Position = vec4(vertexPosition.xy, 0.0, 1.0);
}
