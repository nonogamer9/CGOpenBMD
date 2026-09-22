using System.Collections.Generic;
using System.Linq;
using ProBuilder2.Common;

namespace ProBuilder2.MeshOperations
{
	public class pbMeshUtils
	{
		public static List<pb_Face> GetNeighborFaces(pb_Object pb, pb_Face originFace, Dictionary<int, int> lookup = null, IEnumerable<pb_Face> mask = null)
		{
			if (lookup == null)
			{
				lookup = pb.sharedIndices.ToDictionary();
			}
			List<pb_Face> list = new List<pb_Face>();
			HashSet<pb_Edge> hashSet = new HashSet<pb_Edge>();
			for (int i = 0; i < originFace.edges.Length; i++)
			{
				hashSet.Add(new pb_Edge(lookup[originFace.edges[i].x], lookup[originFace.edges[i].y]));
			}
			pb_Edge item = new pb_Edge(-1, -1);
			for (int j = 0; j < pb.faces.Length; j++)
			{
				pb_Edge[] edges = pb.faces[j].edges;
				for (int k = 0; k < edges.Length; k++)
				{
					pb_Edge pb_Edge2 = edges[k];
					item.x = lookup[pb_Edge2.x];
					item.y = lookup[pb_Edge2.y];
					if (hashSet.Contains(item) && (mask == null || !mask.Contains(pb.faces[j])))
					{
						list.Add(pb.faces[j]);
						break;
					}
				}
			}
			return list;
		}

		public static Dictionary<pb_Face, List<pb_Face>> GenerateNeighborLookup(pb_Object pb, IList<pb_Face> InFaces)
		{
			Dictionary<int, int> sharedIndicesLookup = pb.sharedIndices.ToDictionary();
			Dictionary<pb_Face, List<pb_Face>> dictionary = new Dictionary<pb_Face, List<pb_Face>>();
			int num = InFaces.Count();
			HashSet<pb_Edge>[] array = new HashSet<pb_Edge>[num];
			for (int i = 0; i < num; i++)
			{
				array[i] = new HashSet<pb_Edge>(pb_Edge.GetUniversalEdges(InFaces[i].edges, sharedIndicesLookup));
			}
			for (int j = 0; j < num - 1; j++)
			{
				if (!dictionary.ContainsKey(InFaces[j]))
				{
					dictionary.Add(InFaces[j], new List<pb_Face>());
				}
				for (int k = j + 1; k < num; k++)
				{
					if (array[j].Overlaps(array[k]))
					{
						dictionary[InFaces[j]].Add(InFaces[k]);
						List<pb_Face> value;
						if (dictionary.TryGetValue(InFaces[k], out value))
						{
							value.Add(InFaces[j]);
							continue;
						}
						dictionary.Add(InFaces[k], new List<pb_Face> { InFaces[j] });
					}
				}
			}
			return dictionary;
		}

		public static pb_Face[] GetNeighborFaces(pb_Object pb, Dictionary<int, int> sharedIndicesLookup, pb_Face[] selFaces)
		{
			List<pb_Face> list = new List<pb_Face>();
			pb_Edge[] array = GetPerimeterEdges(sharedIndicesLookup, selFaces).ToArray();
			pb_Edge[] array2 = new pb_Edge[array.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array2[i] = new pb_Edge(sharedIndicesLookup[array[i].x], sharedIndicesLookup[array[i].y]);
			}
			pb_Edge edge = new pb_Edge(-1, -1);
			HashSet<pb_Face> hashSet = new HashSet<pb_Face>(selFaces);
			pb_Face[] faces = pb.faces;
			foreach (pb_Face pb_Face2 in faces)
			{
				if (hashSet.Contains(pb_Face2))
				{
					hashSet.Remove(pb_Face2);
					continue;
				}
				pb_Edge[] edges = pb_Face2.edges;
				for (int k = 0; k < edges.Length; k++)
				{
					pb_Edge pb_Edge2 = edges[k];
					edge.x = sharedIndicesLookup[pb_Edge2.x];
					edge.y = sharedIndicesLookup[pb_Edge2.y];
					if (array2.Contains(edge))
					{
						list.Add(pb_Face2);
						break;
					}
				}
			}
			return list.ToArray();
		}

