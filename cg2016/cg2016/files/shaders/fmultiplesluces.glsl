﻿// FRAGMENT SHADER.
#version 330
#define maxLights 5

struct Material {
	vec3 Ka;
	vec3 Kd;
	vec3 Ks;
	float Shininess;
};
struct Light {
	vec4 position;
	vec3 Ia;
	vec3 Ip;
	float coneAngle;
	vec3 coneDirection;
	int enabled;
};

in vec3 fragPos;
in vec3 fragNormal;
in vec2 f_TexCoord;

uniform int numLights;
uniform sampler2D ColorTex; 
//uniform sampler2D gSampler2;
uniform mat4 viewMatrix;
uniform mat4 modelMatrix;
uniform mat3 normalMatrix;	//IN WORLD SPACE!!!
uniform vec3 cameraPosition; //In World Space.
uniform Light allLights[maxLights];
uniform Material material;
uniform float A;
uniform float B;
uniform float C;

out vec4 fragColor;

// --- SHADOW MAPPING ---
// Sampler del shadow map.
uniform sampler2D uShadowSampler;
uniform int shadowsOn;
// Posicion del fragmento en el espacio de la luz.
in vec4 fragPosLightSpace;

// Calcula la visibilidad del fragmento respecto la luz.
// Retorna 1 si es visible y 0 si no lo es.
float ShadowCalculation(vec4 fragPosLightSpace)
{
	//Por ahora tenemos una luz direccional. [1].
	vec3 lightDir = normalize(allLights[0].position.xyz - fragPosLightSpace.xyz);
	float bias    = max(0.0001 * (1.0 - dot(fragNormal, lightDir)), 0.00001);
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

vec3 applyLight(Light light, Material material, vec3 surfacePos, vec3 surfaceNormal, vec3 surfaceToCamera) {
	float attenuation = 1.0;//no attenuation for directional lights.
	vec3 surfaceToLight;
	float shadow = 0;
	if (light.position.w == 0.0) { //Directional light
		surfaceToLight = normalize(-light.position.xyz);
		if (shadowsOn == 1)
			shadow = ShadowCalculation(fragPosLightSpace); 
	} else { //Positional light (Spot or Point)
		surfaceToLight = normalize(light.position.xyz - surfacePos);
		//Cone restrictions
		vec3 coneDirection = normalize(light.coneDirection);
		vec3 rayDirection = -surfaceToLight;
		float lightToSurfaceAngle = degrees(acos(dot(rayDirection, coneDirection)));
		if (lightToSurfaceAngle <= light.coneAngle) { //Inside cone
			float distanceToLight = length(light.position.xyz - surfacePos);
			attenuation = 1.0 / ( A + B * distanceToLight + C * pow(distanceToLight, 2));
		} else {
			attenuation = 0.0;
		}
	}

	//Obtenemos el color de la textura
	vec4 colorTex=texture2D(ColorTex, f_TexCoord);

	//Descarto los fragmentos con valor de transparencia
	if(colorTex.a < 0.5)
		discard;
	//vec4 colorTex=colorTex2 * texture2D(gSampler2, f_TexCoord);

	//AMBIENT
	vec3 ambient = light.Ia * material.Ka * vec3(colorTex);

	//DIFUSSE
	float diffuseCoefficient = max(0.0, dot(surfaceNormal, surfaceToLight));
	vec3 diffuse = light.Ip * material.Kd * diffuseCoefficient * vec3(colorTex);

	//SPECULAR
	float specularCoefficient = 0.0;
	if (diffuseCoefficient > 0.0) {
		vec3 incidenceVector = -surfaceToLight;
		vec3 reflectionVector = reflect(incidenceVector, surfaceNormal);
		float cosAngle = max(0.0, dot(surfaceToCamera, reflectionVector));
		specularCoefficient = pow(cosAngle, material.Shininess);
	}
	vec3 specular = light.Ip * material.Ks * specularCoefficient;
	return ambient + attenuation * (1 - shadow) * (diffuse + specular) * light.enabled;
}

void main() {
	vec3 surfacePos = vec3(modelMatrix * vec4(fragPos, 1));
	vec3 surfaceNormal = normalize(normalMatrix * fragNormal);
	vec3 surfaceToCamera = normalize(cameraPosition - surfacePos);
	
	vec3 linearColor=vec3(0);
	for(int i=0; i<numLights; i++)
		linearColor += applyLight(allLights[i], material, surfacePos, surfaceNormal, surfaceToCamera);

	fragColor = vec4(linearColor, 1.0);

}
