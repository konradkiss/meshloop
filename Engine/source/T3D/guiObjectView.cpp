//------------------------------------------------------------------------------
// Torque 3D
// Copyright (C) GarageGames.com, Inc.
//------------------------------------------------------------------------------

// >>>

#include "T3D/guiObjectView.h"
#include "gfx/sim/debugDraw.h"
#include "renderInstance/renderPassManager.h"
#include "T3D/lightSetup/lightSetup.h"
#include "math/mathUtils.h"
#include "console/consoleTypes.h"
#include "core/resourceManager.h"
#include "sceneGraph/sceneGraph.h"
#include "sceneGraph/sceneState.h"
#include "sceneGraph/reflectionManager.h"
#include "postFx/postEffectManager.h"
#include "gfx/gfxDrawUtil.h"
#include "gui/core/guiCanvas.h"
#include "core/hash/md5File.h"
#include "materials/matTextureTarget.h"
#include "core/stream/fileStream.h"
#include "renderInstance/renderBinManager.h"

//-----------------------------------------------------------------------------
// GuiObjectView
//-----------------------------------------------------------------------------

GuiObjectView::GuiObjectView()
:  mMaxOrbitDist(500.0f),
   mMinOrbitDist(1.0f),
   mOrbitDist(5.0f),
   mMouseState(None),
   mLastMouseState(None),
   mModel(NULL),
   mMountedModel(NULL),
   mLastMousePoint(0, 0),
   mLastMouseWPoint(0, 0, 0),
   mLastOrbitDist(5.0f),
   lastRenderTime(0),
   seqThread(0),
   mMountNode(NO_NODE),
   mAnimationSeq(0),
   mNodeSkeletonHidden(true),
   mTransparent(false),
   mIsOrtho(false),
   mShapeRadius(10.0f),
   mGizmoProfile(NULL),
   mModelName(String::EmptyString),
   mGizmo(NULL),
   mView(0)
{
   mActive = true;

   mLastClickCoords = Point2I(0,0);
   mLCCDirty = false;

   for (U32 i=0;i<VIEWS_NUM;i++) {
      mCameraRot[i].set(0,0,0);
   }

   mLockOrbitDist = false;
   mLockXRot = false;
   mLockZRot = false;
   mLockedX = 0.0f;
   mLockedZ = 0.0f;
   mLockedOrbitDist = 10.0f;
   mOrbitOffset = Point3F(0,0,0);
   mLastOrbitOffset = Point3F(0,0,0);

	mFov = 45.0f;
	mMountedObjectScale = Point3F(1,1,1);

   LS = new LightSetup();

   mCameraMatrix.identity();
   mCameraPos.set(0.0f, 1.75f, 1.25f);
   mCameraMatrix.setColumn(3, mCameraPos);
   mOrbitPos.set(0.0f, 0.0f, 0.0f);

	mCamPosition.set(0.8f, 0.8f, 1.6f);
   mEyeCamMode	= false;

   // By default don't do dynamic reflection
   // updates for this viewport.
   mReflectPriority = 0.0f;

   mNodeTex = NULL;

   mDisplayNodes = false;

   mAnimSlider = NULL;
   mSlider = NULL;
   mSliderOverride = false;

   inited = false;

   bmiCollisionMat = NULL;
   bmiLOSMat = NULL;

   mModelRotationDegrees = 0.0f;

   loadCameraPrefs();
}

GuiObjectView::~GuiObjectView()
{
   if( mModel )
      SAFE_DELETE( mModel );
   if( mMountedModel )
      SAFE_DELETE( mMountedModel );
   if ( LS )
      SAFE_DELETE( LS );
   if ( bmiCollisionMat )
      SAFE_DELETE( bmiCollisionMat );
   if ( bmiLOSMat )
      SAFE_DELETE( bmiLOSMat );
}

bool GuiObjectView::onAdd()
{
   if(!Parent::onAdd())
      return(false);

   if ( !mGizmoProfile )
   {
      Con::errorf( "EditTSCtrl::onadd - gizmoProfile was not assigned, cannot create control!" );
      return false;
   }
   mGizmoProfile->mode = NoneMode;

   mGizmo = new Gizmo();
   mGizmo->setProfile( mGizmoProfile );
   mGizmo->registerObject();

   return true;
}

void GuiObjectView::onRemove()
{
   Parent::onRemove();

   if ( mGizmo )
      mGizmo->deleteObject();
}

bool GuiObjectView::onWake()
{
   if( !Parent::onWake() )
      return false;

   loadCameraPrefs();

   LS->wake();
   LS->updateLights(mCameraMatrix);

   return(true);
}

void GuiObjectView::onMouseDown(const GuiEvent &event)
{
   if (!mActive || !mVisible || !mAwake)
   {
      return;
   }

   mMouseState |= Rotating;
   mLastMouseState = Rotating;
   mLastMousePoint = event.mousePoint;
   mLastOrbitDist = mOrbitDist;
   mouseLock();
}

void GuiObjectView::onMouseUp(const GuiEvent &event)
{
   mouseUnlock();
   if (mMouseState & Rotating)
      mMouseState ^= Rotating;
   if (mLastMouseState == Rotating)
      mLastMouseState = mMouseState;
   mLastClickCoords = event.mousePoint;
   mLCCDirty = true;
   if (mLastMouseState != None) {
      // trigger a mouseup and mousedown on the opposite button
      onRightMouseUp(event);
      onRightMouseDown(event);
   }
}

void GuiObjectView::onMouseDragged(const GuiEvent &event)
{
   if (! (mMouseState & Rotating)) {
      return;
   }

   if (mLastMouseState != Rotating)
      return;

   if (event.modifier & SI_ALT) {
      // while dragging with the left mouse button and pressing alt, 
      // the mouse x coord change is used to zoom in / out
      F32 delta = 2;
      if (event.modifier & SI_CTRL) {
         delta = 0.2f;
      }
      if (event.modifier & SI_SHIFT) {
         delta = 20;
      }
      if (!mIsOrtho) {
         if (mView == 0) {
            mOrbitDist = mLastOrbitDist + (F32(delta) * (F32(event.mousePoint.x - mLastMousePoint.x)/100.0f));
            if (mOrbitDist<0.0f)
               mOrbitDist = 0.0f;
            String model = Con::getVariable("$model");
            Con::setVariable(String("$Pref::MeshLoop::"+model+"::orbitDist"), String::ToString("%g",mOrbitDist));
            Con::setVariable(String("$Pref::MeshLoop::orbitDist"), String::ToString("%g",mOrbitDist));
         }
      }
   } else {
      Point2I delta = event.mousePoint - mLastMousePoint;
      mLastMousePoint = event.mousePoint;

	   mCameraRot[mView].x += (delta.y * 0.01f);
	   mCameraRot[mView].z += (delta.x * 0.01f);

      // save this value
      String model = Con::getVariable("$model");
      Con::setVariable(String("$Pref::MeshLoop::"+model+"::camRot"+String::ToString(mView)), String::ToString("%g %g %g",mCameraRot[mView].x,mCameraRot[mView].y,mCameraRot[mView].z));
      Con::setVariable(String("$Pref::MeshLoop::camRot"+String::ToString(mView)), String::ToString("%g %g %g",mCameraRot[mView].x,mCameraRot[mView].y,mCameraRot[mView].z));
   }
}

void GuiObjectView::onRightMouseDown(const GuiEvent &event)
{
   mMouseState |= Panning;
   mLastMouseState = Panning;
   mLastMousePoint = event.mousePoint;
   mLastOrbitOffset = mOrbitOffset;

   Point3F screenPoint((F32)event.mousePoint.x, (F32)event.mousePoint.y, 1.0f);
   unproject(screenPoint, &mLastMouseWPoint);
   
   mouseLock();
}

void GuiObjectView::onRightMouseUp(const GuiEvent &event)
{
   mouseUnlock();
   if (mMouseState & Panning)
      mMouseState ^= Panning;
   if (mLastMouseState == Panning)
      mLastMouseState = mMouseState;

   if (mLastMouseState != None) {
      // trigger a mouseup and mousedown on the opposite button
      onMouseUp(event);
      onMouseDown(event);
   }
}

