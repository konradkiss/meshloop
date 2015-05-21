
function addAnimations()
{
   if ( isObject( AnimationVis.array ) )
      AnimationVis.array.delete();
   SequenceList.clear();

   $animationInit = false;

   $MeshLoop::ActiveSequenceID = 0;
   %firstseq = View.getSequenceName($MeshLoop::ActiveSequenceID);

   // add anims to the list
   for (%i=0;%i<View.getSequenceCount();%i++) {
      %seqName = View.getSequenceName(%i);
      AnimationVis.addOption(%i, %seqName);
   }
   
   AnimationVis.setActiveSequence(%firstseq, $MeshLoop::ActiveSequenceID);
}

function SelectedSequence::onClick(%this) {
   SequencesVis.visible = !SequencesVis.visible;
   if (SequencesVis.visible) {
      SequenceList.scrollVisible(SequenceList.getRowNumById(%id));
   }
}

function View::playAnimation(%this, %seqname) {
   %this.setSeqByName(%seqname);
//   SequencesVis.visible = 0;
}

function AnimationVis::onWake( %this )
{
   // Create the array if it
   // doesn't already exist.
   if ( !isObject( %this.array ) )
      %this.array = new ArrayObject();

   %this.updateOptions();
}

function AnimationVis::setActiveSequence(%this, %sequence, %row) 
{
   View.playAnimation(%sequence);
   SequenceList.scrollVisible(%row);
   eval("$Pref::MeshLoop::AnimationVis::"@$model@" = %sequence;");
   SelectedSequence.text = "  " @ %sequence;

   %frames = View.getSequenceFrameCount($MeshLoop::ActiveSequenceID);
   SequenceSlider.altMultiplier = %frames;
   $MeshLoop::FrameCount = %frames;
   if (%frames<1) %frames = 1;
   SequenceSlider.ticks = %frames-1;

   /*   
   %duration = View.getDuration($MeshLoop::ActiveSequenceID);
   echo("Duration: " @ %duration);
   */
   
   BtnRepeat.setStateOn(View.isCyclic($MeshLoop::ActiveSequenceID));
   BtnBlend.setStateOn(View.isBlend($MeshLoop::ActiveSequenceID));
}

function playNextSequence(%val)
{
   if (!%val)
      return;
      
   if (!isObject(AnimationVis.array))
      return;
      
   %row = trim(SequenceList.getRowNumById($MeshLoop::ActiveSequenceID));
   %row += 1;
   if (%row >= SequenceList.rowCount())
      return;
      
   %id = SequenceList.getRowId(%row);
   %seq = AnimationVis.array.getValue(%id);
   if (%seq !$= "") {
      $MeshLoop::ActiveSequenceID = %id;
      AnimationVis.setActiveSequence(%seq, %row);
   }
}

function playPrevSequence(%val)
{
   if (!%val)
      return;
      
   if (!isObject(AnimationVis.array))
      return;
      
   %row = trim(SequenceList.getRowNumById($MeshLoop::ActiveSequenceID));
   %row -= 1;
   if (%row < 0)
      return;
      
   %id = SequenceList.getRowId(%row);
   %seq = AnimationVis.array.getValue(%id);
   if (%seq !$= "") {
      $MeshLoop::ActiveSequenceID = %id;
      AnimationVis.setActiveSequence(%seq, %row);
   }
}

function SequenceList::onSelect(%this, %id, %text)
{
   $MeshLoop::ActiveSequenceID = %id;
   AnimationVis.setActiveSequence( trim(%text), trim(SequenceList.getRowNumById(%id)) );
}

function SequenceList::onSelectDblClick(%this, %id, %text)
{
   if (%id != $MeshLoop::ActiveSequenceID) {
      $MeshLoop::ActiveSequenceID = %id;
      AnimationVis.setActiveSequence( trim(%text), trim(SequenceList.getRowNumById(%id)) );
   } else {
      playPause(1);
   }
}

function AnimationVis::updateOptions( %this )
{
   // First clear the stack control.
   SequenceList.clear();

   // go through all seauences in the array
   for ( %i = 0; %i < %this.array.count(); %i++ )
   {
      %seqid = %this.array.getKey( %i );
      %seqname = %this.array.getValue( %i );

      %thisrow = SequenceList.addRow(%seqid, %seqname);

      //%cmd = "AnimationVis.setActiveSequence( \"" @ trim(%seqname) @ "\", \""@ trim(%thisrow) @"\" );";
      
      %sel = 0;
      if ($Pref::MeshLoop::RememberShapeSetup) {
         eval("if ($Pref::MeshLoop::AnimationVis::"@$model@" $= trim(%text)) %sel=1;");
         if (%sel) {
            SequenceList.scrollVisible(%thisrow);
            SequenceList.setSelectedRow(%thisrow);
         }
      } else if (%i==0) {
         SequenceList.scrollVisible(%thisrow);
         SequenceList.setSelectedRow(%thisrow);
      }
   }   
}

function AnimationVis::addOption( %this, %id, %text )
{
   // Create the array if it
   // doesn't already exist.
   if ( !isObject( %this.array ) )
      %this.array = new ArrayObject();   
   
   %this.array.push_back( %id, %text );
   %this.array.uniqueKey();
//   %this.array.sortd(); 
   %this.updateOptions();
}

