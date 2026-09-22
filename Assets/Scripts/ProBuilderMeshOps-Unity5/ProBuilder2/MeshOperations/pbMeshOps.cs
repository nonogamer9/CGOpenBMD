using System;
using System.Collections.Generic;
using System.Linq;
using ProBuilder2.Common;
using UnityEngine;

namespace ProBuilder2.MeshOperations
{
	public static class pbMeshOps
	{
		public static void CenterPivot(this pb_Object pb, int[] indices)
		{
			Vector3 vector = Vector3.zero;
			if (indices != null && indices.Length > 0)
			{
				Vector3[] array = pb.VerticesInWorldSpace(indices);
				Vector3[] array2 = array;
				foreach (Vector3 vector2 in array2)
				{
					vector += vector2;
				}
				vector /= (float)array.Length;
			}
			else
			{
				vector = pb.transform.TransformPoint(pb.msh.bounds.center);
			}
			Vector3 offset = pb.transform.position - vector;
			pb.transform.position = vector;
			pb.ToMesh();
			pb.TranslateVertices_World(pb.msh.triangles, offset);
			pb.Refresh();
		}

		public static void CenterPivot(this pb_Object pb, Vector3 worldPosition)
		{
			Vector3 offset = pb.transform.position - worldPosition;
			pb.transform.position = worldPosition;
			pb.ToMesh();
			pb.TranslateVertices_World(pb.msh.triangles, offset);
			pb.Refresh();
		}

		public static void FreezeScaleTransform(this pb_Object pb)
		{
			Vector3[] vertices = pb.vertices;
			for (int i = 0; i < vertices.Length; i++)
			{
				vertices[i] = Vector3.Scale(vertices[i], pb.transform.localScale);
			}
			pb.SetVertices(vertices);
			pb.transform.localScale = new Vector3(1f, 1f, 1f);
		}

		[Obsolete("Please use `bool Extrude(this pb_Object pb, pb_Face[] faces, ExtrudeMethod method, float distance)`")]
		public static bool Extrude(this pb_Object pb, pb_Face[] faces, float extrudeDistance)
		{
			pb_Face[] appendedFaces;
			return pb.Extrude(faces, extrudeDistance, true, out appendedFaces);
		}

		[Obsolete("Please use `bool Extrude(this pb_Object pb, pb_Face[] faces, ExtrudeMethod method, float distance)`")]
		public static bool Extrude(this pb_Object pb, pb_Face[] faces, float extrudeDistance, bool extrudeAsGroup, out pb_Face[] appendedFaces)
		{
			return pb.Extrude(faces, extrudeAsGroup ? ExtrudeMethod.VertexNormal : ExtrudeMethod.IndividualFaces, extrudeDistance, out appendedFaces);
		}

