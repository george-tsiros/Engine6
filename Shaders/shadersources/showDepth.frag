uniform sampler2D depth;
uniform float far, near;
in vec2 uv;
out vec4 color0;
void main() {
    float c = (2.0 * near) / (far + near - texture2D(depth, uv).x * (far - near));
    color0 = vec4(c, c, c, 1);
}
