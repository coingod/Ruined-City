// VERTEX SHADER. Simple

#version 330

//Vertex Attrib
in vec3 vPos;
in vec3 vNormal;
in vec2 TexCoord;
in vec3 vTangente;
in vec3 vBitangente;

//Light Attrib
uniform vec4 posL;

//Matrices
uniform mat4 projMatrix;
uniform mat4 modelMatrix;
uniform mat4 viewMatrix;

//Output
out vec3 fragPos;
out vec3 fragLightPos;
out vec3 fragNormal;
out vec2 f_TexCoord;

void main(){

	//Cachear la ModelView en 3x3
	mat3 modelViewMatrix = mat3(viewMatrix * modelMatrix);

	//Transformar vectores N, T y B de ObjectSpace a CameraSpace
	vec3 vNormalCE = modelViewMatrix * vNormal;
	vec3 vTangenteCE = modelViewMatrix * vTangente;
	vec3 vBitangenteCE = modelViewMatrix * vBitangente;

	//Construir la Matris de transformacion de CameraSpace a TangentSpace
	mat3 TBN = transpose( mat3(vTangenteCE, vBitangenteCE, vNormalCE) );

	//Transformar Posicion de ObjectSpace a TangentSpace
	fragPos = TBN * modelViewMatrix * vPos;

	//Transformar Posicion de la Luz de WorldSpace a TangentSpace
	fragLightPos = TBN * mat3(viewMatrix) * vec3(posL);

	//Paso vectores para interpolar
	fragNormal = vNormal;

	//Arreglo la coordenada "y" del NormalMap
	f_TexCoord = vec2(TexCoord.s, 1 - TexCoord.t);

	gl_Position = projMatrix * viewMatrix * modelMatrix * vec4(vPos, 1.0);
}