		[Obsolete("Please use `bool Extrude(this pb_Object pb, pb_Face[] faces, ExtrudeMethod method, float distance)`")]
		public static bool Extrude(this pb_Object pb, pb_Face[] faces, ExtrudeMethod method, float extrudeDistance, out pb_Face[] appendedFaces)
		{
			appendedFaces = null;
			if (faces == null || faces.Length < 1)
			{
				return false;
			}
			pb_IntArray[] sharedIndices = pb.GetSharedIndices();
			Dictionary<int, int> dictionary = sharedIndices.ToDictionary();
			int vertexCount = pb.vertexCount;
			Vector3[] vertices = pb.vertices;
			bool flag = method != ExtrudeMethod.IndividualFaces;
			pb_Edge[][] array = ((!flag) ? faces.Select((pb_Face pb_Face7) => pb_Face7.edges).ToArray() : new pb_Edge[1][] { pbMeshUtils.GetPerimeterEdges(dictionary, faces).ToArray() });
			if (array == null || array.Length < 1 || (flag && array[0].Length < 3))
			{
				Debug.LogWarning("No perimeter edges found.  Try deselecting and reselecting this object and trying again.");
				return false;
			}
			pb_Face[][] array2 = new pb_Face[array.Length][];
			int[][] array3 = new int[array.Length][];
			int num = 0;
			for (int num2 = 0; num2 < array.Length; num2++)
			{
				num = 0;
				array3[num2] = new int[array[num2].Length * 2];
				array2[num2] = new pb_Face[array[num2].Length];
				for (int num3 = 0; num3 < array[num2].Length; num3++)
				{
					foreach (pb_Face pb_Face2 in faces)
					{
						if (pb_Face2.edges.Contains(array[num2][num3]))
						{
							array2[num2][num3] = pb_Face2;
							break;
						}
					}
					array3[num2][num++] = array[num2][num3].x;
					array3[num2][num++] = array[num2][num3].y;
				}
			}
			List<pb_Edge>[] array4 = new List<pb_Edge>[array.Length];
			Vector3[] normals = pb.msh.normals;
			Vector3[] array5 = new Vector3[vertexCount];
			List<Vector3[]> list = new List<Vector3[]>();
			List<Color[]> list2 = new List<Color[]>();
			List<Vector2[]> list3 = new List<Vector2[]>();
			List<pb_Face> list4 = new List<pb_Face>();
			List<int[]> list5 = new List<int[]>();
			for (int num5 = 0; num5 < array.Length; num5++)
			{
				array4[num5] = new List<pb_Edge>();
				for (int num6 = 0; num6 < array[num5].Length; num6++)
				{
					pb_Edge pb_Edge2 = array[num5][num6];
					pb_Face pb_Face3 = array2[num5][num6];
					Vector3 vector = pb_Math.Normal(pb, pb_Face3);
					Vector3 to = Vector3.zero;
					Vector3 to2 = Vector3.zero;
					if (Mathf.Abs(extrudeDistance) > Mathf.Epsilon)
					{
						if (!flag)
						{
							to = vector;
							to2 = vector;
						}
						else
						{
							to = Norm(sharedIndices[dictionary[pb_Edge2.x]], array3[num5], normals);
							to2 = Norm(sharedIndices[dictionary[pb_Edge2.y]], array3[num5], normals);
						}
					}
					int num7 = dictionary[pb_Edge2.x];
					int num8 = dictionary[pb_Edge2.y];
					float num9 = extrudeDistance;
					float num10 = extrudeDistance;
					if (method == ExtrudeMethod.FaceNormal)
					{
						num9 = pb_Math.Secant(Vector3.Angle(vector, to) * ((float)Math.PI / 180f)) * extrudeDistance;
						num10 = pb_Math.Secant(Vector3.Angle(vector, to2) * ((float)Math.PI / 180f)) * extrudeDistance;
					}
					array5[pb_Edge2.x] = to.normalized * num9;
					array5[pb_Edge2.y] = to2.normalized * num10;
					list.Add(new Vector3[4]
					{
						vertices[pb_Edge2.x],
						vertices[pb_Edge2.y],
						vertices[pb_Edge2.x] + array5[pb_Edge2.x],
						vertices[pb_Edge2.y] + array5[pb_Edge2.y]
					});
					list2.Add(new Color[4]
					{
						pb.colors[pb_Edge2.x],
						pb.colors[pb_Edge2.y],
						pb.colors[pb_Edge2.x],
						pb.colors[pb_Edge2.y]
					});
					list3.Add(new Vector2[4]);
					list4.Add(new pb_Face(new int[6] { 0, 1, 2, 1, 3, 2 }, pb_Face3.material, new pb_UV(pb_Face3.uv), pb_Face3.smoothingGroup, -1, -1, false));
					list5.Add(new int[4] { num7, num8, -1, -1 });
					array4[num5].Add(new pb_Edge(num7, -1));
					array4[num5].Add(new pb_Edge(num8, -1));
				}
			}
			appendedFaces = pb.AppendFaces(list.ToArray(), list2.ToArray(), list3.ToArray(), list4.ToArray(), list5.ToArray());
			int num11 = 0;
			int num12 = 0;
			for (; num11 < array4.Length; num11++)
			{
				for (int num13 = 0; num13 < array4[num11].Count; num13 += 2)
				{
					array4[num11][num13] = new pb_Edge(array4[num11][num13].x, appendedFaces[num12].indices[2]);
					array4[num11][num13 + 1] = new pb_Edge(array4[num11][num13 + 1].x, appendedFaces[num12++].indices[4]);
				}
			}
			pb_IntArray[] sharedIndices2 = pb.sharedIndices;
			Dictionary<int, int> dictionary2 = sharedIndices2.ToDictionary();
			for (int num14 = 0; num14 < array4.Length; num14++)
			{
				for (int num15 = 0; num15 < array4[num14].Count - 1; num15++)
				{
					int x = array4[num14][num15].x;
					for (int num16 = num15 + 1; num16 < array4[num14].Count; num16++)
					{
						if (array4[num14][num16].x == x)
						{
							dictionary2[array4[num14][num15].y] = dictionary2[array4[num14][num16].y];
							break;
						}
					}
				}
			}
			vertices = pb.vertices;
			foreach (pb_Face pb_Face4 in faces)
			{
				pb_Face4.smoothingGroup = 0;
				pb_Face4.textureGroup = -1;
			}
			if (flag)
			{
				foreach (pb_Face pb_Face5 in faces)
				{
					int[] distinctIndices = pb_Face5.distinctIndices;
					int[] array6 = distinctIndices;
					foreach (int num20 in array6)
					{
						int num21 = sharedIndices2.IndexOf(num20);
						for (int num22 = 0; num22 < array3.Length; num22++)
						{
							for (int num23 = 0; num23 < array4[num22].Count; num23++)
							{
								if (num21 == array4[num22][num23].x)
								{
									dictionary2[num20] = dictionary2[array4[num22][num23].y];
									break;
								}
							}
						}
					}
				}
			}
			else
			{
				for (int num24 = 0; num24 < array2.Length; num24++)
				{
					int[] array7 = pb_Face.AllTrianglesDistinct(array2[num24]);
					foreach (int key in array7)
					{
						int old_si_index = dictionary[key];
						int num26 = array4[num24].FindIndex((pb_Edge pb_Edge3) => pb_Edge3.x == old_si_index);
						if (num26 >= 0)
						{
							int y = array4[num24][num26].y;
							if (dictionary2.ContainsKey(y))
							{
								dictionary2[key] = dictionary2[y];
							}
						}
					}
				}
			}
			sharedIndices2 = dictionary2.ToSharedIndices();
			pb.SplitUVs(pb_Face.AllTriangles(faces));
			int[] all = pb_Face.AllTrianglesDistinct(faces);
			float num27 = extrudeDistance;
			foreach (pb_Face pb_Face6 in faces)
			{
				Vector3 vector2 = pb_Math.Normal(vertices[pb_Face6.indices[0]], vertices[pb_Face6.indices[1]], vertices[pb_Face6.indices[2]]);
				Vector3 to3 = ((!flag) ? vector2 : Vector3.zero);
				int[] distinctIndices2 = pb_Face6.distinctIndices;
				foreach (int num30 in distinctIndices2)
				{
					if (flag)
					{
						to3 = Norm(sharedIndices[dictionary[num30]], all, normals);
						if (method == ExtrudeMethod.FaceNormal)
						{
							num27 = pb_Math.Secant(Vector3.Angle(vector2, to3) * ((float)Math.PI / 180f)) * extrudeDistance;
						}
					}
					vertices[num30] += to3.normalized * num27;
				}
			}
			pb.SetSharedIndices(sharedIndices2);
			pb.SetVertices(vertices);
			List<pb_Face> list6 = new List<pb_Face>(appendedFaces);
			list6.AddRange(faces);
			HashSet<pb_Face> hashSet = new HashSet<pb_Face>(faces);
			List<pb_WingedEdge> wingedEdges = pb_WingedEdge.GetWingedEdges(pb, list6);
			foreach (pb_WingedEdge item in wingedEdges)
			{
				if (!hashSet.Contains(item.face))
				{
					continue;
				}
				hashSet.Remove(item.face);
				foreach (pb_WingedEdge item2 in item)
				{
					pb_ConformNormals.ConformOppositeNormal(item2);
				}
			}
			return true;
		}

