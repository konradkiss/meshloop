//---------------------------------------------------------------------------------------------
// Torque Game Builder
// Copyright (C) GarageGames.com, Inc.
//---------------------------------------------------------------------------------------------

function validateDatablockName(%name)
{
   // remove whitespaces at beginning and end
   %name = trim( %name );
   
   // remove numbers at the beginning
   %numbers = "0123456789";   
   while( strlen(%name) > 0 )
   {
      // the first character
      %firstChar = getSubStr( %name, 0, 1 );
      // if the character is a number remove it
      if( strpos( %numbers, %firstChar ) != -1 )
      {
         %name = getSubStr( %name, 1, strlen(%name) -1 );
         %name = ltrim( %name );
      }
      else
         break;
   }
   
   // replace whitespaces with underscores
   %name = strreplace( %name, " ", "_" );
   
   // remove any other invalid characters
   %invalidCharacters = "-+*/%$&§=()[].?\"#,;!~<>|°^{}";
   %name = stripChars( %name, %invalidCharacters );
   
   if( %name $= "" )
      %name = "Unnamed";
   
   return %name;
}

//--------------------------------------------------------------------------
// Finds location of %word in %text, starting at %start.  Works just like strPos
//--------------------------------------------------------------------------

function wordPos(%text, %word, %start)
{
   if (%start $= "") %start = 0;
   
   if (strpos(%text, %word, 0) == -1) return -1;
   %count = getWordCount(%text);
   if (%start >= %count) return -1;
   for (%i = %start; %i < %count; %i++)
   {
      if (getWord( %text, %i) $= %word) return %i;
   }
   return -1;
}

//--------------------------------------------------------------------------
// Finds location of %field in %text, starting at %start.  Works just like strPos
//--------------------------------------------------------------------------

function fieldPos(%text, %field, %start)
{
   if (%start $= "") %start = 0;
   
   if (strpos(%text, %field, 0) == -1) return -1;
   %count = getFieldCount(%text);
   if (%start >= %count) return -1;
   for (%i = %start; %i < %count; %i++)
   {
      if (getField( %text, %i) $= %field) return %i;
   }
   return -1;
}

//--------------------------------------------------------------------------
// returns the text in a file with "\n" at the end of each line
//--------------------------------------------------------------------------

function loadFileText( %file)
{
   %fo = new FileObject();
   %fo.openForRead(%file);
   %text = "";
   while(!%fo.isEOF())
   {
      %text = %text @ %fo.readLine();
      if (!%fo.isEOF()) %text = %text @ "\n";
   }

   %fo.delete();
   return %text;
}

function setValueSafe(%dest, %val)
{
   %cmd = %dest.command;
   %alt = %dest.altCommand;
   %dest.command = "";
   %dest.altCommand = "";

   %dest.setValue(%val);
   
   %dest.command = %cmd;
   %dest.altCommand = %alt;
}

function shareValueSafe(%source, %dest)
{
   setValueSafe(%dest, %source.getValue());
}

function shareValueSafeDelay(%source, %dest, %delayMs)
{
   schedule(%delayMs, 0, shareValueSafe, %source, %dest);
}


//------------------------------------------------------------------------------
// An Aggregate Control is a plain GuiControl that contains other controls, 
// which all share a single job or represent a single value.
//------------------------------------------------------------------------------

// AggregateControl.setValue( ) propagates the value to any control that has an 
// internal name.
function AggregateControl::setValue(%this, %val, %child)
{
   for(%i = 0; %i < %this.getCount(); %i++)
   {
      %obj = %this.getObject(%i);
      if( %obj == %child )
         continue;
         
      if(%obj.internalName !$= "")
         setValueSafe(%obj, %val);
   }
}

// AggregateControl.getValue() uses the value of the first control that has an
// internal name, if it has not cached a value via .setValue
function AggregateControl::getValue(%this)
{
   for(%i = 0; %i < %this.getCount(); %i++)
   {
      %obj = %this.getObject(%i);
      if(%obj.internalName !$= "")
      {
         //warn("obj = " @ %obj.getId() @ ", " @ %obj.getName() @ ", " @ %obj.internalName );
         //warn(" value = " @ %obj.getValue());
         return %obj.getValue();
      }
   }
}

