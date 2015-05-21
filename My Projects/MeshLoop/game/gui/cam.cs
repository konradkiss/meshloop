
function GuiObjectView::changeView(%this, %isortho)
{
   if (%isortho) {
      setCam(0);
   }
   
   $Pref::MeshLoop::ortho = %isortho;

   ViewMenuPopup.checkItem(0, !%isortho);
   ViewMenuPopup.checkItem(1, %isortho);

   updateCam();
}

function MLMenu::onSetView(%this, %isortho)
{
   View.changeView(%isortho);
}

function toggleCam()
{
   %mfov = $Pref::MeshLoop::ortho;
   $Pref::MeshLoop::ortho = !$Pref::MeshLoop::ortho;
   updateCam();
}

function updateCam() {
   if ($Pref::MeshLoop::RememberShapeSetup) {
      %mfov = $Pref::MeshLoop::ortho;
      %ofov = $Pref::MeshLoop::Fov::ortho;
   }
   if (%mfov $= "") %mfov = $Pref::MeshLoop::ortho;
   if (%ofov $= "") %ofov = $Pref::MeshLoop::Fov::ortho;
   View.setOrtho(%mfov);
   View.setFov(%mfov ? %ofov : $Pref::MeshLoop::Fov::perspective);
   $Pref::MeshLoop::ortho = %mfov;
   $Pref::MeshLoop::Fov::ortho = %ofov;

   if ($targetFile !$= "") {
      View.setOrbitDistance(%mfov ? 10 : $Pref::MeshLoop::orbitDist);
      View.setModelRotation($Pref::MeshLoop::modelRotation $= "" ? 0 : $Pref::MeshLoop::modelRotation);
      if (%mfov) {
         //PositioningDistanceSlider.setValue(mRound($Pref::MeshLoop::Fov::ortho * 2));
         PositioningDistanceSliderVal.setValue("FOV: " @ mFloatLength($Pref::MeshLoop::Fov::ortho, 1) @ " degrees");
      } else {
         //PositioningDistanceSlider.setValue(mRound($Pref::MeshLoop::orbitDist));
         PositioningDistanceSliderVal.setValue("Distance: " @ mRound($Pref::MeshLoop::orbitDist) @ " units");
      }
      //PositioningRotationSlider.setValue(mRound($Pref::MeshLoop::modelRotation));
      PositioningRotationSliderVal.setValue("Rotation: " @ mRound($Pref::MeshLoop::modelRotation) @ " degrees");
      PositioningElevationSliderVal.setValue("Elevation: " @ mRound($Pref::MeshLoop::modelElevation) @ " degrees");
   }
   updateStatusBar();
}
