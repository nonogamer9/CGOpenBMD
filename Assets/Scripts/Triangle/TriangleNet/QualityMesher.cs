using System;
using System.Collections.Generic;
using TriangleNet.Data;
using TriangleNet.Geometry;
using TriangleNet.Log;

namespace TriangleNet
{
	internal class QualityMesher
	{
		private Queue<BadSubseg> badsubsegs;

		private BadTriQueue queue;

		private Mesh mesh;

		private Behavior behavior;

		private NewLocation newLocation;

		private ILog<SimpleLogItem> logger;

		public QualityMesher(Mesh mesh)
		{
			logger = SimpleLog.Instance;
			badsubsegs = new Queue<BadSubseg>();
			queue = new BadTriQueue();
			this.mesh = mesh;
			behavior = mesh.behavior;
			newLocation = new NewLocation(mesh);
		}

		public void AddBadSubseg(BadSubseg badseg)
		{
			badsubsegs.Enqueue(badseg);
		}

		public int CheckSeg4Encroach(ref Osub testsubseg)
		{
			Otri ot = default(Otri);
			Osub o = default(Osub);
			int num = 0;
			int num2 = 0;
			Vertex vertex = testsubseg.Org();
			Vertex vertex2 = testsubseg.Dest();
			testsubseg.TriPivot(ref ot);
			if (ot.triangle != Mesh.dummytri)
			{
				num2++;
				Vertex vertex3 = ot.Apex();
				double num3 = (vertex.x - vertex3.x) * (vertex2.x - vertex3.x) + (vertex.y - vertex3.y) * (vertex2.y - vertex3.y);
				if (num3 < 0.0 && (behavior.ConformingDelaunay || num3 * num3 >= (2.0 * behavior.goodAngle - 1.0) * (2.0 * behavior.goodAngle - 1.0) * ((vertex.x - vertex3.x) * (vertex.x - vertex3.x) + (vertex.y - vertex3.y) * (vertex.y - vertex3.y)) * ((vertex2.x - vertex3.x) * (vertex2.x - vertex3.x) + (vertex2.y - vertex3.y) * (vertex2.y - vertex3.y))))
				{
					num = 1;
				}
			}
			testsubseg.Sym(ref o);
			o.TriPivot(ref ot);
			if (ot.triangle != Mesh.dummytri)
			{
				num2++;
				Vertex vertex3 = ot.Apex();
				double num3 = (vertex.x - vertex3.x) * (vertex2.x - vertex3.x) + (vertex.y - vertex3.y) * (vertex2.y - vertex3.y);
				if (num3 < 0.0 && (behavior.ConformingDelaunay || num3 * num3 >= (2.0 * behavior.goodAngle - 1.0) * (2.0 * behavior.goodAngle - 1.0) * ((vertex.x - vertex3.x) * (vertex.x - vertex3.x) + (vertex.y - vertex3.y) * (vertex.y - vertex3.y)) * ((vertex2.x - vertex3.x) * (vertex2.x - vertex3.x) + (vertex2.y - vertex3.y) * (vertex2.y - vertex3.y))))
				{
					num += 2;
				}
			}
			if (num > 0 && (behavior.NoBisect == 0 || (behavior.NoBisect == 1 && num2 == 2)))
			{
				BadSubseg badSubseg = new BadSubseg();
				if (num == 1)
				{
					badSubseg.encsubseg = testsubseg;
					badSubseg.subsegorg = vertex;
					badSubseg.subsegdest = vertex2;
				}
				else
				{
					badSubseg.encsubseg = o;
					badSubseg.subsegorg = vertex2;
					badSubseg.subsegdest = vertex;
				}
				badsubsegs.Enqueue(badSubseg);
			}
			return num;
		}