void GuiObjectView::onRightMouseDragged(const GuiEvent &event)
{
   if (!(mMouseState & Panning) || mView != 0)
      return;

   if (mLastMouseState != Panning)
      return;

   F32 vdiv = 1.0f;
   if (event.modifier & SI_CTRL) {
      vdiv = 0.5f;
   }
   if (event.modifier & SI_SHIFT) {
      vdiv = 2.0f;
   }

   Point3F screenPoint((F32)event.mousePoint.x, (F32)event.mousePoint.y, 1.0f);
   Point3F wp;
   unproject(screenPoint, &wp);
   VectorF vec = wp - mLastMouseWPoint;
   mOrbitOffset = mLastOrbitOffset - vec/(200/vdiv);

   // save this value
   String model = Con::getVariable("$model");
   Con::setVariable(String("$Pref::MeshLoop::"+model+"::orbitOffset"), String::ToString("%g %g %g",mOrbitOffset.x,mOrbitOffset.y,mOrbitOffset.z));
   Con::setVariable(String("$Pref::MeshLoop::orbitOffset"), String::ToString("%g %g %g",mOrbitOffset.x,mOrbitOffset.y,mOrbitOffset.z));
}

bool GuiObjectView::onMouseWheelDown(const GuiEvent &event)
{
   F32 delta = 0.05*mFabs(event.fval);
   if (event.modifier & SI_CTRL) {
      delta *= 0.1f;
   }
   if (event.modifier & SI_SHIFT) {
      delta *= 10;
   }

   if (!mIsOrtho) {
      if (mView > 0 && (event.modifier & SI_ALT)) {
         LS->lowerBrightness(mView-1, 0.1f, 0.0f);
      } else {
         mOrbitDist -= (delta * 0.1f);
         String model = Con::getVariable("$model");
         Con::setVariable(String("$Pref::MeshLoop::"+model+"::orbitDist"), String::ToString("%g",mOrbitDist));
         Con::setVariable(String("$Pref::MeshLoop::orbitDist"), String::ToString("%g",mOrbitDist));
      }
   } else {
      mFov += (delta * 0.01f);
      if (mFov>90) mFov=90;
      setFov(mFov); // this is just to save the variable
   }
   return true;
}

bool GuiObjectView::onMouseWheelUp(const GuiEvent &event)
{
   F32 delta = 0.05*mFabs(event.fval);
   if (event.modifier & SI_CTRL) {
      delta *= 0.1f;
   }
   if (event.modifier & SI_SHIFT) {
      delta *= 10;
   }

   if (!mIsOrtho) {
      if (mView > 0 && (event.modifier & SI_ALT)) {
         LS->raiseBrightness(mView-1, 0.1f, 20.0f);
      } else {
         mOrbitDist += (delta * 0.1f);
         if (mOrbitDist<0) mOrbitDist = 0;
         String model = Con::getVariable("$model");
         Con::setVariable(String("$Pref::MeshLoop::"+model+"::orbitDist"), String::ToString("%g",mOrbitDist));
         Con::setVariable(String("$Pref::MeshLoop::orbitDist"), String::ToString("%g",mOrbitDist));
      }
   } else {
      mFov -= (delta * 0.01f);
      if (mFov<0.1f) mFov = 0.1f;
      setFov(mFov); // this is just to save the variable
   }
   return true;
}

void GuiObjectView::advanceTime( F32 timeDelta )
{
   F32 tpos = 0.0f;

   if (!seqThread)
      return;

   if(mModel) {
      mModel->advanceTime( timeDelta, seqThread );
      tpos = mModel->getPos(seqThread);
   }
   if (!mSlider) {
      if (mAnimSlider!=NULL) {
         mSlider = dynamic_cast<GuiSliderCtrl*>(Sim::findObject(mAnimSlider));
      }
   }
   if (mSlider) {
      if (!mSliderOverride) {
         // normal operation - the slider displays the sequence's position
         mSlider->setValue(tpos);
      } else {
         // overridden operation - the slider's position becomes the sequence's position
         mModel->setPos(seqThread, mSlider->getValue());
      }
   }

   // finally, load a variable with the current thread's time position
   F32 thrdT = mModel->getTime(seqThread);
   String threadTime = String::ToString(thrdT);
   if (thrdT == mFloor(thrdT)) {
      threadTime = String(threadTime + ":000");
   } else {
      threadTime = String(threadTime.replace(".", ":") + "000");
   }
   Con::setVariable("$MeshLoop::SequenceFrameMSecs", threadTime.c_str());
}

void GuiObjectView::setObjectAnimationByName(const char* name)
{
	if(mModel) {
		S32 seq = mModel->getShape()->findSequence(name);
		if (seq>-1)
			setObjectAnimation(seq);
	}
}

ConsoleMethod( GuiObjectView, setSeqByName, void, 3, 3, "(string sequence_name)")
{
   object->setObjectAnimationByName(argv[2]);
}

void GuiObjectView::fitToShape()
{
   if ( !mModel )
      return;

   // Determine the shape bounding box given the current camera rotation
   MatrixF camRotMatrix( mCameraMatrix );
   camRotMatrix.setPosition( Point3F::Zero );
   camRotMatrix.inverse();

   Box3F bounds = mModel->getShape()->bounds;
   camRotMatrix.mul( bounds );

   // Estimate the camera distance to fill the view by comparing the radii
   // of the box and the viewport
   F32 len_x = bounds.len_x();
   F32 len_z = bounds.len_z();
   F32 shapeRadius = mSqrt( len_x*len_x + len_z*len_z ) / 2;
   F32 viewRadius = 0.45f * getMin( getExtent().x, getExtent().y );

   mOrbitOffset = Point3F(0,0,0);
   mOrbitPos = mModel->getShape()->bounds.getCenter();
   mOrbitDist = ( shapeRadius / viewRadius ) * mSaveWorldToScreenScale.y;
}

void GuiObjectView::setObjectAnimation(S32 index)
{
   if ((0 > index) || (index > mModel->getShape()->sequences.size()))
   {
      Con::errorf(avar("GuiObjectView: The index %d is outside the permissible range. Please specify an animation index in the range [0, %d]", index, mModel->getShape()->sequences.size()));
      return;
   }

   if(mModel)
   {
      mAnimationSeq = index;
      if(seqThread) {
	      mModel->setSequence(seqThread,mAnimationSeq,0);
//         if (mSlider) {
//            F32 keyFrames = (F32)mModel->getShape()->sequences[index].numKeyframes;
//            mSlider->setField("range", String("0 "+String::ToString(keyFrames)).c_str());
//         }
      }
   }
}

void GuiObjectView::initChildDepths(S32 parent, S32 depth) {
   for (U32 i=0;i<mModel->getShape()->nodes.size();i++) {
      if (mModel->getShape()->nodes[i].parentIndex==parent) {
         mNodeDepths[i] = depth;
         initChildDepths(i, depth+1);
      }
   }
}

