using System.Collections.Generic;
using System.Linq;
using ProBuilder2.Common;
using UnityEngine;

namespace ProBuilder2.MeshOperations
{
	public static class pbVertexOps
	{
		public static bool MergeVertices(this pb_Object pb, int[] indices, out int collapsedIndex, bool collapseToFirst = false)
		{
			pb_Vertex[] vertices = pb_Vertex.GetVertices(pb);
			pb_Vertex vertex = ((!collapseToFirst) ? pb_Vertex.Average(vertices, indices) : vertices[indices[0]]);
			pb_IntArray[] sharedIndices = pb.sharedIndices;
			pb_IntArray[] sharedIndices2 = pb.sharedIndicesUV;
			int num = pb_IntArrayUtility.MergeSharedIndices(ref sharedIndices, indices);
			pb_IntArrayUtility.MergeSharedIndices(ref sharedIndices2, indices);
			pb.SetSharedIndices(sharedIndices);
			pb.SetSharedIndicesUV(sharedIndices2);
			pb.SetSharedVertexValues(num, vertex);
			int[] array = pb.GetSharedIndices()[num].array;
			int[] removed;
			pb.RemoveDegenerateTriangles(out removed);
			int num2 = -1;
			for (int i = 0; i < array.Length; i++)
			{
				if (!removed.Contains(array[i]))
				{
					num2 = array[i];
				}
			}
			int num3 = num2;
			for (int j = 0; j < removed.Length; j++)
			{
				if (num2 > removed[j])
				{
					num3--;
				}
			}
			if (num3 > -1)
			{
				collapsedIndex = num3;
				return true;
			}
			collapsedIndex = -1;
			return false;
		}

		public static bool SplitCommonVertices(this pb_Object pb, int[] indices)
		{
			Dictionary<int, int> dictionary = pb.sharedIndices.ToDictionary();
			pb_IntArray[] sharedIndices = pb.sharedIndices;
			List<int> list = new List<int>();
			List<int> list2 = new List<int>();
			for (int i = 0; i < indices.Length; i++)
			{
				int num = dictionary[indices[i]];
				if (!list.Contains(num))
				{
					list.Add(num);
					list2.AddRange(sharedIndices[num].array);
				}
			}
			pb_IntArrayUtility.RemoveValues(ref sharedIndices, list2.ToArray());
			foreach (int item in list2)
			{
				pb_IntArrayUtility.AddValueAtIndex(ref sharedIndices, -1, item);
			}
			pb.SetSharedIndices(sharedIndices);
			return true;
		}

		public static void SplitVertices(this pb_Object pb, pb_Edge edge)
		{
			pb.SplitVertices(new int[2] { edge.x, edge.y });
		}

		public static void SplitVertices(this pb_Object pb, IEnumerable<int> indices)
		{
			Dictionary<int, int> dictionary = pb.sharedIndices.ToDictionary();
			int num = dictionary.Count();
			foreach (int index in indices)
			{
				num = (dictionary[index] = num + 1);
			}
			pb.SetSharedIndices(dictionary);
		}

