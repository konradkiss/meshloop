//-----------------------------------------------------------------------------
// Torque 3D
// Copyright (C) GarageGames.com, Inc.
//-----------------------------------------------------------------------------


/// Calculate fog based on a start and end positions in worldSpace.
float computeSceneFog(  vec3 startPos,
                        vec3 endPos,
                        float fogDensity,
                        float fogDensityOffset,
                        float fogHeightFalloff )
{      
   float f = length( startPos - endPos ) - fogDensityOffset;
   float h = 1.0 - ( endPos.z * fogHeightFalloff );  
   return exp( -fogDensity * f * h );  
}


/// Calculate fog based on a start and end position and a height.
/// Positions do not need to be in worldSpace but height does.
float computeSceneFog( vec3 startPos,
                       vec3 endPos,
                       float height,
                       float fogDensity,
                       float fogDensityOffset,
                       float fogHeightFalloff )
{
   float f = length( startPos - endPos ) - fogDensityOffset;
   float h = 1.0 - ( height * fogHeightFalloff );
   return exp( -fogDensity * f * h );
}


/// Calculate fog based on a distance, height is not used.
float computeSceneFog( float dist, float fogDensity, float fogDensityOffset )
{
   float f = dist - fogDensityOffset;
   return exp( -fogDensity * f );
}


/// Convert a vec4 uv in viewport space to render target space.
vec2 viewportCoordToRenderTarget( vec4 inCoord, vec4 rtParams )
{   
   vec2 outCoord = inCoord.xy / inCoord.w;
   outCoord = ( outCoord * rtParams.zw ) + rtParams.xy;  
   return outCoord;
}


/// Convert a vec2 uv in viewport space to render target space.
vec2 viewportCoordToRenderTarget( vec2 inCoord, vec4 rtParams )
{   
   vec2 outCoord = ( inCoord * rtParams.zw ) + rtParams.xy;  
   return outCoord;
}

/// Basic assert macro.  If the condition fails, then the shader will output color.
/// @param condition This should be a bvec[2-4].  If any items is false, condition is considered to fail.
/// @param color The color that should be outputted if the condition fails.
/// @note This macro will only work in the void main() method of a pixel shader.
#define assert(condition, color) { if(!any(condition)) { gl_FragColor = color; return; } }
