using System;
using System.Collections.Generic;
using TriangleNet.Algorithm;
using TriangleNet.Data;
using TriangleNet.Geometry;
using TriangleNet.IO;
using TriangleNet.Log;
using TriangleNet.Smoothing;
using TriangleNet.Tools;

namespace TriangleNet
{
	public class Mesh
	{
		private ILog<SimpleLogItem> logger;

		private QualityMesher quality;

		private Stack<Otri> flipstack;

		internal Dictionary<int, Triangle> triangles;

		internal Dictionary<int, Segment> subsegs;

		internal Dictionary<int, Vertex> vertices;

		internal int hash_vtx;

		internal int hash_seg;

		internal int hash_tri;

		internal List<Point> holes;

		internal List<RegionPointer> regions;

		internal BoundingBox bounds;

		internal int invertices;

		internal int inelements;

		internal int insegments;

		internal int undeads;

		internal int edges;

		internal int mesh_dim;

		internal int nextras;

		internal int hullsize;

		internal int steinerleft;

		internal bool checksegments;

		internal bool checkquality;

		internal Vertex infvertex1;

		internal Vertex infvertex2;

		internal Vertex infvertex3;

		internal static Triangle dummytri;

		internal static Segment dummysub;

		internal TriangleLocator locator;

		internal Behavior behavior;

		internal NodeNumbering numbering;

		public Behavior Behavior
		{
			get
			{
				return behavior;
			}
		}

		public BoundingBox Bounds
		{
			get
			{
				return bounds;
			}
		}

		public ICollection<Vertex> Vertices
		{
			get
			{
				return vertices.Values;
			}
		}

		public IList<Point> Holes
		{
			get
			{
				return holes;
			}
		}

		public ICollection<Triangle> Triangles
		{
			get
			{
				return triangles.Values;
			}
		}

		public ICollection<Segment> Segments
		{
			get
			{
				return subsegs.Values;
			}
		}

		public IEnumerable<Edge> Edges
		{
			get
			{
				EdgeEnumerator e = new EdgeEnumerator(this);
				while (e.MoveNext())
				{
					yield return e.Current;
				}
			}
		}

		public int NumberOfInputPoints
		{
			get
			{
				return invertices;
			}
		}

		public int NumberOfEdges
		{
			get
			{
				return edges;
			}
		}

		public bool IsPolygon
		{
			get
			{
				return insegments > 0;
			}
		}

		public NodeNumbering CurrentNumbering
		{
			get
			{
				return numbering;
			}
		}

		public Mesh()
			: this(new Behavior())
		{
		}

		public Mesh(Behavior behavior)
		{
			this.behavior = behavior;
			logger = SimpleLog.Instance;
			vertices = new Dictionary<int, Vertex>();
			triangles = new Dictionary<int, Triangle>();
			subsegs = new Dictionary<int, Segment>();
			flipstack = new Stack<Otri>();
			holes = new List<Point>();
			regions = new List<RegionPointer>();
			quality = new QualityMesher(this);
			locator = new TriangleLocator(this);
			Primitives.ExactInit();
			if (dummytri == null)
			{
				DummyInit();
			}
		}

		public void Load(string filename)
		{
			InputGeometry geometry;
			List<ITriangle> list;
			FileReader.Read(filename, out geometry, out list);
			if (geometry != null && list != null)
			{
				Load(geometry, list);
			}
		}

		public void Load(InputGeometry input, List<ITriangle> triangles)
		{
			if (input == null || triangles == null)
			{
				throw new ArgumentException("Invalid input (argument is null).");
			}
			ResetData();
			if (input.HasSegments)
			{
				behavior.Poly = true;
				holes.AddRange(input.Holes);
			}
			if (!behavior.Poly)
			{
				behavior.VarArea = false;
				behavior.useRegions = false;
			}
			behavior.useRegions = input.Regions.Count > 0;
			TransferNodes(input);
			hullsize = DataReader.Reconstruct(this, input, triangles.ToArray());
			edges = (3 * triangles.Count + hullsize) / 2;
		}

		public void Triangulate(string inputFile)
		{
			InputGeometry input = FileReader.Read(inputFile);
			Triangulate(input);
		}

