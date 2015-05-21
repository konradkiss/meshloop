//-----------------------------------------------------------------------------
// Torque 3D
// Copyright (C) GarageGames.com, Inc.
//-----------------------------------------------------------------------------

#include "shadergen:/autogenConditioners.h"
#include "../torque.hlsl"

//-----------------------------------------------------------------------------
// Defines                                                                  
//-----------------------------------------------------------------------------

// miscParams
#define FRESNEL_BIAS       miscParams[0]
#define FRESNEL_POWER      miscParams[1]
// miscParams[2] is unused
#define ISRIVER            miscParams[3]

// reflectParams
#define REFLECT_PLANE_Z    reflectParams[0]
#define REFLECT_MIN_DIST   reflectParams[1]
#define REFLECT_MAX_DIST   reflectParams[2]
#define NO_REFLECT         reflectParams[3]

// fogParams
#define FOG_DENSITY        fogParams[0]
#define FOG_DENSITY_OFFSET fogParams[1]

// wetnessParams
#define WET_DEPTH          wetnessParams[0]
#define WET_COLOR_FACTOR   wetnessParams[1]

// distortionParams
#define DISTORT_START_DIST distortionParams[0]
#define DISTORT_END_DIST   distortionParams[1]
#define DISTORT_FULL_DEPTH distortionParams[2]

// foamParams
#define FOAM_SCALE         foamParams[0]
#define FOAM_MAX_DEPTH     foamParams[1]

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
   
   float4 foamTexCoords    : TEXCOORD8;
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
uniform sampler2D    prepassTex  : register( S1 );
uniform sampler2D    reflectMap  : register( S2 );
uniform sampler      refractBuff : register( S3 );
uniform samplerCUBE  skyMap      : register( S4 );
uniform sampler      foamMap     : register( S5 );
uniform float4       specularColor;
uniform float        specularPower;
uniform float4       baseColor;
uniform float4       miscParams;
uniform float2       fogParams;
uniform float4       reflectParams;
uniform float3       reflectNormal;
uniform float2       wetnessParams;
uniform float        farPlaneDist;
uniform float3       distortionParams;
uniform float2       foamParams;
uniform float3       foamColorMod;
uniform float3       ambientColor;
uniform float3       eyePos;
uniform float3       inLightVec;
uniform float3       fogData;
uniform float4       fogColor;
uniform float3       rippleMagnitude;
uniform float4       rtParams1;

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
   
   // Get depth of the water surface (this pixel).
   // Convert from WorldSpace to EyeSpace.
   float pixelDepth = IN.pixelDist / farPlaneDist; 
   
   float2 prepassCoord = viewportCoordToRenderTarget( IN.posPostWave, rtParams1 );

   float startDepth = prepassUncondition( prepassTex, prepassCoord ).w;  
   
   // The water depth in world units of the undistorted pixel.
   float startDelta = ( startDepth - pixelDepth );
   startDelta = max( startDelta, 0.0 );
   startDelta *= farPlaneDist;
            
   // Calculate the distortion amount for the water surface.
   // 
   // We subtract a little from it so that we don't 
   // distort where the water surface intersects the
   // camera near plane.
   float distortAmt = saturate( ( IN.pixelDist - DISTORT_START_DIST ) / DISTORT_END_DIST );
   
   // Scale down distortion in shallow water.
   distortAmt *= saturate( startDelta / DISTORT_FULL_DEPTH );

   // Do the intial distortion... we might remove it below.
   float2 distortDelta = bumpNorm.xy * distortAmt;
   float4 distortPos = IN.posPostWave;
   distortPos.xy += distortDelta;      
      
   prepassCoord = viewportCoordToRenderTarget( distortPos, rtParams1 );   

   // Get prepass depth at the position of this distorted pixel.
   float prepassDepth = prepassUncondition( prepassTex, prepassCoord ).w;      
    
   float delta = ( prepassDepth - pixelDepth ) * farPlaneDist;
      
   if ( delta < 0.0 )
   {
      // If we got a negative delta then the distorted
      // sample is above the water surface.  Mask it out
      // by removing the distortion.
      distortPos = IN.posPostWave;
      delta = startDelta;
      distortAmt = 0;
   } 
   else
   {
      float diff = ( prepassDepth - startDepth ) * farPlaneDist;
   
      if ( diff < 0 )
      {
         distortAmt = saturate( ( IN.pixelDist - DISTORT_START_DIST ) / DISTORT_END_DIST );
         distortAmt *= saturate( delta / DISTORT_FULL_DEPTH );

         distortDelta = bumpNorm.xy * distortAmt;
         
         distortPos = IN.posPostWave;         
         distortPos.xy += distortDelta;    
        
         prepassCoord = viewportCoordToRenderTarget( distortPos, rtParams1 );

         // Get prepass depth at the position of this distorted pixel.
         prepassDepth = prepassUncondition( prepassTex, prepassCoord ).w;
         delta = ( prepassDepth - pixelDepth ) * farPlaneDist;
      }
       
      if ( delta < 0.1 )
      {
         // If we got a negative delta then the distorted
         // sample is above the water surface.  Mask it out
         // by removing the distortion.
         distortPos = IN.posPostWave;
         delta = startDelta;
         distortAmt = 0;
      } 
   }
     
   float4 temp = IN.posPreWave;
   temp.xy += bumpNorm.xy * distortAmt;   
   float2 reflectCoord = viewportCoordToRenderTarget( temp, rtParams1 );     
   
   float2 refractCoord = viewportCoordToRenderTarget( distortPos, rtParams1 );
   
   // Color that replaces the reflection color when we do not
   // have one that is appropriate.
   float4 fakeColor = float4(ambientColor,1);
      
   float3 eyeVec = IN.worldPos - eyePos;
   float3 reflectionVec = reflect( eyeVec, bumpNorm );
   
   // Use fakeColor for ripple-normals that are angled towards the camera   
   eyeVec = -eyeVec;
   eyeVec = normalize( eyeVec );
   float ang = saturate( dot( eyeVec, bumpNorm ) );   
   float fakeColorAmt = ang;  
   
   // for verts far from the reflect plane z position
   float rplaneDist = abs( REFLECT_PLANE_Z - IN.worldPos.z );
   rplaneDist = saturate( ( rplaneDist - 1.0 ) / 2.0 );  
   rplaneDist *= ISRIVER;   
   fakeColorAmt = max( fakeColorAmt, rplaneDist );        
 
