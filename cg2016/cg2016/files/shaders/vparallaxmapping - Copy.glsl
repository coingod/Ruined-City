// VERTEX SHADER. TEXTURE + BUMP + SPECULAR

#version 330

in vec2 TexCoord;
in vec3 vPos;
in vec3 vNormal;
in vec3 vTangente;
in vec3 vBitangente; 

out vec2 f_TexCoord;
out vec3 fPos_CS;
out mat3 TBN;
out vec3 fnormal;

uniform mat4 projMatrix;
uniform mat4 viewMatrix;
uniform mat4 modelMatrix;
uniform mat3 normalMatrix;

void main()
{
	mat4 modelViewMatrix = viewMatrix * modelMatrix;

	//Transformar vectores N, T y B de ObjectSpace a CameraSpace
	vec3 normal = normalize(normalMatrix * vNormal);
	vec3 tangente = normalize(normalMatrix * vTangente);
	vec3 bitangente = normalize(normalMatrix * vBitangente);

	//Construir la Matris de transformacion de CameraSpace a TangentSpace
	TBN = transpose( mat3(tangente, bitangente, normal) );

	//Transformar Posicion de ObjectSpace a CameraSpace
	fPos_CS = vec3( modelViewMatrix * vec4(vPos,1.0) );

	fnormal = vNormal;

	//Invertir la coordenada "y" de textura
	f_TexCoord = vec2(TexCoord.s, 1 - TexCoord.t);

	gl_Position = projMatrix * viewMatrix * modelMatrix * vec4(vPos, 1.0);
}
