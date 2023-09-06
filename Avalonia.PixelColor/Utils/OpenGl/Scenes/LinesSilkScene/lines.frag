// based https://github.com/Vidvox/ISF-Files/blob/master/ISF/Lines.fs

#version 330 core

uniform float spacing;
uniform float line_width;
uniform float left_gradient_width;
uniform float right_gradient_width;
uniform float angle;
uniform float shift;
uniform vec4 color1;
uniform vec4 color2;

in vec4 isf_Position;
in vec2 isf_FragNormCoord;
uniform vec2 RENDERSIZE;
#define gl_FragColor isf_FragColor
out vec4 gl_FragColor;

const float pi = 3.14159265359;

float pattern() {
	float s = sin(angle * pi);
	float c = cos(angle * pi);
	vec2 tex = isf_FragNormCoord * RENDERSIZE;
	float spaced = RENDERSIZE.y * spacing;
	vec2 point = vec2( c * tex.x - s * tex.y, s * tex.x + c * tex.y ) * max(1.0 / spaced, 0.001);
	float d = point.y;
	float w = left_gradient_width + right_gradient_width + line_width;
	float center = (left_gradient_width + line_width / 2.0) / w;
	return ( mod(d + shift * spacing + w * center, spacing) );
}


void main() {
	vec4 out_color = color2;
	float w = left_gradient_width + right_gradient_width + line_width;

	float pat = pattern();

	if ((left_gradient_width > 0)&&(pat > 0.0)&&(pat <= left_gradient_width))	{
		float percent = (1.0 - abs(left_gradient_width - 1.0 * pat) / left_gradient_width);
		percent = clamp(percent,0.0,1.0);
		out_color = mix(color2,color1,percent);  
	}
	if ((pat > left_gradient_width)&&(pat <= left_gradient_width + line_width))	{
		out_color = color1; 
	}
	if ((right_gradient_width > 0)&&(pat > left_gradient_width + line_width)&&(pat <= w))	{
		float percent = (1.0 - abs(right_gradient_width - 1.0 * (w - pat)) / right_gradient_width);
		percent = clamp(percent,0.0,1.0);
		out_color = mix(color2,color1,percent);  
	}
	
	gl_FragColor = out_color;
}