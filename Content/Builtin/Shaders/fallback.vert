#version 330 core
layout (location = 0) in vec3 attr_Position;
//layout (location = 1) in vec3 attr_Normal;
layout (location = 1) in vec2 attr_TexCoord;

out vec3 Color;
//out vec2 Normal;
out vec2 TexCoord;

uniform mat4 uTransform;
uniform mat4 uView;
uniform mat4 uProjection;

void main()
{
    gl_Position = uProjection * uView * uTransform * vec4(attr_Position, 1.0);
    Color = vec3(1,1,1);
    TexCoord = attr_TexCoord;
}
