uniform vec4 lightDirection;
in vec4 n;
in vec4 c;
out vec4 color0;
void main() {
    vec4 nn = normalize(n);
    vec4 nl = normalize(lightDirection);
    float d = max(dot(nn,-nl), 0.0);
    color0 = vec4(c.rgb * (0.9 * d + 0.1), 1.0);
}
