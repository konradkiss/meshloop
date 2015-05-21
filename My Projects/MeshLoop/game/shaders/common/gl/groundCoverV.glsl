//-----------------------------------------------------------------------------
// Torque 3D
// Copyright (C) GarageGames.com, Inc.
//-----------------------------------------------------------------------------

#include "groundCover.glsl"

uniform mat4 modelViewProj;
uniform vec3 camRight;
uniform vec3 camUp;
uniform vec3 camPos;
uniform vec2 fadeParams;
uniform vec3 terrainInfo;
uniform vec2 windDir;
uniform vec3 gustInfo;
uniform vec2 turbInfo;
uniform vec4 typeRects[MAX_COVERTYPES];

varying vec4 vAmbient;
varying vec2 texCoord;
varying vec4 vPos;

void main()       
{
   // Some useful variables
   float sCornerRight[4];
   sCornerRight[0] = -0.5;
   sCornerRight[1] =  0.5;
   sCornerRight[2] =  0.5;
   sCornerRight[3] = -0.5;

   float sCornerUp[4];
   sCornerUp[0] = 0.0;
   sCornerUp[1] = 0.0;
   sCornerUp[2] = 1.0;
   sCornerUp[3] = 1.0;

   vec2 sUVCornerExtent[4];
   sUVCornerExtent[0] = vec2( 0.0, 1.0 );
   sUVCornerExtent[1] = vec2( 1.0, 1.0 );
   sUVCornerExtent[2] = vec2( 1.0, 0.0 );
   sUVCornerExtent[3] = vec2( 0.0, 0.0 );

   vec3 position = gl_Vertex.xyz;
   vec4 params = gl_MultiTexCoord0.xyzw;
   vec4 ambient = gl_Color.rgba;
   
   // Pull some of the parameters for clarity.     
   float    fadeStart      = fadeParams.x;
   float    fadeEnd        = fadeParams.y;
   float fadeRange   = fadeEnd - fadeStart;

   //float    maxFadeJitter  = ( fadeEnd - fadeStart ) / 2.0f;    
   int      corner      = int(( ambient.a * 255.0f ) + 0.5f);
   vec2   size        = params.xy;  
   int      typeIndex   = int(params.z);
           
   // The billboarding is based on the camera direction.
   vec3 rightVec   = camRight * sCornerRight[corner];
   vec3 upVec      = camUp * sCornerUp[corner];               

   // Figure out the corner position.
   vec4 outPoint;
   outPoint.xyz = ( upVec * size.y ) + ( rightVec * size.x );
   float len = length( outPoint.xyz );
   
   // We derive the billboard phase used for wind calculations from its position.
   float bbPhase = dot( position.xyz, vec3(1.0) );

   // Get the overall wind gust and turbulence effects.
   vec3 wind;
   wind.xy = windEffect(   bbPhase,
                           windDir,
                           gustInfo.x, gustInfo.y, gustInfo.z,
                           turbInfo.x, turbInfo.y );
   wind.z = 0.0;

   // Add the summed wind effect into the point.
   outPoint.xyz += wind.xyz * params.w;

   // Do a simple spherical clamp to keep the foliage
   // from stretching too much by wind effect.
   outPoint.xyz = normalize( outPoint.xyz ) * len;

   // Move the point into world space.
   outPoint.xyz += position.xyz;
   outPoint.w = 1.0;

   // Grab the uv set and setup the texture coord.
   vec4 uvSet = typeRects[typeIndex]; 
   texCoord.x = uvSet.x + ( uvSet.z * sUVCornerExtent[corner].x );
   texCoord.y = uvSet.y + ( uvSet.w * sUVCornerExtent[corner].y );

   // Get the alpha fade value.
   float dist = distance( camPos, outPoint.xyz ) - fadeStart;
   float alpha = 1.0 - clamp( dist / fadeRange, 0.0, 1.0 );

   // Setup the shader output data.
   gl_Position = modelViewProj * outPoint;
   vAmbient = vec4( ambient.rgb, alpha );
   vPos = gl_Position;
}
