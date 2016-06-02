// FRAGMENT SHADER. TEXTURE + BUMP + SPECULAR

#version 330
#define maxLights 5

in vec2 f_TexCoord;
in mat3 TBN;
in vec3 fPos_CS;
in vec3 fnormal;

struct Light {
	vec4 position; //Light position in World Space
	vec3 Ia;
	vec3 Ip;
	float coneAngle;
	vec3 coneDirection; //Cone direction in World Space
	int enabled;
	int direccional;
};

struct Material {
	vec3 Ka;
	vec3 Kd;
	vec3 Ks;
	float Shininess;
};

uniform sampler2D ColorTex;
uniform sampler2D NormalMapTex;
uniform sampler2D SpecularMapTex;
uniform mat4 viewMatrix;
uniform vec3 cameraPosition; //In World Space.
uniform int numLights;
uniform Light allLights[maxLights];
uniform Material material;

uniform float A;
uniform float B;
uniform float C;

out vec4 FragColor;


float beckmannDistribution(float x, float roughness) 
{
	float NdotH = max(x, 0.0001);
	float cos2Alpha = NdotH * NdotH;
	float tan2Alpha = (cos2Alpha - 1.0) / cos2Alpha;
	float roughness2 = roughness * roughness;
	float denom = 3.141592653589793 * roughness2 * cos2Alpha * cos2Alpha;
	return exp(tan2Alpha / roughness2) / denom;
}

float cookTorranceSpecular(vec3 lightDirection,	vec3 viewDirection,	vec3 surfaceNormal,	float roughness, float fresnel) 
{
	float VdotN = max(dot(viewDirection, surfaceNormal), 0.0);
	float LdotN = max(dot(lightDirection, surfaceNormal), 0.0);

	//Half angle vector
	vec3 H = normalize(lightDirection + viewDirection);

	//Geometric term
	float NdotH = max(dot(surfaceNormal, H), 0.0);
	float VdotH = max(dot(viewDirection, H), 0.000001);
	float LdotH = max(dot(lightDirection, H), 0.000001);
	float G1 = (2.0 * NdotH * VdotN) / VdotH;
	float G2 = (2.0 * NdotH * LdotN) / LdotH;
	float G = min(1.0, min(G1, G2));
  
	//Distribution term
	float D = beckmannDistribution(NdotH, roughness);

	//Fresnel term
	float F = pow(1.0 - VdotN, fresnel);

	//Multiply terms and done
	return  G * F * D / max(3.14159265 * VdotN, 0.000001);
}

float phongSpecular(vec3 lightDirection, vec3 viewDirection, vec3 surfaceNormal, float roughness) 
{
	float spec = 0;
	float sDotN = max( dot(lightDirection, surfaceNormal), 0.0 );
	vec3 reflectionVector = reflect( -lightDirection, surfaceNormal );
	if( sDotN > 0.0 )
		spec = pow( max( dot(reflectionVector, viewDirection), 0.0 ), roughness );
	return spec;
}

float blinnPhongSpecular(vec3 lightDirection, vec3 viewDirection, vec3 surfaceNormal, float roughness) 
{
	float spec = 0;
	float sDotN = max( dot(lightDirection, surfaceNormal), 0.0 );
	vec3 h = normalize(viewDirection + lightDirection);
	if( sDotN > 0.0 )
		spec = pow( max( dot(h, surfaceNormal), 0.0 ), roughness );
	return spec;
}

//Calculo de la iluminacion por metodo de Phong
vec3 phongModel( vec3 norm, vec3 diffR, vec3 specMap, Light light, vec3 ViewDir) 
{
	float fAtt = 1.0;
	vec3 LightPos;
	float falloff = 1.0;

	if(light.direccional==1)
	{ 
		LightPos = normalize( transpose(inverse(TBN)) * ( (transpose(inverse(viewMatrix)) * -light.position).xyz) );
	}
	else
	{
		//Transformar POSICION de la Luz de CameraSpace a TangentSpace
		LightPos = normalize( TBN * ( (viewMatrix * light.position).xyz - fPos_CS) );

		//Restricciones del cono de luz
		vec3 coneDirection = normalize(TBN * (mat3(viewMatrix) * light.coneDirection) );
		vec3 rayDirection = -LightPos;
		float lightToSurfaceAngle = degrees(acos(dot(rayDirection, coneDirection)));
		//Dentro del cono
		if (lightToSurfaceAngle <= light.coneAngle) 
		{ 
			//Atenuacion a la distancia
			float distanceToLight = length(light.position.xyz);
			fAtt = (0.5 / ( A + B * distanceToLight + C * pow(distanceToLight, 2)) );

			//Atenuacion de los bordes 
			float innerCone = light.coneAngle*0.75;
			falloff = smoothstep(light.coneAngle, innerCone, lightToSurfaceAngle);
		}//Fuera del cono 
		else 
			fAtt = 0.0;
	}
	
	//Normal real del obj (WS -> TS) (Para testear el shader sin NormalMap)
	//norm = normalize( transpose(inverse(TBN)) * vec3(( (transpose(inverse(viewMatrix)) * vec4(fnormal,1.0)).xyz)) );

	//Ambiente
	vec3 ambient = material.Ka * light.Ia * diffR;

	//Difuso
	float sDotN = max( dot(LightPos, norm), 0.0 );
	vec3 diffuse = sDotN * light.Ip * material.Kd * diffR;

	//Especular
	vec3 spec = specMap;
	//spec *= cookTorranceSpecular(LightPos, ViewDir, norm, 0.25, 1);
	spec *= phongSpecular(LightPos, ViewDir, norm, material.Shininess);
	//spec *= blinnPhongSpecular(LightPos, ViewDir, norm, material.Shininess * 4);

	//Retorna el color final con conservacion de energia
	return (ambient + fAtt * falloff * (diffuse * 0.6 + spec * 0.4) ) * light.enabled;
}

void main() 
{
	// Lookup the normal from the normal map
	vec4 normal = 2.0*texture2D( NormalMapTex, f_TexCoord ) - 1;

	// The color texture is used as the diff. reflectivity
	vec4 texColor = texture2D( ColorTex, f_TexCoord );

	// The specular texture is used as the spec intensity
	vec4 specular = texture2D( SpecularMapTex, f_TexCoord ) ;

	//Transformar Posicion de CameraSpace a TangentSpace
	vec3 ViewDir = TBN * normalize(-fPos_CS);

	//Acumular iluminacion de cada fuente de luz
	vec3 linearColor=vec3(0);
	for(int i=0; i<numLights; i++)
		linearColor += phongModel(normal.xyz, texColor.rgb, specular.rgb, allLights[i], ViewDir);

	FragColor = vec4( linearColor, 1.0 );
}