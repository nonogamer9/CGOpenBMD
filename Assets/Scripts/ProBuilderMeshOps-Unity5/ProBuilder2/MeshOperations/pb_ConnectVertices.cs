using System.Collections.Generic;
using System.Linq;
using ProBuilder2.Common;
using UnityEngine;

namespace ProBuilder2.MeshOperations
{
	public static class pb_ConnectVertices
	{
		public static pb_ActionResult Connect(this pb_Object pb, IList<int> indices, out int[] newVertices)
		{
			int num = pb.sharedIndices.Length;
			Dictionary<int, int> lookup = pb.sharedIndices.ToDictionary();
			HashSet<int> hashSet = new HashSet<int>(indices.Select((int x) => lookup[x]));
			HashSet<int> hashSet2 = new HashSet<int>();
			foreach (int item in hashSet)
			{
				hashSet2.UnionWith(pb.sharedIndices[item].array);
			}
			Dictionary<pb_Face, List<int>> dictionary = new Dictionary<pb_Face, List<int>>();
			List<pb_Vertex> vertices = new List<pb_Vertex>(pb_Vertex.GetVertices(pb));
			pb_Face[] faces = pb.faces;
			foreach (pb_Face pb_Face2 in faces)
			{
				int[] distinctIndices = pb_Face2.distinctIndices;
				for (int num3 = 0; num3 < distinctIndices.Length; num3++)
				{
					if (hashSet2.Contains(distinctIndices[num3]))
					{
						dictionary.AddOrAppend(pb_Face2, distinctIndices[num3]);
					}
				}
			}
			List<ConnectFaceRebuildData> list = new List<ConnectFaceRebuildData>();
			List<pb_Face> list2 = new List<pb_Face>();
			HashSet<int> hashSet3 = new HashSet<int>(pb.faces.Select((pb_Face x) => x.textureGroup));
			int num4 = 1;
			foreach (KeyValuePair<pb_Face, List<int>> item2 in dictionary)
			{
				pb_Face key = item2.Key;
				List<ConnectFaceRebuildData> list3 = ((item2.Value.Count != 2) ? ConnectIndicesInFace(key, item2.Value, vertices, lookup, num++) : ConnectIndicesInFace(key, item2.Value[0], item2.Value[1], vertices, lookup));
				if (list3 == null)
				{
					continue;
				}
				if (key.textureGroup < 0)
				{
					for (; hashSet3.Contains(num4); num4++)
					{
					}
					hashSet3.Add(num4);
				}
				foreach (ConnectFaceRebuildData item3 in list3)
				{
					item3.faceRebuildData.face.textureGroup = ((key.textureGroup >= 0) ? key.textureGroup : num4);
					item3.faceRebuildData.face.uv = new pb_UV(key.uv);
					item3.faceRebuildData.face.smoothingGroup = key.smoothingGroup;
					item3.faceRebuildData.face.manualUV = key.manualUV;
					item3.faceRebuildData.face.material = key.material;
				}
				list2.Add(key);
				list.AddRange(list3);
			}
			pb_FaceRebuildData.Apply(list.Select((ConnectFaceRebuildData x) => x.faceRebuildData), pb, vertices, null, lookup);
			pb.SetSharedIndices(lookup);
			pb.SetSharedIndicesUV(new pb_IntArray[0]);
			int num5 = pb.DeleteFaces(list2).Length;
			lookup = pb.sharedIndices.ToDictionary();
			HashSet<int> hashSet4 = new HashSet<int>();
			for (int num6 = 0; num6 < list.Count; num6++)
			{
				for (int num7 = 0; num7 < list[num6].newVertexIndices.Count; num7++)
				{
					hashSet4.Add(lookup[list[num6].newVertexIndices[num7] + (list[num6].faceRebuildData.Offset() - num5)]);
				}
			}
			newVertices = hashSet4.Select((int x) => pb.sharedIndices[x][0]).ToArray();
			pb.ToMesh();
			return new pb_ActionResult(Status.Success, string.Format("Connected {0} Vertices", hashSet.Count));
		}