		public static List<pb_Tuple<pb_Face, pb_Edge>> GetNeighborFaces(pb_Object pb, pb_Edge edge, Dictionary<int, int> lookup = null)
		{
			if (lookup == null)
			{
				lookup = pb.sharedIndices.ToDictionary();
			}
			List<pb_Tuple<pb_Face, pb_Edge>> list = new List<pb_Tuple<pb_Face, pb_Edge>>();
			pb_Edge pb_Edge2 = new pb_Edge(lookup[edge.x], lookup[edge.y]);
			pb_Edge pb_Edge3 = new pb_Edge(0, 0);
			for (int i = 0; i < pb.faces.Length; i++)
			{
				pb_Edge[] edges = pb.faces[i].edges;
				for (int j = 0; j < edges.Length; j++)
				{
					pb_Edge3.x = edges[j].x;
					pb_Edge3.y = edges[j].y;
					if ((pb_Edge2.x == lookup[pb_Edge3.x] && pb_Edge2.y == lookup[pb_Edge3.y]) || (pb_Edge2.x == lookup[pb_Edge3.y] && pb_Edge2.y == lookup[pb_Edge3.x]))
					{
						list.Add(new pb_Tuple<pb_Face, pb_Edge>(pb.faces[i], new pb_Edge(edges[j])));
						break;
					}
				}
			}
			return list;
		}

		public static pb_Face[] GetNeighborFaces(pb_Object pb, pb_Edge[] edges)
		{
			List<pb_Face> list = new List<pb_Face>();
			Dictionary<int, int> lookup = pb.sharedIndices.ToDictionary();
			pb_Face[] faces = pb.faces;
			foreach (pb_Face pb_Face2 in faces)
			{
				foreach (pb_Edge edge in edges)
				{
					if (pb_Face2.edges.IndexOf(edge, lookup) > -1)
					{
						list.Add(pb_Face2);
					}
				}
			}
			return list.Distinct().ToArray();
		}

		internal static List<pb_Face>[][] GetNeighborFacesJagged(pb_Object pb, pb_Edge[][] selEdges)
		{
			int num = selEdges.Length;
			List<pb_Face>[][] array = new List<pb_Face>[num][];
			for (int i = 0; i < num; i++)
			{
				array[i] = new List<pb_Face>[selEdges[i].Length];
				for (int j = 0; j < selEdges[i].Length; j++)
				{
					array[i][j] = new List<pb_Face>();
				}
			}
			pb_IntArray[] sharedIndices = pb.sharedIndices;
			pb_Edge[][] array2 = new pb_Edge[num][];
			for (int k = 0; k < num; k++)
			{
				array2[k] = pb_Edge.GetUniversalEdges(selEdges[k], sharedIndices).Distinct().ToArray();
			}
			for (int l = 0; l < pb.faces.Length; l++)
			{
				pb_Edge[] edges = pb_Edge.GetUniversalEdges(pb.faces[l].edges, sharedIndices).Distinct().ToArray();
				for (int m = 0; m < num; m++)
				{
					int num2 = -1;
					for (int n = 0; n < array2[m].Length; n++)
					{
						if (edges.Contains(array2[m][n]))
						{
							num2 = n;
							break;
						}
					}
					if (num2 > -1)
					{
						array[m][num2].Add(pb.faces[l]);
					}
				}
			}
			return array;
		}

		public static List<pb_Face> GetNeighborFaces(pb_Object pb, int index)
		{
			List<pb_Face> list = new List<pb_Face>();
			pb_IntArray[] sharedIndices = pb.sharedIndices;
			int num = sharedIndices.IndexOf(index);
			pb_Face[] faces = pb.faces;
			foreach (pb_Face pb_Face2 in faces)
			{
				if (pb_Face2.distinctIndices.ContainsMatch(sharedIndices[num]))
				{
					list.Add(pb_Face2);
				}
			}
			return list;
		}

