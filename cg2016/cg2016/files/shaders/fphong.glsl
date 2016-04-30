#version 330

in vec3 tnorm;
in vec3 eyeCoords;

//Light Attrib
uniform vec4 posL;

//Material Attrib
uniform vec3 ka;
uniform vec3 kd;
uniform vec3 ks;
uniform float CoefEsp;

out vec4 FragColor;

vec3 ads()
{
	//Calcular luz en el EspacioOjo
	vec3 n = normalize(tnorm);
	vec3 s = normalize(vec3(posL) - eyeCoords);
	vec3 v = normalize(-eyeCoords.xyz);
	vec3 r = reflect(-s, n);
	float diffuse = max(dot(s, tnorm), 0.0);
	float spec = 0.0;
	if(diffuse > 0.0)
		spec = pow(max(dot(r,v), 0.0), CoefEsp);

	float d = distance(vec3(posL), eyeCoords);
	float fatt = 0.5/(0.3 + 0.007*d + 0.00008*d*d);

	return ka + (kd*diffuse + ks*spec)*fatt;
}

void main()
{
	FragColor = vec4(ads(), 1.0);
}