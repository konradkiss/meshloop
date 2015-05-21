//-----------------------------------------------------------------------------
// Torque 3D
// Copyright (C) GarageGames.com, Inc.
//-----------------------------------------------------------------------------

#include "../postFx.hlsl"

uniform sampler2D frameSampler : register( s0 );
uniform sampler2D backBuffer : register( s1 );

uniform float3 camForward;
uniform float3 lightDirection;
uniform float2 screenSunPos;
uniform float2 oneOverTargetSize;

#define NUM_SAMPLES 100

#define Density 0.94 //0.84
#define Weight 5.65 //5.65
#define Decay 1 //1.0
#define Exposure 0.0005

float4 main( PFXVertToPix IN ) : COLOR0  
{  
    float2 texCoord = IN.uv0;
    
    // Calculate vector from pixel to light source in screen space.
    half2 deltaTexCoord = ( texCoord - screenSunPos );  

    // Divide by number of samples and scale by control factor.  
    deltaTexCoord *= 1.0f / (float)NUM_SAMPLES * Density; 
    
    // Store initial sample.
    half3 color = tex2D( frameSampler, texCoord );  

    // Set up illumination decay factor.
    half illuminationDecay = 1.0f;  

    // Evaluate summation from Equation 3 NUM_SAMPLES iterations.  
    for ( int i = 0; i < NUM_SAMPLES; i++ )  
    {  
        // Step sample location along ray.
        texCoord -= deltaTexCoord;  

        // Retrieve sample at new location.
        half3 sample = tex2D( frameSampler, texCoord );  

        // Apply sample attenuation scale/decay factors.
        sample *= illuminationDecay * Weight;

        // Accumulate combined color.
        color += sample;

        // Update exponential decay factor.
        illuminationDecay *= Decay;
    }  

    float4 bbCol = tex2D( backBuffer, IN.uv1 );

    float amount = dot( -lightDirection, camForward );
   
    // Output final color with a further scale control factor.      
    return saturate( amount ) * float4( color * Exposure, 1 ) + bbCol;
}  
