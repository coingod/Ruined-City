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
uniform sampler2D DepthMapTex;
uniform mat4 viewMatrix;
uniform vec3 cameraPosition; //In World Space.
uniform int numLights;
uniform Light allLights[maxLights];
uniform Material material;

uniform float A;
uniform float B;
uniform float C;
uniform float height_scale;

out vec4 FragColor;

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
	vec3 spec = vec3(1);//specMap;
	//spec *= cookTorranceSpecular(LightPos, ViewDir, norm, 0.25, 1);
	spec *= phongSpecular(LightPos, ViewDir, norm, material.Shininess);
	//spec *= blinnPhongSpecular(LightPos, ViewDir, norm, material.Shininess * 4);

	//Retorna el color final con conservacion de energia
	return (ambient + fAtt * falloff * (diffuse * 0.6 + spec * 0.4) ) * light.enabled;
}

vec2 ParallaxMapping(vec2 texCoords, vec3 viewDir)
{ 
    float height = texture2D(DepthMapTex, texCoords).r;    
    vec2 p = viewDir.xy / viewDir.z * (height * height_scale);
    return texCoords - p;    
} 

vec2 ParallaxOcclusionMapping(vec2 texCoords, vec3 viewDir)
{ 
    // number of depth layers
    //const float numLayers = 10;
	const float minLayers = 8;
	const float maxLayers = 32;
	float numLayers = mix(maxLayers, minLayers, abs(dot(vec3(0.0, 0.0, 1.0), viewDir))); 
    // calculate the size of each layer
    float layerDepth = 1.0 / numLayers;
    // depth of current layer
    float currentLayerDepth = 0.0;
    // the amount to shift the texture coordinates per layer (from vector P)
    vec2 P = viewDir.xy * height_scale; 
    vec2 deltaTexCoords = P / numLayers;
	// get initial values
	vec2  currentTexCoords     = texCoords;
	float currentDepthMapValue = texture2D(DepthMapTex, currentTexCoords).r;
  
	while(currentLayerDepth < currentDepthMapValue)
	{
		// shift texture coordinates along direction of P
		currentTexCoords -= deltaTexCoords;
		// get depthmap value at current texture coordinates
		currentDepthMapValue = texture2D(DepthMapTex, currentTexCoords).r;  
		// get depth of next layer
		currentLayerDepth += layerDepth;  
	}

	// get texture coordinates before collision (reverse operations)
	vec2 prevTexCoords = currentTexCoords + deltaTexCoords;

	// get depth after and before collision for linear interpolation
	float afterDepth  = currentDepthMapValue - currentLayerDepth;
	float beforeDepth = texture2D(DepthMapTex, prevTexCoords).r - currentLayerDepth + layerDepth;
 
	// interpolation of texture coordinates
	float weight = afterDepth / (afterDepth - beforeDepth);
	vec2 finalTexCoords = prevTexCoords * weight + currentTexCoords * (1.0 - weight);

	return finalTexCoords;    
}  

void main() 
{
	//Transformar Posicion de la Camara a TangentSpace
	vec3 ViewPos = TBN * vec3(0);
	//Transformar Posicion de CameraSpace a TangentSpace
	vec3 FragPos = TBN * normalize(fPos_CS);
	//Desplazar coordenadas de textura con Parallax Mapping
	vec3 ViewDir = normalize(ViewPos - FragPos);
	vec2 texCoords = ParallaxOcclusionMapping(f_TexCoord, ViewDir);
	
	//Descartar fragmentos con coordenadas invalidas
	if(texCoords.x > 1.0 || texCoords.y > 1.0 || texCoords.x < 0.0 || texCoords.y < 0.0)
		discard;
	
	// Lookup the normal from the normal map
	vec4 normal = 2.0*texture2D( NormalMapTex, texCoords ) - 1;

	// The color texture is used as the diff. reflectivity
	vec4 texColor = texture2D( ColorTex, texCoords );

	//Acumular iluminacion de cada fuente de luz
	vec3 linearColor=vec3(0);
	for(int i=0; i<numLights; i++)
		linearColor += phongModel(normal.xyz, texColor.rgb, allLights[i], ViewDir);

	FragColor = vec4( linearColor, 1.0 );
}