
// Set the name of our application
$appName = "MeshLoop 1.05";

// we always default to basic lighting
$Pref::MeshLoop::LightingMode  = "1";

// cache collada - defaults to false since version 1.03
if ($Pref::MeshLoop::cacheCollada $= "") $Pref::MeshLoop::cacheCollada = false;

if ($Pref::MeshLoop::cacheCollada) {
   // this is needed so that we get a cached.dts when a collada file is opened
   // and its materials are also updated
   $collada::forceLoadDAE = false;
   $Pref::collada::cacheDts = true;
   $Pref::collada::updateMaterials = true;
} else {
   $collada::forceLoadDAE = true;
   $Pref::collada::cacheDts = false;
   $Pref::collada::updateMaterials = false;
}

// exit on blur
if ($Pref::MeshLoop::exitOnBlur $= "") $Pref::MeshLoop::exitOnBlur = "0";

// bg
if ($Pref::MeshLoop::bg $= "") $Pref::MeshLoop::bg = 1;
$Pref::MeshLoop::userbgcolor = "0 0 0 1";

// view mode
if ($Pref::MeshLoop::ortho $= "") $Pref::MeshLoop::ortho = false;
if ($Pref::MeshLoop::Fov::ortho $= "") $Pref::MeshLoop::Fov::ortho = 1;
if ($Pref::MeshLoop::Fov::perspective $= "") $Pref::MeshLoop::Fov::perspective = 45;

// overlays
if ($Pref::MeshLoop::showJoints $= "") $Pref::MeshLoop::showJoints = false;
if ($Pref::MeshLoop::showNodes $= "") $Pref::MeshLoop::showNodes = false;
if ($Pref::MeshLoop::showLights $= "") $Pref::MeshLoop::showLights = false;
if ($Pref::MeshLoop::showGrid $= "") $Pref::MeshLoop::showGrid = true;
if ($Pref::MeshLoop::showAxis $= "") $Pref::MeshLoop::showAxis = true;
if ($Pref::MeshLoop::showBounds $= "") $Pref::MeshLoop::showBounds = false;
if ($Pref::MeshLoop::showCol $= "") $Pref::MeshLoop::showCol = false;
if ($Pref::MeshLoop::showLOS $= "") $Pref::MeshLoop::showLOS = false;
if ($Pref::MeshLoop::showCamPos $= "") $Pref::MeshLoop::showCamPos = false;

// recent files
if ($Pref::MeshLoop::Recent1 $= "") $Pref::MeshLoop::Recent1 = "";
if ($Pref::MeshLoop::Recent2 $= "") $Pref::MeshLoop::Recent2 = "";
if ($Pref::MeshLoop::Recent3 $= "") $Pref::MeshLoop::Recent3 = "";
if ($Pref::MeshLoop::Recent4 $= "") $Pref::MeshLoop::Recent4 = "";

// various settings
if ($Pref::MeshLoop::RememberShapeSetup $= "")  $Pref::MeshLoop::RememberShapeSetup = 0;
if ($Pref::MeshLoop::RemoveNodeOverlapping $= "")  $Pref::MeshLoop::RemoveNodeOverlapping = 0;
if ($Pref::MeshLoop::DiffuseAlphaNormalMap $= "")  $Pref::MeshLoop::DiffuseAlphaNormalMap = 0;

// animation defaults
$MeshLoop::AnimationSpeed = 1.0;
$MeshLoop::FrameCount = 1;
$MeshLoop::SequencePosition = "0.00";
$MeshLoop::SequenceFramePosition = "0.00";
$MeshLoop::ActiveSequenceID = -1;
$MeshLoop::SequenceFrameMSecs = 0.0;

// LODs
$MeshLoop::AutoLOD = 1;

// cam and lights defaults
exec("./cldefaults.cs");

// window position and extent defaults
exec("./wdefaults.cs");