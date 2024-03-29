
#include "shadergen:/autogenConditioners.h"
#include "./../postFx.hlsl"

#define DOSMALL
#define DOLARGE

uniform sampler2D prepassMap : register(S0);
uniform sampler2D randNormalTex : register(S1);
uniform sampler1D powTable : register(S2);

uniform float2 nearFar;
uniform float2 worldToScreenScale;
uniform float2 texSize0;
uniform float2 texSize1;
uniform float2 targetSize;

// Script-set constants.

uniform float overallStrength;

uniform float sRadius;
uniform float sStrength;
uniform float sDepthMin;
uniform float sDepthMax;
uniform float sDepthPow;
uniform float sNormalTol;
uniform float sNormalPow;

uniform float lRadius;
uniform float lStrength;
uniform float lDepthMin;
uniform float lDepthMax;
uniform float lDepthPow;
uniform float lNormalTol;
uniform float lNormalPow;


#ifndef QUALITY
   #define QUALITY 2
#endif


#if QUALITY == 0
   #define sSampleCount 4
   #define totalSampleCount 12
#elif QUALITY == 1
   #define sSampleCount 6
   #define totalSampleCount 24
#elif QUALITY == 2
   #define sSampleCount 8
   #define totalSampleCount 32
#endif


float getOcclusion( float depthDiff, float depthMin, float depthMax, float depthPow, 
                    float normalDiff, float dt, float normalTol, float normalPow )
{
   if ( depthDiff < 0.0 )
      return 0.0;
      
   float delta = abs( depthDiff );
   
   if ( delta < depthMin || delta > depthMax )
      return 0.0;   
      
   delta = saturate( delta / depthMax );
   
   if ( dt > 0.0 )
      normalDiff *= dt;
   else
      normalDiff = 1.0;
      
      
   normalDiff *= 1.0 - ( dt * 0.5 + 0.5 );
        
   return ( 1.0 - tex1D( powTable, delta ) ) * normalDiff;
}


