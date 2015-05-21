//-----------------------------------------------------------------------------
// Torque 3D
// Copyright (C) GarageGames.com, Inc.
//-----------------------------------------------------------------------------

#include "../../gl/hlslCompat.glsl"

//-----------------------------------------------------------------------------
// Defines                                                                  
//-----------------------------------------------------------------------------

// waveData
#define WAVE_SPEED(i)      waveData[i].x
#define WAVE_MAGNITUDE(i)  waveData[i].y
   
// Worldspace position of this pixel
varying vec3 worldPos;

// TexCoord 0 and 1 (xy,zw) for ripple texture lookup
varying vec4 rippleTexCoord01;

// TexCoord 2 for ripple texture lookup
varying vec2 rippleTexCoord2;

// Screenspace vert position BEFORE wave transformation
varying vec4 posPreWave;

// Screenspace vert position AFTER wave transformation
varying vec4 posPostWave;  

// Worldspace unit distance/depth of this vertex/pixel
varying float  pixelDist;   

varying vec3 fogPos;

varying float worldSpaceZ;

//-----------------------------------------------------------------------------
// Uniforms                                                                  
//-----------------------------------------------------------------------------
uniform mat4 modelMat;
uniform mat4 modelview;
uniform mat3 cubeTrans;
uniform mat4 objTrans;
uniform vec3   cubeEyePos;
uniform vec3   eyePos;       
uniform vec2   waveDir[3];
uniform vec2   waveData[3];
uniform vec2   rippleDir[3];
uniform vec2   rippleTexScale[3];                    
uniform vec3   rippleSpeed;
uniform vec3   inLightVec;
uniform vec3   reflectNormal;
uniform float    gridElementSize;
uniform float    elapsedTime;
uniform float    undulateMaxDist;
uniform vec4   renderTargetParams;

//-----------------------------------------------------------------------------
// Main                                                                        
//-----------------------------------------------------------------------------
void main()
{	
   vec4 position = gl_Vertex;
   vec3 normal = gl_Normal;
   vec2 undulateData = gl_MultiTexCoord0.st;
   vec4 horizonFactor = gl_MultiTexCoord1;
   
   // use projection matrix for reflection / refraction texture coords
   mat4 texGen = mat4(0.5, 0.0, 0.0, 0.0,
                      0.0, 0.5, 0.0, 0.0,
                      0.0, 0.0, 1.0, 0.0,
                      0.5, 0.5, 0.0, 1.0);

   // Move the vertex based on the horizonFactor if specified to do so for this vert.
   if ( horizonFactor.z > 0.0 )
   {
      vec2 offsetXY = eyePos.xy - mod(eyePos.xy, gridElementSize);         
      position.xy += offsetXY;
      undulateData += offsetXY; 
   }      
   
   fogPos = position.xyz;
   position.z = mix( position.z, eyePos.z, horizonFactor.x );
   
   // Send pre-undulation screenspace position
   posPreWave = modelview * position;
   posPreWave = texGen * posPreWave;
      
   // Calculate the undulation amount for this vertex.   
   vec2 undulatePos = undulateData;
   if ( undulatePos.x < 0.0 )
      undulatePos = position.xy;
   
   float undulateAmt = 0.0;
   
   //#ifndef UNDERWATER  
   
   for ( int i = 0; i < 3; i++ )
   {
      undulateAmt += WAVE_MAGNITUDE(i) * sin( elapsedTime * WAVE_SPEED(i) + 
                                              undulatePos.x * waveDir[i].x +
                                              undulatePos.y * waveDir[i].y );
   }      
   
   // Scale down wave magnitude amount based on distance from the camera.   
   float dist = length( position.xyz - eyePos );
   dist = clamp( dist, 1.0, undulateMaxDist );          
   undulateAmt *= ( 1.0 - dist / undulateMaxDist ); 
   
   // Also scale down wave magnitude if the camera is very very close.
   undulateAmt *= saturate( ( length( position.xyz - eyePos ) - 0.5 ) / 10.0 );
   
   //#endif
   //undulateAmt = 0;
   
   // Apply wave undulation to the vertex.
   posPostWave = position;
   posPostWave.xyz += normal.xyz * undulateAmt;   
   
   // Save worldSpace position of this pixel/vert
   worldPos = posPostWave.xyz;   
   
   worldSpaceZ = ( modelMat * vec4(fogPos,1.0) ).z;
   if ( horizonFactor.x > 0.0 )
   {
      vec3 awayVec = normalize( fogPos.xyz - eyePos );
      fogPos.xy += awayVec.xy * 1000.0;
   }
   
   // Convert to screen 
   posPostWave = modelview * posPostWave;   
   
   // Setup the OUT position symantic variable
   gl_Position = posPostWave;
   gl_Position.z = mix(gl_Position.z, gl_Position.w, horizonFactor.x);
   
   // Save world space camera dist/depth of the outgoing pixel
   pixelDist = gl_Position.z;              

   // Convert to reflection texture space   
   posPostWave = texGen * posPostWave;
        
   vec2 txPos = normalize( position.xy );
   txPos *= 50000.0;
      
   // set up tex coordinates for the 3 interacting normal maps
   rippleTexCoord01.xy = mix( position.xy * rippleTexScale[0], txPos.xy * rippleTexScale[0], horizonFactor.x );
   rippleTexCoord01.xy += rippleDir[0] * elapsedTime * rippleSpeed.x;

   rippleTexCoord01.zw = mix( position.xy * rippleTexScale[1], txPos.xy * rippleTexScale[1], horizonFactor.x );
   rippleTexCoord01.zw += rippleDir[1] * elapsedTime * rippleSpeed.y;

   rippleTexCoord2.xy = mix( position.xy * rippleTexScale[2], txPos.xy * rippleTexScale[2], horizonFactor.x );
   rippleTexCoord2.xy += rippleDir[2] * elapsedTime * rippleSpeed.z; 
}

