#version 330 core

in vec2 fragTexCoord;
out vec4 finalColor;
uniform sampler2D texture0;
//uniform vec4 bgColor;
//uniform vec4 fgColor;

float median(float r, float g, float b) {
    return max(min(r, g), min(max(r, g), b));
}

//void main() {
//    vec3 sample2 = texture(texture0, fragTexCoord).rgb;
//    ivec2 sz = textureSize(texture0, 0);
//    float dx = dFdx(fragTexCoord.x) * sz.x;
//    float dy = dFdy(fragTexCoord.y) * sz.y;
//    float toPixels = 2.0 * inversesqrt(dx * dx + dy * dy);
//    float sigDist = median(sample2.r, sample2.g, sample2.b) - 0.5;
//    float opacity = clamp(sigDist * toPixels + 0.5, 0.0, 1.0);
//
//    finalColor = mix(vec4(0, 0, 0, 1), vec4(1, 1, 1, 1), opacity);
//    finalColor = vec4(0, 0, 0, 1) * opacity;
//}
//
void main() {
//    float screenPxRange = 4 / 32;
    float screenPxRange = 32 / 4;
    screenPxRange = 3; // 32 scale
    screenPxRange = 1.6;
    
    vec3 msd = texture(texture0, fragTexCoord).rgb;
    float sd = median(msd.r, msd.g, msd.b);
    float screenPxDistance = screenPxRange * (sd - 0.5);
    float opacity = clamp(screenPxDistance + 0.5, 0.0, 1.0);
    vec4 f_color = vec4(1, 1, 1, opacity);

    finalColor = mix(vec4(1, 1, 1, 1), vec4(0, 0, 0, 1), opacity);
    finalColor = mix(vec4(0, 0, 0, 1), vec4(1, 1, 1, 1), opacity);
    finalColor = vec4(0,0,0, opacity);
//       finalColor = f_color;
//    finalColor = vec4(1,1,1,1);
    
}

//void main() {
//    vec3 s = texture(texture0, fragTexCoord).rgb;
//    ivec2 sz = textureSize(texture0, 0);
//    float dx = dFdx(fragTexCoord.x) * sz.x;
//    float dy = dFdy(fragTexCoord.y) * sz.y;
//    float toPixels = 3.0 * inversesqrt(dx * dx + dy * dy);
//    float sigDist = median(s.r, s.g, s.b) - 0.5;
//    float opacity = clamp(sigDist * toPixels + 0.5, 0.0, 1.0);
//
//    finalColor = vec4(0, 0, 0, 1) * opacity;
//}
