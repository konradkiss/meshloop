//---------------------------------------------------------------------------------------------
// Torque 3D
// Copyright (C) GarageGames.com, Inc.
//---------------------------------------------------------------------------------------------


// Vector Light State
new GFXStateBlockData( AL_VectorLightState )
{
   blendDefined = true;
   blendEnable = true;
   blendSrc = GFXBlendOne;
   blendDest = GFXBlendOne;
   blendOp = GFXBlendOpAdd;
   
   zDefined = true;
   zEnable = false;
   zWriteEnable = false;

   samplersDefined = true;
   samplerStates[0] = SamplerClampPoint;  // G-buffer
   samplerStates[1] = SamplerClampLinear; // Shadow Map
   samplerStates[3] = SamplerWrapPoint; // Random Direction Map
   
   cullDefined = true;
   cullMode = GFXCullNone;
   
   stencilDefined = true;
   stencilEnable = true;
   stencilFailOp = GFXStencilOpKeep;
   stencilZFailOp = GFXStencilOpKeep;
   stencilPassOp = GFXStencilOpKeep;
   stencilFunc = GFXCmpLess;
   stencilRef = 0;
};

// Vector Light Material
new ShaderData( AL_VectorLightShader )
{
   DXVertexShaderFile = "shaders/common/lighting/advanced/farFrustumQuadV.hlsl";
   DXPixelShaderFile  = "shaders/common/lighting/advanced/vectorLightP.hlsl";

   OGLVertexShaderFile = "shaders/common/lighting/advanced/gl/farFrustumQuadV.glsl";
   OGLPixelShaderFile  = "shaders/common/lighting/advanced/gl/vectorLightP.glsl";
   
   samplerNames[0] = "$prePassBuffer";
   samplerNames[1] = "$ShadowMap";
   samplerNames[3] = "$gTapRotationTex";
   
   pixVersion = 3.0;
};

new CustomMaterial( AL_VectorLightMaterial )
{
   shader = AL_VectorLightShader;
   stateBlock = AL_VectorLightState;
   
   texture[0] = "#prepass";
   texture[1] = "$dynamiclight";
   
   target = "lightinfo";
   
   pixVersion = 3.0;
};

//------------------------------------------------------------------------------

// Convex-geometry light states
new GFXStateBlockData( AL_ConvexLightState )
{
   blendDefined = true;
   blendEnable = true;
   blendSrc = GFXBlendOne;
   blendDest = GFXBlendOne;
   blendOp = GFXBlendOpAdd;
   
   zDefined = true;
   zEnable = true;
   zWriteEnable = false;
   zFunc = GFXCmpGreaterEqual;

   samplersDefined = true;
   samplerStates[0] = SamplerClampPoint;  // G-buffer
   samplerStates[1] = SamplerClampLinear; // Shadow Map
   samplerStates[2] = SamplerClampPoint;  // Scratch buffer
   samplerStates[3] = SamplerWrapPoint;   // Random Direction Map
   
   cullDefined = true;
   cullMode = GFXCullCW;
   
   stencilDefined = true;
   stencilEnable = true;
   stencilFailOp = GFXStencilOpKeep;
   stencilZFailOp = GFXStencilOpKeep;
   stencilPassOp = GFXStencilOpKeep;
   stencilFunc = GFXCmpLess;
   stencilRef = 0;
};

new GFXStateBlockData( AL_PointLightState : AL_ConvexLightState )
{
   samplerStates[1] = SamplerClampPoint; // Shadow Map
};

// Point Light Material
new ShaderData( AL_PointLightShader )
{
   DXVertexShaderFile = "shaders/common/lighting/advanced/convexGeometryV.hlsl";
   DXPixelShaderFile  = "shaders/common/lighting/advanced/pointLightP.hlsl";

   OGLVertexShaderFile = "shaders/common/lighting/advanced/gl/convexGeometryV.glsl";
   OGLPixelShaderFile  = "shaders/common/lighting/advanced/gl/pointLightP.glsl";
   
   samplerNames[0] = "$prePassBuffer";
   samplerNames[1] = "$shadowMap";
   samplerNames[2] = "$scratchTarget";
   samplerNames[3] = "$gTapRotationTex";
      
   pixVersion = 3.0;
};

new CustomMaterial( AL_PointLightMaterial )
{
   shader = AL_PointLightShader;
   stateBlock = AL_PointLightState;
   
   texture[0] = "#prepass";
   texture[1] = "$dynamiclight";
   
   target = "lightinfo";
   
   pixVersion = 3.0;
};

// Spot Light Material
new ShaderData( AL_SpotLightShader )
{
   DXVertexShaderFile = "shaders/common/lighting/advanced/convexGeometryV.hlsl";
   DXPixelShaderFile  = "shaders/common/lighting/advanced/spotLightP.hlsl";

   OGLVertexShaderFile = "shaders/common/lighting/advanced/gl/convexGeometryV.glsl";
   OGLPixelShaderFile  = "shaders/common/lighting/advanced/gl/spotLightP.glsl";
   
   samplerNames[0] = "$prePassBuffer";
   samplerNames[1] = "$shadowMap";
   samplerNames[2] = "$scratchTarget";
   samplerNames[3] = "$gTapRotationTex";
   
   pixVersion = 3.0;
};

new CustomMaterial( AL_SpotLightMaterial )
{
   shader = AL_SpotLightShader;
   stateBlock = AL_ConvexLightState;
   
   texture[0] = "#prepass";
   texture[1] = "$dynamiclight";
   
   target = "lightinfo";
   
   pixVersion = 3.0;
};

/// This material is used for generating prepass 
/// materials for objects that do not have materials.
new Material( AL_DefaultPrePassMaterial )
{
   // We need at least one texture else it 
   // won't create a proper material instance.
   diffuseMap[0] = "white";  
};

/// This material is used for generating shadow 
/// materials for objects that do not have materials.
new Material( AL_DefaultShadowMaterial )
{
   // We need at least one texture else it 
   // won't create a proper material instance.
   diffuseMap[0] = "white";
   
   // This is here mostly for terrain which uses
   // this material to create its shadow material.
   //
   // At sunset/sunrise the sun is looking thru 
   // backsides of the terrain which often are not
   // closed.  By changing the material to be double
   // sided we avoid holes in the shadowed geometry.
   //   
   doubleSided = true;   
};
