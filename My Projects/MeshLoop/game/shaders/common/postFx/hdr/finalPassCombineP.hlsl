//-----------------------------------------------------------------------------
// Torque 3D
// Copyright (C) GarageGames.com, Inc.
//-----------------------------------------------------------------------------

#include "../../torque.hlsl"
#include "../postFx.hlsl"

uniform sampler2D sceneTex : register( s0 );
uniform sampler2D luminanceTex : register( s1 );
uniform sampler2D bloomTex : register( s2 );

uniform float2 texSize0;
uniform float2 texSize2;

uniform float g_fEnableToneMapping;
uniform float g_fMiddleGray;
uniform float g_fWhiteCutoff;

uniform float g_fEnableBlueShift;
uniform float3 g_fBlueShiftColor; 

uniform float g_fBloomScale;


float4 main( PFXVertToPix IN ) : COLOR0
{
   float4 sample = hdrDecode( tex2D( sceneTex, IN.uv0 ) );
   float adaptedLum = tex2D( luminanceTex, float2( 0.5f, 0.5f ) ).r;
   float4 bloom = tex2D( bloomTex, IN.uv0 );

   // For very low light conditions, the rods will dominate the perception
   // of light, and therefore color will be desaturated and shifted
   // towards blue.
   if ( g_fEnableBlueShift > 0.0f )
   {
      const float3 LUMINANCE_VECTOR = float3(0.2125f, 0.7154f, 0.0721f);

      // Define a linear blending from -1.5 to 2.6 (log scale) which
      // determines the lerp amount for blue shift
      float coef = 1.0f - ( adaptedLum + 1.5 ) / 4.1;
      coef = saturate( coef * g_fEnableBlueShift );

      // Lerp between current color and blue, desaturated copy
      float3 rodColor = dot( sample.rgb, LUMINANCE_VECTOR ) * g_fBlueShiftColor;
      sample.rgb = lerp( sample.rgb, rodColor, coef );
   }

   // Map the high range of color values into a range appropriate for
   // display, taking into account the user's adaptation level, 
   // white point, and selected value for for middle gray.
   if ( g_fEnableToneMapping > 0.0f )
   {
      float Lp = (g_fMiddleGray / (adaptedLum + 0.0001)) * hdrLuminance( sample.rgb );
      float toneScalar = ( Lp * ( 1.0 + ( Lp / ( g_fWhiteCutoff ) ) ) ) / ( 1.0 + Lp );
      sample.rgb = lerp( sample.rgb, sample.rgb * toneScalar, g_fEnableToneMapping );
   }

   // Add the bloom effect.
   sample += g_fBloomScale * bloom;

   return sample;
}
