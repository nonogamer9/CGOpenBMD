using System.Collections.Generic;
using System.Linq;
using ProBuilder2.Common;
using UnityEngine;

namespace ProBuilder2.MeshOperations
{
	public static class pb_ConnectEdges
	{
		public static pb_ActionResult Connect(this pb_Object pb, IEnumerable<pb_Face> faces, out pb_Face[] subdividedFaces)
		{
			IEnumerable<pb_Edge> edges = faces.SelectMany((pb_Face x) => x.edges);
			HashSet<pb_Face> faceMask = new HashSet<pb_Face>(faces);
			pb_Edge[] connections;
			return pb.Connect(edges, out subdividedFaces, out connections, true, false, faceMask);
		}

		public static pb_ActionResult Connect(this pb_Object pb, IEnumerable<pb_Edge> edges, out pb_Face[] faces)
		{
			pb_Edge[] connections;
			return pb.Connect(edges, out faces, out connections, true);
		}

		public static pb_ActionResult Connect(this pb_Object pb, IEnumerable<pb_Edge> edges, out pb_Edge[] connections)
		{
			pb_Face[] addedFaces;
			return pb.Connect(edges, out addedFaces, out connections, false, true);
		}

		private static pb_ActionResult Connect(this pb_Object pb, IEnumerable<pb_Edge> edges, out pb_Face[] addedFaces, out pb_Edge[] connections, bool returnFaces = false, bool returnEdges = false, HashSet<pb_Face> faceMask = null)
		{
			Dictionary<int, int> lookup = pb.sharedIndices.ToDictionary();
			Dictionary<int, int> lookupUV = ((pb.sharedIndicesUV == null) ? null : pb.sharedIndicesUV.ToDictionary());
			HashSet<pb_EdgeLookup> hashSet = new HashSet<pb_EdgeLookup>(pb_EdgeLookup.GetEdgeLookup(edges, lookup));
			List<pb_WingedEdge> wingedEdges = pb_WingedEdge.GetWingedEdges(pb);
			Dictionary<pb_Face, List<pb_WingedEdge>> dictionary = new Dictionary<pb_Face, List<pb_WingedEdge>>();
			foreach (pb_WingedEdge item in wingedEdges)
			{
				if (hashSet.Contains(item.edge))
				{
					List<pb_WingedEdge> value;
					if (dictionary.TryGetValue(item.face, out value))
					{
						value.Add(item);
						continue;
					}
					dictionary.Add(item.face, new List<pb_WingedEdge> { item });
				}
			}
			Dictionary<pb_Face, List<pb_WingedEdge>> dictionary2 = new Dictionary<pb_Face, List<pb_WingedEdge>>();
			foreach (KeyValuePair<pb_Face, List<pb_WingedEdge>> item2 in dictionary)
			{
				if (item2.Value.Count <= 1)
				{
					pb_WingedEdge opposite = item2.Value[0].opposite;
					List<pb_WingedEdge> value2;
					if (opposite == null || !dictionary.TryGetValue(opposite.face, out value2) || value2.Count <= 1)
					{
						continue;
					}
				}
				dictionary2.Add(item2.Key, item2.Value);
			}
			List<pb_Vertex> vertices = new List<pb_Vertex>(pb_Vertex.GetVertices(pb));
			List<ConnectFaceRebuildData> list = new List<ConnectFaceRebuildData>();
			List<pb_Face> list2 = new List<pb_Face>();
			HashSet<int> hashSet2 = new HashSet<int>(pb.faces.Select((pb_Face x) => x.textureGroup));
			int num = 1;
			foreach (KeyValuePair<pb_Face, List<pb_WingedEdge>> item3 in dictionary2)
			{
				pb_Face key = item3.Key;
				List<pb_WingedEdge> value3 = item3.Value;
				int count = value3.Count;
				Vector3 lhs = pb_Math.Normal(vertices, key.indices);
				if (count == 1 || (faceMask != null && !faceMask.Contains(key)))
				{
					ConnectFaceRebuildData connectFaceRebuildData = InsertVertices(key, value3, vertices);
					Vector3 rhs = pb_Math.Normal(connectFaceRebuildData.faceRebuildData.vertices, connectFaceRebuildData.faceRebuildData.face.indices);
					if (Vector3.Dot(lhs, rhs) < 0f)
					{
						connectFaceRebuildData.faceRebuildData.face.ReverseIndices();
					}
					list.Add(connectFaceRebuildData);
				}
				else
				{
					if (count <= 1)
					{
						continue;
					}
					List<ConnectFaceRebuildData> list3 = ((count != 2) ? ConnectEdgesInFace(key, value3, vertices) : ConnectEdgesInFace(key, value3[0], value3[1], vertices));
					if (key.textureGroup < 0)
					{
						for (; hashSet2.Contains(num); num++)
						{
						}
						hashSet2.Add(num);
					}
					foreach (ConnectFaceRebuildData item4 in list3)
					{
						list2.Add(item4.faceRebuildData.face);
						Vector3 rhs2 = pb_Math.Normal(item4.faceRebuildData.vertices, item4.faceRebuildData.face.indices);
						if (Vector3.Dot(lhs, rhs2) < 0f)
						{
							item4.faceRebuildData.face.ReverseIndices();
						}
						item4.faceRebuildData.face.textureGroup = ((key.textureGroup >= 0) ? key.textureGroup : num);
						item4.faceRebuildData.face.uv = new pb_UV(key.uv);
						item4.faceRebuildData.face.smoothingGroup = key.smoothingGroup;
						item4.faceRebuildData.face.manualUV = key.manualUV;
						item4.faceRebuildData.face.material = key.material;
					}
					list.AddRange(list3);
				}
			}
			pb_FaceRebuildData.Apply(list.Select((ConnectFaceRebuildData x) => x.faceRebuildData), pb, vertices, null, lookup, lookupUV);
			pb.SetSharedIndicesUV(new pb_IntArray[0]);
			int num2 = pb.DeleteFaces(dictionary2.Keys).Length;
			pb.SetSharedIndices(pb_IntArrayUtility.ExtractSharedIndices(pb.vertices));
			pb.ToMesh();
			if (returnEdges)
			{
				HashSet<int> appendedIndices = new HashSet<int>();
				for (int num3 = 0; num3 < list.Count; num3++)
				{
					for (int num4 = 0; num4 < list[num3].newVertexIndices.Count; num4++)
					{
						appendedIndices.Add(list[num3].newVertexIndices[num4] + list[num3].faceRebuildData.Offset() - num2);
					}
				}
				Dictionary<int, int> lookup2 = pb.sharedIndices.ToDictionary();
				IEnumerable<pb_Edge> edges2 = from x in list.SelectMany((ConnectFaceRebuildData x) => x.faceRebuildData.face.edges)
					where appendedIndices.Contains(x.x) && appendedIndices.Contains(x.y)
					select x;
				IEnumerable<pb_EdgeLookup> edgeLookup = pb_EdgeLookup.GetEdgeLookup(edges2, lookup2);
				connections = (from x in edgeLookup.Distinct()
					select x.local).ToArray();
			}
			else
			{
				connections = null;
			}
			if (returnFaces)
			{
				addedFaces = list2.ToArray();
			}
			else
			{
				addedFaces = null;
			}
			return new pb_ActionResult(Status.Success, string.Format("Connected {0} Edges", list.Count));
		}

