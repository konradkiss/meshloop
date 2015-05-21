//-----------------------------------------------------------------------------
// Torque 3D
// Copyright (C) GarageGames.com, Inc.
//-----------------------------------------------------------------------------

#include "../postFx.hlsl"
#include "shadergen:/autogenConditioners.h"


uniform sampler2D inputTex : register(S0);
uniform float2 oneOverTargetSize;
uniform float brightPassThreshold;

static const float3 LUMINANCE_VECTOR = float3(0.3125f, 0.6154f, 0.0721f);


static float2 gTapOffsets[4] = 
{
   { -0.5, 0.5 },  { 0.5, -0.5 },
   { -0.5, -0.5 }, { 0.5, 0.5 }
};

float4 main( PFXVertToPix IN ) : COLOR
{
   float4 average = { 0.0f, 0.0f, 0.0f, 0.0f };

   // Combine and average 4 samples from the source HDR texture.
   for( int i = 0; i < 4; i++ )
      average += tex2D( inputTex, IN.uv0 + ( gTapOffsets[i] * oneOverTargetSize ) );
   average *= 0.25f;

   // Determine the brightness of this particular pixel.
   float lum = dot( average.rgb, LUMINANCE_VECTOR );
   
   // Determine whether this pixel passes the test...
   if ( lum < brightPassThreshold )
      average = float4( 0.0f, 0.0f, 0.0f, 1.0f );

   // Write the colour to the bright-pass render target
   return average;
}
