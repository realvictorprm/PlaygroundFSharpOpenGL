#version 330 core
struct Material{
    vec3 ambient;
    vec3 diffuse;
    vec3 specular;
    float shininess;
};

in vec3 Normal;
in vec3 FragPos;
in vec2 TexCoords;

out vec4 FragColor;

uniform bool useTexture;
uniform float time;
uniform vec3 objectColor;
uniform vec3 lightColor;
uniform vec3 lightPos;
uniform vec3 viewPos;
uniform Material material;

uniform mat4 view;
uniform samplerCube skybox; 
uniform sampler2D texture_diffuse1;
uniform sampler2D texture_diffuse2;
uniform sampler2D texture_diffuse3;
uniform sampler2D texture_specular1;
uniform sampler2D texture_specular2;

vec3 interpolate(vec3 a, vec3 b, float c){
    //c = abs(normalize(c));
    float d = 1. - c;
    return normalize(a * c + b * d);
}

vec3 interpolate3(vec3 a, vec3 b, vec3 c, float d){
    if(d > 0.5){
        return interpolate(c, b, (d - 0.5) * 2.);
    }
    else{
        return interpolate(b, a, d * 2.);
    }
}

float triangleFunc(float x){
    if(sin(x) < 0.)
        return 1. - abs(x - floor(x));
    return abs(x - floor(x));
}

float lightDecraseFactor(){
    return 1. / length(lightPos - FragPos);
}

void main()
{
    if(false){
        //texture(floorTexture, fs_in.TexCoords).rgb;
        vec3 color = objectColor;
        // ambient
        vec3 ambient = color * material.ambient;
        // diffuse
        vec3 lightDir = normalize(lightPos - FragPos);
        vec3 normal = normalize(Normal);
        float diff = max(dot(lightDir, normal), 0.0);
        // vec3 diffuseHdrColor = texture(texture_diffuse1, TexCoords).rgb;
        // vec3 diffuseMapped = diffuseHdrColor / (diffuseHdrColor + vec3(1.0));
        float distanceFactor = 1.0;//1. / (0.1 * length(lightPos - FragPos));
        vec3 diffuse = diff * color * material.diffuse * distanceFactor;

        // specular
        vec3 viewDir = normalize(viewPos - FragPos);
        vec3 halfwayDir = normalize(lightDir + viewDir);  
        float spec = pow(max(dot(normal, halfwayDir), 0.0), material.shininess * 128);
        vec3 specular = spec * material.specular; //* vec3(texture(texture_specular1, TexCoords)); // assuming bright white light color
        
        // reflection
        float ratio = 1.00 / 1.52;
        vec3 R = reflect(normalize(+ viewPos - FragPos), normal);//refract(normalize(FragPos - viewPos), normal, 1.);

        float exposure = 1.;
        vec3 res = ambient + diffuse + specular;
        res *= vec3(texture(skybox, R).rgb);
        vec3 mapped = vec3(1.0) - exp(-res * exposure);
        FragColor = vec4(res, 1.0);
    }else{
        vec3 color = vec3(1., 1., 1.);
        // ambient
        vec3 ambient = 0.1 * color;// * material.ambient ;
        // diffuse
        vec3 lightDir = normalize(lightPos - FragPos);
        vec3 normal = normalize(Normal);
        float diff = max(dot(lightDir, normal), 0.0);
        // vec3 diffuseHdrColor = texture(texture_diffuse1, TexCoords).rgb;
        // vec3 diffuseMapped = diffuseHdrColor / (diffuseHdrColor + vec3(1.0));
        vec3 distanceFactor = vec3(1., 1., 1.) * exp(-length(lightPos - FragPos) * .5);//1. / (2. * length(lightPos - FragPos));
        vec3 diffuse = diff * (vec3(1., 1., 1.)) * distanceFactor; //* material.diffuse * distanceFactor;
        // specular
        vec3 viewDir = normalize(viewPos - FragPos);
        vec3 halfwayDir = normalize(lightDir + viewDir);  
        float spec = pow(max(dot(normal, halfwayDir), 0.0), material.shininess * 128);
        vec3 specular = spec * vec3(1., 9.3, 2.1) * 2.9;//material.specular; //* vec3(texture(texture_specular1, TexCoords)); // assuming bright white light color
        // reflection
        //vec3 R = refract(normalize(FragPos - viewPos), normal, 1.);//reflect(normalize(FragPos - viewPos), normal);
        vec3 R = reflect(normalize(- viewPos + FragPos), normal);
        float exposure = 1.;
        vec3 res = ambient + diffuse * 1.; //+ specular;
        //res *= (vec3(texture(skybox, R).rgb) * 0.1);
        vec3 mapped = vec3(1.0) - exp(-res * exposure);
        FragColor = vec4(mapped, 1.0);
    }
}