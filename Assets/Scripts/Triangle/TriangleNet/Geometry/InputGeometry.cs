using System;
using System.Collections.Generic;
using System.Linq;
using TriangleNet.Data;

namespace TriangleNet.Geometry
{
	public class InputGeometry
	{
		internal List<Vertex> points;

		internal List<Edge> segments;

		internal List<Point> holes;

		internal List<RegionPointer> regions;

		private BoundingBox bounds;

		private int pointAttributes = -1;

		public BoundingBox Bounds
		{
			get
			{
				return bounds;
			}
		}

		public bool HasSegments
		{
			get
			{
				return segments.Count > 0;
			}
		}

		public int Count
		{
			get
			{
				return points.Count;
			}
		}

		public IEnumerable<Point> Points
		{
			get
			{
				return ((IEnumerable<Vertex>)points).Select((Func<Vertex, Point>)((Vertex v) => v));
			}
		}

		public ICollection<Edge> Segments
		{
			get
			{
				return segments;
			}
		}

		public ICollection<Point> Holes
		{
			get
			{
				return holes;
			}
		}

		public ICollection<RegionPointer> Regions
		{
			get
			{
				return regions;
			}
		}

		public InputGeometry()
			: this(3)
		{
		}

		public InputGeometry(int capacity)
		{
			points = new List<Vertex>(capacity);
			segments = new List<Edge>();
			holes = new List<Point>();
			regions = new List<RegionPointer>();
			bounds = new BoundingBox();
			pointAttributes = -1;
		}

		public void Clear()
		{
			points.Clear();
			segments.Clear();
			holes.Clear();
			regions.Clear();
			pointAttributes = -1;
		}

		public void AddPoint(double x, double y)
		{
			AddPoint(x, y, 0);
		}

		public void AddPoint(double x, double y, int boundary)
		{
			points.Add(new Vertex(x, y, boundary));
			bounds.Expand(x, y);
		}

		public void AddPoint(double x, double y, int boundary, double attribute)
		{
			AddPoint(x, y, 0, new double[1] { attribute });
		}

		public void AddPoint(double x, double y, int boundary, double[] attribs)
		{
			if (pointAttributes < 0)
			{
				pointAttributes = ((attribs != null) ? attribs.Length : 0);
			}
			else
			{
				if (attribs == null && pointAttributes > 0)
				{
					throw new ArgumentException("Inconsitent use of point attributes.");
				}
				if (attribs != null && pointAttributes != attribs.Length)
				{
					throw new ArgumentException("Inconsitent use of point attributes.");
				}
			}
			points.Add(new Vertex(x, y, boundary)
			{
				attributes = attribs
			});
			bounds.Expand(x, y);
		}

		public void AddPoint(Vertex v)
		{
			double[] attributes = v.attributes;
			if (pointAttributes < 0)
			{
				pointAttributes = ((attributes != null) ? attributes.Length : 0);
			}
			else
			{
				if (attributes == null && pointAttributes > 0)
				{
					throw new ArgumentException("Inconsitent use of point attributes.");
				}
				if (attributes != null && pointAttributes != attributes.Length)
				{
					throw new ArgumentException("Inconsitent use of point attributes.");
				}
			}
			points.Add(v);
			bounds.Expand(v.x, v.y);
		}

		public void AddHole(double x, double y)
		{
			holes.Add(new Point(x, y));
		}

		public void AddRegion(double x, double y, int id)
		{
			regions.Add(new RegionPointer(x, y, id));
		}

		public void AddSegment(int p0, int p1)
		{
			AddSegment(p0, p1, 0);
		}

		public void AddSegment(int p0, int p1, int boundary)
		{
			if (p0 == p1 || p0 < 0 || p1 < 0)
			{
				throw new NotSupportedException("Invalid endpoints.");
			}
			segments.Add(new Edge(p0, p1, boundary));
		}
	}
}