		private static List<ConnectFaceRebuildData> ConnectIndicesInFace(pb_Face face, int a, int b, List<pb_Vertex> vertices, Dictionary<int, int> lookup)
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
			List<int>[] array3 = new List<int>[2]
			{
				new List<int>(),
				new List<int>()
			};
			int num = 0;
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].Contains(a) && list[i].Contains(b))
				{
					return null;
				}
				int x = list[i].x;
				array[num].Add(vertices[x]);
				array2[num].Add(lookup[x]);
				if (x == a || x == b)
				{
					num = (num + 1) % 2;
					array3[num].Add(array[num].Count);
					array[num].Add(vertices[x]);
					array2[num].Add(lookup[x]);
				}
			}
			List<ConnectFaceRebuildData> list2 = new List<ConnectFaceRebuildData>();
			Vector3 lhs = pb_Math.Normal(vertices, face.indices);
			for (int j = 0; j < array.Length; j++)
			{
				pb_FaceRebuildData pb_FaceRebuildData2 = pb_AppendPolygon.FaceWithVertices(array[j], false);
				pb_FaceRebuildData2.sharedIndices = array2[j];
				Vector3 rhs = pb_Math.Normal(array[j], pb_FaceRebuildData2.face.indices);
				if (Vector3.Dot(lhs, rhs) < 0f)
				{
					pb_FaceRebuildData2.face.ReverseIndices();
				}
				list2.Add(new ConnectFaceRebuildData(pb_FaceRebuildData2, array3[j]));
			}
			return list2;
		}

		private static List<ConnectFaceRebuildData> ConnectIndicesInFace(pb_Face face, List<int> indices, List<pb_Vertex> vertices, Dictionary<int, int> lookup, int sharedIndexOffset)
		{
			if (indices.Count < 3)
			{
				return null;
			}
			List<pb_Edge> list = pb_WingedEdge.SortEdgesByAdjacency(face);
			int count = indices.Count;
			List<List<pb_Vertex>> list2 = pbUtil.Fill((int num4) => new List<pb_Vertex>(), count);
			List<List<int>> list3 = pbUtil.Fill((int num4) => new List<int>(), count);
			List<List<int>> list4 = pbUtil.Fill((int num4) => new List<int>(), count);
			pb_Vertex item = pb_Vertex.Average(vertices, indices);
			Vector3 lhs = pb_Math.Normal(vertices, face.indices);
			int num = 0;
			for (int num2 = 0; num2 < list.Count; num2++)
			{
				int x = list[num2].x;
				list2[num].Add(vertices[x]);
				list3[num].Add(lookup[x]);
				if (indices.Contains(x))
				{
					list4[num].Add(list2[num].Count);
					list2[num].Add(item);
					list3[num].Add(sharedIndexOffset);
					num = (num + 1) % count;
					list4[num].Add(list2[num].Count);
					list2[num].Add(vertices[x]);
					list3[num].Add(lookup[x]);
				}
			}
			List<ConnectFaceRebuildData> list5 = new List<ConnectFaceRebuildData>();
			for (int num3 = 0; num3 < list2.Count; num3++)
			{
				if (list2[num3].Count >= 3)
				{
					pb_FaceRebuildData pb_FaceRebuildData2 = pb_AppendPolygon.FaceWithVertices(list2[num3], false);
					pb_FaceRebuildData2.sharedIndices = list3[num3];
					Vector3 rhs = pb_Math.Normal(list2[num3], pb_FaceRebuildData2.face.indices);
					if (Vector3.Dot(lhs, rhs) < 0f)
					{
						pb_FaceRebuildData2.face.ReverseIndices();
					}
					list5.Add(new ConnectFaceRebuildData(pb_FaceRebuildData2, list4[num3]));
				}
			}
			return list5;
		}
	}
}