void GuiObjectView::setObjectModel(const char* modelName)
{
	// set up the node texture
   if (!mNodeTex)
      mNodeTex.set( String("gui/images/node.png"), &GFXDefaultGUIProfile, avar("%s() - mNodeTex (line %d)", __FUNCTION__, __LINE__) );

   LS->setLightTex();

   inited = false;
   seqThread = 0;
   // let's clear node rects.. we'll rebuild them later
   mNodeRects.clear();

   // remove the shape instance, but make sure to keep a pointer to the shape
   SAFE_DELETE(mModel);

   /*
   // if this is another shape that's being loaded
   if (mModelName.isNotEmpty()) {
      // we try to remove the previous resource first
      // so it will not be kept in the cache
      ResourceManager::get().remove(mModelName.c_str());
   }
   */

   Resource<TSShape> model = ResourceManager::get().load(modelName);
   if (! bool(model))
   {
      Con::warnf(avar("GuiObjectView: Failed to load model %s. Please check your model name and load a valid model.", modelName));
      return;
   }
   mModelName = String(modelName);

   /*
   // Attempt to preload the Materials for this shape
   if ( !model->preloadMaterialList( model.getPath() ) )
   {
      model = NULL;
      return;
   }
   */

   mModel = new TSShapeInstance(model, true);
   AssertFatal(mModel, avar("GuiObjectView: Failed to load model %s. Please check your model name and load a valid model.", modelName));

   // init the material list
   mModel->initMaterialList();

   // Init node hidden state and node depth
   mNodeHidden.clear();
   mNodeDepths.clear();
   mNodePositions.clear();
   for (U32 i=0;i<mModel->getShape()->nodes.size();i++) {
      mNodeHidden.push_back(false);
      mNodeDepths.push_back(-1);
      mNodePositions.push_back(Point2F(0,0));
   }

   // Calculate node depths
   for (U32 i=0;i<mModel->getShape()->nodes.size();i++) {
      if (mModel->getShape()->nodes[i].parentIndex==-1) {
         mNodeDepths[i] = 0;
         initChildDepths(i, 1);
      }
   }

   // Initialize camera values:
   mOrbitPos = mModel->getShape()->center;

   if (mEyeCamMode) {
		mMinOrbitDist = 0;
	} else {
		mMinOrbitDist = mModel->getShape()->radius;
	}

   if (mMinOrbitDist > mMaxOrbitDist)
   {
      mMaxOrbitDist = mMinOrbitDist + 10.0f;
   }

   // the first time recording
   lastRenderTime = Platform::getVirtualMilliseconds();

   char * mountName = new char[16];
   dStrncpy(mountName, avar("mount%d", 0), 15);
   mMountNode = mModel->getShape()->findNode(mountName);
   delete [] mountName;

   seqThread = mModel->addThread();

   // we take care of setting the material list to truly be unique
   /*
   if (!mModel->ownMaterialList())
      mModel->cloneMaterialList();
   */

   // let's set the main shape to appear in the portrait
   TSMaterialList* pMatList = mModel->getMaterialList();  
   for (S32 j = 0; j < pMatList->mMatInstList.size(); j++)   
   {  
      BaseMatInstance * bmi = pMatList->getMaterialInst(j);  
      bmi->setPortrait(true);  
   }

   // now let's set anything that's mounted to this object
   // (object mounts are not yet supported)
   /*
   for (int i = 0; i < ShapeBase::MaxMountedImages; i++)
   {
      MountedImage& image = mMountedImageList[i];
      if (image.shapeInstance)
      {
         TSMaterialList* pMatList = image.shapeInstance->getMaterialList();  
         for (S32 j = 0; j < pMatList->mMatInstList.size(); j++)   
         {  
            BaseMatInstance * bmi = pMatList->getMaterialInst(j);  
            bmi->setPortrait(true);  
         }
      }
   }
   */

   // set other variables that depend on the shape

   // shape radius
   Box3F bounds = mModel->getShape()->bounds;
   F32 len_x = bounds.len_x();
   F32 len_z = bounds.len_z();
   mShapeRadius = mSqrt( len_x*len_x + len_z*len_z ) / 2;

   // light distance
   LS->setLightGuiDistance(2*mShapeRadius);
}

void GuiObjectView::loadCameraPrefs()
{
   // Load saved preferences for the cam
   String gCamRot[VIEWS_NUM];
   for (U32 i=0;i<VIEWS_NUM;i++) {
      gCamRot[i] = Con::getVariable(String("$Pref::MeshLoop::camRot"+String::ToString(i)));
   }
   String gOrbitOffset =   Con::getVariable(String("$Pref::MeshLoop::orbitOffset"));
   String gOrbitDist =     Con::getVariable(String("$Pref::MeshLoop::orbitDist"));
   for (U32 i=0;i<VIEWS_NUM;i++) {
      if (gCamRot[i].isNotEmpty())
         dSscanf(gCamRot[i], "%g %g %g", &mCameraRot[i].x,&mCameraRot[i].y,&mCameraRot[i].z);
   }
   if (gOrbitOffset.isNotEmpty())
      dSscanf(gOrbitOffset, "%g %g %g", &mOrbitOffset.x,&mOrbitOffset.y,&mOrbitOffset.z);
   if (gOrbitDist.isNotEmpty())
      dSscanf(gOrbitDist, "%g", &mOrbitDist);

   String isOrtho = Con::getVariable(String("$Pref::MeshLoop::ortho"));
   String orthoFov = Con::getVariable(String("$Pref::MeshLoop::Fov::ortho"));
   if (isOrtho.isNotEmpty())
      mIsOrtho = dAtob(isOrtho);
   if (orthoFov.isNotEmpty() && mIsOrtho)
      dSscanf(orthoFov, "%g", &mFov);


   // ---
   // now load shape specific preferences that can override global prefs
   if (Con::getBoolVariable("$Pref::MeshLoop::RememberShapeSetup")) {
      String model =          Con::getVariable("$model");
      String camRot[VIEWS_NUM];
      for (U32 i=0;i<VIEWS_NUM;i++) {
         camRot[i] = Con::getVariable(String("$Pref::MeshLoop::"+model+"::camRot"+String::ToString(i)));
      }
      String orbitOffset =    Con::getVariable(String("$Pref::MeshLoop::"+model+"::orbitOffset"));
      String orbitDist =      Con::getVariable(String("$Pref::MeshLoop::"+model+"::orbitDist"));
      for (U32 i=0;i<VIEWS_NUM;i++) {
         if (camRot[i].isNotEmpty())
            dSscanf(camRot[i], "%g %g %g", &mCameraRot[i].x,&mCameraRot[i].y,&mCameraRot[i].z);
      }
      if (orbitOffset.isNotEmpty())
         dSscanf(orbitOffset, "%g %g %g", &mOrbitOffset.x,&mOrbitOffset.y,&mOrbitOffset.z);
      if (orbitDist.isNotEmpty())
         dSscanf(orbitDist, "%g", &mOrbitDist);
   }
}



void GuiObjectView::setMountedObject(const char * modelName, S32 mountPoint)
{
   AssertFatal( mModel != NULL, "GuiObjectView::setMountedObject - model not set; can't mount to nothing" );
   SAFE_DELETE(mMountedModel);

   Resource<TSShape> model = ResourceManager::get().load(modelName);

   if (! bool(model))
   {
      Con::warnf(avar("GuiObjectView: Failed to load mounted object model %s. Please check your model name and load a valid model.", modelName));
      return;
   }

   char * mountName = new char[16];
   dStrncpy(mountName, avar("mount%d", 0), 15);
   mMountNode = mModel->getShape()->findNode(mountName);
   delete [] mountName;


   mMountedModel = new TSShapeInstance(model, true);
   AssertFatal(mMountedModel, avar("GuiObjectView: Failed to load mounted object model %s. Please check your model name and load a valid model.", modelName));
}

void GuiObjectView::getMountedObjTransform(MatrixF * mat)
{
   if ((! mMountedModel) || (mMountNode == NO_NODE))
   {
      // there is no mounted model or node to mount to
      return;
   }

   MatrixF mountedTrans;
   mountedTrans.identity();

   S32 mountPoint = mMountedModel->getShape()->findNode("mountPoint");
   if (mountPoint != -1)
      mountedTrans = mMountedModel->mNodeTransforms[mountPoint];
    
   Point3F mountedOffset = -mountedTrans.getPosition();
   MatrixF modelTrans = mModel->mNodeTransforms[mMountNode];
   modelTrans.mulP(mountedOffset);
   modelTrans.setPosition(mountedOffset);
   *mat = modelTrans;
}

