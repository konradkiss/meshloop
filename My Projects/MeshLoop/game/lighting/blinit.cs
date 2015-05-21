//---------------------------------------------------------------------------------------------
// Torque 3D
// Copyright (C) GarageGames.com, Inc.
//---------------------------------------------------------------------------------------------

exec( "./shadowFilter.cs" );

singleton GFXStateBlockData( BL_ProjectedShadowSBData )
{
   blendDefined = true;
   blendEnable = true;
   blendSrc = GFXBlendSrcAlpha;
   blendDest = GFXBlendInvSrcAlpha;
         
   zDefined = true;
   zEnable = true;
   zWriteEnable = false;
               
   samplersDefined = true;
   samplerStates[0] = SamplerClampLinear;   
   vertexColorEnable = true;
};

singleton ShaderData( BL_ProjectedShadowShaderData )
{
   DXVertexShaderFile     = "shaders/common/projectedShadowV.hlsl";
   DXPixelShaderFile      = "shaders/common/projectedShadowP.hlsl";   
   
   OGLVertexShaderFile     = "shaders/common/gl/projectedShadowV.glsl";
   OGLPixelShaderFile      = "shaders/common/gl/projectedShadowP.glsl";   
   
   samplerNames[0] = "$inputTex";
   
   pixVersion = 2.0;
};

singleton CustomMaterial( BL_ProjectedShadowMaterial )
{
   texture[0] = "$miscbuff";
 
   shader = BL_ProjectedShadowShaderData;
   stateBlock = BL_ProjectedShadowSBData;
   version = 2.0;
};

function onActivateBasicLM()
{
   // If HDR is enabled... enable the special format token.
   if ( $platform !$= "macos" && HDRPostFx.isEnabled )
      AL_FormatToken.enable();
      
   // Create render pass for projected shadow.
   new RenderPassManager( BL_ProjectedShadowRPM );
   BL_ProjectedShadowRPM.addManager( new RenderMeshMgr() );
}

function onDeactivateBasicLM()
{
   BL_ProjectedShadowRPM.delete();
}

function setBasicLighting()
{
   setLightManager( "Basic Lighting" );   
}
