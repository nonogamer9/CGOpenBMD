using System;
using TriangleNet.Data;
using TriangleNet.Log;

namespace TriangleNet.Algorithm
{
	internal class Dwyer
	{
		private static Random rand = new Random(DateTime.Now.Millisecond);

		private bool useDwyer = true;

		private Vertex[] sortarray;

		private Mesh mesh;

		private void VertexSort(int left, int right)
		{
			int num = left;
			int num2 = right;
			if (right - left + 1 < 32)
			{
				for (int i = left + 1; i <= right; i++)
				{
					Vertex vertex = sortarray[i];
					int num3 = i - 1;
					while (num3 >= left && (sortarray[num3].x > vertex.x || (sortarray[num3].x == vertex.x && sortarray[num3].y > vertex.y)))
					{
						sortarray[num3 + 1] = sortarray[num3];
						num3--;
					}
					sortarray[num3 + 1] = vertex;
				}
				return;
			}
			int num4 = rand.Next(left, right);
			double x = sortarray[num4].x;
			double y = sortarray[num4].y;
			left--;
			right++;
			while (left < right)
			{
				do
				{
					left++;
				}
				while (left <= right && (sortarray[left].x < x || (sortarray[left].x == x && sortarray[left].y < y)));
				do
				{
					right--;
				}
				while (left <= right && (sortarray[right].x > x || (sortarray[right].x == x && sortarray[right].y > y)));
				if (left < right)
				{
					Vertex vertex2 = sortarray[left];
					sortarray[left] = sortarray[right];
					sortarray[right] = vertex2;
				}
			}
			if (left > num)
			{
				VertexSort(num, left);
			}
			if (num2 > right + 1)
			{
				VertexSort(right + 1, num2);
			}
		}

		private void VertexMedian(int left, int right, int median, int axis)
		{
			int num = right - left + 1;
			int left2 = left;
			int right2 = right;
			if (num == 2)
			{
				if (sortarray[left][axis] > sortarray[right][axis] || (sortarray[left][axis] == sortarray[right][axis] && sortarray[left][1 - axis] > sortarray[right][1 - axis]))
				{
					Vertex vertex = sortarray[right];
					sortarray[right] = sortarray[left];
					sortarray[left] = vertex;
				}
				return;
			}
			int num2 = rand.Next(left, right);
			double num3 = sortarray[num2][axis];
			double num4 = sortarray[num2][1 - axis];
			left--;
			right++;
			while (left < right)
			{
				do
				{
					left++;
				}
				while (left <= right && (sortarray[left][axis] < num3 || (sortarray[left][axis] == num3 && sortarray[left][1 - axis] < num4)));
				do
				{
					right--;
				}
				while (left <= right && (sortarray[right][axis] > num3 || (sortarray[right][axis] == num3 && sortarray[right][1 - axis] > num4)));
				if (left < right)
				{
					Vertex vertex = sortarray[left];
					sortarray[left] = sortarray[right];
					sortarray[right] = vertex;
				}
			}
			if (left > median)
			{
				VertexMedian(left2, left - 1, median, axis);
			}
			if (right < median - 1)
			{
				VertexMedian(right + 1, right2, median, axis);
			}
		}

		private void AlternateAxes(int left, int right, int axis)
		{
			int num = right - left + 1;
			int num2 = num >> 1;
			if (num <= 3)
			{
				axis = 0;
			}
			VertexMedian(left, right, left + num2, axis);
			if (num - num2 >= 2)
			{
				if (num2 >= 2)
				{
					AlternateAxes(left, left + num2 - 1, 1 - axis);
				}
				AlternateAxes(left + num2, right, 1 - axis);
			}
		}

