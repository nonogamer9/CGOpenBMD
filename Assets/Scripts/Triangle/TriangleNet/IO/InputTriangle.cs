using TriangleNet.Data;
using TriangleNet.Geometry;

namespace TriangleNet.IO
{
	public class InputTriangle : ITriangle
	{
		internal int[] vertices;

		internal int region;

		internal double area;

		public int ID
		{
			get
			{
				return 0;
			}
		}

		public int P0
		{
			get
			{
				return vertices[0];
			}
		}

		public int P1
		{
			get
			{
				return vertices[1];
			}
		}

		public int P2
		{
			get
			{
				return vertices[2];
			}
		}

		public bool SupportsNeighbors
		{
			get
			{
				return false;
			}
		}

		public int N0
		{
			get
			{
				return -1;
			}
		}

		public int N1
		{
			get
			{
				return -1;
			}
		}

		public int N2
		{
			get
			{
				return -1;
			}
		}

		public double Area
		{
			get
			{
				return area;
			}
			set
			{
				area = value;
			}
		}

		public int Region
		{
			get
			{
				return region;
			}
			set
			{
				region = value;
			}
		}

		public InputTriangle(int p0, int p1, int p2)
		{
			vertices = new int[3] { p0, p1, p2 };
		}

		public Vertex GetVertex(int index)
		{
			return null;
		}

		public ITriangle GetNeighbor(int index)
		{
			return null;
		}

		public ISegment GetSegment(int index)
		{
			return null;
		}
	}
}
