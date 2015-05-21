//-----------------------------------------------------------------------------
// Torque 3D
// Copyright (C) GarageGames.com, Inc.
//-----------------------------------------------------------------------------

//------------------------------------------------------------------------------
// Fog
//------------------------------------------------------------------------------

singleton ShaderData( FogPassShader )
{   
   DXVertexShaderFile 	= "shaders/common/postFx/postFxV.hlsl";
   DXPixelShaderFile 	= "shaders/common/postFx/fogP.hlsl";
         
//   OGLVertexShaderFile  = "shaders/common/postFx/gl//postFxV.glsl";
//   OGLPixelShaderFile   = "shaders/common/postFx/gl/fogP.glsl";
            
   samplerNames[0] = "$prepassTex";
   
   pixVersion = 2.0;
};


singleton GFXStateBlockData( FogPassStateBlock : PFX_DefaultStateBlock )
{   
   blendDefined = true;
   blendEnable = true; 
   blendSrc = GFXBlendSrcAlpha;
   blendDest = GFXBlendInvSrcAlpha;
};


singleton PostEffect( FogPostFx )
{   
   // Let the fog effect render during the 
   // reflection pass.
   allowReflectPass = true;
      
   renderTime = "PFXBeforeBin";
   renderBin = "ObjTranslucentBin";   
   requirements = "PrePassDepth";
   
   shader = FogPassShader;
   stateBlock = FogPassStateBlock;
   texture[0] = "#prepass";
   
   renderPriority = 5;
   
   isEnabled = true;
};


//------------------------------------------------------------------------------
// UnderwaterFog
//------------------------------------------------------------------------------

singleton ShaderData( UnderwaterFogPassShader )
{   
   DXVertexShaderFile 	= "shaders/common/postFx/postFxV.hlsl";
   DXPixelShaderFile 	= "shaders/common/postFx/underwaterFogP.hlsl";
         
//   OGLVertexShaderFile  = "shaders/common/postFx/gl/postFxV.glsl";
//   OGLPixelShaderFile   = "shaders/common/postFx/gl/fogP.glsl";
            
   samplerNames[0] = "$prepassTex";
   
   pixVersion = 2.0;      
};


singleton GFXStateBlockData( UnderwaterFogPassStateBlock : PFX_DefaultStateBlock )
{   
   samplersDefined = true;
   samplerStates[0] = SamplerClampPoint;
   samplerStates[1] = SamplerClampPoint;   
};


singleton PostEffect( UnderwaterFogPostFx )
{
   oneFrameOnly = true;
   onThisFrame = false;
   
   // Let the fog effect render during the 
   // reflection pass.
   allowReflectPass = true;
      
   renderTime = "PFXBeforeBin";
   renderBin = "ObjTranslucentBin";   
   requirements = "PrePassDepth";
  
   shader = UnderwaterFogPassShader;
   stateBlock = UnderwaterFogPassStateBlock;
   texture[0] = "#prepass";
   texture[1] = "$backBuffer";
   
   // Needs to happen after the FogPostFx
   renderPriority = 4;
   
   isEnabled = true;
};

