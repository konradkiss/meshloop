//-----------------------------------------------------------------------------
// Torque 3D
// Copyright (C) GarageGames.com, Inc.
//-----------------------------------------------------------------------------

#include "./postFx.hlsl"
#include "./../torque.hlsl"


uniform float2 texSize0;

struct VertToPix
{
   float4 hpos       : POSITION;

   float2 uv0        : TEXCOORD0;
   float2 uv1        : TEXCOORD1;
   float2 uv2        : TEXCOORD2;
   float2 uv3        : TEXCOORD3;

   float2 uv4        : TEXCOORD4;
   float2 uv5        : TEXCOORD5;
   float2 uv6        : TEXCOORD6;
   float2 uv7        : TEXCOORD7;
};

VertToPix main( PFXVert IN )
{
   VertToPix OUT;
   
   OUT.hpos = IN.pos;

   OUT.uv0 = IN.uv + ( ( BLUR_DIR * 3.5f ) / texSize0 );
   OUT.uv1 = IN.uv + ( ( BLUR_DIR * 2.5f ) / texSize0 );
   OUT.uv2 = IN.uv + ( ( BLUR_DIR * 1.5f ) / texSize0 );
   OUT.uv3 = IN.uv + ( ( BLUR_DIR * 0.5f ) / texSize0 );

   OUT.uv4 = IN.uv - ( ( BLUR_DIR * 3.5f ) / texSize0 );
   OUT.uv5 = IN.uv - ( ( BLUR_DIR * 2.5f ) / texSize0 );
   OUT.uv6 = IN.uv - ( ( BLUR_DIR * 1.5f ) / texSize0 );
   OUT.uv7 = IN.uv - ( ( BLUR_DIR * 0.5f ) / texSize0 );
   
   return OUT;
}
