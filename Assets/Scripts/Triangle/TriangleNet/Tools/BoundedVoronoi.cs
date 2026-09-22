using System;
using System.Collections.Generic;
using TriangleNet.Data;
using TriangleNet.Geometry;

namespace TriangleNet.Tools
{
	public class BoundedVoronoi : IVoronoi
	{
		private Mesh mesh;

		private Point[] points;

		private List<VoronoiRegion> regions;

		private List<Point> segPoints;

		private int segIndex;

		private Dictionary<int, Segment> subsegMap;

		private bool includeBoundary = true;

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
				return regions;
			}
		}

		public BoundedVoronoi(Mesh mesh)
			: this(mesh, true)
		{
		}

		public BoundedVoronoi(Mesh mesh, bool includeBoundary)
		{
			this.mesh = mesh;
			this.includeBoundary = includeBoundary;
			Generate();
		}

		private void Generate()
		{
			mesh.Renumber();
			mesh.MakeVertexMap();
			regions = new List<VoronoiRegion>(mesh.vertices.Count);
			points = new Point[mesh.triangles.Count];
			segPoints = new List<Point>(mesh.subsegs.Count * 4);
			ComputeCircumCenters();
			TagBlindTriangles();
			foreach (Vertex value in mesh.vertices.Values)
			{
				if (value.type == VertexType.FreeVertex || value.Boundary == 0)
				{
					ConstructCell(value);
				}
				else if (includeBoundary)
				{
					ConstructBoundaryCell(value);
				}
			}
			int num = points.Length;
			Array.Resize(ref points, num + segPoints.Count);
			for (int i = 0; i < segPoints.Count; i++)
			{
				points[num + i] = segPoints[i];
			}
			segPoints.Clear();
			segPoints = null;
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
			}
		}

		private void TagBlindTriangles()
		{
			int num = 0;
			subsegMap = new Dictionary<int, Segment>();
			Otri ot = default(Otri);
			Otri o = default(Otri);
			Osub seg = default(Osub);
			Osub os = default(Osub);
			foreach (Triangle value in mesh.triangles.Values)
			{
				value.infected = false;
			}
			foreach (Segment value2 in mesh.subsegs.Values)
			{
				Stack<Triangle> stack = new Stack<Triangle>();
				seg.seg = value2;
				seg.orient = 0;
				seg.TriPivot(ref ot);
				if (ot.triangle != Mesh.dummytri && !ot.triangle.infected)
				{
					stack.Push(ot.triangle);
				}
				seg.SymSelf();
				seg.TriPivot(ref ot);
				if (ot.triangle != Mesh.dummytri && !ot.triangle.infected)
				{
					stack.Push(ot.triangle);
				}
				while (stack.Count > 0)
				{
					ot.triangle = stack.Pop();
					ot.orient = 0;
					if (!TriangleIsBlinded(ref ot, ref seg))
					{
						continue;
					}
					ot.triangle.infected = true;
					num++;
					subsegMap.Add(ot.triangle.hash, seg.seg);
					ot.orient = 0;
					while (ot.orient < 3)
					{
						ot.Sym(ref o);
						o.SegPivot(ref os);
						if (o.triangle != Mesh.dummytri && !o.triangle.infected && os.seg == Mesh.dummysub)
						{
							stack.Push(o.triangle);
						}
						ot.orient++;
					}
				}
			}
			num = 0;
		}

		private bool TriangleIsBlinded(ref Otri tri, ref Osub seg)
		{
			Vertex p = tri.Org();
			Vertex p2 = tri.Dest();
			Vertex p3 = tri.Apex();
			Vertex p4 = seg.Org();
			Vertex p5 = seg.Dest();
			Point p6 = points[tri.triangle.id];
			Point p7;
			if (SegmentsIntersect(p4, p5, p6, p, out p7, true))
			{
				return true;
			}
			if (SegmentsIntersect(p4, p5, p6, p2, out p7, true))
			{
				return true;
			}
			if (SegmentsIntersect(p4, p5, p6, p3, out p7, true))
			{
				return true;
			}
			return false;
		}

		private void ConstructCell(Vertex vertex)
		{
			VoronoiRegion voronoiRegion = new VoronoiRegion(vertex);
			regions.Add(voronoiRegion);
			Otri o = default(Otri);
			Otri o2 = default(Otri);
			Otri o3 = default(Otri);
			Osub osub = default(Osub);
			Osub o4 = default(Osub);
			int count = mesh.triangles.Count;
			List<Point> list = new List<Point>();
			vertex.tri.Copy(ref o2);
			if (o2.Org() != vertex)
			{
				throw new Exception("ConstructBvdCell: inconsistent topology.");
			}
			o2.Copy(ref o);
			o2.Onext(ref o3);
			do
			{
				Point point = points[o.triangle.id];
				Point p = points[o3.triangle.id];
				Point p2;
				if (!o.triangle.infected)
				{
					list.Add(point);
					if (o3.triangle.infected)
					{
						o4.seg = subsegMap[o3.triangle.hash];
						if (SegmentsIntersect(o4.SegOrg(), o4.SegDest(), point, p, out p2, true))
						{
							p2.id = count + segIndex++;
							segPoints.Add(p2);
							list.Add(p2);
						}
					}
				}
				else
				{
					osub.seg = subsegMap[o.triangle.hash];
					if (!o3.triangle.infected)
					{
						if (SegmentsIntersect(osub.SegOrg(), osub.SegDest(), point, p, out p2, true))
						{
							p2.id = count + segIndex++;
							segPoints.Add(p2);
							list.Add(p2);
						}
					}
					else
					{
						o4.seg = subsegMap[o3.triangle.hash];
						if (!osub.Equal(o4))
						{
							if (SegmentsIntersect(osub.SegOrg(), osub.SegDest(), point, p, out p2, true))
							{
								p2.id = count + segIndex++;
								segPoints.Add(p2);
								list.Add(p2);
							}
							if (SegmentsIntersect(o4.SegOrg(), o4.SegDest(), point, p, out p2, true))
							{
								p2.id = count + segIndex++;
								segPoints.Add(p2);
								list.Add(p2);
							}
						}
					}
				}
				o3.Copy(ref o);
				o3.OnextSelf();
			}
			while (!o.Equal(o2));
			voronoiRegion.Add(list);
		}

		private void ConstructBoundaryCell(Vertex vertex)
		{
			VoronoiRegion voronoiRegion = new VoronoiRegion(vertex);
			regions.Add(voronoiRegion);
			Otri o = default(Otri);
			Otri o2 = default(Otri);
			Otri o3 = default(Otri);
			Otri o4 = default(Otri);
			Osub osub = default(Osub);
			Osub o5 = default(Osub);
			int count = mesh.triangles.Count;
			List<Point> list = new List<Point>();
			vertex.tri.Copy(ref o2);
			if (o2.Org() != vertex)
			{
				throw new Exception("ConstructBoundaryBvdCell: inconsistent topology.");
			}
			o2.Copy(ref o);
			o2.Onext(ref o3);
			o2.Oprev(ref o4);
			if (o4.triangle != Mesh.dummytri)
			{
				while (o4.triangle != Mesh.dummytri && !o4.Equal(o2))
				{
					o4.Copy(ref o);
					o4.OprevSelf();
				}
				o.Copy(ref o2);
				o.Onext(ref o3);
			}
			Point point;
			if (o4.triangle == Mesh.dummytri)
			{
				point = new Point(vertex.x, vertex.y);
				point.id = count + segIndex++;
				segPoints.Add(point);
				list.Add(point);
			}
			Vertex vertex2 = o.Org();
			Vertex vertex3 = o.Dest();
			point = new Point((vertex2.X + vertex3.X) / 2.0, (vertex2.Y + vertex3.Y) / 2.0);
			point.id = count + segIndex++;
			segPoints.Add(point);
			list.Add(point);
			do
			{
				Point point2 = points[o.triangle.id];
				if (o3.triangle == Mesh.dummytri)
				{
					if (!o.triangle.infected)
					{
						list.Add(point2);
					}
					vertex2 = o.Org();
					Vertex vertex4 = o.Apex();
					point = new Point((vertex2.X + vertex4.X) / 2.0, (vertex2.Y + vertex4.Y) / 2.0);
					point.id = count + segIndex++;
					segPoints.Add(point);
					list.Add(point);
					break;
				}
				Point p = points[o3.triangle.id];
				if (!o.triangle.infected)
				{
					list.Add(point2);
					if (o3.triangle.infected)
					{
						o5.seg = subsegMap[o3.triangle.hash];
						if (SegmentsIntersect(o5.SegOrg(), o5.SegDest(), point2, p, out point, true))
						{
							point.id = count + segIndex++;
							segPoints.Add(point);
							list.Add(point);
						}
					}
				}
				else
				{
					osub.seg = subsegMap[o.triangle.hash];
					Vertex p2 = osub.SegOrg();
					Vertex p3 = osub.SegDest();
					if (!o3.triangle.infected)
					{
						vertex3 = o.Dest();
						Vertex vertex4 = o.Apex();
						Point p4 = new Point((vertex3.X + vertex4.X) / 2.0, (vertex3.Y + vertex4.Y) / 2.0);
						if (SegmentsIntersect(p2, p3, p4, point2, out point, false))
						{
							point.id = count + segIndex++;
							segPoints.Add(point);
							list.Add(point);
						}
						if (SegmentsIntersect(p2, p3, point2, p, out point, true))
						{
							point.id = count + segIndex++;
							segPoints.Add(point);
							list.Add(point);
						}
					}
					else
					{
						o5.seg = subsegMap[o3.triangle.hash];
						if (!osub.Equal(o5))
						{
							if (SegmentsIntersect(p2, p3, point2, p, out point, true))
							{
								point.id = count + segIndex++;
								segPoints.Add(point);
								list.Add(point);
							}
							if (SegmentsIntersect(o5.SegOrg(), o5.SegDest(), point2, p, out point, true))
							{
								point.id = count + segIndex++;
								segPoints.Add(point);
								list.Add(point);
							}
						}
						else
						{
							Point p5 = new Point((vertex2.X + vertex3.X) / 2.0, (vertex2.Y + vertex3.Y) / 2.0);
							if (SegmentsIntersect(p2, p3, p5, p, out point, false))
							{
								point.id = count + segIndex++;
								segPoints.Add(point);
								list.Add(point);
							}
						}
					}
				}
				o3.Copy(ref o);
				o3.OnextSelf();
			}
			while (!o.Equal(o2));
			voronoiRegion.Add(list);
		}

		private bool SegmentsIntersect(Point p1, Point p2, Point p3, Point p4, out Point p, bool strictIntersect)
		{
			p = null;
			double x = p1.X;
			double y = p1.Y;
			double x2 = p2.X;
			double y2 = p2.Y;
			double x3 = p3.X;
			double y3 = p3.Y;
			double x4 = p4.X;
			double y4 = p4.Y;
			if ((x == x2 && y == y2) || (x3 == x4 && y3 == y4))
			{
				return false;
			}
			if ((x == x3 && y == y3) || (x2 == x3 && y2 == y3) || (x == x4 && y == y4) || (x2 == x4 && y2 == y4))
			{
				return false;
			}
			x2 -= x;
			y2 -= y;
			x3 -= x;
			y3 -= y;
			x4 -= x;
			y4 -= y;
			double num = Math.Sqrt(x2 * x2 + y2 * y2);
			double num2 = x2 / num;
			double num3 = y2 / num;
			double num4 = x3 * num2 + y3 * num3;
			y3 = y3 * num2 - x3 * num3;
			x3 = num4;
			double num5 = x4 * num2 + y4 * num3;
			y4 = y4 * num2 - x4 * num3;
			x4 = num5;
			if ((y3 < 0.0 && y4 < 0.0) || ((y3 >= 0.0 && y4 >= 0.0) & strictIntersect))
			{
				return false;
			}
			double num6 = x4 + (x3 - x4) * y4 / (y4 - y3);
			if (num6 < 0.0 || ((num6 > num) & strictIntersect))
			{
				return false;
			}
			p = new Point(x + num6 * num2, y + num6 * num3);
			return true;
		}
	}
}
