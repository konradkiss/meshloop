//-----------------------------------------------------------------------------
// Torque 3D
// Copyright (C) GarageGames.com, Inc.
//-----------------------------------------------------------------------------

uniform mat4x4 modelview;

varying vec4 hpos;
varying vec2 uv0;


void main()
{
   hpos = vec4( modelview * gl_Vertex );
   gl_Position = hpos;

   uv0 = gl_MultiTexCoord0.st;
}
