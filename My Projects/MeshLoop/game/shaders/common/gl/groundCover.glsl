//-----------------------------------------------------------------------------
// Torque 3D
// Copyright (C) GarageGames.com, Inc.
//-----------------------------------------------------------------------------

//static float sPi     = 3.14159265f;
//static float sTwoPi  = 6.28318530f;

///////////////////////////////////////////////////////////////////////////////
// The following wind effect was derived from the GPU Gems 3 chapter...
//
// "Vegetation Procedural Animation and Shading in Crysis"
// by Tiago Sousa, Crytek
//

vec2 smoothCurve( vec2 x )
{
   return x * x * ( 3.0 - 2.0 * x );
}

vec2 triangleWave( vec2 x )
{
   return abs( fract( x + 0.5 ) * 2.0 - 1.0 );
}

vec2 smoothTriangleWave( vec2 x )
{
   return smoothCurve( triangleWave( x ) );
}

float windTurbulence( float bbPhase, float frequency, float strength )
{
   // We create the input value for wave generation from the frequency and phase.
   vec2 waveIn = vec2(bbPhase) + vec2(frequency);

   // We use two square waves to generate the effect which
   // is then scaled by the overall strength.
   vec2 waves = ( fract( waveIn.xy * vec2( 1.975, 0.793 ) ) * 2.0 - 1.0 );
   waves = smoothTriangleWave( waves );

   // Sum up the two waves into a single wave.
   return ( waves.x + waves.y ) * strength;
}

vec2 windEffect(   float bbPhase, 
                     vec2 windDirection,
                     float gustLength,
                     float gustFrequency,
                     float gustStrength,
                     float turbFrequency,
                     float turbStrength )
{
   // Calculate the ambient wind turbulence.
   float turbulence = windTurbulence( bbPhase, turbFrequency, turbStrength );

   // We simulate the overall gust via a sine wave.
   float gustPhase = clamp( sin( ( bbPhase - gustFrequency ) / gustLength ) , 0.0, 1.0 );
   float gustOffset = ( gustPhase * gustStrength ) + ( ( 0.2 + gustPhase ) * turbulence );

   // Return the final directional wind effect.
   return vec2(gustOffset) * windDirection.xy;
}
