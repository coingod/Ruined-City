// VERTEX SHADER. Simple

#version 330
uniform mat4 projMatrix;
uniform mat4 viewMatrix;
uniform mat4 modelMatrix;

in vec2 TexCoord;
in vec3 vPos;
in vec3 vNormal;

out vec2 f_TexCoord;
out vec3 fragPos;
out vec3 fragNormal;

void main(){
 fragPos = vPos;
 fragNormal = vNormal;
 gl_Position = projMatrix * viewMatrix * modelMatrix * vec4(vPos, 1.0);
 f_TexCoord = TexCoord;
}
