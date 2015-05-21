//-----------------------------------------------------------------------------
// Copyright (C) Sickhead Games, LLC
//-----------------------------------------------------------------------------

// Calculates the Mie phase function
float getMiePhase(float fCos, float fCos2, float g, float g2)
{
	return 1.5 * ((1.0 - g2) / (2.0 + g2)) * (1.0 + fCos2) / pow(abs(1.0 + g2 - 2.0*g*fCos), 1.5);
}

// Calculates the Rayleigh phase function
float getRayleighPhase(float fCos2)
{
	//return 1.0;
	return 0.75 + 0.75*fCos2;
}

varying vec4 rayleighColor;
varying vec4 mieColor;
varying vec3 v3Direction;
varying float zPosition;

uniform samplerCube nightSky;
uniform vec3 pLightDir;
uniform vec4 nightColor;
uniform vec2 nightInterpAndExposure;
uniform float useCubemap;

void main()         
{ 
   float fCos = dot( pLightDir, v3Direction ) / length(v3Direction);
   float fCos2 = fCos*fCos;
    
   float g = -0.991;
   float g2 = -0.991 * -0.991;

   float fMiePhase = 1.5 * ((1.0 - g2) / (2.0 + g2)) * (1.0 + fCos2) / pow(abs(1.0 + g2 - 2.0*g*fCos), 1.5);
   
   vec4 color = rayleighColor + fMiePhase * mieColor;//getRayleighPhase(fCos2) * rayleighColor + fMiePhase * mieColor;////
   color.a = color.b;
   
   vec4 outColor = vec4(1.0) - exp(-nightInterpAndExposure.x * color);
     
   vec4 nightSkyColor = textureCube(nightSky, -v3Direction);
   nightSkyColor = mix(nightColor, nightSkyColor, useCubemap);
     
   gl_FragColor = mix( outColor, nightSkyColor, nightInterpAndExposure.y );
   
   // Clip based on the camera-relative
   // z position of the vertex, passed through
   // from the vertex position.
   if(zPosition < 0.0)
      discard;
}
