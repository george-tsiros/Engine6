#version 460 core
#pragma debug(on)
in vec2 uv;
uniform sampler2D font;
out vec4 out0;
void main () { 
    out0 = vec4(texture(font, uv).rgb, 1.0);
}