		public void TestTriangle(ref Otri testtri)
		{
			Otri o = default(Otri);
			Otri o2 = default(Otri);
			Osub os = default(Osub);
			Vertex vertex = testtri.Org();
			Vertex vertex2 = testtri.Dest();
			Vertex vertex3 = testtri.Apex();
			double num = vertex.x - vertex2.x;
			double num2 = vertex.y - vertex2.y;
			double num3 = vertex2.x - vertex3.x;
			double num4 = vertex2.y - vertex3.y;
			double num5 = vertex3.x - vertex.x;
			double num6 = vertex3.y - vertex.y;
			double num7 = num * num;
			double num8 = num2 * num2;
			double num9 = num3 * num3;
			double num10 = num4 * num4;
			double num11 = num5 * num5;
			double num12 = num6 * num6;
			double num13 = num7 + num8;
			double num14 = num9 + num10;
			double num15 = num11 + num12;
			double minedge;
			Vertex vertex4;
			Vertex vertex5;
			double num16;
			if (num13 < num14 && num13 < num15)
			{
				minedge = num13;
				num16 = num3 * num5 + num4 * num6;
				num16 = num16 * num16 / (num14 * num15);
				vertex4 = vertex;
				vertex5 = vertex2;
				testtri.Copy(ref o);
			}
			else if (num14 < num15)
			{
				minedge = num14;
				num16 = num * num5 + num2 * num6;
				num16 = num16 * num16 / (num13 * num15);
				vertex4 = vertex2;
				vertex5 = vertex3;
				testtri.Lnext(ref o);
			}
			else
			{
				minedge = num15;
				num16 = num * num3 + num2 * num4;
				num16 = num16 * num16 / (num13 * num14);
				vertex4 = vertex3;
				vertex5 = vertex;
				testtri.Lprev(ref o);
			}
			if (behavior.VarArea || behavior.fixedArea || behavior.UserTest != null)
			{
				double num17 = 0.5 * (num * num4 - num2 * num3);
				if (behavior.fixedArea && num17 > behavior.MaxArea)
				{
					queue.Enqueue(ref testtri, minedge, vertex3, vertex, vertex2);
					return;
				}
				if (behavior.VarArea && num17 > testtri.triangle.area && testtri.triangle.area > 0.0)
				{
					queue.Enqueue(ref testtri, minedge, vertex3, vertex, vertex2);
					return;
				}
				if (behavior.UserTest != null && behavior.UserTest(testtri.triangle, num17))
				{
					queue.Enqueue(ref testtri, minedge, vertex3, vertex, vertex2);
					return;
				}
			}
			double num18 = ((num13 > num14 && num13 > num15) ? ((num14 + num15 - num13) / (2.0 * Math.Sqrt(num14 * num15))) : ((!(num14 > num15)) ? ((num13 + num14 - num15) / (2.0 * Math.Sqrt(num13 * num14))) : ((num13 + num15 - num14) / (2.0 * Math.Sqrt(num13 * num15)))));
			if (!(num16 > behavior.goodAngle) && (!(num18 < behavior.maxGoodAngle) || behavior.MaxAngle == 0.0))
			{
				return;
			}
			if (vertex4.type == VertexType.SegmentVertex && vertex5.type == VertexType.SegmentVertex)
			{
				o.SegPivot(ref os);
				if (os.seg == Mesh.dummysub)
				{
					o.Copy(ref o2);
					do
					{
						o.OprevSelf();
						o.SegPivot(ref os);
					}
					while (os.seg == Mesh.dummysub);
					Vertex vertex6 = os.SegOrg();
					Vertex vertex7 = os.SegDest();
					do
					{
						o2.DnextSelf();
						o2.SegPivot(ref os);
					}
					while (os.seg == Mesh.dummysub);
					Vertex vertex8 = os.SegOrg();
					Vertex vertex9 = os.SegDest();
					Vertex vertex10 = null;
					if (vertex7.x == vertex8.x && vertex7.y == vertex8.y)
					{
						vertex10 = vertex7;
					}
					else if (vertex6.x == vertex9.x && vertex6.y == vertex9.y)
					{
						vertex10 = vertex6;
					}
					if (vertex10 != null)
					{
						double num19 = (vertex4.x - vertex10.x) * (vertex4.x - vertex10.x) + (vertex4.y - vertex10.y) * (vertex4.y - vertex10.y);
						double num20 = (vertex5.x - vertex10.x) * (vertex5.x - vertex10.x) + (vertex5.y - vertex10.y) * (vertex5.y - vertex10.y);
						if (num19 < 1.001 * num20 && num19 > 0.999 * num20)
						{
							return;
						}
					}
				}
			}
			queue.Enqueue(ref testtri, minedge, vertex3, vertex, vertex2);
		}

		private void TallyEncs()
		{
			Osub testsubseg = new Osub
			{
				orient = 0
			};
			foreach (Segment value in mesh.subsegs.Values)
			{
				testsubseg.seg = value;
				CheckSeg4Encroach(ref testsubseg);
			}
		}

