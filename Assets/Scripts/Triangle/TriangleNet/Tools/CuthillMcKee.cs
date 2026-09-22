using System;
using TriangleNet.Log;

namespace TriangleNet.Tools
{
	public class CuthillMcKee
	{
		private int node_num;

		private AdjacencyMatrix matrix;

		public int[] Renumber(Mesh mesh)
		{
			node_num = mesh.vertices.Count;
			mesh.Renumber(NodeNumbering.Linear);
			matrix = new AdjacencyMatrix(mesh);
			int num = matrix.Bandwidth();
			int[] perm = GenerateRcm();
			int[] array = PermInverse(node_num, perm);
			int num2 = PermBandwidth(perm, array);
			if (Behavior.Verbose)
			{
				SimpleLog.Instance.Info(string.Format("Reverse Cuthill-McKee (Bandwidth: {0} > {1})", num, num2));
			}
			return array;
		}

		private int PermBandwidth(int[] perm, int[] perm_inv)
		{
			int[] adjacencyRow = matrix.AdjacencyRow;
			int[] adjacency = matrix.Adjacency;
			int num = 0;
			int num2 = 0;
			for (int i = 0; i < node_num; i++)
			{
				for (int j = adjacencyRow[perm[i]]; j <= adjacencyRow[perm[i] + 1] - 1; j++)
				{
					int num3 = perm_inv[adjacency[j - 1]];
					num = Math.Max(num, i - num3);
					num2 = Math.Max(num2, num3 - i);
				}
			}
			return num + 1 + num2;
		}

		private int[] GenerateRcm()
		{
			int[] array = new int[node_num];
			int iccsze = 0;
			int level_num = 0;
			int[] level_row = new int[node_num + 1];
			int[] array2 = new int[node_num];
			for (int i = 0; i < node_num; i++)
			{
				array2[i] = 1;
			}
			int num = 1;
			for (int i = 0; i < node_num; i++)
			{
				if (array2[i] != 0)
				{
					int root = i;
					FindRoot(ref root, array2, ref level_num, level_row, array, num - 1);
					Rcm(root, array2, array, num - 1, ref iccsze);
					num += iccsze;
					if (node_num < num)
					{
						return array;
					}
				}
			}
			return array;
		}

		private void Rcm(int root, int[] mask, int[] perm, int offset, ref int iccsze)
		{
			int[] adjacencyRow = matrix.AdjacencyRow;
			int[] adjacency = matrix.Adjacency;
			int[] array = new int[node_num];
			Degree(root, mask, array, ref iccsze, perm, offset);
			mask[root] = 0;
			if (iccsze <= 1)
			{
				return;
			}
			int num = 0;
			int num2 = 1;
			while (num < num2)
			{
				int num3 = num + 1;
				num = num2;
				for (int i = num3; i <= num; i++)
				{
					int num4 = perm[offset + i - 1];
					int num5 = adjacencyRow[num4];
					int num6 = adjacencyRow[num4 + 1] - 1;
					int num7 = num2 + 1;
					for (int j = num5; j <= num6; j++)
					{
						int num8 = adjacency[j - 1];
						if (mask[num8] != 0)
						{
							num2++;
							mask[num8] = 0;
							perm[offset + num2 - 1] = num8;
						}
					}
					if (num2 <= num7)
					{
						continue;
					}
					int num9 = num7;
					while (num9 < num2)
					{
						int num10 = num9;
						num9++;
						int num8 = perm[offset + num9 - 1];
						while (num7 < num10)
						{
							int num11 = perm[offset + num10 - 1];
							if (array[num11 - 1] <= array[num8 - 1])
							{
								break;
							}
							perm[offset + num10] = num11;
							num10--;
						}
						perm[offset + num10] = num8;
					}
				}
			}
			ReverseVector(perm, offset, iccsze);
		}

