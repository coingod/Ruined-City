// VERTEX SHADER. Simple

#version 330
uniform mat4 projMatrix;
uniform mat4 viewMatrix;
uniform mat4 modelMatrix;

in vec2 TexCoord;
in vec3 vPos;
in vec3 vNormal;

in vec3 vTangente;
in vec3 vBitangente;

out vec2 f_TexCoord;
out vec3 fragPos;
out vec3 fragNormal;

void main(){
	
	//Tengo que usar tang y bitang para que no las descarte y falle al compilar
	vec3 alpedo = (vTangente + vBitangente) * 0.0001;

	fragPos = vPos + alpedo*0.0001;
	fragNormal = vNormal;
	gl_Position = projMatrix * viewMatrix * modelMatrix * vec4(vPos, 1.0);
	f_TexCoord = vec2(TexCoord.s, 1 - TexCoord.t);
}