		private void MergeHulls(ref Otri farleft, ref Otri innerleft, ref Otri innerright, ref Otri farright, int axis)
		{
			Otri o = default(Otri);
			Otri o2 = default(Otri);
			Otri o3 = default(Otri);
			Otri o4 = default(Otri);
			Otri o5 = default(Otri);
			Otri o6 = default(Otri);
			Otri o7 = default(Otri);
			Otri newotri = default(Otri);
			Vertex vertex = innerleft.Dest();
			Vertex vertex2 = innerleft.Apex();
			Vertex vertex3 = innerright.Org();
			Vertex vertex4 = innerright.Apex();
			Vertex vertex5;
			Vertex vertex7;
			if (useDwyer && axis == 1)
			{
				vertex5 = farleft.Org();
				Vertex vertex6 = farleft.Apex();
				vertex7 = farright.Dest();
				Vertex vertex8 = farright.Apex();
				while (vertex6.y < vertex5.y)
				{
					farleft.LnextSelf();
					farleft.SymSelf();
					vertex5 = vertex6;
					vertex6 = farleft.Apex();
				}
				innerleft.Sym(ref o7);
				Vertex vertex9 = o7.Apex();
				while (vertex9.y > vertex.y)
				{
					o7.Lnext(ref innerleft);
					vertex2 = vertex;
					vertex = vertex9;
					innerleft.Sym(ref o7);
					vertex9 = o7.Apex();
				}
				while (vertex4.y < vertex3.y)
				{
					innerright.LnextSelf();
					innerright.SymSelf();
					vertex3 = vertex4;
					vertex4 = innerright.Apex();
				}
				farright.Sym(ref o7);
				vertex9 = o7.Apex();
				while (vertex9.y > vertex7.y)
				{
					o7.Lnext(ref farright);
					vertex8 = vertex7;
					vertex7 = vertex9;
					farright.Sym(ref o7);
					vertex9 = o7.Apex();
				}
			}
			bool flag;
			do
			{
				flag = false;
				if (Primitives.CounterClockwise(vertex, vertex2, vertex3) > 0.0)
				{
					innerleft.LprevSelf();
					innerleft.SymSelf();
					vertex = vertex2;
					vertex2 = innerleft.Apex();
					flag = true;
				}
				if (Primitives.CounterClockwise(vertex4, vertex3, vertex) > 0.0)
				{
					innerright.LnextSelf();
					innerright.SymSelf();
					vertex3 = vertex4;
					vertex4 = innerright.Apex();
					flag = true;
				}
			}
			while (flag);
			innerleft.Sym(ref o);
			innerright.Sym(ref o2);
			mesh.MakeTriangle(ref newotri);
			newotri.Bond(ref innerleft);
			newotri.LnextSelf();
			newotri.Bond(ref innerright);
			newotri.LnextSelf();
			newotri.SetOrg(vertex3);
			newotri.SetDest(vertex);
			vertex5 = farleft.Org();
			if (vertex == vertex5)
			{
				newotri.Lnext(ref farleft);
			}
			vertex7 = farright.Dest();
			if (vertex3 == vertex7)
			{
				newotri.Lprev(ref farright);
			}
			Vertex vertex10 = vertex;
			Vertex vertex11 = vertex3;
			Vertex vertex12 = o.Apex();
			Vertex vertex13 = o2.Apex();
			while (true)
			{
				bool flag2 = Primitives.CounterClockwise(vertex12, vertex10, vertex11) <= 0.0;
				bool flag3 = Primitives.CounterClockwise(vertex13, vertex10, vertex11) <= 0.0;
				if (flag2 & flag3)
				{
					break;
				}
				if (!flag2)
				{
					o.Lprev(ref o3);
					o3.SymSelf();
					Vertex vertex14 = o3.Apex();
					if (vertex14 != null)
					{
						bool flag4 = Primitives.InCircle(vertex10, vertex11, vertex12, vertex14) > 0.0;
						while (flag4)
						{
							o3.LnextSelf();
							o3.Sym(ref o5);
							o3.LnextSelf();
							o3.Sym(ref o4);
							o3.Bond(ref o5);
							o.Bond(ref o4);
							o.LnextSelf();
							o.Sym(ref o6);
							o3.LprevSelf();
							o3.Bond(ref o6);
							o.SetOrg(vertex10);
							o.SetDest(null);
							o.SetApex(vertex14);
							o3.SetOrg(null);
							o3.SetDest(vertex12);
							o3.SetApex(vertex14);
							vertex12 = vertex14;
							o4.Copy(ref o3);
							vertex14 = o3.Apex();
							flag4 = vertex14 != null && Primitives.InCircle(vertex10, vertex11, vertex12, vertex14) > 0.0;
						}
					}
				}
				if (!flag3)
				{
					o2.Lnext(ref o3);
					o3.SymSelf();
					Vertex vertex14 = o3.Apex();
					if (vertex14 != null)
					{
						bool flag4 = Primitives.InCircle(vertex10, vertex11, vertex13, vertex14) > 0.0;
						while (flag4)
						{
							o3.LprevSelf();
							o3.Sym(ref o5);
							o3.LprevSelf();
							o3.Sym(ref o4);
							o3.Bond(ref o5);
							o2.Bond(ref o4);
							o2.LprevSelf();
							o2.Sym(ref o6);
							o3.LnextSelf();
							o3.Bond(ref o6);
							o2.SetOrg(null);
							o2.SetDest(vertex11);
							o2.SetApex(vertex14);
							o3.SetOrg(vertex13);
							o3.SetDest(null);
							o3.SetApex(vertex14);
							vertex13 = vertex14;
							o4.Copy(ref o3);
							vertex14 = o3.Apex();
							flag4 = vertex14 != null && Primitives.InCircle(vertex10, vertex11, vertex13, vertex14) > 0.0;
						}
					}
				}
				if (flag2 || (!flag3 && Primitives.InCircle(vertex12, vertex10, vertex11, vertex13) > 0.0))
				{
					newotri.Bond(ref o2);
					o2.Lprev(ref newotri);
					newotri.SetDest(vertex10);
					vertex11 = vertex13;
					newotri.Sym(ref o2);
					vertex13 = o2.Apex();
				}
				else
				{
					newotri.Bond(ref o);
					o.Lnext(ref newotri);
					newotri.SetOrg(vertex11);
					vertex10 = vertex12;
					newotri.Sym(ref o);
					vertex12 = o.Apex();
				}
			}
			mesh.MakeTriangle(ref o3);
			o3.SetOrg(vertex10);
			o3.SetDest(vertex11);
			o3.Bond(ref newotri);
			o3.LnextSelf();
			o3.Bond(ref o2);
			o3.LnextSelf();
			o3.Bond(ref o);
			if (useDwyer && axis == 1)
			{
				vertex5 = farleft.Org();
				Vertex vertex6 = farleft.Apex();
				vertex7 = farright.Dest();
				Vertex vertex8 = farright.Apex();
				farleft.Sym(ref o7);
				Vertex vertex9 = o7.Apex();
				while (vertex9.x < vertex5.x)
				{
					o7.Lprev(ref farleft);
					vertex6 = vertex5;
					vertex5 = vertex9;
					farleft.Sym(ref o7);
					vertex9 = o7.Apex();
				}
				while (vertex8.x > vertex7.x)
				{
					farright.LprevSelf();
					farright.SymSelf();
					vertex7 = vertex8;
					vertex8 = farright.Apex();
				}
			}
		}