		public void Triangulate(InputGeometry input)
		{
			ResetData();
			behavior.Poly = input.HasSegments;
			if (!behavior.Poly)
			{
				behavior.VarArea = false;
				behavior.useRegions = false;
			}
			behavior.useRegions = input.Regions.Count > 0;
			steinerleft = behavior.SteinerPoints;
			TransferNodes(input);
			hullsize = Delaunay();
			infvertex1 = null;
			infvertex2 = null;
			infvertex3 = null;
			ConstraintMesher constraintMesher = new ConstraintMesher(this);
			if (behavior.useSegments)
			{
				checksegments = true;
				constraintMesher.FormSkeleton(input);
			}
			if (behavior.Poly && triangles.Count > 0)
			{
				foreach (Point hole in input.holes)
				{
					holes.Add(hole);
				}
				foreach (RegionPointer region in input.regions)
				{
					regions.Add(region);
				}
				constraintMesher.CarveHoles();
			}
			else
			{
				holes.Clear();
				regions.Clear();
			}
			if ((behavior.Quality || behavior.ConformingDelaunay) && triangles.Count > 0)
			{
				quality.EnforceQuality();
			}
			edges = (3 * triangles.Count + hullsize) / 2;
		}

		public void Refine(bool halfArea)
		{
			if (halfArea)
			{
				double num = 0.0;
				foreach (Triangle value2 in triangles.Values)
				{
					double value = (value2.vertices[2].x - value2.vertices[0].x) * (value2.vertices[1].y - value2.vertices[0].y) - (value2.vertices[1].x - value2.vertices[0].x) * (value2.vertices[2].y - value2.vertices[0].y);
					value = Math.Abs(value) / 2.0;
					if (value > num)
					{
						num = value;
					}
				}
				Refine(num / 2.0);
			}
			else
			{
				Refine();
			}
		}

		public void Refine(double areaConstraint)
		{
			behavior.fixedArea = true;
			behavior.MaxArea = areaConstraint;
			Refine();
			behavior.fixedArea = false;
			behavior.MaxArea = -1.0;
		}

		public void Refine()
		{
			inelements = triangles.Count;
			invertices = vertices.Count;
			if (behavior.Poly)
			{
				if (behavior.useSegments)
				{
					insegments = subsegs.Count;
				}
				else
				{
					insegments = hullsize;
				}
			}
			Reset();
			steinerleft = behavior.SteinerPoints;
			infvertex1 = null;
			infvertex2 = null;
			infvertex3 = null;
			if (behavior.useSegments)
			{
				checksegments = true;
			}
			if (triangles.Count > 0)
			{
				quality.EnforceQuality();
			}
			edges = (3 * triangles.Count + hullsize) / 2;
		}

		public void Smooth()
		{
			numbering = NodeNumbering.None;
			((ISmoother)new SimpleSmoother(this)).Smooth();
		}

		public void Renumber()
		{
			Renumber(NodeNumbering.Linear);
		}

		public void Renumber(NodeNumbering num)
		{
			if (num == numbering)
			{
				return;
			}
			int num2;
			switch (num)
			{
			case NodeNumbering.Linear:
				num2 = 0;
				foreach (Vertex value in vertices.Values)
				{
					value.id = num2++;
				}
				break;
			case NodeNumbering.CuthillMcKee:
			{
				int[] array = new CuthillMcKee().Renumber(this);
				foreach (Vertex value2 in vertices.Values)
				{
					value2.id = array[value2.id];
				}
				break;
			}
			}
			numbering = num;
			num2 = 0;
			foreach (Triangle value3 in triangles.Values)
			{
				value3.id = num2++;
			}
		}

		private int Delaunay()
		{
			int num = 0;
			num = ((behavior.Algorithm == TriangulationAlgorithm.Dwyer) ? new Dwyer().Triangulate(this) : ((behavior.Algorithm != TriangulationAlgorithm.SweepLine) ? new Incremental().Triangulate(this) : new SweepLine().Triangulate(this)));
			if (triangles.Count != 0)
			{
				return num;
			}
			return 0;
		}