		private static Vector3 Norm(int[] shared, int[] all, Vector3[] norm)
		{
			Vector3 zero = Vector3.zero;
			int num = 0;
			for (int i = 0; i < all.Length; i++)
			{
				if (Array.IndexOf(shared, all[i]) > -1)
				{
					zero += norm[all[i]];
					num++;
				}
			}
			return zero / num;
		}

		public static bool Extrude(this pb_Object pb, pb_Edge[] edges, float extrudeDistance, bool extrudeAsGroup, bool enableManifoldExtrude, out pb_Edge[] extrudedEdges)
		{
			pb_IntArray[] sharedIndices = pb.sharedIndices;
			Dictionary<int, int> dictionary = sharedIndices.ToDictionary();
			List<pb_Edge> list = new List<pb_Edge>();
			List<pb_Face> list2 = new List<pb_Face>();
			foreach (pb_Edge pb_Edge2 in edges)
			{
				int num = 0;
				pb_Face item = null;
				pb_Face[] faces = pb.faces;
				foreach (pb_Face pb_Face2 in faces)
				{
					if (pb_Face2.edges.IndexOf(pb_Edge2, dictionary) > -1)
					{
						item = pb_Face2;
						if (++num > 1)
						{
							break;
						}
					}
				}
				if (enableManifoldExtrude || num < 2)
				{
					list.Add(pb_Edge2);
					list2.Add(item);
				}
			}
			if (list.Count < 1)
			{
				extrudedEdges = null;
				return false;
			}
			Vector3[] vertices = pb.vertices;
			Vector3[] normals = pb.msh.normals;
			int[] array = new int[list.Count * 2];
			int num2 = 0;
			for (int k = 0; k < list.Count; k++)
			{
				array[num2++] = list[k].x;
				array[num2++] = list[k].y;
			}
			List<pb_Edge> list3 = new List<pb_Edge>();
			List<pb_Edge> list4 = new List<pb_Edge>();
			for (int l = 0; l < list.Count; l++)
			{
				pb_Edge pb_Edge3 = list[l];
				pb_Face pb_Face3 = list2[l];
				Vector3 vector = ((!extrudeAsGroup) ? pb_Math.Normal(pb, pb_Face3) : Norm(sharedIndices[dictionary[pb_Edge3.x]], array, normals));
				Vector3 vector2 = ((!extrudeAsGroup) ? pb_Math.Normal(pb, pb_Face3) : Norm(sharedIndices[dictionary[pb_Edge3.y]], array, normals));
				int num3 = dictionary[pb_Edge3.x];
				int num4 = dictionary[pb_Edge3.y];
				pb_Face pb_Face4 = pb.AppendFace(new Vector3[4]
				{
					vertices[pb_Edge3.x],
					vertices[pb_Edge3.y],
					vertices[pb_Edge3.x] + vector.normalized * extrudeDistance,
					vertices[pb_Edge3.y] + vector2.normalized * extrudeDistance
				}, new Color[4]
				{
					pb.colors[pb_Edge3.x],
					pb.colors[pb_Edge3.y],
					pb.colors[pb_Edge3.x],
					pb.colors[pb_Edge3.y]
				}, new Vector2[4], new pb_Face(new int[6] { 2, 1, 0, 2, 3, 1 }, pb_Face3.material, new pb_UV(), 0, -1, -1, false), new int[4] { num3, num4, -1, -1 });
				list4.Add(new pb_Edge(pb_Face4.indices[3], pb_Face4.indices[4]));
				list3.Add(new pb_Edge(num3, pb_Face4.indices[3]));
				list3.Add(new pb_Edge(num4, pb_Face4.indices[4]));
			}
			sharedIndices = pb.sharedIndices;
			if (extrudeAsGroup)
			{
				for (int m = 0; m < list3.Count; m++)
				{
					int x = list3[m].x;
					for (int n = 0; n < list3.Count; n++)
					{
						if (n != m && list3[n].x == x)
						{
							pb_IntArrayUtility.MergeSharedIndices(ref sharedIndices, list3[n].y, list3[m].y);
							break;
						}
					}
				}
			}
			pb.SetSharedIndices(sharedIndices);
			pb_Face[] faces2 = pb.faces;
			foreach (pb_Face pb_Face5 in faces2)
			{
				pb_Face5.RebuildCaches();
			}
			extrudedEdges = list4.ToArray();
			return true;
		}

