using System;
using System.Collections.Generic;
using TriangleNet.Data;
using TriangleNet.Geometry;
using TriangleNet.Log;
using TriangleNet.Tools;

namespace TriangleNet
{
	internal class ConstraintMesher
	{
		private Mesh mesh;

		private Behavior behavior;

		private TriangleLocator locator;

		private List<Triangle> viri;

		private ILog<SimpleLogItem> logger;

		public ConstraintMesher(Mesh mesh)
		{
			this.mesh = mesh;
			behavior = mesh.behavior;
			locator = mesh.locator;
			viri = new List<Triangle>();
			logger = SimpleLog.Instance;
		}

		public void CarveHoles()
		{
			Otri searchtri = default(Otri);
			Triangle[] array = null;
			if (!mesh.behavior.Convex)
			{
				InfectHull();
			}
			if (!mesh.behavior.NoHoles)
			{
				foreach (Point hole in mesh.holes)
				{
					if (mesh.bounds.Contains(hole))
					{
						searchtri.triangle = Mesh.dummytri;
						searchtri.orient = 0;
						searchtri.SymSelf();
						Vertex pa = searchtri.Org();
						Vertex pb = searchtri.Dest();
						if (Primitives.CounterClockwise(pa, pb, hole) > 0.0 && mesh.locator.Locate(hole, ref searchtri) != LocateResult.Outside && !searchtri.IsInfected())
						{
							searchtri.Infect();
							viri.Add(searchtri.triangle);
						}
					}
				}
			}
			if (mesh.regions.Count > 0)
			{
				int num = 0;
				array = new Triangle[mesh.regions.Count];
				foreach (RegionPointer region in mesh.regions)
				{
					array[num] = Mesh.dummytri;
					if (mesh.bounds.Contains(region.point))
					{
						searchtri.triangle = Mesh.dummytri;
						searchtri.orient = 0;
						searchtri.SymSelf();
						Vertex pa2 = searchtri.Org();
						Vertex pb = searchtri.Dest();
						if (Primitives.CounterClockwise(pa2, pb, region.point) > 0.0 && mesh.locator.Locate(region.point, ref searchtri) != LocateResult.Outside && !searchtri.IsInfected())
						{
							array[num] = searchtri.triangle;
							array[num].region = region.id;
						}
					}
					num++;
				}
			}
			if (viri.Count > 0)
			{
				Plague();
			}
			if (array != null)
			{
				RegionIterator regionIterator = new RegionIterator(mesh);
				for (int i = 0; i < array.Length; i++)
				{
					if (array[i] != Mesh.dummytri && !Otri.IsDead(array[i]))
					{
						regionIterator.Process(array[i]);
					}
				}
			}
			viri.Clear();
		}

		public void FormSkeleton(InputGeometry input)
		{
			mesh.insegments = 0;
			if (behavior.Poly)
			{
				if (mesh.triangles.Count == 0)
				{
					return;
				}
				if (input.HasSegments)
				{
					mesh.MakeVertexMap();
				}
				int num = 0;
				foreach (Edge segment in input.segments)
				{
					mesh.insegments++;
					int p = segment.P0;
					int p2 = segment.P1;
					num = segment.Boundary;
					if (p < 0 || p >= mesh.invertices)
					{
						if (Behavior.Verbose)
						{
							logger.Warning("Invalid first endpoint of segment.", "Mesh.FormSkeleton().1");
						}
						continue;
					}
					if (p2 < 0 || p2 >= mesh.invertices)
					{
						if (Behavior.Verbose)
						{
							logger.Warning("Invalid second endpoint of segment.", "Mesh.FormSkeleton().2");
						}
						continue;
					}
					Vertex vertex = mesh.vertices[p];
					Vertex vertex2 = mesh.vertices[p2];
					if (vertex.x == vertex2.x && vertex.y == vertex2.y)
					{
						if (Behavior.Verbose)
						{
							logger.Warning("Endpoints of segment (IDs " + p + "/" + p2 + ") are coincident.", "Mesh.FormSkeleton()");
						}
					}
					else
					{
						InsertSegment(vertex, vertex2, num);
					}
				}
			}
			if (behavior.Convex || !behavior.Poly)
			{
				MarkHull();
			}
		}