function readSequences(%filename) {
   if (%filename $= "/.cs")
      return;
   
   %filepath = filePath(%filename);
   if (strstr(%filepath, "game")!=-1) {
      %gameroot = getSubStr(%filepath, 0, strstr(%filepath, "game")+4);
   } else {
      %gameroot = getSubStr(%filepath, 0, strstr(%filepath, ":/")+1);
   }

   %inFileHandle = new FileObject();
   %inFileHandle.openForRead(%filename);
   while(!%inFileHandle.IsEOF()) {
      %inLine = %inFileHandle.readLine();
      // check for the first line and try rearranging it a bit...
      if (strstr(strlwr(%inLine), "tsshapeconstructor")!=-1) {
         %inLine = strreplace(%inLine, "datablock", "singleton");
         %f = strchr(%inLine, "(");
         %l = strrchr(%inLine, ")");
         %dtscn = trim(getSubStr(%f, 1, strlen(%f)-strlen(%l)-1));
         if (%dtscn $= "") {
            %dtscn = "DTSConstructor" @ getRealTime();
            %inLine = %f @ %dtscn @ %l;
         }
         $shapeConstructor = %dtscn;
      }
      // replace relative and absolute paths for basic fields
      if (  strstr(strlwr(%inLine), "baseshape")!=-1 || 
            strstr(strlwr(%inLine), "sequence")!=-1
         )
      {
         %inLine = convertParamToFullPath(%inLine, %filepath, %gameroot);
      }
      %file = %file @ %inLine @ "\n";
      if (strstr(%inLine, "}")!=-1) {
         // end of a material definition? could be.. eval it!
         %thisfile = $Con::File;
         $Con::File = %filename;
         eval(%file);
         $Con::File = %thisfile;
         // and make sure %file stays small
         %file = "";
      }
   }
   %inFileHandle.close();
   %inFileHandle.delete();
}

function SpeedSlider::onValueChanged(%this) {
   $MeshLoop::SpeedValue = SpeedSlider.getValue();
   View.setTimeScale($MeshLoop::SpeedValue);
   SpeedSliderPercent.text = mRound($MeshLoop::SpeedValue*100) @ " %";
   updatePlayPauseBtn();
}

function SequenceSlider::onMouseDown(%this) {
   View.sliderOverride = true;
   %oldspeed = SpeedSlider.getValue();
   SpeedSlider.setValue(0);
   $MeshLoop::SpeedValue = %oldspeed;
   updatePlayPauseBtn();
}

function SequenceSlider::onMouseUp(%this) {
   View.sliderOverride = false;
}

function SequenceSlider::onMouseDragged(%this) {
   $MeshLoop::SpeedValue = SpeedSlider.getValue();
}

function gotoNextFrame(%val) {
   if (!%val)
      return;
      
   %currpos = $MeshLoop::FrameCount*SequenceSlider.getValue();
   %currpos = mCeil(%currpos);
   %newpos = %currpos >= $MeshLoop::FrameCount ? 0 : %currpos+1;
   View.setSequencePosition(%newpos/$MeshLoop::FrameCount);
   $MeshLoop::SpeedValue = 0;
   SpeedSlider.setValue($MeshLoop::SpeedValue);
   return %newpos;
}

function gotoPrevFrame(%val) {
   if (!%val)
      return;
      
   %currpos = $MeshLoop::FrameCount*SequenceSlider.getValue();
   %currpos = mFloor(%currpos);
   %newpos = %currpos < 1 ? $MeshLoop::FrameCount-1 : %currpos-1;
   View.setSequencePosition(%newpos/$MeshLoop::FrameCount);
   $MeshLoop::SpeedValue = 0;
   SpeedSlider.setValue($MeshLoop::SpeedValue);
}

function playPause(%val) {
   if (!%val)
      return;

   if (SpeedSlider.getValue()!=0) {
      %oldspeed = SpeedSlider.getValue();
      SpeedSlider.setValue(0);
      $MeshLoop::SpeedValue = %oldspeed;
   } else {
      if ($MeshLoop::SpeedValue==0) $MeshLoop::SpeedValue = 1;
      SpeedSlider.setValue($MeshLoop::SpeedValue);
   }
   updatePlayPauseBtn();
}

function updatePlayPauseBtn() {
   if (SpeedSlider.getValue()!=0) {
      BtnPlayFrame.setBitmap("gui/images/animation/pause");
   } else {
      BtnPlayFrame.setBitmap("gui/images/animation/play");
   }
}

function updateAnimationBlend() {
   // can't touch this
   %isblend = View.isBlend($MeshLoop::ActiveSequenceID);
   BtnBlend.setStateOn(%isblend);
}

function updateAnimationRepeat() {
   %state = BtnRepeat.getStateOn();
   View.setCyclic($MeshLoop::ActiveSequenceID, %state);
}

function SequencesVis::saveDims(%this)
{
   $Pref::MeshLoop::SequencesVis::Position = %this.getPosition();
   $Pref::MeshLoop::SequencesVis::Extent = %this.getExtent();
}