		public static List<pb_Face> DetachFaces(this pb_Object pb, IEnumerable<pb_Face> faces)
		{
			List<pb_Vertex> list = new List<pb_Vertex>(pb_Vertex.GetVertices(pb));
			int num = pb.sharedIndices.Length;
			Dictionary<int, int> dictionary = pb.sharedIndices.ToDictionary();
			List<pb_FaceRebuildData> list2 = new List<pb_FaceRebuildData>();
			foreach (pb_Face face in faces)
			{
				pb_FaceRebuildData pb_FaceRebuildData2 = new pb_FaceRebuildData();
				pb_FaceRebuildData2.vertices = new List<pb_Vertex>();
				pb_FaceRebuildData2.sharedIndices = new List<int>();
				pb_FaceRebuildData2.face = new pb_Face(face);
				Dictionary<int, int> dictionary2 = new Dictionary<int, int>();
				int[] array = new int[face.indices.Length];
				for (int i = 0; i < face.indices.Length; i++)
				{
					int value;
					if (dictionary2.TryGetValue(face.indices[i], out value))
					{
						array[i] = value;
						continue;
					}
					value = (array[i] = pb_FaceRebuildData2.vertices.Count);
					dictionary2.Add(face.indices[i], value);
					pb_FaceRebuildData2.vertices.Add(list[face.indices[i]]);
					pb_FaceRebuildData2.sharedIndices.Add(dictionary[face.indices[i]] + num);
				}
				pb_FaceRebuildData2.face.SetIndices(array.ToArray());
				list2.Add(pb_FaceRebuildData2);
			}
			pb_FaceRebuildData.Apply(list2, pb, list, null, dictionary);
			pb.DeleteFaces(faces);
			pb.ToMesh();
			return list2.Select((pb_FaceRebuildData x) => x.face).ToList();
		}

