using System;
using System.Collections.Generic;
using TriangleNet.Data;

namespace TriangleNet.Tools
{
	public class RegionIterator
	{
		private Mesh mesh;

		private List<Triangle> viri;

		public RegionIterator(Mesh mesh)
		{
			this.mesh = mesh;
			viri = new List<Triangle>();
		}

		private void ProcessRegion(Action<Triangle> func)
		{
			Otri otri = default(Otri);
			Otri o = default(Otri);
			Osub os = default(Osub);
			Behavior behavior = mesh.behavior;
			for (int i = 0; i < viri.Count; i++)
			{
				otri.triangle = viri[i];
				otri.Uninfect();
				func(otri.triangle);
				otri.orient = 0;
				while (otri.orient < 3)
				{
					otri.Sym(ref o);
					otri.SegPivot(ref os);
					if (o.triangle != Mesh.dummytri && !o.IsInfected() && os.seg == Mesh.dummysub)
					{
						o.Infect();
						viri.Add(o.triangle);
					}
					otri.orient++;
				}
				otri.Infect();
			}
			foreach (Triangle virus in viri)
			{
				virus.infected = false;
			}
			viri.Clear();
		}

		public void Process(Triangle triangle)
		{
			Process(triangle, (Triangle tri) =>
			{
				tri.region = triangle.region;
			});
		}

		public void Process(Triangle triangle, Action<Triangle> func)
		{
			if (triangle != Mesh.dummytri && !Otri.IsDead(triangle))
			{
				triangle.infected = true;
				viri.Add(triangle);
				ProcessRegion(func);
			}
			viri.Clear();
		}
	}
}
