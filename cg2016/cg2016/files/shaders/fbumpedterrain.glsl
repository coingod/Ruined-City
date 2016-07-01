// FRAGMENT SHADER. TEXTURE + BUMP + SPECULAR

#version 330
#define maxLights 10

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
};

struct Material {
	vec3 Ka;
	vec3 Kd;
	vec3 Ks;
	float Shininess;
};

//uniform mat4 modelMatrix;
uniform mat4 viewMatrix;

//SplatMap
uniform sampler2D ColorTex; 
//Texturas
uniform sampler2D Texture1; 
uniform sampler2D Texture2; 
uniform sampler2D Texture3; 
//Texturas
uniform sampler2D Normal1; 
uniform sampler2D Normal2; 
uniform sampler2D Normal3; 

uniform vec3 cameraPosition; //In World Space.

uniform int numLights;
uniform Light allLights[maxLights];
uniform Material material;

uniform float A;
uniform float B;
uniform float C;

out vec4 FragColor;

// --- SHADOW MAPPING ---
uniform int shadowsOn;
// Sampler del shadow map.
uniform sampler2D uShadowSampler;
// Posicion del fragmento en el espacio de la luz.
in vec4 fragPosLightSpace;

// Calcula la visibilidad del fragmento respecto la luz.
// Retorna 1 si es visible y 0 si no lo es.
float ShadowCalculation(vec4 fragPosLightSpace)
{
	//Por ahora tenemos una luz direccional. [1].
	vec3 lightDir = normalize(allLights[0].position.xyz - fragPosLightSpace.xyz);
	float bias    = max(0.0001 * (1.0 - dot(fnormal, lightDir)), 0.00001);
	float shadowDepth = texture(uShadowSampler, fragPosLightSpace.xy).z;
	float fragDepth   = fragPosLightSpace.z;

	float shadow = 0.0;
	vec2 texelSize = 1.0/textureSize(uShadowSampler, 0);
	for (int x = -1; x <= 1; ++x){
		for (int y = -1; y <= 1; ++y){
			float pcfDepth = texture( uShadowSampler, fragPosLightSpace.xy + vec2(x, y) * texelSize).z;
			shadow += fragDepth - bias > pcfDepth ? 1.0f : 0.0f;
		}
	}
	shadow /= 9;
	// Si el fragmento esta fuera del alcance del shadow map entonces es visible.
	if (fragDepth > 1.0)
		shadow = 0.0;

	return shadow;
}

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
vec3 phongModel( vec3 norm, vec3 diffR, Light light, vec3 ViewDir) 
{
	float fAtt = 1.0;
	vec3 LightPos;
	float falloff = 1.0;
	float shadow = 0;
	//vec3 fPos_CS = (viewMatrix * vec4(fPos_WS, 1.0)).xyz;

	if(light.position.w == 0)
	{ 
		LightPos = normalize( transpose(inverse(TBN)) * ( (transpose(inverse(viewMatrix)) * -light.position).xyz) );
		//LightPos = normalize( transpose(inverse(TBN)) * (-light.position).xyz);
		if(shadowsOn == 1)
			shadow = ShadowCalculation(fragPosLightSpace); 
	}
	else
	{
		//Transformar POSICION de la Luz de CameraSpace a TangentSpace
		LightPos = normalize( TBN * ( (viewMatrix * light.position).xyz - fPos_CS) );
		//LightPos = normalize( TBN * ((light.position).xyz - fPos_WS));

		//Restricciones del cono de luz
		vec3 coneDirection = normalize(TBN * (mat3(viewMatrix) * light.coneDirection) );
		//vec3 coneDirection = normalize(TBN * (light.coneDirection).xyz );
		vec3 rayDirection = -LightPos;
		float lightToSurfaceAngle = degrees(acos(dot(rayDirection, coneDirection)));
		//Dentro del cono
		if (lightToSurfaceAngle <= light.coneAngle) 
		{ 
			//Atenuacion a la distancia
			float distanceToLight = length(light.position.xyz);
			fAtt = (0.5 / ( A + B * distanceToLight + C * pow(distanceToLight, 2)) );

			if(light.coneAngle < 180)
			{
				//Atenuacion de los bordes 
				float innerCone = light.coneAngle*0.75;
				falloff = smoothstep(light.coneAngle, innerCone, lightToSurfaceAngle);
			}
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
	vec3 spec = vec3(1);
	//spec *= cookTorranceSpecular(LightPos, ViewDir, norm, 0.25, 1);
	spec *= phongSpecular(LightPos, ViewDir, norm, material.Shininess)*material.Ks;
	//spec *= blinnPhongSpecular(LightPos, ViewDir, norm, material.Shininess * 4);

	//Retorna el color final con conservacion de energia
	return (ambient + fAtt * (1 - shadow) * falloff * (diffuse * 0.6 + spec * 0.4) ) * light.enabled;
}

void main() 
{
	vec2 offsetTexCoords = vec2(f_TexCoord.s * 30, f_TexCoord.t * 50);
	//Obtenemos la contribucion de cada Textura en base al SplatMap
	vec4 splat  = texture2D(ColorTex, f_TexCoord);
	vec4 tex1 = texture2D(Texture1, offsetTexCoords) * splat.r;//vec4(0, 0, 1, 0) * splat.r;
	vec4 tex2 = texture2D(Texture2, offsetTexCoords) * splat.g;//vec4(0, 1, 0, 0) * splat.g;
	vec4 tex3 = texture2D(Texture3, offsetTexCoords) * splat.b;//vec4(1, 0, 0, 0) * splat.b;
	vec4 texColor = tex1 + tex2 + tex3;

	// Lookup the normal from the normal map
	vec4 norm1 = (2*texture2D(Normal1, offsetTexCoords)-1) * splat.r ;//vec4(0, 0, 1, 0) * splat.r;
	vec4 norm2 = (2*texture2D(Normal2, offsetTexCoords)-1) * splat.g;//vec4(0, 1, 0, 0) * splat.g;
	vec4 norm3 = (2*texture2D(Normal3, offsetTexCoords)-1) * splat.b;//vec4(1, 0, 0, 0) * splat.b;
	vec4 normal = norm1 + norm2 + norm3; //2.0*texture2D( NormalMapTex, offsetTexCoords) - 1;

	//Transformar Posicion de CameraSpace a TangentSpace
	vec3 ViewDir = TBN * normalize(-fPos_CS);

	//Acumular iluminacion de cada fuente de luz
	vec3 linearColor=vec3(0);
	for(int i=0; i<numLights; i++)
		linearColor += phongModel(normal.xyz, texColor.rgb, allLights[i], ViewDir);

	FragColor = vec4( linearColor, 1.0 );
}