// AggregateControl.updateFromChild( ) is called by child controls to propagate
// a new value, and to trigger the onAction() callback.
function AggregateControl::updateFromChild(%this, %child)
{
   %val = %child.getValue();
   if(%val == mCeil(%val)){
      %val = mCeil(%val);
   }else{
      if ( %val <= -100){
         %val = mCeil(%val);
      }else if ( %val <= -10){
         %val = mFloatLength(%val, 1);
      }else if ( %val < 0){
         %val = mFloatLength(%val, 2);
      }else if ( %val >= 1000){
         %val = mCeil(%val);
      }else if ( %val >= 100){
         %val = mFloatLength(%val, 1);
      }else if ( %val >= 10){
         %val = mFloatLength(%val, 2);
      }else if ( %val > 0){
         %val = mFloatLength(%val, 3);
      }
   }
   %this.setValue(%val, %child);
   %this.onAction();
}

// default onAction stub, here only to prevent console spam warnings.
function AggregateControl::onAction(%this) 
{
}

// call a method on all children that have an internalName and that implement the method.
function AggregateControl::callMethod(%this, %method, %args)
{
   for(%i = 0; %i < %this.getCount(); %i++)
   {
      %obj = %this.getObject(%i);
      if(%obj.internalName !$= "" && %obj.isMethod(%method))
         eval(%obj @ "." @ %method @ "( " @ %args @ " );");
   }

}

// A function used in order to easily parse the MissionGroup for classes . I'm pretty 
// sure at this point the function can be easily modified to search the any group as well.
function parseMissionGroup( %className, %childGroup )
{
   if( getWordCount( %childGroup ) == 0)
      %currentGroup = "MissionGroup";
   else
      %currentGroup = %childGroup;
      
   for(%i = 0; %i < (%currentGroup).getCount(); %i++)
   {      
      if( (%currentGroup).getObject(%i).getClassName() $= %className )
         return true;
      
      if( (%currentGroup).getObject(%i).getClassName() $= "SimGroup" )
      {
         if( parseMissionGroup( %className, (%currentGroup).getObject(%i).getId() ) )
            return true;         
      }
   } 
}

// A variation of the above used to grab ids from the mission group based on classnames
function parseMissionGroupForIds( %className, %childGroup )
{
   
   if( getWordCount( %childGroup ) == 0)
      %currentGroup = "MissionGroup";
   else
      %currentGroup = %childGroup;
      
   for(%i = 0; %i < (%currentGroup).getCount(); %i++)
   {      
      if( (%currentGroup).getObject(%i).getClassName() $= %className )
         %classIds = %classIds @ (%currentGroup).getObject(%i).getId() @ " ";
      
      if( (%currentGroup).getObject(%i).getClassName() $= "SimGroup" )
         %classIds = %classIds @ parseMissionGroupForIds( %className, (%currentGroup).getObject(%i).getId());
   } 
   return %classIds;
}

//------------------------------------------------------------------------------
// Altered Version of TGB's QuickEditDropDownTextEditCtrl
//------------------------------------------------------------------------------

function QuickEditDropDownTextEditCtrl::updateFromChild( %this, %ctrl )
{
   if( %ctrl.internalName $= "PopUpMenu" )
   {
      for(%i = 0; %i < %this.getCount(); %i++)
      {
         %obj = %this.getObject(%i);
         if( %ctrl == %obj )
            continue;
         
         if( %obj.internalName $= "TextEdit" )
         {
            %obj.setText( %ctrl.getText() );
            break;
         }
      }
   }
   else if ( %ctrl.internalName $= "TextEdit" )
   {
      for(%i = 0; %i < %this.getCount(); %i++)
      {
         %obj = %this.getObject(%i);
         if( %ctrl == %obj )
            continue;
         
         if( %obj.internalName $= "PopUpMenu" )
         {
         }
      }
   }
}

function ColorIToHex(%color) {
   %r = mRound(getWord(%color, 0));
   %g = mRound(getWord(%color, 1));
   %b = mRound(getWord(%color, 2));
   
   %chars = "0123456789ABCDEF";
   
   %r0 = (%r % 16);
   %r1 = (%r - %r0)/16;
   %g0 = (%g % 16);
   %g1 = (%g - %g0)/16;
   %b0 = (%b % 16);
   %b1 = (%b - %b0)/16;
   
   return getSubStr(%chars, %r1, 1) @ getSubStr(%chars, %r0, 1) @ getSubStr(%chars, %g1, 1) @ getSubStr(%chars, %g0, 1) @ getSubStr(%chars, %b1, 1) @ getSubStr(%chars, %b0, 1);
}

function ColorFToHex(%color) {
   %r = mRound(getWord(%color, 0)*255.0);
   %g = mRound(getWord(%color, 1)*255.0);
   %b = mRound(getWord(%color, 2)*255.0);
   
   return ColorIToHex(%r SPC %g SPC %b);
}
