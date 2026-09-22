using System;
using System.Collections;
using System.Collections.Generic;
using TriangleNet.Data;

namespace TriangleNet.Geometry
{
	public class EdgeEnumerator : IEnumerator<Edge>, IDisposable, IEnumerator
	{
		private IEnumerator<Triangle> triangles;

		private Otri tri;

		private Otri neighbor;

		private Osub sub;

		private Edge current;

		private Vertex p1;

		private Vertex p2;

		public Edge Current
		{
			get
			{
				return current;
			}
		}

		object IEnumerator.Current
		{
			get
			{
				return current;
			}
		}

		public EdgeEnumerator(Mesh mesh)
		{
			triangles = mesh.triangles.Values.GetEnumerator();
			triangles.MoveNext();
			tri.triangle = triangles.Current;
			tri.orient = 0;
		}

		public void Dispose()
		{
			triangles.Dispose();
		}

		public bool MoveNext()
		{
			if (tri.triangle == null)
			{
				return false;
			}
			current = null;
			while (current == null)
			{
				if (tri.orient == 3)
				{
					if (!triangles.MoveNext())
					{
						return false;
					}
					tri.triangle = triangles.Current;
					tri.orient = 0;
				}
				tri.Sym(ref neighbor);
				if (tri.triangle.id < neighbor.triangle.id || neighbor.triangle == Mesh.dummytri)
				{
					p1 = tri.Org();
					p2 = tri.Dest();
					tri.SegPivot(ref sub);
					current = new Edge(p1.id, p2.id, sub.seg.boundary);
				}
				tri.orient++;
			}
			return true;
		}

		public void Reset()
		{
			triangles.Reset();
		}
	}
}
