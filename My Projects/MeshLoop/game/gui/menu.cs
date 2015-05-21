//-----------------------------------------------------------------------------
// Torque Game Engine
// Copyright (C) GarageGames.com, Inc.
//-----------------------------------------------------------------------------

function Canvas::createMenuBar(%this)
{
   if(isObject(%this.menuBar))
      return;

   %fn1 = $Pref::MeshLoop::Recent1;
   %fn2 = $Pref::MeshLoop::Recent2;
   %fn3 = $Pref::MeshLoop::Recent3;
   %fn4 = $Pref::MeshLoop::Recent4;
   
   if (strlen(%fn1)>53)
      %fn1 = getSubstr(%fn1, 0, 20)@"..."@getSubstr(%fn1, strlen(%fn1)-30, 30);
   if (strlen(%fn2)>53)
      %fn2 = getSubstr(%fn2, 0, 20)@"..."@getSubstr(%fn2, strlen(%fn2)-30, 30);
   if (strlen(%fn3)>53)
      %fn3 = getSubstr(%fn3, 0, 20)@"..."@getSubstr(%fn3, strlen(%fn3)-30, 30);
   if (strlen(%fn4)>53)
      %fn4 = getSubstr(%fn4, 0, 20)@"..."@getSubstr(%fn4, strlen(%fn4)-30, 30);
           
   // Menu bar
   %this.menuBar = new MenuBar()
   {
      dynamicItemInsertPos = 3;
      
      // File Menu
      new PopupMenu(FileMenuPopup)
      {
         superClass = "MLMenu"; 
         class = "MLFileMenu";
         internalName = "FileMenu";      
         
         barTitle = "File";
         
         item[0] = "Open..." TAB "Ctrl P" TAB  "[this].onNew();";
         item[1] = "Open .dts and .cs..." TAB "Ctrl O" TAB "[this].onOpen();";
         item[2] = "Open materials.cs..." TAB "F3" TAB "[this].onOpenMats();";
         item[3] = "Open .dsq sequence..." TAB "F4" TAB "[this].onOpenSequence();";
         item[4] = "-";
         item[5] = "Take screenshot..." TAB "F12" TAB "[this].onScreenshot();";
         item[6] = "-";
         item[7] = %fn1 TAB "" TAB "[this].onRecentFile(1);";
         item[8] = %fn2 TAB "" TAB "[this].onRecentFile(2);";
         item[9] = %fn3 TAB "" TAB "[this].onRecentFile(3);";
         item[10] = %fn4 TAB "" TAB "[this].onRecentFile(4);";
         item[11] = "-";
         item[12] = "Close" TAB "Ctrl W" TAB "[this].onClose();";
      };      

      // View Menu
      new PopupMenu(ViewMenuPopup)
      {
         superClass = "MLMenu"; 
         class = "MLViewMenu";
         internalName = "ViewMenu";      
         
         barTitle = "View";
         
         item[0] = "Perspective view" TAB "" TAB "[this].onSetView(false);updateShowAxis();" TAB !$Pref::MeshLoop::ortho;
         item[1] = "Orthogonal view" TAB "" TAB "[this].onSetView(true);updateShowAxis();" TAB $Pref::MeshLoop::ortho;
         item[2] = "-";
         item[3] = "Wireframe" TAB "" TAB "[this].onSetMode(0);";
         item[4] = "Transparent" TAB "" TAB "[this].onSetMode(1);";
         item[5] = "Textured" TAB "" TAB "[this].onSetMode(2);" TAB "1";
         item[6] = "-";
         item[7] = "Fit shape to screen" TAB "F" TAB "[this].onFit();";
         item[8] = "Reset lights" TAB "R" TAB "[this].onLightsReset();";
      };      


      // Preferences Menu
      new PopupMenu(PrefMenuPopup)
      {
         superClass = "MLMenu"; 
         class = "MLPrefMenu";
         internalName = "PrefMenu";      
         
         barTitle = "Preferences";
         
         item[0] = "Remember shape specific settings" TAB "" TAB "$Pref::MeshLoop::RememberShapeSetup=!$Pref::MeshLoop::RememberShapeSetup;PrefMenuPopup.checkItem(0,$Pref::MeshLoop::RememberShapeSetup);" TAB $Pref::MeshLoop::RememberShapeSetup;
         item[1] = "Solve overlapping nodes" TAB "" TAB "$Pref::MeshLoop::RemoveNodeOverlapping=!$Pref::MeshLoop::RemoveNodeOverlapping;PrefMenuPopup.checkItem(1,$Pref::MeshLoop::RemoveNodeOverlapping);" TAB $Pref::MeshLoop::RemoveNodeOverlapping;
         item[2] = "Render unmapped diffuse maps opaque" TAB "" TAB "$Pref::MeshLoop::DiffuseAlphaNormalMap=!$Pref::MeshLoop::DiffuseAlphaNormalMap;updateDiffuseTransparency();" TAB $Pref::MeshLoop::DiffuseAlphaNormalMap;
         item[3] = "Cache collada files as DTS" TAB "" TAB "$Pref::MeshLoop::CacheCollada=!$Pref::MeshLoop::CacheCollada;updateCacheCollada();" TAB $Pref::MeshLoop::CacheCollada;
         item[4] = "-";
         item[5] = "Reset" TAB "" TAB "resetPreferences();";
      };

      // Lighting Menu
      new PopupMenu(LightMenuPopup)
      {
         superClass = "MLMenu"; 
         class = "MLLightMenu";
         internalName = "LightMenu";      
         
         barTitle = "Lighting && Fx";
         
         item[0] = "No lighting" TAB "" TAB "$Pref::MeshLoop::LightingMode=0;updateLighting();" TAB ($Pref::MeshLoop::LightingMode==0);
         item[1] = "Basic lighting" TAB "" TAB "$Pref::MeshLoop::LightingMode=1;updateLighting();" TAB ($Pref::MeshLoop::LightingMode==1);
         item[2] = "Advanced lighting" TAB "" TAB "$Pref::MeshLoop::LightingMode=2;updateLighting();" TAB ($Pref::MeshLoop::LightingMode==2);
//         item[3] = "-";
//         item[4] = "Screen-space ambient occlusion" TAB "" TAB "$Pref::MeshLoop::SSAO=!$Pref::MeshLoop::SSAO;updateSSAO();" TAB $Pref::MeshLoop::SSAO;
      };

      // Background Menu
      new PopupMenu(BGMenuPopup)
      {
         superClass = "MLMenu"; 
         class = "MLBGMenu";
         internalName = "BGMenu";      
         
         barTitle = "Backgrounds";
         
         item[0] = "Transparent" TAB "" TAB "setBG(0);";
         item[1] = "Dark showroom" TAB "" TAB "setBG(1);";
         item[2] = "#FF00FF" TAB "" TAB "setBG(2);";
         item[3] = "#00FF00" TAB "" TAB "setBG(3);";
         item[4] = "Choose background color..." TAB "" TAB "setBG(4);";
         item[5] = "Choose background image..." TAB "" TAB "setBG(5);";
      };      

      // Overlay Menu
      new PopupMenu(OverlayMenuPopup)
      {
         superClass = "MLMenu"; 
         class = "MLOverlayMenu";
         internalName = "OverlayMenu";      
         
         barTitle = "Overlays";
         
         item[0] = "Joints" TAB "ALT J" TAB "[this].onToggleJoints();" TAB $Pref::MeshLoop::showJoints;
         item[1] = "Nodes" TAB "ALT K" TAB "[this].onToggleNodes();" TAB $Pref::MeshLoop::showNodes;
         item[2] = "Lights" TAB "ALT H" TAB "[this].onToggleLights();" TAB $Pref::MeshLoop::showLights;
         item[3] = "Grid" TAB "ALT G" TAB "[this].onToggleGrid();" TAB $Pref::MeshLoop::showGrid;
         item[4] = "Axis" TAB "ALT X" TAB "[this].onToggleAxis();" TAB $Pref::MeshLoop::showAxis;
         item[5] = "-";
         item[6] = "Bounds" TAB "ALT B" TAB "[this].onToggleBounds();" TAB $Pref::MeshLoop::showBounds;
         item[7] = "Collision geometry" TAB "ALT C" TAB "$Pref::MeshLoop::showCol=!$Pref::MeshLoop::showCol;OverlayMenuPopup.checkItem(7, $Pref::MeshLoop::showCol);" TAB $Pref::MeshLoop::showCol;
         item[8] = "Line of Sight (LOS) geometry" TAB "ALT R" TAB "$Pref::MeshLoop::showLOS=!$Pref::MeshLoop::showLOS;OverlayMenuPopup.checkItem(8, $Pref::MeshLoop::showLOS);" TAB $Pref::MeshLoop::showLOS;
      };      

      // Window Menu
      new PopupMenu(WindowMenuPopup)
      {
         superClass = "MLMenu"; 
         class = "MLWindowMenu";
         internalName = "WindowMenu";      
         
         barTitle = "Windows";
         
         item[0] = "Meshes" TAB "ALT M" TAB "[this].onToggleVis(\"Meshes\", \"MeshVis\", 0);" TAB $Pref::MeshLoop::Windows::Meshes;
         item[1] = "LODs" TAB "ALT D" TAB "[this].onToggleVis(\"LODs\", \"LODWindow\", 1);" TAB $Pref::MeshLoop::Windows::LODs;
         item[2] = "Nodes" TAB "ALT N" TAB "[this].onToggleVis(\"Nodes\", \"NodeVis\", 2);" TAB $Pref::MeshLoop::Windows::Nodes;
         item[3] = "Skins" TAB "ALT S" TAB "[this].onToggleVis(\"Skins\", \"SkinVis\", 3);" TAB $Pref::MeshLoop::Windows::Skins;
         item[4] = "Animation" TAB "ALT A" TAB "[this].onToggleVis(\"Animation\", \"AnimationVis\", 4);" TAB $Pref::MeshLoop::Windows::Animation;
         item[5] = "Light" TAB "ALT L" TAB "[this].onToggleVis(\"Light\", \"LightVis\", 5);" TAB $Pref::MeshLoop::Windows::Light;
         item[6] = "3D -> 2D" TAB "ALT P" TAB "[this].onToggleVis(\"Positioning\", \"PositioningVis\", 6);" TAB $Pref::MeshLoop::Windows::Positioning;
         item[7] = "-";
         item[8] = "Reset" TAB "Ctrl R" TAB "resetScreen();";
         //item[6] = "Materials" TAB "ALT T" TAB "[this].onToggleVis(\"Materials\", \"MaterialVis\", 6);" TAB $Pref::MeshLoop::Windows::Materials; // TODO
      };      

      // Help Menu
      new PopupMenu(HelpMenuPopup)
      {
         superClass = "MLMenu"; 
         class = "MLHelpMenu";
         internalName = "HelpMenu";      
         
         barTitle = "Help";
         
         item[0] = "About" TAB "" TAB "gotoWebPage(\"http://www.bitgap.com/?ref=MeshLoop\");";
         item[1] = "Support" TAB "" TAB "gotoWebPage(\"http://www.bitgap.com/?p=contact_us&ref=MeshLoop\");";
         item[2] = "-";
         item[3] = "TorquePowered website..." TAB "" TAB "gotoWebPage(\"http://www.torquepowered.com/?ref=MeshLoop\");";
         item[4] = "Bitgap Games website..." TAB "" TAB "gotoWebPage(\"http://www.bitgap.com/?ref=MeshLoop\");";
      };   
   };
}

