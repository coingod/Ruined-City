﻿// VERTEX SHADER. Simple

#version 330
uniform mat4 projMatrix;
uniform mat4 viewMatrix;
uniform mat4 modelMatrix;

in vec2 TexCoord;
in vec3 vPos;
in vec3 vNormal;

in vec3 vTangente;
//in vec3 vBitangente;

out vec2 f_TexCoord;
out vec3 fragPos;
out vec3 fragNormal;

// --- SHADOW MAPPING ---
// Matriz que convierte al espacio de la luz y lo mapea de [-1,1] a [0,1] para acceder a la textura
uniform mat4 uLightBiasMatrix;

// Posicion del fragmento en el espacio de la luz.
out vec4 fragPosLightSpace;

void main(){
	
	//Tengo que usar tang y bitang para que no las descarte y falle al compilar
	vec3 alpedo = (vTangente) * 0.0001;

	fragPos = vPos + alpedo*0.0001;
	fragNormal = vNormal;
	gl_Position = projMatrix * viewMatrix * modelMatrix * vec4(vPos, 1.0);
	f_TexCoord = vec2(TexCoord.s, 1 - TexCoord.t);
	fragPosLightSpace = uLightBiasMatrix * modelMatrix * vec4(vPos, 1.0);
}