bool GuiObjectView::processCameraQuery(CameraQuery* query)
{
   if (!mModel)
      return false;

   LS->readPrefs();

   // Adjust the camera so that we are still facing the model:
   Point3F vec;
   MatrixF xRot, zRot;

   query->ortho = mIsOrtho;

	if (mLockXRot)
		xRot.set(EulerF(mLockedX,0.0f,0.0f));
	else
		xRot.set(EulerF(mCameraRot[mView].x, 0.0f, 0.0f));

	if (mLockZRot)
		zRot.set(EulerF(0.0f,0.0f,mLockedZ));
	else
		zRot.set(EulerF(0.0f, 0.0f, mCameraRot[mView].z));
	
	mCameraMatrix.mul(zRot, xRot);
	mCameraMatrix.getColumn(1, &vec);

	if (mLockOrbitDist)
		vec *= mLockedOrbitDist;
	else
		vec *= mOrbitDist;
	
	if (mEyeCamMode) {
		MatrixF eyeNode;
		S32 eyeNodeId = mModel->getShape()->findName("eye");
		if (eyeNodeId>-1) {
			mModel->getShape()->getNodeWorldTransform(mModel->getShape()->findNode(eyeNodeId), &eyeNode);
			eyeNode.getColumn(3, &mOrbitPos);
		} else {
			mOrbitPos = mModel->getShape()->center + Point3F(0.0f, 0.0f, 1.6f);
		}
	} else {
		mOrbitPos = mModel->getShape()->center;
	}
   mCameraPos = ((mView==0) ? (mOrbitPos+mOrbitOffset) : mModel->getShape()->bounds.getCenter()) - vec;

   query->farPlane = 2100.0f;
   query->nearPlane = query->farPlane / 50000.0f;
   query->fov = mFov;
   mCameraMatrix.setColumn(3, mCameraPos);
   query->cameraMatrix = mCameraMatrix;

   // align lights to camera if we have to
   if (mView>0) {
      Point3F pcampos = mCameraPos-(mModel->getShape()->bounds.getCenter());
      pcampos.normalize();
      pcampos *= LS->getDistance();
      LS->setPosition(mView-1, pcampos);
      LS->setTarget(mView-1, mModel->getShape()->bounds.getCenter());
   }

   if (mModel)
      LS->updateLights(mCameraMatrix);

   return true;
}

ConsoleMethod(GuiObjectView, updateLights, void, 2, 2, "")
{
   if (object->getModel()) {
      object->getLS()->updateLights(object->getCameraMatrix());
      object->addToCameraRot(Point3F(0.0001f, 0.0f, 0.0f), object->getView());
   }
}

void GuiObjectView::onMouseEnter(const GuiEvent & event)
{
   Con::executef(this, "onMouseEnter");
}

void GuiObjectView::onMouseLeave(const GuiEvent & event)
{
   Con::executef(this, "onMouseLeave");
}

void GuiObjectView::glScale(Point3F scale) {
   MatrixF s(true);
   s.scale( scale );
   GFX->multWorld( s );
}

void GuiObjectView::renderWorld(const RectI &updateRect)
{
   if ((! mModel) && (! mMountedModel))
   {
      // nothing to render
      return;
   }
   
   // Determine the camera position, and store off render state...
   MatrixF modelview;
   MatrixF mv;
   Point3F cp;
  
   modelview = GFX->getWorldMatrix();

   mv = modelview;
   mv.inverse();
   mv.getColumn(3, &cp);

   mMatOverrideState = Normal;

   RenderPassManager *renderPass = gClientSceneGraph->getRenderPass();

   S32 time = Platform::getVirtualMilliseconds();
   //S32 dt = time - lastRenderTime;
   lastRenderTime = time;

   // should we render a grid?
   if (Con::getBoolVariable("$Pref::MeshLoop::showGrid")) {
      GFXStateBlockDesc desc;
      desc.setBlend( true );
      desc.setZReadWrite( true, true );

      F32 denseGridSize = 2*(mCeil(mShapeRadius / 10)*10);

      GFX->getDrawUtil()->drawPlaneGrid( desc, Point3F::Zero, Point2F(denseGridSize,denseGridSize), Point2F(1,1), ColorF(1,1,1,0.1f) );
      GFX->getDrawUtil()->drawPlaneGrid( desc, Point3F::Zero, Point2F(denseGridSize,denseGridSize), Point2F(0.25f,0.25f), ColorF(1,1,1,0.05f) );
      //GFX->getDrawUtil()->drawPlaneGrid( desc, Point3F::Zero, Point2F(denseGridSize*10,denseGridSize*10), Point2F(10,10), ColorF(1,1,1,0.1f) );
      //GFX->getDrawUtil()->drawPlaneGrid( desc, Point3F::Zero, Point2F(denseGridSize*10,denseGridSize*10), Point2F(2.5f,2.5f), ColorF(1,1,1,0.05f) );
   }
   // ---

   GFX->setStateBlock(mDefaultGuiSB);

   F32 left, right, top, bottom, nearPlane, farPlane;
   bool isOrtho;
   GFX->getFrustum( &left, &right, &bottom, &top, &nearPlane, &farPlane, &isOrtho);

   Frustum frust( mIsOrtho, left, right, top, bottom, nearPlane, farPlane, MatrixF::Identity );

   SceneState state(
      NULL,
      gClientSceneGraph,
      SPT_Diffuse,
      1,
      frust,
      GFX->getViewport(),
      false,
      false);

   // Set up pass transforms
   renderPass->assignSharedXform(RenderPassManager::View, MatrixF::Identity);
   renderPass->assignSharedXform(RenderPassManager::Projection, GFX->getProjectionMatrix());

   LS->updateLights(mCameraMatrix);
   if (Con::getIntVariable("$Pref::MeshLoop::LightingMode")==0) {
      // this is "No light" - a basic lighting mode feat
      // with one point light at 0 brightness and some ambient
      // so we slightly modify the lighting
      LS->getLight(0)->setColor(ColorF(0.01f,0.01f,0.01f,1));
      LS->getLight(0)->setBrightness(0.22f);
      LS->getLight(0)->setAmbient(ColorF(1,1,1,1));
      LightManager* lm = gClientSceneGraph->getLightManager();
      lm->unregisterAllLights();
      lm->registerGlobalLight(LS->getLight(0), NULL);
      lm->setupLights(NULL, SphereF( Point3F::Zero, 1.0f ) );
   } else {
      LS->registerLights();
   }

   // Set up our TS render state here.
   TSRenderState rdata;
   rdata.setSceneState( &state );

   if (mTransparent) {
      rdata.setFadeOverride( 0.5f );
   }

   // check if we need to override lod display
   if (!Con::getBoolVariable("$MeshLoop::AutoLOD", true)) {
      // force a detail (highest (0) by default)
      mModel->setCurrentDetail( Con::getIntVariable("$MeshLoop::CurrentLOD", 0) );
   } else {
      // do automatic detail selection
      mModel->setDetailFromPosAndScale( &state, mCameraMatrix.getPosition(), Point3F(1,1,1) );
      // we need to load the current detail level into our console global
      S32 currentDetailVar = Con::getIntVariable("$MeshLoop::CurrentLOD", 0);
      S32 currentDetail = mModel->getCurrentDetail();
      if (currentDetailVar != currentDetail) {
         Con::setIntVariable("$MeshLoop::CurrentLOD", mModel->getCurrentDetail());
         Con::executef(this, "onDetailChange");
      }
   }

   if (mModel)
   {
      MatrixF zRot;
      zRot.set(EulerF(0,0,mDegToRad(mModelRotationDegrees)));
      GFX->pushWorldMatrix();
      GFX->multWorld(zRot);

      mModel->animate();
      mModel->render( rdata );

      if (mMountedModel)
      {
         MatrixF mat;
         getMountedObjTransform(&mat);

         GFX->pushWorldMatrix();
         GFX->multWorld( mat );
         
		   glScale(mMountedObjectScale);
         mMountedModel->render( rdata );

         GFX->popWorldMatrix();
      }

      GFX->popWorldMatrix();
   }

   if (Con::getBoolVariable("$Pref::MeshLoop::showBounds")) {
      // bounding box
      GFXStateBlockDesc bbdesc;
      bbdesc.setZReadWrite( true, false );
      bbdesc.setBlend( true );
      bbdesc.fillMode = GFXFillWireframe;
      GFX->getDrawUtil()->drawCube( bbdesc, mModel->getShape()->bounds, ColorI( 255, 127, 127 ) );
   }

   if (!inited) {
      loadCameraPrefs();
      setView(0);
      inited = true;
   }

   renderPass->renderPass(&state);

   if (state.isDiffusePass()) {
      // render collision and los meshes
      if (Con::getBoolVariable("$Pref::MeshLoop::showCol") || Con::getBoolVariable("$Pref::MeshLoop::showLOS") ) {
         for( U32 i = 0; i < renderPass->getManagerCount(); i++ ) {
            RenderBinManager *bin = renderPass->getManager(i);
            bin->getMatOverrideDelegate().bind( this, &GuiObjectView::MatOverrideDelegate );
         }

         for (U32 i=0;i<mModel->getShape()->details.size();i++) {
            TSShape::Detail detail = mModel->getShape()->details[i];
            if (S32(detail.size)<0) {
               String meshName = mModel->getShape()->getName(detail.nameIndex);
               if (  dStrStartsWith(meshName.c_str(), "Collision") ||
                     dStrStartsWith(meshName.c_str(), "Col") ) {
                  if (Con::getBoolVariable("$Pref::MeshLoop::showCol") ) {
                     // this is a collision mesh
                     mMatOverrideState = Collision;
                     mModel->setCurrentDetail(i);
                     mModel->render( rdata );
                  }
               } else if ( dStrStartsWith(meshName.c_str(), "LOSCol") ||
                           dStrStartsWith(meshName.c_str(), "LOS") ) {
                  if (Con::getBoolVariable("$Pref::MeshLoop::showLOS") ) {
                     // this is a los mesh
                     mMatOverrideState = LOS;
                     mModel->setCurrentDetail(i);
                     mModel->render( rdata );
                  }
               }
            }
         }
         renderPass->renderPass(&state);
      }

      // 3d view axis (gizmo)
      if (Con::getBoolVariable("$Pref::MeshLoop::showAxis")) {
         Point3F gO;
         if (mView==0) {
            gO = mOrbitPos + mOrbitOffset;
         } else {
            gO = mModel->getShape()->bounds.getCenter();
         }
         mGizmoProfile->screenLen = 100;
         mGizmo->set( MatrixF::Identity, gO, Point3F(1,1,1));
         mGizmo->renderGizmo( mCameraMatrix );

         if (mDisplayNodes) {
            // let's not render gizmos for nodes now, it's not showing correct data anyway
            /*
            mGizmoProfile->screenLen = 25;
            for (U32 i=0;i<mModel->getShape()->nodes.size();i++) {
               Point3F nodeO = mModel->mNodeTransforms[i].getPosition();
               mGizmo->set( mModel->mNodeTransforms[i], nodeO, Point3F(1,1,1));
               mGizmo->renderGizmo( mCameraMatrix );
            }
            */
         }
      }
   }
}

