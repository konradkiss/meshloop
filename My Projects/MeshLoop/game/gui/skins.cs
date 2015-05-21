
function addSkins()
{
   if ( isObject( SkinVis.array ) )
      SkinVis.array.delete();
   SkinVis-->theVisOptionsList.removeChildren();
   SkinVis-->theVisOptionsList.clear();

   $skinInit = false;

   eval("if ($Pref::MeshLoop::SkinVis::"@$model@"$=\"\") $Pref::MeshLoop::SkinVis::"@$model@" = \"base\";");

   %refpath = "";
  
   // add skins to the radio group
   for (%i=0;%i<View.getTargetCount();%i++) {
      %targetName = View.getTargetName(%i);
      
      // take only the part after the .
      %targetName = strreplace(fileExt(%targetName), ".", "");

      // find all possible skins for this mesh
      %cdir = $targetDir;
      while (%cdir !$= $targetGameRoot) {
         // look for skins for this mesh
         // the name of the file will look something like:
         // <skin>.<mesh>.<jpg|png|dds>
         
         %skinName = %targetName;
         
         // jpg
         %pattern = %cdir @ "/*."@%skinName@".jpg";
         for( %skn = findFirstFile( %pattern, false ); %skn !$= ""; %skn = findNextFile( %pattern ) )
            if (filePath(%skn)$=%cdir) {
               %refpath = %cdir;
               SkinVis.addOption( %targetName SPC strreplace(fileBase(%skn), "."@%skinName, ""), %cdir );
            }
         // png
         %pattern = %cdir @ "/*."@%skinName@".png";
         for( %skn = findFirstFile( %pattern, false ); %skn !$= ""; %skn = findNextFile( %pattern ) )
            if (filePath(%skn)$=%cdir) {
               %refpath = %cdir;
               SkinVis.addOption( %targetName SPC strreplace(fileBase(%skn), "."@%skinName, ""), %cdir );
            }
         // dds
         %pattern = %cdir @ "/*."@%skinName@".dds";
         for( %skn = findFirstFile( %pattern, false ); %skn !$= ""; %skn = findNextFile( %pattern ) )
            if (filePath(%skn)$=%cdir) {
               %refpath = %cdir;
               SkinVis.addOption( %targetName SPC strreplace(fileBase(%skn), "."@%skinName, ""), %cdir );
            }

         %cdir = filePath(%cdir@".ext");
      }
   }
   if ($Pref::MeshLoop::RememberShapeSetup) {
      eval("View.setSkinName($Pref::MeshLoop::SkinVis::"@$model@", \"base\", %refpath);");
   }
}

function SkinVis::onWake( %this )
{
   // Create the array if it
   // doesn't already exist.
   if ( !isObject( %this.array ) )
      %this.array = new ArrayObject();

   %this.updateOptions();
}

function skinToggle(%skin, %refpath) 
{
   eval("View.setSkinName(%skin, $Pref::MeshLoop::SkinVis::"@$model@", %refpath);");
   eval("$Pref::MeshLoop::SkinVis::"@$model@" = %skin;");
}

function SkinVis::updateOptions( %this )
{
   // First clear the stack control.
   %this-->theVisOptionsList.clear();

   // Go through all the
   // parameters in our array and
   // create a check box for each.
   for ( %i = 0; %i < %this.array.count(); %i++ )
   {
      %alltext = %this.array.getKey( %i );
      %meshname = getWord(%alltext, 0);
      
      %text = "  " @ getWord(%alltext, 1);
      %textLength = strlen( %text );
      
      %cmd = "skinToggle( \"" @ trim(%text) @ "\", \"" @ %this.array.getValue( %i ) @ "\" );";
      
      %radioBtn = new GuiCheckBoxCtrl()
      {
         canSaveDynamicFields = "0";
         isContainer = "0";
         Profile = "GuiRadioProfile";
         HorizSizing = "right";
         VertSizing = "bottom";
         Position = "0 0";
         Extent = (%textLength * 4) @ " 18";
         MinExtent = "8 2";
         canSave = "1";
         Visible = "1";
         tooltipprofile = "GuiToolTipProfile";
         hovertime = "1000";
         text = %text;
         groupNum = "0";
         buttonType = "RadioButton";
         useMouseEvents = "0";
         useInactiveState = "0";
         Command = %cmd;
      };
      
      %sel = 0;
      eval("if ($Pref::MeshLoop::SkinVis::"@$model@" $= trim(%text)) %sel=1;");
      if (%sel)
         %radioBtn.setStateOn(true);

      %this-->theVisOptionsList.addGuiControl( %radioBtn );
   }   
}

function SkinVis::addOption( %this, %text, %refpath )
{
   // don't add this, if it has a dot
   if (strstr(%text, ".")!=-1)
      return;
      
   // make sure that this is a unique tag
   %tag = getWord(%text, 1);
   eval("%seen = SkinVis.me"@%tag@";");
   if (%seen$="1")
      return;
      
   eval("SkinVis.me"@%tag@" = 1;");
      
   // Create the array if it
   // doesn't already exist.
   if ( !isObject( %this.array ) )
      %this.array = new ArrayObject();   
   
   %this.array.push_back( %text, %refpath );
   %this.array.uniqueKey();  
   %this.array.sortd(); 
   %this.updateOptions();
}

function SkinVis::saveDims(%this)
{
   $Pref::MeshLoop::SkinVis::Position = %this.getPosition();
   $Pref::MeshLoop::SkinVis::Extent = %this.getExtent();
}
