//-----------------------------------------------------------------------------
// Torque 3D
// Copyright (C) GarageGames.com, Inc.
//-----------------------------------------------------------------------------


///
$SSAOPostFx::overallStrength = 2.0;

// TODO: Add small/large param docs.

// The small radius SSAO settings.
$SSAOPostFx::sRadius = 0.1;
$SSAOPostFx::sStrength = 6.0;
$SSAOPostFx::sDepthMin = 0.1;
$SSAOPostFx::sDepthMax = 1.0;
$SSAOPostFx::sDepthPow = 1.0;
$SSAOPostFx::sNormalTol = 0.0;
$SSAOPostFx::sNormalPow = 1.0;

// The large radius SSAO settings.
$SSAOPostFx::lRadius = 1.0;
$SSAOPostFx::lStrength = 10.0;
$SSAOPostFx::lDepthMin = 0.2;
$SSAOPostFx::lDepthMax = 2.0;
$SSAOPostFx::lDepthPow = 0.2;
$SSAOPostFx::lNormalTol = -0.5;
$SSAOPostFx::lNormalPow = 2.0;

/// Valid values: 0, 1, 2
$SSAOPostFx::quality = 0;

///
$SSAOPostFx::blurDepthTol = 0.001;

/// 
$SSAOPostFx::blurNormalTol = 0.95;


function SSAOPostFx::onAdd( %this )
{  
   %this.wasVis = "Uninitialized";
   %this.quality = "Uninitialized";
}

function SSAOPostFx::preProcess( %this )
{   
   if ( $SSAOPostFx::quality !$= %this.quality )
   {
      %this.quality = mClamp( mRound( $SSAOPostFx::quality ), 0, 2 );
      
      %this.setShaderMacro( "QUALITY", %this.quality );      
   }      
}

function SSAOPostFx::setShaderConsts( %this )
{      
   %this.setShaderConst( "$overallStrength", $SSAOPostFx::overallStrength );

   // Abbreviate is s-small l-large.   
   
   %this.setShaderConst( "$sRadius",      $SSAOPostFx::sRadius );
   %this.setShaderConst( "$sStrength",    $SSAOPostFx::sStrength );
   %this.setShaderConst( "$sDepthMin",    $SSAOPostFx::sDepthMin );
   %this.setShaderConst( "$sDepthMax",    $SSAOPostFx::sDepthMax );
   %this.setShaderConst( "$sDepthPow",    $SSAOPostFx::sDepthPow );
   %this.setShaderConst( "$sNormalTol",   $SSAOPostFx::sNormalTol );
   %this.setShaderConst( "$sNormalPow",   $SSAOPostFx::sNormalPow );
   
   %this.setShaderConst( "$lRadius",      $SSAOPostFx::lRadius );
   %this.setShaderConst( "$lStrength",    $SSAOPostFx::lStrength );
   %this.setShaderConst( "$lDepthMin",    $SSAOPostFx::lDepthMin );
   %this.setShaderConst( "$lDepthMax",    $SSAOPostFx::lDepthMax );
   %this.setShaderConst( "$lDepthPow",    $SSAOPostFx::lDepthPow );
   %this.setShaderConst( "$lNormalTol",   $SSAOPostFx::lNormalTol );
   %this.setShaderConst( "$lNormalPow",   $SSAOPostFx::lNormalPow );
   
   %blur = %this->blurY;
   %blur.setShaderConst( "$blurDepthTol", $SSAOPostFx::blurDepthTol );
   %blur.setShaderConst( "$blurNormalTol", $SSAOPostFx::blurNormalTol );   
   
   %blur = %this->blurX;
   %blur.setShaderConst( "$blurDepthTol", $SSAOPostFx::blurDepthTol );
   %blur.setShaderConst( "$blurNormalTol", $SSAOPostFx::blurNormalTol );   
   
   %blur = %this->blurY2;
   %blur.setShaderConst( "$blurDepthTol", $SSAOPostFx::blurDepthTol );
   %blur.setShaderConst( "$blurNormalTol", $SSAOPostFx::blurNormalTol );
      
   %blur = %this->blurX2;
   %blur.setShaderConst( "$blurDepthTol", $SSAOPostFx::blurDepthTol );
   %blur.setShaderConst( "$blurNormalTol", $SSAOPostFx::blurNormalTol );         
}

//-----------------------------------------------------------------------------
// GFXStateBlockData / ShaderData
//-----------------------------------------------------------------------------

