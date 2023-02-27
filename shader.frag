#version 330 core
out vec4 FragColor;

in vec3 Normal;
in vec3 FragPos;

vec3 lightColor = vec3(1.0f, 1.0f, 1.0f);
vec3 objectColor = vec3(0.0f, 0.7f, 0.1f);

vec3 lightPos = vec3(100.0f, 100.0f, 100.0f);

void main()
{
    vec3 norm = normalize(Normal);
    //vec3 lightDir = normalize(lightPos - FragPos); 

    vec3 lightDir = normalize(vec3(1.0f, 1.5f, 1.5f));

    float diff = max(dot(norm, lightDir), 0.0);
    vec3 diffuse = diff * lightColor;

    float ambientStrength = 0.3;
    vec3 ambient = ambientStrength * lightColor;

    vec3 result = (ambient + diffuse) * objectColor;
    FragColor = vec4(result, 1.0);
}