		public static bool AppendVerticesToFace(this pb_Object pb, pb_Face face, Vector3[] points, Color[] addColors, out pb_Face newFace)
		{
			if (!face.IsValid())
			{
				newFace = face;
				return false;
			}
			List<pb_Vertex> list = pb_Vertex.GetVertices(pb).ToList();
			List<pb_Face> list2 = new List<pb_Face>(pb.faces);
			Dictionary<int, int> dictionary = pb.sharedIndices.ToDictionary();
			Dictionary<int, int> dictionary2 = ((pb.sharedIndicesUV != null) ? pb.sharedIndicesUV.ToDictionary() : null);
			List<pb_Edge> list3 = pb_WingedEdge.SortEdgesByAdjacency(face);
			List<pb_Vertex> list4 = new List<pb_Vertex>();
			List<int> list5 = new List<int>();
			List<int> list6 = ((dictionary2 == null) ? null : new List<int>());
			for (int i = 0; i < list3.Count; i++)
			{
				list4.Add(list[list3[i].x]);
				list5.Add(dictionary[list3[i].x]);
				if (dictionary2 != null)
				{
					int value;
					if (dictionary2.TryGetValue(list3[i].x, out value))
					{
						list6.Add(value);
					}
					else
					{
						list6.Add(-1);
					}
				}
			}
			for (int j = 0; j < points.Length; j++)
			{
				int num = -1;
				float num2 = float.PositiveInfinity;
				Vector3 vector = points[j];
				int count = list4.Count;
				for (int k = 0; k < count; k++)
				{
					Vector3 position = list4[k].position;
					Vector3 position2 = list4[(k + 1) % count].position;
					float num3 = pb_Math.DistancePointLineSegment(vector, position, position2);
					if (num3 < num2)
					{
						num2 = num3;
						num = k;
					}
				}
				pb_Vertex pb_Vertex2 = list4[num];
				pb_Vertex pb_Vertex3 = list4[(num + 1) % count];
				float sqrMagnitude = (vector - pb_Vertex2.position).sqrMagnitude;
				float sqrMagnitude2 = (vector - pb_Vertex3.position).sqrMagnitude;
				pb_Vertex item = pb_Vertex.Mix(pb_Vertex2, pb_Vertex3, sqrMagnitude / (sqrMagnitude + sqrMagnitude2));
				list4.Insert((num + 1) % count, item);
				list5.Insert((num + 1) % count, -1);
				if (list6 != null)
				{
					list6.Insert((num + 1) % count, -1);
				}
			}
			List<int> triangles;
			try
			{
				pb_Triangulation.TriangulateVertices(list4, out triangles, false);
			}
			catch
			{
				Debug.Log("Failed triangulating face after appending vertices.");
				newFace = null;
				return false;
			}
			pb_FaceRebuildData pb_FaceRebuildData2 = new pb_FaceRebuildData();
			pb_FaceRebuildData2.face = new pb_Face(triangles.ToArray(), face.material, new pb_UV(face.uv), face.smoothingGroup, face.textureGroup, -1, face.manualUV);
			pb_FaceRebuildData2.vertices = list4;
			pb_FaceRebuildData2.sharedIndices = list5;
			pb_FaceRebuildData2.sharedIndicesUV = list6;
			List<pb_FaceRebuildData> list7 = new List<pb_FaceRebuildData>();
			list7.Add(pb_FaceRebuildData2);
			pb_FaceRebuildData.Apply(list7, list, list2, dictionary, dictionary2);
			newFace = pb_FaceRebuildData2.face;
			pb.SetVertices(list);
			pb.SetFaces(list2.ToArray());
			pb.SetSharedIndices(dictionary);
			pb.SetSharedIndicesUV(dictionary2);
			Vector3 lhs = pb_Math.Normal(pb, face);
			Vector3 rhs = pb_Math.Normal(pb, newFace);
			if (Vector3.Dot(lhs, rhs) < 0f)
			{
				newFace.ReverseIndices();
			}
			pb.DeleteFace(face);
			return true;
		}

		public static pb_ActionResult AppendVerticesToEdge(this pb_Object pb, pb_Edge edge, int count, out List<pb_Edge> newEdges)
		{
			return pb.AppendVerticesToEdge(new pb_Edge[1] { edge }, count, out newEdges);
		}

