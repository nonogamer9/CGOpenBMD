using TriangleNet.Geometry;

namespace TriangleNet.Data
{
	public class Triangle : ITriangle
	{
		internal int hash;

		internal int id;

		internal Otri[] neighbors;

		internal Vertex[] vertices;

		internal Osub[] subsegs;

		internal int region;

		internal double area;

		internal bool infected;

		public int ID
		{
			get
			{
				return id;
			}
		}

		public int P0
		{
			get
			{
				if (!(vertices[0] == null))
				{
					return vertices[0].id;
				}
				return -1;
			}
		}

		public int P1
		{
			get
			{
				if (!(vertices[1] == null))
				{
					return vertices[1].id;
				}
				return -1;
			}
		}

		public int P2
		{
			get
			{
				if (!(vertices[2] == null))
				{
					return vertices[2].id;
				}
				return -1;
			}
		}

		public bool SupportsNeighbors
		{
			get
			{
				return true;
			}
		}

		public int N0
		{
			get
			{
				return neighbors[0].triangle.id;
			}
		}

		public int N1
		{
			get
			{
				return neighbors[1].triangle.id;
			}
		}

		public int N2
		{
			get
			{
				return neighbors[2].triangle.id;
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
		}

		public Triangle()
		{
			neighbors = new Otri[3];
			neighbors[0].triangle = Mesh.dummytri;
			neighbors[1].triangle = Mesh.dummytri;
			neighbors[2].triangle = Mesh.dummytri;
			vertices = new Vertex[3];
			subsegs = new Osub[3];
			subsegs[0].seg = Mesh.dummysub;
			subsegs[1].seg = Mesh.dummysub;
			subsegs[2].seg = Mesh.dummysub;
		}

		public Vertex GetVertex(int index)
		{
			return vertices[index];
		}

		public ITriangle GetNeighbor(int index)
		{
			if (neighbors[index].triangle != Mesh.dummytri)
			{
				return neighbors[index].triangle;
			}
			return null;
		}

		public ISegment GetSegment(int index)
		{
			if (subsegs[index].seg != Mesh.dummysub)
			{
				return subsegs[index].seg;
			}
			return null;
		}

		public override int GetHashCode()
		{
			return hash;
		}

		public override string ToString()
		{
			return string.Format("TID {0}", hash);
		}
	}
}
