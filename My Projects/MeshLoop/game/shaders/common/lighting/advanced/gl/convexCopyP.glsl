//-----------------------------------------------------------------------------
// Torque 3D
// Copyright (C) GarageGames.com, Inc.
//-----------------------------------------------------------------------------

#include "farFrustumQuad.glsl"

varying vec4 wsEyeDir;
varying vec4 ssPos;

uniform sampler2D lightInfoBuffer;
uniform float4 renderTargetParams;

void main()
{ 
   // Compute scene UV
   vec3 ssPos = ssPos.xyz / ssPos.w;
   vec2 uvScene = getUVFromSSPos( ssPos, renderTargetParams );
   
   // Sample and return
   return texture2DLod( lightInfoBuffer, uvScene, 0 );
}
