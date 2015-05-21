#include "platform/platform.h"
#include "renderInstance/renderPortraitMgr.h"

#include "sceneGraph/sceneGraph.h"
#include "sceneGraph/sceneState.h"
#include "materials/sceneData.h"
#include "materials/matInstance.h"
//#include "materials/materialFeatureTypes.h"
#include "materials/processedMaterial.h"
#include "postFx/postEffect.h"
#include "gfx/gfxTransformSaver.h"
#include "gfx/gfxDebugEvent.h"
#include "math/util/matrixSet.h"
#include "T3D/guiObjectView.h"

IMPLEMENT_CONOBJECT( RenderPortraitMgr );

RenderPortraitMgr::RenderPortraitMgr()
   : RenderTexTargetBinManager(  RenderPassManager::RIT_Mesh,
                                 1.0f,
                                 1.0f,
                                 GFXFormatR8G8B8A8,
                                 Point2I( 512, 512 ) )
{
   mTargetSizeType = WindowSize;
   MatTextureTarget::registerTarget( "portrait", this );
}

RenderPortraitMgr::~RenderPortraitMgr()
{
   MatTextureTarget::unregisterTarget( "portrait", this );
}

RenderBinManager::AddInstResult RenderPortraitMgr::addElement( RenderInst *inst )
{
	BaseMatInstance* matInst = getMaterial(inst);
	bool inPortrait = matInst && matInst->inPortrait();
   if ( !inPortrait )
      return RenderBinManager::arSkipped;

   internalAddElement(inst);

   return RenderBinManager::arAdded;
}


void RenderPortraitMgr::render( SceneState *state )
{
   PROFILE_SCOPE( RenderPortraitMgr_Render );

   // Don't allow non-diffuse passes.
   if ( !state->isDiffusePass() )
      return;

   // even if this is a diffuse pass, we have several of those.. we need the one
   // where the materials are not overridden in the GuiObjectView..
   GuiObjectView* view;
   if (Sim::findObject("View", view)) {
      if (view->getMatOverrideState()>0)
         return;
   } else {
      // we skip this if View doesn't exist
      return;
   }

   GFXDEBUGEVENT_SCOPE( RenderPortraitMgr_Render, ColorI::GREEN );

   GFXTransformSaver saver;

    // Restore transforms
   MatrixSet &matrixSet = getParentManager()->getMatrixSet();
   matrixSet.restoreSceneViewProjection();

   // Tell the superclass we're about to render, preserve contents
   const bool isRenderingToTarget = _onPreRender( state );

   // Clear all the buffers to transparent black
   GFX->clear( GFXClearTarget, ColorI(0,0,0,0), 1.0f, 0);

   // init loop data
   SceneGraphData sgData;
   U32 binSize = mElementList.size();

   for( U32 j=0; j<binSize; )
   {
      MeshRenderInst *ri = static_cast<MeshRenderInst*>(mElementList[j].inst);

      setupSGData( ri, sgData );
      sgData.binType = SceneGraphData::PortraitBin;

      BaseMatInstance *mat = ri->matInst;

      U32 matListEnd = j;

      while( mat->setupPass( state, sgData ) )
      {
         U32 a;
         for( a=j; a<binSize; a++ )
         {
            MeshRenderInst *passRI = static_cast<MeshRenderInst*>(mElementList[a].inst);

            if (newPassNeeded(mat, passRI))
               break;

            matrixSet.setWorld(*passRI->objectToWorld);
            matrixSet.setView(*passRI->worldToCamera);
            matrixSet.setProjection(*passRI->projection);
            mat->setTransforms(matrixSet, state);

            mat->setBuffers(passRI->vertBuff, passRI->primBuff);

            if ( passRI->prim )
               GFX->drawPrimitive( *passRI->prim );
            else
               GFX->drawPrimitive( passRI->primBuffIndex );
         }
         matListEnd = a;
      }

      // force increment if none happened, otherwise go to end of batch
      j = ( j == matListEnd ) ? j+1 : matListEnd;
   }

   // Finish up.
   if ( isRenderingToTarget )
      _onPostRender();
}