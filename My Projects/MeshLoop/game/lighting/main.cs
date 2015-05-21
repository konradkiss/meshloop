//---------------------------------------------------------------------------------------------
// Torque Game Engine Advanced
// Copyright (C) GarageGames.com, Inc.
//---------------------------------------------------------------------------------------------

function initLightingSystems()
{
   //displaySplashWindow(true);
   
   // advanced lighting
   exec( "./shaders.cs" );
   exec( "./lightViz.cs" );
   exec( "./shadowViz.cs" );
   exec( "./shadowViz.gui" );
   // basic lighting
   exec( "./blinit.cs" );
   
   if ($Pref::MeshLoop::LightingMode == 2) {
      if (!setAdvancedLighting()) {
         $Pref::MeshLoop::LightingMode = 1;
         setBasicLighting();
      }
   } else {
      setBasicLighting();
   }
   
   //displaySplashWindow(false);
}

function setAdvancedLighting()
{
   return setLightManager( "Advanced Lighting" );
}

function onLightManagerActivate( %lmName )
{
   $pref::lightManager = %lmName;
   echo( "Using " @ $pref::lightManager );
}