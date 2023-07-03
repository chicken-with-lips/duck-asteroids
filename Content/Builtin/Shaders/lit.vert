#version 330 core

layout (location = 0) in vec3 attr_Position;
layout (location = 1) in vec3 attr_Normal;
layout (location = 2) in vec2 attr_TexCoord;

uniform mat4 uTransform;
uniform mat4 uView;
uniform mat4 uProjection;

out vec3 FragPosition;
out vec3 Normal;
out vec2 TexCoords;

void main()
{
    gl_Position = uProjection * uView * uTransform * vec4(attr_Position, 1.0);
    FragPosition = vec3(uTransform * vec4(attr_Position, 1.0));
    Normal = mat3(transpose(inverse(uTransform))) * attr_Normal;
    TexCoords = attr_TexCoord;
} 
