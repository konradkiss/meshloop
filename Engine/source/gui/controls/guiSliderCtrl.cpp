//-----------------------------------------------------------------------------
// Torque 3D
// Copyright (C) GarageGames.com, Inc.
//-----------------------------------------------------------------------------

#include "console/console.h"
#include "console/consoleTypes.h"
#include "gfx/gfxTextureManager.h"
#include "gui/controls/guiSliderCtrl.h"
#include "gui/core/guiDefaultControlRender.h"
#include "platform/event.h"
#include "gfx/primBuilder.h"
#include "gfx/gfxDrawUtil.h"
#include "sfx/sfxSystem.h"

IMPLEMENT_CONOBJECT(GuiSliderCtrl);

//----------------------------------------------------------------------------
GuiSliderCtrl::GuiSliderCtrl(void)
{
   mActive = true;
   mRange.set( 0.0f, 1.0f );
   mTicks = 10;
   mValue = NULL; // >>> <<<
   mThumbSize.set(8,20);
   mShiftPoint = 5;
   mShiftExtent = 10;
   mIncAmount = 0.0f;
   mDisplayValue = false;
   mMouseOver = false;
   mDepressed = false;
	// >>>
   mMouseDragged = false; 
	mSnap = false;
   mDefaultValue = NULL;
   mFluid = false;
   mCanControl = true;
   mAltConsoleVariable = StringTable->insert("");
   mAltMultiplier = 1.0f;
	// <<<
}

//----------------------------------------------------------------------------
void GuiSliderCtrl::initPersistFields()
{
   addGroup( "Slider" );
   addField("range", TypePoint2F,   Offset(mRange, GuiSliderCtrl));
   addField("ticks", TypeS32,       Offset(mTicks, GuiSliderCtrl));
   addField("value", TypeF32,       Offset(mValue, GuiSliderCtrl));
   // >>>
   addField("defaultValue", TypeF32,Offset(mDefaultValue, GuiSliderCtrl)); 
   addField("snap",  TypeBool,      Offset(mSnap,  GuiSliderCtrl)); 
   addField("fluid",  TypeBool,     Offset(mFluid, GuiSliderCtrl)); 
   addField("canControl",  TypeBool,     Offset(mCanControl, GuiSliderCtrl)); 
   addField("mouseDragged", TypeBool, Offset(mMouseDragged, GuiSliderCtrl)); 
   addField("altVariable",    TypeString,    Offset( mAltConsoleVariable, GuiSliderCtrl ));
   addField("altMultiplier",  TypeF32,			Offset( mAltMultiplier,		GuiSliderCtrl ));
   // <<<
   endGroup( "Slider" );

   Parent::initPersistFields();
}

//----------------------------------------------------------------------------
ConsoleMethod( GuiSliderCtrl, getValue, F32, 2, 2, "Get the position of the slider.")
{
   return object->getValue();
}

//----------------------------------------------------------------------------
ConsoleMethod( GuiSliderCtrl, setValue, void, 3, 3, "( float pos ) - Set position of the slider." )
{
   object->setValue( dAtof( argv[ 2 ] ) );
}

//----------------------------------------------------------------------------
void GuiSliderCtrl::setValue(F32 val)
{
   mValue = val;
   updateThumb(mValue, false);
}

//----------------------------------------------------------------------------
void GuiSliderCtrl::setActive( bool value )
{
   if( !value && mDepressed )
   {
      // We're in the middle of a drag.  Finish it here as once we've
      // been deactivated, we are not going to see a mouse-up event.

      mDepressed = false;
      mouseUnlock();
      execConsoleCallback();
   }

   Parent::setActive( value );
}

