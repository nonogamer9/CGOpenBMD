using System.Collections.Generic;
using TriangleNet.Data;
using TriangleNet.Geometry;

namespace TriangleNet.Tools
{
	public class VoronoiRegion
	{
		private int id;

		private Point generator;

		private List<Point> vertices;

		private bool bounded;

		private Dictionary<int, VoronoiRegion> neighbors;

		public int ID
		{
			get
			{
				return id;
			}
		}

		public Point Generator
		{
			get
			{
				return generator;
			}
		}

		public ICollection<Point> Vertices
		{
			get
			{
				return vertices;
			}
		}

		public bool Bounded
		{
			get
			{
				return bounded;
			}
			set
			{
				bounded = value;
			}
		}

		public VoronoiRegion(Vertex generator)
		{
			id = generator.id;
			this.generator = generator;
			vertices = new List<Point>();
			bounded = true;
			neighbors = new Dictionary<int, VoronoiRegion>();
		}

		public void Add(Point point)
		{
			vertices.Add(point);
		}

		public void Add(List<Point> points)
		{
			vertices.AddRange(points);
		}

		public VoronoiRegion GetNeighbor(Point p)
		{
			VoronoiRegion value;
			if (neighbors.TryGetValue(p.id, out value))
			{
				return value;
			}
			return null;
		}

		internal void AddNeighbor(int id, VoronoiRegion neighbor)
		{
			neighbors.Add(id, neighbor);
		}

		public override string ToString()
		{
			return string.Format("R-ID {0}", id);
		}
	}
}