		private void InfectHull()
		{
			Otri o = default(Otri);
			Otri o2 = default(Otri);
			Otri o3 = default(Otri);
			Osub os = default(Osub);
			o.triangle = Mesh.dummytri;
			o.orient = 0;
			o.SymSelf();
			o.Copy(ref o3);
			do
			{
				if (!o.IsInfected())
				{
					o.SegPivot(ref os);
					if (os.seg == Mesh.dummysub)
					{
						if (!o.IsInfected())
						{
							o.Infect();
							viri.Add(o.triangle);
						}
					}
					else if (os.seg.boundary == 0)
					{
						os.seg.boundary = 1;
						Vertex vertex = o.Org();
						Vertex vertex2 = o.Dest();
						if (vertex.mark == 0)
						{
							vertex.mark = 1;
						}
						if (vertex2.mark == 0)
						{
							vertex2.mark = 1;
						}
					}
				}
				o.LnextSelf();
				o.Oprev(ref o2);
				while (o2.triangle != Mesh.dummytri)
				{
					o2.Copy(ref o);
					o.Oprev(ref o2);
				}
			}
			while (!o.Equal(o3));
		}

		private void Plague()
		{
			Otri o = default(Otri);
			Otri o2 = default(Otri);
			Osub os = default(Osub);
			for (int i = 0; i < viri.Count; i++)
			{
				o.triangle = viri[i];
				o.Uninfect();
				o.orient = 0;
				while (o.orient < 3)
				{
					o.Sym(ref o2);
					o.SegPivot(ref os);
					if (o2.triangle == Mesh.dummytri || o2.IsInfected())
					{
						if (os.seg != Mesh.dummysub)
						{
							mesh.SubsegDealloc(os.seg);
							if (o2.triangle != Mesh.dummytri)
							{
								o2.Uninfect();
								o2.SegDissolve();
								o2.Infect();
							}
						}
					}
					else if (os.seg == Mesh.dummysub)
					{
						o2.Infect();
						viri.Add(o2.triangle);
					}
					else
					{
						os.TriDissolve();
						if (os.seg.boundary == 0)
						{
							os.seg.boundary = 1;
						}
						Vertex vertex = o2.Org();
						Vertex vertex2 = o2.Dest();
						if (vertex.mark == 0)
						{
							vertex.mark = 1;
						}
						if (vertex2.mark == 0)
						{
							vertex2.mark = 1;
						}
					}
					o.orient++;
				}
				o.Infect();
			}
			foreach (Triangle virus in viri)
			{
				o.triangle = virus;
				o.orient = 0;
				while (o.orient < 3)
				{
					Vertex vertex3 = o.Org();
					if (vertex3 != null)
					{
						bool flag = true;
						o.SetOrg(null);
						o.Onext(ref o2);
						while (o2.triangle != Mesh.dummytri && !o2.Equal(o))
						{
							if (o2.IsInfected())
							{
								o2.SetOrg(null);
							}
							else
							{
								flag = false;
							}
							o2.OnextSelf();
						}
						if (o2.triangle == Mesh.dummytri)
						{
							o.Oprev(ref o2);
							while (o2.triangle != Mesh.dummytri)
							{
								if (o2.IsInfected())
								{
									o2.SetOrg(null);
								}
								else
								{
									flag = false;
								}
								o2.OprevSelf();
							}
						}
						if (flag)
						{
							vertex3.type = VertexType.UndeadVertex;
							mesh.undeads++;
						}
					}
					o.orient++;
				}
				o.orient = 0;
				while (o.orient < 3)
				{
					o.Sym(ref o2);
					if (o2.triangle == Mesh.dummytri)
					{
						mesh.hullsize--;
					}
					else
					{
						o2.Dissolve();
						mesh.hullsize++;
					}
					o.orient++;
				}
				mesh.TriangleDealloc(o.triangle);
			}
			viri.Clear();
		}

