using System.Collections.Generic;
using System.Linq;
using TriangleNet.Geometry;

namespace TriangleNet.Tools
{
	public class QuadTree
	{
		private QuadNode root;

		internal ITriangle[] triangles;

		internal int sizeBound;

		internal int maxDepth;

		public QuadTree(Mesh mesh, int maxDepth, int sizeBound)
		{
			this.maxDepth = maxDepth;
			this.sizeBound = sizeBound;
			triangles = mesh.Triangles.ToArray();
			int num = 0;
			root = new QuadNode(mesh.Bounds, this, true);
			root.CreateSubRegion(++num);
		}

		public QuadTree(Mesh mesh)
			: this(mesh, 10, 10)
		{
		}

		public ITriangle Query(double x, double y)
		{
			Point point = new Point(x, y);
			List<int> list = root.FindTriangles(point);
			List<ITriangle> list2 = new List<ITriangle>();
			foreach (int item in list)
			{
				ITriangle triangle = triangles[item];
				if (IsPointInTriangle(point, triangle.GetVertex(0), triangle.GetVertex(1), triangle.GetVertex(2)))
				{
					list2.Add(triangle);
					break;
				}
			}
			return list2.FirstOrDefault();
		}

		internal static bool IsPointInTriangle(Point p, Point t0, Point t1, Point t2)
		{
			Point point = new Point(t1.X - t0.X, t1.Y - t0.Y);
			Point point2 = new Point(t2.X - t0.X, t2.Y - t0.Y);
			Point p2 = new Point(p.X - t0.X, p.Y - t0.Y);
			Point q = new Point(0.0 - point.Y, point.X);
			Point q2 = new Point(0.0 - point2.Y, point2.X);
			double num = DotProduct(p2, q2) / DotProduct(point, q2);
			double num2 = DotProduct(p2, q) / DotProduct(point2, q);
			if (num >= 0.0 && num2 >= 0.0 && num + num2 <= 1.0)
			{
				return true;
			}
			return false;
		}

		internal static double DotProduct(Point p, Point q)
		{
			return p.X * q.X + p.Y * q.Y;
		}
	}
}
