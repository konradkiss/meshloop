//-----------------------------------------------------------------------------
// Torque Game Engine Advanced
// Copyright (C) GarageGames.com, Inc.
//-----------------------------------------------------------------------------

#include "groundCover.glsl"

uniform sampler2D diffuseMap;

#ifdef TORQUE_ADVANCED_LIGHTING
uniform sampler2D lightTex;
uniform vec4 lightRT;
#endif

varying vec4 vAmbient;
varying vec2 texCoord;
varying vec4 vPos;

void main()
{
   #ifdef TORQUE_ADVANCED_LIGHTING

      // Convert the screen position into the light tex space.
      vec2 lightCoord = vPos.xy / vPos.w;
      lightCoord = ( lightCoord + 1.0 ) / 2.0;
      lightCoord = ( lightCoord * lightRT.zw ) + lightRT.xy;
      lightCoord.y = 1.0 - lightCoord.y; 

      // Get the lighting at this pixel.
      vec4 lightColor;
      float nlAttenuation, specularPower;
      lightinfoUncondition( texture2D( lightTex, lightCoord ), lightColor.rgb, nlAttenuation, specularPower );
      lightColor.a = 1;

   #else

      // Why the 2x?  In the brightest areas the lightmap value is 2/3s of the
      // sunlight color and in the darkest areas its 1/2 of the ambient value.
      // So the 2x ensures that the ambient doesn't get darker than it should
      // at the expense of a little over brightening in the sunlight.
      vec4 lightColor = vec4( clamp( vAmbient.rgb * 2.0, 0.0, 1.0 ), vAmbient.a );

   #endif

   // Return the final color.
   gl_FragColor = texture2D( diffuseMap, texCoord ) * lightColor;
}
