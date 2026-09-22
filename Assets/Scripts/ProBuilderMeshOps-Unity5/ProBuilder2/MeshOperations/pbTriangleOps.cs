using System;
using System.Collections.Generic;
using ProBuilder2.Common;
using UnityEngine;

namespace ProBuilder2.MeshOperations
{
	public static class pbTriangleOps
	{
		public static void ReverseWindingOrder(this pb_Object pb, pb_Face[] faces)
		{
			for (int i = 0; i < faces.Length; i++)
			{
				faces[i].ReverseIndices();
			}
		}

		public static WindingOrder GetWindingOrder(this pb_Object pb, pb_Face face)
		{
			Vector2[] points = pb_Projection.PlanarProject(pb, face);
			return GetWindingOrder(points);
		}

		public static WindingOrder GetWindingOrder(IList<pb_Vertex> vertices, IList<int> indices)
		{
			Vector2[] points = pb_Projection.PlanarProject(vertices, indices);
			return GetWindingOrder(points);
		}

		public static WindingOrder GetWindingOrder(IList<Vector2> points)
		{
			float num = 0f;
			int count = points.Count;
			for (int i = 0; i < count; i++)
			{
				Vector2 vector = points[i];
				Vector2 vector2 = ((i >= count - 1) ? points[0] : points[i + 1]);
				num += (vector2.x - vector.x) * (vector2.y + vector.y);
			}
			return (num != 0f) ? ((num > 0f) ? WindingOrder.Clockwise : WindingOrder.CounterClockwise) : WindingOrder.Unknown;
		}

		public static bool FlipEdge(this pb_Object pb, pb_Face face)
		{
			int[] indices = face.indices;
			if (indices.Length != 6)
			{
				return false;
			}
			int[] array = pbUtil.FilledArray(1, indices.Length);
			for (int i = 0; i < indices.Length - 1; i++)
			{
				for (int j = i + 1; j < indices.Length; j++)
				{
					if (indices[i] == indices[j])
					{
						array[i]++;
						array[j]++;
					}
				}
			}
			if (array[0] + array[1] + array[2] != 5 || array[3] + array[4] + array[5] != 5)
			{
				return false;
			}
			int num = indices[(array[0] != 1) ? ((array[1] == 1) ? 1 : 2) : 0];
			int num2 = indices[(array[3] == 1) ? 3 : ((array[4] != 1) ? 5 : 4)];
			int num3 = -1;
			if (array[0] == 2)
			{
				num3 = indices[0];
				indices[0] = num2;
			}
			else if (array[1] == 2)
			{
				num3 = indices[1];
				indices[1] = num2;
			}
			else if (array[2] == 2)
			{
				num3 = indices[2];
				indices[2] = num2;
			}
			if (array[3] == 2 && indices[3] != num3)
			{
				indices[3] = num;
			}
			else if (array[4] == 2 && indices[4] != num3)
			{
				indices[4] = num;
			}
			else if (array[5] == 2 && indices[5] != num3)
			{
				indices[5] = num;
			}
			return true;
		}

		public static bool RemoveDegenerateTriangles(this pb_Object pb, out int[] removed)
		{
			Dictionary<int, int> dictionary = pb.sharedIndices.ToDictionary();
			Dictionary<int, int> dictionary2 = ((pb.sharedIndicesUV == null) ? new Dictionary<int, int>() : pb.sharedIndicesUV.ToDictionary());
			Vector3[] vertices = pb.vertices;
			Dictionary<int, int> dictionary3 = new Dictionary<int, int>();
			Dictionary<int, int> dictionary4 = new Dictionary<int, int>();
			List<pb_Face> list = new List<pb_Face>();
			pb_Face[] faces = pb.faces;
			foreach (pb_Face pb_Face2 in faces)
			{
				List<int> list2 = new List<int>();
				int[] indices = pb_Face2.indices;
				for (int j = 0; j < indices.Length; j += 3)
				{
					float num = pb_Math.TriangleArea(vertices[indices[j]], vertices[indices[j + 1]], vertices[indices[j + 2]]);
					if (!(num > Mathf.Epsilon))
					{
						continue;
					}
					int num2 = dictionary[indices[j]];
					int num3 = dictionary[indices[j + 1]];
					int num4 = dictionary[indices[j + 2]];
					if (num2 != num3 && num2 != num4 && num3 != num4)
					{
						list2.Add(indices[j]);
						list2.Add(indices[j + 1]);
						list2.Add(indices[j + 2]);
						if (!dictionary3.ContainsKey(indices[j]))
						{
							dictionary3.Add(indices[j], num2);
						}
						if (!dictionary3.ContainsKey(indices[j + 1]))
						{
							dictionary3.Add(indices[j + 1], num3);
						}
						if (!dictionary3.ContainsKey(indices[j + 2]))
						{
							dictionary3.Add(indices[j + 2], num4);
						}
						if (dictionary2.ContainsKey(indices[j]) && !dictionary4.ContainsKey(indices[j]))
						{
							dictionary4.Add(indices[j], dictionary2[indices[j]]);
						}
						if (dictionary2.ContainsKey(indices[j + 1]) && !dictionary4.ContainsKey(indices[j + 1]))
						{
							dictionary4.Add(indices[j + 1], dictionary2[indices[j + 1]]);
						}
						if (dictionary2.ContainsKey(indices[j + 2]) && !dictionary4.ContainsKey(indices[j + 2]))
						{
							dictionary4.Add(indices[j + 2], dictionary2[indices[j + 2]]);
						}
					}
				}
				if (list2.Count > 0)
				{
					pb_Face2.SetIndices(list2.ToArray());
					pb_Face2.RebuildCaches();
					list.Add(pb_Face2);
				}
			}
			pb.SetFaces(list.ToArray());
			pb.SetSharedIndices(dictionary3);
			pb.SetSharedIndicesUV(dictionary4);
			removed = pb.RemoveUnusedVertices();
			return removed.Length > 0;
		}

		[Obsolete("Please use pb_MergeFaces.Merge(pb_Object target, IEnumerable<pb_Face> faces)")]
		public static pb_Face MergeFaces(this pb_Object pb, pb_Face[] faces)
		{
			List<int> list = new List<int>(faces[0].indices);
			for (int i = 1; i < faces.Length; i++)
			{
				list.AddRange(faces[i].indices);
			}
			pb_Face pb_Face2 = new pb_Face(list.ToArray(), faces[0].material, faces[0].uv, faces[0].smoothingGroup, faces[0].textureGroup, faces[0].elementGroup, faces[0].manualUV);
			pb_Face[] array = new pb_Face[pb.faces.Length - faces.Length + 1];
			int num = 0;
			pb_Face[] faces2 = pb.faces;
			foreach (pb_Face pb_Face3 in faces2)
			{
				if (Array.IndexOf(faces, pb_Face3) < 0)
				{
					array[num++] = pb_Face3;
				}
			}
			array[num] = pb_Face2;
			pb.SetFaces(array);
			Dictionary<int, int> dictionary = new Dictionary<int, int>();
			for (int k = 0; k < pb_Face2.indices.Length; k++)
			{
				int key = pb.sharedIndices.IndexOf(pb_Face2.indices[k]);
				if (dictionary.ContainsKey(key))
				{
					pb_Face2.indices[k] = dictionary[key];
				}
				else
				{
					dictionary.Add(key, pb_Face2.indices[k]);
				}
			}
			pb.RemoveUnusedVertices();
			return pb_Face2;
		}
	}
}
