
function initMaterials() {
   if (($targetExt $= ".dts") || ($targetExt $= ".dae")) {
      // clear materials
      warn("Removing materials:");
      removeMaterials();
      
      // create default materials for the textures in the directory
      if (!isFile($targetDir @ "/materials.cs"))
         makeDefaultMaterials();

      // Include materials for the model - each one up to the root dir
      lookForMaterials();
      
      // list materials for debugging
      listMaterials();
   }
}

function initShape(%noreload) { // noreload is not used currently

   //displaySplashWindow(true);

   $MeshLoop::ActiveSequenceID = -1;
   
   if (($targetExt $= ".dts") || ($targetExt $= ".dae")) {

      // If we want to work on a collada model, we cache it as a dts and load that
      colladaCheck();
      
      // find our materials
      initMaterials();
      
      // Set the target file as the viewer model
      View.setModel($targetFile);
      $model = getStringMD5($targetFile);
      
      // Update the camera
      GuiCamBtn.setStateOn(true);
      setCam(0);
      updateCam();

      // Add mesh, skin and node lists
      addMeshes();
      addLODs();
      addSkins();
      addNodes();
      addAnimations();
   }

   loadShowJoints();
   loadShowNodes();
   loadShowBounds();
   loadShowLights();

   updateBg();
   updateWindows();
   if ($firstRun) setupWindows();
   
   initRenderMode();
   
   SpeedSlider.defaultValue = $MeshLoop::AnimationSpeed;
   SpeedSlider.setValue($MeshLoop::AnimationSpeed);

   //displaySplashWindow(false);
   
   $firstRun = false;
}

function addToRecentFiles(%fn) {
   %fn1 = $Pref::MeshLoop::Recent1;
   %fn2 = $Pref::MeshLoop::Recent2;
   %fn3 = $Pref::MeshLoop::Recent3;
   %fn4 = $Pref::MeshLoop::Recent4;
   if (%fn$=%fn1 || %fn$=%fn2 || %fn$=%fn3 || %fn$=%fn4) {
      if (%fn$=%fn2) {
         %fn2 = %fn1;
         %fn1 = %fn;
      } else if (%fn$=%fn3) {
         %fn3 = %fn2;
         %fn2 = %fn1;
         %fn1 = %fn;
      } else if (%fn$=%fn4) {
         %fn4 = %fn3;
         %fn3 = %fn2;
         %fn2 = %fn1;
         %fn1 = %fn;
      }
   } else {
      // new recent file;
      %fn4 = %fn3;
      %fn3 = %fn2;
      %fn2 = %fn1;
      %fn1 = %fn;
   }
   $Pref::MeshLoop::Recent1 = %fn1;
   $Pref::MeshLoop::Recent2 = %fn2;
   $Pref::MeshLoop::Recent3 = %fn3;
   $Pref::MeshLoop::Recent4 = %fn4;
   
   if (strlen(%fn1)>53)
      %fn1 = getSubstr(%fn1, 0, 20)@"..."@getSubstr(%fn1, strlen(%fn1)-30, 30);
   if (strlen(%fn2)>53)
      %fn2 = getSubstr(%fn2, 0, 20)@"..."@getSubstr(%fn2, strlen(%fn2)-30, 30);
   if (strlen(%fn3)>53)
      %fn3 = getSubstr(%fn3, 0, 20)@"..."@getSubstr(%fn3, strlen(%fn3)-30, 30);
   if (strlen(%fn4)>53)
      %fn4 = getSubstr(%fn4, 0, 20)@"..."@getSubstr(%fn4, strlen(%fn4)-30, 30);
   
   FileMenuPopup.setItemName(7, %fn1);
   FileMenuPopup.setItemName(8, %fn2);
   FileMenuPopup.setItemName(9, %fn3);
   FileMenuPopup.setItemName(10, %fn4);
}

function openShape() {
   %fd = new FileDialog() {
      defaultPath = $Pref::MeshLoop::lastPath;
      filters = "Torque Shape Files|*.dts;*.dae";
      mustExist = true;
      multipleFiles = false;
      title = "MeshLoop - Please choose a shape to open.";
   };
   if (%fd.Execute()) {
      // save preferences of the previous file
      if ($targetFile !$= "")
         exportPreferences();
      // set the new file to load         
      $Pref::MeshLoop::lastPath = filePath(%fd.FileName);
      $targetFile = %fd.FileName;
      // add this to the recent files list
      addToRecentFiles($targetFile);
      return true;
   } else {
      return false;
   }
}