		public static bool Bridge(this pb_Object pb, pb_Edge a, pb_Edge b, bool enforcePerimiterEdgesOnly = false)
		{
			pb_IntArray[] sharedIndices = pb.GetSharedIndices();
			Dictionary<int, int> lookup = sharedIndices.ToDictionary();
			if (enforcePerimiterEdgesOnly && (pbMeshUtils.GetNeighborFaces(pb, a).Count > 1 || pbMeshUtils.GetNeighborFaces(pb, b).Count > 1))
			{
				return false;
			}
			pb_Face[] faces = pb.faces;
			foreach (pb_Face pb_Face2 in faces)
			{
				if (pb_Face2.edges.IndexOf(a, lookup) >= 0 && pb_Face2.edges.IndexOf(b, lookup) >= 0)
				{
					Debug.LogWarning("Face already exists between these two edges!");
					return false;
				}
			}
			Vector3[] vertices = pb.vertices;
			pb_UV u = new pb_UV();
			Material m = pb_Constant.DefaultMaterial;
			pb_Tuple<pb_Face, pb_Edge> validEdge = null;
			if (!pb_Edge.ValidateEdge(pb, a, out validEdge))
			{
				pb_Edge.ValidateEdge(pb, b, out validEdge);
			}
			if (validEdge != null)
			{
				u = new pb_UV(validEdge.Item1.uv);
				m = validEdge.Item1.material;
			}
			Vector3[] array;
			Color[] array2;
			int[] array3;
			if (a.Contains(b.x, sharedIndices) || a.Contains(b.y, sharedIndices))
			{
				array = new Vector3[3];
				array2 = new Color[3];
				array3 = new int[3];
				bool flag = Array.IndexOf(sharedIndices[sharedIndices.IndexOf(a.x)], b.x) > -1;
				bool flag2 = Array.IndexOf(sharedIndices[sharedIndices.IndexOf(a.x)], b.y) > -1;
				bool flag3 = Array.IndexOf(sharedIndices[sharedIndices.IndexOf(a.y)], b.x) > -1;
				bool flag4 = Array.IndexOf(sharedIndices[sharedIndices.IndexOf(a.y)], b.y) > -1;
				if (flag)
				{
					array[0] = vertices[a.x];
					array2[0] = pb.colors[a.x];
					array3[0] = sharedIndices.IndexOf(a.x);
					array[1] = vertices[a.y];
					array2[1] = pb.colors[a.y];
					array3[1] = sharedIndices.IndexOf(a.y);
					array[2] = vertices[b.y];
					array2[2] = pb.colors[b.y];
					array3[2] = sharedIndices.IndexOf(b.y);
				}
				else if (flag2)
				{
					array[0] = vertices[a.x];
					array2[0] = pb.colors[a.x];
					array3[0] = sharedIndices.IndexOf(a.x);
					array[1] = vertices[a.y];
					array2[1] = pb.colors[a.y];
					array3[1] = sharedIndices.IndexOf(a.y);
					array[2] = vertices[b.x];
					array2[2] = pb.colors[b.x];
					array3[2] = sharedIndices.IndexOf(b.x);
				}
				else if (flag3)
				{
					array[0] = vertices[a.y];
					array2[0] = pb.colors[a.y];
					array3[0] = sharedIndices.IndexOf(a.y);
					array[1] = vertices[a.x];
					array2[1] = pb.colors[a.x];
					array3[1] = sharedIndices.IndexOf(a.x);
					array[2] = vertices[b.y];
					array2[2] = pb.colors[b.y];
					array3[2] = sharedIndices.IndexOf(b.y);
				}
				else if (flag4)
				{
					array[0] = vertices[a.y];
					array2[0] = pb.colors[a.y];
					array3[0] = sharedIndices.IndexOf(a.y);
					array[1] = vertices[a.x];
					array2[1] = pb.colors[a.x];
					array3[1] = sharedIndices.IndexOf(a.x);
					array[2] = vertices[b.x];
					array2[2] = pb.colors[b.x];
					array3[2] = sharedIndices.IndexOf(b.x);
				}
				pb.AppendFace(array, array2, new Vector2[array.Length], new pb_Face((flag || flag2) ? new int[3] { 2, 1, 0 } : new int[3] { 0, 1, 2 }, m, u, 0, -1, -1, false), array3);
				return true;
			}
			array = new Vector3[4];
			array2 = new Color[4];
			array3 = new int[4];
			array[0] = vertices[a.x];
			array2[0] = pb.colors[a.x];
			array3[0] = sharedIndices.IndexOf(a.x);
			array[1] = vertices[a.y];
			array2[1] = pb.colors[a.y];
			array3[1] = sharedIndices.IndexOf(a.y);
			Vector3 normalized = Vector3.Cross(vertices[b.x] - vertices[a.x], vertices[a.y] - vertices[a.x]).normalized;
			Vector2[] array4 = pb_Projection.PlanarProject(new Vector3[4]
			{
				vertices[a.x],
				vertices[a.y],
				vertices[b.x],
				vertices[b.y]
			}, normalized);
			Vector2 intersect = Vector2.zero;
			if (!pb_Math.GetLineSegmentIntersect(array4[0], array4[2], array4[1], array4[3], ref intersect))
			{
				array[2] = vertices[b.x];
				array2[2] = pb.colors[b.x];
				array3[2] = sharedIndices.IndexOf(b.x);
				array[3] = vertices[b.y];
				array2[3] = pb.colors[b.y];
				array3[3] = sharedIndices.IndexOf(b.y);
			}
			else
			{
				array[2] = vertices[b.y];
				array2[2] = pb.colors[b.y];
				array3[2] = sharedIndices.IndexOf(b.y);
				array[3] = vertices[b.x];
				array2[3] = pb.colors[b.x];
				array3[3] = sharedIndices.IndexOf(b.x);
			}
			pb.AppendFace(array, array2, new Vector2[array.Length], new pb_Face(new int[6] { 2, 1, 0, 2, 3, 1 }, m, u, 0, -1, -1, false), array3);
			return true;
		}

