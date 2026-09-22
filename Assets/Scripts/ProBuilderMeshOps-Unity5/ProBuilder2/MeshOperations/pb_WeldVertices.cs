using System;
using System.Collections.Generic;
using System.Linq;
using KdTree;
using KdTree.Math;
using ProBuilder2.Common;
using UnityEngine;

namespace ProBuilder2.MeshOperations
{
	public static class pb_WeldVertices
	{
		public static pb_ActionResult WeldVertices(this pb_Object pb, int[] indices, float neighborRadius, out int[] welds)
		{
			pb_Vertex[] vertices = pb_Vertex.GetVertices(pb);
			pb_IntArray[] sharedIndices = pb.sharedIndices;
			Dictionary<int, int> dictionary = sharedIndices.ToDictionary();
			HashSet<int> commonIndices = pb_IntArrayUtility.GetCommonIndices(dictionary, indices);
			int count = commonIndices.Count;
			int num = Math.Min(32, commonIndices.Count());
			KdTree<float, int> kdTree = new KdTree<float, int>(3, new FloatMath(), AddDuplicateBehavior.Collect);
			foreach (int item in commonIndices)
			{
				Vector3 position = vertices[sharedIndices[item][0]].position;
				kdTree.Add(new float[3] { position.x, position.y, position.z }, item);
			}
			float[] array = new float[3];
			Dictionary<int, int> dictionary2 = new Dictionary<int, int>();
			Dictionary<int, Vector3> dictionary3 = new Dictionary<int, Vector3>();
			int num2 = sharedIndices.Length;
			foreach (int item2 in commonIndices)
			{
				if (dictionary2.ContainsKey(item2))
				{
					continue;
				}
				Vector3 position2 = vertices[sharedIndices[item2][0]].position;
				array[0] = position2.x;
				array[1] = position2.y;
				array[2] = position2.z;
				KdTreeNode<float, int>[] array2 = kdTree.RadialSearch(array, neighborRadius, num);
				if (num < count && array2.Length >= num)
				{
					array2 = kdTree.RadialSearch(array, neighborRadius, count);
					num = Math.Min(count, array2.Length + array2.Length / 2);
				}
				Vector3 zero = Vector3.zero;
				float num3 = 0f;
				for (int i = 0; i < array2.Length; i++)
				{
					int value = array2[i].Value;
					if (dictionary2.ContainsKey(value))
					{
						continue;
					}
					zero.x += array2[i].Point[0];
					zero.y += array2[i].Point[1];
					zero.z += array2[i].Point[2];
					dictionary2.Add(value, num2);
					num3++;
					if (array2[i].Duplicates != null)
					{
						for (int j = 0; j < array2[i].Duplicates.Count; j++)
						{
							dictionary2.Add(array2[i].Duplicates[j], num2);
						}
					}
				}
				zero.x /= num3;
				zero.y /= num3;
				zero.z /= num3;
				dictionary3.Add(num2, zero);
				num2++;
			}
			welds = new int[dictionary2.Count];
			int num4 = 0;
			foreach (KeyValuePair<int, int> item3 in dictionary2)
			{
				int[] array3 = sharedIndices[item3.Key];
				welds[num4++] = array3[0];
				for (int k = 0; k < array3.Length; k++)
				{
					dictionary[array3[k]] = item3.Value;
					vertices[array3[k]].position = dictionary3[item3.Value];
				}
			}
			pb.SetSharedIndices(dictionary);
			pb.SetVertices(vertices);
			pb.ToMesh();
			return new pb_ActionResult(Status.Success, "Weld Vertices");
		}
	}
}
