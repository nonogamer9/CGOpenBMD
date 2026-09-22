using System.Collections.Generic;
using System.Linq;
using ProBuilder2.Common;

namespace ProBuilder2.MeshOperations
{
	public static class pb_ConformNormals
	{
		public static pb_ActionResult ConformNormals(this pb_Object pb, IList<pb_Face> faces)
		{
			List<pb_WingedEdge> wingedEdges = pb_WingedEdge.GetWingedEdges(pb, faces);
			HashSet<pb_Face> hashSet = new HashSet<pb_Face>();
			int num = 0;
			for (int i = 0; i < wingedEdges.Count; i++)
			{
				if (hashSet.Contains(wingedEdges[i].face))
				{
					continue;
				}
				Dictionary<pb_Face, bool> dictionary = new Dictionary<pb_Face, bool>();
				GetWindingFlags(wingedEdges[i], true, dictionary);
				int num2 = 0;
				foreach (KeyValuePair<pb_Face, bool> item in dictionary)
				{
					num2 += (item.Value ? 1 : (-1));
				}
				bool flag = num2 > 0;
				foreach (KeyValuePair<pb_Face, bool> item2 in dictionary)
				{
					if (flag != item2.Value)
					{
						num++;
						item2.Key.ReverseIndices();
					}
				}
				hashSet.UnionWith(dictionary.Keys);
			}
			if (num > 0)
			{
				return new pb_ActionResult(Status.Success, (num <= 1) ? "Flipped 1 face" : string.Format("Flipped {0} faces", num));
			}
			return new pb_ActionResult(Status.NoChange, "Faces Uniform");
		}

		private static void GetWindingFlags(pb_WingedEdge edge, bool flag, Dictionary<pb_Face, bool> flags)
		{
			flags.Add(edge.face, flag);
			pb_WingedEdge pb_WingedEdge2 = edge;
			do
			{
				pb_WingedEdge opposite = pb_WingedEdge2.opposite;
				if (opposite != null && !flags.ContainsKey(opposite.face))
				{
					pb_Edge commonEdgeInWindingOrder = GetCommonEdgeInWindingOrder(pb_WingedEdge2);
					pb_Edge commonEdgeInWindingOrder2 = GetCommonEdgeInWindingOrder(opposite);
					GetWindingFlags(opposite, (commonEdgeInWindingOrder.x != commonEdgeInWindingOrder2.x) ? flag : (!flag), flags);
				}
				pb_WingedEdge2 = pb_WingedEdge2.next;
			}
			while (pb_WingedEdge2 != edge);
		}

		public static pb_ActionResult ConformOppositeNormal(pb_WingedEdge source)
		{
			if (source == null || source.opposite == null)
			{
				return new pb_ActionResult(Status.Failure, "Source edge does not share an edge with another face.");
			}
			pb_Edge commonEdgeInWindingOrder = GetCommonEdgeInWindingOrder(source);
			pb_Edge commonEdgeInWindingOrder2 = GetCommonEdgeInWindingOrder(source.opposite);
			if (commonEdgeInWindingOrder.x == commonEdgeInWindingOrder2.x)
			{
				source.opposite.face.ReverseIndices();
				return new pb_ActionResult(Status.Success, "Reversed target face winding order.");
			}
			return new pb_ActionResult(Status.NoChange, "Faces already unified.");
		}

		private static pb_Edge GetCommonEdgeInWindingOrder(pb_WingedEdge wing)
		{
			int[] indices = wing.face.indices;
			int num = indices.Length;
			for (int i = 0; i < num; i += 3)
			{
				pb_Edge local = wing.edge.local;
				int num2 = indices[i];
				int num3 = indices[i + 1];
				int num4 = indices[i + 2];
				if (local.x == num2 && local.y == num3)
				{
					return new pb_Edge(wing.edge.common);
				}
				if (local.x == num3 && local.y == num2)
				{
					return new pb_Edge(wing.edge.common.y, wing.edge.common.x);
				}
				if (local.x == num3 && local.y == num4)
				{
					return new pb_Edge(wing.edge.common);
				}
				if (local.x == num4 && local.y == num3)
				{
					return new pb_Edge(wing.edge.common.y, wing.edge.common.x);
				}
				if (local.x == num4 && local.y == num2)
				{
					return new pb_Edge(wing.edge.common);
				}
				if (local.x == num2 && local.y == num4)
				{
					return new pb_Edge(wing.edge.common.y, wing.edge.common.x);
				}
			}
			return pb_Edge.Empty;
		}

		public static void MatchNormal(pb_Face source, pb_Face target, Dictionary<int, int> lookup)
		{
			List<pb_EdgeLookup> list = pb_EdgeLookup.GetEdgeLookup(source.edges, lookup).ToList();
			List<pb_EdgeLookup> list2 = pb_EdgeLookup.GetEdgeLookup(target.edges, lookup).ToList();
			bool flag = false;
			int num = 0;
			while (!flag && num < list.Count)
			{
				pb_Edge common = list[num].common;
				int num2 = 0;
				while (!flag && num2 < list2.Count)
				{
					pb_Edge common2 = list2[num2].common;
					if (common.Equals(common2))
					{
						if (common.x == common2.x)
						{
							target.ReverseIndices();
						}
						flag = true;
					}
					num2++;
				}
				num++;
			}
		}
	}
}
