in vec4 vertexPosition;
in vec4 faceNormal;

uniform vec4 lightDirection;
uniform mat4 model, view, projection;

out vec4 color;

void main () {
    float lightIntensity = dot(normalize(model * faceNormal).xyz, -lightDirection.xyz);
    color = vec4(lightIntensity, lightIntensity, lightIntensity, 1);
    gl_Position = projection * view * model * vertexPosition; 
}
