
function colladaCheck()
{
   return; 
   // if the model was a .dae, we will need to convert it to dts...
   // the collada loader will generate a materials.cs for it if there's none yet
   if ($targetExt $= ".dae") {
      if (colladaToDTS($targetFile)) {
         $targetFile = $targetDir @ "/" @ $targetBase @ ".cached.dts";
         $targetDir = filePath($targetFile);
         $targetExt = fileExt($targetFile);
         $targetBase = fileBase($targetFile);
         $targetDriveLetter = getSubStr($targetDir, 0, 1);
      }
   }
}
