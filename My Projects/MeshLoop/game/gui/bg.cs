
function updateBg()
{
   ViewGui.wrap = "0";
   if ($Pref::MeshLoop::bg>5) $Pref::MeshLoop::bg=0;

   if ($Pref::MeshLoop::bg==0) {
      ViewGui.setBgColor(0,0,0,0);
      ViewGui.wrap = "1";
      ViewGui.setBitmap("gui/images/bg"@$Pref::MeshLoop::bg@".jpg");
   } else if ($Pref::MeshLoop::bg==1) {
      ViewGui.setBgColor(0,0,0,0);
      ViewGui.setBitmap("gui/images/bg"@$Pref::MeshLoop::bg@".jpg");
   } else if ($Pref::MeshLoop::bg==2) {
      ViewGui.setBgColor(255,0,255,255);
      ViewGui.setBitmap("");
   } else if ($Pref::MeshLoop::bg==3) {
      ViewGui.setBgColor(0,255,0,255);
      ViewGui.setBitmap("");
   } else if ($Pref::MeshLoop::bg==4) {
      ViewGui.setBgColor(getWord($Pref::MeshLoop::userbgcolor,0)*255,getWord($Pref::MeshLoop::userbgcolor,1)*255,getWord($Pref::MeshLoop::userbgcolor,2)*255,getWord($Pref::MeshLoop::userbgcolor,3)*255);
      ViewGui.setBitmap("");
   } else if ($Pref::MeshLoop::bg==5) {
      if (isFile($Pref::MeshLoop::userbg)) {
         ViewGui.setBgColor(0,0,0,0);
         ViewGui.setBitmap($Pref::MeshLoop::userbg);
      } else {
         $Pref::MeshLoop::bg = 1;
         ViewGui.setBgColor(0,0,0,0);
         ViewGui.setBitmap("gui/images/bg"@$Pref::MeshLoop::bg@".jpg");
      }
   }

   BGMenuPopup.checkItem(0, false);
   BGMenuPopup.checkItem(1, false);
   BGMenuPopup.checkItem(2, false);
   BGMenuPopup.checkItem(3, false);
   BGMenuPopup.checkItem(4, false);
   BGMenuPopup.checkItem(5, false);
   
   BGMenuPopup.checkItem($Pref::MeshLoop::bg, true);
}

function toggleBg()
{
   $Pref::MeshLoop::bg++;
   updateBg();
   updateStatusBar();
}

function setBG(%bgid) {
   $Pref::MeshLoop::bg = %bgid;
   if (%bgid==4) {
      getColorF($Pref::MeshLoop::userbgcolor, "updateUserBGColor");
      return;
   } else if (%bgid==5) {
      %fd = new FileDialog() {
         defaultPath = $Pref::MeshLoop::lastBGPath;
         filters = "Image Files|*.png;*.jpg;*.dds";
         mustExist = true;
         multipleFiles = false;
         title = "MeshLoop - Please choose an image for the background.";
      };
      if (%fd.Execute()) {
         $Pref::MeshLoop::lastBGPath = filePath(%fd.FileName);
         $Pref::MeshLoop::userbg = %fd.FileName;
      }
   }
   updateBg();
}

function updateUserBGColor(%color)
{
   %color = setWord(%color, 3, "1");
   $Pref::MeshLoop::userbgcolor = %color;
   updateBg();
   return false;
}

