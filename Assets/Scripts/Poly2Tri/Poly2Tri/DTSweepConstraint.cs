namespace Poly2Tri
{
	public class DTSweepConstraint : TriangulationConstraint
	{
		public DTSweepConstraint(TriangulationPoint p1, TriangulationPoint p2)
		{
			P = p1;
			Q = p2;
			if (p1.Y > p2.Y)
			{
				Q = p1;
				P = p2;
			}
			else if (p1.Y == p2.Y)
			{
				if (p1.X > p2.X)
				{
					Q = p1;
					P = p2;
				}
				else
				{
					double x = p1.X;
					double x2 = p2.X;
				}
			}
			Q.AddEdge(this);
		}
	}
}