		private FindDirectionResult FindDirection(ref Otri searchtri, Vertex searchpoint)
		{
			Otri o = default(Otri);
			Vertex vertex = searchtri.Org();
			Vertex pc = searchtri.Dest();
			Vertex pc2 = searchtri.Apex();
			double num = Primitives.CounterClockwise(searchpoint, vertex, pc2);
			bool flag = num > 0.0;
			double num2 = Primitives.CounterClockwise(vertex, searchpoint, pc);
			bool flag2 = num2 > 0.0;
			if (flag & flag2)
			{
				searchtri.Onext(ref o);
				if (o.triangle == Mesh.dummytri)
				{
					flag = false;
				}
				else
				{
					flag2 = false;
				}
			}
			while (flag)
			{
				searchtri.OnextSelf();
				if (searchtri.triangle == Mesh.dummytri)
				{
					logger.Error("Unable to find a triangle on path.", "Mesh.FindDirection().1");
					throw new Exception("Unable to find a triangle on path.");
				}
				pc2 = searchtri.Apex();
				num2 = num;
				num = Primitives.CounterClockwise(searchpoint, vertex, pc2);
				flag = num > 0.0;
			}
			while (flag2)
			{
				searchtri.OprevSelf();
				if (searchtri.triangle == Mesh.dummytri)
				{
					logger.Error("Unable to find a triangle on path.", "Mesh.FindDirection().2");
					throw new Exception("Unable to find a triangle on path.");
				}
				pc = searchtri.Dest();
				num = num2;
				num2 = Primitives.CounterClockwise(vertex, searchpoint, pc);
				flag2 = num2 > 0.0;
			}
			if (num == 0.0)
			{
				return FindDirectionResult.Leftcollinear;
			}
			if (num2 == 0.0)
			{
				return FindDirectionResult.Rightcollinear;
			}
			return FindDirectionResult.Within;
		}

		private void SegmentIntersection(ref Otri splittri, ref Osub splitsubseg, Vertex endpoint2)
		{
			Osub o = default(Osub);
			Vertex vertex = splittri.Apex();
			Vertex vertex2 = splittri.Org();
			Vertex vertex3 = splittri.Dest();
			double num = vertex3.x - vertex2.x;
			double num2 = vertex3.y - vertex2.y;
			double num3 = endpoint2.x - vertex.x;
			double num4 = endpoint2.y - vertex.y;
			double num5 = vertex2.x - endpoint2.x;
			double num6 = vertex2.y - endpoint2.y;
			double num7 = num2 * num3 - num * num4;
			if (num7 == 0.0)
			{
				logger.Error("Attempt to find intersection of parallel segments.", "Mesh.SegmentIntersection()");
				throw new Exception("Attempt to find intersection of parallel segments.");
			}
			double num8 = (num4 * num5 - num3 * num6) / num7;
			Vertex vertex4 = new Vertex(vertex2.x + num8 * (vertex3.x - vertex2.x), vertex2.y + num8 * (vertex3.y - vertex2.y), splitsubseg.seg.boundary, mesh.nextras);
			vertex4.hash = mesh.hash_vtx++;
			vertex4.id = vertex4.hash;
			for (int i = 0; i < mesh.nextras; i++)
			{
				vertex4.attributes[i] = vertex2.attributes[i] + num8 * (vertex3.attributes[i] - vertex2.attributes[i]);
			}
			mesh.vertices.Add(vertex4.hash, vertex4);
			if (mesh.InsertVertex(vertex4, ref splittri, ref splitsubseg, false, false) != InsertVertexResult.Successful)
			{
				logger.Error("Failure to split a segment.", "Mesh.SegmentIntersection()");
				throw new Exception("Failure to split a segment.");
			}
			vertex4.tri = splittri;
			if (mesh.steinerleft > 0)
			{
				mesh.steinerleft--;
			}
			splitsubseg.SymSelf();
			splitsubseg.Pivot(ref o);
			splitsubseg.Dissolve();
			o.Dissolve();
			do
			{
				splitsubseg.SetSegOrg(vertex4);
				splitsubseg.NextSelf();
			}
			while (splitsubseg.seg != Mesh.dummysub);
			do
			{
				o.SetSegOrg(vertex4);
				o.NextSelf();
			}
			while (o.seg != Mesh.dummysub);
			FindDirection(ref splittri, vertex);
			Vertex vertex5 = splittri.Dest();
			Vertex vertex6 = splittri.Apex();
			if (vertex6.x == vertex.x && vertex6.y == vertex.y)
			{
				splittri.OnextSelf();
			}
			else if (vertex5.x != vertex.x || vertex5.y != vertex.y)
			{
				logger.Error("Topological inconsistency after splitting a segment.", "Mesh.SegmentIntersection()");
				throw new Exception("Topological inconsistency after splitting a segment.");
			}
		}

