using System.Collections.Generic;

namespace Poly2Tri
{
	public class TriangulationPoint
	{
		public const int INSERTED_INDEX = -1;

		public const int INVALID_INDEX = -2;

		public double X;

		public double Y;

		public int Index;

		public List<DTSweepConstraint> Edges { get; private set; }

		public float Xf
		{
			get
			{
				return (float)X;
			}
			set
			{
				X = value;
			}
		}

		public float Yf
		{
			get
			{
				return (float)Y;
			}
			set
			{
				Y = value;
			}
		}

		public bool HasEdges
		{
			get
			{
				return Edges != null;
			}
		}

		public TriangulationPoint(double x, double y, int index = -1)
		{
			X = x;
			Y = y;
			Index = index;
		}

		public override string ToString()
		{
			return "[" + X + "," + Y + "]";
		}

		public void AddEdge(DTSweepConstraint e)
		{
			if (Edges == null)
			{
				Edges = new List<DTSweepConstraint>();
			}
			Edges.Add(e);
		}
	}
}