		public static pb_ActionResult AppendVerticesToEdge(this pb_Object pb, IList<pb_Edge> edges, int count, out List<pb_Edge> newEdges)
		{
			newEdges = new List<pb_Edge>();
			if (count < 1 || count > 512)
			{
				return new pb_ActionResult(Status.Failure, "New edge vertex count is less than 1 or greater than 512.");
			}
			List<pb_Vertex> list = new List<pb_Vertex>(pb_Vertex.GetVertices(pb));
			Dictionary<int, int> dictionary = pb.sharedIndices.ToDictionary();
			Dictionary<int, int> dictionary2 = pb.sharedIndicesUV.ToDictionary();
			List<int> list2 = new List<int>();
			pb_Edge[] universalEdges = pb_Edge.GetUniversalEdges(edges.ToArray(), dictionary);
			List<pb_Edge> list3 = universalEdges.Distinct().ToList();
			Dictionary<pb_Face, pb_FaceRebuildData> dictionary3 = new Dictionary<pb_Face, pb_FaceRebuildData>();
			int num = dictionary.Count();
			int num2 = num;
			foreach (pb_Edge item2 in list3)
			{
				pb_Edge localEdgeFast = pb_Edge.GetLocalEdgeFast(item2, pb.sharedIndices);
				List<pb_Vertex> list4 = new List<pb_Vertex>(count);
				for (int i = 0; i < count; i++)
				{
					list4.Add(pb_Vertex.Mix(list[localEdgeFast.x], list[localEdgeFast.y], (float)(i + 1) / ((float)count + 1f)));
				}
				List<pb_Tuple<pb_Face, pb_Edge>> neighborFaces = pbMeshUtils.GetNeighborFaces(pb, localEdgeFast);
				foreach (pb_Tuple<pb_Face, pb_Edge> item3 in neighborFaces)
				{
					pb_Face item = item3.Item1;
					pb_FaceRebuildData value;
					if (!dictionary3.TryGetValue(item, out value))
					{
						value = new pb_FaceRebuildData();
						value.face = new pb_Face(null, item.material, new pb_UV(item.uv), item.smoothingGroup, item.textureGroup, -1, item.manualUV);
						value.vertices = new List<pb_Vertex>(list.ValuesWithIndices(item.distinctIndices));
						value.sharedIndices = new List<int>();
						value.sharedIndicesUV = new List<int>();
						int[] distinctIndices = item.distinctIndices;
						foreach (int key in distinctIndices)
						{
							int value2;
							if (dictionary.TryGetValue(key, out value2))
							{
								value.sharedIndices.Add(value2);
							}
							if (dictionary2.TryGetValue(key, out value2))
							{
								value.sharedIndicesUV.Add(value2);
							}
						}
						list2.AddRange(item.distinctIndices);
						dictionary3.Add(item, value);
					}
					value.vertices.AddRange(list4);
					for (int k = 0; k < count; k++)
					{
						value.sharedIndices.Add(num2 + k);
						value.sharedIndicesUV.Add(-1);
					}
				}
				num2 += count;
			}
			List<pb_Face> list5 = dictionary3.Keys.ToList();
			List<pb_FaceRebuildData> list6 = dictionary3.Values.ToList();
			List<pb_EdgeLookup> list7 = new List<pb_EdgeLookup>();
			for (int l = 0; l < list5.Count; l++)
			{
				pb_Face pb_Face2 = list5[l];
				pb_FaceRebuildData pb_FaceRebuildData2 = list6[l];
				Vector3 planeNormal = pb_Math.Normal(pb, pb_Face2);
				Vector2[] points = pb_Projection.PlanarProject(pb_FaceRebuildData2.vertices.Select((pb_Vertex x) => x.position).ToArray(), planeNormal);
				int count2 = list.Count;
				List<int> indices;
				if (!pb_Triangulation.SortAndTriangulate(points, out indices))
				{
					continue;
				}
				pb_FaceRebuildData2.face.SetIndices(indices.ToArray());
				pb_FaceRebuildData2.face.ShiftIndices(count2);
				pb_Face2.CopyFrom(pb_FaceRebuildData2.face);
				for (int num3 = 0; num3 < pb_FaceRebuildData2.vertices.Count; num3++)
				{
					dictionary.Add(count2 + num3, pb_FaceRebuildData2.sharedIndices[num3]);
				}
				if (pb_FaceRebuildData2.sharedIndicesUV.Count == pb_FaceRebuildData2.vertices.Count)
				{
					for (int num4 = 0; num4 < pb_FaceRebuildData2.vertices.Count; num4++)
					{
						dictionary2.Add(count2 + num4, pb_FaceRebuildData2.sharedIndicesUV[num4]);
					}
				}
				list.AddRange(pb_FaceRebuildData2.vertices);
				pb_Edge[] edges2 = pb_Face2.edges;
				for (int num5 = 0; num5 < edges2.Length; num5++)
				{
					pb_Edge local = edges2[num5];
					pb_EdgeLookup pb_EdgeLookup2 = new pb_EdgeLookup(new pb_Edge(dictionary[local.x], dictionary[local.y]), local);
					if (pb_EdgeLookup2.common.x >= num || pb_EdgeLookup2.common.y >= num)
					{
						list7.Add(pb_EdgeLookup2);
					}
				}
			}
			list2 = list2.Distinct().ToList();
			int delCount = list2.Count;
			newEdges = (from x in list7.Distinct()
				select x.local - delCount).ToList();
			pb.SetVertices(list);
			pb.SetSharedIndices(dictionary.ToSharedIndices());
			pb.SetSharedIndicesUV(dictionary2.ToSharedIndices());
			pb.DeleteVerticesWithIndices(list2);
			return new pb_ActionResult(Status.Success, "Subdivide Edges");
		}

