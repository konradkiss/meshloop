
function addMeshes()
{
   if ( isObject( MeshVis.array ) )
      MeshVis.array.delete();
   MeshVis-->theVisOptionsList.removeChildren();
   MeshVis-->theVisOptionsList.clear();
   
   // add mesh visibility options to the gui
   for (%i=0;%i<View.getMeshCount();%i++) {
      %meshName = View.getMeshName(%i);
      eval("if ($Pref::MeshLoop::MeshVis::"@$model@"::"@strreplace(strreplace(%meshName, "-", "_"), " ", "_")@"$=\"\" || !$Pref::MeshLoop::RememberShapeSetup) { $Pref::MeshLoop::MeshVis::"@$model@"::"@strreplace(strreplace(%meshName, "-", "_"), " ", "_")@" = 1; }");
      if ($Pref::MeshLoop::RememberShapeSetup) {
         eval("View.setMeshForceHidden(%meshName, !$Pref::MeshLoop::MeshVis::"@$model@"::"@strreplace(strreplace(%meshName, "-", "_"), " ", "_")@");");
      } else {
         eval("if ($Pref::MeshLoop::MeshVis::"@$model@"::"@strreplace(strreplace(%meshName, "-", "_"), " ", "_")@" $= \"\") $Pref::MeshLoop::MeshVis::"@$model@"::"@strreplace(strreplace(%meshName, "-", "_"), " ", "_")@" = true;");
         eval("View.setMeshForceHidden(%meshName, !$Pref::MeshLoop::MeshVis::"@$model@"::"@strreplace(strreplace(%meshName, "-", "_"), " ", "_")@");");
      }
      MeshVis.addOption( %meshName );
   }
}

function MeshVis::onWake( %this )
{
   // Create the array if it
   // doesn't already exist.
   if ( !isObject( %this.array ) )
      %this.array = new ArrayObject();

   %this.updateOptions();
}

function meshToggle(%mesh) {
   $allMeshesToggle = 0;
   eval("%val = $Pref::MeshLoop::MeshVis::"@$model@"::"@strreplace(strreplace(%mesh, "-", "_"), " ", "_")@";");
   View.setMeshForceHidden(%mesh, !%val);
}

function toggleAllMeshes() {
   if ($allMeshesToggle$="") $allMeshesToggle = 0;
   for (%i=0;%i<View.getMeshCount();%i++) {
      %meshName = View.getMeshName(%i);
      eval("$Pref::MeshLoop::MeshVis::"@$model@"::"@strreplace(strreplace(%meshName, "-", "_"), " ", "_")@" = \""@$allMeshesToggle@"\";");
      View.setMeshForceHidden(%meshName, !$allMeshesToggle);
   }
}

function MeshVis::updateOptions( %this )
{
   // First clear the stack control.
   %this-->theVisOptionsList.clear();

   // Go through all the
   // parameters in our array and
   // create a check box for each.
   for ( %i = -1; %i < %this.array.count(); %i++ )
   {
      %text = "  " @ %this.array.getValue( %i );
      
      %cmd = "meshToggle( \"" @ trim(%text) @ "\" );";
      if (!$Pref::MeshLoop::RememberShapeSetup) {
         eval("\$Pref::MeshLoop::MeshVis::"@$model@"::"@strreplace(strreplace(trim(%text), "-", "_"), " ", "_")@" = true;");
      }
      eval("%var = \"\$Pref::MeshLoop::MeshVis::"@$model@"::"@strreplace(strreplace(trim(%text), "-", "_"), " ", "_")@"\";");

      if (%i==-1) {
         // all nodes toggle
         %text = "  [ toggle all ]";
         %var = "$allMeshesToggle";
         %cmd = "toggleAllMeshes();";
      }
      %textLength = strlen( %text );
      
      %checkBox = new GuiCheckBoxCtrl()
      {
         canSaveDynamicFields = "0";
         isContainer = "0";
         Profile = "GuiCheckBoxListProfile";
         HorizSizing = "right";
         VertSizing = "bottom";
         Position = "0 0";
         Extent = (%textLength * 5) @ " 18";
         MinExtent = "8 2";
         canSave = "1";
         Visible = "1";
         Variable = %var;
         tooltipprofile = "GuiToolTipProfile";
         hovertime = "1000";
         text = %text;
         groupNum = "-1";
         buttonType = "ToggleButton";
         useMouseEvents = "0";
         useInactiveState = "0";
         Command = %cmd;
      };

      %this-->theVisOptionsList.addGuiControl( %checkBox );
   }   
}

function MeshVis::addOption( %this, %text )
{
   // Create the array if it
   // doesn't already exist.
   if ( !isObject( %this.array ) )
      %this.array = new ArrayObject();   
   
   %this.array.push_back( %text, %text );
   %this.array.uniqueKey();  
   %this.array.sortd(); 
   %this.updateOptions();
}

function MeshVis::saveDims(%this)
{
   $Pref::MeshLoop::MeshVis::Position = %this.getPosition();
   $Pref::MeshLoop::MeshVis::Extent = %this.getExtent();
}

function addLODs()
{
   $MeshLoop::LODsCount = View.getLODsCount();
   LODSlider.range = "0" SPC $MeshLoop::LODsCount-1;
   if ($MeshLoop::LODsCount>2) {
      LODSlider.ticks = $MeshLoop::LODsCount - 2;
   } else {
      LODSlider.ticks = 0;
   }
   LODSlider.setValue(0);
   LODSliderVal.text = "0";
}

function LODWindowSaveDims()
{
   $Pref::MeshLoop::LODWindow::Position = LODWindow.position;
   $Pref::MeshLoop::LODWindow::Extent = LODWindow.extent;
}

function LODSlider::onValueChanged(%this)
{
   LODSliderVal.text = %this.value;
   LODSliderSize.text = "> " @ View.getLODSize(%this.value) SPC "pixels";
   LODSliderPolys.text = View.getLODPolyCount(%this.value) SPC "polygons";
   $MeshLoop::CurrentLOD = %this.value;
}

function LODEnabled::onClick(%this)
{
   LODSlider.canControl = !$MeshLoop::AutoLOD;
}

function View::onDetailChange(%this)
{
   LODSlider.setValue($MeshLoop::CurrentLOD);
}
