in vec2 not_FragPos;

uniform sampler2D tex;

out vec4 out0;

void main () { 
    out0 = texture(tex, not_FragPos);
}
