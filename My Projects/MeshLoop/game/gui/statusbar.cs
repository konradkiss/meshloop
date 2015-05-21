
function updateStatusBar() {
   %vm = Canvas.getVideoMode();
   $Pref::MeshLoop::Width = getWord(%vm, 0);
   $Pref::MeshLoop::Height = getWord(%vm, 1);
   StatusBar.setText($Pref::MeshLoop::Width @ "x" @ $Pref::MeshLoop::Height @ ($Pref::MeshLoop::ortho ? " orthogonal view" : " perspective view"));
}
