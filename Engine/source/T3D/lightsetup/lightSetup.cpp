//-----------------------------------------------------------------------------
// Torque Shader Engine
// Copyright (C) GarageGames.com, Inc.
//-----------------------------------------------------------------------------

#include "platform/platform.h"
#include "T3D/lightSetup/lightSetup.h"
#include "console/consoleTypes.h"
#include "sceneGraph/sceneGraph.h"
#include "lighting/lightManager.h"
#include "gui/core/guiTypes.h"
#include "gfx/gfxDrawUtil.h"
#include "T3D/guiObjectView.h"
#include "lighting/shadowMap/lightShadowMap.h"

IMPLEMENT_CONOBJECT(LightSetup);

LightSetup::LightSetup()
{
   for (U32 i=0;i<LIGHTS_NUM;i++) {
      LSLight clight;
      LightInfo* light;
      light = NULL;
      clight.position =  getDirection(150+i*15, 15+i*15);
      clight.target = Point3F(0,0,0);
      clight.brightness = 4.0f;
      clight.enabled = true;
      switch (i%3) {
         case 1:
            clight.color.set(0.0f, 1.0f, 0.0f);
            break;
         case 2:
            clight.color.set(0.0f, 0.0f, 1.0f);
            break;
         default:
            clight.color.set(1.0f, 0.0f, 0.0f);
      }
      clight.ambient.set(0.1f, 0.1f, 0.1f);

      readPrefs();

      mLights.push_back(light);
      mLS.push_back(clight);
   }
   mLightTex = NULL;

   mLightGuiDistance = LIGHT_GUI_DISTANCE;
}

LightSetup::~LightSetup()
{
   for (U32 i=0;i<mLights.size();i++) {
      if( mLights[i] )
         mLights[i] = NULL;
   }
}

void LightSetup::wake()
{
   for (U32 i=0;i<mLS.size();i++) {
      if ( !mLights[i] ) {
         mLights[i] = gClientSceneGraph->getLightManager()->createLightInfo();
      }
      mLights[i]->setRange( LIGHT_RANGE );
      mLights[i]->setCastShadows( true );
   }
}

void LightSetup::readPrefs()
{
   for (U32 i=0;i<mLights.size();i++) {
      if (mLights[i]) {
         // load basic stuff from console variables
         String val = Con::getVariable(String("$Pref::MeshLoop::light"+String::ToString(i)+"::position"));
         if (val!=String::EmptyString) {
            dSscanf(val, "%g %g %g", &mLS[i].position.x,&mLS[i].position.y,&mLS[i].position.z);
         }
         val = Con::getVariable(String("$Pref::MeshLoop::light"+String::ToString(i)+"::brightness"));
         if (val!=String::EmptyString) {
            dSscanf(val, "%g", &mLS[i].brightness);
         }
         val = Con::getVariable(String("$Pref::MeshLoop::light"+String::ToString(i)+"::enabled"));
         if (val!=String::EmptyString) {
            mLS[i].enabled = dAtob(val);
         }
         val = Con::getVariable(String("$Pref::MeshLoop::light"+String::ToString(i)+"::color"));
         if (val!=String::EmptyString) {
            dSscanf(val, "%g %g %g", &mLS[i].color.red,&mLS[i].color.green,&mLS[i].color.blue);
         }
         val = Con::getVariable(String("$Pref::MeshLoop::light"+String::ToString(i)+"::ambient"));
         if (val!=String::EmptyString) {
            dSscanf(val, "%g %g %g", &mLS[i].ambient.red,&mLS[i].ambient.green,&mLS[i].ambient.blue);
         }
      }
   }
}

void LightSetup::updateLights(MatrixF cameraMatrix)
{
   for (U32 i=0;i<mLights.size();i++) {
      if (mLights[i]) {
         mLights[i]->setType(LightInfo::Point);

         // ---
         MatrixF ltrans;
         ltrans.identity();
         mLights[i]->setTransform(ltrans);

         mLights[i]->setColor(ColorF(mLS[i].color.red,mLS[i].color.green,mLS[i].color.blue));
         mLights[i]->setAmbient(ColorF(mLS[i].ambient.red,mLS[i].ambient.green,mLS[i].ambient.blue));
         if (mLights[i]->getBrightness()>=0.0f)
            mLights[i]->setBrightness( mLS[i].brightness );

         Point3F lightP = mLS[i].position;
         lightP.normalize();
         Point3F lightPos = lightP * LIGHT_DISTANCE;
         mLS[i].position = lightPos;
         mLights[i]->setPosition( lightPos );

         VectorF lightdir = mLS[i].target - mLS[i].position;
         lightdir.normalize();
         mLights[i]->setDirection(-lightdir);

         MatrixF camRotMatrix( cameraMatrix );
         Point3F camPos;
         camRotMatrix.getColumn(3, &camPos);
         camRotMatrix.setPosition( -lightPos );
         camRotMatrix.inverse();

         mLights[i]->setTransform(camRotMatrix);
      }
   }
}

