using System;
using System.Collections.Generic;
using System.Linq;
using ProBuilder2.Common;
using UnityEngine;

namespace ProBuilder2.MeshOperations
{
	public static class pbUVOps
	{
		public static bool SewUVs(this pb_Object pb, int[] indices, float delta)
		{
			int[] array = new int[indices.Length];
			Vector2[] array2 = pb.uv;
			if (array2 == null || array2.Length != pb.vertexCount)
			{
				array2 = new Vector2[pb.vertexCount];
			}
			for (int i = 0; i < indices.Length; i++)
			{
				array[i] = -(i + 1);
			}
			pb_IntArray[] sharedIndices = pb.sharedIndicesUV;
			for (int j = 0; j < indices.Length - 1; j++)
			{
				for (int k = j + 1; k < indices.Length; k++)
				{
					if (array[j] != array[k] && Vector2.Distance(array2[indices[j]], array2[indices[k]]) < delta)
					{
						Vector3 vector = (array2[indices[j]] + array2[indices[k]]) / 2f;
						array2[indices[j]] = vector;
						array2[indices[k]] = vector;
						array[k] = (array[j] = pb_IntArrayUtility.MergeSharedIndices(ref sharedIndices, new int[2]
						{
							indices[j],
							indices[k]
						}));
					}
				}
			}
			pb.SetUV(array2);
			pb.SetSharedIndicesUV(sharedIndices);
			return true;
		}

		public static void CollapseUVs(this pb_Object pb, int[] indices)
		{
			Vector2[] uv = pb.uv;
			Vector2 vector = pb_Math.Average(uv.ValuesWithIndices(indices));
			foreach (int num in indices)
			{
				uv[num] = vector;
			}
			pb_IntArray[] sharedIndices = pb.sharedIndicesUV;
			pb_IntArrayUtility.MergeSharedIndices(ref sharedIndices, indices);
			pb.SetUV(uv);
			pb.SetSharedIndicesUV(sharedIndices);
		}

		public static bool SplitUVs(this pb_Object pb, int[] indices)
		{
			pb_IntArray[] sharedIndices = pb.sharedIndicesUV;
			if (sharedIndices == null)
			{
				return false;
			}
			List<int> list = indices.Distinct().ToList();
			for (int i = 0; i < list.Count; i++)
			{
				int num = sharedIndices.IndexOf(list[i]);
				if (num >= 0)
				{
					sharedIndices[num].array = sharedIndices[num].array.Remove(list[i]);
				}
			}
			foreach (int item in list)
			{
				pb_IntArrayUtility.AddValueAtIndex(ref sharedIndices, -1, item);
			}
			pb.SetSharedIndicesUV(sharedIndices);
			return true;
		}

		public static void ProjectFacesAuto(pb_Object pb, pb_Face[] faces)
		{
			int[] array = pb_Face.AllTrianglesDistinct(faces);
			Vector3 zero = Vector3.zero;
			foreach (pb_Face face in faces)
			{
				zero += pb_Math.Normal(pb, face);
			}
			zero /= (float)faces.Length;
			Vector2[] array2 = pb_Projection.PlanarProject(pb.vertices.ValuesWithIndices(array), zero);
			Vector2[] uv = pb.uv;
			for (int j = 0; j < array.Length; j++)
			{
				uv[array[j]] = array2[j];
			}
			pb.SetUV(uv);
			pb.msh.uv = uv;
			foreach (pb_Face pb_Face2 in faces)
			{
				pb_Face2.elementGroup = -1;
				pb.SplitUVs(pb_Face2.distinctIndices);
			}
			pb.SewUVs(pb_Face.AllTrianglesDistinct(faces), 0.001f);
		}

		public static void ProjectFacesBox(pb_Object pb, pb_Face[] faces)
		{
			Vector2[] uv = pb.uv;
			Dictionary<ProjectionAxis, List<pb_Face>> dictionary = new Dictionary<ProjectionAxis, List<pb_Face>>();
			for (int i = 0; i < faces.Length; i++)
			{
				Vector3 plane = pb_Math.Normal(pb, faces[i]);
				ProjectionAxis key = pb_Projection.VectorToProjectionAxis(plane);
				if (dictionary.ContainsKey(key))
				{
					dictionary[key].Add(faces[i]);
				}
				else
				{
					dictionary.Add(key, new List<pb_Face> { faces[i] });
				}
				faces[i].elementGroup = -1;
				faces[i].manualUV = true;
			}
			foreach (KeyValuePair<ProjectionAxis, List<pb_Face>> item in dictionary)
			{
				int[] array = pb_Face.AllTrianglesDistinct(item.Value.ToArray());
				Vector2[] array2 = pb_Projection.PlanarProject(pb.vertices.ValuesWithIndices(array), pb_Projection.ProjectionAxisToVector(item.Key), item.Key);
				for (int j = 0; j < array.Length; j++)
				{
					uv[array[j]] = array2[j];
				}
				pb.SplitUVs(array);
			}
			pb.SetUV(uv);
		}

