//-----------------------------------------------------------------------------
// Torque Game Engine Advanced
// Copyright (C) GarageGames.com, Inc.
//-----------------------------------------------------------------------------

#ifdef TORQUE_ADVANCED_LIGHTING
#include "shadergen:/autogenConditioners.h"
#endif

#include "groundCover.hlsl"


float4 main(   VertOut IN,
               uniform sampler2D diffuseMap : register(S0)

               #ifdef TORQUE_ADVANCED_LIGHTING

                  ,
                  uniform sampler2D lightTex : register(S1),
                  uniform float4 lightRT : register(C0)

               #endif
               
               ) : COLOR
{
   #ifdef TORQUE_ADVANCED_LIGHTING

      // Convert the screen position into the light tex space.
      float2 lightCoord = IN.vPos.xy / IN.vPos.w;
      lightCoord = ( lightCoord + 1.0 ) / 2.0;
      lightCoord = ( lightCoord * lightRT.zw ) + lightRT.xy;
      lightCoord.y = 1.0 - lightCoord.y; 

      // Get the lighting at this pixel.
      float4 lightColor;
      float nlAttenuation, specularPower;
      lightinfoUncondition( tex2D( lightTex, lightCoord ), lightColor.rgb, nlAttenuation, specularPower );
      lightColor.a = 1;

   #else

      // Why the 2x?  In the brightest areas the lightmap value is 2/3s of the
      // sunlight color and in the darkest areas its 1/2 of the ambient value.
      // So the 2x ensures that the ambient doesn't get darker than it should
      // at the expense of a little over brightening in the sunlight.
      float4 lightColor = float4( saturate( IN.ambient.rgb * 2 ), IN.ambient.a );

   #endif

   // Return the final color.
   return tex2D( diffuseMap, IN.texCoord ) * lightColor;
}
