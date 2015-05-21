//-----------------------------------------------------------------------------
// Torque Shader Engine
// Copyright (C) GarageGames.com, Inc.
//-----------------------------------------------------------------------------
#ifndef _LIGHTSETUP_H_
#define _LIGHTSETUP_H_

#ifndef _SIMOBJECT_H_
#include "console/simObject.h"
#endif
#ifndef _COLOR_H_
#include "core/color.h"
#endif
#ifndef _TSSHAPEINSTANCE_H_
#include "ts/tsShapeInstance.h"
#endif
#ifndef _GUITSCONTROL_H_
#include "T3D/guiObjectView.h"
#endif

class LightInfo;
class GuiObjectView;

//**************************************************************************
// LightSetup - stores the position and brightness of LIGHTS_NUM lights
//              that can be assigned to a shape's instance
//**************************************************************************
class LightSetup : public SimObject
{
   typedef SimObject Parent;

   enum {
      LIGHTS_NUM     = 3,
      LIGHT_DISTANCE = 500,
      LIGHT_GUI_DISTANCE = 5,
      LIGHT_RANGE    = 1000
   };

   struct LSLight {
      bool           enabled;
      ColorF		   color;
      ColorF		   ambient;
      Point3F		   position;
      Point3F        target;
      F32		      brightness;
   };

   Vector<LSLight>      mLS;
   Vector<LightInfo*>   mLights;

   TSShapeInstance*  mShapeInstance;
   GFXTexHandle      mLightTex;

   F32               mLightGuiDistance;

public:

   LightSetup();
   ~LightSetup();

   void wake();
   void registerLights();
   void updateLights(MatrixF cameraMatrix);
   void readPrefs();
   VectorF getDirection(F32 azimuth, F32 elevation);
   void renderGuiOverlays(Point2I offset, const RectI& updateRect, GuiObjectView* screen, MatrixF cameraMatrix, S32 currLight);
   
   void raiseBrightness(U32 light, F32 amount, F32 clamp);
   void lowerBrightness(U32 light, F32 amount, F32 clamp);
   void setBrightness(U32 light, F32 brightness);
   void setPosition(U32 light, Point3F position);
   void setTarget(U32 light, Point3F target);
   void setEnabled(U32 light, bool state);
   void setColor(U32 light, ColorF color);
   void setAmbient(U32 light, ColorF ambient);

   void setLightGuiDistance(F32 distance) { mLightGuiDistance = distance; };

   void setLightTex();
   F32  getDistance() { return LIGHT_DISTANCE; };
   LightInfo* getLight(U32 id) { return mLights[id]; };

   ///
   /// SimObject interface
   ///
   virtual bool onAdd();
   virtual void onRemove();
   virtual void inspectPostApply();

   //
   // ConsoleObject interface
   //
   static void initPersistFields();

   DECLARE_CONOBJECT(LightSetup);

};

#endif // _LIGHTSETUP_H_