		public static void ProjectFacesSphere(pb_Object pb, int[] indices)
		{
			pb_Face[] faces = pb.faces;
			foreach (pb_Face pb_Face2 in faces)
			{
				if (pb_Face2.distinctIndices.ContainsMatch(indices))
				{
					pb_Face2.elementGroup = -1;
					pb_Face2.manualUV = true;
				}
			}
			pb.SplitUVs(indices);
			Vector2[] array = pb_Projection.SphericalProject(pb.vertices, indices);
			Vector2[] uv = pb.uv;
			for (int j = 0; j < indices.Length; j++)
			{
				uv[indices[j]] = array[j];
			}
			pb.SetUV(uv);
		}

		public static Vector2[] FitUVs(Vector2[] uvs)
		{
			Vector2 vector = pb_Math.SmallestVector2(uvs);
			for (int i = 0; i < uvs.Length; i++)
			{
				uvs[i] -= vector;
			}
			float num = pb_Math.LargestValue(pb_Math.LargestVector2(uvs));
			for (int i = 0; i < uvs.Length; i++)
			{
				uvs[i] /= num;
			}
			return uvs;
		}

		public static bool AutoStitch(pb_Object pb, pb_Face f1, pb_Face f2)
		{
			Dictionary<int, int> lookup = pb.sharedIndices.ToDictionary();
			for (int i = 0; i < f1.edges.Length; i++)
			{
				int num = f2.edges.IndexOf(f1.edges[i], lookup);
				if (num > -1)
				{
					ProjectFacesAuto(pb, new pb_Face[1] { f2 });
					f1.manualUV = true;
					f2.manualUV = true;
					f1.textureGroup = -1;
					f2.textureGroup = -1;
					AlignEdges(pb, f1, f2, f1.edges[i], f2.edges[num]);
					return true;
				}
			}
			return false;
		}

		private static bool AlignEdges(pb_Object pb, pb_Face f1, pb_Face f2, pb_Edge edge1, pb_Edge edge2)
		{
			Vector2[] uv = pb.uv;
			pb_IntArray[] sharedIndices = pb.sharedIndices;
			pb_IntArray[] sharedIndices2 = pb.sharedIndicesUV;
			int[] array = new int[2] { edge1.x, -1 };
			int[] array2 = new int[2] { edge1.y, -1 };
			int num = sharedIndices.IndexOf(edge1.x);
			if (num < 0)
			{
				return false;
			}
			if (sharedIndices[num].array.Contains(edge2.x))
			{
				array[1] = edge2.x;
				array2[1] = edge2.y;
			}
			else
			{
				array[1] = edge2.y;
				array2[1] = edge2.x;
			}
			float num2 = Vector2.Distance(uv[edge1.x], uv[edge1.y]);
			float num3 = Vector2.Distance(uv[edge2.x], uv[edge2.y]);
			float num4 = num2 / num3;
			int[] distinctIndices = f2.distinctIndices;
			foreach (int num5 in distinctIndices)
			{
				uv[num5] = uv[num5].ScaleAroundPoint(Vector2.zero, Vector2.one * num4);
			}
			Vector2 vector = (uv[edge1.x] + uv[edge1.y]) / 2f;
			Vector2 vector2 = (uv[edge2.x] + uv[edge2.y]) / 2f;
			Vector2 vector3 = vector - vector2;
			int[] distinctIndices2 = f2.distinctIndices;
			foreach (int num6 in distinctIndices2)
			{
				uv[num6] += vector3;
			}
			Vector2 vector4 = uv[array2[0]] - uv[array[0]];
			Vector2 vector5 = uv[array2[1]] - uv[array[1]];
			float num7 = Vector2.Angle(vector4, vector5);
			if (Vector3.Cross(vector4, vector5).z < 0f)
			{
				num7 = 360f - num7;
			}
			int[] distinctIndices3 = f2.distinctIndices;
			foreach (int num8 in distinctIndices3)
			{
				uv[num8] = uv[num8].RotateAroundPoint(vector, num7);
			}
			float num9 = Mathf.Abs(Vector2.Distance(uv[array[0]], uv[array[1]])) + Mathf.Abs(Vector2.Distance(uv[array2[0]], uv[array2[1]]));
			if (num9 > 0.02f)
			{
				int[] distinctIndices4 = f2.distinctIndices;
				foreach (int num10 in distinctIndices4)
				{
					uv[num10] = uv[num10].RotateAroundPoint(vector, 180f);
				}
				float num11 = Mathf.Abs(Vector2.Distance(uv[array[0]], uv[array[1]])) + Mathf.Abs(Vector2.Distance(uv[array2[0]], uv[array2[1]]));
				if (num11 < num9)
				{
					num9 = num11;
				}
				else
				{
					int[] distinctIndices5 = f2.distinctIndices;
					foreach (int num12 in distinctIndices5)
					{
						uv[num12] = uv[num12].RotateAroundPoint(vector, 180f);
					}
				}
			}
			pb.SplitUVs(f2.distinctIndices);
			pb_IntArrayUtility.MergeSharedIndices(ref sharedIndices2, array);
			pb_IntArrayUtility.MergeSharedIndices(ref sharedIndices2, array2);
			pb_IntArray.RemoveEmptyOrNull(ref sharedIndices2);
			pb.SetSharedIndicesUV(sharedIndices2);
			pb.SetUV(uv);
			return true;
		}

