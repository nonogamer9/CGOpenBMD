namespace TriangleNet.Data
{
	internal struct Otri
	{
		public Triangle triangle;

		public int orient;

		private static readonly int[] plus1Mod3 = new int[3] { 1, 2, 0 };

		private static readonly int[] minus1Mod3 = new int[3] { 2, 0, 1 };

		public override string ToString()
		{
			if (triangle == null)
			{
				return "O-TID [null]";
			}
			return string.Format("O-TID {0}", triangle.hash);
		}

		public void Sym(ref Otri o2)
		{
			o2.triangle = triangle.neighbors[orient].triangle;
			o2.orient = triangle.neighbors[orient].orient;
		}

		public void SymSelf()
		{
			int num = orient;
			orient = triangle.neighbors[num].orient;
			triangle = triangle.neighbors[num].triangle;
		}

		public void Lnext(ref Otri o2)
		{
			o2.triangle = triangle;
			o2.orient = plus1Mod3[orient];
		}

		public void LnextSelf()
		{
			orient = plus1Mod3[orient];
		}

		public void Lprev(ref Otri o2)
		{
			o2.triangle = triangle;
			o2.orient = minus1Mod3[orient];
		}

		public void LprevSelf()
		{
			orient = minus1Mod3[orient];
		}

		public void Onext(ref Otri o2)
		{
			o2.triangle = triangle;
			o2.orient = minus1Mod3[orient];
			int num = o2.orient;
			o2.orient = o2.triangle.neighbors[num].orient;
			o2.triangle = o2.triangle.neighbors[num].triangle;
		}

		public void OnextSelf()
		{
			orient = minus1Mod3[orient];
			int num = orient;
			orient = triangle.neighbors[num].orient;
			triangle = triangle.neighbors[num].triangle;
		}

		public void Oprev(ref Otri o2)
		{
			o2.triangle = triangle.neighbors[orient].triangle;
			o2.orient = triangle.neighbors[orient].orient;
			o2.orient = plus1Mod3[o2.orient];
		}

		public void OprevSelf()
		{
			int num = orient;
			orient = triangle.neighbors[num].orient;
			triangle = triangle.neighbors[num].triangle;
			orient = plus1Mod3[orient];
		}

		public void Dnext(ref Otri o2)
		{
			o2.triangle = triangle.neighbors[orient].triangle;
			o2.orient = triangle.neighbors[orient].orient;
			o2.orient = minus1Mod3[o2.orient];
		}

		public void DnextSelf()
		{
			int num = orient;
			orient = triangle.neighbors[num].orient;
			triangle = triangle.neighbors[num].triangle;
			orient = minus1Mod3[orient];
		}

		public void Dprev(ref Otri o2)
		{
			o2.triangle = triangle;
			o2.orient = plus1Mod3[orient];
			int num = o2.orient;
			o2.orient = o2.triangle.neighbors[num].orient;
			o2.triangle = o2.triangle.neighbors[num].triangle;
		}

		public void DprevSelf()
		{
			orient = plus1Mod3[orient];
			int num = orient;
			orient = triangle.neighbors[num].orient;
			triangle = triangle.neighbors[num].triangle;
		}

		public void Rnext(ref Otri o2)
		{
			o2.triangle = triangle.neighbors[orient].triangle;
			o2.orient = triangle.neighbors[orient].orient;
			o2.orient = plus1Mod3[o2.orient];
			int num = o2.orient;
			o2.orient = o2.triangle.neighbors[num].orient;
			o2.triangle = o2.triangle.neighbors[num].triangle;
		}

		public void RnextSelf()
		{
			int num = orient;
			orient = triangle.neighbors[num].orient;
			triangle = triangle.neighbors[num].triangle;
			orient = plus1Mod3[orient];
			num = orient;
			orient = triangle.neighbors[num].orient;
			triangle = triangle.neighbors[num].triangle;
		}

		public void Rprev(ref Otri o2)
		{
			o2.triangle = triangle.neighbors[orient].triangle;
			o2.orient = triangle.neighbors[orient].orient;
			o2.orient = minus1Mod3[o2.orient];
			int num = o2.orient;
			o2.orient = o2.triangle.neighbors[num].orient;
			o2.triangle = o2.triangle.neighbors[num].triangle;
		}

		public void RprevSelf()
		{
			int num = orient;
			orient = triangle.neighbors[num].orient;
			triangle = triangle.neighbors[num].triangle;
			orient = minus1Mod3[orient];
			num = orient;
			orient = triangle.neighbors[num].orient;
			triangle = triangle.neighbors[num].triangle;
		}

		public Vertex Org()
		{
			return triangle.vertices[plus1Mod3[orient]];
		}

		public Vertex Dest()
		{
			return triangle.vertices[minus1Mod3[orient]];
		}

		public Vertex Apex()
		{
			return triangle.vertices[orient];
		}

		public void SetOrg(Vertex ptr)
		{
			triangle.vertices[plus1Mod3[orient]] = ptr;
		}

		public void SetDest(Vertex ptr)
		{
			triangle.vertices[minus1Mod3[orient]] = ptr;
		}

		public void SetApex(Vertex ptr)
		{
			triangle.vertices[orient] = ptr;
		}

		public void Bond(ref Otri o2)
		{
			triangle.neighbors[orient].triangle = o2.triangle;
			triangle.neighbors[orient].orient = o2.orient;
			o2.triangle.neighbors[o2.orient].triangle = triangle;
			o2.triangle.neighbors[o2.orient].orient = orient;
		}

		public void Dissolve()
		{
			triangle.neighbors[orient].triangle = Mesh.dummytri;
			triangle.neighbors[orient].orient = 0;
		}

		public void Copy(ref Otri o2)
		{
			o2.triangle = triangle;
			o2.orient = orient;
		}

		public bool Equal(Otri o2)
		{
			if (triangle == o2.triangle)
			{
				return orient == o2.orient;
			}
			return false;
		}

		public void Infect()
		{
			triangle.infected = true;
		}

		public void Uninfect()
		{
			triangle.infected = false;
		}

		public bool IsInfected()
		{
			return triangle.infected;
		}

		public static bool IsDead(Triangle tria)
		{
			return tria.neighbors[0].triangle == null;
		}

		public static void Kill(Triangle tria)
		{
			tria.neighbors[0].triangle = null;
			tria.neighbors[2].triangle = null;
		}

		public void SegPivot(ref Osub os)
		{
			os = triangle.subsegs[orient];
		}

		public void SegBond(ref Osub os)
		{
			triangle.subsegs[orient] = os;
			os.seg.triangles[os.orient] = this;
		}

		public void SegDissolve()
		{
			triangle.subsegs[orient].seg = Mesh.dummysub;
		}
	}
}
