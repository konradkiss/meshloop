//-----------------------------------------------------------------------------
// Torque 3D
// Copyright (C) GarageGames.com, Inc.
//-----------------------------------------------------------------------------

#include "shaders/common/postFx/postFx.hlsl"

uniform sampler2D diffuseMap : register(S0);

struct VertToPix
{
   float4 hpos       : POSITION;

   float2 uv        : TEXCOORD0;
};

float rand( float2 co )
{
   float r = frac( sin( dot( co.xy, float2(12.9898,78.233) ) ) * 43758.5453 );
   return ( r * 2 ) - 1;
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

uniform float2 oneOverTargetSize;

float4 main( VertToPix IN ) : COLOR
{
   float4 OUT = 0;
   
   float2 texScale = 1.0;
   
   for ( int i=0; i < 4; i++ )
   {
      //float2 offset = oneOverTargetSize * rand( IN.uv );
      float2 offset = (oneOverTargetSize * texScale) * sNonUniformTaps[i];
      OUT += tex2D( diffuseMap, IN.uv + offset );
   }
   
   OUT /= 4;
   OUT.rgb = 0;

   return OUT;
}
