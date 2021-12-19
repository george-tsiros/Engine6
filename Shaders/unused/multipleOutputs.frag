#version 460 core
#pragma debug(on)

in vec2 uv;
flat in int inst;
uniform sampler2D tex[4];

out vec4 fragData[4];
out int fragData0;
void main () { 
    for (int i = 0; i < 4; ++i)
        fragData[i] = texture(tex[i], uv);
    fragData0 = inst;
}
