namespace TriangleNet.Data
{
	internal class BadTriangle
	{
		public static int OTID;

		public int ID;

		public Otri poortri;

		public double key;

		public Vertex triangorg;

		public Vertex triangdest;

		public Vertex triangapex;

		public BadTriangle nexttriang;

		public BadTriangle()
		{
			ID = OTID++;
		}

		public override string ToString()
		{
			return string.Format("B-TID {0}", poortri.triangle.hash);
		}
	}
}
