
function makeDefaultMaterials()
{
   // first we generate materials for all the textures in the model's directory in case there is no
   // materials.cs file
   if ($Pref::MeshLoop::DiffuseAlphaNormalMap) {
      %transparentcode = "";
   } else {
      %transparentcode = " translucent = 0; translucentBlendOp = None; translucentZWrite = 1; alphaRef = 85; alphaTest = 1; ";
   }
   %m = getSimTime();
   %pattern = $targetDir @ "/*.jpg";
   for( %tex = findFirstFile( %pattern, false ); %tex !$= ""; %tex = findNextFile( %pattern ) )
      if (filePath(%tex)$=$targetDir) {
         %newmat = "singleton Material( \"MAT"@%m@"\" ) { mapTo = \""@fileBase(%tex)@"\"; diffuseMap[0] = \""@%tex@"\"; };";
         warn("Creating material MAT"@%m@" for "@fileBase(%tex));
         eval(%newmat);
         eval("MAT"@%m@".remap();");
         %newmatwext = "singleton Material( \"MAT"@%m@"_ext\" ) { mapTo = \""@fileBase(%tex)@fileExt(%tex)@"\"; diffuseMap[0] = \""@%tex@"\"; };";
         warn("Creating material MAT"@%m@"_ext for "@fileBase(%tex)@fileExt(%tex));
         eval(%newmatwext);
         eval("MAT"@%m@"_ext.remap();");
         %m++;
      }
   %pattern = $targetDir @ "/*.png";
   for( %tex = findFirstFile( %pattern, false ); %tex !$= ""; %tex = findNextFile( %pattern ) )
      if (filePath(%tex)$=$targetDir) {
         %newmat = "singleton Material( \"MAT"@%m@"\" ) { mapTo = \""@fileBase(%tex)@"\"; diffuseMap[0] = \""@%tex@"\"; " @ %transparentcode @ " };";
         warn("Creating material MAT"@%m@" for "@fileBase(%tex));
         eval(%newmat);
         eval("MAT"@%m@".remap();");
         %newmatwext = "singleton Material( \"MAT"@%m@"_ext\" ) { mapTo = \""@fileBase(%tex)@fileExt(%tex)@"\"; diffuseMap[0] = \""@%tex@"\"; " @ %transparentcode @ " };";
         warn("Creating material MAT"@%m@"_ext for "@fileBase(%tex)@fileExt(%tex));
         eval(%newmatwext);
         eval("MAT"@%m@"_ext.remap();");
         %m++;
      }
   %pattern = $targetDir @ "/*.dds";
   for( %tex = findFirstFile( %pattern, false ); %tex !$= ""; %tex = findNextFile( %pattern ) )
      if (filePath(%tex)$=$targetDir) {
         %newmat = "singleton Material( \"MAT"@%m@"\" ) { mapTo = \""@fileBase(%tex)@"\"; diffuseMap[0] = \""@%tex@"\"; " @%transparentcode @ " };";
         warn("Creating material MAT"@%m@" for "@fileBase(%tex));
         eval(%newmat);
         eval("MAT"@%m@".remap();");
         %newmatwext = "singleton Material( \"MAT"@%m@"_ext\" ) { mapTo = \""@fileBase(%tex)@fileExt(%tex)@"\"; diffuseMap[0] = \""@%tex@"\"; " @ %transparentcode @ " };";
         warn("Creating material MAT"@%m@"_ext for "@fileBase(%tex)@fileExt(%tex));
         eval(%newmatwext);
         eval("MAT"@%m@"_ext.remap();");
         %m++;
      }
}

function lookForMaterials()
{
   if (strstr($targetDir, "/game")!=-1) {
      $targetGameRoot = getSubStr($targetDir, 0, strstr($targetDir, "/game")+5);
      // this will be looking for a materials.cs in the current directory, in the parent directory, and all
      // child directories in the parent directory
      %cdir = filePath($targetDir@".ext");
      %pattern = %cdir @ "/*";
      for( %matcs = findFirstFile( %pattern, false ); %matcs !$= ""; %matcs = findNextFile( %pattern ) ) {
         if (isDirectory(%matcs) && %matcs !$= $targetDir) {
            parseMaterialsFile(%matcs);
         }
      }
      parseMaterialsFile(filePath($targetDir@".ext"));
      parseMaterialsFile($targetDir);
   } else {
      // this will be looking for materials.cs files in the current directory or any parent directories
      $targetGameRoot = getSubStr($targetDir, 0, strstr($targetDir, ":/")+1);
      %cdir = $targetDir;
      while (%cdir !$= $targetGameRoot && strlen(%cdir)>=strlen($target)) {
         // load the contents of the materials.cs file into %file
         parseMaterialsFile(%cdir);      
         // we need to trick the system into believing it is
         // actually reading material info from this file
         %cdir = filePath(%cdir@".ext");
      }
   }
}