		private void ResetData()
		{
			vertices.Clear();
			triangles.Clear();
			subsegs.Clear();
			holes.Clear();
			regions.Clear();
			hash_vtx = 0;
			hash_seg = 0;
			hash_tri = 0;
			flipstack.Clear();
			hullsize = 0;
			edges = 0;
			Reset();
			locator.Reset();
		}

		private void Reset()
		{
			numbering = NodeNumbering.None;
			undeads = 0;
			checksegments = false;
			checkquality = false;
			Statistic.InCircleCount = 0L;
			Statistic.CounterClockwiseCount = 0L;
			Statistic.InCircleAdaptCount = 0L;
			Statistic.CounterClockwiseAdaptCount = 0L;
			Statistic.Orient3dCount = 0L;
			Statistic.HyperbolaCount = 0L;
			Statistic.CircleTopCount = 0L;
			Statistic.CircumcenterCount = 0L;
		}

		private void DummyInit()
		{
			dummytri = new Triangle();
			dummytri.hash = -1;
			dummytri.id = -1;
			dummytri.neighbors[0].triangle = dummytri;
			dummytri.neighbors[1].triangle = dummytri;
			dummytri.neighbors[2].triangle = dummytri;
			if (behavior.useSegments)
			{
				dummysub = new Segment();
				dummysub.hash = -1;
				dummysub.subsegs[0].seg = dummysub;
				dummysub.subsegs[1].seg = dummysub;
				dummytri.subsegs[0].seg = dummysub;
				dummytri.subsegs[1].seg = dummysub;
				dummytri.subsegs[2].seg = dummysub;
			}
		}

		private void TransferNodes(InputGeometry data)
		{
			List<Vertex> points = data.points;
			invertices = points.Count;
			mesh_dim = 2;
			if (invertices < 3)
			{
				logger.Error("Input must have at least three input vertices.", "MeshReader.TransferNodes()");
				throw new Exception("Input must have at least three input vertices.");
			}
			nextras = ((points[0].attributes != null) ? points[0].attributes.Length : 0);
			foreach (Vertex item in points)
			{
				item.hash = hash_vtx++;
				item.id = item.hash;
				vertices.Add(item.hash, item);
			}
			bounds = data.Bounds;
		}

		internal void MakeVertexMap()
		{
			Otri tri = default(Otri);
			foreach (Triangle value in triangles.Values)
			{
				tri.triangle = value;
				tri.orient = 0;
				while (tri.orient < 3)
				{
					tri.Org().tri = tri;
					tri.orient++;
				}
			}
		}

		internal void MakeTriangle(ref Otri newotri)
		{
			Triangle triangle = new Triangle();
			triangle.hash = hash_tri++;
			triangle.id = triangle.hash;
			newotri.triangle = triangle;
			newotri.orient = 0;
			triangles.Add(triangle.hash, triangle);
		}

		internal void MakeSegment(ref Osub newsubseg)
		{
			Segment segment = new Segment();
			segment.hash = hash_seg++;
			newsubseg.seg = segment;
			newsubseg.orient = 0;
			subsegs.Add(segment.hash, segment);
		}

