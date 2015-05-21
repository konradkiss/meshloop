//-----------------------------------------------------------------------------
// Torque 3D
// Copyright (C) GarageGames.com, Inc.
//-----------------------------------------------------------------------------

#include "shadergen:/autogenConditioners.h"

#include "farFrustumQuad.hlsl"
#include "../../torque.hlsl"
#include "../../lighting.hlsl"
#include "lightingUtils.hlsl"
#include "../shadowMap/shadowMapIO_HLSL.h"
#include "softShadow.hlsl"

//#define PSSM_DEBUG

uniform sampler2D ShadowMap : register(S1);
uniform sampler2D ssaoMask : register(S2);
uniform float4 rtParams2;

float4 main( FarFrustumQuadConnectP IN,

             uniform sampler2D prePassBuffer : register(S0),
             
             uniform float3 lightDirection,
             uniform float4 lightColor,
             uniform float  lightBrightness,
             uniform float4 lightAmbient,
             uniform float4 lightTrilight,
             
             uniform float3 eyePosWorld,
             
             uniform float4x4 worldToLightProj,
             uniform float4 splitDistStart,
             uniform float4 splitDistEnd,
             uniform float4 scaleX,
             uniform float4 scaleY,
             uniform float4 offsetX,
             uniform float4 offsetY,
             uniform float4 atlasXOffset,
             uniform float4 atlasYOffset,
             uniform float2 atlasScale,
             uniform float4 zNearFarInvNearFar,
             uniform float4 lightMapParams,

             uniform float constantSpecularPower,
             uniform float2 fadeStartLength,
             uniform float4 farPlaneScalePSSM,
             uniform float4 splitFade,
             uniform float4 overDarkPSSM,
             uniform float shadowSoftness ) : COLOR0
{
   // Sample/unpack the normal/z data
   float4 prepassSample = prepassUncondition( prePassBuffer, IN.uv0 );
   float3 normal = prepassSample.rgb;
   float depth = prepassSample.a;

   // Use eye ray to get ws pos
   float4 worldPos = float4(eyePosWorld + IN.wsEyeRay * depth, 1.0f);

   // Get the light attenuation.
   float dotNL = dot(-lightDirection, normal);

   #ifdef PSSM_DEBUG
      float3 debugColor = 0;
   #endif
   
   #ifdef NO_SHADOW

      // Fully unshadowed.
      float shadowed = 1.0;
      float ao = 1.0;

      #ifdef PSSM_DEBUG
         debugColor = 1.0;
      #endif

   #else

      // Compute shadow map coordinate
      float4 pxlPosLightProj = mul(worldToLightProj, worldPos);
      float2 baseShadowCoord = pxlPosLightProj.xy / pxlPosLightProj.w;   

      // Distance to light, in shadowmap space
      float distToLight = pxlPosLightProj.z / pxlPosLightProj.w;
         
      // Figure out which split to sample from.  Basically, we compute the shadowmap sample coord
      // for all of the splits and then check if its valid.  
      float4 finalMask = float4(0, 0, 0, 1);
      float4 shadowCoordX = baseShadowCoord.xxxx;
      float4 shadowCoordY = baseShadowCoord.yyyy;
      float4 farPlaneDists = distToLight.xxxx;      
      shadowCoordX *= scaleX;
      shadowCoordY *= scaleY;
      shadowCoordX += offsetX;
      shadowCoordY += offsetY;
      farPlaneDists *= farPlaneScalePSSM;
      
      // If the shadow sample is within -1..1 and the distance to the light for this pixel is less than the far plane of the
      // split, use it.
      if (shadowCoordX.x > -0.99 && shadowCoordX.x < 0.99 && shadowCoordY.x > -0.99 && shadowCoordY.x < 0.99 && farPlaneDists.x < 1.0)
      {
         finalMask = float4(1, 0, 0, 0);
      }
      else if (shadowCoordX.y > -0.99 && shadowCoordX.y < 0.99 && shadowCoordY.y > -0.99 && shadowCoordY.y < 0.99 && farPlaneDists.y < 1.0)
      {
         finalMask = float4(0, 1, 0, 0);
      }         
      else if (shadowCoordX.z > -0.99 && shadowCoordX.z < 0.99 && shadowCoordY.z > -0.99 && shadowCoordY.z < 0.99 && farPlaneDists.z < 1.0)
      {
         finalMask = float4(0, 0, 1, 0);
      } else {
         finalMask = float4(0, 0, 0, 1);
      }

      #ifdef PSSM_DEBUG
         if ( finalMask.x > 0 )
            debugColor += float4( 1, 0, 0, 1 );
         else if ( finalMask.y > 0 )
            debugColor += float4( 0, 1, 0, 1 );
         else if ( finalMask.z > 0 )
            debugColor += float4( 0, 0, 1, 1 );
         else if ( finalMask.w > 0 )
            debugColor += float4( 1, 1, 0, 1 );
      #endif

      // Here we know what split we're sampling from, so recompute the texcoord location
      // Yes, we could just use the result from above, but doing it this way actually saves
      // shader instructions.
      float2 finalScale;
      finalScale.x = dot(finalMask, scaleX);
      finalScale.y = dot(finalMask, scaleY);

      float2 finalOffset;
      finalOffset.x = dot(finalMask, offsetX);
      finalOffset.y = dot(finalMask, offsetY);

      float2 shadowCoord;                  
      shadowCoord = baseShadowCoord * finalScale;      
      shadowCoord += finalOffset;

      // Convert to texcoord space
      shadowCoord = 0.5 * shadowCoord + float2(0.5, 0.5);
      shadowCoord.y = 1.0f - shadowCoord.y;

      // Move around inside of atlas 
      float2 aOffset;
      aOffset.x = dot(finalMask, atlasXOffset);
      aOffset.y = dot(finalMask, atlasYOffset);

      shadowCoord *= atlasScale;
      shadowCoord += aOffset;
              
      // Each split has a different far plane, take this into account.
      float farPlaneScale = dot( farPlaneScalePSSM, finalMask );
      distToLight *= farPlaneScale;
      
      float shadowed = softShadow_filter(   ShadowMap,
                                             IN.uv0.xy,
                                             shadowCoord,
                                             farPlaneScale * shadowSoftness,
                                             distToLight,
                                             dotNL,
                                             dot( finalMask, overDarkPSSM ) );
  
      // Fade out the shadow at the end of the range.
      float4 zDist = (zNearFarInvNearFar.x + zNearFarInvNearFar.y * depth);
      float fadeOutAmt = ( zDist.x - fadeStartLength.x ) * fadeStartLength.y;
      shadowed = lerp( shadowed, 1.0, saturate( fadeOutAmt ) );

      #ifdef PSSM_DEBUG
         if ( fadeOutAmt > 1.0 )
            debugColor = 1.0;
      #endif

      // Sample the AO texture.      
      float ao = 1.0 - tex2D( ssaoMask, viewportCoordToRenderTarget( IN.uv0.xy, rtParams2 ) ).r;

   #endif // !NO_SHADOW

   // Specular term
   float specular = calcSpecular(   -lightDirection, 
                                    normal, 
                                    normalize(-IN.vsEyeRay),
                                    constantSpecularPower ) * lightColor.a;
                                    
   float Sat_NL_Att = saturate( dotNL * shadowed ) * lightBrightness;
   float3 lightColorOut = lightMapParams.rgb * lightColor.rgb;
   float4 addToResult = lightAmbient;

   // TODO: This needs to be removed when lightmapping is disabled
   // as its extra work per-pixel on dynamic lit scenes.
   //
   // Special lightmapping pass.
   if ( lightMapParams.a < 0.0 )
   {
      // This disables shadows on the backsides of objects.
      shadowed = dotNL < 0.0f ? 1.0f : shadowed;

      Sat_NL_Att = 1.0f;
      lightColorOut = shadowed;
      addToResult = ( 1.0 - shadowed ) * abs(lightMapParams);
   }

   addToResult *= ao;

   #ifdef PSSM_DEBUG
      lightColorOut = debugColor;
   #endif

   return lightinfoCondition( lightColorOut, Sat_NL_Att, specular, addToResult );  
}
