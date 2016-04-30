// VERTEX SHADER. Simple color en base a la normal
#version 330

//Vertex Attrib
in vec3 vPos;
in vec3 norm;

//Matrices
uniform mat4 Proy;
uniform mat4 MV;
uniform mat4 MVP;
uniform mat4 MN;

//Output color
out vec3 fNormal;

void main()
{

	//Transformar Normal de EspacioObjeto a EspacioOjo para un efecto mas "Luminico"
	fNormal = normalize(mat3(MN) * norm);

	//O simpllemente se aplican las normales verdaderas
	//fNormal = normalize(norm);

	gl_Position = Proy * MV * vec4(vPos, 1.0);
}