		internal InsertVertexResult InsertVertex(Vertex newvertex, ref Otri searchtri, ref Osub splitseg, bool segmentflaws, bool triflaws)
		{
			Otri o = default(Otri);
			Otri o2 = default(Otri);
			Otri o3 = default(Otri);
			Otri o4 = default(Otri);
			Otri o5 = default(Otri);
			Otri o6 = default(Otri);
			Otri newotri = default(Otri);
			Otri newotri2 = default(Otri);
			Otri newotri3 = default(Otri);
			Otri o7 = default(Otri);
			Otri o8 = default(Otri);
			Otri o9 = default(Otri);
			Otri o10 = default(Otri);
			Otri o11 = default(Otri);
			Osub os = default(Osub);
			Osub os2 = default(Osub);
			Osub os3 = default(Osub);
			Osub os4 = default(Osub);
			Osub os5 = default(Osub);
			Osub os6 = default(Osub);
			Osub o12 = default(Osub);
			Osub os7 = default(Osub);
			LocateResult locateResult;
			if (splitseg.seg == null)
			{
				if (searchtri.triangle == dummytri)
				{
					o.triangle = dummytri;
					o.orient = 0;
					o.SymSelf();
					locateResult = locator.Locate(newvertex, ref o);
				}
				else
				{
					searchtri.Copy(ref o);
					locateResult = locator.PreciseLocate(newvertex, ref o, true);
				}
			}
			else
			{
				searchtri.Copy(ref o);
				locateResult = LocateResult.OnEdge;
			}
			Vertex dest;
			Vertex org;
			switch (locateResult)
			{
			case LocateResult.OnVertex:
				o.Copy(ref searchtri);
				locator.Update(ref o);
				return InsertVertexResult.Duplicate;
			case LocateResult.OnEdge:
			case LocateResult.Outside:
			{
				if (checksegments && splitseg.seg == null)
				{
					o.SegPivot(ref os5);
					if (os5.seg != dummysub)
					{
						if (segmentflaws)
						{
							bool flag = behavior.NoBisect != 2;
							if (flag && behavior.NoBisect == 1)
							{
								o.Sym(ref o11);
								flag = o11.triangle != dummytri;
							}
							if (flag)
							{
								BadSubseg badSubseg = new BadSubseg();
								badSubseg.encsubseg = os5;
								badSubseg.subsegorg = os5.Org();
								badSubseg.subsegdest = os5.Dest();
								quality.AddBadSubseg(badSubseg);
							}
						}
						o.Copy(ref searchtri);
						locator.Update(ref o);
						return InsertVertexResult.Violating;
					}
				}
				o.Lprev(ref o4);
				o4.Sym(ref o8);
				o.Sym(ref o6);
				bool flag2 = o6.triangle != dummytri;
				if (flag2)
				{
					o6.LnextSelf();
					o6.Sym(ref o10);
					MakeTriangle(ref newotri3);
				}
				else
				{
					hullsize++;
				}
				MakeTriangle(ref newotri2);
				dest = o.Org();
				org = o.Dest();
				Vertex vertex = o.Apex();
				newotri2.SetOrg(vertex);
				newotri2.SetDest(dest);
				newotri2.SetApex(newvertex);
				o.SetOrg(newvertex);
				newotri2.triangle.region = o4.triangle.region;
				if (behavior.VarArea)
				{
					newotri2.triangle.area = o4.triangle.area;
				}
				if (flag2)
				{
					Vertex dest2 = o6.Dest();
					newotri3.SetOrg(dest);
					newotri3.SetDest(dest2);
					newotri3.SetApex(newvertex);
					o6.SetOrg(newvertex);
					newotri3.triangle.region = o6.triangle.region;
					if (behavior.VarArea)
					{
						newotri3.triangle.area = o6.triangle.area;
					}
				}
				if (checksegments)
				{
					o4.SegPivot(ref os2);
					if (os2.seg != dummysub)
					{
						o4.SegDissolve();
						newotri2.SegBond(ref os2);
					}
					if (flag2)
					{
						o6.SegPivot(ref os4);
						if (os4.seg != dummysub)
						{
							o6.SegDissolve();
							newotri3.SegBond(ref os4);
						}
					}
				}
				newotri2.Bond(ref o8);
				newotri2.LprevSelf();
				newotri2.Bond(ref o4);
				newotri2.LprevSelf();
				if (flag2)
				{
					newotri3.Bond(ref o10);
					newotri3.LnextSelf();
					newotri3.Bond(ref o6);
					newotri3.LnextSelf();
					newotri3.Bond(ref newotri2);
				}
				if (splitseg.seg != null)
				{
					splitseg.SetDest(newvertex);
					Vertex segOrg = splitseg.SegOrg();
					Vertex segDest = splitseg.SegDest();
					splitseg.SymSelf();
					splitseg.Pivot(ref o12);
					InsertSubseg(ref newotri2, splitseg.seg.boundary);
					newotri2.SegPivot(ref os7);
					os7.SetSegOrg(segOrg);
					os7.SetSegDest(segDest);
					splitseg.Bond(ref os7);
					os7.SymSelf();
					os7.Bond(ref o12);
					splitseg.SymSelf();
					if (newvertex.mark == 0)
					{
						newvertex.mark = splitseg.seg.boundary;
					}
				}
				if (checkquality)
				{
					flipstack.Clear();
					flipstack.Push(default(Otri));
					flipstack.Push(o);
				}
				o.LnextSelf();
				break;
			}
			default:
			{
				o.Lnext(ref o3);
				o.Lprev(ref o4);
				o3.Sym(ref o7);
				o4.Sym(ref o8);
				MakeTriangle(ref newotri);
				MakeTriangle(ref newotri2);
				dest = o.Org();
				org = o.Dest();
				Vertex vertex = o.Apex();
				newotri.SetOrg(org);
				newotri.SetDest(vertex);
				newotri.SetApex(newvertex);
				newotri2.SetOrg(vertex);
				newotri2.SetDest(dest);
				newotri2.SetApex(newvertex);
				o.SetApex(newvertex);
				newotri.triangle.region = o.triangle.region;
				newotri2.triangle.region = o.triangle.region;
				if (behavior.VarArea)
				{
					double area = o.triangle.area;
					newotri.triangle.area = area;
					newotri2.triangle.area = area;
				}
				if (checksegments)
				{
					o3.SegPivot(ref os);
					if (os.seg != dummysub)
					{
						o3.SegDissolve();
						newotri.SegBond(ref os);
					}
					o4.SegPivot(ref os2);
					if (os2.seg != dummysub)
					{
						o4.SegDissolve();
						newotri2.SegBond(ref os2);
					}
				}
				newotri.Bond(ref o7);
				newotri2.Bond(ref o8);
				newotri.LnextSelf();
				newotri2.LprevSelf();
				newotri.Bond(ref newotri2);
				newotri.LnextSelf();
				o3.Bond(ref newotri);
				newotri2.LprevSelf();
				o4.Bond(ref newotri2);
				if (checkquality)
				{
					flipstack.Clear();
					flipstack.Push(o);
				}
				break;
			}
			}
			InsertVertexResult result = InsertVertexResult.Successful;
			Vertex vertex2 = o.Org();
			dest = vertex2;
			org = o.Dest();
			while (true)
			{
				bool flag3 = true;
				if (checksegments)
				{
					o.SegPivot(ref os6);
					if (os6.seg != dummysub)
					{
						flag3 = false;
						if (segmentflaws && quality.CheckSeg4Encroach(ref os6) > 0)
						{
							result = InsertVertexResult.Encroaching;
						}
					}
				}
				if (flag3)
				{
					o.Sym(ref o2);
					if (o2.triangle == dummytri)
					{
						flag3 = false;
					}
					else
					{
						Vertex vertex3 = o2.Apex();
						flag3 = ((!(org == infvertex1) && !(org == infvertex2) && !(org == infvertex3)) ? ((!(dest == infvertex1) && !(dest == infvertex2) && !(dest == infvertex3)) ? (!(vertex3 == infvertex1) && !(vertex3 == infvertex2) && !(vertex3 == infvertex3) && Primitives.InCircle(org, newvertex, dest, vertex3) > 0.0) : (Primitives.CounterClockwise(vertex3, org, newvertex) > 0.0)) : (Primitives.CounterClockwise(newvertex, dest, vertex3) > 0.0));
						if (flag3)
						{
							o2.Lprev(ref o5);
							o5.Sym(ref o9);
							o2.Lnext(ref o6);
							o6.Sym(ref o10);
							o.Lnext(ref o3);
							o3.Sym(ref o7);
							o.Lprev(ref o4);
							o4.Sym(ref o8);
							o5.Bond(ref o7);
							o3.Bond(ref o8);
							o4.Bond(ref o10);
							o6.Bond(ref o9);
							if (checksegments)
							{
								o5.SegPivot(ref os3);
								o3.SegPivot(ref os);
								o4.SegPivot(ref os2);
								o6.SegPivot(ref os4);
								if (os3.seg == dummysub)
								{
									o6.SegDissolve();
								}
								else
								{
									o6.SegBond(ref os3);
								}
								if (os.seg == dummysub)
								{
									o5.SegDissolve();
								}
								else
								{
									o5.SegBond(ref os);
								}
								if (os2.seg == dummysub)
								{
									o3.SegDissolve();
								}
								else
								{
									o3.SegBond(ref os2);
								}
								if (os4.seg == dummysub)
								{
									o4.SegDissolve();
								}
								else
								{
									o4.SegBond(ref os4);
								}
							}
							o.SetOrg(vertex3);
							o.SetDest(newvertex);
							o.SetApex(dest);
							o2.SetOrg(newvertex);
							o2.SetDest(vertex3);
							o2.SetApex(org);
							int region = Math.Min(o2.triangle.region, o.triangle.region);
							o2.triangle.region = region;
							o.triangle.region = region;
							if (behavior.VarArea)
							{
								double area = ((!(o2.triangle.area <= 0.0) && !(o.triangle.area <= 0.0)) ? (0.5 * (o2.triangle.area + o.triangle.area)) : (-1.0));
								o2.triangle.area = area;
								o.triangle.area = area;
							}
							if (checkquality)
							{
								flipstack.Push(o);
							}
							o.LprevSelf();
							org = vertex3;
						}
					}
				}
				if (!flag3)
				{
					if (triflaws)
					{
						quality.TestTriangle(ref o);
					}
					o.LnextSelf();
					o.Sym(ref o11);
					if (org == vertex2 || o11.triangle == dummytri)
					{
						break;
					}
					o11.Lnext(ref o);
					dest = org;
					org = o.Dest();
				}
			}
			o.Lnext(ref searchtri);
			Otri o13 = default(Otri);
			o.Lnext(ref o13);
			locator.Update(ref o13);
			return result;
		}

