using System;
using System.Collections.Generic;
using System.Linq;
using ProBuilder2.Common;

namespace ProBuilder2.MeshOperations
{
	public static class pb_MergeFaces
	{
		public static List<pb_Face> MergePairs(pb_Object target, IEnumerable<pb_Tuple<pb_Face, pb_Face>> pairs, bool collapseCoincidentVertices = true)
		{
			HashSet<pb_Face> remove = new HashSet<pb_Face>();
			List<pb_Face> list = new List<pb_Face>();
			foreach (pb_Tuple<pb_Face, pb_Face> pair in pairs)
			{
				pb_Face item = pair.Item1;
				pb_Face item2 = pair.Item2;
				int num = item.indices.Length;
				int num2 = item2.indices.Length;
				int[] array = new int[num + num2];
				Array.Copy(item.indices, 0, array, 0, num);
				Array.Copy(item2.indices, 0, array, num, num2);
				list.Add(new pb_Face(array, item.material, item.uv, item.smoothingGroup, item.textureGroup, item.elementGroup, item.manualUV));
				remove.Add(item);
				remove.Add(item2);
			}
			List<pb_Face> list2 = target.faces.Where((pb_Face x) => !remove.Contains(x)).ToList();
			list2.AddRange(list);
			target.SetFaces(list2.ToArray());
			if (collapseCoincidentVertices)
			{
				CollapseCoincidentVertices(target, list);
			}
			return list;
		}

		public static pb_Face Merge(pb_Object target, IEnumerable<pb_Face> faces)
		{
			int num = ((faces != null) ? faces.Count() : 0);
			if (num < 1)
			{
				return null;
			}
			pb_Face pb_Face2 = faces.First();
			pb_Face pb_Face3 = new pb_Face(faces.SelectMany((pb_Face x) => x.indices).ToArray(), pb_Face2.material, pb_Face2.uv, pb_Face2.smoothingGroup, pb_Face2.textureGroup, pb_Face2.elementGroup, pb_Face2.manualUV);
			pb_Face[] array = new pb_Face[target.faces.Length - num + 1];
			int num2 = 0;
			HashSet<pb_Face> hashSet = new HashSet<pb_Face>(faces);
			pb_Face[] faces2 = target.faces;
			foreach (pb_Face pb_Face4 in faces2)
			{
				if (!hashSet.Contains(pb_Face4))
				{
					array[num2++] = pb_Face4;
				}
			}
			array[num2] = pb_Face3;
			target.SetFaces(array);
			CollapseCoincidentVertices(target, new pb_Face[1] { pb_Face3 });
			return pb_Face3;
		}

		internal static void CollapseCoincidentVertices(pb_Object pb, IEnumerable<pb_Face> faces)
		{
			Dictionary<int, int> dictionary = pb.sharedIndices.ToDictionary();
			Dictionary<int, int> dictionary2 = new Dictionary<int, int>();
			foreach (pb_Face face in faces)
			{
				dictionary2.Clear();
				for (int i = 0; i < face.indices.Length; i++)
				{
					int key = dictionary[face.indices[i]];
					if (dictionary2.ContainsKey(key))
					{
						face.indices[i] = dictionary2[key];
					}
					else
					{
						dictionary2.Add(key, face.indices[i]);
					}
				}
			}
			pb.RemoveUnusedVertices();
		}
	}
}
