uniform vec4 color;
uniform vec4 lightDirection;
in vec4 n;
out vec4 color0;
void main() {
    vec4 nn = normalize(n);
    vec4 nl = normalize(lightDirection);

    color0 = vec4(max(0.0, dot(nn, -nl)) * color.rgb, 1.0);
}