		internal void InsertSubseg(ref Otri tri, int subsegmark)
		{
			Otri o = default(Otri);
			Osub os = default(Osub);
			Vertex vertex = tri.Org();
			Vertex vertex2 = tri.Dest();
			if (vertex.mark == 0)
			{
				vertex.mark = subsegmark;
			}
			if (vertex2.mark == 0)
			{
				vertex2.mark = subsegmark;
			}
			tri.SegPivot(ref os);
			if (os.seg == dummysub)
			{
				MakeSegment(ref os);
				os.SetOrg(vertex2);
				os.SetDest(vertex);
				os.SetSegOrg(vertex2);
				os.SetSegDest(vertex);
				tri.SegBond(ref os);
				tri.Sym(ref o);
				os.SymSelf();
				o.SegBond(ref os);
				os.seg.boundary = subsegmark;
			}
			else if (os.seg.boundary == 0)
			{
				os.seg.boundary = subsegmark;
			}
		}

		internal void Flip(ref Otri flipedge)
		{
			Otri o = default(Otri);
			Otri o2 = default(Otri);
			Otri o3 = default(Otri);
			Otri o4 = default(Otri);
			Otri o5 = default(Otri);
			Otri o6 = default(Otri);
			Otri o7 = default(Otri);
			Otri o8 = default(Otri);
			Otri o9 = default(Otri);
			Osub os = default(Osub);
			Osub os2 = default(Osub);
			Osub os3 = default(Osub);
			Osub os4 = default(Osub);
			Vertex apex = flipedge.Org();
			Vertex apex2 = flipedge.Dest();
			Vertex vertex = flipedge.Apex();
			flipedge.Sym(ref o5);
			Vertex vertex2 = o5.Apex();
			o5.Lprev(ref o3);
			o3.Sym(ref o8);
			o5.Lnext(ref o4);
			o4.Sym(ref o9);
			flipedge.Lnext(ref o);
			o.Sym(ref o6);
			flipedge.Lprev(ref o2);
			o2.Sym(ref o7);
			o3.Bond(ref o6);
			o.Bond(ref o7);
			o2.Bond(ref o9);
			o4.Bond(ref o8);
			if (checksegments)
			{
				o3.SegPivot(ref os3);
				o.SegPivot(ref os);
				o2.SegPivot(ref os2);
				o4.SegPivot(ref os4);
				if (os3.seg == dummysub)
				{
					o4.SegDissolve();
				}
				else
				{
					o4.SegBond(ref os3);
				}
				if (os.seg == dummysub)
				{
					o3.SegDissolve();
				}
				else
				{
					o3.SegBond(ref os);
				}
				if (os2.seg == dummysub)
				{
					o.SegDissolve();
				}
				else
				{
					o.SegBond(ref os2);
				}
				if (os4.seg == dummysub)
				{
					o2.SegDissolve();
				}
				else
				{
					o2.SegBond(ref os4);
				}
			}
			flipedge.SetOrg(vertex2);
			flipedge.SetDest(vertex);
			flipedge.SetApex(apex);
			o5.SetOrg(vertex);
			o5.SetDest(vertex2);
			o5.SetApex(apex2);
		}