function MLMenu::onNew(%this) {
   %this.onOpen();
}

function MLMenu::onOpen(%this) {
   if (openShape()) {
      setTargetFile();
      initShape();
   }
}

function MLMenu::onOpenMats(%this) {
   %fd = new FileDialog() {
      defaultPath = $Pref::MeshLoop::lastPath;
      filters = "Torque Material Files|materials.cs";
      mustExist = true;
      multipleFiles = false;
      title = "MeshLoop - Please choose a materials.cs file to open.";
   };
   if (%fd.Execute()) {
      if (strstr(filePath(%fd.FileName), "game")!=-1) {
         %targetGameRoot = getSubStr(filePath(%fd.FileName), 0, strstr(filePath(%fd.FileName), "game")+4);
      } else {
         %targetGameRoot = getSubStr(filePath(%fd.FileName), 0, strstr(filePath(%fd.FileName), ":/")+1);
      }
      $Pref::MeshLoop::lastPath = filePath(%fd.FileName);
      %inFileHandle = new FileObject();
      %inFileHandle.openForRead(%fd.FileName);
      while(!%inFileHandle.IsEOF()) {
         %inLine = %inFileHandle.readLine();
         if (strstr(strlwr(%inLine), "material")!=-1) {
            %inLine = strreplace(%inLine, "new ", "singleton ");
            %f = strchr(%inLine, "(");
            %l = strrchr(%inLine, ")");
            %dtscn = trim(getSubStr(%f, 1, strlen(%f)-strlen(%l)-1));
            $materialName = %dtscn;
         }
         // replace relative and absolute paths for basic fields
         if (  strstr(strlwr(%inLine), "diffusemap")!=-1 || 
               strstr(strlwr(%inLine), "basetex")!=-1 || 
               strstr(strlwr(%inLine), "overlaymap")!=-1 || 
               strstr(strlwr(%inLine), "overlaytex")!=-1 || 
               strstr(strlwr(%inLine), "lightmap")!=-1 || 
               strstr(strlwr(%inLine), "tonemap")!=-1 || 
               strstr(strlwr(%inLine), "detailmap")!=-1 || 
               strstr(strlwr(%inLine), "detailtex")!=-1 || 
               strstr(strlwr(%inLine), "normalmap")!=-1 || 
               strstr(strlwr(%inLine), "bumptex")!=-1 || 
               strstr(strlwr(%inLine), "specularmap")!=-1 || 
               strstr(strlwr(%inLine), "envmap")!=-1 || 
               strstr(strlwr(%inLine), "envtex")!=-1
            )
         {
            %inLine = convertParamToFullPath(%inLine, filePath(%fd.FileName), %targetGameRoot);
         }
         %file = %file @ %inLine @ "\n";
         if (strstr(%inLine, "}")!=-1) {
            // end of a material definition? could be.. eval it!
            $Con::MatFile = %fd.FileName;
            eval(%file);
            warn("Creating material " @ $materialName @ " for " @ $materialName.mapTo);
            // now let's force a remap for this, so it overwrites automatic materials
            $materialName.remap();
            // and make sure %file stays small
            %file = "";
         }
      }
      %inFileHandle.close();
      %inFileHandle.delete();
      // let's reinit the shape - just in case it was affected
      initShape(true);
      return true;
   }
   return false;
}

