//-----------------------------------------------------------------------------
// Torque 3D
// Copyright (C) GarageGames.com, Inc.
//-----------------------------------------------------------------------------

struct Conn
{
   float2 texCoord : TEXCOORD0;
   float4 color : COLOR0;
   float fade : TEXCOORD1;
};

uniform sampler2D inputTex : register(S0);
uniform float4 ambient;

static float3 LUMINANCE_VECTOR  = float3(0.2125f, 0.4154f, 0.1721f);
static float esmFactor = 200.0;
   
float4 main( Conn IN ) : COLOR0
{
   float4 Out;

   float lum = dot( ambient.rgb, LUMINANCE_VECTOR );   

   Out.rgb = ambient.rgb * lum; 
   Out.a = 0;
   float depth = tex2D( inputTex, IN.texCoord ).a;

   depth = depth * exp( depth - 10.0 );
   depth = exp( esmFactor * depth ) - 1.0;

   Out.a = saturate( depth * 300 ) * ( 1.0 - lum ) * IN.fade * IN.color.a;   
   
   return Out;
}
