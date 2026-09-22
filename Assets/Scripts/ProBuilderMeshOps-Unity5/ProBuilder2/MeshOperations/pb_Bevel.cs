using System;
using System.Collections.Generic;
using System.Linq;
using ProBuilder2.Common;
using UnityEngine;

namespace ProBuilder2.MeshOperations
{
	public static class pb_Bevel
	{
		private static readonly int[] BRIDGE_INDICES_NRM = new int[3] { 2, 1, 0 };

		public static pb_ActionResult BevelEdges(pb_Object pb, IList<pb_Edge> edges, float amount, out List<pb_Face> createdFaces)
		{
			createdFaces = null;
			Dictionary<int, int> lookup = pb.sharedIndices.ToDictionary();
			List<pb_Vertex> list = new List<pb_Vertex>(pb_Vertex.GetVertices(pb));
			List<pb_EdgeLookup> list2 = pb_EdgeLookup.GetEdgeLookup(edges, lookup).Distinct().ToList();
			List<pb_WingedEdge> wingedEdges = pb_WingedEdge.GetWingedEdges(pb);
			List<pb_FaceRebuildData> list3 = new List<pb_FaceRebuildData>();
			Dictionary<pb_Face, List<int>> ignore = new Dictionary<pb_Face, List<int>>();
			HashSet<int> hashSet = new HashSet<int>();
			int num = 0;
			Dictionary<int, List<pb_Tuple<pb_FaceRebuildData, List<int>>>> dictionary = new Dictionary<int, List<pb_Tuple<pb_FaceRebuildData, List<int>>>>();
			Dictionary<int, List<pb_WingedEdge>> spokes = pb_WingedEdge.GetSpokes(wingedEdges);
			HashSet<int> hashSet2 = new HashSet<int>();
			foreach (pb_EdgeLookup item in list2)
			{
				if (hashSet2.Add(item.common.x))
				{
					foreach (pb_WingedEdge item2 in spokes[item.common.x])
					{
						pb_Edge local = item2.edge.local;
						amount = Mathf.Min(Vector3.Distance(list[local.x].position, list[local.y].position) - 0.001f, amount);
					}
				}
				if (!hashSet2.Add(item.common.y))
				{
					continue;
				}
				foreach (pb_WingedEdge item3 in spokes[item.common.y])
				{
					pb_Edge local2 = item3.edge.local;
					amount = Mathf.Min(Vector3.Distance(list[local2.x].position, list[local2.y].position) - 0.001f, amount);
				}
			}
			if (amount < 0.001f)
			{
				return new pb_ActionResult(Status.Canceled, "Bevel Distance > Available Surface");
			}
			foreach (pb_EdgeLookup lup in list2)
			{
				pb_WingedEdge pb_WingedEdge2 = wingedEdges.FirstOrDefault((pb_WingedEdge x) => x.edge.Equals(lup));
				if (pb_WingedEdge2 != null && pb_WingedEdge2.opposite != null)
				{
					num++;
					ignore.AddOrAppend(pb_WingedEdge2.face, pb_WingedEdge2.edge.common.x);
					ignore.AddOrAppend(pb_WingedEdge2.face, pb_WingedEdge2.edge.common.y);
					ignore.AddOrAppend(pb_WingedEdge2.opposite.face, pb_WingedEdge2.edge.common.x);
					ignore.AddOrAppend(pb_WingedEdge2.opposite.face, pb_WingedEdge2.edge.common.y);
					hashSet.Add(pb_WingedEdge2.edge.common.x);
					hashSet.Add(pb_WingedEdge2.edge.common.y);
					SlideEdge(list, pb_WingedEdge2, amount);
					SlideEdge(list, pb_WingedEdge2.opposite, amount);
					list3.AddRange(GetBridgeFaces(list, pb_WingedEdge2, pb_WingedEdge2.opposite, dictionary));
				}
			}
			if (num < 1)
			{
				createdFaces = null;
				return new pb_ActionResult(Status.Canceled, "Cannot Bevel Open Edges");
			}
			createdFaces = new List<pb_Face>(list3.Select((pb_FaceRebuildData x) => x.face));
			Dictionary<pb_Face, List<pb_Tuple<pb_WingedEdge, int>>> dictionary2 = new Dictionary<pb_Face, List<pb_Tuple<pb_WingedEdge, int>>>();
			foreach (int c in hashSet)
			{
				IEnumerable<pb_WingedEdge> enumerable = wingedEdges.Where((pb_WingedEdge x) => x.edge.common.Contains(c) && (!ignore.ContainsKey(x.face) || !ignore[x.face].Contains(c)));
				HashSet<pb_Face> hashSet3 = new HashSet<pb_Face>();
				foreach (pb_WingedEdge item4 in enumerable)
				{
					if (hashSet3.Add(item4.face))
					{
						dictionary2.AddOrAppend(item4.face, new pb_Tuple<pb_WingedEdge, int>(item4, c));
					}
				}
			}
			foreach (KeyValuePair<pb_Face, List<pb_Tuple<pb_WingedEdge, int>>> item5 in dictionary2)
			{
				Dictionary<int, List<int>> appendedVertices;
				pb_FaceRebuildData pb_FaceRebuildData2 = pbVertexOps.ExplodeVertex(list, item5.Value, amount, out appendedVertices);
				if (pb_FaceRebuildData2 == null)
				{
					continue;
				}
				list3.Add(pb_FaceRebuildData2);
				foreach (KeyValuePair<int, List<int>> item6 in appendedVertices)
				{
					dictionary.AddOrAppend(item6.Key, new pb_Tuple<pb_FaceRebuildData, List<int>>(pb_FaceRebuildData2, item6.Value));
				}
			}
			pb_FaceRebuildData.Apply(list3, pb, list);
			int num2 = pb.DeleteFaces(dictionary2.Keys).Length;
			pb.SetSharedIndicesUV(new pb_IntArray[0]);
			pb.SetSharedIndices(pb_IntArrayUtility.ExtractSharedIndices(pb.vertices));
			pb_IntArray[] sharedIndices = pb.sharedIndices;
			lookup = sharedIndices.ToDictionary();
			List<HashSet<int>> list4 = new List<HashSet<int>>();
			foreach (KeyValuePair<int, List<pb_Tuple<pb_FaceRebuildData, List<int>>>> item7 in dictionary)
			{
				if (item7.Value.Sum((pb_Tuple<pb_FaceRebuildData, List<int>> x) => x.Item2.Count) < 3)
				{
					continue;
				}
				HashSet<int> hashSet4 = new HashSet<int>();
				foreach (pb_Tuple<pb_FaceRebuildData, List<int>> item8 in item7.Value)
				{
					int num3 = item8.Item1.Offset() - num2;
					for (int num4 = 0; num4 < item8.Item2.Count; num4++)
					{
						hashSet4.Add(lookup[item8.Item2[num4] + num3]);
					}
				}
				list4.Add(hashSet4);
			}
			List<pb_WingedEdge> wingedEdges2 = pb_WingedEdge.GetWingedEdges(pb, list3.Select((pb_FaceRebuildData x) => x.face));
			list = new List<pb_Vertex>(pb_Vertex.GetVertices(pb));
			List<pb_FaceRebuildData> list5 = new List<pb_FaceRebuildData>();
			foreach (HashSet<int> item9 in list4)
			{
				if (item9.Count < 3)
				{
					continue;
				}
				if (item9.Count < 4)
				{
					List<pb_Vertex> vertices = new List<pb_Vertex>(pb_Vertex.GetVertices(pb, item9.Select((int x) => sharedIndices[x][0]).ToList()));
					list5.Add(pb_AppendPolygon.FaceWithVertices(vertices));
					continue;
				}
				List<int> source = pb_WingedEdge.SortCommonIndicesByAdjacency(wingedEdges2, item9);
				List<pb_Vertex> path = new List<pb_Vertex>(pb_Vertex.GetVertices(pb, source.Select((int x) => sharedIndices[x][0]).ToList()));
				list5.AddRange(pb_AppendPolygon.TentCapWithVertices(path));
			}
			pb_FaceRebuildData.Apply(list5, pb, list);
			pb.SetSharedIndices(pb_IntArrayUtility.ExtractSharedIndices(pb.vertices));
			HashSet<pb_Face> hashSet5 = new HashSet<pb_Face>(list5.Select((pb_FaceRebuildData x) => x.face));
			list3.AddRange(list5);
			List<pb_WingedEdge> wingedEdges3 = pb_WingedEdge.GetWingedEdges(pb, list3.Select((pb_FaceRebuildData x) => x.face));
			for (int num5 = 0; num5 < wingedEdges3.Count; num5++)
			{
				if (hashSet5.Count <= 0)
				{
					break;
				}
				pb_WingedEdge pb_WingedEdge3 = wingedEdges3[num5];
				if (!hashSet5.Contains(pb_WingedEdge3.face))
				{
					continue;
				}
				hashSet5.Remove(pb_WingedEdge3.face);
				foreach (pb_WingedEdge item10 in pb_WingedEdge3)
				{
					if (!hashSet5.Contains(item10.opposite.face))
					{
						item10.face.material = item10.opposite.face.material;
						item10.face.uv = new pb_UV(item10.opposite.face.uv);
						pb_ConformNormals.ConformOppositeNormal(item10.opposite);
						break;
					}
				}
			}
			pb.ToMesh();
			return new pb_ActionResult(Status.Success, "Bevel Edges");
		}

