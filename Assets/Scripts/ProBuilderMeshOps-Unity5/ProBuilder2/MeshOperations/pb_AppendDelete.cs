using System;
using System.Collections.Generic;
using System.Linq;
using ProBuilder2.Common;
using UnityEngine;

namespace ProBuilder2.MeshOperations
{
	public static class pb_AppendDelete
	{
		public static pb_Face AppendFace(this pb_Object pb, Vector3[] v, Color[] c, Vector2[] u, pb_Face face)
		{
			int[] array = new int[v.Length];
			for (int i = 0; i < v.Length; i++)
			{
				array[i] = -1;
			}
			return pb.AppendFace(v, c, u, face, array);
		}

		public static pb_Face AppendFace(this pb_Object pb, Vector3[] v, Color[] c, Vector2[] u, pb_Face face, int[] sharedIndex)
		{
			int vertexCount = pb.vertexCount;
			Vector3[] array = new Vector3[vertexCount + v.Length];
			Color[] array2 = new Color[vertexCount + c.Length];
			Vector2[] array3 = new Vector2[pb.uv.Length + u.Length];
			List<pb_Face> list = new List<pb_Face>(pb.faces);
			pb_IntArray[] sharedIndices = pb.sharedIndices;
			Array.Copy(pb.vertices, 0, array, 0, vertexCount);
			Array.Copy(v, 0, array, vertexCount, v.Length);
			Array.Copy(pb.colors, 0, array2, 0, vertexCount);
			Array.Copy(c, 0, array2, vertexCount, c.Length);
			Array.Copy(pb.uv, 0, array3, 0, pb.uv.Length);
			Array.Copy(u, 0, array3, pb.uv.Length, u.Length);
			face.ShiftIndicesToZero();
			face.ShiftIndices(vertexCount);
			face.RebuildCaches();
			list.Add(face);
			for (int i = 0; i < sharedIndex.Length; i++)
			{
				pb_IntArrayUtility.AddValueAtIndex(ref sharedIndices, sharedIndex[i], i + vertexCount);
			}
			pb.SetVertices(array);
			pb.SetColors(array2);
			pb.SetUV(array3);
			pb.SetSharedIndices(sharedIndices);
			pb.SetFaces(list.ToArray());
			return face;
		}

		public static pb_Face[] AppendFaces(this pb_Object pb, Vector3[][] new_Vertices, Color[][] new_Colors, Vector2[][] new_uvs, pb_Face[] new_Faces, int[][] new_SharedIndices)
		{
			List<Vector3> list = new List<Vector3>(pb.vertices);
			List<Color> list2 = new List<Color>(pb.colors);
			List<Vector2> list3 = new List<Vector2>(pb.uv);
			List<pb_Face> list4 = new List<pb_Face>(pb.faces);
			pb_IntArray[] sharedIndices = pb.sharedIndices;
			int num = pb.vertexCount;
			for (int i = 0; i < new_Faces.Length; i++)
			{
				list.AddRange(new_Vertices[i]);
				list2.AddRange(new_Colors[i]);
				list3.AddRange(new_uvs[i]);
				new_Faces[i].ShiftIndicesToZero();
				new_Faces[i].ShiftIndices(num);
				new_Faces[i].RebuildCaches();
				list4.Add(new_Faces[i]);
				if (new_SharedIndices != null && new_Vertices[i].Length != new_SharedIndices[i].Length)
				{
					Debug.LogError("Append Face failed because sharedIndex array does not match new vertex array.");
					return null;
				}
				if (new_SharedIndices != null)
				{
					for (int j = 0; j < new_SharedIndices[i].Length; j++)
					{
						pb_IntArrayUtility.AddValueAtIndex(ref sharedIndices, new_SharedIndices[i][j], j + num);
					}
				}
				else
				{
					for (int k = 0; k < new_Vertices[i].Length; k++)
					{
						pb_IntArrayUtility.AddValueAtIndex(ref sharedIndices, -1, k + num);
					}
				}
				num = list.Count;
			}
			pb.SetSharedIndices(sharedIndices);
			pb.SetVertices(list.ToArray());
			pb.SetColors(list2.ToArray());
			pb.SetUV(list3.ToArray());
			pb.SetFaces(list4.ToArray());
			return new_Faces;
		}

