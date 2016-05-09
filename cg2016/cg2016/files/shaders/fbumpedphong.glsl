#version 330

in vec3 fragPos;
in vec3 fragNormal;
in vec2 f_TexCoord;

//Light Attrib
in vec3 fragLightPos;

//Material Attrib
uniform vec3 ka;
uniform vec3 kd;
uniform vec3 ks;
uniform float CoefEsp;
uniform sampler2D gSampler;

out vec4 FragColor;

vec3 ads(vec3 newNormal)
{
	//Calcular luz en el Espacio Tangente
	vec3 n = normalize(fragNormal);
	vec3 s = normalize(fragLightPos - fragPos);
	vec3 v = normalize(-fragPos.xyz);
	vec3 r = reflect(-s, n);
	float diffuse = max(dot(s, fragNormal), 0.0);
	float spec = 0.0;
	if(diffuse > 0.0)
		spec = pow(max(dot(r,v), 0.0), CoefEsp);

	float d = distance(fragLightPos, fragPos);
	float fatt = 0.5/(0.3 + 0.007*d + 0.00008*d*d);

	return ka + (kd*diffuse + ks*spec)*fatt;
}

void main()
{
	//Obtengo la nueva normal del NormalMap
	vec3 newNormal = normalize( texture(gSampler, f_TexCoord.st).rgb*2 - 1 ); 

	//Calculo la iluminacion con el metodo de Phong y la nueva normal
	FragColor = vec4(ads(newNormal), 1.0);
}