#version 460 core
#pragma debug(on)

in vec2 char, charLocation, quadCoordinates;

uniform vec2 screenSize, fontSize;

out vec2 uv;

void main () {
    uv = (char + vec2(quadCoordinates.x, 1 - quadCoordinates.y)) / 16.0;
    
    vec2 po = charLocation + (quadCoordinates) * fontSize;
    gl_Position = vec4(2 * po / screenSize - vec2(1), 0, 1); 
}