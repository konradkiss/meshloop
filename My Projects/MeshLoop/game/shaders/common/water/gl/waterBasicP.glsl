//-----------------------------------------------------------------------------
// Torque 3D
// Copyright (C) GarageGames.com, Inc.
//-----------------------------------------------------------------------------

#include "../../gl/torque.glsl"
#include "../../gl/hlslCompat.glsl"

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

varying vec4 foamTexCoords;

//-----------------------------------------------------------------------------
// approximate Fresnel function
//-----------------------------------------------------------------------------
float fresnel(float NdotV, float bias, float power)
{
   return bias + (1.0-bias)*pow(abs(1.0 - max(NdotV, 0.0)), power);
}

//-----------------------------------------------------------------------------
// Uniforms                                                                  
//-----------------------------------------------------------------------------
uniform sampler2D      bumpMap;
//uniform sampler2D    prepassTex;
uniform sampler2D    reflectMap;
uniform sampler2D      refractBuff;
uniform samplerCube  skyMap;
//uniform sampler      foamMap;
uniform vec4       baseColor;
uniform vec4       miscParams;
uniform vec4       reflectParams;
uniform vec3       ambientColor;
uniform vec3       eyePos;
uniform vec3       distortionParams;
uniform vec3       fogData;
uniform vec4       fogColor;
uniform vec3       rippleMagnitude;

//-----------------------------------------------------------------------------
// Main                                                                        
//-----------------------------------------------------------------------------
void main()
{ 
   // Modulate baseColor by the ambientColor.
   vec4 waterBaseColor = baseColor * vec4( ambientColor.rgb, 1.0 );
   
   // Get the bumpNorm...
   vec3 bumpNorm = ( texture2D( bumpMap, rippleTexCoord01.xy ).rgb * 2.0 - 1.0 ) * rippleMagnitude.x;
   bumpNorm       += ( texture2D( bumpMap, rippleTexCoord01.zw ).rgb * 2.0 - 1.0 ) * rippleMagnitude.y;      
   bumpNorm       += ( texture2D( bumpMap, rippleTexCoord2 ).rgb * 2.0 - 1.0 ) * rippleMagnitude.z;  
      
   // We subtract a little from it so that we don't 
   // distort where the water surface intersects the
   // camera near plane.
   float distortAmt = saturate( pixelDist / 1.0 ) * 0.8;
      
   vec4 distortPos = posPostWave;
   distortPos.xy += bumpNorm.xy * distortAmt;   
 
 #ifdef UNDERWATER
   gl_FragColor = texture2DProj( refractBuff, distortPos.xyz );   
 #else

   vec3 eyeVec = worldPos - eyePos;
   vec3 reflectionVec = reflect( eyeVec, normalize(bumpNorm) ); 

   // Color that replaces the reflection color when we do not
   // have one that is appropriate.
   vec4 fakeColor = vec4(ambientColor,1.0);
   
   // Use fakeColor for ripple-normals that are angled towards the camera  
   eyeVec = -eyeVec;
   eyeVec = normalize( eyeVec );
   float ang = saturate( dot( eyeVec, bumpNorm ) );   
   float fakeColorAmt = ang;   
      
    // Get reflection map color
   vec4 refMapColor = texture2DProj( reflectMap, distortPos ); 
   // If we do not have a reflection texture then we use the cubemap.
   refMapColor = mix( refMapColor, textureCube( skyMap, -reflectionVec ), NO_REFLECT );      
   
   // Combine reflection color and fakeColor.
   vec4 reflectColor = mix( refMapColor, fakeColor, fakeColorAmt );
   //return refMapColor;
   
   // Get refract color
   vec4 refractColor = texture2DProj( refractBuff, distortPos.xyz );   
   
   // calc "diffuse" color by lerping from the water color
   // to refraction image based on the water clarity.
   vec4 diffuseColor = mix( refractColor, waterBaseColor, 1.0 - CLARITY );   
   
   // fresnel calculation 
   float fresnelTerm = fresnel( ang, FRESNEL_BIAS, FRESNEL_POWER );	
   
   // Also scale the frensel by our distance to the
   // water surface.  This removes the hard reflection
   // when really close to the water surface.
   fresnelTerm *= saturate( pixelDist - 0.1 );
   
   // Combine the diffuse color and reflection image via the
   // fresnel term and set out output color.
   gl_FragColor = mix( diffuseColor, reflectColor, fresnelTerm );  
   
   // Fog it.   
   float factor = computeSceneFog( eyePos, 
                                   fogPos, 
                                   worldSpaceZ,
                                   fogData.x,
                                   fogData.y,
                                   fogData.z );

   gl_FragColor.rgb = mix( gl_FragColor.rgb, fogColor.rgb, 1.0 - saturate( factor ) );   
#endif   
}