		public static bool CombineObjects(pb_Object[] pbs, out pb_Object combined)
		{
			combined = null;
			if (pbs.Length < 1)
			{
				return false;
			}
			List<Vector3> list = new List<Vector3>();
			List<Vector2> list2 = new List<Vector2>();
			List<Color> list3 = new List<Color>();
			List<pb_Face> list4 = new List<pb_Face>();
			List<pb_IntArray> list5 = new List<pb_IntArray>();
			List<pb_IntArray> list6 = new List<pb_IntArray>();
			foreach (pb_Object pb_Object2 in pbs)
			{
				int count = list.Count;
				list.AddRange(pb_Object2.VerticesInWorldSpace());
				list2.AddRange(pb_Object2.uv);
				list3.AddRange(pb_Object2.colors);
				pb_Face[] array = new pb_Face[pb_Object2.faces.Length];
				for (int j = 0; j < array.Length; j++)
				{
					array[j] = new pb_Face(pb_Object2.faces[j]);
					array[j].manualUV = true;
					array[j].ShiftIndices(count);
					array[j].RebuildCaches();
				}
				list4.AddRange(array);
				pb_IntArray[] sharedIndices = pb_Object2.GetSharedIndices();
				for (int k = 0; k < sharedIndices.Length; k++)
				{
					for (int l = 0; l < sharedIndices[k].Length; l++)
					{
						sharedIndices[k][l] += count;
					}
				}
				list5.AddRange(sharedIndices);
				pb_IntArray[] sharedIndicesUV = pb_Object2.GetSharedIndicesUV();
				for (int m = 0; m < sharedIndicesUV.Length; m++)
				{
					for (int n = 0; n < sharedIndicesUV[m].Length; n++)
					{
						sharedIndicesUV[m][n] += count;
					}
				}
				list6.AddRange(sharedIndicesUV);
			}
			GameObject gameObject = UnityEngine.Object.Instantiate(pbs[0].gameObject);
			gameObject.transform.position = Vector3.zero;
			gameObject.transform.localRotation = Quaternion.identity;
			gameObject.transform.localScale = Vector3.one;
			foreach (Transform item in gameObject.transform)
			{
				UnityEngine.Object.DestroyImmediate(item.gameObject);
			}
			if ((bool)gameObject.GetComponent<pb_Object>())
			{
				UnityEngine.Object.DestroyImmediate(gameObject.GetComponent<pb_Object>());
			}
			if ((bool)gameObject.GetComponent<pb_Entity>())
			{
				UnityEngine.Object.DestroyImmediate(gameObject.GetComponent<pb_Entity>());
			}
			combined = gameObject.AddComponent<pb_Object>();
			combined.SetVertices(list.ToArray());
			combined.SetUV(list2.ToArray());
			combined.SetColors(list3.ToArray());
			combined.SetFaces(list4.ToArray());
			combined.SetSharedIndices(list5.ToArray() ?? pb_IntArrayUtility.ExtractSharedIndices(list.ToArray()));
			combined.SetSharedIndicesUV(list6.ToArray() ?? new pb_IntArray[0]);
			combined.ToMesh();
			combined.GetComponent<pb_Entity>().SetEntity(pbs[0].GetComponent<pb_Entity>().entityType);
			combined.CenterPivot(pbs[0].transform.position);
			combined.Refresh();
			foreach (pb_Object pb_Object3 in pbs)
			{
				pb_Object3.Verify();
			}
			return true;
		}