BaseMatInstance* GuiObjectView::MatOverrideDelegate( BaseMatInstance *inMat )
{
   switch (mMatOverrideState) {
      case Collision:
         if (!bmiCollisionMat) {
            const GFXVertexFormat *vertexFormat = getGFXVertexFormat<GFXVertexPNTTB>();
            bmiCollisionMat = static_cast<BaseMatInstance*>(MATMGR->createMatInstance("MLMAT_COL", vertexFormat));
            bmiCollisionMat->setPortrait(false);
            if (!bmiCollisionMat)
               return inMat;
         }
         return bmiCollisionMat;
      case LOS:
         if (!bmiLOSMat) {
            const GFXVertexFormat *vertexFormat = getGFXVertexFormat<GFXVertexPNTTB>();
            bmiLOSMat = static_cast<BaseMatInstance*>(MATMGR->createMatInstance("MLMAT_LOS", vertexFormat));
            bmiCollisionMat->setPortrait(false);
            if (!bmiLOSMat)
               return inMat;
         }
         return bmiLOSMat;
      default:
         inMat->setPortrait(true);
         return inMat;
   }
}

void GuiObjectView::renderGuiOverlays(Point2I offset, const RectI& updateRect)
{
   GFX->pushWorldMatrix();
   GFX->setStateBlock(mDefaultGuiSB);

   F32 minz = 1.0f;
   F32 maxz = 0.0f;

   LS->renderGuiOverlays(offset, updateRect, this, mCameraMatrix, mView-1);

   // prepare node rects
   mNodeRects.clear();
   for ( S32 i = 0; i < mModel->getShape()->nodes.size(); i++) {
      const TSShape::Node& node = mModel->getShape()->nodes[i];
      const String& nodeName = mModel->getShape()->getName( node.nameIndex );

      Point3F projPos;
      Point3F tpos = mModel->mNodeTransforms[i].getPosition();

      project( tpos, &projPos );

      Point2I pos = offset + Point2I( projPos.x-4, projPos.y-4 );
      RectI ovrect = RectI(pos.x, pos.y, mProfile->mFont->getStrNWidthPrecise(nodeName.c_str(), dStrlen(nodeName.c_str()))+15, mProfile->mFont->getHeight()+5);

      // save that rect
      NodeRect nr;
      nr.name = nodeName;
      nr.rect = ovrect;
      nr.node = pos;
      nr.visible = !mNodeHidden[i];
      nr.overlapped = false;
      nr.parent = node.parentIndex;
      nr.depth = mNodeDepths[i];
      nr.rawz = projPos.z;

      if (nr.rawz<minz) minz = nr.rawz;
      if (nr.rawz>maxz) maxz = nr.rawz;

      U32 rm = nr.depth%35;
      switch (rm%7) {
         case 0:
            nr.color = ColorI(195+0,195+0,195+0,80);
            break;
         case 1:
            nr.color = ColorI(195+0,195+0,195+U32(5-(rm/5))*12,80);
            break;
         case 2:
            nr.color = ColorI(195+0,195+U32(5-(rm/5))*12,195+0,80);
            break;
         case 3:
            nr.color = ColorI(195+U32(5-(rm/5))*12,195+0,195+0,80);
            break;
         case 4:
            nr.color = ColorI(195+0,195+U32(5-(rm/5))*12,195+U32(5-(rm/5))*12,80);
            break;
         case 5:
            nr.color = ColorI(195+U32(5-(rm/5))*12,195+U32(5-(rm/5))*12,195+0,80);
            break;
         default:
            nr.color = ColorI(195+U32(5-(rm/5))*12,195+U32(5-(rm/5))*12,195+U32(5-(rm/5))*12,80);
            break;
      }

      mNodeRects.push_back(nr);
   }

   // calculate z
   for ( S32 i = 0; i < mModel->getShape()->nodes.size(); i++) {
      mNodeRects[i].z = U8((mNodeRects[i].rawz-minz)*7/(maxz-minz));
   }

   if (Con::getBoolVariable("$Pref::MeshLoop::RemoveNodeOverlapping")) {
      // keep the rects moving until we find a good enough place for all of them
      bool allsafe = false;
      S32 bailin = 10;
      while (!allsafe && bailin>0) {
         allsafe = true;
         bailin--;
         for ( S32 i = 0; i < mNodeRects.size(); i++) {
            if (mNodeHidden[i])
               continue;

            Point2I movDir;
            const char * namehash = MD5String( mNodeRects[i].name.c_str() );
            switch (namehash[i%32] % 4) {
               case 0:
                  movDir = Point2I(1,-1);
                  break;
               case 1:
                  movDir = Point2I(1,1);
                  break;
               case 2:
                  movDir = Point2I(1,0);
                  break;
               default:
                  movDir = Point2I(0,1);
                  break;
            }

            for ( S32 j = 0; j < mNodeRects.size(); j++) {
               if (i==j)
                  continue;

               if (mNodeHidden[j])
                  continue;

               while (mNodeRects[i].rect.overlaps(mNodeRects[j].rect)) {
                  mNodeRects[i].rect.point += movDir;
                  mNodeRects[j].rect.point -= movDir;
               }
            }
         }
      }

      // check if the final positions are overlapped by gui elements
      for ( S32 i = 0; i < mNodeRects.size(); i++) {
         /*
         GuiControl *hit1 = getRoot()->findHitControl(mNodeRects[i].rect.point);
         GuiControl *hit2 = getRoot()->findHitControl(Point2I(mNodeRects[i].rect.point.x+mNodeRects[i].rect.extent.x, mNodeRects[i].rect.point.y));
         GuiControl *hit3 = getRoot()->findHitControl(mNodeRects[i].rect.point+mNodeRects[i].rect.extent);
         GuiControl *hit4 = getRoot()->findHitControl(Point2I(mNodeRects[i].rect.point.x, mNodeRects[i].rect.point.y+mNodeRects[i].rect.extent.y));

         if (hit1==hit2 && hit3==hit4 && hit1==hit3 && hit1==findObject("ViewHUD")) {
            // it is visible.. at least it's not colliding with any of the gui elements
            mNodeRects[i].visible = true && mNodeRects[i].visible;
         } else {
            mNodeRects[i].visible = false;
         }

         GuiControl *hit5 = getRoot()->findHitControl(mNodeRects[i].node);

         if (hit5==findObject("ViewHUD")) {
            // it is visible.. at least it's not colliding with any of the gui elements
            mNodeRects[i].visible = true && mNodeRects[i].visible;
            mNodeRects[i].overlapped = false;
         } else {
            mNodeRects[i].visible = false;
            mNodeRects[i].overlapped = true;
         }
         */
         mNodeRects[i].visible = true && mNodeRects[i].visible;
         mNodeRects[i].overlapped = false;
      }
   }


   if (!mNodeSkeletonHidden) {
      // render node skeleton
      GFX->getDrawUtil()->clearBitmapModulation();
      for ( S32 i = 0; i < mNodeRects.size(); i++) {
         if (mNodeRects[i].parent>=0) {
            // connect this and its parent node if none of the nodes are overlapped
            if (!mNodeRects[i].overlapped && !mNodeRects[mNodeRects[i].parent].overlapped) {
               U8 col = U8((mNodeRects[i].z + mNodeRects[mNodeRects[i].parent].z)/2);
               GFX->getDrawUtil()->drawLine( mNodeRects[i].node+Point2I(4,4), mNodeRects[mNodeRects[i].parent].node+Point2I(4,4), ColorI(70-col*10,(210-col*30)+45,0,255));
            }
         }
      }
   }

   if (mDisplayNodes) {
      // render nodes
      for ( S32 i = 0; i < mNodeRects.size(); i++) {
         GFX->getDrawUtil()->setBitmapModulation(ColorI(mNodeRects[i].color,255));
         if (mNodeRects[i].visible) {
            RectI rect(mNodeRects[i].node, Point2I(8,8));
            if (mNodeTex)
               GFX->getDrawUtil()->drawBitmapStretch(mNodeTex, rect, GFXBitmapFlip_None, GFXTextureFilterLinear);

            if (Con::getBoolVariable("$Pref::MeshLoop::RemoveNodeOverlapping")) {
               if (mNodePositions[i]!=Point2F(0,0)) {
                  mNodePositions[i] = mNodePositions[i] + (Point2F(mNodeRects[i].rect.point.x, mNodeRects[i].rect.point.y) - mNodePositions[i])*0.04f;
               } else {
                  mNodePositions[i] = Point2F(mNodeRects[i].rect.point.x, mNodeRects[i].rect.point.y);
               }

               //GFX->getDrawUtil()->drawRectFill(mNodeRects[i].rect, ColorI(255,255,255,80));
               Point2I sp = Point2I(mNodePositions[i].x, mNodePositions[i].y)+(mNodeRects[i].rect.extent/2);
               Point2I ep = mNodeRects[i].node+Point2I(4,4);
               if (mAbs(ep.x-sp.x)>=mAbs(ep.y-sp.y)) {
                  if (ep.x<=sp.x) {
                     sp.x = mNodePositions[i].x;
                     //GFX->getDrawUtil()->drawLine( mNodeRects[i].rect.point, mNodeRects[i].rect.point + Point2I(0, mNodeRects[i].rect.extent.y), ColorI(255,255,255,80));
                  } else {
                     sp.x = mNodePositions[i].x + mNodeRects[i].rect.extent.x;
                     //GFX->getDrawUtil()->drawLine( mNodeRects[i].rect.point+mNodeRects[i].rect.extent, mNodeRects[i].rect.point + mNodeRects[i].rect.extent + Point2I(0, mNodeRects[i].rect.extent.y), ColorI(255,255,255,80));
                  }
               } else {
                  if (ep.y<=sp.y) {
                     sp.y = mNodePositions[i].y;
                     //GFX->getDrawUtil()->drawLine( mNodeRects[i].rect.point, mNodeRects[i].rect.point + Point2I(mNodeRects[i].rect.extent.x,0), ColorI(255,255,255,80));
                  } else {
                     sp.y = mNodePositions[i].y + mNodeRects[i].rect.extent.y;
                     //GFX->getDrawUtil()->drawLine( mNodeRects[i].rect.point+Point2I(0,mNodeRects[i].rect.extent.y), mNodeRects[i].rect.point + mNodeRects[i].rect.extent, ColorI(255,255,255,80));
                  }
               }
           
               F32 pdist = Point2I(ep-sp).len();
               if (!mNodeRects[i].rect.pointInRect(ep) || pdist<60 || mNodeRects[i].name.equal(String::EmptyString)) {
                  F32 alpha = 1.0f;
                  if (pdist<20) pdist = 20;
                  if (pdist>60) pdist = 60;
                  alpha = 1.0f - F32((pdist-20.0f)/40.0f);

                  //GFX->getDrawUtil()->setBitmapModulation(ColorI(255,255,255,255));
                  if (!mNodeRects[i].rect.pointInRect(ep)) {
                     GFX->getDrawUtil()->drawLine( sp, ep, ColorI(mNodeRects[i].color.red, mNodeRects[i].color.green, mNodeRects[i].color.blue, U8(alpha * 255.0f)));
                  }

                  GFX->getDrawUtil()->setBitmapModulation(ColorI(0,0,0,U8(alpha * 255.0f)));
                  GFX->getDrawUtil()->drawText( mProfile->mFont, Point2I(S32(mNodePositions[i].x), S32(mNodePositions[i].y))+Point2I(7, 3), mNodeRects[i].name.c_str() );
                  GFX->getDrawUtil()->setBitmapModulation(ColorI(mNodeRects[i].color,U8(alpha * 255.0f)));
                  GFX->getDrawUtil()->drawText( mProfile->mFont, Point2I(S32(mNodePositions[i].x), S32(mNodePositions[i].y))+Point2I(6, 2), mNodeRects[i].name.c_str() );
               }
   
            } else {
               // just display the nodes where they are
               //GFX->getDrawUtil()->setBitmapModulation(ColorI(0,0,0,255));
               //GFX->getDrawUtil()->drawText( mProfile->mFont, mNodeRects[i].node+Point2I(7, 3), mNodeRects[i].name.c_str() );
               GFX->getDrawUtil()->setBitmapModulation(ColorI(mNodeRects[i].color,U8(0.3f * 255.0f)));
               GFX->getDrawUtil()->drawText( mProfile->mFont, mNodeRects[i].node+Point2I(6, 2), mNodeRects[i].name.c_str() );
            }
         }
      }
   }
   // set it to false either way, we don't want to check again for this
   mLCCDirty = false;

   GFX->getDrawUtil()->clearBitmapModulation();
   GFX->popWorldMatrix();
}

