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

static float blurRadius = 5.0;
              
float4 main( PFXVertToPix IN ) : COLOR
{
   float2 texCoordSample = 0;     
   float2 pixelSize = 1.0 / targetSize;
   float4 cOut;
   
   //return float4(1,0,1,1);
   //return tex2D(tLowResImage, IN.uv0);
   
   cOut = 0.5 * tex2D(tLowResImage, IN.uv0);
      
   texCoordSample.y = IN.uv0.y;
   texCoordSample.x = IN.uv0.x + pixelSize.x;
   cOut += 0.25 * tex2D(tLowResImage, texCoordSample);
   
   texCoordSample.x = IN.uv0.x - pixelSize.x;
   cOut += 0.25 * tex2D(tLowResImage, texCoordSample);
      
   return cOut;
}