		public static pb_Object CreatePbObjectWithTransform(Transform t, bool preserveFaces)
		{
			Mesh sharedMesh = t.GetComponent<MeshFilter>().sharedMesh;
			Vector3[] meshAttribute = pb_MeshUtility.GetMeshAttribute(t.gameObject, (Mesh x) => x.vertices);
			Color[] meshAttribute2 = pb_MeshUtility.GetMeshAttribute(t.gameObject, (Mesh x) => x.colors);
			Vector2[] meshAttribute3 = pb_MeshUtility.GetMeshAttribute(t.gameObject, (Mesh x) => x.uv);
			List<Vector3> list = ((!preserveFaces) ? new List<Vector3>() : new List<Vector3>(sharedMesh.vertices));
			List<Color> list2 = ((!preserveFaces) ? new List<Color>() : new List<Color>(sharedMesh.colors));
			List<Vector2> list3 = ((!preserveFaces) ? new List<Vector2>() : new List<Vector2>(sharedMesh.uv));
			List<pb_Face> list4 = new List<pb_Face>();
			for (int num = 0; num < sharedMesh.subMeshCount; num++)
			{
				int[] triangles = sharedMesh.GetTriangles(num);
				for (int num2 = 0; num2 < triangles.Length; num2 += 3)
				{
					int num3 = -1;
					if (preserveFaces)
					{
						for (int num4 = 0; num4 < list4.Count; num4++)
						{
							if (list4[num4].distinctIndices.Contains(triangles[num2]) || list4[num4].distinctIndices.Contains(triangles[num2 + 1]) || list4[num4].distinctIndices.Contains(triangles[num2 + 2]))
							{
								num3 = num4;
								break;
							}
						}
					}
					if (num3 > -1 && preserveFaces)
					{
						int num5 = list4[num3].indices.Length;
						int[] array = new int[num5 + 3];
						Array.Copy(list4[num3].indices, 0, array, 0, num5);
						array[num5] = triangles[num2];
						array[num5 + 1] = triangles[num2 + 1];
						array[num5 + 2] = triangles[num2 + 2];
						list4[num3].SetIndices(array);
						list4[num3].RebuildCaches();
						continue;
					}
					int[] i;
					if (preserveFaces)
					{
						i = new int[3]
						{
							triangles[num2],
							triangles[num2 + 1],
							triangles[num2 + 2]
						};
					}
					else
					{
						list.Add(meshAttribute[triangles[num2]]);
						list.Add(meshAttribute[triangles[num2 + 1]]);
						list.Add(meshAttribute[triangles[num2 + 2]]);
						list2.Add((meshAttribute2 == null) ? Color.white : meshAttribute2[triangles[num2]]);
						list2.Add((meshAttribute2 == null) ? Color.white : meshAttribute2[triangles[num2 + 1]]);
						list2.Add((meshAttribute2 == null) ? Color.white : meshAttribute2[triangles[num2 + 2]]);
						list3.Add(meshAttribute3[triangles[num2]]);
						list3.Add(meshAttribute3[triangles[num2 + 1]]);
						list3.Add(meshAttribute3[triangles[num2 + 2]]);
						i = new int[3]
						{
							num2,
							num2 + 1,
							num2 + 2
						};
					}
					list4.Add(new pb_Face(i, t.GetComponent<MeshRenderer>().sharedMaterials[num], new pb_UV(), 0, -1, -1, true));
				}
			}
			GameObject gameObject = UnityEngine.Object.Instantiate(t.gameObject);
			gameObject.GetComponent<MeshFilter>().sharedMesh = null;
			pb_Object pb_Object2 = gameObject.AddComponent<pb_Object>();
			pb_Object2.GeometryWithVerticesFaces(list.ToArray(), list4.ToArray());
			pb_Object2.SetColors(list2.ToArray());
			pb_Object2.SetUV(list3.ToArray());
			pb_Object2.gameObject.name = t.name;
			gameObject.transform.position = t.position;
			gameObject.transform.localRotation = t.localRotation;
			gameObject.transform.localScale = t.localScale;
			pb_Object2.CenterPivot(null);
			return pb_Object2;
		}