void LightSetup::raiseBrightness(U32 light, F32 amount, F32 clamp)
{
   if (light>=mLS.size())
      return;

   if (mLS[light].enabled) {
      mLS[light].brightness += amount;
      if (mLS[light].brightness > clamp)
         mLS[light].brightness = clamp;
   }
   Con::setVariable(String("$Pref::MeshLoop::light"+String::ToString(light)+"::brightness"), String::ToString(mLS[light].brightness));
   Con::executef("onBrightnessChanged", String::ToString(mLS[light].brightness).c_str());
}

void LightSetup::lowerBrightness(U32 light, F32 amount, F32 clamp)
{
   if (light>=mLS.size())
      return;

   if (mLS[light].enabled) {
      mLS[light].brightness -= amount;
      if (mLS[light].brightness < clamp)
         mLS[light].brightness = clamp;
   }
   Con::setVariable(String("$Pref::MeshLoop::light"+String::ToString(light)+"::brightness"), String::ToString(mLS[light].brightness));
   Con::executef("onBrightnessChanged", String::ToString(mLS[light].brightness).c_str());
}

void LightSetup::setBrightness(U32 light, F32 brightness)
{
   if (light>=mLS.size())
      return;

   Con::setVariable(String("$Pref::MeshLoop::light"+String::ToString(light)+"::brightness"), String::ToString(brightness));
   mLS[light].brightness = brightness;
}

void LightSetup::setPosition(U32 light, Point3F position)
{
   if (light>=mLS.size())
      return;

   Con::setVariable(String("$Pref::MeshLoop::light"+String::ToString(light)+"::position"), String::ToString(position.x)+" "+String::ToString(position.y)+" "+String::ToString(position.z));
   mLS[light].position = position;
}

void LightSetup::setTarget(U32 light, Point3F target)
{
   if (light>=mLS.size())
      return;

   mLS[light].target = target;
}

void LightSetup::setEnabled(U32 light, bool state)
{
   if (light>=mLS.size())
      return;

   Con::setVariable(String("$Pref::MeshLoop::light"+String::ToString(light)+"::enabled"), String::ToString(state));
   mLS[light].enabled = state;
}

void LightSetup::setColor(U32 light, ColorF color)
{
   if (light>=mLS.size())
      return;

   Con::setVariable(String("$Pref::MeshLoop::light"+String::ToString(light)+"::color"), String::ToString(color.red)+" "+String::ToString(color.green)+" "+String::ToString(color.blue));
   mLS[light].color = color;
}

void LightSetup::setAmbient(U32 light, ColorF ambient)
{
   if (light>=mLS.size())
      return;

   Con::setVariable(String("$Pref::MeshLoop::light"+String::ToString(light)+"::ambient"), String::ToString(ambient.red)+" "+String::ToString(ambient.green)+" "+String::ToString(ambient.blue));
   mLS[light].ambient = ambient;
}

void LightSetup::registerLights()
{
   LightManager* lm = gClientSceneGraph->getLightManager();
   lm->unregisterAllLights();

   for (U32 i=0;i<mLS.size();i++) {
      if (mLS[i].enabled)
         lm->registerGlobalLight(mLights[i], NULL);
   }
   lm->setupLights(NULL, SphereF( Point3F::Zero, 1.0f ) );
}

void LightSetup::setLightTex()
{
   if (!mLightTex)
      mLightTex.set( String("gui/images/light.png"), &GFXDefaultGUIProfile, avar("%s() - mLightTex (line %d)", __FUNCTION__, __LINE__) );
}

