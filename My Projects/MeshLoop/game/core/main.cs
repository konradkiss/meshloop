
exec("./defaults.cs");
exec("./canvas.cs");
exec("./renderManager.cs");
exec("./materials.cs");
exec("./collada.cs");
exec("./helperfuncs.cs");

function systemInit()
{
   // Seed the random number generator.
   //setRandomSeed();
   
   // Set up networking.
   //setNetPort(0);
  
   // Start processing file change events.   
   //startFileChangeNotifications();

   // Initialize the canvas.
   initializeCanvas($targetFile);

   // Set our video mode
   Canvas.setVideoMode($Pref::MeshLoop::Width, $Pref::MeshLoop::Height+20, 0, 32, 60, 0);

   // include scripts for the renderManager and lighting, and init them
   initRenderManager();
   initLightingSystems(); 
   
   exec("lighting/postFx.cs");
   initPostEffects();   
}

function bail() {
   error("Usage: MeshLoop.exe <dts_file>");
   quit();
}
