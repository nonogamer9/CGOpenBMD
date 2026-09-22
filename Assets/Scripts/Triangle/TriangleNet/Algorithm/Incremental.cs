using TriangleNet.Data;
using TriangleNet.Geometry;
using TriangleNet.Log;

namespace TriangleNet.Algorithm
{
	internal class Incremental
	{
		private Mesh mesh;

		private void GetBoundingBox()
		{
			Otri newotri = default(Otri);
			BoundingBox bounds = mesh.bounds;
			double num = bounds.Width;
			if (bounds.Height > num)
			{
				num = bounds.Height;
			}
			if (num == 0.0)
			{
				num = 1.0;
			}
			mesh.infvertex1 = new Vertex(bounds.MinX - 50.0 * num, bounds.MinY - 40.0 * num);
			mesh.infvertex2 = new Vertex(bounds.MaxX + 50.0 * num, bounds.MinY - 40.0 * num);
			mesh.infvertex3 = new Vertex(0.5 * (bounds.MinX + bounds.MaxX), bounds.MaxY + 60.0 * num);
			mesh.MakeTriangle(ref newotri);
			newotri.SetOrg(mesh.infvertex1);
			newotri.SetDest(mesh.infvertex2);
			newotri.SetApex(mesh.infvertex3);
			Mesh.dummytri.neighbors[0] = newotri;
		}

		private int RemoveBox()
		{
			Otri o = default(Otri);
			Otri o2 = default(Otri);
			Otri o3 = default(Otri);
			Otri o4 = default(Otri);
			Otri o5 = default(Otri);
			Otri o6 = default(Otri);
			bool flag = !mesh.behavior.Poly;
			o4.triangle = Mesh.dummytri;
			o4.orient = 0;
			o4.SymSelf();
			o4.Lprev(ref o5);
			o4.LnextSelf();
			o4.SymSelf();
			o4.Lprev(ref o2);
			o2.SymSelf();
			o4.Lnext(ref o3);
			o3.SymSelf();
			if (o3.triangle == Mesh.dummytri)
			{
				o2.LprevSelf();
				o2.SymSelf();
			}
			Mesh.dummytri.neighbors[0] = o2;
			int num = -2;
			while (!o4.Equal(o5))
			{
				num++;
				o4.Lprev(ref o6);
				o6.SymSelf();
				if (flag && o6.triangle != Mesh.dummytri)
				{
					Vertex vertex = o6.Org();
					if (vertex.mark == 0)
					{
						vertex.mark = 1;
					}
				}
				o6.Dissolve();
				o4.Lnext(ref o);
				o.Sym(ref o4);
				mesh.TriangleDealloc(o.triangle);
				if (o4.triangle == Mesh.dummytri)
				{
					o6.Copy(ref o4);
				}
			}
			mesh.TriangleDealloc(o5.triangle);
			return num;
		}

		public int Triangulate(Mesh mesh)
		{
			this.mesh = mesh;
			Otri searchtri = default(Otri);
			GetBoundingBox();
			foreach (Vertex value in mesh.vertices.Values)
			{
				searchtri.triangle = Mesh.dummytri;
				Osub splitseg = default(Osub);
				if (mesh.InsertVertex(value, ref searchtri, ref splitseg, false, false) == InsertVertexResult.Duplicate)
				{
					if (Behavior.Verbose)
					{
						SimpleLog.Instance.Warning("A duplicate vertex appeared and was ignored.", "Incremental.IncrementalDelaunay()");
					}
					value.type = VertexType.UndeadVertex;
					mesh.undeads++;
				}
			}
			return RemoveBox();
		}
	}
}