		private static List<pb_FaceRebuildData> GetBridgeFaces(IList<pb_Vertex> vertices, pb_WingedEdge left, pb_WingedEdge right, Dictionary<int, List<pb_Tuple<pb_FaceRebuildData, List<int>>>> holes)
		{
			List<pb_FaceRebuildData> list = new List<pb_FaceRebuildData>();
			pb_FaceRebuildData pb_FaceRebuildData2 = new pb_FaceRebuildData();
			pb_EdgeLookup edge = left.edge;
			pb_EdgeLookup edge2 = right.edge;
			pb_FaceRebuildData2.vertices = new List<pb_Vertex>
			{
				vertices[edge.local.x],
				vertices[edge.local.y],
				vertices[(edge.common.x != edge2.common.x) ? edge2.local.y : edge2.local.x],
				vertices[(edge.common.x != edge2.common.x) ? edge2.local.x : edge2.local.y]
			};
			Vector3 lhs = pb_Math.Normal(vertices, left.face.indices);
			Vector3 rhs = pb_Math.Normal(pb_FaceRebuildData2.vertices, BRIDGE_INDICES_NRM);
			int[] array = new int[6] { 2, 1, 0, 2, 3, 1 };
			if (Vector3.Dot(lhs, rhs) < 0f)
			{
				Array.Reverse(array);
			}
			pb_FaceRebuildData2.face = new pb_Face(array, left.face.material, new pb_UV(), -1, -1, -1, false);
			list.Add(pb_FaceRebuildData2);
			holes.AddOrAppend(edge.common.x, new pb_Tuple<pb_FaceRebuildData, List<int>>(pb_FaceRebuildData2, new List<int> { 0, 2 }));
			holes.AddOrAppend(edge.common.y, new pb_Tuple<pb_FaceRebuildData, List<int>>(pb_FaceRebuildData2, new List<int> { 1, 3 }));
			return list;
		}