float4 main( PFXVertToPix IN ) : COLOR
{          
   float3 ptSphere[32] =
   {
   	float3( 0.295184, 0.077723, 0.068429 ),
   	float3( -0.271976, -0.365221, -0.838363 ),
   	float3( 0.547713, 0.467576, 0.488515 ),
   	float3( 0.662808, -0.031733, -0.584758 ),
   	float3( -0.025717, 0.218955, -0.657094 ),
   	float3( -0.310153, -0.365223, -0.370701 ),
   	float3( -0.101407, -0.006313, -0.747665 ),
   	float3( -0.769138, 0.360399, -0.086847 ),
   	float3( -0.271988, -0.275140, -0.905353 ),
   	float3( 0.096740, -0.566901, 0.700151 ),
   	float3( 0.562872, -0.735136, -0.094647 ),
   	float3( 0.379877, 0.359278, 0.190061 ),
   	float3( 0.519064, -0.023055, 0.405068 ),
   	float3( -0.301036, 0.114696, -0.088885 ),
   	float3( -0.282922, 0.598305, 0.487214 ),
   	float3( -0.181859, 0.251670, -0.679702 ),
   	float3( -0.191463, -0.635818, -0.512919 ),
   	float3( -0.293655, 0.427423, 0.078921 ),
   	float3( -0.267983, 0.680534, -0.132880 ),
   	float3( 0.139611, 0.319637, 0.477439 ),
   	float3( -0.352086, 0.311040, 0.653913 ),
   	float3( 0.321032, 0.805279, 0.487345 ),
   	float3( 0.073516, 0.820734, -0.414183 ),
   	float3( -0.155324, 0.589983, -0.411460 ),
   	float3( 0.335976, 0.170782, -0.527627 ),
   	float3( 0.463460, -0.355658, -0.167689 ),
   	float3( 0.222654, 0.596550, -0.769406 ),
   	float3( 0.922138, -0.042070, 0.147555 ),
   	float3( -0.727050, -0.329192, 0.369826 ),
   	float3( -0.090731, 0.533820, 0.463767 ),
   	float3( -0.323457, -0.876559, -0.238524 ),
   	float3( -0.663277, -0.372384, -0.342856 )
   };
   
   // Sample a random normal for reflecting the 
   // sphere vector later in our loop.   
   float4 noiseMapUV = float4( ( IN.uv1 * ( targetSize / texSize1 ) ).xy, 0, 0 );
   float3 reflectNormal = normalize( tex2Dlod( randNormalTex, noiseMapUV ).xyz * 2.0 - 1.0 );   
   //return float4( reflectNormal, 1 );
   
   float4 prepass = prepassUncondition( prepassMap, IN.uv0 );
   float3 normal = prepass.xyz;
   float depth = prepass.a;
   //return float4( ( depth ).xxx, 1 );
      
   // Early out if too far away.
   if ( depth > 0.99999999 )
      return float4( 1,1,1,1 );      

   // current fragment coords in screen space
   float3 ep = float3( IN.uv0, depth );        
   
   float bl;
   float3 baseRay, ray, se, occNorm, projRadius, normalDiff;
   normalDiff = float3( 0,0,0 );
   float depthMin, depthMax, dt, depthDiff;    
   float4 occluderFragment;
   int i;
   float sOcclusion = 0.0;
   float lOcclusion = 0.0;
   
   //------------------------------------------------------------
   // Small radius
   //------------------------------------------------------------   

#ifdef DOSMALL

   bl = 0.0;
   
   projRadius.xy =  ( float2( sRadius.rr ) / ( depth * nearFar.y ) ) * ( worldToScreenScale / texSize0 );
   projRadius.z = sRadius / nearFar.y;
   
   depthMin = projRadius.z * sDepthMin;
   depthMax = projRadius.z * sDepthMax;
   
   //float maxr = 1;
   //radiusDepth = clamp( radiusDepth, 0.0001, maxr.rrr );   
   //if ( radiusDepth.x < 1.0 / targetSize.x )
   //   return color;      
   //radiusDepth.xyz = 0.0009;
   
   for ( i = 0; i < sSampleCount; i++ )
   {
      baseRay = reflect( ptSphere[i], reflectNormal );
      
      dt = dot( baseRay.xyz, normal );
      
      baseRay *= sign( dt );
         
      ray = ( projRadius * baseRay.xzy );
      ray.y = -ray.y;      
       
      se = ep + ray;
            
      occluderFragment = prepassUncondition( prepassMap, se.xy );                  
      
      depthDiff = se.z - occluderFragment.a; 
      
      dt = dot( occluderFragment.xyz, baseRay.xyz );
      normalDiff = dot( occluderFragment.xyz, normal );        
      
      bl += getOcclusion( depthDiff, depthMin, depthMax, sDepthPow, normalDiff, dt, sNormalTol, sNormalPow );         
   }
   
   sOcclusion = sStrength * ( bl / (float)sSampleCount );

#endif // DOSMALL
   
   
   //------------------------------------------------------------
   // Large radius
   //------------------------------------------------------------
   
#ifdef DOLARGE
      
   bl = 0.0;

   projRadius.xy =  ( float2( lRadius.rr ) / ( depth * nearFar.y ) ) * ( worldToScreenScale / texSize0 );
   projRadius.z = lRadius / nearFar.y;
   
   depthMin = projRadius.z * lDepthMin;
   depthMax = projRadius.z * lDepthMax;
   
   //projRadius.xy = clamp( projRadius.xy, 0.0, 0.01 );
   //float maxr = 1;
   //radiusDepth = clamp( radiusDepth, 0.0001, maxr.rrr );   
   //if ( radiusDepth.x < 1.0 / targetSize.x )
   //   return color;      
   //radiusDepth.xyz = 0.0009;   
   
   for ( i = sSampleCount; i < totalSampleCount; i++ )
   {
      baseRay = reflect( ptSphere[i], reflectNormal );
      
      dt = dot( baseRay.xyz, normal );
      
      baseRay *= sign( dt );
         
      ray = ( projRadius * baseRay.xzy );
      ray.y = -ray.y;      
       
      se = ep + ray;
            
      occluderFragment = prepassUncondition( prepassMap, se.xy );                  
      
      depthDiff = se.z - occluderFragment.a;       
      
      normalDiff = dot( occluderFragment.xyz, normal );        
      dt = dot( occluderFragment.xyz, baseRay.xyz );         
               
      bl += getOcclusion( depthDiff, depthMin, depthMax, lDepthPow, normalDiff, dt, lNormalTol, lNormalPow );        
   }
      
   lOcclusion = lStrength * ( bl / (float)( totalSampleCount - sSampleCount ) );

#endif // DOLARGE
   
   float occlusion = saturate( max( sOcclusion, lOcclusion ) * overallStrength );   
   
   // Note black is unoccluded and white is fully occluded.  This
   // seems backwards, but it makes it simple to deal with the SSAO
   // being disabled in the lighting shaders.   
   
   return occlusion;      
}


