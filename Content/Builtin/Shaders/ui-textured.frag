#version 330 core

uniform sampler2D texture0;

in vec2 fragTexCoord;
in vec4 fragColor;

out vec4 finalColor;

void main()
{
    vec4 texColor = texture(texture0, fragTexCoord);
//    finalColor = fragColor * texColor;
    finalColor = fragColor;
}