		private static void SlideEdge(IList<pb_Vertex> vertices, pb_WingedEdge we, float amount)
		{
			we.face.manualUV = true;
			we.face.textureGroup = -1;
			pb_Edge leadingEdge = GetLeadingEdge(we, we.edge.common.x);
			pb_Edge leadingEdge2 = GetLeadingEdge(we, we.edge.common.y);
			if (leadingEdge.IsValid() && leadingEdge2.IsValid())
			{
				pb_Vertex pb_Vertex2 = vertices[leadingEdge.x] - vertices[leadingEdge.y];
				pb_Vertex2.Normalize();
				pb_Vertex pb_Vertex3 = vertices[leadingEdge2.x] - vertices[leadingEdge2.y];
				pb_Vertex3.Normalize();
				vertices[we.edge.local.x].Add(pb_Vertex2 * amount);
				vertices[we.edge.local.y].Add(pb_Vertex3 * amount);
			}
		}

		private static pb_Edge GetLeadingEdge(pb_WingedEdge wing, int common)
		{
			if (wing.previous.edge.common.x == common)
			{
				return new pb_Edge(wing.previous.edge.local.y, wing.previous.edge.local.x);
			}
			if (wing.previous.edge.common.y == common)
			{
				return new pb_Edge(wing.previous.edge.local.x, wing.previous.edge.local.y);
			}
			if (wing.next.edge.common.x == common)
			{
				return new pb_Edge(wing.next.edge.local.y, wing.next.edge.local.x);
			}
			if (wing.next.edge.common.y == common)
			{
				return new pb_Edge(wing.next.edge.local.x, wing.next.edge.local.y);
			}
			return pb_Edge.Empty;
		}
	}
}