		public static IEnumerable<pb_Face> GetNeighborFaces(pb_Object pb, IEnumerable<int> indices)
		{
			List<pb_Face> list = new List<pb_Face>();
			Dictionary<int, int> dictionary = pb.sharedIndices.ToDictionary();
			HashSet<int> hashSet = new HashSet<int>();
			foreach (int index in indices)
			{
				hashSet.Add(dictionary[index]);
			}
			for (int i = 0; i < pb.faces.Length; i++)
			{
				int[] distinctIndices = pb.faces[i].distinctIndices;
				for (int j = 0; j < distinctIndices.Length; j++)
				{
					if (hashSet.Contains(dictionary[distinctIndices[j]]))
					{
						list.Add(pb.faces[i]);
						break;
					}
				}
			}
			return list;
		}

		public static pb_Edge[] GetConnectedEdges(pb_Object pb, int[] indices)
		{
			Dictionary<int, int> dictionary = pb.sharedIndices.ToDictionary();
			List<pb_Edge> list = new List<pb_Edge>();
			HashSet<int> hashSet = new HashSet<int>();
			for (int i = 0; i < indices.Length; i++)
			{
				hashSet.Add(dictionary[indices[i]]);
			}
			pb_Edge[] array = pb_Edge.AllEdges(pb.faces);
			HashSet<pb_Edge> hashSet2 = new HashSet<pb_Edge>();
			pb_Edge item = new pb_Edge(0, 0);
			for (int j = 0; j < array.Length; j++)
			{
				pb_Edge item2 = new pb_Edge(dictionary[array[j].x], dictionary[array[j].y]);
				if (hashSet.Contains(item2.x) || (hashSet.Contains(item2.y) && !hashSet2.Contains(item)))
				{
					list.Add(array[j]);
					hashSet2.Add(item2);
				}
			}
			return list.ToArray();
		}

		public static IEnumerable<pb_Edge> GetPerimeterEdges(pb_Object pb, IEnumerable<pb_Face> faces)
		{
			return GetPerimeterEdges(pb.sharedIndices.ToDictionary(), faces);
		}

		public static IEnumerable<pb_Edge> GetPerimeterEdges(Dictionary<int, int> sharedIndicesLookup, IEnumerable<pb_Face> faces)
		{
			List<pb_Edge> list = faces.SelectMany((pb_Face x) => x.edges).ToList();
			int count = list.Count;
			Dictionary<pb_Edge, List<pb_Edge>> dictionary = new Dictionary<pb_Edge, List<pb_Edge>>();
			for (int num = 0; num < count; num++)
			{
				pb_Edge key = new pb_Edge(sharedIndicesLookup[list[num].x], sharedIndicesLookup[list[num].y]);
				List<pb_Edge> value;
				if (dictionary.TryGetValue(key, out value))
				{
					value.Add(list[num]);
					continue;
				}
				dictionary.Add(key, new List<pb_Edge> { list[num] });
			}
			return from x in dictionary
				where x.Value.Count < 2
				select x.Value[0];
		}

		public static int[] GetPerimeterEdges(pb_Object pb, pb_Edge[] edges)
		{
			if (edges.Length == pb_Edge.AllEdges(pb.faces).Length || edges.Length < 3)
			{
				return new int[0];
			}
			pb_Edge[] universalEdges = pb_Edge.GetUniversalEdges(edges, pb.sharedIndices.ToDictionary());
			int[] array = new int[universalEdges.Length];
			for (int i = 0; i < universalEdges.Length - 1; i++)
			{
				for (int j = i + 1; j < universalEdges.Length; j++)
				{
					if (universalEdges[i].x == universalEdges[j].x || universalEdges[i].x == universalEdges[j].y || universalEdges[i].y == universalEdges[j].x || universalEdges[i].y == universalEdges[j].y)
					{
						array[i]++;
						array[j]++;
					}
				}
			}
			int num = pb_Math.Min(array);
			List<int> list = new List<int>();
			for (int k = 0; k < array.Length; k++)
			{
				if (array[k] <= num)
				{
					list.Add(k);
				}
			}
			return (list.Count == edges.Length) ? new int[0] : list.ToArray();
		}