		private void SplitEncSegs(bool triflaws)
		{
			Otri ot = default(Otri);
			Otri o = default(Otri);
			Osub os = default(Osub);
			Osub osub = default(Osub);
			while (badsubsegs.Count > 0 && mesh.steinerleft != 0)
			{
				BadSubseg badSubseg = badsubsegs.Dequeue();
				osub = badSubseg.encsubseg;
				Vertex vertex = osub.Org();
				Vertex vertex2 = osub.Dest();
				if (!Osub.IsDead(osub.seg) && vertex == badSubseg.subsegorg && vertex2 == badSubseg.subsegdest)
				{
					osub.TriPivot(ref ot);
					ot.Lnext(ref o);
					o.SegPivot(ref os);
					bool flag = os.seg != Mesh.dummysub;
					o.LnextSelf();
					o.SegPivot(ref os);
					bool flag2 = os.seg != Mesh.dummysub;
					if (!behavior.ConformingDelaunay && !flag && !flag2)
					{
						Vertex vertex3 = ot.Apex();
						while (vertex3.type == VertexType.FreeVertex && (vertex.x - vertex3.x) * (vertex2.x - vertex3.x) + (vertex.y - vertex3.y) * (vertex2.y - vertex3.y) < 0.0)
						{
							mesh.DeleteVertex(ref o);
							osub.TriPivot(ref ot);
							vertex3 = ot.Apex();
							ot.Lprev(ref o);
						}
					}
					ot.Sym(ref o);
					if (o.triangle != Mesh.dummytri)
					{
						o.LnextSelf();
						o.SegPivot(ref os);
						bool flag3 = os.seg != Mesh.dummysub;
						flag2 |= flag3;
						o.LnextSelf();
						o.SegPivot(ref os);
						bool flag4 = os.seg != Mesh.dummysub;
						flag |= flag4;
						if (!behavior.ConformingDelaunay && !flag4 && !flag3)
						{
							Vertex vertex3 = o.Org();
							while (vertex3.type == VertexType.FreeVertex && (vertex.x - vertex3.x) * (vertex2.x - vertex3.x) + (vertex.y - vertex3.y) * (vertex2.y - vertex3.y) < 0.0)
							{
								mesh.DeleteVertex(ref o);
								ot.Sym(ref o);
								vertex3 = o.Apex();
								o.LprevSelf();
							}
						}
					}
					double num3;
					if (flag | flag2)
					{
						double num = Math.Sqrt((vertex2.x - vertex.x) * (vertex2.x - vertex.x) + (vertex2.y - vertex.y) * (vertex2.y - vertex.y));
						double num2 = 1.0;
						while (num > 3.0 * num2)
						{
							num2 *= 2.0;
						}
						while (num < 1.5 * num2)
						{
							num2 *= 0.5;
						}
						num3 = num2 / num;
						if (flag2)
						{
							num3 = 1.0 - num3;
						}
					}
					else
					{
						num3 = 0.5;
					}
					Vertex vertex4 = new Vertex(vertex.x + num3 * (vertex2.x - vertex.x), vertex.y + num3 * (vertex2.y - vertex.y), osub.Mark(), mesh.nextras);
					vertex4.type = VertexType.SegmentVertex;
					vertex4.hash = mesh.hash_vtx++;
					vertex4.id = vertex4.hash;
					mesh.vertices.Add(vertex4.hash, vertex4);
					for (int i = 0; i < mesh.nextras; i++)
					{
						vertex4.attributes[i] = vertex.attributes[i] + num3 * (vertex2.attributes[i] - vertex.attributes[i]);
					}
					if (!Behavior.NoExact)
					{
						double num4 = Primitives.CounterClockwise(vertex, vertex2, vertex4);
						double num5 = (vertex.x - vertex2.x) * (vertex.x - vertex2.x) + (vertex.y - vertex2.y) * (vertex.y - vertex2.y);
						if (num4 != 0.0 && num5 != 0.0)
						{
							num4 /= num5;
							if (!double.IsNaN(num4))
							{
								vertex4.x += num4 * (vertex2.y - vertex.y);
								vertex4.y += num4 * (vertex.x - vertex2.x);
							}
						}
					}
					if ((vertex4.x == vertex.x && vertex4.y == vertex.y) || (vertex4.x == vertex2.x && vertex4.y == vertex2.y))
					{
						logger.Error("Ran out of precision: I attempted to split a segment to a smaller size than can be accommodated by the finite precision of floating point arithmetic.", "Quality.SplitEncSegs()");
						throw new Exception("Ran out of precision");
					}
					InsertVertexResult insertVertexResult = mesh.InsertVertex(vertex4, ref ot, ref osub, true, triflaws);
					if (insertVertexResult != InsertVertexResult.Successful && insertVertexResult != InsertVertexResult.Encroaching)
					{
						logger.Error("Failure to split a segment.", "Quality.SplitEncSegs()");
						throw new Exception("Failure to split a segment.");
					}
					if (mesh.steinerleft > 0)
					{
						mesh.steinerleft--;
					}
					CheckSeg4Encroach(ref osub);
					osub.NextSelf();
					CheckSeg4Encroach(ref osub);
				}
				badSubseg.subsegorg = null;
			}
		}

