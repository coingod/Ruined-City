// VERTEX SHADER. Simple

#version 330

in vec2 TexCoord;
in vec3 vPos;
in vec3 vNormal;
in vec3 vTangente;
in vec3 vBitangente; 

out vec2 f_TexCoord;
//out vec3 LightDir;
//out vec3 ViewDir;
out vec3 fPos_CS;
out mat3 TBN;

struct Light {
	vec4 position;	//Light position in World Space
	//vec3 intensity;
};

uniform Light light;

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

	/*
	//Transformar Posicion de la Luz de CameraSpace a TangentSpace
	LightDir = normalize( TBN * ( (viewMatrix*light.position).xyz - pos) );

	//Transformar Posicion de CameraSpace a TangentSpace
	ViewDir = TBN * normalize(-pos);
	*/

	//Invertir la coordenada "y" de textura
	f_TexCoord = vec2(TexCoord.s, 1 - TexCoord.t);

	gl_Position = projMatrix * viewMatrix * modelMatrix * vec4(vPos, 1.0);
}
