uniform sampler2D tex;
in vec2 uv;
out vec4 out0;

void main () { 
    out0 = texture(tex, uv);
}