#ifndef UNDERWATER
   
   // Get foam color and amount
   IN.foamTexCoords.xy += distortDelta * 0.5; 
   IN.foamTexCoords.zw += distortDelta * 0.5;
   
   float4 foamColor = tex2D( foamMap, IN.foamTexCoords.xy );   
   foamColor += tex2D( foamMap, IN.foamTexCoords.zw ); 
   foamColor = saturate( foamColor );
   
   // Modulate foam color by ambient color
   // so we don't have glowing white foam at night.
   foamColor.rgb = lerp( foamColor.rgb, ambientColor.rgb, foamColorMod.rgb );
   
   float foamDelta = saturate( delta / FOAM_MAX_DEPTH );      
   float foamAmt = 1.0 - foamDelta;
   
   // Fade out the foam in very very low depth,
   // this improves the shoreline a lot.
   float diff = 0.8 - foamAmt;
   if ( diff < 0.0 )
   {
      foamAmt -= foamAmt * abs( diff ) / 0.2;
   }
   
   foamAmt *= FOAM_SCALE * foamColor.a;

   // Get reflection map color.
   float4 refMapColor = hdrDecode( tex2D( reflectMap, reflectCoord ) );   
   // If we do not have a reflection texture then we use the cubemap.
   refMapColor = lerp( refMapColor, texCUBE( skyMap, -reflectionVec ), NO_REFLECT );
   
   // Combine reflection color and fakeColor.
   float4 reflectColor = lerp( refMapColor, fakeColor, fakeColorAmt );
   
   // Get refract color
   float4 refractColor = hdrDecode( tex2D( refractBuff, refractCoord ) );    
   
   // We darken the refraction color a bit to make underwater 
   // elements look wet.  We fade out this darkening near the
   // surface in order to not have hard water edges.
   // @param WET_DEPTH The depth in world units at which full darkening will be recieved.
   // @param WET_COLOR_FACTOR The refract color is scaled down by this amount when at WET_DEPTH
   refractColor.rgb *= 1.0f - ( saturate( delta / WET_DEPTH ) * WET_COLOR_FACTOR );
   
   // Add Water fog/haze.
   float fogDelta = delta - FOG_DENSITY_OFFSET;

   if ( fogDelta < 0.0 )
      fogDelta = 0.0;     
   float fogAmt = 1.0 - saturate( exp( -FOG_DENSITY * fogDelta )  );
   
   // calc "diffuse" color by lerping from the water color
   // to refraction image based on the water clarity.
   float4 diffuseColor = lerp( refractColor, waterBaseColor, fogAmt );
   
   // fresnel calculation   
   float fresnelTerm = fresnel( ang, FRESNEL_BIAS, FRESNEL_POWER );	
   
   // Scale the frensel strength by fog amount
   // so that parts that are very clear get very little reflection.
   fresnelTerm *= fogAmt;    
   
   // Also scale the frensel by our distance to the
   // water surface.  This removes the hard reflection
   // when really close to the water surface.
   fresnelTerm *= saturate( IN.pixelDist - 0.1 );
   
   // Combine the diffuse color and reflection image via the
   // fresnel term and set out output color.
   float4 OUT = lerp( diffuseColor, reflectColor, fresnelTerm );
   
#else

   float4 refractColor = hdrDecode( tex2D( refractBuff, refractCoord ) );   
   float4 OUT = refractColor;  
   
#endif

#ifndef UNDERWATER

   OUT.rgb = lerp( OUT.rgb, foamColor.rgb, foamAmt );

   float factor = computeSceneFog( eyePos, 
                                   IN.fogPos, 
                                   IN.worldSpaceZ,
                                   fogData.x,
                                   fogData.y,
                                   fogData.z );

   OUT.rgb = lerp( OUT.rgb, fogColor.rgb, 1.0 - saturate( factor ) );  
   
#endif

   OUT.a = 1.0;
   
   return hdrEncode( OUT );
}