		internal void Unflip(ref Otri flipedge)
		{
			Otri o = default(Otri);
			Otri o2 = default(Otri);
			Otri o3 = default(Otri);
			Otri o4 = default(Otri);
			Otri o5 = default(Otri);
			Otri o6 = default(Otri);
			Otri o7 = default(Otri);
			Otri o8 = default(Otri);
			Otri o9 = default(Otri);
			Osub os = default(Osub);
			Osub os2 = default(Osub);
			Osub os3 = default(Osub);
			Osub os4 = default(Osub);
			Vertex apex = flipedge.Org();
			Vertex apex2 = flipedge.Dest();
			Vertex vertex = flipedge.Apex();
			flipedge.Sym(ref o5);
			Vertex vertex2 = o5.Apex();
			o5.Lprev(ref o3);
			o3.Sym(ref o8);
			o5.Lnext(ref o4);
			o4.Sym(ref o9);
			flipedge.Lnext(ref o);
			o.Sym(ref o6);
			flipedge.Lprev(ref o2);
			o2.Sym(ref o7);
			o3.Bond(ref o9);
			o.Bond(ref o8);
			o2.Bond(ref o6);
			o4.Bond(ref o7);
			if (checksegments)
			{
				o3.SegPivot(ref os3);
				o.SegPivot(ref os);
				o2.SegPivot(ref os2);
				o4.SegPivot(ref os4);
				if (os3.seg == dummysub)
				{
					o.SegDissolve();
				}
				else
				{
					o.SegBond(ref os3);
				}
				if (os.seg == dummysub)
				{
					o2.SegDissolve();
				}
				else
				{
					o2.SegBond(ref os);
				}
				if (os2.seg == dummysub)
				{
					o4.SegDissolve();
				}
				else
				{
					o4.SegBond(ref os2);
				}
				if (os4.seg == dummysub)
				{
					o3.SegDissolve();
				}
				else
				{
					o3.SegBond(ref os4);
				}
			}
			flipedge.SetOrg(vertex);
			flipedge.SetDest(vertex2);
			flipedge.SetApex(apex2);
			o5.SetOrg(vertex2);
			o5.SetDest(vertex);
			o5.SetApex(apex);
		}