		public static IEnumerable<pb_Face> GetPerimeterFaces(pb_Object pb, IEnumerable<pb_Face> faces)
		{
			Dictionary<int, int> dictionary = pb.sharedIndices.ToDictionary();
			Dictionary<pb_Edge, List<pb_Face>> dictionary2 = new Dictionary<pb_Edge, List<pb_Face>>();
			foreach (pb_Face face in faces)
			{
				pb_Edge[] edges = face.edges;
				for (int i = 0; i < edges.Length; i++)
				{
					pb_Edge pb_Edge2 = edges[i];
					pb_Edge key = new pb_Edge(dictionary[pb_Edge2.x], dictionary[pb_Edge2.y]);
					if (dictionary2.ContainsKey(key))
					{
						dictionary2[key].Add(face);
						continue;
					}
					dictionary2.Add(key, new List<pb_Face> { face });
				}
			}
			return (from x in dictionary2
				where x.Value.Count < 2
				select x.Value[0]).Distinct();
		}

		public static int[] GetPerimeterVertices(pb_Object pb, int[] indices, pb_Edge[] universal_edges_all)
		{
			int num = indices.Length;
			pb_IntArray[] sharedIndices = pb.sharedIndices;
			int[] array = new int[num];
			for (int i = 0; i < num; i++)
			{
				array[i] = sharedIndices.IndexOf(indices[i]);
			}
			int[] array2 = new int[indices.Length];
			for (int j = 0; j < indices.Length - 1; j++)
			{
				for (int k = j + 1; k < indices.Length; k++)
				{
					if (universal_edges_all.Contains(array[j], array[k]))
					{
						array2[j]++;
						array2[k]++;
					}
				}
			}
			int num2 = pb_Math.Min(array2);
			List<int> list = new List<int>();
			for (int l = 0; l < num; l++)
			{
				if (array2[l] <= num2)
				{
					list.Add(l);
				}
			}
			return (list.Count >= num) ? new int[0] : list.ToArray();
		}

		private static pb_WingedEdge EdgeRingNext(pb_WingedEdge edge)
		{
			if (edge == null)
			{
				return null;
			}
			pb_WingedEdge pb_WingedEdge2 = edge.next;
			pb_WingedEdge previous = edge.previous;
			int num = 0;
			while (pb_WingedEdge2 != previous && pb_WingedEdge2 != edge)
			{
				pb_WingedEdge2 = pb_WingedEdge2.next;
				if (pb_WingedEdge2 == previous)
				{
					return null;
				}
				previous = previous.previous;
				num++;
			}
			if (num % 2 == 0 || pb_WingedEdge2 == edge)
			{
				pb_WingedEdge2 = null;
			}
			return pb_WingedEdge2;
		}

		public static IEnumerable<pb_Edge> GetEdgeRing(pb_Object pb, pb_Edge[] edges)
		{
			List<pb_WingedEdge> wingedEdges = pb_WingedEdge.GetWingedEdges(pb);
			List<pb_EdgeLookup> list = pb_EdgeLookup.GetEdgeLookup(edges, pb.sharedIndices.ToDictionary()).ToList();
			list.Distinct();
			Dictionary<pb_Edge, pb_WingedEdge> dictionary = new Dictionary<pb_Edge, pb_WingedEdge>();
			for (int i = 0; i < wingedEdges.Count; i++)
			{
				if (!dictionary.ContainsKey(wingedEdges[i].edge.common))
				{
					dictionary.Add(wingedEdges[i].edge.common, wingedEdges[i]);
				}
			}
			HashSet<pb_EdgeLookup> hashSet = new HashSet<pb_EdgeLookup>();
			for (int j = 0; j < list.Count; j++)
			{
				pb_WingedEdge value;
				if (!dictionary.TryGetValue(list[j].common, out value) || hashSet.Contains(value.edge))
				{
					continue;
				}
				pb_WingedEdge pb_WingedEdge2 = value;
				while (pb_WingedEdge2 != null && hashSet.Add(pb_WingedEdge2.edge))
				{
					pb_WingedEdge2 = EdgeRingNext(pb_WingedEdge2);
					if (pb_WingedEdge2 != null && pb_WingedEdge2.opposite != null)
					{
						pb_WingedEdge2 = pb_WingedEdge2.opposite;
					}
				}
				pb_WingedEdge2 = EdgeRingNext(value.opposite);
				if (pb_WingedEdge2 != null && pb_WingedEdge2.opposite != null)
				{
					pb_WingedEdge2 = pb_WingedEdge2.opposite;
				}
				while (pb_WingedEdge2 != null && hashSet.Add(pb_WingedEdge2.edge))
				{
					pb_WingedEdge2 = EdgeRingNext(pb_WingedEdge2);
					if (pb_WingedEdge2 != null && pb_WingedEdge2.opposite != null)
					{
						pb_WingedEdge2 = pb_WingedEdge2.opposite;
					}
				}
			}
			return hashSet.Select((pb_EdgeLookup x) => x.local);
		}

