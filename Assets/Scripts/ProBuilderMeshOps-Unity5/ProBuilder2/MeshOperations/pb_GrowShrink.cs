using System.Collections.Generic;
using ProBuilder2.Common;
using UnityEngine;

namespace ProBuilder2.MeshOperations
{
	public static class pb_GrowShrink
	{
		private static readonly Vector3 Vector3_Zero = new Vector3(0f, 0f, 0f);

		public static HashSet<pb_Face> GrowSelection(pb_Object pb, IList<pb_Face> faces, float maxAngleDiff = -1f)
		{
			List<pb_WingedEdge> wingedEdges = pb_WingedEdge.GetWingedEdges(pb, true);
			HashSet<pb_Face> hashSet = new HashSet<pb_Face>(faces);
			HashSet<pb_Face> hashSet2 = new HashSet<pb_Face>();
			Vector3 vector = Vector3.zero;
			bool flag = maxAngleDiff > 0f;
			for (int i = 0; i < wingedEdges.Count; i++)
			{
				if (!hashSet.Contains(wingedEdges[i].face))
				{
					continue;
				}
				if (flag)
				{
					vector = pb_Math.Normal(pb, wingedEdges[i].face);
				}
				foreach (pb_WingedEdge item in wingedEdges[i])
				{
					if (item.opposite == null || hashSet.Contains(item.opposite.face))
					{
						continue;
					}
					if (flag)
					{
						Vector3 to = pb_Math.Normal(pb, item.opposite.face);
						if (Vector3.Angle(vector, to) < maxAngleDiff)
						{
							hashSet2.Add(item.opposite.face);
						}
					}
					else
					{
						hashSet2.Add(item.opposite.face);
					}
				}
			}
			return hashSet2;
		}

		public static void Flood(pb_WingedEdge wing, HashSet<pb_Face> selection)
		{
			Flood(null, wing, Vector3_Zero, -1f, selection);
		}

		public static void Flood(pb_Object pb, pb_WingedEdge wing, Vector3 wingNrm, float maxAngle, HashSet<pb_Face> selection)
		{
			pb_WingedEdge pb_WingedEdge2 = wing;
			do
			{
				pb_WingedEdge opposite = pb_WingedEdge2.opposite;
				if (opposite != null && !selection.Contains(opposite.face))
				{
					if (maxAngle > 0f)
					{
						Vector3 vector = pb_Math.Normal(pb, opposite.face);
						if (Vector3.Angle(wingNrm, vector) < maxAngle && selection.Add(opposite.face))
						{
							Flood(pb, opposite, vector, maxAngle, selection);
						}
					}
					else if (selection.Add(opposite.face))
					{
						Flood(pb, opposite, wingNrm, maxAngle, selection);
					}
				}
				pb_WingedEdge2 = pb_WingedEdge2.next;
			}
			while (pb_WingedEdge2 != wing);
		}

		public static HashSet<pb_Face> FloodSelection(pb_Object pb, IList<pb_Face> faces, float maxAngleDiff)
		{
			List<pb_WingedEdge> wingedEdges = pb_WingedEdge.GetWingedEdges(pb, true);
			HashSet<pb_Face> hashSet = new HashSet<pb_Face>(faces);
			HashSet<pb_Face> hashSet2 = new HashSet<pb_Face>();
			for (int i = 0; i < wingedEdges.Count; i++)
			{
				if (!hashSet2.Contains(wingedEdges[i].face) && hashSet.Contains(wingedEdges[i].face))
				{
					hashSet2.Add(wingedEdges[i].face);
					Flood(pb, wingedEdges[i], (!(maxAngleDiff > 0f)) ? Vector3_Zero : pb_Math.Normal(pb, wingedEdges[i].face), maxAngleDiff, hashSet2);
				}
			}
			return hashSet2;
		}
	}
}