function setTargetFile()
{
   if ($targetFile $= "") {
      if ($Game::argc<2) {
         // no target file specified!
         $targetFile = "";
      } else {
         $targetFile = "";
         for (%i=1;%i<$Game::argc;%i++) {
            $targetFile = $targetFile SPC $Game::argv[%i];      
         }
      }
   }
   if (isObject(Canvas))
      Canvas.setWindowTitle($appName @ " - " @ $targetFile);

   $targetFile = strreplace(trim($targetFile), "\"", "");
   $targetFile = strreplace($targetFile, "\\", "/");
   $targetDir = filePath($targetFile);
   $targetExt = fileExt($targetFile);
   $targetBase = fileBase($targetFile);
   $targetDriveLetter = getSubStr($targetDir, 0, 1);
   $Pref::MeshLoop::lastPath = filePath($targetFile);
   
   // try loading a TSShapeConstructor script
   %constructor = $targetDir @ "/" @ $targetBase @ ".cs";
   readSequences(%constructor);
}

function convertParamToFullPath(%inLine, %cdir, %gameroot) {
   if (strlen(%inLine) == 0)
      return "";

   // remove comments
   if (strstr(%inLine, "//")!=-1) {
      %inLine = strreplace(%inLine, getSubStr(%inLine, strstr(%inLine, "//"), strlen(%inLine)), "");
   }

   // replace backslashes with slashes
   %inLine = strreplace( %inLine, "\\", "/");
   
   // fix a weird collada generated material problem 
   // if this is not a full directory path
   if (strstr(%inLine, ":/")==-1) {
      %inLine = strreplace( %inLine, $targetDriveLetter @ ":", "");
   }

   // root dir path
   if (strchr(%inLine, "~")!$="") {
      // it has a tilde character which represents a game dir top level folder
      // let's subtract the path
      %fq = strchr(%inLine, "\"");
      %lq = strrchr(%inLine, "\"");
      %ilparam = getSubStr(%fq, 1, strlen(%fq)-strlen(%lq)-1);
      %oparam = %ilparam;
      // cut that tilde off
      %ilparam = strreplace( %ilparam, "~/", "");
      // now go through targetgameroot and try finding the correct directory
      %inLinePattern = %gameroot@"/*";
      // we need to make sure this path exists, so let's find the most likely directory
      for( %directory = findFirstFile( %inLinePattern, false ); %directory !$= ""; %directory = findNextFile( %inLinePattern ) ) {
         if (IsDirectory(%directory)) {
            %tf = %directory @ "/" @ %ilparam;
            if (isFile(%tf)) {
               %ilparam = %tf;
               break;
            } else if (fileExt(%tf) $= "") {
               if (isFile(%tf @ ".jpg") || isFile(%tf @ ".png") || isFile(%tf @ ".dds") || isFile(%tf @ ".tga") || isFile(%tf @ ".bmp") || isFile(%tf @ ".jpeg")) {
                  %ilparam = %tf;
                  break;
               }
            }
         }
      }
      %inLine = strreplace(%inLine, %oparam, %ilparam);
   } else {
      // convert the end, so its not disturbed
      %inLine = strreplace( %inLine, "\";", "<!--END-->");
      %orig = %inLine;
      
      // relative path
      %inLine = strreplace( %inLine, "\".", "\""@%cdir);
     
      // no signs - root path
      if (%orig $= %inLine) {
         if (strstr(%inLine, "/")!=-1) {
            %inLine = strreplace( %inLine, "\"", "\""@%gameroot@"/");
         } else {
            %inLine = strreplace( %inLine, "\"", "\""@%cdir@"/");
         }
      }
      
      // convert that end back              
      %inLine = strreplace( %inLine, "<!--END-->", "\";");
   }

   // make sure it is not an empty path string, otherwise make the entire row empty
   if (strstr(strlwr(%inLine), "\"\"")!=-1) {
      %inLine = "";
   }
   
   return %inLine;
}