function MLMenu::onOpenSequence(%this) {
   %fd = new FileDialog() {
      defaultPath = $Pref::MeshLoop::lastPath;
      filters = "Torque Sequence Files|*.dsq";
      mustExist = true;
      multipleFiles = false;
      title = "MeshLoop - Please choose a sequence to open.";
   };
   if (%fd.Execute()) {
      $Pref::MeshLoop::lastPath = filePath(%fd.FileName);
      
      // add the sequence to the rest of the sequences
      %sequenceName = fileBase(%fd.FileName);
      // let's reinit the shape - just in case it was affected
      View.removeSequence(%sequenceName);
      View.addSequence(%fd.FileName, %sequenceName);
      initShape(true);
   }
}

function MLMenu::onScreenshot(%this) {
   %fd = new SaveFileDialog() {
      defaultPath = $Pref::MeshLoop::lastScreenshotPath;
      filters = "PNG Image Files|*.png";
      mustExist = false;
      multipleFiles = false;
      title = "MeshLoop - Please enter a file name to save your screenshot.";
   };
   if (%fd.Execute()) {
      $Pref::MeshLoop::lastScreenshotPath = filePath(%fd.FileName);
      $screenShotFile = %fd.FileName;
      captureScreen();
   }
}

function MLMenu::onRecentFile(%this, %pos) {
   eval("%val = $Pref::MeshLoop::Recent"@%pos@";");
   if (%val !$= "") {
      $targetFile = %val;
      addToRecentFiles($targetFile);
      setTargetFile();
      initShape();
   }
}