		public static pb_Transform2D MatchCoordinates(Vector2[] points, Vector2[] target)
		{
			int length = ((points.Length >= target.Length) ? target.Length : points.Length);
			pb_Bounds2D pb_Bounds2D2 = new pb_Bounds2D(target, length);
			Vector2 vector = pb_Bounds2D2.center - pb_Bounds2D.Center(points, length);
			Vector2[] array = new Vector2[points.Length];
			for (int i = 0; i < points.Length; i++)
			{
				array[i] = points[i] + vector;
			}
			Vector2 vector2 = target[1] - target[0];
			Vector2 vector3 = array[1] - array[0];
			float num = Vector2.Angle(vector2, vector3);
			float num2 = Vector2.Dot(pb_Math.Perpendicular(vector2), vector3);
			if (num2 < 0f)
			{
				num = 360f - num;
			}
			for (int j = 0; j < points.Length; j++)
			{
				array[j] = array[j].RotateAroundPoint(pb_Bounds2D2.center, num);
			}
			pb_Bounds2D pb_Bounds2D3 = new pb_Bounds2D(array, length);
			Vector2 scale = pb_Bounds2D2.size.DivideBy(pb_Bounds2D3.size);
			return new pb_Transform2D(vector, num, scale);
		}

		public static void SetAutoUV(pb_Object pb, pb_Face[] faces, bool auto)
		{
			if (auto)
			{
				faces = Array.FindAll(faces, (pb_Face x) => x.manualUV).ToArray();
				pb.SplitUVs(pb_Face.AllTriangles(faces));
				Vector2[][] array = new Vector2[faces.Length][];
				for (int num = 0; num < faces.Length; num++)
				{
					array[num] = pb.uv.ValuesWithIndices(faces[num].distinctIndices);
				}
				for (int num2 = 0; num2 < faces.Length; num2++)
				{
					faces[num2].uv.Reset();
					faces[num2].manualUV = !auto;
					faces[num2].elementGroup = -1;
				}
				pb.RefreshUV(faces);
				for (int num3 = 0; num3 < faces.Length; num3++)
				{
					pb_Transform2D pb_Transform2D2 = MatchCoordinates(pb.uv.ValuesWithIndices(faces[num3].distinctIndices), array[num3]);
					faces[num3].uv.offset = -pb_Transform2D2.position;
					faces[num3].uv.rotation = pb_Transform2D2.rotation;
					if (Mathf.Abs(pb_Transform2D2.scale.sqrMagnitude - 2f) > 0.1f)
					{
						faces[num3].uv.scale = pb_Transform2D2.scale;
					}
				}
			}
			else
			{
				pb_Face[] array2 = faces;
				foreach (pb_Face pb_Face2 in array2)
				{
					pb_Face2.textureGroup = -1;
					pb_Face2.manualUV = !auto;
				}
			}
		}

		public static Vector2 NearestVector2(Vector2 pos, Vector2[] uvs)
		{
			if (uvs.Length < 1)
			{
				return pos;
			}
			Vector2 vector = uvs[0];
			float num = Vector2.Distance(pos, vector);
			for (int i = 1; i < uvs.Length; i++)
			{
				float num2 = Vector2.Distance(pos, uvs[i]);
				if (num2 < num)
				{
					num = num2;
					vector = uvs[i];
				}
			}
			return vector;
		}
	}
}
