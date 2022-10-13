in vec2 uv;
flat in vec4 n;
//uniform sampler2D tex;
uniform vec4 lightDirection;

out vec4 color0;

void main() {
    float intensity = max(dot(-lightDirection.xyz, n.xyz), 0.0);
    color0 = vec4(intensity * vec3(1), 1);
}
