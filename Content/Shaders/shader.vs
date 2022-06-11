#version 330 core
layout (location = 0) in vec3 attr_Position;
layout (location = 1) in vec3 attr_Normal;
layout (location = 2) in vec2 attr_TexCoord;

out vec3 Color;
//out vec2 Normal;
out vec2 TexCoord;

uniform mat4 in_Model;
uniform mat4 in_View;
uniform mat4 in_Projection;

void main()
{
    gl_Position = in_Projection * in_View * in_Model * vec4(attr_Position, 1.0);
    Color = vec3(1,1,1);
    TexCoord = attr_TexCoord;
}