		private void TriangulatePolygon(Otri firstedge, Otri lastedge, int edgecount, bool doflip, bool triflaws)
		{
			Otri o = default(Otri);
			Otri o2 = default(Otri);
			Otri o3 = default(Otri);
			int num = 1;
			Vertex pa = lastedge.Apex();
			Vertex pb = firstedge.Dest();
			firstedge.Onext(ref o2);
			Vertex pc = o2.Dest();
			o2.Copy(ref o);
			for (int i = 2; i <= edgecount - 2; i++)
			{
				o.OnextSelf();
				Vertex vertex = o.Dest();
				if (Primitives.InCircle(pa, pb, pc, vertex) > 0.0)
				{
					o.Copy(ref o2);
					pc = vertex;
					num = i;
				}
			}
			if (num > 1)
			{
				o2.Oprev(ref o3);
				TriangulatePolygon(firstedge, o3, num + 1, true, triflaws);
			}
			if (num < edgecount - 2)
			{
				o2.Sym(ref o3);
				TriangulatePolygon(o2, lastedge, edgecount - num, true, triflaws);
				o3.Sym(ref o2);
			}
			if (doflip)
			{
				Flip(ref o2);
				if (triflaws)
				{
					o2.Sym(ref o);
					quality.TestTriangle(ref o);
				}
			}
			o2.Copy(ref lastedge);
		}

