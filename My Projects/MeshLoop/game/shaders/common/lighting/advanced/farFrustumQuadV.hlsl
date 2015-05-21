//-----------------------------------------------------------------------------
// Torque 3D
// Copyright (C) GarageGames.com, Inc.
//-----------------------------------------------------------------------------

#include "../../hlslStructs.h"
#include "farFrustumQuad.hlsl"


FarFrustumQuadConnectV main( VertexIn_PNT IN,
                             uniform float4 rtParams0,
                             uniform float4x4 worldToCamera )
{
   FarFrustumQuadConnectV OUT;

   // Expand the SS coordinate (stored in uv0)
   OUT.hpos = float4( IN.uv0 * 2.0 - 1.0, 1.0, 1.0 );
   
   // Get a RT-corrected UV from the SS coord
   OUT.uv0 = getUVFromSSPos( OUT.hpos.xyz, rtParams0 );
   
   // Interpolators will generate eye ray from far-frustum corners
   OUT.wsEyeRay = IN.pos.xyz;
   OUT.vsEyeRay = mul(worldToCamera, IN.pos).xyz;

   return OUT;
}
