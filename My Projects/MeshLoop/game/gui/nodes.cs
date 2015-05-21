
function updateNodes() {
   // ...
}

function addChildNodes(%parent, %depth) {
   for (%i=0;%i<View.getNodeCount();%i++) {
      %tp = View.getParentNodeIndex(%i);
      if (%parent==%tp) {
         %nodeName = View.getNodeName(%i);
         eval("if ($Pref::MeshLoop::NodeVis::"@$model@"::"@strreplace(strreplace(%nodeName, "-", "_"), " ", "_")@"$=\"\") { $Pref::MeshLoop::NodeVis::"@$model@"::"@strreplace(strreplace(%nodeName, "-", "_"), " ", "_")@" = 1; }");
         eval("View.setNodeForceHidden(%nodeName, !$Pref::MeshLoop::NodeVis::"@$model@"::"@strreplace(strreplace(%nodeName, "-", "_"), " ", "_")@");");
         NodeVis.addOption( %nodeName, %depth );
         addChildNodes(%i, %depth+1);
      }
   }
}

function addNodes()
{
   if ( isObject( NodeVis.array ) )
      NodeVis.array.delete();
   NodeVis-->theVisOptionsList.removeChildren();
   NodeVis-->theVisOptionsList.clear();

   // add node visibility options to the gui
   // for nodes there's a tree view.. we'll need to look for any nodes with a -1 parent, and add those
   // and all children recursively
   for (%i=0;%i<View.getNodeCount();%i++) {
      %parent = View.getParentNodeIndex(%i);
      if (%parent==-1) {
         %nodeName = View.getNodeName(%i);
         eval("if ($Pref::MeshLoop::NodeVis::"@$model@"::"@strreplace(strreplace(%nodeName, "-", "_"), " ", "_")@"$=\"\") { $Pref::MeshLoop::NodeVis::"@$model@"::"@strreplace(strreplace(%nodeName, "-", "_"), " ", "_")@" = 1; }");
         if ($Pref::MeshLoop::RememberShapeSetup) {
            eval("View.setNodeForceHidden(%nodeName, !$Pref::MeshLoop::NodeVis::"@$model@"::"@strreplace(strreplace(%nodeName, "-", "_"), " ", "_")@");");
         } else {
            eval("$Pref::MeshLoop::NodeVis::"@$model@"::"@strreplace(strreplace(%nodeName, "-", "_"), " ", "_")@" = true;");
            View.setNodeForceHidden(%nodeName, false);
         }
         NodeVis.addOption( %nodeName, 0 );
         addChildNodes(%i, 1);
      }
   }
   updateNodes();
}

function NodeVis::onWake( %this )
{
   // Create the array if it
   // doesn't already exist.
   if ( !isObject( %this.array ) )
      %this.array = new ArrayObject();

   %this.updateOptions();
}

function childNodeSet(%parent, %val) {
   for (%i=0;%i<View.getNodeCount();%i++) {
      %tp = View.getParentNodeIndex(%i);
      if (%parent==%tp) {
         %node = View.getNodeName(%i);
         eval("$Pref::MeshLoop::NodeVis::"@$model@"::"@strreplace(strreplace(%node, "-", "_"), " ", "_")@" = \""@%val@"\";");
         View.setNodeForceHidden(%node, !%val);
         childNodeSet(%i, %val);
      }
   }
}

function nodeToggle(%node, %nodeid) {
   $allNodesToggle = 0;
   eval("%val = $Pref::MeshLoop::NodeVis::"@$model@"::"@strreplace(strreplace(%node, "-", "_"), " ", "_")@";");
   View.setNodeForceHidden(%node, !%val);
   childNodeSet(%nodeid, %val);
   updateNodeSkeleton();
}

function toggleAllNodes() {
   if ($allNodesToggle$="") $allNodesToggle = 0;
   for (%i=0;%i<View.getNodeCount();%i++) {
      %nodeName = View.getNodeName(%i);
      eval("$Pref::MeshLoop::NodeVis::"@$model@"::"@strreplace(strreplace(%nodeName, "-", "_"), " ", "_")@" = \""@$allNodesToggle@"\";");
      View.setNodeForceHidden(%nodeName, !$allNodesToggle);
   }
}

function NodeVis::updateOptions( %this )
{
   // First clear the stack control.
   %this-->theVisOptionsList.clear();

   // Go through all the
   // parameters in our array and
   // create a check box for each.
   for ( %i = -1; %i < %this.array.count(); %i++ )
   {
      %text = "  " @ %this.array.getKey( %i );
      %depth = %this.array.getValue( %i );
      
      %cmd = "nodeToggle( \"" @ trim(%text) @ "\", \"" @ trim(%i) @ "\" );";
      eval("%var = \"\$Pref::MeshLoop::NodeVis::"@$model@"::"@strreplace(strreplace(trim(%text), "-", "_"), " ", "_")@"\";");
      
      if (%i==-1) {
         // all nodes toggle
         %text = "  [ toggle all ]";
         %var = "$allNodesToggle";
         %cmd = "toggleAllNodes();";
         %depth = 0;
      }
      %textLength = strlen( %text );
      
      %checkBox = new GuiControl() {      
         profile = "GuiDefaultProfile";
         horizSizing = "right";
         vertSizing = "bottom";
         position = "0 0";
         extent = ((%textLength * 12)+(%depth*10)) @ " 18";
         minExtent = "18 18";
         visible = "1";
         helpTag = "0";
         useMouseEvents = "0";
         isContainer = "1";
         border = "0";

         new GuiCheckBoxCtrl()
         {
            canSaveDynamicFields = "0";
            isContainer = "0";
            Profile = "GuiCheckBoxListProfile";
            HorizSizing = "right";
            VertSizing = "bottom";
            Position = (%depth*10) SPC "0";
            Extent = (%textLength * 10) @ " 18";
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
      };

      %this-->theVisOptionsList.addGuiControl( %checkBox );
   }   
}

function NodeVis::addOption( %this, %text, %depth )
{
   // Create the array if it
   // doesn't already exist.
   if ( !isObject( %this.array ) )
      %this.array = new ArrayObject();   
   
   %this.array.push_back( %text, %depth );
   %this.array.uniqueKey();  
//   %this.array.sortd(); 
   %this.updateOptions();
}

function NodeVis::saveDims(%this)
{
   $Pref::MeshLoop::NodeVis::Position = %this.getPosition();
   $Pref::MeshLoop::NodeVis::Extent = %this.getExtent();
}
