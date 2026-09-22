using TriangleNet.Data;
using TriangleNet.Log;

namespace TriangleNet
{
	public static class MeshValidator
	{
		public static bool IsConsistent(Mesh mesh)
		{
			Otri otri = default(Otri);
			Otri o = default(Otri);
			Otri o2 = default(Otri);
			ILog<SimpleLogItem> instance = SimpleLog.Instance;
			bool noExact = Behavior.NoExact;
			Behavior.NoExact = false;
			int num = 0;
			foreach (Triangle value in mesh.triangles.Values)
			{
				otri.triangle = value;
				otri.orient = 0;
				while (otri.orient < 3)
				{
					Vertex vertex = otri.Org();
					Vertex vertex2 = otri.Dest();
					if (otri.orient == 0)
					{
						Vertex pc = otri.Apex();
						if (Primitives.CounterClockwise(vertex, vertex2, pc) <= 0.0)
						{
							if (Behavior.Verbose)
							{
								instance.Warning("Triangle is flat or inverted.", "Quality.CheckMesh()");
							}
							num++;
						}
					}
					otri.Sym(ref o);
					if (o.triangle != Mesh.dummytri)
					{
						o.Sym(ref o2);
						if (otri.triangle != o2.triangle || otri.orient != o2.orient)
						{
							if (otri.triangle == o2.triangle && Behavior.Verbose)
							{
								instance.Warning("Asymmetric triangle-triangle bond: (Right triangle, wrong orientation)", "Quality.CheckMesh()");
							}
							num++;
						}
						Vertex vertex3 = o.Org();
						Vertex vertex4 = o.Dest();
						if (vertex != vertex4 || vertex2 != vertex3)
						{
							if (Behavior.Verbose)
							{
								instance.Warning("Mismatched edge coordinates between two triangles.", "Quality.CheckMesh()");
							}
							num++;
						}
					}
					otri.orient++;
				}
			}
			mesh.MakeVertexMap();
			foreach (Vertex value2 in mesh.vertices.Values)
			{
				if (value2.tri.triangle == null && Behavior.Verbose)
				{
					instance.Warning("Vertex (ID " + value2.id + ") not connected to mesh (duplicate input vertex?)", "Quality.CheckMesh()");
				}
			}
			Behavior.NoExact = noExact;
			return num == 0;
		}

		public static bool IsDelaunay(Mesh mesh)
		{
			return IsDelaunay(mesh, false);
		}

		public static bool IsConstrainedDelaunay(Mesh mesh)
		{
			return IsDelaunay(mesh, true);
		}

		private static bool IsDelaunay(Mesh mesh, bool constrained)
		{
			Otri otri = default(Otri);
			Otri o = default(Otri);
			Osub os = default(Osub);
			ILog<SimpleLogItem> instance = SimpleLog.Instance;
			bool noExact = Behavior.NoExact;
			Behavior.NoExact = false;
			int num = 0;
			Vertex infvertex = mesh.infvertex1;
			Vertex infvertex2 = mesh.infvertex2;
			Vertex infvertex3 = mesh.infvertex3;
			foreach (Triangle value in mesh.triangles.Values)
			{
				otri.triangle = value;
				otri.orient = 0;
				while (otri.orient < 3)
				{
					Vertex vertex = otri.Org();
					Vertex vertex2 = otri.Dest();
					Vertex vertex3 = otri.Apex();
					otri.Sym(ref o);
					Vertex vertex4 = o.Apex();
					bool flag = otri.triangle.id < o.triangle.id && !Otri.IsDead(o.triangle) && o.triangle != Mesh.dummytri && vertex != infvertex && vertex != infvertex2 && vertex != infvertex3 && vertex2 != infvertex && vertex2 != infvertex2 && vertex2 != infvertex3 && vertex3 != infvertex && vertex3 != infvertex2 && vertex3 != infvertex3 && vertex4 != infvertex && vertex4 != infvertex2 && vertex4 != infvertex3;
					if ((constrained && mesh.checksegments) & flag)
					{
						otri.SegPivot(ref os);
						if (os.seg != Mesh.dummysub)
						{
							flag = false;
						}
					}
					if (flag && Primitives.NonRegular(vertex, vertex2, vertex3, vertex4) > 0.0)
					{
						if (Behavior.Verbose)
						{
							instance.Warning(string.Format("Non-regular pair of triangles found (IDs {0}/{1}).", otri.triangle.id, o.triangle.id), "Quality.CheckDelaunay()");
						}
						num++;
					}
					otri.orient++;
				}
			}
			Behavior.NoExact = noExact;
			return num == 0;
		}
	}
}