		public static pb_FaceRebuildData ExplodeVertex(IList<pb_Vertex> vertices, IList<pb_Tuple<pb_WingedEdge, int>> edgeAndCommonIndex, float distance, out Dictionary<int, List<int>> appendedVertices)
		{
			pb_Face face = edgeAndCommonIndex.FirstOrDefault().Item1.face;
			List<pb_Edge> list = pb_WingedEdge.SortEdgesByAdjacency(face);
			appendedVertices = new Dictionary<int, List<int>>();
			Vector3 lhs = pb_Math.Normal(vertices, face.indices);
			Dictionary<int, int> dictionary = new Dictionary<int, int>();
			foreach (pb_Tuple<pb_WingedEdge, int> item3 in edgeAndCommonIndex)
			{
				if (item3.Item2 == item3.Item1.edge.common.x)
				{
					dictionary.Add(item3.Item1.edge.local.x, item3.Item2);
				}
				else
				{
					dictionary.Add(item3.Item1.edge.local.y, item3.Item2);
				}
			}
			int count = list.Count;
			List<pb_Vertex> list2 = new List<pb_Vertex>();
			for (int i = 0; i < count; i++)
			{
				int y = list[i].y;
				if (dictionary.ContainsKey(y))
				{
					pb_Vertex pb_Vertex2 = vertices[list[i].x];
					pb_Vertex pb_Vertex3 = vertices[list[i].y];
					pb_Vertex pb_Vertex4 = vertices[list[(i + 1) % count].y];
					pb_Vertex pb_Vertex5 = pb_Vertex2 - pb_Vertex3;
					pb_Vertex pb_Vertex6 = pb_Vertex4 - pb_Vertex3;
					pb_Vertex5.Normalize();
					pb_Vertex6.Normalize();
					pb_Vertex item = vertices[y] + pb_Vertex5 * distance;
					pb_Vertex item2 = vertices[y] + pb_Vertex6 * distance;
					appendedVertices.AddOrAppend(dictionary[y], list2.Count);
					list2.Add(item);
					appendedVertices.AddOrAppend(dictionary[y], list2.Count);
					list2.Add(item2);
				}
				else
				{
					list2.Add(vertices[y]);
				}
			}
			List<int> triangles;
			if (pb_Triangulation.TriangulateVertices(list2, out triangles, false))
			{
				pb_FaceRebuildData pb_FaceRebuildData2 = new pb_FaceRebuildData();
				pb_FaceRebuildData2.vertices = list2;
				pb_FaceRebuildData2.face = new pb_Face(face);
				Vector3 rhs = pb_Math.Normal(list2, triangles);
				if (Vector3.Dot(lhs, rhs) < 0f)
				{
					triangles.Reverse();
				}
				pb_FaceRebuildData2.face.SetIndices(triangles.ToArray());
				return pb_FaceRebuildData2;
			}
			return null;
		}

		private static pb_Edge AlignEdgeWithDirection(pb_EdgeLookup edge, int commonIndex)
		{
			if (edge.common.x == commonIndex)
			{
				return new pb_Edge(edge.local.x, edge.local.y);
			}
			return new pb_Edge(edge.local.y, edge.local.x);
		}

		public static void Quantize(pb_Object pb, IList<int> indices, Vector3 snap)
		{
			Vector3[] vertices = pb.vertices;
			for (int i = 0; i < indices.Count; i++)
			{
				vertices[indices[i]] = pb.transform.InverseTransformPoint(pb_Snap.SnapValue(pb.transform.TransformPoint(vertices[indices[i]]), snap));
			}
		}
	}
}