		public static bool ResetPbObjectWithMeshFilter(pb_Object pb, bool preserveFaces)
		{
			MeshFilter component = pb.gameObject.GetComponent<MeshFilter>();
			if (component == null || component.sharedMesh == null)
			{
				pb_Log.Error(pb.name + " does not have a mesh or Mesh Filter component.");
				return false;
			}
			Mesh sharedMesh = component.sharedMesh;
			int vertexCount = sharedMesh.vertexCount;
			Vector3[] meshAttribute = pb_MeshUtility.GetMeshAttribute(pb.gameObject, (Mesh x) => x.vertices);
			Color[] meshAttribute2 = pb_MeshUtility.GetMeshAttribute(pb.gameObject, (Mesh x) => x.colors);
			Vector2[] meshAttribute3 = pb_MeshUtility.GetMeshAttribute(pb.gameObject, (Mesh x) => x.uv);
			List<Vector3> list = ((!preserveFaces) ? new List<Vector3>() : new List<Vector3>(sharedMesh.vertices));
			List<Color> list2 = ((!preserveFaces) ? new List<Color>() : new List<Color>(sharedMesh.colors));
			List<Vector2> list3 = ((!preserveFaces) ? new List<Vector2>() : new List<Vector2>(sharedMesh.uv));
			List<pb_Face> list4 = new List<pb_Face>();
			MeshRenderer meshRenderer = pb.gameObject.GetComponent<MeshRenderer>();
			if (meshRenderer == null)
			{
				meshRenderer = pb.gameObject.AddComponent<MeshRenderer>();
			}
			Material[] sharedMaterials = meshRenderer.sharedMaterials;
			int num = sharedMaterials.Length;
			for (int num2 = 0; num2 < sharedMesh.subMeshCount; num2++)
			{
				int[] triangles = sharedMesh.GetTriangles(num2);
				for (int num3 = 0; num3 < triangles.Length; num3 += 3)
				{
					int num4 = -1;
					if (preserveFaces)
					{
						for (int num5 = 0; num5 < list4.Count; num5++)
						{
							if (list4[num5].distinctIndices.Contains(triangles[num3]) || list4[num5].distinctIndices.Contains(triangles[num3 + 1]) || list4[num5].distinctIndices.Contains(triangles[num3 + 2]))
							{
								num4 = num5;
								break;
							}
						}
					}
					if (num4 > -1 && preserveFaces)
					{
						int num6 = list4[num4].indices.Length;
						int[] array = new int[num6 + 3];
						Array.Copy(list4[num4].indices, 0, array, 0, num6);
						array[num6] = triangles[num3];
						array[num6 + 1] = triangles[num3 + 1];
						array[num6 + 2] = triangles[num3 + 2];
						list4[num4].SetIndices(array);
						list4[num4].RebuildCaches();
						continue;
					}
					int[] i;
					if (preserveFaces)
					{
						i = new int[3]
						{
							triangles[num3],
							triangles[num3 + 1],
							triangles[num3 + 2]
						};
					}
					else
					{
						list.Add(meshAttribute[triangles[num3]]);
						list.Add(meshAttribute[triangles[num3 + 1]]);
						list.Add(meshAttribute[triangles[num3 + 2]]);
						list2.Add((meshAttribute2 == null || meshAttribute2.Length != vertexCount) ? Color.white : meshAttribute2[triangles[num3]]);
						list2.Add((meshAttribute2 == null || meshAttribute2.Length != vertexCount) ? Color.white : meshAttribute2[triangles[num3 + 1]]);
						list2.Add((meshAttribute2 == null || meshAttribute2.Length != vertexCount) ? Color.white : meshAttribute2[triangles[num3 + 2]]);
						list3.Add(meshAttribute3[triangles[num3]]);
						list3.Add(meshAttribute3[triangles[num3 + 1]]);
						list3.Add(meshAttribute3[triangles[num3 + 2]]);
						i = new int[3]
						{
							num3,
							num3 + 1,
							num3 + 2
						};
					}
					list4.Add(new pb_Face(i, sharedMaterials[(num2 < num) ? num2 : (num - 1)], new pb_UV(), 0, -1, -1, true));
				}
			}
			pb.SetVertices(list.ToArray());
			pb.SetUV(list3.ToArray());
			pb.SetFaces(list4.ToArray());
			pb.SetSharedIndices(pb_IntArrayUtility.ExtractSharedIndices(list.ToArray()));
			pb.SetColors(list2.ToArray());
			return true;
		}
	}
}