		private bool ScoutSegment(ref Otri searchtri, Vertex endpoint2, int newmark)
		{
			Otri o = default(Otri);
			Osub os = default(Osub);
			FindDirectionResult findDirectionResult = FindDirection(ref searchtri, endpoint2);
			Vertex vertex = searchtri.Dest();
			Vertex vertex2 = searchtri.Apex();
			if ((vertex2.x == endpoint2.x && vertex2.y == endpoint2.y) || (vertex.x == endpoint2.x && vertex.y == endpoint2.y))
			{
				if (vertex2.x == endpoint2.x && vertex2.y == endpoint2.y)
				{
					searchtri.LprevSelf();
				}
				mesh.InsertSubseg(ref searchtri, newmark);
				return true;
			}
			switch (findDirectionResult)
			{
			case FindDirectionResult.Leftcollinear:
				searchtri.LprevSelf();
				mesh.InsertSubseg(ref searchtri, newmark);
				return ScoutSegment(ref searchtri, endpoint2, newmark);
			case FindDirectionResult.Rightcollinear:
				mesh.InsertSubseg(ref searchtri, newmark);
				searchtri.LnextSelf();
				return ScoutSegment(ref searchtri, endpoint2, newmark);
			default:
				searchtri.Lnext(ref o);
				o.SegPivot(ref os);
				if (os.seg == Mesh.dummysub)
				{
					return false;
				}
				SegmentIntersection(ref o, ref os, endpoint2);
				o.Copy(ref searchtri);
				mesh.InsertSubseg(ref searchtri, newmark);
				return ScoutSegment(ref searchtri, endpoint2, newmark);
			}
		}

		private void DelaunayFixup(ref Otri fixuptri, bool leftside)
		{
			Otri o = default(Otri);
			Otri o2 = default(Otri);
			Osub os = default(Osub);
			fixuptri.Lnext(ref o);
			o.Sym(ref o2);
			if (o2.triangle == Mesh.dummytri)
			{
				return;
			}
			o.SegPivot(ref os);
			if (os.seg != Mesh.dummysub)
			{
				return;
			}
			Vertex vertex = o.Apex();
			Vertex vertex2 = o.Org();
			Vertex vertex3 = o.Dest();
			Vertex vertex4 = o2.Apex();
			if (leftside)
			{
				if (Primitives.CounterClockwise(vertex, vertex2, vertex4) <= 0.0)
				{
					return;
				}
			}
			else if (Primitives.CounterClockwise(vertex4, vertex3, vertex) <= 0.0)
			{
				return;
			}
			if (!(Primitives.CounterClockwise(vertex3, vertex2, vertex4) > 0.0) || !(Primitives.InCircle(vertex2, vertex4, vertex3, vertex) <= 0.0))
			{
				mesh.Flip(ref o);
				fixuptri.LprevSelf();
				DelaunayFixup(ref fixuptri, leftside);
				DelaunayFixup(ref o2, leftside);
			}
		}

