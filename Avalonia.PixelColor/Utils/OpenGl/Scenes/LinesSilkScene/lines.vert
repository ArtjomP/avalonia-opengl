//Here we specify the version of our shader.
#version 330 core
layout (location = 0) in vec4 VERTEXDATA;

uniform float shift;

uniform vec2 RENDERSIZE;
out vec2 isf_FragNormCoord;
out vec4 isf_Position;
        
void main()
{
mat4 projectionMatrix = mat4(2./RENDERSIZE.x, 0., 0., -1.,
                                 0., 2./RENDERSIZE.y, 0., -1.,
                                 0., 0., -1., 0.,
                                 0., 0., 0., 1.);
    gl_Position = VERTEXDATA;// * projectionMatrix;
    isf_Position = gl_Position;
    isf_FragNormCoord = vec2((gl_Position.x+1.0)/2.0, (gl_Position.y+1.0)/2.0);
}