//-----------------------------------------------------------------------------
// Torque 3D
// Copyright (C) GarageGames.com, Inc.
//-----------------------------------------------------------------------------

#include "shadergen:/autogenConditioners.h"


struct Conn
{
	float4 position      : POSITION;
	float2 texCoord      : TEXCOORD0;
   float2 bumpCoord     : TEXCOORD1;

   #ifdef TORQUE_ADVANCED_LIGHTING
   
      #ifdef TORQUE_PREPASS

         float4 wsEyeVec : TEXCOORD2;

         float3x3 worldToTangent : TEXCOORD4;

      #else

         float4 lightCoord : TEXCOORD2;

      #endif

   #else

      float3 lightVec : TEXCOORD2;

   #endif

   float fade : TEXCOORD3;

};


float4 main(   Conn IN,

               uniform sampler2D diffuseMap : register(S0),
               uniform sampler2D bumpMap : register(S1),

               #ifdef TORQUE_ADVANCED_LIGHTING

                  #ifdef TORQUE_PREPASS

                     uniform float3 vEye : register(C0)

                  #else

                     uniform sampler2D lightTex : register(S2),
                     uniform float4 lightTexRT : register(C0)                           
                  
                  #endif

               #else

                  uniform float4 lightColor : register(C0),
                  uniform float4 ambient : register(C1) 
               
               #endif

               ) : COLOR
{
   // Fetch the unlit imposter texel.
   float4 Out = tex2D( diffuseMap, IN.texCoord );

   #ifdef TORQUE_ADVANCED_LIGHTING

      // We have to clip ourselves when doing advanced lighting.
      float alphaTest = 84.0f / 255.0f;
      clip( ( Out.a * IN.fade ) - alphaTest );

      #ifdef TORQUE_PREPASS

         float4 bumpNormal = tex2D( bumpMap, IN.texCoord );
         bumpNormal.xyz = bumpNormal.xyz * 2.0 - 1.0;
         float3 gbNormal = -mul( bumpNormal, IN.worldToTangent );
         
         float eyeSpaceDepth = dot(vEye, (IN.wsEyeVec.xyz / IN.wsEyeVec.w));

         // Return the encoded normal and depth.
         return prepassCondition( float4( gbNormal, eyeSpaceDepth ) );

      #else

         // Get the lighting at this screen position.
         float2 lightCoord = IN.lightCoord.xy / IN.lightCoord.w;
         lightCoord = ( lightCoord + 1.0 ) / 2.0;
         lightCoord = ( lightCoord * lightTexRT.zw ) + lightTexRT.xy;
         lightCoord.y = 1.0 - lightCoord.y; 

         float3 lightColor;
         float nlAttenuation;
         float specularPower;
         lightinfoUncondition( tex2D( lightTex, lightCoord ), lightColor, nlAttenuation, specularPower );

         Out.rgb *= lightColor.rgb;

      #endif

   #else

      // Decompress the object space bump normal.
      float3 bumpNormal = ( tex2D( bumpMap, IN.bumpCoord ) * 2 ) - 1;

      // Should we be renormalizing?
      //bumpNormal = normalize( bumpNormal );

      // Remember that the light vector was transformed into 
      // object space in the vertex shader.  We can now dot 
      // it to get the correct directional light intensity.
      float3 bumpDot = saturate( dot( bumpNormal, IN.lightVec ) );

      // We can now apply the standard lighting calculation.
      Out.rgb *= ambient.xyz + ( bumpDot * lightColor.xyz );

   #endif

   // Add distance fog.
   //float4 fogColor = tex2D( fogMap, In.fogCoord.xy );
   //Out.col.rgb = lerp( Out.col.rgb, fogColor.rgb, fogColor.a );

   // Finally apply the alpha fade value.
   Out.a *= IN.fade;

   return Out;
}
