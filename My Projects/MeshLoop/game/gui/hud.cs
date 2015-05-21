
function viewHudVisible(%state)
{
   ViewHUD.Visible = %state;
   updateStatusBar();
}

function captureScreen()
{
   if ($targetFile $= "")
      return;
      
   if ($screenShotFile $= "") {
      $screenshotFile = filePath($targetFile)@"/"@fileBase($targetFile)@fileExt($targetFile)@".screenshot.png";
   } else {
      if (fileExt($screenShotFile)!$=".png") {
         $screenShotFile = $screenShotFile @ ".png";
      }
   }
   if ($Pref::MeshLoop::bg==0) {
      View.capture($screenShotFile);
   } else {
      viewHudVisible(false);
      schedule(32, 0, "screenShot", $screenShotFile, "PNG");
      schedule(64, 0, "viewHudVisible", true);
   }
}