		public static void DuplicateAndFlip(this pb_Object pb, pb_Face[] faces)
		{
			List<pb_FaceRebuildData> list = new List<pb_FaceRebuildData>();
			List<pb_Vertex> list2 = new List<pb_Vertex>(pb_Vertex.GetVertices(pb));
			Dictionary<int, int> dictionary = pb.sharedIndices.ToDictionary();
			foreach (pb_Face pb_Face2 in faces)
			{
				pb_FaceRebuildData pb_FaceRebuildData2 = new pb_FaceRebuildData();
				pb_FaceRebuildData2.vertices = new List<pb_Vertex>();
				pb_FaceRebuildData2.face = new pb_Face(pb_Face2);
				pb_FaceRebuildData2.sharedIndices = new List<int>();
				Dictionary<int, int> dictionary2 = new Dictionary<int, int>();
				int num = pb_FaceRebuildData2.face.indices.Length;
				for (int j = 0; j < num; j++)
				{
					if (!dictionary2.ContainsKey(pb_Face2.indices[j]))
					{
						dictionary2.Add(pb_Face2.indices[j], dictionary2.Count);
						pb_FaceRebuildData2.vertices.Add(list2[pb_Face2.indices[j]]);
						pb_FaceRebuildData2.sharedIndices.Add(dictionary[pb_Face2.indices[j]]);
					}
				}
				for (int k = 0; k < num; k++)
				{
					pb_FaceRebuildData2.face.indices[k] = dictionary2[pb_FaceRebuildData2.face.indices[k]];
				}
				pb_FaceRebuildData2.face.ReverseIndices();
				list.Add(pb_FaceRebuildData2);
			}
			pb_FaceRebuildData.Apply(list, pb, list2, null, dictionary);
		}

		public static int[] DeleteFace(this pb_Object pb, pb_Face face)
		{
			return pb.DeleteFaces(new pb_Face[1] { face });
		}

		public static int[] DeleteFaces(this pb_Object pb, IEnumerable<pb_Face> faces)
		{
			return pb.DeleteFaces(faces.Select((pb_Face x) => Array.IndexOf(pb.faces, x)).ToList());
		}

		public static int[] DeleteFaces(this pb_Object pb, IList<int> faceIndices)
		{
			pb_Face[] array = new pb_Face[faceIndices.Count];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = pb.faces[faceIndices[i]];
			}
			List<int> list = array.SelectMany((pb_Face x) => x.distinctIndices).Distinct().ToList();
			list.Sort();
			int num = pb.vertices.Length;
			Vector3[] vertices = pb.vertices.SortedRemoveAt(list);
			Color[] colors = pb.colors.SortedRemoveAt(list);
			Vector2[] uV = pb.uv.SortedRemoveAt(list);
			pb_Face[] array2 = pb.faces.RemoveAt(faceIndices);
			Dictionary<int, int> dictionary = new Dictionary<int, int>();
			for (int num2 = 0; num2 < num; num2++)
			{
				dictionary.Add(num2, pbUtil.NearestIndexPriorToValue(list, num2) + 1);
			}
			for (int num3 = 0; num3 < array2.Length; num3++)
			{
				int[] indices = array2[num3].indices;
				for (int num4 = 0; num4 < indices.Length; num4++)
				{
					indices[num4] -= dictionary[indices[num4]];
				}
				array2[num3].SetIndices(indices);
			}
			pb_IntArray[] sharedIndices = pb.sharedIndices;
			pb_IntArray[] sharedIndices2 = pb.sharedIndicesUV;
			pb_IntArrayUtility.RemoveValuesAndShift(ref sharedIndices, list);
			pb_IntArrayUtility.RemoveValuesAndShift(ref sharedIndices2, list);
			pb.SetSharedIndices(sharedIndices);
			pb.SetSharedIndicesUV(sharedIndices2);
			pb.SetVertices(vertices);
			pb.SetColors(colors);
			pb.SetUV(uV);
			pb.SetFaces(array2);
			return list.ToArray();
		}
	}
}
