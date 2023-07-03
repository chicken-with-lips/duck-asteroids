#version 330 core
layout (location = 0) in vec3 attr_Position;

out vec3 Color;

uniform mat4 uModel;
uniform mat4 uView;
uniform mat4 uProjection;

void main()
{
    gl_Position = uProjection * uView * uModel * vec4(attr_Position, 1.0);
    Color = vec3(1,0,0);
}
