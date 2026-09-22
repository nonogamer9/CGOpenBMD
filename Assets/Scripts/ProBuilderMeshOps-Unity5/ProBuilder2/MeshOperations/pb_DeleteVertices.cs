using System.Collections.Generic;
using System.Linq;
using ProBuilder2.Common;

namespace ProBuilder2.MeshOperations
{
	public static class pb_DeleteVertices
	{
		public static int[] RemoveUnusedVertices(this pb_Object pb)
		{
			List<int> list = new List<int>();
			HashSet<int> hashSet = new HashSet<int>(pb_Face.AllTriangles(pb.faces));
			for (int i = 0; i < pb.vertices.Length; i++)
			{
				if (!hashSet.Contains(i))
				{
					list.Add(i);
				}
			}
			pb.DeleteVerticesWithIndices(list);
			return list.ToArray();
		}

		public static void DeleteVerticesWithIndices(this pb_Object pb, IEnumerable<int> distInd)
		{
			if (distInd == null || distInd.Count() < 1)
			{
				return;
			}
			pb_Vertex[] vertices = pb_Vertex.GetVertices(pb);
			int num = vertices.Length;
			int[] offset = new int[num];
			List<int> sorted = new List<int>(distInd);
			sorted.Sort();
			vertices = vertices.SortedRemoveAt(sorted);
			for (int i = 0; i < num; i++)
			{
				offset[i] = pbUtil.NearestIndexPriorToValue(sorted, i) + 1;
			}
			pb_Face[] faces = pb.faces;
			foreach (pb_Face pb_Face2 in faces)
			{
				int[] indices = pb_Face2.indices;
				for (int k = 0; k < indices.Length; k++)
				{
					indices[k] -= offset[indices[k]];
				}
				pb_Face2.RebuildCaches();
			}
			IEnumerable<KeyValuePair<int, int>> sharedIndices = from y in pb.sharedIndices.ToDictionary()
				where sorted.BinarySearch(y.Key) < 0
				select new KeyValuePair<int, int>(y.Key - offset[y.Key], y.Value);
			IEnumerable<KeyValuePair<int, int>> sharedIndicesUV = from y in pb.sharedIndicesUV.ToDictionary()
				where sorted.BinarySearch(y.Key) < 0
				select new KeyValuePair<int, int>(y.Key - offset[y.Key], y.Value);
			pb.SetVertices(vertices);
			pb.SetSharedIndices(sharedIndices);
			pb.SetSharedIndicesUV(sharedIndicesUV);
		}
	}
}
