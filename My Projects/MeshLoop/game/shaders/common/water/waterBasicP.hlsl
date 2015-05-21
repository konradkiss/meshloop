//-----------------------------------------------------------------------------
// Torque 3D
// Copyright (C) GarageGames.com, Inc.
//-----------------------------------------------------------------------------

#include "../torque.hlsl"

//-----------------------------------------------------------------------------
// Defines                                                                  
//-----------------------------------------------------------------------------

// miscParams
#define FRESNEL_BIAS       miscParams[0]
#define FRESNEL_POWER      miscParams[1]
#define CLARITY            miscParams[2]
#define ISRIVER           miscParams[3]

// reflectParams
#define REFLECT_PLANE_Z    reflectParams[0]
#define REFLECT_MIN_DIST   reflectParams[1]
#define REFLECT_MAX_DIST   reflectParams[2]
#define NO_REFLECT         reflectParams[3]

// distortionParams
#define DISTORT_START_DIST distortionParams[0]
#define DISTORT_END_DIST   distortionParams[1]
#define DISTORT_FULL_DEPTH distortionParams[2]

//-----------------------------------------------------------------------------
// Structures                                                                  
//-----------------------------------------------------------------------------

struct ConnectData
{
   float4 hpos             : POSITION;
   
   // Worldspace position of this pixel
   float3 worldPos         : TEXCOORD0;
   
   // TexCoord 0 and 1 (xy,zw) for ripple texture lookup
   float4 rippleTexCoord01 : TEXCOORD1;   
   
   // TexCoord 2 for ripple texture lookup
   float2 rippleTexCoord2  : TEXCOORD2;
   
   // Screenspace vert position BEFORE wave transformation
   float4 posPreWave       : TEXCOORD3;
   
   // Screenspace vert position AFTER wave transformation
   float4 posPostWave      : TEXCOORD4;   
   
   // Worldspace unit distance/depth of this vertex/pixel
   float  pixelDist        : TEXCOORD5;   
   
   float3 fogPos           : TEXCOORD6;
   
   float worldSpaceZ       : TEXCOORD7;
};

//-----------------------------------------------------------------------------
// approximate Fresnel function
//-----------------------------------------------------------------------------
float fresnel(float NdotV, float bias, float power)
{
   return bias + (1.0-bias)*pow(abs(1.0 - max(NdotV, 0)), power);
}

//-----------------------------------------------------------------------------
// Uniforms                                                                  
//-----------------------------------------------------------------------------
uniform sampler      bumpMap     : register( S0 );
//uniform sampler2D    prepassTex  : register( S1 );
uniform sampler2D    reflectMap  : register( S2 );
uniform sampler      refractBuff : register( S3 );
uniform samplerCUBE  skyMap      : register( S4 );
//uniform sampler      foamMap     : register( S5 );
uniform float4       baseColor;
uniform float4       miscParams;
uniform float4       reflectParams;
uniform float3       ambientColor;
uniform float3       eyePos;
uniform float3       distortionParams;
uniform float3       fogData;
uniform float4       fogColor;
uniform float3       rippleMagnitude;

//-----------------------------------------------------------------------------
// Main                                                                        
//-----------------------------------------------------------------------------
float4 main( ConnectData IN ) : COLOR
{ 
   // Modulate baseColor by the ambientColor.
   float4 waterBaseColor = baseColor * float4( ambientColor.rgb, 1 );
   
   // Get the bumpNorm...
   float3 bumpNorm = ( tex2D( bumpMap, IN.rippleTexCoord01.xy ).rgb * 2.0 - 1.0 ) * rippleMagnitude.x;
   bumpNorm       += ( tex2D( bumpMap, IN.rippleTexCoord01.zw ).rgb * 2.0 - 1.0 ) * rippleMagnitude.y;      
   bumpNorm       += ( tex2D( bumpMap, IN.rippleTexCoord2 ).rgb * 2.0 - 1.0 ) * rippleMagnitude.z;  
   
   //return float4( bumpNorm.xyz, 1 );
   
   // We subtract a little from it so that we don't 
   // distort where the water surface intersects the
   // camera near plane.
   float distortAmt = saturate( IN.pixelDist / 1.0 ) * 0.8;
      
   float4 distortPos = IN.posPostWave;
   distortPos.xy += bumpNorm.xy * distortAmt;   
 
 #ifdef UNDERWATER
   return hdrEncode( tex2Dproj( refractBuff, distortPos ) );   
 #else

   float3 eyeVec = IN.worldPos - eyePos;
   float3 reflectionVec = reflect( eyeVec, normalize(bumpNorm) ); 

   // Color that replaces the reflection color when we do not
   // have one that is appropriate.
   float4 fakeColor = float4(ambientColor,1);
   
   // Use fakeColor for ripple-normals that are angled towards the camera  
   eyeVec = -eyeVec;
   eyeVec = normalize( eyeVec );
   float ang = saturate( dot( eyeVec, bumpNorm ) );   
   float fakeColorAmt = ang;   
      
    // Get reflection map color
   float4 refMapColor = hdrDecode( tex2Dproj( reflectMap, distortPos ) ); 
   // If we do not have a reflection texture then we use the cubemap.
   refMapColor = lerp( refMapColor, texCUBE( skyMap, -reflectionVec ), NO_REFLECT );      
   
   // Combine reflection color and fakeColor.
   float4 reflectColor = lerp( refMapColor, fakeColor, fakeColorAmt );
   //return refMapColor;
   
   // Get refract color
   float4 refractColor = hdrDecode( tex2Dproj( refractBuff, distortPos ) );   
   
   // calc "diffuse" color by lerping from the water color
   // to refraction image based on the water clarity.
   float4 diffuseColor = lerp( refractColor, waterBaseColor, 1.0f - CLARITY );   
   
   // fresnel calculation 
   float fresnelTerm = fresnel( ang, FRESNEL_BIAS, FRESNEL_POWER );	
   //return float4( fresnelTerm.rrr, 1 );
   
   // Also scale the frensel by our distance to the
   // water surface.  This removes the hard reflection
   // when really close to the water surface.
   fresnelTerm *= saturate( IN.pixelDist - 0.1 );
   
   // Combine the diffuse color and reflection image via the
   // fresnel term and set out output color.
   float4 OUT = lerp( diffuseColor, reflectColor, fresnelTerm );  
   
   // Fog it.   
   float factor = computeSceneFog( eyePos, 
                                   IN.fogPos, 
                                   IN.worldSpaceZ,
                                   fogData.x,
                                   fogData.y,
                                   fogData.z );

   OUT.rgb = lerp( OUT.rgb, fogColor.rgb, 1.0 - saturate( factor ) );   
   
   return hdrEncode( OUT );
#endif   
}
