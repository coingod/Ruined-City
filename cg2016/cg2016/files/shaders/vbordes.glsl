// VERTEX SHADER. Simple

#version 330

//Vertex Attrib
in vec3 vPos;
in vec3 norm;

//Material Attrib
//uniform float CoefEsp;

//Matrices
uniform mat4 MV;
uniform mat4 MVP;
uniform mat4 MN;

//Output light
out vec3 fNormal;
out vec3 fPosition;

void main()
{
	//Normales de EObj a EOjo
	fNormal = normalize(mat3(MN) * norm);
	
	vec4 pos = MV * vec4(vPos, 1.0);
	fPosition = pos.xyz;
  
	gl_Position = MVP * vec4(vPos, 1.0);
}