		private void ConstrainedEdge(ref Otri starttri, Vertex endpoint2, int newmark)
		{
			Otri o = default(Otri);
			Otri o2 = default(Otri);
			Osub os = default(Osub);
			Vertex pa = starttri.Org();
			starttri.Lnext(ref o);
			mesh.Flip(ref o);
			bool flag = false;
			bool flag2 = false;
			do
			{
				Vertex vertex = o.Org();
				if (vertex.x == endpoint2.x && vertex.y == endpoint2.y)
				{
					o.Oprev(ref o2);
					DelaunayFixup(ref o, false);
					DelaunayFixup(ref o2, true);
					flag2 = true;
					continue;
				}
				double num = Primitives.CounterClockwise(pa, endpoint2, vertex);
				if (num == 0.0)
				{
					flag = true;
					o.Oprev(ref o2);
					DelaunayFixup(ref o, false);
					DelaunayFixup(ref o2, true);
					flag2 = true;
					continue;
				}
				if (num > 0.0)
				{
					o.Oprev(ref o2);
					DelaunayFixup(ref o2, true);
					o.LprevSelf();
				}
				else
				{
					DelaunayFixup(ref o, false);
					o.OprevSelf();
				}
				o.SegPivot(ref os);
				if (os.seg == Mesh.dummysub)
				{
					mesh.Flip(ref o);
					continue;
				}
				flag = true;
				SegmentIntersection(ref o, ref os, endpoint2);
				flag2 = true;
			}
			while (!flag2);
			mesh.InsertSubseg(ref o, newmark);
			if (flag && !ScoutSegment(ref o, endpoint2, newmark))
			{
				ConstrainedEdge(ref o, endpoint2, newmark);
			}
		}

		private void InsertSegment(Vertex endpoint1, Vertex endpoint2, int newmark)
		{
			Otri otri = default(Otri);
			Otri otri2 = default(Otri);
			Vertex vertex = null;
			otri = endpoint1.tri;
			if (otri.triangle != null)
			{
				vertex = otri.Org();
			}
			if (vertex != endpoint1)
			{
				otri.triangle = Mesh.dummytri;
				otri.orient = 0;
				otri.SymSelf();
				if (locator.Locate(endpoint1, ref otri) != LocateResult.OnVertex)
				{
					logger.Error("Unable to locate PSLG vertex in triangulation.", "Mesh.InsertSegment().1");
					throw new Exception("Unable to locate PSLG vertex in triangulation.");
				}
			}
			locator.Update(ref otri);
			if (ScoutSegment(ref otri, endpoint2, newmark))
			{
				return;
			}
			endpoint1 = otri.Org();
			vertex = null;
			otri2 = endpoint2.tri;
			if (otri2.triangle != null)
			{
				vertex = otri2.Org();
			}
			if (vertex != endpoint2)
			{
				otri2.triangle = Mesh.dummytri;
				otri2.orient = 0;
				otri2.SymSelf();
				if (locator.Locate(endpoint2, ref otri2) != LocateResult.OnVertex)
				{
					logger.Error("Unable to locate PSLG vertex in triangulation.", "Mesh.InsertSegment().2");
					throw new Exception("Unable to locate PSLG vertex in triangulation.");
				}
			}
			locator.Update(ref otri2);
			if (!ScoutSegment(ref otri2, endpoint1, newmark))
			{
				endpoint2 = otri2.Org();
				ConstrainedEdge(ref otri, endpoint2, newmark);
			}
		}

		private void MarkHull()
		{
			Otri tri = default(Otri);
			Otri o = default(Otri);
			Otri o2 = default(Otri);
			tri.triangle = Mesh.dummytri;
			tri.orient = 0;
			tri.SymSelf();
			tri.Copy(ref o2);
			do
			{
				mesh.InsertSubseg(ref tri, 1);
				tri.LnextSelf();
				tri.Oprev(ref o);
				while (o.triangle != Mesh.dummytri)
				{
					o.Copy(ref tri);
					tri.Oprev(ref o);
				}
			}
			while (!tri.Equal(o2));
		}
	}
}
