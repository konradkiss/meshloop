//-----------------------------------------------------------------------------

exec("core/main.cs");
exec("gui/main.cs");
exec("lighting/main.cs");
exec("shaders/main.cs");
exec("prefs.cs");

// ----------------------------------------------------------------

$firstRun = true;

function exportPreferences()
{
   Canvas.saveWindowDims();
   %path = filePath($Con::File);
   absExport( "$Pref::MeshLoop::*", %path@"/prefs.cs", false );
}

function resetPreferences()
{
   %path = filePath($Con::File);
   fileDelete(%path@"/prefs.cs");
   exec("core/defaults.cs");
}

// makes sure that $targetFile is set either through a file dialog or as a param
// also gives value to all the global filename values
setTargetFile();

// Init canvas, video mode, renderManager and lighting
if (!$systemInited) {
   systemInit();
   $systemInited = true;
}

// Set the content of the guiObjectView
exec("gui/app.gui");
Canvas.setContent(ViewGui);

// Initialize the shape
initShape();