function loadShowJoints() {
   updateShowJoints();
}

function updateShowJoints() {
   OverlayMenuPopup.checkItem(0, $Pref::MeshLoop::showJoints);
   View.setNodeSkeletonHidden(!$Pref::MeshLoop::showJoints);
}

function MLMenu::onToggleJoints( %this ) {
   $Pref::MeshLoop::showJoints = !$Pref::MeshLoop::showJoints;
   updateShowJoints();
}

function loadShowLights() {
   updateShowLights();
}

function updateShowLights() {
   OverlayMenuPopup.checkItem(2, $Pref::MeshLoop::showLights);
}

function MLMenu::onToggleLights( %this ) {
   $Pref::MeshLoop::showLights = !$Pref::MeshLoop::showLights;
   updateShowLights();
}

function loadShowGrid() {
   updateShowGrid();
}

function updateShowGrid() {
   OverlayMenuPopup.checkItem(3, $Pref::MeshLoop::showGrid);
}

function MLMenu::onToggleGrid( %this ) {
   $Pref::MeshLoop::showGrid = !$Pref::MeshLoop::showGrid;
   updateShowGrid();
}

function loadShowAxis() {
   updateShowAxis();
}

function updateShowAxis() {
   if ($Pref::MeshLoop::ortho)
      $Pref::MeshLoop::showAxis = false;
   OverlayMenuPopup.checkItem(4, $Pref::MeshLoop::showAxis);
   Axis.Visible = $Pref::MeshLoop::showAxis;
}