		internal void DeleteVertex(ref Otri deltri)
		{
			Otri o = default(Otri);
			Otri o2 = default(Otri);
			Otri o3 = default(Otri);
			Otri o4 = default(Otri);
			Otri o5 = default(Otri);
			Otri o6 = default(Otri);
			Otri o7 = default(Otri);
			Otri o8 = default(Otri);
			Osub os = default(Osub);
			Osub os2 = default(Osub);
			Vertex dyingvertex = deltri.Org();
			VertexDealloc(dyingvertex);
			deltri.Onext(ref o);
			int num = 1;
			while (!deltri.Equal(o))
			{
				num++;
				o.OnextSelf();
			}
			if (num > 3)
			{
				deltri.Onext(ref o2);
				deltri.Oprev(ref o3);
				TriangulatePolygon(o2, o3, num, false, behavior.NoBisect == 0);
			}
			deltri.Lprev(ref o4);
			deltri.Dnext(ref o5);
			o5.Sym(ref o7);
			o4.Oprev(ref o6);
			o6.Sym(ref o8);
			deltri.Bond(ref o7);
			o4.Bond(ref o8);
			o5.SegPivot(ref os);
			if (os.seg != dummysub)
			{
				deltri.SegBond(ref os);
			}
			o6.SegPivot(ref os2);
			if (os2.seg != dummysub)
			{
				o4.SegBond(ref os2);
			}
			Vertex org = o5.Org();
			deltri.SetOrg(org);
			if (behavior.NoBisect == 0)
			{
				quality.TestTriangle(ref deltri);
			}
			TriangleDealloc(o5.triangle);
			TriangleDealloc(o6.triangle);
		}

		internal void UndoVertex()
		{
			Otri o = default(Otri);
			Otri o2 = default(Otri);
			Otri o3 = default(Otri);
			Otri o4 = default(Otri);
			Otri o5 = default(Otri);
			Otri o6 = default(Otri);
			Otri o7 = default(Otri);
			Osub os = default(Osub);
			Osub os2 = default(Osub);
			Osub os3 = default(Osub);
			while (flipstack.Count > 0)
			{
				Otri flipedge = flipstack.Pop();
				if (flipstack.Count == 0)
				{
					flipedge.Dprev(ref o);
					o.LnextSelf();
					flipedge.Onext(ref o2);
					o2.LprevSelf();
					o.Sym(ref o4);
					o2.Sym(ref o5);
					Vertex apex = o.Dest();
					flipedge.SetApex(apex);
					flipedge.LnextSelf();
					flipedge.Bond(ref o4);
					o.SegPivot(ref os);
					flipedge.SegBond(ref os);
					flipedge.LnextSelf();
					flipedge.Bond(ref o5);
					o2.SegPivot(ref os2);
					flipedge.SegBond(ref os2);
					TriangleDealloc(o.triangle);
					TriangleDealloc(o2.triangle);
				}
				else if (flipstack.Peek().triangle == null)
				{
					flipedge.Lprev(ref o7);
					o7.Sym(ref o2);
					o2.LnextSelf();
					o2.Sym(ref o5);
					Vertex org = o2.Dest();
					flipedge.SetOrg(org);
					o7.Bond(ref o5);
					o2.SegPivot(ref os2);
					o7.SegBond(ref os2);
					TriangleDealloc(o2.triangle);
					flipedge.Sym(ref o7);
					if (o7.triangle != dummytri)
					{
						o7.LnextSelf();
						o7.Dnext(ref o3);
						o3.Sym(ref o6);
						o7.SetOrg(org);
						o7.Bond(ref o6);
						o3.SegPivot(ref os3);
						o7.SegBond(ref os3);
						TriangleDealloc(o3.triangle);
					}
					flipstack.Clear();
				}
				else
				{
					Unflip(ref flipedge);
				}
			}
		}

		internal void TriangleDealloc(Triangle dyingtriangle)
		{
			Otri.Kill(dyingtriangle);
			triangles.Remove(dyingtriangle.hash);
		}

		internal void VertexDealloc(Vertex dyingvertex)
		{
			dyingvertex.type = VertexType.DeadVertex;
			vertices.Remove(dyingvertex.hash);
		}

		internal void SubsegDealloc(Segment dyingsubseg)
		{
			Osub.Kill(dyingsubseg);
			subsegs.Remove(dyingsubseg.hash);
		}
	}
}