function parseMaterialsFile(%cdir)
{
   %file = "";
   %inFileHandle = new FileObject();
   %inFileHandle.openForRead(%cdir @ "/materials.cs");
   %inmatcode = false;
   while(!%inFileHandle.IsEOF()) {
      %inLine = %inFileHandle.readLine();
      // remove comments
      if (strstr(%inLine, "//")!=-1) {
         %inLine = strreplace(%inLine, getSubStr(%inLine, strstr(%inLine, "//"), strlen(%inLine)), "");
      }
      if (strstr(strlwr(%inLine), "material")!=-1) {
         %inLine = strreplace(%inLine, "new ", "singleton ");
         %f = strchr(%inLine, "(");
         %l = strrchr(%inLine, ")");
         if (%f $= "" && %l $= "") {
            // this is not a material definition, ignore it
         } else {
            %dtscn = trim(getSubStr(%f, 1, strlen(%f)-strlen(%l)-1));
            if (strstr(strlwr(%inLine), "custommaterial")==-1) {
               // looks like a real material
               %inmatcode = true;
            }
            if (strstr(strlwr(%dtscn), ":")==-1) {
               $materialName = %dtscn;
            } else {
               $materialName = trim(getSubStr(%dtscn, 0, strstr(%dtscn, ":")));
               // materialName could still contain an inheritance object - get rid of that
            }
         }
      }
      if (%inmatcode) {
         // replace relative and absolute paths for basic fields
         if (  strstr(strlwr(%inLine), "diffusemap")!=-1 || 
               strstr(strlwr(%inLine), "basetex")!=-1 || 
               strstr(strlwr(%inLine), "overlaymap")!=-1 || 
               strstr(strlwr(%inLine), "overlaytex")!=-1 || 
               strstr(strlwr(%inLine), "lightmap")!=-1 || 
               strstr(strlwr(%inLine), "tonemap")!=-1 || 
               strstr(strlwr(%inLine), "detailmap")!=-1 || 
               strstr(strlwr(%inLine), "detailtex")!=-1 || 
               strstr(strlwr(%inLine), "normalmap")!=-1 || 
               strstr(strlwr(%inLine), "bumptex")!=-1 || 
               strstr(strlwr(%inLine), "specularmap")!=-1 || 
               strstr(strlwr(%inLine), "envmap")!=-1 || 
               strstr(strlwr(%inLine), "envtex")!=-1
            )
         {
            %inLine = convertParamToFullPath(%inLine, %cdir, $targetGameRoot);
         }
         %file = %file @ %inLine @ "\n";
      }
      if (strstr(%inLine, "}")!=-1) {
         if (%inmatcode) {
            // end of a material definition? could be.. eval it!
            $Con::MatFile = %cdir @ "/materials.cs";
            eval(%file);
            warn("Creating material " @ $materialName @ " for " @ $materialName.mapTo);
            // now let's force a remap for this, so it overwrites automatic materials
            $materialName.remap();
            // and make sure %file stays small
         }
         %file = "";
         %inmatcode = false;
      }
   }
   %inFileHandle.close();
   %inFileHandle.delete();
}

function removeMaterials()
{
   for(%i = 0; %i < RootGroup.getCount(); %i++)
   {
      if( RootGroup.getObject(%i).getClassName() $= "Material" )
      {
         %matname = RootGroup.getObject(%i).getName();
         if (  %matname !$= "AL_DefaultPrePassMaterial" && 
               %matname !$= "AL_DefaultShadowMaterial" && 
               %matname !$= "MLMAT_COL" && // MeshLoop collision geometry material
               %matname !$= "MLMAT_LOS" && // MeshLoop line-of-sight geometry material 
               %matname !$= "WarningMaterial") {
            warn("    * " @ RootGroup.getObject(%i).mapTo @ " (" @ %matname @ ")");
            RootGroup.remove(RootGroup.getObject(%i));
            // let's make it recursive, so it removes everything
            removeMaterials();
            break;
         }
      }
   }
}

function listMaterials()
{
   warn("Currently loaded materials:");
   for(%i = 0; %i < RootGroup.getCount(); %i++)
   {
      if( RootGroup.getObject(%i).getClassName() $= "Material" )
      {
         %matname = RootGroup.getObject(%i).getName();
         if (  %matname !$= "AL_DefaultPrePassMaterial" && 
               %matname !$= "AL_DefaultShadowMaterial" && 
               %matname !$= "WarningMaterial") {
            warn("    " @ RootGroup.getObject(%i).mapTo @ " (" @ %matname @ ")");
         }
      }
   }
}

// some default materials

// collision geometry
singleton Material(MLMAT_COL)
{
   colorMultiply[0] = "0.5 0.5 1 1";
   diffuseMap[0] = "gui/images/white";
   translucent = "1";
   translucentBlendOp = "Mul";
};

// line of sight geometry
singleton Material(MLMAT_LOS)
{
   colorMultiply[0] = "0.5 1 0.5 1";
   diffuseMap[0] = "gui/images/white";
   translucent = "1";
   translucentBlendOp = "Mul";
};
