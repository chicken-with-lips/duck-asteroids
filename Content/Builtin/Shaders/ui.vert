#version 330 core

uniform mat4 uTransform;
uniform mat4 uProjection;

layout (location = 0) in vec2 inPosition;
layout (location = 1) in vec3 inNormal0;
layout (location = 2) in vec2 inTexCoord0;
layout (location = 3) in vec4 inColor0;

out vec2 fragTexCoord;
out vec4 fragColor;

void main()
{
    fragTexCoord = inTexCoord0;
    fragColor = inColor0;

    vec4 outPos = uProjection * uTransform * vec4(inPosition.xy, 0, 1);

    gl_Position = outPos;
}
