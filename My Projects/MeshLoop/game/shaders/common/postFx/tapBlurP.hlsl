//-----------------------------------------------------------------------------
// Torque Game Engine Advanced
// Copyright (C) GarageGames.com, Inc.
//-----------------------------------------------------------------------------

#include "./postFx.hlsl"

static const int TAPS = 13;

float gKernel[TAPS] =
	{ -6, -5, -4, -3, -2, -1, 0, 1, 2, 3, 4, 5, 6 };
				
static const float gWeights[TAPS] = 
{
	0.002216,
	0.008764,
	0.026995,
	0.064759,
	0.120985,
	0.176033,
	0.199471,
	0.176033,
	0.120985,
	0.064759,
	0.026995,
	0.008764,
	0.002216,
};
	
float4 main(	PFXVertToPix IN,
				uniform sampler2D inputTex : register(S0),
				uniform float4 texSize0 : register(C0) ) : COLOR
{
    float4 blur = 0;
                
    for ( int i = 0; i < TAPS; i++ )
    {
		float2 offset = BLUR_DIR * ( texSize0.zw * gKernel[i] );				
		blur += tex2D( inputTex, IN.uv0 + offset ) * gWeights[i];
	}
	
	return blur;
}