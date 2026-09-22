using System.Collections.Generic;
using System.Linq;

namespace ProBuilder2.Common
{
	public static class EdgeExtensions
	{
		public static bool ContainsDuplicate(this List<pb_Edge> edges, pb_Edge edge, Dictionary<int, int> lookup)
		{
			int num = 0;
			for (int i = 0; i < edges.Count; i++)
			{
				if (edges[i].Equals(edge, lookup) && ++num > 1)
				{
					return true;
				}
			}
			return false;
		}

		public static bool Contains(this pb_Edge[] edges, pb_Edge edge)
		{
			for (int i = 0; i < edges.Length; i++)
			{
				if (edges[i].Equals(edge))
				{
					return true;
				}
			}
			return false;
		}

		public static bool Contains(this pb_Edge[] edges, int x, int y)
		{
			for (int i = 0; i < edges.Length; i++)
			{
				if ((x == edges[i].x && y == edges[i].y) || (x == edges[i].y && y == edges[i].x))
				{
					return true;
				}
			}
			return false;
		}

		public static IEnumerable<pb_Edge> DistinctCommon(this IEnumerable<pb_Edge> edges, Dictionary<int, int> lookup)
		{
			IEnumerable<pb_EdgeLookup> source = edges.Select((pb_Edge x) => new pb_EdgeLookup(new pb_Edge(lookup[x.x], lookup[x.y]), x));
			source = source.Distinct();
			return source.Select((pb_EdgeLookup x) => x.local);
		}

		public static int IndexOf(this IList<pb_Edge> edges, pb_Edge edge, Dictionary<int, int> lookup)
		{
			for (int i = 0; i < edges.Count; i++)
			{
				if (edges[i].Equals(edge, lookup))
				{
					return i;
				}
			}
			return -1;
		}

		public static List<int> ToIntList(this List<pb_Edge> edges)
		{
			List<int> list = new List<int>();
			foreach (pb_Edge edge in edges)
			{
				list.Add(edge.x);
				list.Add(edge.y);
			}
			return list;
		}

		public static int[] AllTriangles(this pb_Edge[] edges)
		{
			int[] array = new int[edges.Length * 2];
			int num = 0;
			for (int i = 0; i < edges.Length; i++)
			{
				array[num++] = edges[i].x;
				array[num++] = edges[i].y;
			}
			return array;
		}

		public static List<int> AllTriangles(this List<pb_Edge> edges)
		{
			List<int> list = new List<int>();
			for (int i = 0; i < edges.Count; i++)
			{
				list.Add(edges[i].x);
				list.Add(edges[i].y);
			}
			return list;
		}
	}
}
