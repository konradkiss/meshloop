//-----------------------------------------------------------------------------
// Torque 3D
// Copyright (C) GarageGames.com, Inc.
//-----------------------------------------------------------------------------


struct Vert
{
   // .xyz = imposter center
   // .w = billboard corner index
	float4 center : POSITION;
	
   // .x = half size
   // .y = alpha fade out
   // .z = object scale
   float3 miscCoord : TEXCOORD0;
   
   // .xyzw = object orientation quaternion
   float4 rotQuat : TEXCOORD1;
};


struct Conn
{
	float4 position      : POSITION;
	float2 texCoord      : TEXCOORD0;
   float2 bumpCoord     : TEXCOORD1;

   #ifdef TORQUE_ADVANCED_LIGHTING
   
      #ifdef TORQUE_PREPASS

         float4 wsEyeVec : TEXCOORD2;
         float3x3 worldToTangent : TEXCOORD4;

      #else

         float4 lightCoord : TEXCOORD2;

      #endif

   #else

      float3 lightVec : TEXCOORD2;

   #endif

   float fade : TEXCOORD3;

};

static float sHalfPi = 1.57079632f;
static float sPi     = 3.14159265f;
static float sTwoPi  = 6.28318530f;

static float sCornerRight[4] = { -1, 1, 1, -1 };
static float sCornerUp[4] = { -1, -1, 1, 1 };
    
static float2 sUVCornerExtent[4] =
{ 
   float2( 0, 1 ),
   float2( 1, 1 ), 
   float2( 1, 0 ), 
   float2( 0, 0 )
};
                   
#define MAX_NUM_UVS 64

float3x3 quatToMat( float4 quat )
{
   float xs = quat.x * 2.0f;
   float ys = quat.y * 2.0f;
   float zs = quat.z * 2.0f;

   float wx = quat.w * xs;
   float wy = quat.w * ys;
   float wz = quat.w * zs;
   
   float xx = quat.x * xs;
   float xy = quat.x * ys;
   float xz = quat.x * zs;
   
   float yy = quat.y * ys;
   float yz = quat.y * zs;
   float zz = quat.z * zs;
   
   float3x3 mat;
   
   mat[0][0] = 1.0f - (yy + zz);
   mat[0][1] = xy - wz;
   mat[0][2] = xz + wy;

   mat[1][0] = xy + wz;
   mat[1][1] = 1.0f - (xx + zz);
   mat[1][2] = yz - wx;

   mat[2][0] = xz - wy;
   mat[2][1] = yz + wx;
   mat[2][2] = 1.0f - (xx + yy);   

   return mat;
}

Conn main(  Vert In, 
            uniform float4x4 modelViewProj : register(C0),

	         uniform float3 camPos : register(C6),	         
	         uniform float3 camRight : register(C7),	         
	         uniform float3 camUp : register(C8),
           
            uniform float4 params : register(C30),

            #ifdef TORQUE_ADVANCED_LIGHTING

            #else
               uniform float3 sunDir : register(C11),
            #endif

	         uniform float4 uvs[MAX_NUM_UVS] : register(C32) )
{
   // Pull out the parameters for clarity.   
   int      numEquatorSteps   = params.y;
   int      numPolarSteps     = params.x;
   float    polarAngle        = params.z;
   bool     includePoles      = params.w;
   
   int      corner      = In.center.w;
   float    halfSize    = In.miscCoord.x;
   float    fade        = In.miscCoord.y;
   float    scale       = In.miscCoord.z;
   
   // Get the imposter object transform.
   float3x3 mat = quatToMat( In.rotQuat );
   
   float equatorStepSize = sTwoPi / numEquatorSteps;
   float equatorHalfStep = ( equatorStepSize / 2 ) - 0.0001;

   float polarStepSize = sPi / numPolarSteps;
   float polarHalfStep = ( polarStepSize / 2 ) - 0.0001;

   // The vector between the camera and the billboard.
   float3 lookVec = normalize( camPos.xyz - In.center.xyz );

   // Generate the camera up and right vectors from
   // the object transform and camera forward.
   camUp = float3( mat[0][2], mat[1][2], mat[2][2] );   
   camRight = normalize( cross( -lookVec, camUp ) );

   // The billboarding is based on the camera directions.
   float3 rightVec   = camRight * sCornerRight[corner];
   float3 upVec      = camUp * sCornerUp[corner];

   float lookPitch = acos( dot( float3( mat[0][2], mat[1][2], mat[2][2] ), lookVec ) );

   // First check to see if we need to render the top billboard.   
   int index;
   /*
   if ( includePoles && ( lookPitch < polarAngle || lookPitch > sPi - polarAngle ) )
   {
      index = numEquatorSteps * 3; 

      // When we render the top/bottom billboard we always use
      // a fixed vector that matches the rotation of the object.
      rightVec = float3( 1, 0, 0 ) * sCornerRight[corner];
      upVec = float3( 0, 1, 0 ) * sCornerUp[corner];

      if ( lookPitch > sPi - polarAngle )
      {
         upVec = -upVec;
         index++;
      }
   }
   else
   */
   {
      // Calculate the rotation around the z axis then add the
      // equator half step.  This gets the images to switch a
      // half step before the captured angle is met.
      float lookAzimuth = atan2( lookVec.y, lookVec.x );
      float azimuth = atan2( mat[1][0], mat[0][0] );
      float rotZ = ( lookAzimuth - azimuth ) + equatorHalfStep;

      // The y rotation is calculated from the look vector and 
      // the object up vector.
      float rotY = lookPitch - polarHalfStep;

      // TODO: How can we do this without conditionals?
      // Normalize the result to 0 to 2PI.
      if ( rotZ < 0 )
         rotZ += sTwoPi;
      if ( rotZ > sTwoPi )
         rotZ -= sTwoPi;
      if ( rotY < 0 )
         rotY += sTwoPi;
      if ( rotY > sPi )
         rotY -= sTwoPi;
           
      float polarIdx = round( abs( rotY ) / polarStepSize );

      // Get the index to the start of the right polar
      // images for this viewing angle.
      int numPolarOffset = numEquatorSteps * polarIdx;
      
      // Calculate the final image index for lookup 
      // of the texture coords.
      index = ( rotZ / equatorStepSize ) + numPolarOffset;
   }

   //center = mul( mat, float4( center.x, center.y, center.z, 1.0f ) ).xyz;

   // Figure out the final point.         
   float4 finalPt;
   finalPt.xyz = In.center + ( upVec * halfSize ) + ( rightVec * halfSize );
   finalPt.w = 1;

   // Grab the uv set and setup the texture coord.
   float4 uvSet = uvs[index];
   float2 texCoord;
   texCoord.x = uvSet.x + ( uvSet.z * sUVCornerExtent[corner].x );
   texCoord.y = uvSet.y + ( uvSet.w * sUVCornerExtent[corner].y );

   Conn Out;               
   Out.position = mul( modelViewProj, finalPt );
   Out.texCoord = texCoord;
   Out.bumpCoord = texCoord;

   Out.fade = fade;

   #ifdef TORQUE_ADVANCED_LIGHTING

      #ifdef TORQUE_PREPASS

         Out.wsEyeVec = finalPt - float4( camPos, 0.0 );
         Out.worldToTangent = transpose( mat );

      #else
   
         Out.lightCoord = Out.position;

      #endif
 
   #else

      // The normal map is object space... so we just 
      // need to transform the light vector by the 
      // inverse object transform.
      //
      // Lucky for us the inverse of a rotation matrix
      // is its transpose.
      Out.lightVec = mul( transpose( mat ), sunDir );

   #endif

   return Out;
}

