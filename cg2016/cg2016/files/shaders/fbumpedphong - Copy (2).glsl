// FRAGMENT SHADER. Simple

#version 330
#define maxLights 5

//in vec3 LightDir;
in vec2 f_TexCoord;
//in vec3 ViewDir;
in mat3 TBN;
in vec3 pos;

struct Light {
	vec4 position; //Light position in World Space
	vec3 Ia;
	vec3 Id;
	vec3 Is;
	/*
	float coneAngle;
	vec3 coneDirection;
	*/
	int enabled;
};

struct Material {
	vec3 Ka;
	vec3 Kd;
	vec3 Ks;
	float Shininess;
};

uniform sampler2D ColorTex;
uniform sampler2D NormalMapTex;
uniform mat4 viewMatrix;
uniform int numLights;
uniform Light allLights[maxLights];
uniform Material material;

uniform float A;
uniform float B;
uniform float C;

out vec4 FragColor;

vec3 phongModel( vec3 norm, vec3 diffR, Light light, vec3 ViewDir) 
{
	//Transformar Posicion de la Luz de CameraSpace a TangentSpace
	vec3 LightDir = normalize( TBN * ( (viewMatrix*light.position).xyz - pos) );

	vec3 r = reflect( -LightDir, norm );
	vec3 ambient = material.Ka * light.Ia;
	float sDotN = max( dot(LightDir, norm), 0.0 );
	vec3 diffuse = diffR * sDotN * light.Id;// * material.Kd;
	vec3 spec = vec3(0.0);
	if( sDotN > 0.0 )
		spec = material.Ks * pow( max( dot(r,ViewDir), 0.0 ), material.Shininess ) * light.Is;

	float attenuation = 1.0;
	float distanceToLight = length(light.position.xyz);
	attenuation = 0.5 / ( A + B * distanceToLight + C * pow(distanceToLight, 2));

	return ambient + attenuation * (diffuse + spec) * light.enabled;
}

void main() 
{
	// Lookup the normal from the normal map
	vec4 normal = 2.0*texture2D( NormalMapTex, f_TexCoord ) - 1;

	// The color texture is used as the diff. reflectivity
	vec4 texColor = texture2D( ColorTex, f_TexCoord );

	//Transformar Posicion de CameraSpace a TangentSpace
	vec3 ViewDir = TBN * normalize(-pos);

	//Acumular iluminacion de cada fuente de luz
	vec3 linearColor=vec3(0);
	for(int i=0; i<numLights; i++)
		linearColor += phongModel(normal.xyz, texColor.rgb, allLights[i], ViewDir);

	FragColor = vec4( linearColor, 1.0 );
}