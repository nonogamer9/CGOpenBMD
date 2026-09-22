using System;
using TriangleNet.Geometry;

namespace TriangleNet.Data
{
	public class Vertex : Point
	{
		internal int hash;

		internal VertexType type;

		internal Otri tri;

		public VertexType Type
		{
			get
			{
				return type;
			}
		}

		public double this[int i]
		{
			get
			{
				switch (i)
				{
				case 0:
					return x;
				case 1:
					return y;
				default:
					throw new ArgumentOutOfRangeException("Index must be 0 or 1.");
				}
			}
		}

		public Vertex()
			: this(0.0, 0.0, 0, 0)
		{
		}

		public Vertex(double x, double y)
			: this(x, y, 0, 0)
		{
		}

		public Vertex(double x, double y, int mark)
			: this(x, y, mark, 0)
		{
		}

		public Vertex(double x, double y, int mark, int attribs)
			: base(x, y, mark)
		{
			type = VertexType.InputVertex;
			if (attribs > 0)
			{
				attributes = new double[attribs];
			}
		}

		public override int GetHashCode()
		{
			return hash;
		}
	}
}