		private void DivconqRecurse(int left, int right, int axis, ref Otri farleft, ref Otri farright)
		{
			Otri newotri = default(Otri);
			Otri newotri2 = default(Otri);
			Otri newotri3 = default(Otri);
			Otri newotri4 = default(Otri);
			Otri farright2 = default(Otri);
			Otri farleft2 = default(Otri);
			int num = right - left + 1;
			switch (num)
			{
			case 2:
				mesh.MakeTriangle(ref farleft);
				farleft.SetOrg(sortarray[left]);
				farleft.SetDest(sortarray[left + 1]);
				mesh.MakeTriangle(ref farright);
				farright.SetOrg(sortarray[left + 1]);
				farright.SetDest(sortarray[left]);
				farleft.Bond(ref farright);
				farleft.LprevSelf();
				farright.LnextSelf();
				farleft.Bond(ref farright);
				farleft.LprevSelf();
				farright.LnextSelf();
				farleft.Bond(ref farright);
				farright.Lprev(ref farleft);
				break;
			case 3:
			{
				mesh.MakeTriangle(ref newotri);
				mesh.MakeTriangle(ref newotri2);
				mesh.MakeTriangle(ref newotri3);
				mesh.MakeTriangle(ref newotri4);
				double num3 = Primitives.CounterClockwise(sortarray[left], sortarray[left + 1], sortarray[left + 2]);
				if (num3 == 0.0)
				{
					newotri.SetOrg(sortarray[left]);
					newotri.SetDest(sortarray[left + 1]);
					newotri2.SetOrg(sortarray[left + 1]);
					newotri2.SetDest(sortarray[left]);
					newotri3.SetOrg(sortarray[left + 2]);
					newotri3.SetDest(sortarray[left + 1]);
					newotri4.SetOrg(sortarray[left + 1]);
					newotri4.SetDest(sortarray[left + 2]);
					newotri.Bond(ref newotri2);
					newotri3.Bond(ref newotri4);
					newotri.LnextSelf();
					newotri2.LprevSelf();
					newotri3.LnextSelf();
					newotri4.LprevSelf();
					newotri.Bond(ref newotri4);
					newotri2.Bond(ref newotri3);
					newotri.LnextSelf();
					newotri2.LprevSelf();
					newotri3.LnextSelf();
					newotri4.LprevSelf();
					newotri.Bond(ref newotri2);
					newotri3.Bond(ref newotri4);
					newotri2.Copy(ref farleft);
					newotri3.Copy(ref farright);
					break;
				}
				newotri.SetOrg(sortarray[left]);
				newotri2.SetDest(sortarray[left]);
				newotri4.SetOrg(sortarray[left]);
				if (num3 > 0.0)
				{
					newotri.SetDest(sortarray[left + 1]);
					newotri2.SetOrg(sortarray[left + 1]);
					newotri3.SetDest(sortarray[left + 1]);
					newotri.SetApex(sortarray[left + 2]);
					newotri3.SetOrg(sortarray[left + 2]);
					newotri4.SetDest(sortarray[left + 2]);
				}
				else
				{
					newotri.SetDest(sortarray[left + 2]);
					newotri2.SetOrg(sortarray[left + 2]);
					newotri3.SetDest(sortarray[left + 2]);
					newotri.SetApex(sortarray[left + 1]);
					newotri3.SetOrg(sortarray[left + 1]);
					newotri4.SetDest(sortarray[left + 1]);
				}
				newotri.Bond(ref newotri2);
				newotri.LnextSelf();
				newotri.Bond(ref newotri3);
				newotri.LnextSelf();
				newotri.Bond(ref newotri4);
				newotri2.LprevSelf();
				newotri3.LnextSelf();
				newotri2.Bond(ref newotri3);
				newotri2.LprevSelf();
				newotri4.LprevSelf();
				newotri2.Bond(ref newotri4);
				newotri3.LnextSelf();
				newotri4.LprevSelf();
				newotri3.Bond(ref newotri4);
				newotri2.Copy(ref farleft);
				if (num3 > 0.0)
				{
					newotri3.Copy(ref farright);
				}
				else
				{
					farleft.Lnext(ref farright);
				}
				break;
			}
			default:
			{
				int num2 = num >> 1;
				DivconqRecurse(left, left + num2 - 1, 1 - axis, ref farleft, ref farright2);
				DivconqRecurse(left + num2, right, 1 - axis, ref farleft2, ref farright);
				MergeHulls(ref farleft, ref farright2, ref farleft2, ref farright, axis);
				break;
			}
			}
		}

