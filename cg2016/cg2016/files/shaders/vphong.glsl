// VERTEX SHADER. Simple

#version 330

//Vertex Attrib
in vec3 vPos;
in vec3 norm;

//Matrices
uniform mat4 Proy;
uniform mat4 MV;
uniform mat4 MN;

//Output
out vec3 tnorm;
out vec3 eyeCoords;

void main(){
	//Transformar Normal de EspacioObjeto a EspacioOjo
	tnorm = normalize( mat3(MN) * norm );

	//Transformar Posicion de EspacioObjeto a EspacioOjo
	eyeCoords = vec3(MV * vec4(vPos, 1.0));

	gl_Position = Proy * MV * vec4(vPos, 1.0);
}