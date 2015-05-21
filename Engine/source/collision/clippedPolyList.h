//-----------------------------------------------------------------------------
// Torque 3D
// Copyright (C) GarageGames.com, Inc.
//-----------------------------------------------------------------------------

#ifndef _CLIPPEDPOLYLIST_H_
#define _CLIPPEDPOLYLIST_H_

#ifndef _MMATH_H_
#include "math/mMath.h"
#endif
#ifndef _TVECTORSPEC_H_
#include "core/util/tVectorSpecializations.h"
#endif
#ifndef _ABSTRACTPOLYLIST_H_
#include "collision/abstractPolyList.h"
#endif


#define CLIPPEDPOLYLIST_FLAG_ALLOWCLIPPING		0x01


/// The clipped polylist class takes the geometry passed to it and clips 
/// it against the PlaneList set.
///
/// It also contains helper functions for 
/// @see AbstractPolyList
class ClippedPolyList : public AbstractPolyList
{
   void memcpy(U32* d, U32* s,U32 size);

public:
   struct Vertex {
      Point3F point;
      U32 mask;
   };

   struct Poly {
      PlaneF plane;
      SceneObject* object;
      BaseMatInstance* material;

      U32 vertexStart;
      U32 vertexCount;
      U32 surfaceKey;
	  U32 polyFlags;
   };

   /// ???
   static bool allowClipping;

   typedef Vector<PlaneF> PlaneList;
   typedef Vector<Vertex> VertexList;
   typedef Vector<Poly> PolyList;
   typedef FastVector<U32> IndexList;

   typedef PlaneList::iterator PlaneListIterator;
   typedef VertexList::iterator VertexListIterator;
   typedef PolyList::iterator PolyListIterator;
   typedef IndexList::iterator IndexListIterator;

   // Internal data
   PolyList   mPolyList;
   VertexList mVertexList;
   IndexList  mIndexList;

   const static U32 IndexListReserveSize = 128;

   /// The per-vertex normals.
   /// @see generateNormals()
   Vector<VectorF> mNormalList;

   PlaneList mPolyPlaneList;

   /// The list of planes to clip against.
   ///
   /// This should be set before filling the polylist.
   PlaneList mPlaneList;
   
   /// If non-zero any poly facing away from this 
   /// normal is removed from the list.
   /// 
   /// This should be set before filling the polylist.
   VectorF mNormal;

   //
   ClippedPolyList();
   ~ClippedPolyList();
   void clear();

   // AbstractPolyList
   bool isEmpty() const;
   U32 addPoint(const Point3F& p);
   U32 addPlane(const PlaneF& plane);
   void begin(BaseMatInstance* material,U32 surfaceKey);
   void plane(U32 v1,U32 v2,U32 v3);
   void plane(const PlaneF& p);
   void plane(const U32 index);
   void vertex(U32 vi);
   void end();

   /// Often after clipping you'll end up with orphan verticies
   /// that are unused by the poly list.  This removes these unused
   /// verts and updates the index list.
   void cullUnusedVerts();
   
   /// This breaks all polys in the polylist into triangles.
   void triangulate();
   
   /// Generates averaged normals from the poly normals.
   /// @see mNormalList
   void generateNormals();

  protected:

   // AbstractPolyList
   const PlaneF& getIndexedPlane(const U32 index);
};


#endif
