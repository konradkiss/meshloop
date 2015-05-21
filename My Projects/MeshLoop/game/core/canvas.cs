
function createCanvas(%file)
{
   // Create the Canvas
   %foo = new GuiCanvas(Canvas);
   
   // Set the window title
   if (isObject(Canvas))
      Canvas.setWindowTitle($appName @ " - " @ %file);
   
   return true;
}

function initializeCanvas(%file)
{
   if (!createCanvas(%file))
   {
      error("Canvas creation failed. Shutting down.");
      quit();
   }
}

//---------------------------------------------------------------------------------------------
// resetCanvas
// Forces the canvas to redraw itself.
//---------------------------------------------------------------------------------------------
function resetCanvas()
{
   if (isObject(Canvas))
      Canvas.repaint();
}

function GuiCanvas::onLoseFocus(%this)
{
   exportPreferences();
   if ($Pref::MeshLoop::exitOnBlur)
      quit();
}

function Canvas::onWindowClose()
{
   exportPreferences();
   quit();
}

function Canvas::onResize(%this) {
   updateStatusBar();
   View.updateLights();
   // save window positions and extents 5 seconds from now
   // hackery to leave time for the controls to update
}

function LightSetupWindowSaveDims()
{
   $Pref::MeshLoop::LightSetupWindow::Position = LightSetupWindow.position;
   $Pref::MeshLoop::LightSetupWindow::Extent = LightSetupWindow.extent;
}

function Canvas::saveWindowDims(%this)
{
   if ($systemInited) {
      MeshVis.saveDims();
      SkinVis.saveDims();
      NodeVis.saveDims();
      SequencesVis.saveDims();
      LightSetupWindowSaveDims();
      LODWindowSaveDims();
      PositioningVis.saveDims();
   }
}