function MLMenu::onToggleAxis( %this ) {
   $Pref::MeshLoop::showAxis = !$Pref::MeshLoop::showAxis;
   updateShowAxis();
}

function loadShowBounds() {
   updateShowBounds();
}

function updateShowBounds() {
   OverlayMenuPopup.checkItem(6, $Pref::MeshLoop::showBounds);
}

function MLMenu::onToggleBounds( %this ) {
   $Pref::MeshLoop::showBounds = !$Pref::MeshLoop::showBounds;
   updateShowBounds();
}

function initRenderMode() {
   if ($Pref::MeshLoop::renderMode $= "")
      $Pref::MeshLoop::renderMode = 2; // textured
   ViewMenuPopup.onSetMode( $Pref::MeshLoop::renderMode );
}

function MLMenu::onSetMode(%this, %mode) {
   if (%mode==0) {
      $gfx::wireframe = 1;
      View.transparent = false;
   } else if (%mode==1) {
      $gfx::wireframe = 0;
      View.transparent = true;
   } else {
      $gfx::wireframe = 0;
      View.transparent = false;
   }
   ViewMenuPopup.checkItem(3, false);
   ViewMenuPopup.checkItem(4, false);
   ViewMenuPopup.checkItem(5, false);

   ViewMenuPopup.checkItem(3+%mode, true);
   
   $Pref::MeshLoop::renderMode = %mode;
}


// -----------------------------------------------------

function loadShowNodes() {
   View.displayNodes = $Pref::MeshLoop::ShowNodes;
   updateShowNodes();
}

function updateShowNodes() {
   OverlayMenuPopup.checkItem(1, $Pref::MeshLoop::showNodes);
}

function MLMenu::onToggleNodes( %this ) {
   View.displayNodes = !View.displayNodes;
   $Pref::MeshLoop::showNodes = View.displayNodes;
   updateShowNodes();
}

function MLMenu::onClose(%this) {
   exportPreferences();
   quit();
}

function MLMenu::onFit(%this) {
   PositioningVisClose();
   $Pref::MeshLoop::Windows::Positioning = false;
   updateVis("Positioning", "PositionVis", 6);
   %this.onSetView(false);
   View.fitToShape();
}

function MLMenu::onLightsReset(%this) {
   exec("core/cldefaults.cs");
   if ($model !$= "") {
      eval("$Pref::MeshLoop::" @ $model @ "::camRot1 = $Pref::MeshLoop::camRot1;");
      eval("$Pref::MeshLoop::" @ $model @ "::camRot2 = $Pref::MeshLoop::camRot2;");
      eval("$Pref::MeshLoop::" @ $model @ "::camRot3 = $Pref::MeshLoop::camRot3;");
   }
}

function updateWindows() {
   updateVis("Meshes", "MeshVis", 0);
   updateVis("LODs", "LODWindow", 1);
   updateVis("Nodes", "NodeVis", 2);
   updateVis("Skins", "SkinVis", 3);
   updateVis("Animation", "AnimationVis", 4);
   updateVis("Light", "LightVis", 5);
   updateVis("Positioning", "PositioningVis", 5);
   //updateVis("Materials", "MaterialVis", 6);
}

function GuiWindowCtrl::setDims(%this, %pos, %ext)
{
   %this.setPosition(getWord(%pos, 0), getWord(%pos, 1));
   %this.setExtent(getWord(%ext, 0), getWord(%ext, 1));
   %this.position = %pos;
   %this.Extent = %ext;
}