//----------------------------------------------------------------------------
bool GuiSliderCtrl::onWake()
{
   if (! Parent::onWake())
      return false;

	// >>>
   mHasTexture = mProfile->constructBitmapArray() >= NumBitmaps;  
	if( mHasTexture ) {
      mBitmapBounds = mProfile->mBitmapArrayRects.address();
		mThumbSize = Point2I(mBitmapBounds[SliderButtonNormal].extent.x, mBitmapBounds[SliderButtonNormal].extent.y);
	}

	if (mValue)
		mValue = mClampF(mValue, mRange.x, mRange.y);
	else
		mValue = mClampF(getFloatVariable(), mRange.x, mRange.y);
	// <<<

   // mouse scroll increment percentage is 5% of the range
   mIncAmount = ((mRange.y - mRange.x) * 0.05);

   if(mThumbSize.y + mProfile->mFont->getHeight()-4 <= getExtent().y)
      mDisplayValue = true;
   else
      mDisplayValue = false;

   updateThumb( mValue, false, true );

   // >>> <<<

   return true;
}

// >>>
void GuiSliderCtrl::updateSize()
{
	// we need to reposition the thumb
	updateThumb( mValue, false );
}

void GuiSliderCtrl::parentResized(const RectI &oldParentRect, const RectI &newParentRect)
{
   Parent::parentResized( oldParentRect, newParentRect );

   updateSize();
}
// <<<

// >>>
//----------------------------------------------------------------------------
void GuiSliderCtrl::onMouseDown(const GuiEvent &event)
{
   if ( !mActive || !mAwake || !mVisible || !mCanControl)
      return;

   mouseLock();
   setFirstResponder();
   mDepressed = true;

   if (event.mouseClickCount > 1) {
      // dbl-click
      updateThumb( mDefaultValue, false );
   } else {
      Point2I curMousePos = globalToLocalCoord(event.mousePoint);
      F32 value;
      if (getWidth() >= getHeight())
         value = F32(curMousePos.x-mShiftPoint) / F32(getWidth()-mShiftExtent)*(mRange.y-mRange.x) + mRange.x;
      else
         value = F32(curMousePos.y) / F32(getHeight())*(mRange.y-mRange.x) + mRange.x;
      
      updateThumb( value, ( event.modifier & SI_SHIFT ) );
   }
   Con::executef(this, "onMouseDown");
}
// <<<

//----------------------------------------------------------------------------
void GuiSliderCtrl::onMouseDragged(const GuiEvent &event)
{
   if ( !mActive || !mAwake || !mVisible || !mCanControl)
      return;
	
	// >>>
   mMouseDragged = true;

	F32 value = getThumbValue(event);
   updateThumb( value, ( event.modifier & SI_SHIFT ) );

   Con::executef(this, "onMouseDragged");
	// <<<
}

//----------------------------------------------------------------------------
void GuiSliderCtrl::onMouseUp(const GuiEvent &event)
{
   if ( !mActive || !mAwake || !mVisible || !mCanControl)
      return;
   mDepressed = false;
	mMouseDragged = false;
   mouseUnlock();
	// >>>
   if (event.mouseClickCount > 1) {
      // dbl-click mouseup
	   updateThumb( mDefaultValue, ( event.modifier & SI_SHIFT ) );
   } else {
      // not a dbl-click mouseup
	   F32 value = getThumbValue(event);
	   updateThumb( value, ( event.modifier & SI_SHIFT ) );
   }
   Con::executef(this, "onMouseUp");
	// <<<
   execConsoleCallback();
}

// >>>
F32 GuiSliderCtrl::getThumbValue(const GuiEvent &event)
{
   Point2I curMousePos = globalToLocalCoord(event.mousePoint);
   F32 value;
   if (getWidth() >= getHeight())
      value = F32(curMousePos.x-mShiftPoint) / F32(getWidth()-mShiftExtent)*(mRange.y-mRange.x) + mRange.x;
   else
      value = F32(curMousePos.y) / F32(getHeight())*(mRange.y-mRange.x) + mRange.x;

   if (value > mRange.y)
      value = mRange.y;
   else if (value < mRange.x)
      value = mRange.x;

   // >>>
   if (!mFluid && event.modifier & SI_SHIFT)
      if ((!(event.modifier & SI_SHIFT) && mTicks >= 1) || mSnap) { 
         // If the shift key is held, snap to the nearest tick, if any are being drawn

         F32 tickStep = (mRange.y - mRange.x) / F32(mTicks + 1);

         F32 tickSteps = (value - mRange.x) / tickStep;
         S32 actualTick = S32(tickSteps + 0.5);

         value = actualTick * tickStep + mRange.x;
         AssertFatal(value <= mRange.y && value >= mRange.x, "Error, out of bounds value generated from shift-snap of slider");
      }
   // <<<

	return value;
}
// <<<