void GuiObjectView::setOrbitDistance(F32 distance)
{
   // Make sure the orbit distance is within the acceptable range
   mOrbitDist = mClampF(distance, mMinOrbitDist, mMaxOrbitDist);
}

void GuiObjectView::setModelRotation(F32 degrees)
{
   mModelRotationDegrees = mClampF(mFmod(degrees, 360.0f), 0.0f, 360.0f);
}

void GuiObjectView::setOrbitOffset(Point3F offset)
{
   // Make sure the orbit distance is within the acceptable range
   mOrbitOffset = offset;
}
void GuiObjectView::setEyeCamMode(bool state)
{
	mEyeCamMode = state;
}

void GuiObjectView::setFov(F32 fov)
{
   mFov = fov;
   if (mIsOrtho) {
      String model = Con::getVariable("$model");
      Con::setVariable(String("$Pref::MeshLoop::"+model+"::Fov::ortho"), String::ToString("%g",mFov));
      Con::setVariable(String("$Pref::MeshLoop::"+model+"::ortho"), mIsOrtho ? String("1") : String("0"));
   }
}

void GuiObjectView::initPersistFields()
{
   Parent::initPersistFields();

   addGroup("Camera");
   addField("lockOrbit",		TypeBool,      Offset( mLockOrbitDist,		GuiObjectView ));
   addField("orbitDistance",	TypeF32,			Offset( mLockedOrbitDist,	GuiObjectView ));
   addField("lockXRotation",	TypeBool,		Offset( mLockXRot,			GuiObjectView ));
   addField("lockedXRotation",TypeF32,			Offset( mLockedX,				GuiObjectView ));
   addField("lockZRotation",	TypeBool,		Offset( mLockZRot,			GuiObjectView ));
   addField("lockedZRotation",TypeF32,			Offset( mLockedZ,				GuiObjectView ));
   addField("orbitOffset",		TypePoint2F,   Offset( mOrbitOffset,		GuiObjectView ));
   endGroup("Camera");

   addGroup("Other");
   addField("displayNodes",	TypeBool,      Offset( mDisplayNodes,		GuiObjectView ));
   addField("transparent",	   TypeBool,      Offset( mTransparent,		GuiObjectView ));
   addField("animSlider",	   TypeString,    Offset( mAnimSlider,		   GuiObjectView ));
   addField("sliderOverride",	TypeBool,      Offset( mSliderOverride,   GuiObjectView ));
   addField("gizmoProfile",   TypeSimObjectPtr, Offset(mGizmoProfile,   GuiObjectView ));
   endGroup("Other");
}

