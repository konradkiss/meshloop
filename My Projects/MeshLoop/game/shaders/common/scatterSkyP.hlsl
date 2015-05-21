//-----------------------------------------------------------------------------
// Torque 3D
// Copyright (C) GarageGames.com, Inc.
//-----------------------------------------------------------------------------

#include "torque.hlsl"

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

struct Conn
{
   float4 position : POSITION;
   float4 rayleighColor : TEXCOORD0;
   float4 mieColor : TEXCOORD1;
   float3 v3Direction : TEXCOORD2;
   float zPosition : TEXCOORD3;
};

uniform samplerCUBE nightSky : register(S0);
uniform float3 pLightDir;
uniform float4 nightColor;
uniform float2 nightInterpAndExposure;
uniform float useCubemap;

float4 main( Conn In ) : COLOR0
{ 

   float fCos = dot( pLightDir, In.v3Direction ) / length(In.v3Direction);
   float fCos2 = fCos*fCos;
    
   float g = -0.991;
   float g2 = -0.991 * -0.991;

   float fMiePhase = 1.5 * ((1.0 - g2) / (2.0 + g2)) * (1.0 + fCos2) / pow(abs(1.0 + g2 - 2.0*g*fCos), 1.5);
   
   float4 color = In.rayleighColor + fMiePhase * In.mieColor;//getRayleighPhase(fCos2) * In.rayleighColor + fMiePhase * In.mieColor;////
   color.a = color.b;
   
   float4 outColor = 1 - exp(-nightInterpAndExposure.x * color);
  
   float4 Out; 
   
   float4 nightSkyColor = texCUBE( nightSky, -In.v3Direction );
   nightSkyColor = lerp( nightColor, nightSkyColor, useCubemap );
   
   Out = lerp( outColor, nightSkyColor, nightInterpAndExposure.y );
     
   // Clip based on the camera-relative
   // z position of the vertex, passed through
   // from the vertex position.
   clip( In.zPosition );
   
   return hdrEncode( Out );
}
