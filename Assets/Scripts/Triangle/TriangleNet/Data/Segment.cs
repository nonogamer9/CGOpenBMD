using TriangleNet.Geometry;

namespace TriangleNet.Data
{
	public class Segment : ISegment
	{
		internal int hash;

		internal Osub[] subsegs;

		internal Vertex[] vertices;

		internal Otri[] triangles;

		internal int boundary;

		public int P0
		{
			get
			{
				return vertices[0].id;
			}
		}

		public int P1
		{
			get
			{
				return vertices[1].id;
			}
		}

		public int Boundary
		{
			get
			{
				return boundary;
			}
		}

		public Segment()
		{
			subsegs = new Osub[2];
			subsegs[0].seg = Mesh.dummysub;
			subsegs[1].seg = Mesh.dummysub;
			vertices = new Vertex[4];
			triangles = new Otri[2];
			triangles[0].triangle = Mesh.dummytri;
			triangles[1].triangle = Mesh.dummytri;
			boundary = 0;
		}

		public Vertex GetVertex(int index)
		{
			return vertices[index];
		}

		public ITriangle GetTriangle(int index)
		{
			if (triangles[index].triangle != Mesh.dummytri)
			{
				return triangles[index].triangle;
			}
			return null;
		}

		public override int GetHashCode()
		{
			return hash;
		}

		public override string ToString()
		{
			return string.Format("SID {0}", hash);
		}
	}
}