void GuiObjectView::onStaticModified(const char* slotName, const char* newValue)
{
   // rebuild when anything is modified.
   if (mModel)
      LS->updateLights(mCameraMatrix);
}

//-----------------------------------------------------------------------------
// Console stuff (GuiObjectView)
//-----------------------------------------------------------------------------

IMPLEMENT_CONOBJECT(GuiObjectView);

ConsoleMethod(GuiObjectView, setModel, void, 3, 3,
              "(string shapeName)\n"
              "Sets the model to be displayed in this control\n\n"
              "\\param shapeName Name of the model to display.\n")
{
   TORQUE_UNUSED( argc );
   object->setObjectModel(argv[2]);
}

ConsoleMethod(GuiObjectView, setSeq, void, 3, 3,
              "(int index)\n"
              "Sets the animation to play for the viewed object.\n\n"
              "\\param index The index of the animation to play.")
{
   TORQUE_UNUSED( argc );
   object->setObjectAnimation(dAtoi(argv[2]));
}

ConsoleMethod(GuiObjectView, setMount, void, 4, 5,
              "(string shapeName, int mountPoint)\n"
              "Mounts the given model to the specified mount point of the primary model displayed in this control.\n\n"
              "\\param shapeName Name of the model to mount."
              "\\param mountPoint Index of the mount point to be mounted to. Corresponds to \"mountPointN\" in your shape where N is the number passed here."
              "\\param scale Scaling of the mounted object in \"x z y\" format."
				  )
{
   if( !object->getModel() )
   {
      Con::errorf( "GuiObjectView::setMount - must set model first" );
      return;
   }

   Point3F scale(1.0f, 1.0f, 1.0f);
	if (argc > 4) {
		dSscanf(argv[4],"%g %g %g",
			&scale.x,&scale.y,&scale.z);
		object->setMountedObjectScale(scale);
	}

   object->setMountedObject( argv[ 2 ], dAtoi( argv[ 3 ] ) );
}

ConsoleMethod(GuiObjectView, setModelRotation, void, 3, 3, "")
{
   TORQUE_UNUSED( argc );
   object->setModelRotation(dAtof(argv[2]));
}

ConsoleMethod(GuiObjectView, setOrbitDistance, void, 3, 3,
              "(float distance)\n"
              "Sets the distance at which the camera orbits the object. Clamped to the acceptable range defined in the class by min and max orbit distances.\n\n"
              "\\param distance The distance to set the orbit to (will be clamped).")
{
   TORQUE_UNUSED( argc );
   object->setOrbitDistance(dAtof(argv[2]));
}

ConsoleMethod(GuiObjectView, setOrbitOffset, void, 3, 3, "(Point3F offset)"
              "Set the viewer camera's offset.\n\n"
              "@param offset   Orbit offset in the form of: 'x y z'")
{
   Point3F ofs(0.0f, 0.0f, 0.0f);

   dSscanf(argv[2],"%g %g %g",
      &ofs.x,&ofs.y,&ofs.z);

   object->setOrbitOffset(ofs);
}

ConsoleMethod(GuiObjectView, setCamPosition, void, 3, 3, "(Point3F pos)"
				  "Set the free camera's position.\n\n"
              "@param pos   Cam position in the form of: 'x y z'")
{
   Point3F pos(0.0f, 0.0f, 0.0f);

   dSscanf(argv[2],"%g %g %g",
      &pos.x,&pos.y,&pos.z);

   object->setCamPosition(pos);
}

ConsoleMethod(GuiObjectView, setEyeCamMode, void, 3, 3, "(bool)"
              "Indicate if free cam mode is on")
{
   object->setEyeCamMode(dAtob(argv[2]));
}

ConsoleMethod(GuiObjectView, setFov, void, 3, 3,
              "(float fov)\n")
{
   TORQUE_UNUSED( argc );
   object->setFov(dAtof(argv[2]));
}

ConsoleMethod(GuiObjectView, setMeshForceHidden, void, 4, 4, 
   "( string meshName, bool hidden )\n"
   "Set the force hidden state on the named mesh." )
{
	if (object->getModel()) {
		object->getModel()->setMeshForceHidden( argv[2], dAtob( argv[3] ) );
	}
}

ConsoleMethod(GuiObjectView, getMeshCount, S32, 2, 2, "")
{
	if (object->getModel()) {
		return object->getModel()->meshCount();
   } else {
      return 0;
   }
}

ConsoleMethod(GuiObjectView, getMeshName, const char *, 3, 3, "int meshIndex")
{
	if (object->getModel()) {
      return object->getModel()->getShape()->getMeshName( dAtoi(argv[2]) );
   } else {
      return "";
   }
}

ConsoleMethod(GuiObjectView, getTargetCount, S32, 2, 2, "")
{
	if (object->getModel()) {
		return object->getModel()->getShape()->getTargetCount();
   } else {
      return 0;
   }
}

ConsoleMethod(GuiObjectView, getTargetName, const char *, 3, 3, "int meshIndex")
{
	if (object->getModel()) {
      return object->getModel()->getShape()->getTargetName( dAtoi(argv[2]) );
   } else {
      return "";
   }
}

ConsoleMethod( GuiObjectView, fitToShape, void, 2, 2,
              "()")
{
   TORQUE_UNUSED( argc );
   object->fitToShape();
}

ConsoleMethod( GuiObjectView, loadCamPrefs, void, 2, 2,
              "()")
{
   object->loadCameraPrefs();
}

ConsoleMethod( GuiObjectView, setSkinName, void, 5, 5, "(string newskin, string oldskin, string refpath)")
{
   if (object->getModel()) {
      object->getModel()->reSkin( argv[2], argv[3], argv[4] );
   }
}

ConsoleMethod(GuiObjectView, setOrtho, void, 3, 3,
              "(bool isOrtho)\n")
{
   TORQUE_UNUSED( argc );
   object->setOrtho(dAtob(argv[2]));
}

ConsoleMethod(GuiObjectView, setTimeScale, void, 3, 3, "(float timeScale)\n")
{
   TORQUE_UNUSED( argc );
   if (object->getSeqThread() && object->getModel())
      object->getModel()->setTimeScale(object->getSeqThread(), dAtof(argv[2]));
}

ConsoleMethod(GuiObjectView, getNodeCount, S32, 2, 2, "")
{
   if (object->getModel())
      return object->getModel()->getShape()->nodes.size();
   return 0;
}

ConsoleMethod(GuiObjectView, getNodeName, const char *, 3, 3, "int nodeIndex")
{
	if (object->getModel()) {
      S32 index = dAtoi(argv[2]);
      if (index>=0 && index < object->getModel()->getShape()->nodes.size()) {
         const TSShape::Node& node = object->getModel()->getShape()->nodes[index];
         const String& nodeName = object->getModel()->getShape()->getName( node.nameIndex );
         return nodeName.c_str();
      }
   }
   return "";
}

ConsoleMethod(GuiObjectView, getParentNodeIndex, S32, 3, 3, "int nodeIndex")
{
	if (object->getModel()) {
      S32 index = dAtoi(argv[2]);
      if (index>=0 && index < object->getModel()->getShape()->nodes.size()) {
         const TSShape::Node& node = object->getModel()->getShape()->nodes[index];
         return node.parentIndex;
      }
   }
   return -1;
}

ConsoleMethod(GuiObjectView, setNodeForceHidden, void, 4, 4, "string nodeName, bool hidden")
{
   if (object->getModel()) {
      S32 index = object->getModel()->getShape()->findNode(argv[2]);
      if (index>=0 && index < object->getModel()->getShape()->nodes.size())
         object->setNodeHidden(index, dAtob(argv[3]));
   }
}