VectorF LightSetup::getDirection(F32 azimuth, F32 elevation)
{
   // Calculate Light Direction.
   F32 Yaw = mDegToRad(mClampF(azimuth,0,359));
   F32 Pitch = mDegToRad(mClampF(elevation,-360,+360));
   VectorF vec;
   MathUtils::getVectorFromAngles(vec, Yaw, Pitch);
   return vec;
}

void LightSetup::initPersistFields()
{
   Parent::initPersistFields();
}

void LightSetup::renderGuiOverlays(Point2I offset, const RectI& updateRect, GuiObjectView* screen, MatrixF cameraMatrix, S32 currLight)
{
   if (!Con::getBoolVariable("$Pref::MeshLoop::showLights"))
      return;

   VectorF camDir;
   Point3F camPos;
   cameraMatrix.getColumn(1, &camDir);
   cameraMatrix.getColumn(3, &camPos);

   // render lights
   for (U32 i=0;i<mLS.size();i++) {
      if (currLight!=i && mLights[i]) {
         // render light icon
         //Point3F lp;
         //mLights[i]->getTransform().getColumn(3, &lp);
         Point3F lightPos = mLS[i].position * (F32(mLightGuiDistance)/F32(LIGHT_DISTANCE)); // lp;
         Point3F lightPosOnGrid = lightPos;
         lightPosOnGrid.z = 0.0f;
         Point3F projPos;
         Point3F projGridPos;
         screen->project( lightPos, &projPos );
         screen->project( lightPosOnGrid, &projGridPos );

         F32 camFov = mDegToRad(180.0f) / 2;
	      F32 cosCamFov = mCos(camFov);
         VectorF targetDir = lightPos - camPos;
         F32 targetDist = targetDir.lenSquared();
	      if (targetDist == 0)
		      continue;
	      targetDist = mSqrt(targetDist);

	      F32 dot = mDot(targetDir, camDir);  
         // in view cone?
	      dot /= targetDist;  
	      if (dot < cosCamFov)  
		      continue; 
   
         // draw a line to the grid
         GFX->getDrawUtil()->clearBitmapModulation();
         if (lightPos.z<0) {
            GFX->getDrawUtil()->drawLine( Point2I(projPos.x, projPos.y), Point2I(projGridPos.x, projGridPos.y), ColorI(255,0,0,128));
            GFX->getDrawUtil()->drawLine( Point2I(projGridPos.x-3, projGridPos.y), Point2I(projGridPos.x+3, projGridPos.y), ColorI(255,0,0,128));
         } else {
            GFX->getDrawUtil()->drawLine( Point2I(projPos.x, projPos.y), Point2I(projGridPos.x, projGridPos.y), ColorI(0,255,0,128));
            GFX->getDrawUtil()->drawLine( Point2I(projGridPos.x-3, projGridPos.y), Point2I(projGridPos.x+3, projGridPos.y), ColorI(0,255,0,128));
         }

         ColorF bitmapColor = mLS[i].color;
         String state = String("on");
         RectI rect(Point2I(projPos.x-8, projPos.y-8), Point2I(16,16));
         // do a check if this is where the last click fell if the LCC is dirty
         if (screen->getLCCDirty()) {
            if (rect.pointInRect(screen->getLastClickCoords())) {
               Con::executef("onLightClick", String::ToString(i).c_str());
            }
         }

         if (!mLS[i].enabled) {
            bitmapColor = ColorF(1.0f,1.0f,1.0f,0.3f);
            state = String("off");
         }
         // render the light icons
         GFX->getDrawUtil()->setBitmapModulation(bitmapColor);
         if (mLightTex)
            GFX->getDrawUtil()->drawBitmapStretch(mLightTex, rect, GFXBitmapFlip_None, GFXTextureFilterLinear);
         GFX->getDrawUtil()->clearBitmapModulation();
         GFX->getDrawUtil()->drawText( screen->getProfile()->mFont, rect.point+Point2I(20, 0), String::ToString(i+1).c_str() );
         GFX->getDrawUtil()->drawText( screen->getProfile()->mFont, rect.point+Point2I(20, 14), state.c_str() );
      }
   }
   GFX->getDrawUtil()->clearBitmapModulation();
}

bool LightSetup::onAdd()
{
   if (Parent::onAdd() == false)
      return false;

   return true;
}

void LightSetup::onRemove()
{
   Parent::onRemove();
}

void LightSetup::inspectPostApply()
{
   Parent::inspectPostApply();
}