function setupWindows() {
   // read positions and extents
   MeshVis.setDims($Pref::MeshLoop::MeshVis::Position, $Pref::MeshLoop::MeshVis::Extent);
   SkinVis.setDims($Pref::MeshLoop::SkinVis::Position, $Pref::MeshLoop::SkinVis::Extent);
   NodeVis.setDims($Pref::MeshLoop::NodeVis::Position, $Pref::MeshLoop::NodeVis::Extent);
   SequencesVis.setDims($Pref::MeshLoop::SequencesVis::Position, $Pref::MeshLoop::SequencesVis::Extent);
   LightSetupWindow.setDims($Pref::MeshLoop::LightSetupWindow::Position, $Pref::MeshLoop::LightSetupWindow::Extent);
   LODWindow.setDims($Pref::MeshLoop::LODWindow::Position, $Pref::MeshLoop::LODWindow::Extent);
   PositioningVis.setDims($Pref::MeshLoop::PositioningVis::Position, $Pref::MeshLoop::PositioningVis::Extent);
}

function updateVis(%varname, %guiname, %itempos) {
   if ($model!$="") {
      eval("%val = $Pref::MeshLoop::Windows::" @ %varname @ ";");
      eval(%guiname @ ".visible = \""@ %val @ "\";");
      WindowMenuPopup.checkItem(%itempos, %val);
      if (%varname $= "Positioning") {
         if (%val) {
            View.lockXRotation = "1";
            View.lockZRotation = "1";
            $Pref::MeshLoop::orbitDist = mRound($Pref::MeshLoop::orbitDist);
            PositioningDistanceSlider.setValue($Pref::MeshLoop::orbitDist);
         } else {
            View.lockXRotation = "0";
            View.lockZRotation = "0";
         }
      }
   } else {
      eval(%guiname @ ".visible = false;");
      WindowMenuPopup.checkItem(%itempos, false);
   }
}

function MLMenu::onToggleVis(%this, %varname, %guiname, %itempos) {
   eval("$Pref::MeshLoop::Windows::" @ %varname @ " = !$Pref::MeshLoop::Windows::" @ %varname @ ";");
   updateVis(%varname, %guiname, %itempos);
}

function Canvas::destroyMenuBar( %this )
{
   if( isObject( %this.menuBar ) )
      %this.menuBar.delete();
}

function Canvas::onCreateMenu(%this)
{
   if( !isObject( %this.menuBar ) )
      %this.createMenuBar();
      
   %this.menuBar.attachToCanvas( %this, 0 );
}

function Canvas::onDestroyMenu(%this)
{
   if( isObject( %this.menuBar ) )
   {
      %this.destroyMenuBar();
      %this.menuBar.removeFromCanvas( %this );
   }
}

function Canvas::onAdd( %this )
{
   %this.createMenuBar();
   
   %panel = new GuiPanel() { internalName = "DocumentContainer"; };
   %this.setContent( %panel );
   
   %xOffset = 20;
   %yOffset = 20;
   
   for( %i =0; %i<10; %i++ )
   {
      %window = new GuiWindowCtrl() 
      { 
         extent = "200 100"; 
         position = %xOffset SPC %yOffset;
      };
      %panel.add( %window );
      
      %xOffset += 30;
      %yOffset += 30;

   }
}

function Canvas::onRemove( %this )
{
   %this.destroyMenuBar();
}

function MLMenu::addItem(%this, %pos, %item)
{
   if(%item $= "")
      %item = %this.item[%pos];
   
   if(%item !$= %this.item[%pos])
      %this.item[%pos] = %item;
   
   %name = getField(%item, 0);
   %accel = getField(%item, 1);
   %cmd = getField(%item, 2);
   %check = getField(%item, 3);
   
   // We replace the [this] token with our object ID
   %cmd = strreplace( %cmd, "[this]", %this );
   %this.item[%pos] = setField( %item, 2, %cmd );
   
   if(isObject(%accel))
   {
      // If %accel is an object, we want to add a sub menu
      %this.insertSubmenu(%pos, %name, %accel);
   }
   else
   {
      %this.insertItem(%pos, %name !$= "-" ? %name : "", %accel);
   }
   
   if (%check $= "1") {
      %this.checkItem(%pos, true);
   }
}

function MLMenu::appendItem(%this, %item)
{
   %this.addItem(%this.getItemCount(), %item);
}

function MLMenu::onAdd(%this)
{
   if(! isObject(%this.canvas))
      %this.canvas = Canvas;
      
   for(%i = 0;%this.item[%i] !$= "";%i++)
   {
      %this.addItem(%i);
   }
}

