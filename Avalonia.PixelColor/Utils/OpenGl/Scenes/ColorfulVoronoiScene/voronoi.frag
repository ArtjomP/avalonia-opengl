#version 330 core

precision highp float;

uniform float time;
uniform float resolution_x;
uniform float resolution_y;
uniform float line_width;
uniform float inner_gradient_width;
uniform float outer_gradient_width;

#define gl_FragColor isf_FragColor
out vec4 gl_FragColor;

//https://iquilezles.org/articles/palettes/
vec3 palette( float t ) {
    vec3 a = vec3(0.5, 0.5, 0.5);
    vec3 b = vec3(0.5, 0.5, 0.5);
    vec3 c = vec3(1.0, 1.0, 1.0);
    vec3 d = vec3(0.263,0.416,0.557);

    return a + b * cos(6.28318 * (c * t + d));
}

void mainImage( out vec4 fragColor, in vec2 fragCoord ) {
    vec2 resolution = vec2(resolution_x, resolution_y);

    vec2 uv = (fragCoord * 2.0 - resolution.xy) / min(resolution.x, resolution.y);
    vec2 uv0 = uv;
    vec3 finalColor = vec3(0.0);

    for (float i = 0.0; i < 4.0; i++) {
        uv = fract(uv * 1.5) - 0.5;

        float d = length(uv) * exp(-length(uv0));

        vec3 col = palette(length(uv0) + i*.4 + time*.4);

        d = tan(d * 8. + time)/8.;
        float s = d; //tan(d * 8. + time)/8.;
        d = abs(d);                
        d = pow(0.01 / d, 1.2);
        float p = clamp(d * (line_width + outer_gradient_width + inner_gradient_width), 0.0, 1.0);
        float lw = 1. - line_width;
        if(p > lw)
            finalColor += col;
        if(s >= 0 && p >= lw - outer_gradient_width && p <= lw) 
            finalColor += col * p * 0.9;
        if(s < 0 && p >= lw - inner_gradient_width && p <= lw) 
            finalColor += col * p * 0.9;
    }

    fragColor = vec4(finalColor, 1.0);
}

void main() {
	vec4 fragment_color;
	mainImage(fragment_color, gl_FragCoord.xy);
	gl_FragColor = fragment_color;
}