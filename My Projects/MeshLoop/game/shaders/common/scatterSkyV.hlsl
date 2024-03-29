//-----------------------------------------------------------------------------
// Copyright (C) Sickhead Games, LLC
//-----------------------------------------------------------------------------

#define IN_HLSL
//#include "shdrConsts.h"

const int nSamples = 4;
const float fSamples = 4.0;

// The scale depth (the altitude at which the average atmospheric density is found)
const float fScaleDepth = 0.25;
const float fInvScaleDepth = 1.0 / 0.25;

// The scale equation calculated by Vernier's Graphical Analysis
float vernierScale(float fCos)
{
	float x = 1.0 - fCos;
	float x5 = x * 5.25;
	float x5p6 = (-6.80 + x5);
	float xnew = (3.83 + x * x5p6);
	float xfinal = (0.459 + x * xnew);
	float xfinal2 = -0.00287 + x * xfinal;
	float outx = exp( xfinal2 ); 
	return 0.25 * outx;
}

// This is the shader input vertex structure.
struct Vert
{
   // .xyz  = point
   float4 position : POSITION;
   
   float3 normal   : NORMAL;
   
   float4 color : TEXCOORD0;  
};

// This is the shader output data.
struct Conn
{
   float4 position : POSITION;
   float4 rayleighColor : TEXCOORD0;
   float4 mieColor : TEXCOORD1;
   float3 v3Direction : TEXCOORD2;
   float  zPosition : TEXCOORD3;
};
 
Conn main(  Vert In,
            uniform float4x4 modelView          : register(C0),
            uniform float4 misc                 : register(C4),
            uniform float4 sphereRadii          : register(C5),
            uniform float4 scatteringCoeffs     : register(C6),
            uniform float3 camPos               : register(C7),
            uniform float3 lightDir             : register(C8),
            uniform float4 invWaveLength        : register(C9)
            )	         
{
   // Pull some variables out:
   float camHeight = misc.x;
   float camHeightSqr = misc.y;
   
   float scale = misc.z;
   float scaleOverScaleDepth = misc.w;
   
   float outerRadius = sphereRadii.x;
   float outerRadiusSqr = sphereRadii.y;
   
   float innerRadius = sphereRadii.z;
   float innerRadiusSqr = sphereRadii.w;
   
   float rayleighBrightness = scatteringCoeffs.x; // Kr * ESun
   float rayleigh4PI = scatteringCoeffs.y; // Kr * 4 * PI

   float mieBrightness = scatteringCoeffs.z; // Km * ESun
   float mie4PI = scatteringCoeffs.w; // Km * 4 * PI
   
   // Get the ray from the camera to the vertex, 
   // and its length (which is the far point of the ray 
   // passing through the atmosphere).
   float4 v3Pos = In.position / 6378000.0;// / outerRadius;
   float3 newCamPos = float3( 0, 0, camHeight );
   v3Pos.z += innerRadius;
   float3 v3Ray = v3Pos.xyz - newCamPos;
   float fFar = length(v3Ray);
   v3Ray /= fFar;

   // Calculate the ray's starting position, 
   // then calculate its scattering offset.
   float3 v3Start = newCamPos;
   float fHeight = length(v3Start); 
   float fDepth = exp(scaleOverScaleDepth * (innerRadius - camHeight));
   float fStartAngle = dot(v3Ray, v3Start) / fHeight;

   float x = 1.0 - fStartAngle;
   float x5 = x * 5.25;
   float x5p6 = (-6.80 + x5);
   float xnew = (3.83 + x * x5p6);
   float xfinal = (0.459 + x * xnew);
   float xfinal2 = -0.00287 + x * xfinal;
   float othx = exp( xfinal2 ); 
   float vscale1 = 0.25 * othx;

   float fStartOffset = fDepth * vscale1;//vernierScale(fStartAngle);

   // Initialize the scattering loop variables.
   float fSampleLength = fFar / 2;
   float fScaledLength = fSampleLength * scale;
   float3 v3SampleRay = v3Ray * fSampleLength;
   float3 v3SamplePoint = v3Start + v3SampleRay * 0.5;

	  // Now loop through the sample rays
	  float3 v3FrontColor = float3(0.0, 0.0, 0.0);
   for(int i=0; i<2; i++)
   {
      float fHeight = length(v3SamplePoint);
      float fDepth = exp(scaleOverScaleDepth * (innerRadius - fHeight));
      float fLightAngle = dot(lightDir, v3SamplePoint) / fHeight;
      float fCameraAngle = dot(v3Ray, v3SamplePoint) / fHeight;

      x = 1.0 - fCameraAngle;
      x5 = x * 5.25;
      x5p6 = (-6.80 + x5);
      xnew = (3.83 + x * x5p6);
      xfinal = (0.459 + x * xnew);
      xfinal2 = -0.00287 + x * xfinal;
      othx = exp( xfinal2 ); 
      float vscale3 = 0.25 * othx;


      x = 1.0 - fLightAngle;
      x5 = x * 5.25;
      x5p6 = (-6.80 + x5);
      xnew = (3.83 + x * x5p6);
      xfinal = (0.459 + x * xnew);
      xfinal2 = -0.00287 + x * xfinal;
      othx = exp( xfinal2 ); 
      float vscale2 = 0.25 * othx;

      float fScatter = (fStartOffset + fDepth*(vscale2 - vscale3));
      float3 v3Attenuate = exp(-fScatter * (invWaveLength.xyz * rayleigh4PI + mie4PI));
      v3FrontColor += v3Attenuate * (fDepth * fScaledLength);
      v3SamplePoint += v3SampleRay;
   } 

   Conn Out;   
   

   
	  // Finally, scale the Mie and Rayleigh colors 
   // and set up the varying variables for the pixel shader.
   Out.position = mul( modelView, In.position );//mul( modelView, v3Pos );//
   Out.mieColor.rgb = v3FrontColor * mieBrightness;
   Out.mieColor.a = 1.0f;
	  Out.rayleighColor.rgb = v3FrontColor * (invWaveLength.xyz * rayleighBrightness);
	  Out.rayleighColor.a = 1.0f;
   Out.v3Direction = newCamPos - v3Pos.xyz;      
   
   // This offset is to get rid of the black line between the atmosky and the waterPlane
   // along the horizon.
   Out.zPosition = In.position.z + 4000;

   return Out;
}