		private int RemoveGhosts(ref Otri startghost)
		{
			Otri o = default(Otri);
			Otri o2 = default(Otri);
			Otri o3 = default(Otri);
			bool flag = !mesh.behavior.Poly;
			startghost.Lprev(ref o);
			o.SymSelf();
			Mesh.dummytri.neighbors[0] = o;
			startghost.Copy(ref o2);
			int num = 0;
			do
			{
				num++;
				o2.Lnext(ref o3);
				o2.LprevSelf();
				o2.SymSelf();
				if (flag && o2.triangle != Mesh.dummytri)
				{
					Vertex vertex = o2.Org();
					if (vertex.mark == 0)
					{
						vertex.mark = 1;
					}
				}
				o2.Dissolve();
				o3.Sym(ref o2);
				mesh.TriangleDealloc(o3.triangle);
			}
			while (!o2.Equal(startghost));
			return num;
		}

		public int Triangulate(Mesh m)
		{
			Otri farleft = default(Otri);
			Otri farright = default(Otri);
			mesh = m;
			sortarray = new Vertex[m.invertices];
			int num = 0;
			foreach (Vertex value in m.vertices.Values)
			{
				sortarray[num++] = value;
			}
			VertexSort(0, m.invertices - 1);
			num = 0;
			for (int i = 1; i < m.invertices; i++)
			{
				if (sortarray[num].x == sortarray[i].x && sortarray[num].y == sortarray[i].y)
				{
					if (Behavior.Verbose)
					{
						SimpleLog.Instance.Warning(string.Format("A duplicate vertex appeared and was ignored (ID {0}).", sortarray[i].hash), "DivConquer.DivconqDelaunay()");
					}
					sortarray[i].type = VertexType.UndeadVertex;
					m.undeads++;
				}
				else
				{
					num++;
					sortarray[num] = sortarray[i];
				}
			}
			num++;
			if (useDwyer)
			{
				int num2 = num >> 1;
				if (num - num2 >= 2)
				{
					if (num2 >= 2)
					{
						AlternateAxes(0, num2 - 1, 1);
					}
					AlternateAxes(num2, num - 1, 1);
				}
			}
			DivconqRecurse(0, num - 1, 0, ref farleft, ref farright);
			return RemoveGhosts(ref farleft);
		}
	}
}
