//-----------------------------------------------------------------------------
// Torque 3D
// Copyright (C) GarageGames.com, Inc.
//-----------------------------------------------------------------------------

#include "./../postFx.hlsl"

float4 main( PFXVertToPix IN ) : COLOR
{  
   float power = pow( IN.uv0.x, 0.1 );   
   return float4( power, 0, 0, 1 );   
}