using TriangleNet.Data;
using TriangleNet.Geometry;

namespace TriangleNet
{
	internal class TriangleLocator
	{
		private Sampler sampler;

		private Mesh mesh;

		internal Otri recenttri;

		public TriangleLocator(Mesh mesh)
		{
			this.mesh = mesh;
			sampler = new Sampler();
		}

		public void Update(ref Otri otri)
		{
			otri.Copy(ref recenttri);
		}

		public void Reset()
		{
			recenttri.triangle = null;
		}

		public LocateResult PreciseLocate(Point searchpoint, ref Otri searchtri, bool stopatsubsegment)
		{
			Otri o = default(Otri);
			Osub os = default(Osub);
			Vertex vertex = searchtri.Org();
			Vertex vertex2 = searchtri.Dest();
			Vertex vertex3 = searchtri.Apex();
			while (true)
			{
				if (vertex3.x == searchpoint.X && vertex3.y == searchpoint.Y)
				{
					searchtri.LprevSelf();
					return LocateResult.OnVertex;
				}
				double num = Primitives.CounterClockwise(vertex, vertex3, searchpoint);
				double num2 = Primitives.CounterClockwise(vertex3, vertex2, searchpoint);
				bool flag;
				if (num > 0.0)
				{
					flag = !(num2 > 0.0) || (vertex3.x - searchpoint.X) * (vertex2.x - vertex.x) + (vertex3.y - searchpoint.Y) * (vertex2.y - vertex.y) > 0.0;
				}
				else
				{
					if (!(num2 > 0.0))
					{
						if (num == 0.0)
						{
							searchtri.LprevSelf();
							return LocateResult.OnEdge;
						}
						if (num2 == 0.0)
						{
							searchtri.LnextSelf();
							return LocateResult.OnEdge;
						}
						return LocateResult.InTriangle;
					}
					flag = false;
				}
				if (flag)
				{
					searchtri.Lprev(ref o);
					vertex2 = vertex3;
				}
				else
				{
					searchtri.Lnext(ref o);
					vertex = vertex3;
				}
				o.Sym(ref searchtri);
				if (mesh.checksegments & stopatsubsegment)
				{
					o.SegPivot(ref os);
					if (os.seg != Mesh.dummysub)
					{
						o.Copy(ref searchtri);
						return LocateResult.Outside;
					}
				}
				if (searchtri.triangle == Mesh.dummytri)
				{
					break;
				}
				vertex3 = searchtri.Apex();
			}
			o.Copy(ref searchtri);
			return LocateResult.Outside;
		}

		public LocateResult Locate(Point searchpoint, ref Otri searchtri)
		{
			Otri otri = default(Otri);
			Vertex vertex = searchtri.Org();
			double num = (searchpoint.X - vertex.x) * (searchpoint.X - vertex.x) + (searchpoint.Y - vertex.y) * (searchpoint.Y - vertex.y);
			if (recenttri.triangle != null && !Otri.IsDead(recenttri.triangle))
			{
				vertex = recenttri.Org();
				if (vertex.x == searchpoint.X && vertex.y == searchpoint.Y)
				{
					recenttri.Copy(ref searchtri);
					return LocateResult.OnVertex;
				}
				double num2 = (searchpoint.X - vertex.x) * (searchpoint.X - vertex.x) + (searchpoint.Y - vertex.y) * (searchpoint.Y - vertex.y);
				if (num2 < num)
				{
					recenttri.Copy(ref searchtri);
					num = num2;
				}
			}
			sampler.Update(mesh);
			int[] samples = sampler.GetSamples(mesh);
			foreach (int key in samples)
			{
				otri.triangle = mesh.triangles[key];
				if (!Otri.IsDead(otri.triangle))
				{
					vertex = otri.Org();
					double num2 = (searchpoint.X - vertex.x) * (searchpoint.X - vertex.x) + (searchpoint.Y - vertex.y) * (searchpoint.Y - vertex.y);
					if (num2 < num)
					{
						otri.Copy(ref searchtri);
						num = num2;
					}
				}
			}
			vertex = searchtri.Org();
			Vertex vertex2 = searchtri.Dest();
			if (vertex.x == searchpoint.X && vertex.y == searchpoint.Y)
			{
				return LocateResult.OnVertex;
			}
			if (vertex2.x == searchpoint.X && vertex2.y == searchpoint.Y)
			{
				searchtri.LnextSelf();
				return LocateResult.OnVertex;
			}
			double num3 = Primitives.CounterClockwise(vertex, vertex2, searchpoint);
			if (num3 < 0.0)
			{
				searchtri.SymSelf();
			}
			else if (num3 == 0.0 && vertex.x < searchpoint.X == searchpoint.X < vertex2.x && vertex.y < searchpoint.Y == searchpoint.Y < vertex2.y)
			{
				return LocateResult.OnEdge;
			}
			return PreciseLocate(searchpoint, ref searchtri, false);
		}
	}
}