		private static List<ConnectFaceRebuildData> ConnectEdgesInFace(pb_Face face, pb_WingedEdge a, pb_WingedEdge b, List<pb_Vertex> vertices)
		{
			List<pb_Edge> list = pb_WingedEdge.SortEdgesByAdjacency(face);
			List<pb_Vertex>[] array = new List<pb_Vertex>[2]
			{
				new List<pb_Vertex>(),
				new List<pb_Vertex>()
			};
			List<int>[] array2 = new List<int>[2]
			{
				new List<int>(),
				new List<int>()
			};
			int num = 0;
			for (int i = 0; i < list.Count; i++)
			{
				array[num % 2].Add(vertices[list[i].x]);
				if (list[i].Equals(a.edge.local) || list[i].Equals(b.edge.local))
				{
					pb_Vertex item = pb_Vertex.Mix(vertices[list[i].x], vertices[list[i].y], 0.5f);
					array2[num % 2].Add(array[num % 2].Count);
					array[num % 2].Add(item);
					num++;
					array2[num % 2].Add(array[num % 2].Count);
					array[num % 2].Add(item);
				}
			}
			List<ConnectFaceRebuildData> list2 = new List<ConnectFaceRebuildData>();
			for (int j = 0; j < array.Length; j++)
			{
				pb_FaceRebuildData faceRebuildData = pb_AppendPolygon.FaceWithVertices(array[j], false);
				list2.Add(new ConnectFaceRebuildData(faceRebuildData, array2[j]));
			}
			return list2;
		}