void GuiSliderCtrl::onMouseEnter(const GuiEvent &event)
{
   setUpdate();
	// >>>
	if (!mMouseOver)
		Con::executef(this, "onMouseOver");
	// <<<
   if(isMouseLocked())
   {
      mDepressed = true;
      mMouseOver = true;
   }
   else
   {
      if ( mActive && mProfile->mSoundButtonOver )
      {
         //F32 pan = (F32(event.mousePoint.x)/F32(getRoot()->getWidth())*2.0f-1.0f)*0.8f;
         SFX->playOnce(mProfile->mSoundButtonOver);
      }
      mMouseOver = true;
   }
}

void GuiSliderCtrl::onMouseLeave(const GuiEvent &)
{
   setUpdate();
   if(isMouseLocked())
      mDepressed = false;
   mMouseOver = false;
}
//----------------------------------------------------------------------------
bool GuiSliderCtrl::onMouseWheelUp(const GuiEvent &event)
{
   if ( !mActive || !mAwake || !mVisible )
      return Parent::onMouseWheelUp(event);

   mValue += mIncAmount;
   updateThumb( mValue, ( event.modifier & SI_SHIFT ) );

   return true;
}

bool GuiSliderCtrl::onMouseWheelDown(const GuiEvent &event)
{
   if ( !mActive || !mAwake || !mVisible )
      return Parent::onMouseWheelUp(event);

   mValue -= mIncAmount;
   updateThumb( mValue, ( event.modifier & SI_SHIFT ) );

   return true;
}
//----------------------------------------------------------------------------
void GuiSliderCtrl::updateThumb( F32 _value, bool snap, bool onWake )
{
   // >>>
   if (!mFluid || snap)
      if ((snap && mTicks >= 1) || mSnap) { // >>> <<<
         // If the shift key is held, snap to the nearest tick, if any are being drawn

         F32 tickStep = (mRange.y - mRange.x) / F32(mTicks + 1);

         F32 tickSteps = (_value - mRange.x) / tickStep;
         S32 actualTick = S32(tickSteps + 0.5);

         _value = actualTick * tickStep + mRange.x;
      }
   // <<<
   
	// >>>
	Con::executef(this, "onValueChanged");
	// <<<

   mValue = _value;
   // clamp the thumb to legal values
   if (mValue < mRange.x)  mValue = mRange.x;
   if (mValue > mRange.y)  mValue = mRange.y;

   Point2I ext = getExtent();
	ext.x -= ( mShiftExtent + mThumbSize.x ) / 2;
   // update the bounding thumb rect
   if (getWidth() >= getHeight())
   {  // HORZ thumb
      S32 mx = (S32)((F32(ext.x) * (mValue-mRange.x) / (mRange.y-mRange.x)));
      S32 my = ext.y/2;
      if(mDisplayValue)
         my = mThumbSize.y/2;

      mThumb.point.x  = mx - (mThumbSize.x/2);
      mThumb.point.y  = my - (mThumbSize.y/2);
      mThumb.extent   = mThumbSize;
   }
   else
   {  // VERT thumb
      S32 mx = ext.x/2;
      S32 my = (S32)((F32(ext.y) * (mValue-mRange.x) / (mRange.y-mRange.x)));
      mThumb.point.x  = mx - (mThumbSize.y/2);
      mThumb.point.y  = my - (mThumbSize.x/2);
      mThumb.extent.x = mThumbSize.y;
      mThumb.extent.y = mThumbSize.x;
   }
   setFloatVariable(mValue);
   setUpdate();

   // Use the alt console command if you want to continually update:
   if ( !onWake )
      execAltConsoleCallback();
}

