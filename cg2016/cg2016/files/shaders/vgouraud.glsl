// VERTEX SHADER. Simple

#version 330

//Vertex Attrib
in vec3 vPos;
in vec3 norm;

//Light Attrib
uniform vec4 posL;

//Material Attrib
uniform vec3 ka;
uniform vec3 kd;
uniform vec3 ks;
uniform float CoefEsp;

//Matrices
uniform mat4 MV;
uniform mat4 MVP;
uniform mat4 MN;

//Output light
out vec3 colorV;

void main(){
	//Transformar Normal de EspacioObjeto a EspacioOjo
	vec3 tnorm = normalize( mat3(MN) * norm );

	//Transformar Posicion de EspacioObjeto a EspacioOjo
	vec4 eyeCoords = MV * vec4(vPos, 1.0);

	//Calcular luz en el EspacioOjo
	vec3 s = normalize(vec3(posL - eyeCoords));
	vec3 v = normalize(-eyeCoords.xyz);
	vec3 r = reflect(-s, tnorm);
	float aDotN = max(dot(s, tnorm), 0.0);
	float spec = 0.0;
	if(aDotN > 0.0)
		spec = pow(max(dot(r,v), 0.0), CoefEsp);

	colorV = ka + kd*aDotN + ks*spec;
	gl_Position = MVP * vec4(vPos, 1.0);
}