function MLMenu::onRemove(%this)
{
   %this.removeFromMenuBar();
}

function MLMenu::onSelectItem(%this, %id, %text)
{
   %cmd = getField(%this.item[%id], 2);
   if(%cmd !$= "")
   {
      eval( %cmd );
      return true;
   }
   return false;
}

function MLMenu::setItemName( %this, %id, %name )
{
   %item = %this.item[%id];
   %accel = getField(%item, 1);
   %this.setItem( %id, %name, %accel );
}

function MLMenu::setItemCommand( %this, %id, %command )
{
   %this.item[%id] = setField( %this.item[%id], 2, %command );
}

function MLMenu::attachToMenuBar( %this )
{
   if( %this.barName $= "" )
   {
      error("MLMenu::attachToMenuBar - Menu property 'barName' not specified.");
      return false;
   }
   
   if( %this.barPosition < 0 )
   {
      error("MLMenu::attachToMenuBar - Menu " SPC %this.barName SPC "property 'barPosition' is invalid, must be zero or greater.");
      return false;
   }
   
   Parent::attachToMenuBar( %this, %this.canvas, %this.barPosition, %this.barName );
}

function MLMenu::onAttachToMenuBar(%this, %canvas, %pos, %title)
{
}

function MLMenu::onRemoveFromMenuBar(%this, %canvas)
{
}

function MLMenu::setupDefaultState(%this)
{
   for(%i = 0;%this.item[%i] !$= "";%i++)
   {
      %name = getField(%this.item[%i], 0);
      %accel = getField(%this.item[%i], 1);
      %cmd = getField(%this.item[%i], 2);
      
      // Pass on to sub menus
      if(isObject(%accel))
         %accel.setupDefaultState();
   }
}

function MLMenu::enableAllItems(%this, %enable)
{
   for(%i = 0; %this.item[%i] !$= ""; %i++)
   {
      %this.enableItem(%i, %enable);
   }
}

function MeshVisClose()
{
   $Pref::MeshLoop::Windows::Meshes = false;
   updateWindows();
}

function NodeVisClose()
{
   $Pref::MeshLoop::Windows::Nodes = false;
   updateWindows();
}

function SkinVisClose()
{
   $Pref::MeshLoop::Windows::Skins = false;
   updateWindows();
}

function PositioningVisClose()
{
   $Pref::MeshLoop::Windows::Positioning = false;
   updateWindows();
}

function SequencesVisClose()
{
   SequencesVis.visible = false;
}

function LightSetupClose()
{
   setCam(0);
}

function LODClose()
{
   $Pref::MeshLoop::Windows::LODs = false;
   updateWindows();
}

function updateLighting()
{
   LightMenuPopup.checkItem(0,false);
   LightMenuPopup.checkItem(1,false);
   LightMenuPopup.checkItem(2,false);
   LightMenuPopup.checkItem($Pref::MeshLoop::LightingMode,true);
   initLightingSystems();
}

function updateSSAO()
{
   LightMenuPopup.checkItem(4,$Pref::MeshLoop::SSAO);
   SSAOPostFx.isEnabled = $Pref::MeshLoop::SSAO;
}

function updateDiffuseTransparency()
{
   PrefMenuPopup.checkItem(2,$Pref::MeshLoop::DiffuseAlphaNormalMap);
   initShape(true);
}

function updateCacheCollada()
{
   PrefMenuPopup.checkItem(3,$Pref::MeshLoop::CacheCollada);
}

function resetScreen()
{
   schedule(0, 0, "resetCanvas");
   schedule(500, 0, "resetWindows");
   schedule(1000, 0, "resetCanvas");
   schedule(1500, 0, "resetWindows");
   View.schedule(2000, "updateLights");
}

function resetCanvas()
{
   exec("core/wdefaults.cs");
   Canvas.setVideoMode($Pref::MeshLoop::Width, $Pref::MeshLoop::Height+20, 0, 32, 60, 0);
}

function resetWindows()
{
   setupWindows();
   MeshVis.forceResize();
   SkinVis.forceResize();
   NodeVis.forceResize();
   SequenceVis.forceResize();
   LightSetupWindow.forceResize();
   LODWindow.forceResize();
   PositioningVis.forceResize();
}
