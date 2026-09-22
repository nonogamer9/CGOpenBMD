using System.Collections.Generic;
using System.Linq;
using ProBuilder2.Common;
using UnityEngine;

namespace ProBuilder2.MeshOperations
{
	public static class pb_AppendPolygon
	{
		private const int MAX_HOLE_ITERATIONS = 2048;

		public static pb_ActionResult CreatePolygon(this pb_Object pb, IList<int> indices, bool unordered, out pb_Face face)
		{
			pb_IntArray[] sharedIndices = pb.sharedIndices;
			Dictionary<int, int> dictionary = sharedIndices.ToDictionary();
			HashSet<int> commonIndices = pb_IntArrayUtility.GetCommonIndices(dictionary, indices);
			List<pb_Vertex> list = new List<pb_Vertex>(pb_Vertex.GetVertices(pb));
			List<pb_Vertex> list2 = new List<pb_Vertex>();
			foreach (int item in commonIndices)
			{
				int index = sharedIndices[item][0];
				list2.Add(new pb_Vertex(list[index]));
			}
			pb_FaceRebuildData pb_FaceRebuildData2 = FaceWithVertices(list2, unordered);
			if (pb_FaceRebuildData2 != null)
			{
				pb_FaceRebuildData2.sharedIndices = commonIndices.ToList();
				List<pb_Face> list3 = new List<pb_Face>(pb.faces);
				pb_FaceRebuildData.Apply(new pb_FaceRebuildData[1] { pb_FaceRebuildData2 }, list, list3, dictionary);
				pb.SetVertices(list);
				pb.SetFaces(list3.ToArray());
				pb.SetSharedIndices(dictionary);
				face = pb_FaceRebuildData2.face;
				return new pb_ActionResult(Status.Success, "Create Polygon");
			}
			face = null;
			return new pb_ActionResult(Status.Failure, (!unordered) ? "Points not ordered correctly" : "Too Few Unique Points Selected");
		}

		public static pb_ActionResult CreateShapeFromPolygon(this pb_PolyShape poly)
		{
			return poly.mesh.CreateShapeFromPolygon(poly.points, poly.extrude, poly.flipNormals);
		}

		public static pb_ActionResult CreateShapeFromPolygon(this pb_Object pb, IList<Vector3> points, float extrude, bool flipNormals)
		{
			if (points.Count < 3)
			{
				pb.SetVertices(new Vector3[0]);
				pb.SetFaces(new pb_Face[0]);
				pb.SetSharedIndices(new pb_IntArray[0]);
				return new pb_ActionResult(Status.NoChange, "Too Few Points");
			}
			Vector3[] array = points.ToArray();
			pb_Log.PushLogLevel(pb_LogLevel.Error);
			List<int> triangles;
			if (pb_Triangulation.TriangulateVertices(array, out triangles, false))
			{
				int[] array2 = triangles.ToArray();
				if (pb_Math.PolygonArea(array, array2) < Mathf.Epsilon)
				{
					pb.SetVertices(new Vector3[0]);
					pb.SetFaces(new pb_Face[0]);
					pb.SetSharedIndices(new pb_IntArray[0]);
					pb_Log.PopLogLevel();
					return new pb_ActionResult(Status.Failure, "Polygon Area < Epsilon");
				}
				pb.GeometryWithVerticesFaces(array, new pb_Face[1]
				{
					new pb_Face(array2)
				});
				Vector3 rhs = pb_Math.Normal(pb, pb.faces[0]);
				if (Vector3.Dot(Vector3.up, rhs) > 0f)
				{
					pb.faces[0].ReverseIndices();
				}
				pb.DuplicateAndFlip(pb.faces);
				pb.Extrude(new pb_Face[1] { pb.faces[1] }, ExtrudeMethod.IndividualFaces, extrude);
				if ((extrude < 0f && !flipNormals) || (extrude > 0f && flipNormals))
				{
					pb.ReverseWindingOrder(pb.faces);
				}
				pb_Log.PopLogLevel();
				pb.ToMesh();
				pb.Refresh();
				return new pb_ActionResult(Status.Success, "Create Polygon Shape");
			}
			pb_Log.PopLogLevel();
			return new pb_ActionResult(Status.Failure, "Failed Triangulating Points");
		}

		public static pb_FaceRebuildData FaceWithVertices(List<pb_Vertex> vertices, bool unordered = true)
		{
			List<int> triangles;
			if (pb_Triangulation.TriangulateVertices(vertices, out triangles, unordered))
			{
				pb_FaceRebuildData pb_FaceRebuildData2 = new pb_FaceRebuildData();
				pb_FaceRebuildData2.vertices = vertices;
				pb_FaceRebuildData2.face = new pb_Face(triangles.ToArray());
				return pb_FaceRebuildData2;
			}
			return null;
		}

