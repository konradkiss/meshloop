//-----------------------------------------------------------------------------
// Torque 3D
// Copyright (C) GarageGames.com, Inc.
//-----------------------------------------------------------------------------

// >>>

#ifndef _GUIOBJECTVIEW_H_
#define _GUIOBJECTVIEW_H_

#include "gui/3d/guiTSControl.h"
#include "ts/tsShapeInstance.h"
#include "core/itickable.h"
#include "gui/controls/guiSliderCtrl.h"
#include "T3D/lightSetup/lightSetup.h"
#include "gui/worldEditor/gizmo.h"
#include "materials/materialManager.h"

class LightSetup;

class GuiObjectView : public GuiTSCtrl, public virtual ITickable
{
private:
   typedef GuiTSCtrl Parent;

   bool inited;

protected:
   enum MouseState
   {
      None = 0,
      Rotating = 1,
      Panning  = 2,
      Zooming  = 4
   };

   enum MatOverrideState
   {
      Normal = 0,
      Collision = 1,
      LOS = 2
   };

   MatOverrideState mMatOverrideState;

   enum
   {
      VIEWS_NUM = 4                    // number of camera views (cam + lights)
   };

   LightSetup* LS;

   U8 mMouseState;
   U8 mLastMouseState;

   TSShapeInstance*  mModel;
   String            mModelName;
   TSShapeInstance*  mMountedModel;

   U32   mSkinTag;

   bool			mLockXRot;
   bool			mLockZRot;
   bool			mLockOrbitDist;
   F32			mLockedX;
   F32			mLockedZ;
   F32			mLockedOrbitDist;

	Point3F		mOrbitOffset;
	Point3F		mLastOrbitOffset;
	Point3F		mCamPosition;
	bool			mEyeCamMode;
	F32			mFov;
	Point3F		mMountedObjectScale;

   F32         mShapeRadius;

   Point3F     mCameraPos;
   MatrixF     mCameraMatrix;
   EulerF      mCameraRot[VIEWS_NUM];
   Point3F     mOrbitPos;
   S32         mMountNode;

   TSThread *  seqThread;
   S32         lastRenderTime;
   S32         mAnimationSeq;

   Point2I     mLastMousePoint;
   Point3F     mLastMouseWPoint;
   F32         mLastOrbitDist;
   Point2I     mLastClickCoords;
   bool        mLCCDirty;

   bool        mIsOrtho;

   U32         mView;

   struct NodeRect {
      String   name;
      RectI    rect;
      Point2I  node;
      bool     visible;
      ColorI   color;
      S32      parent;
      U32      depth;
      bool     overlapped;
      F32      rawz;
      U8       z;
   };
   Vector<NodeRect>  mNodeRects;
   Vector<bool>      mNodeHidden;
   Vector<U32>       mNodeDepths;
   Vector<Point2F>   mNodePositions;

   bool mNodeSkeletonHidden;

   GFXTexHandle      mNodeTex;

   bool              mTransparent;

   bool              mInitLightPositions;

   const char *      mAnimSlider;
   GuiSliderCtrl*    mSlider;
   bool              mSliderOverride;

   SimObjectPtr<Gizmo> mGizmo;
   GizmoProfile *mGizmoProfile;

   F32               mModelRotationDegrees;

public:
   bool onWake();

   void onMouseEnter(const GuiEvent &event);
   void onMouseLeave(const GuiEvent &event);
   void onMouseDown(const GuiEvent &event);
   void onMouseUp(const GuiEvent &event);
   void onMouseDragged(const GuiEvent &event);
   void onRightMouseDown(const GuiEvent &event);
   void onRightMouseUp(const GuiEvent &event);
   void onRightMouseDragged(const GuiEvent &event);
   bool onMouseWheelDown(const GuiEvent &event);
   bool onMouseWheelUp(const GuiEvent &event);

   TSShapeInstance* getModel();
   TSShapeInstance* getMountedModel();

   BaseMatInstance* bmiCollisionMat;
   BaseMatInstance* bmiLOSMat;

   BaseMatInstance* MatOverrideDelegate( BaseMatInstance *inMat );

   U32 getMatOverrideState() { return mMatOverrideState; };

   void setObjectModel(const char * modelName);
   void setObjectAnimation(S32 index);
   void setMountedObject(const char * modelName, S32 mountPoint);
   void getMountedObjTransform(MatrixF *mat);
   void reInstanceModel();

   void initChildDepths(S32 parent, S32 depth);
   void setNodeHidden(U32 index, bool hidden) { mNodeHidden[index] = hidden; };
   void setNodeSkeletonHidden(bool hidden) { mNodeSkeletonHidden = hidden; };
   bool getNodeSkeletonHidden() { return mNodeSkeletonHidden; };

   F32 getShapeRadius() { return mShapeRadius; };
   
   void onStaticModified(const char* slotName, const char* newValue);
   void setObjectAnimationByName(const char* name);
   virtual void interpolateTick( F32 delta ) {};
   virtual void processTick() {};
   virtual void advanceTime( F32 timeDelta );

   void fitToShape();
   void loadCameraPrefs();

   void setView(U32 view);
   S32 getView() { return mView; };

   LightSetup* getLS() { return LS; };
   MatrixF getCameraMatrix() { return mCameraMatrix; };
   void addToCameraRot(Point3F rot, U32 view) { mCameraRot[view] += rot; };

   bool getLCCDirty() { return mLCCDirty; };
   Point2I getLastClickCoords() { return mLastClickCoords; };
   GuiControlProfile* getProfile() { return mProfile; };

	void setOrbitOffset(Point3F offset);
	void setEyeCamMode(bool state);
	void setCamPosition(Point3F pos) { mCamPosition = pos; };
	void setFov(F32 fov);
   void setOrtho(bool ortho) { mIsOrtho = ortho; };
   bool getOrtho() { return mIsOrtho; };
   void glScale(Point3F scale);
	void setMountedObjectScale(Point3F scale) { mMountedObjectScale = scale; };

   TSThread * getSeqThread() { return seqThread; };

   /// Sets the distance at which the camera orbits the object. Clamped to the
   /// acceptable range defined in the class by min and max orbit distances.
   ///
   /// \param distance The distance to set the orbit to (will be clamped).
   void setOrbitDistance(F32 distance);

   void setModelRotation(F32 degrees = 0.0f);

   void capture(const char * filename);

   bool processCameraQuery(CameraQuery *query);
   void renderWorld(const RectI &updateRect);
   void renderGuiOverlays(Point2I offset, const RectI &updateRect);

   bool onAdd();
   void onRemove();

   Gizmo* getGizmo() { return mGizmo; };

   DECLARE_CONOBJECT(GuiObjectView);

   static void initPersistFields();

   GuiObjectView();
   ~GuiObjectView();

private:
   F32         mMaxOrbitDist;
   F32         mMinOrbitDist;
   F32         mOrbitDist;

   bool        mDisplayNodes;

   static const S32 NO_NODE         =  -1;   ///< Indicates there is no node with a mounted object
};

inline TSShapeInstance* GuiObjectView::getModel()
{
   return mModel;
}

inline TSShapeInstance* GuiObjectView::getMountedModel()
{
   return mMountedModel;
}

#endif

// <<<