singleton GFXStateBlockData( SSAOStateBlock : PFX_DefaultStateBlock )
{   
   samplersDefined = true;
   samplerStates[0] = SamplerClampPoint;
   samplerStates[1] = SamplerWrapLinear;
   samplerStates[2] = SamplerClampPoint;
};

singleton GFXStateBlockData( SSAOBlurStateBlock : PFX_DefaultStateBlock )
{   
   samplersDefined = true;
   samplerStates[0] = SamplerClampLinear;
   samplerStates[1] = SamplerClampPoint;
};

singleton ShaderData( SSAOShader )
{   
   DXVertexShaderFile 	= "shaders/common/postFx/postFxV.hlsl";
   DXPixelShaderFile 	= "shaders/common/postFx/ssao/SSAO_P.hlsl";            
   pixVersion = 3.0;
};

singleton ShaderData( SSAOBlurYShader )
{
   DXVertexShaderFile 	= "shaders/common/postFx/ssao/SSAO_Blur_V.hlsl";
   DXPixelShaderFile 	= "shaders/common/postFx/ssao/SSAO_Blur_P.hlsl";   
   pixVersion = 3.0;      
   
   defines = "BLUR_DIR=float2(0.0,1.0)";         
};

singleton ShaderData( SSAOBlurXShader : SSAOBlurYShader )
{
   defines = "BLUR_DIR=float2(1.0,0.0)";
};

//-----------------------------------------------------------------------------
// PostEffects
//-----------------------------------------------------------------------------

singleton PostEffect( SSAOPostFx )
{     
   allowReflectPass = false;
     
   renderTime = "PFXBeforeBin";
   renderBin = "AL_LightBinMgr";   
   renderPriority = 10;

   requirements = "PrePassDepthAndNormal";
   
   shader = SSAOShader;
   stateBlock = SSAOStateBlock;
         
   texture[0] = "#prepass";         
   texture[1] = "noise.png";
   texture[2] = "#ssao_pow_table";
   
   target = "$outTex";
   targetScale = "0.5 0.5";
   
   singleton PostEffect()
   {
      internalName = "blurY";
      
      shader = SSAOBlurYShader;
      stateBlock = SSAOBlurStateBlock;
      
      requirements = "PrePassDepthAndNormal";
      
      texture[0] = "$inTex";
      texture[1] = "#prepass";
      
      target = "$outTex"; 
   };
      
   singleton PostEffect()
   {
      internalName = "blurX";
      
      shader = SSAOBlurXShader;
      stateBlock = SSAOBlurStateBlock;
      
      requirements = "PrePassDepthAndNormal";
      
      texture[0] = "$inTex";
      texture[1] = "#prepass";
      
      target = "$outTex"; 
   };   
   
   singleton PostEffect()
   {
      internalName = "blurY2";
      
      shader = SSAOBlurYShader;
      stateBlock = SSAOBlurStateBlock;
      
      requirements = "PrePassDepthAndNormal";
      
      texture[0] = "$inTex";
      texture[1] = "#prepass";
      
      target = "$outTex"; 
   };
   
   singleton PostEffect()
   {
      internalName = "blurX2";
            
      shader = SSAOBlurXShader;
      stateBlock = SSAOBlurStateBlock;
      
      requirements = "PrePassDepthAndNormal";
      
      texture[0] = "$inTex";
      texture[1] = "#prepass";
            
      // We write to a mask texture which is then
      // read by the lighting shaders to mask ambient.
      target = "#ssaoMask";   
   };  
};


/// Just here for debug visualization of the 
/// SSAO mask texture used during lighting.
singleton PostEffect( SSAOVizPostFx )
{      
   allowReflectPass = false;
     
   requirements = "PrePassDepthAndNormal";
   
   shader = PFX_PassthruShader;
   stateBlock = PFX_DefaultStateBlock;
   
   texture[0] = "#ssaoMask";
   
   target = "$backbuffer";
};

singleton ShaderData( SSAOPowTableShader )
{
   DXVertexShaderFile 	= "shaders/common/postFx/ssao/SSAO_PowerTable_V.hlsl";
   DXPixelShaderFile 	= "shaders/common/postFx/ssao/SSAO_PowerTable_P.hlsl";            
   pixVersion = 2.0;
};

singleton PostEffect( SSAOPowTablePostFx )
{
   shader = SSAOPowTableShader;
   stateBlock = PFX_DefaultStateBlock;
   
   renderTime = "PFXTexGenOnDemand";
   
   target = "#ssao_pow_table"; 
   
   targetFormat = "GFXFormatR16F";   
   targetSize = "256 1";
};