		public static List<pb_FaceRebuildData> TentCapWithVertices(List<pb_Vertex> path)
		{
			int count = path.Count;
			pb_Vertex item = pb_Vertex.Average(path);
			List<pb_FaceRebuildData> list = new List<pb_FaceRebuildData>();
			for (int i = 0; i < count; i++)
			{
				List<pb_Vertex> list2 = new List<pb_Vertex>();
				list2.Add(path[i]);
				list2.Add(item);
				list2.Add(path[(i + 1) % count]);
				List<pb_Vertex> vertices = list2;
				pb_FaceRebuildData pb_FaceRebuildData2 = new pb_FaceRebuildData();
				pb_FaceRebuildData2.vertices = vertices;
				pb_FaceRebuildData2.face = new pb_Face(new int[3] { 0, 1, 2 });
				list.Add(pb_FaceRebuildData2);
			}
			return list;
		}

		public static List<List<pb_Edge>> FindHoles(pb_Object pb, IList<int> indices)
		{
			Dictionary<int, int> lookup = pb.sharedIndices.ToDictionary();
			HashSet<int> commonIndices = pb_IntArrayUtility.GetCommonIndices(lookup, indices);
			List<List<pb_Edge>> list = new List<List<pb_Edge>>();
			List<pb_WingedEdge> wingedEdges = pb_WingedEdge.GetWingedEdges(pb);
			foreach (List<pb_WingedEdge> item in FindHoles(wingedEdges, commonIndices))
			{
				list.Add(item.Select((pb_WingedEdge x) => x.edge.local).ToList());
			}
			return list;
		}

		public static List<List<pb_WingedEdge>> FindHoles(List<pb_WingedEdge> wings, HashSet<int> common)
		{
			HashSet<pb_WingedEdge> hashSet = new HashSet<pb_WingedEdge>();
			List<List<pb_WingedEdge>> list = new List<List<pb_WingedEdge>>();
			for (int i = 0; i < wings.Count; i++)
			{
				pb_WingedEdge pb_WingedEdge2 = wings[i];
				if (pb_WingedEdge2.opposite != null || hashSet.Contains(pb_WingedEdge2) || (!common.Contains(pb_WingedEdge2.edge.common.x) && !common.Contains(pb_WingedEdge2.edge.common.y)))
				{
					continue;
				}
				List<pb_WingedEdge> list2 = new List<pb_WingedEdge>();
				pb_WingedEdge pb_WingedEdge3 = pb_WingedEdge2;
				int num = pb_WingedEdge3.edge.common.x;
				int num2 = 0;
				while (pb_WingedEdge3 != null && num2++ < 2048)
				{
					hashSet.Add(pb_WingedEdge3);
					list2.Add(pb_WingedEdge3);
					num = ((pb_WingedEdge3.edge.common.x != num) ? pb_WingedEdge3.edge.common.x : pb_WingedEdge3.edge.common.y);
					pb_WingedEdge3 = FindNextEdgeInHole(pb_WingedEdge3, num);
					if (pb_WingedEdge3 == pb_WingedEdge2)
					{
						break;
					}
				}
				List<pb_Tuple<int, int>> list3 = new List<pb_Tuple<int, int>>();
				for (int j = 0; j < list2.Count; j++)
				{
					pb_WingedEdge pb_WingedEdge4 = list2[j];
					for (int num3 = j - 1; num3 > -1; num3--)
					{
						if (pb_WingedEdge4.edge.common.y == list2[num3].edge.common.x)
						{
							list3.Add(new pb_Tuple<int, int>(num3, j));
							break;
						}
					}
				}
				int count = list3.Count;
				list3.Sort((pb_Tuple<int, int> x, pb_Tuple<int, int> y) => x.Item1.CompareTo(y.Item1));
				int[] array = new int[count];
				for (int num4 = count - 1; num4 > -1; num4--)
				{
					int item = list3[num4].Item1;
					int num5 = list3[num4].Item2 - array[num4];
					int num6 = num5 - item + 1;
					List<pb_WingedEdge> range = list2.GetRange(item, num6);
					list2.RemoveRange(item, num6);
					for (int num7 = num4 - 1; num7 > -1; num7--)
					{
						if (list3[num7].Item2 > list3[num4].Item2)
						{
							array[num7] += num6;
						}
					}
					if (count < 2 || range.Any((pb_WingedEdge w) => common.Contains(w.edge.common.x)) || range.Any((pb_WingedEdge w) => common.Contains(w.edge.common.y)))
					{
						list.Add(range);
					}
				}
			}
			return list;
		}

		private static pb_WingedEdge FindNextEdgeInHole(pb_WingedEdge wing, int common)
		{
			pb_WingedEdge adjacentEdgeWithCommonIndex = wing.GetAdjacentEdgeWithCommonIndex(common);
			int num = 0;
			while (adjacentEdgeWithCommonIndex != null && adjacentEdgeWithCommonIndex != wing && num++ < 2048)
			{
				if (adjacentEdgeWithCommonIndex.opposite == null)
				{
					return adjacentEdgeWithCommonIndex;
				}
				adjacentEdgeWithCommonIndex = adjacentEdgeWithCommonIndex.opposite.GetAdjacentEdgeWithCommonIndex(common);
			}
			return null;
		}
	}
}
