//-----------------------------------------------------------------------------
// Torque 3D
// Copyright (C) GarageGames.com, Inc.
//-----------------------------------------------------------------------------

#include "../../../../../../shaders/common/postFx/postFx.hlsl"
#include "../../../../../../shaders/common/torque.hlsl"

float2 oneOverTargetSize;
float4 rtParams0;

struct VertToPix
{
   float4 hpos       : POSITION;
   float2 uv        : TEXCOORD0;
};

float rand( float2 co )
{
   return 0.5 + ( frac( sin( dot( co.xy, float2(12.9898,78.233) ) ) * 43758.5453 ) ) * 0.5;
}

static float2 sNonUniformTaps[8] = 
{      
   { 0.992833, 0.979309 },
   { -0.998585, 0.985853 },
   { 0.949299, -0.882562 },
   { -0.941358, -0.893924 },
   { 0.545055, -0.589072 },
   { 0.346526, 0.385821 },
   { -0.260183, 0.334412 },
   { 0.248676, -0.679605 },
};

VertToPix main( PFXVert IN )
{
   VertToPix OUT;
   
   OUT.hpos = IN.pos;
   float2 uv = viewportCoordToRenderTarget( IN.uv, rtParams0 ); 
   
   //float2 blurSize = oneOverTargetSize * 5;
      
   OUT.uv = uv;// + ( blurSize * rand( sNonUniformTaps[0] ) );
   /*OUT.uv1 = IN.uv + ( blurSize * rand( sNonUniformTaps[1] ) );
   OUT.uv2 = IN.uv + ( blurSize * rand( sNonUniformTaps[2] ) );
   OUT.uv3 = IN.uv + ( blurSize * rand( sNonUniformTaps[3] ) );

   OUT.uv4 = IN.uv - ( blurSize * rand( sNonUniformTaps[4] ) );
   OUT.uv5 = IN.uv - ( blurSize * rand( sNonUniformTaps[5] ) );
   OUT.uv6 = IN.uv - ( blurSize * rand( sNonUniformTaps[6] ) );
   OUT.uv7 = IN.uv - ( blurSize * rand( sNonUniformTaps[7] ) );*/
   
   return OUT;
}
