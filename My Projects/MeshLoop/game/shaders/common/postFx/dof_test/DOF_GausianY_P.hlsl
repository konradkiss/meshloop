//-----------------------------------------------------------------------------
// Gausian blur shader.
// Gerhard Botha, 2008
// gerhard@hybrid-logic.com
// http://tgeaenhanced.blogspot.com
//-----------------------------------------------------------------------------

#define IN_HLSL
#include "../postFx.hlsl"

uniform sampler2D tLowResImage   : register(S0);
uniform float2 targetSize;

float4 main( PFXVertToPix IN ) : COLOR
{	
   float2 texCoordSample = 0;     
   float2 pixelSize = 1.0 / targetSize;
   float4 cOut;

   //return tex2D(tLowResImage, IN.uv0);
   
   // using bilinear texture lookups, this could be implemented
   // with just 2 texture fetches   
   
   cOut = 0.5 * tex2D(tLowResImage, IN.uv0);
   
   // ideally the vertex shader would compute the texture
   // coords and pass them down   
   texCoordSample.x = IN.uv0.x;
   texCoordSample.y = IN.uv0.y + pixelSize.y;
   cOut += 0.25 * tex2D(tLowResImage, texCoordSample);
   
   texCoordSample.y = IN.uv0.y - pixelSize.y;
   cOut += 0.25 * tex2D(tLowResImage, texCoordSample);
   
   return cOut;   
}


