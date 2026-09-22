using System;
using TriangleNet.Data;

namespace TriangleNet.Tools
{
	public class AdjacencyMatrix
	{
		private int node_num;

		private int adj_num;

		private int[] adj_row;

		private int[] adj;

		public int[] AdjacencyRow
		{
			get
			{
				return adj_row;
			}
		}

		public int[] Adjacency
		{
			get
			{
				return adj;
			}
		}

		public AdjacencyMatrix(Mesh mesh)
		{
			node_num = mesh.vertices.Count;
			adj_row = AdjacencyCount(mesh);
			adj_num = adj_row[node_num] - 1;
			adj = AdjacencySet(mesh, adj_row);
		}

		public int Bandwidth()
		{
			int num = 0;
			int num2 = 0;
			for (int i = 0; i < node_num; i++)
			{
				for (int j = adj_row[i]; j <= adj_row[i + 1] - 1; j++)
				{
					int num3 = adj[j - 1];
					num = Math.Max(num, i - num3);
					num2 = Math.Max(num2, num3 - i);
				}
			}
			return num + 1 + num2;
		}

		private int[] AdjacencyCount(Mesh mesh)
		{
			int[] array = new int[node_num + 1];
			int i;
			for (i = 0; i < node_num; i++)
			{
				array[i] = 1;
			}
			foreach (Triangle value in mesh.triangles.Values)
			{
				int id = value.id;
				int id2 = value.vertices[0].id;
				int id3 = value.vertices[1].id;
				int id4 = value.vertices[2].id;
				int id5 = value.neighbors[2].triangle.id;
				if (id5 < 0 || id < id5)
				{
					array[id2]++;
					array[id3]++;
				}
				id5 = value.neighbors[0].triangle.id;
				if (id5 < 0 || id < id5)
				{
					array[id3]++;
					array[id4]++;
				}
				id5 = value.neighbors[1].triangle.id;
				if (id5 < 0 || id < id5)
				{
					array[id4]++;
					array[id2]++;
				}
			}
			i = node_num;
			while (1 <= i)
			{
				array[i] = array[i - 1];
				i--;
			}
			array[0] = 1;
			for (int j = 1; j <= node_num; j++)
			{
				array[j] = array[j - 1] + array[j];
			}
			return array;
		}

		private int[] AdjacencySet(Mesh mesh, int[] rows)
		{
			int[] array = new int[node_num];
			Array.Copy(rows, array, node_num);
			int num = rows[node_num] - 1;
			int[] array2 = new int[num];
			for (int i = 0; i < num; i++)
			{
				array2[i] = -1;
			}
			for (int i = 0; i < node_num; i++)
			{
				array2[array[i] - 1] = i;
				array[i]++;
			}
			foreach (Triangle value in mesh.triangles.Values)
			{
				int id = value.id;
				int id2 = value.vertices[0].id;
				int id3 = value.vertices[1].id;
				int id4 = value.vertices[2].id;
				int id5 = value.neighbors[2].triangle.id;
				if (id5 < 0 || id < id5)
				{
					array2[array[id2] - 1] = id3;
					array[id2]++;
					array2[array[id3] - 1] = id2;
					array[id3]++;
				}
				id5 = value.neighbors[0].triangle.id;
				if (id5 < 0 || id < id5)
				{
					array2[array[id3] - 1] = id4;
					array[id3]++;
					array2[array[id4] - 1] = id3;
					array[id4]++;
				}
				id5 = value.neighbors[1].triangle.id;
				if (id5 < 0 || id < id5)
				{
					array2[array[id2] - 1] = id4;
					array[id2]++;
					array2[array[id4] - 1] = id2;
					array[id4]++;
				}
			}
			for (int i = 0; i < node_num; i++)
			{
				int num2 = rows[i];
				int num3 = rows[i + 1] - 1;
				HeapSort(array2, num2 - 1, num3 + 1 - num2);
			}
			return array2;
		}

		private void CreateHeap(int[] a, int offset, int size)
		{
			int num = size / 2 - 1;
			while (0 <= num)
			{
				int num2 = a[offset + num];
				int num3 = num;
				while (true)
				{
					int num4 = 2 * num3 + 1;
					if (size <= num4)
					{
						break;
					}
					if (num4 + 1 < size && a[offset + num4] < a[offset + num4 + 1])
					{
						num4++;
					}
					if (num2 >= a[offset + num4])
					{
						break;
					}
					a[offset + num3] = a[offset + num4];
					num3 = num4;
				}
				a[offset + num3] = num2;
				num--;
			}
		}

		private void HeapSort(int[] a, int offset, int size)
		{
			if (size > 1)
			{
				CreateHeap(a, offset, size);
				int num = a[offset];
				a[offset] = a[offset + size - 1];
				a[offset + size - 1] = num;
				int num2 = size - 1;
				while (2 <= num2)
				{
					CreateHeap(a, offset, num2);
					num = a[offset];
					a[offset] = a[offset + num2 - 1];
					a[offset + num2 - 1] = num;
					num2--;
				}
			}
		}
	}
}
