// VERTEX SHADER. Simple

#version 330

//uniform vec4 figureColor;
uniform mat4 projMatrix;
uniform mat4 modelMatrix;
uniform mat4 viewMatrix;

uniform float speed;

in vec2 TexCoord;
in vec3 vPos;
in vec3 vNormal;
in vec3 vTangente;

out vec2 f_TexCoord;
out vec3 fNormal;

void main(){
	vec3 alpedo = (vTangente + vNormal) * 0.00001;

	f_TexCoord = vec2(TexCoord.s, 1 - TexCoord.t + speed);
	gl_Position = projMatrix * viewMatrix * modelMatrix * vec4(vPos + alpedo*0.0001, 1.0);
}
