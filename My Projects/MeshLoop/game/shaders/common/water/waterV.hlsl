//-----------------------------------------------------------------------------
// Torque 3D
// Copyright (C) GarageGames.com, Inc.
//-----------------------------------------------------------------------------

#include "shadergen:/autogenConditioners.h"

//-----------------------------------------------------------------------------
// Structures                                                                  
//-----------------------------------------------------------------------------
struct VertData
{
   float4 position         : POSITION;
   float3 normal           : NORMAL;
   float2 undulateData     : TEXCOORD0;
   float4 horizonFactor    : TEXCOORD1;
};

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
// Uniforms                                                                  
//-----------------------------------------------------------------------------
uniform float3x3 modelMat;
uniform float4x4 modelview;
uniform float3   eyePos;       
uniform float2   waveDir[3];
uniform float2   waveData[3];
uniform float2   rippleDir[3];
uniform float2   rippleTexScale[3];                    
uniform float3   rippleSpeed;
uniform float3   inLightVec;
uniform float3   reflectNormal;
uniform float    gridElementSize;
uniform float    elapsedTime;
uniform float    undulateMaxDist;

//-----------------------------------------------------------------------------
// Main                                                                        
//-----------------------------------------------------------------------------
ConnectData main( VertData IN )
{	
   ConnectData OUT;  
   
   // use projection matrix for reflection / refraction texture coords
   float4x4 texGen = { 0.5,  0.0,  0.0,  0.5,
                       0.0, -0.5,  0.0,  0.5,
                       0.0,  0.0,  1.0,  0.0,
                       0.0,  0.0,  0.0,  1.0 };

   // Move the vertex based on the horizonFactor if specified to do so for this vert.
   if ( IN.horizonFactor.z > 0 )
   {
      float2 offsetXY = eyePos.xy - eyePos.xy % gridElementSize;         
      IN.position.xy += offsetXY;
      IN.undulateData += offsetXY;
   }      
      
   OUT.fogPos = IN.position.xyz;
   IN.position.z = lerp( IN.position.z, eyePos.z, IN.horizonFactor.x );
   
   // Send pre-undulation screenspace position
   OUT.posPreWave = mul( modelview, IN.position );
   OUT.posPreWave = mul( texGen, OUT.posPreWave );
      
   // Calculate the undulation amount for this vertex.   
   float2 undulatePos = IN.undulateData; 
   float undulateAmt = 0.0;

   undulateAmt += waveData[0].y * sin( elapsedTime * waveData[0].x + 
                                       undulatePos.x * waveDir[0].x +
                                       undulatePos.y * waveDir[0].y );
   undulateAmt += waveData[1].y * sin( elapsedTime * waveData[1].x + 
                                       undulatePos.x * waveDir[1].x +
                                       undulatePos.y * waveDir[1].y );
   undulateAmt += waveData[2].y * sin( elapsedTime * waveData[2].x + 
                                       undulatePos.x * waveDir[2].x +
                                       undulatePos.y * waveDir[2].y );  
   
   // Scale down wave magnitude amount based on distance from the camera.   
   float dist = distance( IN.position.xyz, eyePos );
   dist = clamp( dist, 1.0, undulateMaxDist );          
   undulateAmt *= ( 1 - dist / undulateMaxDist ); 
   
   // Also scale down wave magnitude if the camera is very very close.
   undulateAmt *= saturate( ( distance( IN.position.xyz, eyePos ) - 0.5 ) / 10.0 );
   
   // Apply wave undulation to the vertex.
   OUT.posPostWave = IN.position;
   OUT.posPostWave.xyz += IN.normal.xyz * undulateAmt;   
   
   // Save worldSpace position of this pixel/vert
   OUT.worldPos = OUT.posPostWave.xyz;   
   
   // Convert to screen 
   OUT.posPostWave = mul( modelview, OUT.posPostWave );
   
   // Setup the OUT position symantic variable
   OUT.hpos = OUT.posPostWave; 
   OUT.hpos.z = lerp( OUT.hpos.z, OUT.hpos.w, IN.horizonFactor.x );
   
   OUT.worldSpaceZ = mul( modelMat, OUT.fogPos ).z;
   if ( IN.horizonFactor.x > 0 )
   {
      float3 awayVec = normalize( OUT.fogPos.xyz - eyePos );
      OUT.fogPos.xy += awayVec.xy * 1000.0;
   }
   
   // Save world space camera dist/depth of the outgoing pixel
   OUT.pixelDist = OUT.hpos.z;              

   // Convert to reflection texture space   
   OUT.posPostWave = mul( texGen, OUT.posPostWave );
              
   float2 ripplePos = IN.undulateData;     
   float2 txPos = normalize( ripplePos );
   txPos *= 50000.0;   
   ripplePos = lerp( ripplePos, txPos, IN.horizonFactor.x );
      
   // set up tex coordinates for the 3 interacting normal maps   
   OUT.rippleTexCoord01.xy = lerp( ripplePos * rippleTexScale[0], txPos.xy * rippleTexScale[0], IN.horizonFactor.x );
   OUT.rippleTexCoord01.xy += rippleDir[0] * elapsedTime * rippleSpeed.x;

   OUT.rippleTexCoord01.zw = lerp( ripplePos * rippleTexScale[1], txPos.xy * rippleTexScale[1], IN.horizonFactor.x );
   OUT.rippleTexCoord01.zw += rippleDir[1] * elapsedTime * rippleSpeed.y;

   OUT.rippleTexCoord2.xy = lerp( ripplePos * rippleTexScale[2], txPos.xy * rippleTexScale[2], IN.horizonFactor.x );
   OUT.rippleTexCoord2.xy += rippleDir[2] * elapsedTime * rippleSpeed.z; 

   OUT.foamTexCoords.xy = lerp( ripplePos * 0.2, txPos.xy * rippleTexScale[0], IN.horizonFactor.x );   
   OUT.foamTexCoords.xy += rippleDir[0] * sin( ( elapsedTime + 500.0 ) * -0.4 ) * 0.15;

   OUT.foamTexCoords.zw = lerp( ripplePos * 0.3, txPos.xy * rippleTexScale[1], IN.horizonFactor.x );   
   OUT.foamTexCoords.zw += rippleDir[1] * sin( elapsedTime * 0.4 ) * 0.15;
   
   return OUT;
}

