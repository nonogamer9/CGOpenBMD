using System;
using System.Collections.Generic;
using TriangleNet.Data;
using TriangleNet.Geometry;
using TriangleNet.Log;

namespace TriangleNet.IO
{
	internal static class DataReader
	{
		public static int Reconstruct(Mesh mesh, InputGeometry input, ITriangle[] triangles)
		{
			int num = 0;
			Otri newotri = default(Otri);
			Otri o = default(Otri);
			Otri otri = default(Otri);
			Otri o2 = default(Otri);
			Otri o3 = default(Otri);
			Osub newsubseg = default(Osub);
			int[] array = new int[3];
			int[] array2 = new int[2];
			int num2 = 0;
			int inelements = ((triangles != null) ? triangles.Length : 0);
			int count = input.segments.Count;
			mesh.inelements = inelements;
			mesh.regions.AddRange(input.regions);
			for (num2 = 0; num2 < mesh.inelements; num2++)
			{
				mesh.MakeTriangle(ref newotri);
			}
			if (mesh.behavior.Poly)
			{
				mesh.insegments = count;
				for (num2 = 0; num2 < mesh.insegments; num2++)
				{
					mesh.MakeSegment(ref newsubseg);
				}
			}
			List<Otri>[] array3 = new List<Otri>[mesh.vertices.Count];
			for (num2 = 0; num2 < mesh.vertices.Count; num2++)
			{
				Otri item = new Otri
				{
					triangle = Mesh.dummytri
				};
				array3[num2] = new List<Otri>(3);
				array3[num2].Add(item);
			}
			num2 = 0;
			foreach (Triangle value in mesh.triangles.Values)
			{
				newotri.triangle = value;
				array[0] = triangles[num2].P0;
				array[1] = triangles[num2].P1;
				array[2] = triangles[num2].P2;
				for (int i = 0; i < 3; i++)
				{
					if (array[i] < 0 || array[i] >= mesh.invertices)
					{
						SimpleLog.Instance.Error("Triangle has an invalid vertex index.", "MeshReader.Reconstruct()");
						throw new Exception("Triangle has an invalid vertex index.");
					}
				}
				newotri.triangle.region = triangles[num2].Region;
				if (mesh.behavior.VarArea)
				{
					newotri.triangle.area = triangles[num2].Area;
				}
				newotri.orient = 0;
				newotri.SetOrg(mesh.vertices[array[0]]);
				newotri.SetDest(mesh.vertices[array[1]]);
				newotri.SetApex(mesh.vertices[array[2]]);
				newotri.orient = 0;
				while (newotri.orient < 3)
				{
					int num3 = array[newotri.orient];
					int num4 = array3[num3].Count - 1;
					Otri otri2 = array3[num3][num4];
					array3[num3].Add(newotri);
					otri = otri2;
					if (otri.triangle != Mesh.dummytri)
					{
						Vertex vertex = newotri.Dest();
						Vertex vertex2 = newotri.Apex();
						do
						{
							Vertex vertex3 = otri.Dest();
							Vertex vertex4 = otri.Apex();
							if (vertex2 == vertex3)
							{
								newotri.Lprev(ref o);
								o.Bond(ref otri);
							}
							if (vertex == vertex4)
							{
								otri.Lprev(ref o2);
								newotri.Bond(ref o2);
							}
							num4--;
							otri2 = array3[num3][num4];
							otri = otri2;
						}
						while (otri.triangle != Mesh.dummytri);
					}
					newotri.orient++;
				}
				num2++;
			}
			num = 0;
			if (mesh.behavior.Poly)
			{
				int num5 = 0;
				num2 = 0;
				foreach (Segment value2 in mesh.subsegs.Values)
				{
					newsubseg.seg = value2;
					array2[0] = input.segments[num2].P0;
					array2[1] = input.segments[num2].P1;
					num5 = input.segments[num2].Boundary;
					for (int j = 0; j < 2; j++)
					{
						if (array2[j] < 0 || array2[j] >= mesh.invertices)
						{
							SimpleLog.Instance.Error("Segment has an invalid vertex index.", "MeshReader.Reconstruct()");
							throw new Exception("Segment has an invalid vertex index.");
						}
					}
					newsubseg.orient = 0;
					Vertex vertex5 = mesh.vertices[array2[0]];
					Vertex vertex6 = mesh.vertices[array2[1]];
					newsubseg.SetOrg(vertex5);
					newsubseg.SetDest(vertex6);
					newsubseg.SetSegOrg(vertex5);
					newsubseg.SetSegDest(vertex6);
					newsubseg.seg.boundary = num5;
					newsubseg.orient = 0;
					while (newsubseg.orient < 2)
					{
						int num3 = array2[1 - newsubseg.orient];
						int num6 = array3[num3].Count - 1;
						Otri item2 = array3[num3][num6];
						Otri otri2 = array3[num3][num6];
						otri = otri2;
						Vertex vertex7 = newsubseg.Org();
						bool flag = true;
						while (flag && otri.triangle != Mesh.dummytri)
						{
							Vertex vertex3 = otri.Dest();
							if (vertex7 == vertex3)
							{
								array3[num3].Remove(item2);
								otri.SegBond(ref newsubseg);
								otri.Sym(ref o3);
								if (o3.triangle == Mesh.dummytri)
								{
									mesh.InsertSubseg(ref otri, 1);
									num++;
								}
								flag = false;
							}
							num6--;
							item2 = array3[num3][num6];
							otri2 = array3[num3][num6];
							otri = otri2;
						}
						newsubseg.orient++;
					}
					num2++;
				}
			}
			for (num2 = 0; num2 < mesh.vertices.Count; num2++)
			{
				int num7 = array3[num2].Count - 1;
				Otri otri2 = array3[num2][num7];
				otri = otri2;
				while (otri.triangle != Mesh.dummytri)
				{
					num7--;
					otri2 = array3[num2][num7];
					otri.SegDissolve();
					otri.Sym(ref o3);
					if (o3.triangle == Mesh.dummytri)
					{
						mesh.InsertSubseg(ref otri, 1);
						num++;
					}
					otri = otri2;
				}
			}
			return num;
		}
	}
}