		private static List<ConnectFaceRebuildData> ConnectEdgesInFace(pb_Face face, List<pb_WingedEdge> edges, List<pb_Vertex> vertices)
		{
			List<pb_Edge> list = pb_WingedEdge.SortEdgesByAdjacency(face);
			int count = edges.Count;
			pb_Vertex item = pb_Vertex.Average(vertices, face.distinctIndices);
			List<List<pb_Vertex>> list2 = pbUtil.Fill((int x) => new List<pb_Vertex>(), count);
			List<List<int>> list3 = pbUtil.Fill((int x) => new List<int>(), count);
			HashSet<pb_Edge> hashSet = new HashSet<pb_Edge>(edges.Select((pb_WingedEdge x) => x.edge.local));
			int num = 0;
			for (int num2 = 0; num2 < list.Count; num2++)
			{
				list2[num % count].Add(vertices[list[num2].x]);
				if (hashSet.Contains(list[num2]))
				{
					pb_Vertex item2 = pb_Vertex.Mix(vertices[list[num2].x], vertices[list[num2].y], 0.5f);
					list3[num].Add(list2[num].Count);
					list2[num].Add(item2);
					list3[num].Add(list2[num].Count);
					list2[num].Add(item);
					num = (num + 1) % count;
					list2[num].Add(item2);
				}
			}
			List<ConnectFaceRebuildData> list4 = new List<ConnectFaceRebuildData>();
			for (int num3 = 0; num3 < list2.Count; num3++)
			{
				pb_FaceRebuildData faceRebuildData = pb_AppendPolygon.FaceWithVertices(list2[num3], false);
				list4.Add(new ConnectFaceRebuildData(faceRebuildData, list3[num3]));
			}
			return list4;
		}

		private static ConnectFaceRebuildData InsertVertices(pb_Face face, List<pb_WingedEdge> edges, List<pb_Vertex> vertices)
		{
			List<pb_Edge> list = pb_WingedEdge.SortEdgesByAdjacency(face);
			List<pb_Vertex> list2 = new List<pb_Vertex>();
			List<int> list3 = new List<int>();
			HashSet<pb_Edge> hashSet = new HashSet<pb_Edge>(edges.Select((pb_WingedEdge x) => x.edge.local));
			for (int num = 0; num < list.Count; num++)
			{
				list2.Add(vertices[list[num].x]);
				if (hashSet.Contains(list[num]))
				{
					list3.Add(list2.Count);
					list2.Add(pb_Vertex.Mix(vertices[list[num].x], vertices[list[num].y], 0.5f));
				}
			}
			pb_FaceRebuildData pb_FaceRebuildData2 = pb_AppendPolygon.FaceWithVertices(list2, false);
			pb_FaceRebuildData2.face.textureGroup = face.textureGroup;
			pb_FaceRebuildData2.face.uv = new pb_UV(face.uv);
			pb_FaceRebuildData2.face.smoothingGroup = face.smoothingGroup;
			pb_FaceRebuildData2.face.manualUV = face.manualUV;
			pb_FaceRebuildData2.face.material = face.material;
			return new ConnectFaceRebuildData(pb_FaceRebuildData2, list3);
		}
	}
}
