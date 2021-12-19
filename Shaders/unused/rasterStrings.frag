#version 460 core
#pragma debug(on)

in vec2 uv;

uniform sampler2D tex;

out vec4 out0;

void main () { 
    out0 = vec4(texture(tex, uv).rgb, 1.0);
}
