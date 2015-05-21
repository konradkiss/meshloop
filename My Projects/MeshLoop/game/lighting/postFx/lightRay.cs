//-----------------------------------------------------------------------------
// Torque 3D
// Copyright (C) GarageGames.com, Inc.
//-----------------------------------------------------------------------------


$LightRayPostFX::brightScalar = 0.75;


singleton ShaderData( LightRayOccludeShader )
{
   DXVertexShaderFile 	= "shaders/common/postFx/postFxV.hlsl";
   DXPixelShaderFile 	= "shaders/common/postFx/lightRay/lightRayOccludeP.hlsl";

   pixVersion = 3.0;   
};

singleton ShaderData( LightRayShader )
{
   DXVertexShaderFile 	= "shaders/common/postFx/postFxV.hlsl";
   DXPixelShaderFile 	= "shaders/common/postFx/lightRay/lightRayP.hlsl";

   pixVersion = 3.0;   
};

singleton GFXStateBlockData( LightRayStateBlock : PFX_DefaultStateBlock )
{
   samplersDefined = true;
   samplerStates[0] = SamplerClampLinear;
   samplerStates[1] = SamplerClampLinear;     
};

singleton PostEffect( LightRayPostFX )
{
   isEnabled = false;
   allowReflectPass = false;
   
   requirements = "PrePassDepth";
      
   renderTime = "PFXAfterDiffuse";
   renderPriority = 0.1;
      
   shader = LightRayOccludeShader;
   stateBlock = LightRayStateBlock;
   texture[0] = "$backBuffer";
   texture[1] = "#prepass";
   target = "$outTex";
   targetFormat = "GFXFormatR16G16B16A16F";
      
   new PostEffect()
   {
      shader = LightRayShader;
      stateBlock = LightRayStateBlock;
      texture[0] = "$inTex";
      texture[1] = "$backBuffer";
      target = "$backBuffer";
   };
};

function LightRayPostFX::setShaderConsts( %this )
{
   %this.setShaderConst( "$brightScalar", $LightRayPostFX::brightScalar );
}