ConsoleMethod(GuiObjectView, setNodeSkeletonHidden, void, 3, 3, "bool hidden")
{
   if (object->getModel()) {
      object->setNodeSkeletonHidden(dAtob(argv[2]));
   }
}

ConsoleMethod(GuiObjectView, getNodeSkeletonHidden, bool, 2, 2, "")
{
   if (object->getModel()) {
      return object->getNodeSkeletonHidden();
   }
   return true;
}

ConsoleMethod(GuiObjectView, getSequenceCount, S32, 2, 2, "")
{
   if (object->getModel())
      return object->getModel()->getShape()->sequences.size();
   return 0;
}

ConsoleMethod(GuiObjectView, getSequenceName, const char *, 3, 3, "int sequenceIndex")
{
	if (object->getModel()) {
      S32 index = dAtoi(argv[2]);
      if (index>=0 && index < object->getModel()->getShape()->sequences.size()) {
         const TSShape::Sequence& sequence = object->getModel()->getShape()->sequences[index];
         const String& sequenceName = object->getModel()->getShape()->getName( sequence.nameIndex );
         return sequenceName.c_str();
      }
   }
   return "";
}

ConsoleMethod(GuiObjectView, getSequenceFrameCount, S32, 3, 3, "int sequenceIndex")
{
	if (object->getModel()) {
      S32 index = dAtoi(argv[2]);
      if (index>=0 && index < object->getModel()->getShape()->sequences.size()) {
         const TSShape::Sequence& sequence = object->getModel()->getShape()->sequences[index];
         const S32 sfCount = sequence.numKeyframes;
         return sfCount;
      }
   }
   return 0;
}

ConsoleMethod(GuiObjectView, getLODsCount, S32, 2, 2, "")
{
	if (object->getModel()) {
      U32 count = 0;
      for (U32 i=0;i<object->getModel()->getShape()->details.size();i++) {
         TSShape::Detail detail = object->getModel()->getShape()->details[i];
         if (S32(detail.size)>=0) {
            count++;
         }
      }
      return count;
   }
   return 0;
}

ConsoleMethod(GuiObjectView, getLODSize, S32, 3, 3, "(int index)")
{
	if (object->getModel()) {
      return S32(object->getModel()->getShape()->details[dAtoi(argv[2])].size);
   }
   return -1;
}

ConsoleMethod(GuiObjectView, getLODPolyCount, S32, 3, 3, "(int index)")
{
	if (object->getModel()) {
      return object->getModel()->getShape()->details[dAtoi(argv[2])].polyCount;
   }
   return -1;
}

void GuiObjectView::reInstanceModel()
{
   Resource<TSShape> model = ResourceManager::get().load(mModelName);
   if (! bool(model))
   {
      Con::warnf(avar("GuiObjectView: Failed to reInstance model %s.", mModelName));
      return;
   }
   mModel = new TSShapeInstance(model, true);
}

ConsoleMethod(GuiObjectView, addSequence, void, 4, 4, "string path, string sequence_name")
{
	if (object->getModel()) {
      object->getModel()->getShape()->addSequence(argv[2], argv[3], argv[3], 0, -1);
      object->reInstanceModel();
   }
}

ConsoleMethod(GuiObjectView, removeSequence, void, 3, 3, "string sequenceName")
{
	if (object->getModel()) {
      object->getModel()->getShape()->removeSequence(argv[2]);
      object->reInstanceModel();
   }
}

ConsoleMethod( GuiObjectView, playSequence, void, 3, 3, "(int sequenceIndex)")
{
   object->setObjectAnimation(dAtoi(argv[2]));
}

ConsoleMethod( GuiObjectView, setSequencePosition, void, 3, 3, "(float position)")
{
	if (object->getModel()) {
      object->getModel()->setPos(object->getSeqThread(), dAtof(argv[2]));
   }
}

ConsoleMethod(GuiObjectView, isBlend, bool, 3, 3, "int sequenceIndex")
{
	if (object->getModel()) {
      S32 index = dAtoi(argv[2]);
      if (index>=0 && index < object->getModel()->getShape()->sequences.size()) {
         const TSShape::Sequence& sequence = object->getModel()->getShape()->sequences[index];
         return sequence.isBlend();
      }
   }
   return false;
}

ConsoleMethod(GuiObjectView, isCyclic, bool, 3, 3, "int sequenceIndex")
{
	if (object->getModel()) {
      S32 index = dAtoi(argv[2]);
      if (index>=0 && index < object->getModel()->getShape()->sequences.size()) {
         const TSShape::Sequence& sequence = object->getModel()->getShape()->sequences[index];
         return sequence.isCyclic();
      }
   }
   return false;
}

ConsoleMethod(GuiObjectView, setCyclic, void, 4, 4, "int sequenceIndex, bool state")
{
	if (object->getModel()) {
      S32 index = dAtoi(argv[2]);
      if (index>=0 && index < object->getModel()->getShape()->sequences.size()) {
         if (dAtob(argv[3])) {
            object->getModel()->getShape()->sequences[index].setShapeFlags(TSShape::Cyclic);
         } else {
            object->getModel()->getShape()->sequences[index].clearShapeFlags(TSShape::Cyclic);
         }
      }
   }
}

ConsoleMethod(GuiObjectView, getDuration, F32, 3, 3, "int sequenceIndex")
{
	if (object->getModel()) {
      S32 index = dAtoi(argv[2]);
      if (index>=0 && index < object->getModel()->getShape()->sequences.size()) {
         const TSShape::Sequence& sequence = object->getModel()->getShape()->sequences[index];
         return sequence.duration;
      }
   }
   return -1;
}

void GuiObjectView::setView(U32 view)
{
   loadCameraPrefs();
   if (view<VIEWS_NUM) {
      mView = view;
   }
   // Adjust the camera so that we are still facing the model:
   Point3F vec;
   MatrixF xRot, zRot;
	xRot.set(EulerF(mCameraRot[mView].x, 0.0f, 0.0f));
	zRot.set(EulerF(0.0f, 0.0f, mCameraRot[mView].z));
	mCameraMatrix.mul(zRot, xRot);
	mCameraMatrix.getColumn(1, &vec);
	vec *= mOrbitDist;
	mOrbitPos = mModel->getShape()->center;
	mCameraPos = (mOrbitPos+mOrbitOffset) - vec;
   mCameraMatrix.setColumn(3, mCameraPos);
}

ConsoleMethod(GuiObjectView, setView, void, 3, 3, "int view")
{
	if (object->getModel()) {
      if (!object->getOrtho()) {
         object->setView(dAtoi(argv[2]));
      } else {
         object->setView(0);
      }
   }
}

ConsoleMethod(GuiObjectView, getView, S32, 2, 2, "")
{
   return object->getView();
}

void GuiObjectView::capture(const char * filename)
{
   MatTextureTarget *texTarget = MatTextureTarget::findTargetByName( "portrait" );
   if (!texTarget)
      return;

   GFXTextureObject *texObject = texTarget->getTargetTexture(0);
   if (!texObject)
      return;

   GBitmap bmp( texObject->getWidth(), texObject->getHeight(), false, GFXFormatR8G8B8A8 );
   bmp.fill(ColorI(0,0,0,0));
   texObject->copyToBmp( &bmp );

   // we'll need to clamp alpha to 1 where it's not 0 to overcome opaque materials
   // rendering with very low alpha for some reason
   for ( S32 y=0; y < bmp.getHeight(); y++ ) {
      for ( S32 x=0; x < bmp.getWidth(); x++ ) {
         ColorI col;
         bmp.getColor(x, y, col);

         if (col.alpha==0)
            continue;

         // alpha fix
         col.alpha = 255;
         
         bmp.setColor(x, y, col);
      }
   }

   // save the file
   FileStream  stream;
   stream.open( filename, Torque::FS::File::Write );
   if ( stream.getStatus() == Stream::Ok )
      bmp.writeBitmap("png", stream);
}

ConsoleMethod( GuiObjectView, capture, void, 3, 3, "string capture_filename")
{
   object->capture(argv[2]);
}

ConsoleMethod( GuiObjectView, getGizmo, S32, 2, 2, "" )
{
   return object->getGizmo()->getId();
}
// <<<