//----------------------------------------------------------------------------
void GuiSliderCtrl::onRender(Point2I offset, const RectI &updateRect)
{
   // >>>
   mShiftExtent = 2*(S32)mThumb.extent.x/2;
   mShiftPoint = mThumb.extent.x/2;

   F32 altval = F32(mValue * mAltMultiplier);
   if (mFabs(altval-mFloor(altval))<0.001) altval = mFloor(altval);
   if (mFabs(altval-mCeil(altval))<0.001) altval = mCeil(altval);
   String formattedVar = String::ToString("%f2.2", altval);
   Con::setVariable(mAltConsoleVariable, formattedVar);

   if (mValue>0.999) {
      Con::setVariable(mConsoleVariable, "1.000");
   } else if (mValue<0.001) {
      Con::setVariable(mConsoleVariable, "0.000");
   } else {
      formattedVar = String::ToString("%f2.2", F32(mValue));
      Con::setVariable(mConsoleVariable, formattedVar);
   }
   // <<<

   Point2I pos(offset.x+mShiftPoint, offset.y);
   Point2I ext(getWidth() - mShiftExtent, getHeight());
   RectI thumb = mThumb;

   if( mHasTexture )
   {
      if(mTicks > 0)
      {
         // TODO: tick marks should be positioned based on the bitmap dimensions.
         Point2I mid(ext.x, ext.y/2);
         Point2I oldpos = pos;
         pos += Point2I(1, 0);

         PrimBuild::color4f( 0.f, 0.f, 0.f, 1.f );
         PrimBuild::begin( GFXLineList, ( mTicks + 2 ) * 2 );
         // tick marks
         for (U32 t = 0; t <= (mTicks+1); t++)
         {
            // >>>
            //S32 x = (S32)(F32(mid.x+1)/F32(mTicks+1)*F32(t)) + pos.x;
            S32 x = (S32)( F32( getWidth() - mShiftExtent ) / F32( mTicks + 1 ) * F32( t ) ) + offset.x + mShiftPoint;
            // <<<
            S32 y = pos.y + mid.y;
            PrimBuild::vertex2i(x, y + mShiftPoint);
            PrimBuild::vertex2i(x, y + mShiftPoint*2 + 2);
         }
         PrimBuild::end();
         // TODO: it would be nice, if the primitive builder were a little smarter,
         // so that we could change colors midstream.
         PrimBuild::color4f(0.9f, 0.9f, 0.9f, 1.0f);
         PrimBuild::begin( GFXLineList, ( mTicks + 2 ) * 2 );
         // tick marks
         for (U32 t = 0; t <= (mTicks+1); t++)
         {
            // >>>
            //S32 x = (S32)(F32(mid.x+1)/F32(mTicks+1)*F32(t)) + pos.x + 1;
            S32 x = (S32)( F32( getWidth() - mShiftExtent ) / F32( mTicks + 1 ) * F32( t ) ) + offset.x + mShiftPoint + 1;
            // <<<
            S32 y = pos.y + mid.y + 1;
            PrimBuild::vertex2i(x, y + mShiftPoint );
            PrimBuild::vertex2i(x, y + mShiftPoint * 2 + 3);
         }
         PrimBuild::end();
         pos = oldpos;
      }

      S32 index = SliderButtonNormal;
      if ((mMouseOver || mMouseDragged) && mCanControl) // >>> <<<
         index = SliderButtonHighlight;
      GFX->getDrawUtil()->clearBitmapModulation();

      //left border
      GFX->getDrawUtil()->drawBitmapSR(mProfile->mTextureObject, Point2I(offset.x,offset.y), mBitmapBounds[SliderLineLeft]);
      //right border
      GFX->getDrawUtil()->drawBitmapSR(mProfile->mTextureObject, Point2I(offset.x + getWidth() - mBitmapBounds[SliderLineRight].extent.x, offset.y), mBitmapBounds[SliderLineRight]);


      //draw our center piece to our slider control's border and stretch it
      RectI destRect;	
      destRect.point.x = offset.x + mBitmapBounds[SliderLineLeft].extent.x;
      destRect.extent.x = getWidth() - mBitmapBounds[SliderLineLeft].extent.x - mBitmapBounds[SliderLineRight].extent.x;
      destRect.point.y = offset.y;
      destRect.extent.y = mBitmapBounds[SliderLineCenter].extent.y;

      RectI stretchRect;
      stretchRect = mBitmapBounds[SliderLineCenter];
      stretchRect.inset(1,0);

      GFX->getDrawUtil()->drawBitmapStretchSR(mProfile->mTextureObject, destRect, stretchRect);

      //draw our control slider button	
      thumb.point += pos;
      GFX->getDrawUtil()->drawBitmapSR(mProfile->mTextureObject,Point2I(thumb.point.x,offset.y ),mBitmapBounds[index]);

   }
   else if (getWidth() >= getHeight())
   {
      Point2I mid(ext.x, ext.y/2);
      if(mDisplayValue)
         mid.set(ext.x, mThumbSize.y/2);

      PrimBuild::color4f( 0.f, 0.f, 0.f, 1.f );
      PrimBuild::begin( GFXLineList, ( mTicks + 2 ) * 2 + 2);
         // horz rule
         PrimBuild::vertex2i( pos.x, pos.y + mid.y );
         PrimBuild::vertex2i( pos.x + mid.x, pos.y + mid.y );

         // tick marks
         for( U32 t = 0; t <= ( mTicks + 1 ); t++ )
         {
            S32 x = (S32)( F32( mid.x - 1 ) / F32( mTicks + 1 ) * F32( t ) );
            PrimBuild::vertex2i( pos.x + x, pos.y + mid.y - mShiftPoint );
            PrimBuild::vertex2i( pos.x + x, pos.y + mid.y + mShiftPoint );
         }
         PrimBuild::end();
   }
   else
   {
      Point2I mid(ext.x/2, ext.y);

      PrimBuild::color4f( 0.f, 0.f, 0.f, 1.f );
      PrimBuild::begin( GFXLineList, ( mTicks + 2 ) * 2 + 2);
         // horz rule
         PrimBuild::vertex2i( pos.x + mid.x, pos.y );
         PrimBuild::vertex2i( pos.x + mid.x, pos.y + mid.y );

         // tick marks
         for( U32 t = 0; t <= ( mTicks + 1 ); t++ )
         {
            S32 y = (S32)( F32( mid.y - 1 ) / F32( mTicks + 1 ) * F32( t ) );
            PrimBuild::vertex2i( pos.x + mid.x - mShiftPoint, pos.y + y );
            PrimBuild::vertex2i( pos.x + mid.x + mShiftPoint, pos.y + y );
         }
         PrimBuild::end();
      mDisplayValue = false;
   }
   // draw the thumb
   thumb.point += pos;
   renderRaisedBox(thumb, mProfile);

   if(mDisplayValue)
   {
   	char buf[20];
  		dSprintf(buf,sizeof(buf),"%0.3f",mValue);

   	Point2I textStart = thumb.point;

      S32 txt_w = mProfile->mFont->getStrWidth((const UTF8 *)buf);

   	textStart.x += (S32)((thumb.extent.x/2.0f));
   	textStart.y += thumb.extent.y - 2; //19
   	textStart.x -=	(txt_w/2);
   	if(textStart.x	< offset.x)
   		textStart.x = offset.x;
   	else if(textStart.x + txt_w > offset.x+getWidth())
   		textStart.x -=((textStart.x + txt_w) - (offset.x+getWidth()));

    	GFX->getDrawUtil()->setBitmapModulation(mProfile->mFontColor);
    	GFX->getDrawUtil()->drawText(mProfile->mFont, textStart, buf, mProfile->mFontColors);
   }
   renderChildControls(offset, updateRect);
}

