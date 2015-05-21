#ifndef _TEXTARGETBIN_MGR_H_
#include "renderInstance/renderTexTargetBinManager.h"
#endif

class PostEffect;

class RenderPortraitMgr : public RenderTexTargetBinManager
{
   typedef RenderTexTargetBinManager Parent;

public:

   RenderPortraitMgr();
   virtual ~RenderPortraitMgr();

   // RenderBinManager
   virtual RenderBinManager::AddInstResult addElement( RenderInst *inst );
   virtual void render( SceneState *state );

   // ConsoleObject
   DECLARE_CONOBJECT( RenderPortraitMgr );

protected:

   SimObjectPtr<PostEffect> mPortraitEffect;

};