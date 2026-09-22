using System;
using System.Collections.Generic;
using TriangleNet.Data;
using TriangleNet.Geometry;

namespace TriangleNet.Tools
{
	public class Voronoi : IVoronoi
	{
		private Mesh mesh;

		private Point[] points;

		private Dictionary<int, VoronoiRegion> regions;

		private Dictionary<int, Point> rayPoints;

		private int rayIndex;

		private BoundingBox bounds;

		public Point[] Points
		{
			get
			{
				return points;
			}
		}

		public ICollection<VoronoiRegion> Regions
		{
			get
			{
				return regions.Values;
			}
		}

		public Voronoi(Mesh mesh)
		{
			this.mesh = mesh;
			Generate();
		}

		private void Generate()
		{
			mesh.Renumber();
			mesh.MakeVertexMap();
			points = new Point[mesh.triangles.Count + mesh.hullsize];
			regions = new Dictionary<int, VoronoiRegion>(mesh.vertices.Count);
			rayPoints = new Dictionary<int, Point>();
			rayIndex = 0;
			bounds = new BoundingBox();
			ComputeCircumCenters();
			foreach (Vertex value in mesh.vertices.Values)
			{
				regions.Add(value.id, new VoronoiRegion(value));
			}
			foreach (VoronoiRegion value2 in regions.Values)
			{
				ConstructVoronoiRegion(value2);
			}
		}

		private void ComputeCircumCenters()
		{
			Otri otri = default(Otri);
			double xi = 0.0;
			double eta = 0.0;
			foreach (Triangle value in mesh.triangles.Values)
			{
				Triangle triangle = (otri.triangle = value);
				Point point = Primitives.FindCircumcenter(otri.Org(), otri.Dest(), otri.Apex(), ref xi, ref eta);
				point.id = triangle.id;
				points[triangle.id] = point;
				bounds.Expand(point.x, point.y);
			}
			double num = Math.Max(bounds.Width, bounds.Height);
			bounds.Resize(num, num);
		}

		private void ConstructVoronoiRegion(VoronoiRegion region)
		{
			Vertex obj = region.Generator as Vertex;
			List<Point> list = new List<Point>();
			Otri o = default(Otri);
			Otri o2 = default(Otri);
			Otri o3 = default(Otri);
			Otri o4 = default(Otri);
			Osub os = default(Osub);
			obj.tri.Copy(ref o2);
			o2.Copy(ref o);
			o2.Onext(ref o3);
			if (o3.triangle == Mesh.dummytri)
			{
				o2.Oprev(ref o4);
				if (o4.triangle != Mesh.dummytri)
				{
					o2.Copy(ref o3);
					o2.OprevSelf();
					o2.Copy(ref o);
				}
			}
			while (o3.triangle != Mesh.dummytri)
			{
				list.Add(points[o.triangle.id]);
				region.AddNeighbor(o.triangle.id, regions[o.Apex().id]);
				if (o3.Equal(o2))
				{
					region.Add(list);
					return;
				}
				o3.Copy(ref o);
				o3.OnextSelf();
			}
			region.Bounded = false;
			int count = mesh.triangles.Count;
			o.Lprev(ref o3);
			o3.SegPivot(ref os);
			int hash = os.seg.hash;
			list.Add(points[o.triangle.id]);
			region.AddNeighbor(o.triangle.id, regions[o.Apex().id]);
			Point value;
			if (!rayPoints.TryGetValue(hash, out value))
			{
				Vertex vertex = o.Org();
				Vertex vertex2 = o.Apex();
				BoxRayIntersection(points[o.triangle.id], vertex.y - vertex2.y, vertex2.x - vertex.x, out value);
				value.id = count + rayIndex;
				points[count + rayIndex] = value;
				rayIndex++;
				rayPoints.Add(hash, value);
			}
			list.Add(value);
			list.Reverse();
			o2.Copy(ref o);
			o.Oprev(ref o4);
			while (o4.triangle != Mesh.dummytri)
			{
				list.Add(points[o4.triangle.id]);
				region.AddNeighbor(o4.triangle.id, regions[o4.Apex().id]);
				o4.Copy(ref o);
				o4.OprevSelf();
			}
			o.SegPivot(ref os);
			hash = os.seg.hash;
			if (!rayPoints.TryGetValue(hash, out value))
			{
				Vertex vertex = o.Org();
				Vertex vertex3 = o.Dest();
				BoxRayIntersection(points[o.triangle.id], vertex3.y - vertex.y, vertex.x - vertex3.x, out value);
				value.id = count + rayIndex;
				rayPoints.Add(hash, value);
				points[count + rayIndex] = value;
				rayIndex++;
			}
			list.Add(value);
			region.AddNeighbor(value.id, regions[o.Dest().id]);
			list.Reverse();
			region.Add(list);
		}

		private bool BoxRayIntersection(Point pt, double dx, double dy, out Point intersect)
		{
			double x = pt.X;
			double y = pt.Y;
			double minX = bounds.MinX;
			double maxX = bounds.MaxX;
			double minY = bounds.MinY;
			double maxY = bounds.MaxY;
			if (x < minX || x > maxX || y < minY || y > maxY)
			{
				intersect = null;
				return false;
			}
			double num;
			double x2;
			double y2;
			if (dx < 0.0)
			{
				num = (minX - x) / dx;
				x2 = minX;
				y2 = y + num * dy;
			}
			else if (dx > 0.0)
			{
				num = (maxX - x) / dx;
				x2 = maxX;
				y2 = y + num * dy;
			}
			else
			{
				num = double.MaxValue;
				x2 = (y2 = 0.0);
			}
			double num2;
			double x3;
			double y3;
			if (dy < 0.0)
			{
				num2 = (minY - y) / dy;
				x3 = x + num2 * dx;
				y3 = minY;
			}
			else if (dy > 0.0)
			{
				num2 = (maxY - y) / dy;
				x3 = x + num2 * dx;
				y3 = maxY;
			}
			else
			{
				num2 = double.MaxValue;
				x3 = (y3 = 0.0);
			}
			if (num < num2)
			{
				intersect = new Point(x2, y2);
			}
			else
			{
				intersect = new Point(x3, y3);
			}
			return true;
		}
	}
}
