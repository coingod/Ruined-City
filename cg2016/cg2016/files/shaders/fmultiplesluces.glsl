// FRAGMENT SHADER.
#version 330
#define maxLights 5

struct Material {
	vec3 Ka;
	vec3 Kd;
	vec3 Ks;
	float shininess;
};
struct Light {
	vec4 position;
	vec3 Ia;
	vec3 Id;
	vec3 Is;
	float coneAngle;
	vec3 coneDirection;
	int enabled;
};

in vec3 fragPos;
in vec3 fragNormal;
in vec2 f_TexCoord;

uniform int numLights;
uniform sampler2D gSampler; 
uniform mat4 modelMatrix;
uniform mat3 normalMatrix;
uniform vec3 cameraPosition; //In World Space.
uniform Light allLights[maxLights];
uniform Material material;
uniform float A;
uniform float B;
uniform float C;

out vec4 fragColor;

vec3 applyLight(Light light, Material material, vec3 surfacePos, vec3 surfaceNormal, vec3 surfaceToCamera) {
	float attenuation = 1.0;
	vec3 surfaceToLight;
	if (light.position.w == 0.0) { //Directional light
		surfaceToLight = normalize(-light.position.xyz);
		attenuation = 1.0; //no attenuation for directional lights.
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
	vec4 colorTex=texture2D(gSampler, f_TexCoord.st);

	//AMBIENT
	vec3 ambient = light.Ia * material.Ka * vec3(colorTex);

	//DIFUSSE
	float diffuseCoefficient = max(0.0, dot(surfaceNormal, surfaceToLight));
	vec3 diffuse = light.Id * material.Kd * diffuseCoefficient;

	//SPECULAR
	float specularCoefficient = 0.0;
	if (diffuseCoefficient > 0.0) {
		vec3 incidenceVector = -surfaceToLight;
		vec3 reflectionVector = reflect(incidenceVector, surfaceNormal);
		float cosAngle = max(0.0, dot(surfaceToCamera, reflectionVector));
		specularCoefficient = pow(cosAngle, material.shininess);
	}
	vec3 specular = light.Is * material.Ks * specularCoefficient;
	return ambient + attenuation * (diffuse + specular) * light.enabled;
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
