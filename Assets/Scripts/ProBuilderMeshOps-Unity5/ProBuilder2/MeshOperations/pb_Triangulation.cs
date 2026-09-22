using System.Collections.Generic;
using System.Linq;
using Poly2Tri;
using ProBuilder2.Common;
using UnityEngine;

namespace ProBuilder2.MeshOperations
{
	public static class pb_Triangulation
	{
		public static bool SortAndTriangulate(IList<Vector2> points, out List<int> indices, bool convex = false)
		{
			IList<Vector2> list = pb_Projection.Sort(points);
			Dictionary<int, int> dictionary = new Dictionary<int, int>();
			for (int i = 0; i < list.Count; i++)
			{
				dictionary.Add(i, points.IndexOf(list[i]));
			}
			if (!Triangulate(list, out indices, convex))
			{
				return false;
			}
			for (int j = 0; j < indices.Count; j++)
			{
				indices[j] = dictionary[indices[j]];
			}
			return true;
		}

		public static bool TriangulateVertices(IList<pb_Vertex> vertices, out List<int> triangles, bool unordered = true, bool convex = false)
		{
			Vector3[] array = new Vector3[vertices.Count];
			for (int i = 0; i < vertices.Count; i++)
			{
				array[i] = vertices[i].position;
			}
			return TriangulateVertices(array, out triangles, unordered, convex);
		}

		public static bool TriangulateVertices(Vector3[] vertices, out List<int> triangles, bool unordered = true, bool convex = false)
		{
			triangles = null;
			int num = ((vertices != null) ? vertices.Length : 0);
			if (num < 3)
			{
				return false;
			}
			if (num == 3)
			{
				triangles = new List<int> { 0, 1, 2 };
				return true;
			}
			Vector3 normal = pb_Projection.FindBestPlane(vertices).normal;
			Vector2[] points = pb_Projection.PlanarProject(vertices, normal);
			if (unordered)
			{
				return SortAndTriangulate(points, out triangles, convex);
			}
			return Triangulate(points, out triangles, convex);
		}

		public static bool Triangulate(IList<Vector2> points, out List<int> indices, bool convex = false)
		{
			indices = new List<int>();
			int index = 0;
			object obj;
			if (convex)
			{
				Triangulatable triangulatable = new PointSet(points.Select((Vector2 x) => new TriangulationPoint(x.x, x.y, index++)).ToList());
				obj = triangulatable;
			}
			else
			{
				obj = new Polygon(points.Select((Vector2 x) => new PolygonPoint(x.x, x.y, index++)));
			}
			Triangulatable triangulatable2 = (Triangulatable)obj;
			P2T.Triangulate(TriangulationAlgorithm.DTSweep, triangulatable2);
			foreach (DelaunayTriangle triangle in triangulatable2.Triangles)
			{
				if (triangle.Points[0].Index < 0 || triangle.Points[1].Index < 0 || triangle.Points[2].Index < 0)
				{
					pb_Log.Warning("Triangulation failed - additional vertices were inserted.");
					return false;
				}
				indices.Add(triangle.Points[0].Index);
				indices.Add(triangle.Points[1].Index);
				indices.Add(triangle.Points[2].Index);
			}
			WindingOrder windingOrder = pbTriangleOps.GetWindingOrder(points);
			if (pbTriangleOps.GetWindingOrder(new Vector2[3]
			{
				points[indices[0]],
				points[indices[1]],
				points[indices[2]]
			}) != windingOrder)
			{
				indices.Reverse();
			}
			return true;
		}
	}
}