		private void TallyFaces()
		{
			Otri testtri = new Otri
			{
				orient = 0
			};
			foreach (Triangle value in mesh.triangles.Values)
			{
				testtri.triangle = value;
				TestTriangle(ref testtri);
			}
		}

		private void SplitTriangle(BadTriangle badtri)
		{
			Otri otri = default(Otri);
			double xi = 0.0;
			double eta = 0.0;
			otri = badtri.poortri;
			Vertex vertex = otri.Org();
			Vertex vertex2 = otri.Dest();
			Vertex vertex3 = otri.Apex();
			if (Otri.IsDead(otri.triangle) || !(vertex == badtri.triangorg) || !(vertex2 == badtri.triangdest) || !(vertex3 == badtri.triangapex))
			{
				return;
			}
			bool flag = false;
			Point point = ((!behavior.fixedArea && !behavior.VarArea) ? newLocation.FindLocation(vertex, vertex2, vertex3, ref xi, ref eta, true, otri) : Primitives.FindCircumcenter(vertex, vertex2, vertex3, ref xi, ref eta, behavior.offconstant));
			if ((point.x == vertex.x && point.y == vertex.y) || (point.x == vertex2.x && point.y == vertex2.y) || (point.x == vertex3.x && point.y == vertex3.y))
			{
				if (Behavior.Verbose)
				{
					logger.Warning("New vertex falls on existing vertex.", "Quality.SplitTriangle()");
					flag = true;
				}
			}
			else
			{
				Vertex vertex4 = new Vertex(point.x, point.y, 0, mesh.nextras);
				vertex4.type = VertexType.FreeVertex;
				for (int i = 0; i < mesh.nextras; i++)
				{
					vertex4.attributes[i] = vertex.attributes[i] + xi * (vertex2.attributes[i] - vertex.attributes[i]) + eta * (vertex3.attributes[i] - vertex.attributes[i]);
				}
				if (eta < xi)
				{
					otri.LprevSelf();
				}
				Osub splitseg = default(Osub);
				switch (mesh.InsertVertex(vertex4, ref otri, ref splitseg, true, true))
				{
				case InsertVertexResult.Successful:
					vertex4.hash = mesh.hash_vtx++;
					vertex4.id = vertex4.hash;
					mesh.vertices.Add(vertex4.hash, vertex4);
					if (mesh.steinerleft > 0)
					{
						mesh.steinerleft--;
					}
					break;
				case InsertVertexResult.Encroaching:
					mesh.UndoVertex();
					break;
				default:
					if (Behavior.Verbose)
					{
						logger.Warning("New vertex falls on existing vertex.", "Quality.SplitTriangle()");
						flag = true;
					}
					break;
				case InsertVertexResult.Violating:
					break;
				}
			}
			if (flag)
			{
				logger.Error("The new vertex is at the circumcenter of triangle: This probably means that I am trying to refine triangles to a smaller size than can be accommodated by the finite precision of floating point arithmetic.", "Quality.SplitTriangle()");
				throw new Exception("The new vertex is at the circumcenter of triangle.");
			}
		}

		public void EnforceQuality()
		{
			TallyEncs();
			SplitEncSegs(false);
			if (behavior.MinAngle > 0.0 || behavior.VarArea || behavior.fixedArea || behavior.UserTest != null)
			{
				TallyFaces();
				mesh.checkquality = true;
				while (queue.Count > 0 && mesh.steinerleft != 0)
				{
					BadTriangle badtri = queue.Dequeue();
					SplitTriangle(badtri);
					if (badsubsegs.Count > 0)
					{
						queue.Enqueue(badtri);
						SplitEncSegs(true);
					}
				}
			}
			if (Behavior.Verbose && behavior.ConformingDelaunay && badsubsegs.Count > 0 && mesh.steinerleft == 0)
			{
				logger.Warning("I ran out of Steiner points, but the mesh has encroached subsegments, and therefore might not be truly Delaunay. If the Delaunay property is important to you, try increasing the number of Steiner points.", "Quality.EnforceQuality()");
			}
		}
	}
}
