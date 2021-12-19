#version 460 core
#pragma debug(on)

out vec4 fragData;
in vec2 uv;
uniform float theta;
void main () { 
    const float pi = 3.1415926535;
    float r = sin(theta + uv.x * pi );
    float g = sin(theta + uv.y * pi );
    float b = sin(theta + (uv.x + uv.y) * pi);
    fragData = vec4( 0.5 * vec3( 1.0 + r, 1.0 + g, 1.0 + b), 1.0);
}
