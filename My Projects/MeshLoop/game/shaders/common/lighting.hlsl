//-----------------------------------------------------------------------------
// Torque 3D
// Copyright (C) GarageGames.com, Inc.
//-----------------------------------------------------------------------------

// These are the uniforms used by most lighting shaders.

uniform float4 inLightPos[3];
uniform float4 inLightInvRadiusSq;
uniform float4 inLightColor[4];

#ifndef TORQUE_BL_NOSPOTLIGHT
   uniform float4 inLightSpotDir[3];
   uniform float4 inLightSpotAngle;
#endif

uniform float4 ambient;
uniform float specularPower;
uniform float4 specularColor;


void compute4Lights( float3 wsView, 
                     float3 wsPosition, 
                     float3 wsNormal,
                     float4 shadowMask,
                     out float4 outDiffuse,
                     out float4 outSpecular )
{
   // NOTE: The light positions and spotlight directions
   // are stored in SoA order, so inLightPos[0] is the
   // x coord for all 4 lights... inLightPos[1] is y... etc.
   //
   // This is the key to fully utilizing the vector units and
   // saving a huge amount of instructions.
   //
   // For example this change saved more than 10 instructions 
   // over a simple for loop for each light.
   
   int i;

   float4 lightVectors[3];
   for ( i = 0; i < 3; i++ )
      lightVectors[i] = wsPosition[i] - inLightPos[i];

   float4 squareDists = 0;
   for ( i = 0; i < 3; i++ )
      squareDists += lightVectors[i] * lightVectors[i];

   // Accumulate the dot product between the light 
   // vector and the normal.
   //
   // The normal is negated because it faces away from
   // the surface and the light faces towards the
   // surface... this keeps us from needing to flip
   // the light vector direction which complicates
   // the spot light calculations.
   //
   // We normalize the result a little later.
   //
   float4 nDotL = 0;
   for ( i = 0; i < 3; i++ )
      nDotL += lightVectors[i] * -wsNormal[i];

   float4 rDotL = 0;
   #ifndef TORQUE_BL_NOSPECULAR

      // We're using the Phong specular reflection model
      // here where traditionally Torque has used Blinn-Phong
      // which has proven to be more accurate to real materials.
      //
      // We do so because its cheaper as do not need to 
      // calculate the half angle for all 4 lights.
      //   
      // Advanced Lighting still uses Blinn-Phong, but the
      // specular reconstruction it does looks fairly similar
      // to this.
      //
      float3 R = reflect( wsView, -wsNormal );

      for ( i = 0; i < 3; i++ )
         rDotL += lightVectors[i] * R[i];

   #endif
 
   // Normalize the dots.
   //
   // Notice we're using the half type here to get a
   // much faster sqrt via the rsq_pp instruction at 
   // the loss of some precision.
   //
   // Unless we have some extremely large point lights
   // i don't believe the precision loss will matter.
   //
   half4 correction = (half4)rsqrt( squareDists );
   nDotL = saturate( nDotL * correction );
   rDotL = saturate( rDotL * correction );

   // First calculate a simple point light linear 
   // attenuation factor.
   //
   // If this is a directional light the inverse
   // radius should be greater than the distance
   // causing the attenuation to have no affect.
   //
   float4 atten = saturate( 1.0 - ( squareDists * inLightInvRadiusSq ) );

   #ifndef TORQUE_BL_NOSPOTLIGHT

      // A simple spotlight attenuation factor.  This 
      // is really fast... 5 instructions for 4 spots.

      float4 spotAtten = 0;
      for ( i = 0; i < 3; i++ )
         spotAtten += lightVectors[i] * inLightSpotDir[i];

      atten *= ( spotAtten * correction ) > inLightSpotAngle;

   #endif

   // Finally apply the shadow masking on the attenuation.
   atten *= shadowMask;

   // Get the final light intensity.
   float4 intensity = nDotL * atten;

   // Combine the light colors for output.
   outDiffuse = 0;
   for ( i = 0; i < 4; i++ )
      outDiffuse += intensity[i] * inLightColor[i];

   // Apply the per-light specular attenuation.
   atten *= float4( inLightColor[0].a, inLightColor[1].a, inLightColor[2].a, inLightColor[3].a );

   // Output the specular power.
   rDotL = max( rDotL, 0.00001 );
   float4 specular = pow( rDotL, specularPower.xxxx ) * atten;

   // Add the final specular intensity values together
   // using a single dot product operation then get the
   // final specular lighting color.
   outSpecular = specularColor * dot( specular, 1.0 );
}


/// The standard specular calculation.
///
///   @param toLight    Normalized vector representing direction from the pixel 
///                     being lit, to the light source, in world space.
///
///   @param normal  Normalized surface normal.
///   
///   @param toEye   The normalized vector representing direction from the pixel 
///                  being lit to the camera.
///
///   @param specPwr    The specular exponent.
///
///   @param specScale  A scalar on the specular output used in RGB accumulation.
///
float calcSpecular( float3 toLight, float3 normal, float3 toEye, float specPwr )
{
   #ifdef PHONG_SPECULAR 
      // (R.V)^c
      float specVal = dot( normalize( -reflect( toLight, normal ) ), toEye );
   #else
      // (N.H)^c [Blinn-Phong, TGEA style, default]
      float specVal = dot( normal, normalize( toLight + toEye ) );
   #endif

   // Return the specular factor.
   return pow( max( specVal, 0.00001f ), specPwr );
}
