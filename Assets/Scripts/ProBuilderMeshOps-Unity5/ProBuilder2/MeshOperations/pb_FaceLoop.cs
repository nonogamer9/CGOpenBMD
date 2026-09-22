using System.Collections.Generic;
using System.Linq;
using ProBuilder2.Common;

namespace ProBuilder2.MeshOperations
{
	public static class pb_FaceLoop
	{
		public static HashSet<pb_Face> GetFaceLoop(pb_Object pb, pb_Face[] faces, bool ring = false)
		{
			HashSet<pb_Face> hashSet = new HashSet<pb_Face>();
			List<pb_WingedEdge> wingedEdges = pb_WingedEdge.GetWingedEdges(pb);
			foreach (pb_Face face in faces)
			{
				hashSet.UnionWith(GetFaceLoop(wingedEdges, face, ring));
			}
			return hashSet;
		}

		public static HashSet<pb_Face> GetFaceRingAndLoop(pb_Object pb, pb_Face[] faces)
		{
			HashSet<pb_Face> hashSet = new HashSet<pb_Face>();
			List<pb_WingedEdge> wingedEdges = pb_WingedEdge.GetWingedEdges(pb);
			foreach (pb_Face face in faces)
			{
				hashSet.UnionWith(GetFaceLoop(wingedEdges, face, true));
				hashSet.UnionWith(GetFaceLoop(wingedEdges, face, false));
			}
			return hashSet;
		}

		public static HashSet<pb_Face> GetFaceLoop(List<pb_WingedEdge> wings, pb_Face face, bool ring)
		{
			HashSet<pb_Face> hashSet = new HashSet<pb_Face>();
			if (face == null)
			{
				return hashSet;
			}
			pb_WingedEdge pb_WingedEdge2 = wings.FirstOrDefault((pb_WingedEdge x) => x.face == face);
			if (pb_WingedEdge2 == null)
			{
				return hashSet;
			}
			if (ring)
			{
				pb_WingedEdge2 = pb_WingedEdge2.next ?? pb_WingedEdge2.previous;
			}
			for (int num = 0; num < 2; num++)
			{
				pb_WingedEdge pb_WingedEdge3 = pb_WingedEdge2;
				if (num == 1)
				{
					if (pb_WingedEdge2.opposite == null || pb_WingedEdge2.opposite.face == null)
					{
						break;
					}
					pb_WingedEdge3 = pb_WingedEdge2.opposite;
				}
				while (hashSet.Add(pb_WingedEdge3.face) && pb_WingedEdge3.Count() == 4)
				{
					pb_WingedEdge3 = pb_WingedEdge3.next.next.opposite;
					if (pb_WingedEdge3 == null || pb_WingedEdge3.face == null)
					{
						break;
					}
				}
			}
			return hashSet;
		}
	}
}
