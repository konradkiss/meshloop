//-----------------------------------------------------------------------------
// Torque 3D
// Copyright (C) GarageGames.com, Inc.
//-----------------------------------------------------------------------------

#include "shadergen:/autogenConditioners.h"

#include "farFrustumQuad.hlsl"
#include "lightingUtils.hlsl"
#include "../../lighting.hlsl"
#include "../shadowMap/shadowMapIO_HLSL.h"
#include "softShadow.hlsl"


struct ConvexConnectP
{
   float4 wsEyeDir : TEXCOORD0;
   float4 ssPos : TEXCOORD1;
   float4 vsEyeDir : TEXCOORD2;
};


float4 main(   ConvexConnectP IN,

               uniform sampler2D prePassBuffer : register(S0),
               uniform sampler2D shadowMap : register(S1),

               uniform float4 rtParams0,

               uniform float3 lightPosition,
               uniform float4 lightColor,
               uniform float  lightBrightness,
               uniform float  lightRange,
               uniform float2 lightAttenuation,
               uniform float3 lightDirection,
               uniform float4 lightSpotParams,
               uniform float4 lightMapParams,

               uniform float4 vsFarPlane,
               uniform float4x4 viewToLightProj,

               uniform float4 lightParams,
               uniform float shadowSoftness,
               uniform float constantSpecularPower ) : COLOR0
{   
   // Compute scene UV
   float3 ssPos = IN.ssPos.xyz / IN.ssPos.w;
   float2 uvScene = getUVFromSSPos( ssPos, rtParams0 );
   
   // Sample/unpack the normal/z data
   float4 prepassSample = prepassUncondition( prePassBuffer, uvScene );
   float3 normal = prepassSample.rgb;
   float depth = prepassSample.a;
   
   // Eye ray - Eye -> Pixel
   float3 eyeRay = getDistanceVectorToPlane( -vsFarPlane.w, IN.vsEyeDir.xyz, vsFarPlane );
   float3 viewSpacePos = eyeRay * depth;
      
   // Build light vec, get length, clip pixel if needed
   float3 lightToPxlVec = viewSpacePos - lightPosition;
   float lenLightV = length( lightToPxlVec );
   lightToPxlVec /= lenLightV;

   //lightDirection = float3( -lightDirection.xy, lightDirection.z ); //float3( 0, 0, -1 );
   float cosAlpha = dot( lightDirection, lightToPxlVec );   
   clip( cosAlpha - lightSpotParams.x );
   clip( lightRange - lenLightV );

   float atten = attenuate( lightColor, lightAttenuation, lenLightV );
   atten *= ( cosAlpha - lightSpotParams.x ) / lightSpotParams.y;
   clip( atten - 1e-6 );
   atten = saturate( atten );
   
   float nDotL = dot( normal, -lightToPxlVec );

   #ifdef NO_SHADOW
   
      float shadowed = 1.0;
      	
   #else

      // Find Shadow coordinate
      float4 pxlPosLightProj = mul( viewToLightProj, float4( viewSpacePos, 1 ) );
      float2 shadowCoord = ( ( pxlPosLightProj.xy / pxlPosLightProj.w ) * 0.5 ) + float2( 0.5, 0.5 );
      shadowCoord.y = 1.0f - shadowCoord.y;

      // Get a linear depth from the light source.
      float distToLight = pxlPosLightProj.z / lightRange;

      float shadowed = softShadow_filter( shadowMap,
                                          ssPos.xy,
                                          shadowCoord,
                                          shadowSoftness,
                                          distToLight,
                                          nDotL,
                                          lightParams.y );

   #endif // !NO_SHADOW
   
   // NOTE: Do not clip on fully shadowed pixels as it would
   // cause the hardware occlusion query to disable the shadow.

   // Specular term
   float specular = calcSpecular(  -lightToPxlVec, 
                                    normal, 
                                    normalize( -eyeRay ), 
                                    constantSpecularPower ) * lightColor.a;

   float Sat_NL_Att = saturate( nDotL * atten * shadowed ) * lightBrightness;
   float3 lightColorOut = lightMapParams.rgb * lightColor.rgb;
   float4 addToResult = 0.0;

   // TODO: This needs to be removed when lightmapping is disabled
   // as its extra work per-pixel on dynamic lit scenes.
   //
   // Special lightmapping pass.
   if ( lightMapParams.a < 0.0 )
   {
      // This disables shadows on the backsides of objects.
      shadowed = nDotL < 0.0f ? 1.0f : shadowed;

      Sat_NL_Att = 1.0f;
      lightColorOut = shadowed;
      addToResult = ( 1.0 - shadowed ) * abs(lightMapParams);
   }

   return lightinfoCondition( lightColorOut, Sat_NL_Att, specular, addToResult );
}