		public static bool GetEdgeLoop(pb_Object pb, pb_Edge[] edges, out pb_Edge[] loop)
		{
			List<pb_WingedEdge> wingedEdges = pb_WingedEdge.GetWingedEdges(pb);
			IEnumerable<pb_EdgeLookup> edgeLookup = pb_EdgeLookup.GetEdgeLookup(edges, pb.sharedIndices.ToDictionary());
			HashSet<pb_EdgeLookup> hashSet = new HashSet<pb_EdgeLookup>(edgeLookup);
			HashSet<pb_EdgeLookup> hashSet2 = new HashSet<pb_EdgeLookup>();
			for (int i = 0; i < wingedEdges.Count; i++)
			{
				if (!hashSet2.Contains(wingedEdges[i].edge) && hashSet.Contains(wingedEdges[i].edge) && !GetEdgeLoopInternal(wingedEdges[i], wingedEdges[i].edge.common.y, hashSet2))
				{
					GetEdgeLoopInternal(wingedEdges[i], wingedEdges[i].edge.common.x, hashSet2);
				}
			}
			loop = hashSet2.Select((pb_EdgeLookup x) => x.local).ToArray();
			return true;
		}

		private static bool GetEdgeLoopInternal(pb_WingedEdge start, int startIndex, HashSet<pb_EdgeLookup> used)
		{
			int num = startIndex;
			pb_WingedEdge pb_WingedEdge2 = start;
			do
			{
				used.Add(pb_WingedEdge2.edge);
				List<pb_WingedEdge> list = GetSpokes(pb_WingedEdge2, num, true).DistinctBy((pb_WingedEdge x) => x.edge.common).ToList();
				pb_WingedEdge2 = null;
				if (list != null && list.Count == 4)
				{
					pb_WingedEdge2 = list[2];
					num = ((pb_WingedEdge2.edge.common.x != num) ? pb_WingedEdge2.edge.common.x : pb_WingedEdge2.edge.common.y);
				}
			}
			while (pb_WingedEdge2 != null && !used.Contains(pb_WingedEdge2.edge));
			return pb_WingedEdge2 != null;
		}

		private static pb_WingedEdge NextSpoke(pb_WingedEdge wing, int pivot, bool opp)
		{
			if (opp)
			{
				return wing.opposite;
			}
			if (wing.next.edge.common.Contains(pivot))
			{
				return wing.next;
			}
			if (wing.previous.edge.common.Contains(pivot))
			{
				return wing.previous;
			}
			return null;
		}

		public static List<pb_WingedEdge> GetSpokes(pb_WingedEdge wing, int sharedIndex, bool allowHoles = false)
		{
			List<pb_WingedEdge> list = new List<pb_WingedEdge>();
			pb_WingedEdge pb_WingedEdge2 = wing;
			bool flag = false;
			do
			{
				list.Add(pb_WingedEdge2);
				pb_WingedEdge2 = NextSpoke(pb_WingedEdge2, sharedIndex, flag);
				flag = !flag;
				if (pb_WingedEdge2 != null && pb_WingedEdge2.edge.common.Equals(wing.edge.common))
				{
					return list;
				}
			}
			while (pb_WingedEdge2 != null);
			if (!allowHoles)
			{
				return null;
			}
			pb_WingedEdge2 = wing.opposite;
			flag = false;
			List<pb_WingedEdge> list2 = new List<pb_WingedEdge>();
			while (pb_WingedEdge2 != null && !pb_WingedEdge2.edge.common.Equals(wing.edge.common))
			{
				list2.Add(pb_WingedEdge2);
				pb_WingedEdge2 = NextSpoke(pb_WingedEdge2, sharedIndex, flag);
				flag = !flag;
			}
			list2.Reverse();
			list.AddRange(list2);
			return list;
		}
	}
}
