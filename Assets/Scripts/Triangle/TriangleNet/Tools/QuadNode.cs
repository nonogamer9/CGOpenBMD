using System.Collections.Generic;
using TriangleNet.Geometry;

namespace TriangleNet.Tools
{
	internal class QuadNode
	{
		private const int SW = 0;

		private const int SE = 1;

		private const int NW = 2;

		private const int NE = 3;

		private const double EPS = 1E-06;

		private static readonly byte[] BITVECTOR = new byte[4] { 1, 2, 4, 8 };

		private BoundingBox bounds;

		private Point pivot;

		private QuadTree tree;

		private QuadNode[] regions;

		private List<int> triangles;

		private byte bitRegions;

		public QuadNode(BoundingBox box, QuadTree tree)
			: this(box, tree, false)
		{
		}

		public QuadNode(BoundingBox box, QuadTree tree, bool init)
		{
			this.tree = tree;
			bounds = new BoundingBox(box.MinX, box.MinY, box.MaxX, box.MaxY);
			pivot = new Point((box.MinX + box.MaxX) / 2.0, (box.MinY + box.MaxY) / 2.0);
			bitRegions = 0;
			regions = new QuadNode[4];
			triangles = new List<int>();
			if (init)
			{
				int num = tree.triangles.Length;
				triangles.Capacity = num;
				for (int i = 0; i < num; i++)
				{
					triangles.Add(i);
				}
			}
		}

		public List<int> FindTriangles(Point searchPoint)
		{
			int num = FindRegion(searchPoint);
			if (regions[num] == null)
			{
				return triangles;
			}
			return regions[num].FindTriangles(searchPoint);
		}

		public void CreateSubRegion(int currentDepth)
		{
			BoundingBox box = new BoundingBox(bounds.MinX, bounds.MinY, pivot.X, pivot.Y);
			regions[0] = new QuadNode(box, tree);
			box = new BoundingBox(pivot.X, bounds.MinY, bounds.MaxX, pivot.Y);
			regions[1] = new QuadNode(box, tree);
			box = new BoundingBox(bounds.MinX, pivot.Y, pivot.X, bounds.MaxY);
			regions[2] = new QuadNode(box, tree);
			box = new BoundingBox(pivot.X, pivot.Y, bounds.MaxX, bounds.MaxY);
			regions[3] = new QuadNode(box, tree);
			Point[] array = new Point[3];
			foreach (int triangle2 in triangles)
			{
				ITriangle triangle = tree.triangles[triangle2];
				array[0] = triangle.GetVertex(0);
				array[1] = triangle.GetVertex(1);
				array[2] = triangle.GetVertex(2);
				AddTriangleToRegion(array, triangle2);
			}
			for (int i = 0; i < 4; i++)
			{
				if (regions[i].triangles.Count > tree.sizeBound && currentDepth < tree.maxDepth)
				{
					regions[i].CreateSubRegion(currentDepth + 1);
				}
			}
		}

		private void AddTriangleToRegion(Point[] triangle, int index)
		{
			bitRegions = 0;
			if (QuadTree.IsPointInTriangle(pivot, triangle[0], triangle[1], triangle[2]))
			{
				AddToRegion(index, 0);
				AddToRegion(index, 1);
				AddToRegion(index, 2);
				AddToRegion(index, 3);
				return;
			}
			FindTriangleIntersections(triangle, index);
			if (bitRegions == 0)
			{
				int num = FindRegion(triangle[0]);
				regions[num].triangles.Add(index);
			}
		}

		private void FindTriangleIntersections(Point[] triangle, int index)
		{
			int num = 2;
			int num2 = 0;
			while (num2 < 3)
			{
				double num3 = triangle[num2].X - triangle[num].X;
				double num4 = triangle[num2].Y - triangle[num].Y;
				if (num3 != 0.0)
				{
					FindIntersectionsWithX(num3, num4, triangle, index, num);
				}
				if (num4 != 0.0)
				{
					FindIntersectionsWithY(num3, num4, triangle, index, num);
				}
				num = num2++;
			}
		}

		private void FindIntersectionsWithX(double dx, double dy, Point[] triangle, int index, int k)
		{
			double num = (pivot.X - triangle[k].X) / dx;
			if (num < 1.000001 && num > -1E-06)
			{
				double num2 = triangle[k].Y + num * dy;
				if (num2 < pivot.Y && num2 >= bounds.MinY)
				{
					AddToRegion(index, 0);
					AddToRegion(index, 1);
				}
				else if (num2 <= bounds.MaxY)
				{
					AddToRegion(index, 2);
					AddToRegion(index, 3);
				}
			}
			num = (bounds.MinX - triangle[k].X) / dx;
			if (num < 1.000001 && num > -1E-06)
			{
				double num3 = triangle[k].Y + num * dy;
				if (num3 < pivot.Y && num3 >= bounds.MinY)
				{
					AddToRegion(index, 0);
				}
				else if (num3 <= bounds.MaxY)
				{
					AddToRegion(index, 2);
				}
			}
			num = (bounds.MaxX - triangle[k].X) / dx;
			if (num < 1.000001 && num > -1E-06)
			{
				double num4 = triangle[k].Y + num * dy;
				if (num4 < pivot.Y && num4 >= bounds.MinY)
				{
					AddToRegion(index, 1);
				}
				else if (num4 <= bounds.MaxY)
				{
					AddToRegion(index, 3);
				}
			}
		}

		private void FindIntersectionsWithY(double dx, double dy, Point[] triangle, int index, int k)
		{
			double num = (pivot.Y - triangle[k].Y) / dy;
			if (num < 1.000001 && num > -1E-06)
			{
				double num2 = triangle[k].X + num * dx;
				if (num2 > pivot.X && num2 <= bounds.MaxX)
				{
					AddToRegion(index, 1);
					AddToRegion(index, 3);
				}
				else if (num2 >= bounds.MinX)
				{
					AddToRegion(index, 0);
					AddToRegion(index, 2);
				}
			}
			num = (bounds.MinY - triangle[k].Y) / dy;
			if (num < 1.000001 && num > -1E-06)
			{
				double num2 = triangle[k].X + num * dx;
				if (num2 > pivot.X && num2 <= bounds.MaxX)
				{
					AddToRegion(index, 1);
				}
				else if (num2 >= bounds.MinX)
				{
					AddToRegion(index, 0);
				}
			}
			num = (bounds.MaxY - triangle[k].Y) / dy;
			if (num < 1.000001 && num > -1E-06)
			{
				double num2 = triangle[k].X + num * dx;
				if (num2 > pivot.X && num2 <= bounds.MaxX)
				{
					AddToRegion(index, 3);
				}
				else if (num2 >= bounds.MinX)
				{
					AddToRegion(index, 2);
				}
			}
		}

		private int FindRegion(Point point)
		{
			int num = 2;
			if (point.Y < pivot.Y)
			{
				num = 0;
			}
			if (point.X > pivot.X)
			{
				num++;
			}
			return num;
		}

		private void AddToRegion(int index, int region)
		{
			if ((bitRegions & BITVECTOR[region]) == 0)
			{
				regions[region].triangles.Add(index);
				bitRegions |= BITVECTOR[region];
			}
		}
	}
}