		private void FindRoot(ref int root, int[] mask, ref int level_num, int[] level_row, int[] level, int offset)
		{
			int[] adjacencyRow = matrix.AdjacencyRow;
			int[] adjacency = matrix.Adjacency;
			int level_num2 = 0;
			GetLevelSet(ref root, mask, ref level_num, level_row, level, offset);
			int num = level_row[level_num] - 1;
			if (level_num == 1 || level_num == num)
			{
				return;
			}
			do
			{
				int num2 = num;
				int num3 = level_row[level_num - 1];
				root = level[offset + num3 - 1];
				if (num3 < num)
				{
					for (int i = num3; i <= num; i++)
					{
						int num4 = level[offset + i - 1];
						int num5 = 0;
						int num6 = adjacencyRow[num4 - 1];
						int num7 = adjacencyRow[num4] - 1;
						for (int j = num6; j <= num7; j++)
						{
							int num8 = adjacency[j - 1];
							if (mask[num8] > 0)
							{
								num5++;
							}
						}
						if (num5 < num2)
						{
							root = num4;
							num2 = num5;
						}
					}
				}
				GetLevelSet(ref root, mask, ref level_num2, level_row, level, offset);
				if (level_num2 > level_num)
				{
					level_num = level_num2;
					continue;
				}
				break;
			}
			while (num > level_num);
		}

		private void GetLevelSet(ref int root, int[] mask, ref int level_num, int[] level_row, int[] level, int offset)
		{
			int[] adjacencyRow = matrix.AdjacencyRow;
			int[] adjacency = matrix.Adjacency;
			mask[root] = 0;
			level[offset] = root;
			level_num = 0;
			int num = 0;
			int num2 = 1;
			do
			{
				int num3 = num + 1;
				num = num2;
				level_num++;
				level_row[level_num - 1] = num3;
				for (int i = num3; i <= num; i++)
				{
					int num4 = level[offset + i - 1];
					int num5 = adjacencyRow[num4];
					int num6 = adjacencyRow[num4 + 1] - 1;
					for (int j = num5; j <= num6; j++)
					{
						int num7 = adjacency[j - 1];
						if (mask[num7] != 0)
						{
							num2++;
							level[offset + num2 - 1] = num7;
							mask[num7] = 0;
						}
					}
				}
			}
			while (num2 - num > 0);
			level_row[level_num] = num + 1;
			for (int i = 0; i < num2; i++)
			{
				mask[level[offset + i]] = 1;
			}
		}

		private void Degree(int root, int[] mask, int[] deg, ref int iccsze, int[] ls, int offset)
		{
			int[] adjacencyRow = matrix.AdjacencyRow;
			int[] adjacency = matrix.Adjacency;
			int num = 1;
			ls[offset] = root;
			adjacencyRow[root] = -adjacencyRow[root];
			int num2 = 0;
			iccsze = 1;
			while (num > 0)
			{
				int num3 = num2 + 1;
				num2 = iccsze;
				for (int i = num3; i <= num2; i++)
				{
					int num4 = ls[offset + i - 1];
					int num5 = -adjacencyRow[num4];
					int num6 = Math.Abs(adjacencyRow[num4 + 1]) - 1;
					int num7 = 0;
					for (int j = num5; j <= num6; j++)
					{
						int num8 = adjacency[j - 1];
						if (mask[num8] != 0)
						{
							num7++;
							if (0 <= adjacencyRow[num8])
							{
								adjacencyRow[num8] = -adjacencyRow[num8];
								iccsze++;
								ls[offset + iccsze - 1] = num8;
							}
						}
					}
					deg[num4] = num7;
				}
				num = iccsze - num2;
			}
			for (int i = 0; i < iccsze; i++)
			{
				int num4 = ls[offset + i];
				adjacencyRow[num4] = -adjacencyRow[num4];
			}
		}

		private int[] PermInverse(int n, int[] perm)
		{
			int[] array = new int[node_num];
			for (int i = 0; i < n; i++)
			{
				array[perm[i]] = i;
			}
			return array;
		}

		private void ReverseVector(int[] a, int offset, int size)
		{
			for (int i = 0; i < size / 2; i++)
			{
				int num = a[offset + i];
				a[offset + i] = a[offset + size - 1 - i];
				a[offset + size - 1 - i] = num;
			}
		}
	}
}
