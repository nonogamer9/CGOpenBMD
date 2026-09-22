using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace KdTree
{
	[Serializable]
	public class KdTree<TKey, TValue> : IKdTree<TKey, TValue>, IEnumerable<KdTreeNode<TKey, TValue>>, IEnumerable
	{
		private int dimensions;

		private ITypeMath<TKey> typeMath;

		private KdTreeNode<TKey, TValue> root;

		public AddDuplicateBehavior AddDuplicateBehavior { get; private set; }

		public int Count { get; private set; }

		public KdTree(int dimensions, ITypeMath<TKey> typeMath)
		{
			this.dimensions = dimensions;
			this.typeMath = typeMath;
			Count = 0;
		}

		public KdTree(int dimensions, ITypeMath<TKey> typeMath, AddDuplicateBehavior addDuplicateBehavior)
			: this(dimensions, typeMath)
		{
			AddDuplicateBehavior = addDuplicateBehavior;
		}

		public bool Add(TKey[] point, TValue value)
		{
			KdTreeNode<TKey, TValue> value2 = new KdTreeNode<TKey, TValue>(point, value);
			if (root == null)
			{
				root = new KdTreeNode<TKey, TValue>(point, value);
			}
			else
			{
				int num = -1;
				KdTreeNode<TKey, TValue> kdTreeNode = root;
				int compare;
				while (true)
				{
					num = (num + 1) % dimensions;
					if (typeMath.AreEqual(point, kdTreeNode.Point))
					{
						switch (AddDuplicateBehavior)
						{
						case AddDuplicateBehavior.Skip:
							return false;
						case AddDuplicateBehavior.Error:
							throw new DuplicateNodeError();
						case AddDuplicateBehavior.Update:
							break;
						case AddDuplicateBehavior.Collect:
							kdTreeNode.AddDuplicate(value);
							return false;
						default:
							throw new Exception("Unexpected AddDuplicateBehavior");
						}
						kdTreeNode.Value = value;
					}
					compare = typeMath.Compare(point[num], kdTreeNode.Point[num]);
					if (kdTreeNode[compare] == null)
					{
						break;
					}
					kdTreeNode = kdTreeNode[compare];
				}
				kdTreeNode[compare] = value2;
			}
			Count++;
			return true;
		}

		private void ReadChildNodes(KdTreeNode<TKey, TValue> removedNode)
		{
			if (removedNode.IsLeaf)
			{
				return;
			}
			Queue<KdTreeNode<TKey, TValue>> queue = new Queue<KdTreeNode<TKey, TValue>>();
			Queue<KdTreeNode<TKey, TValue>> queue2 = new Queue<KdTreeNode<TKey, TValue>>();
			if (removedNode.LeftChild != null)
			{
				queue2.Enqueue(removedNode.LeftChild);
			}
			if (removedNode.RightChild != null)
			{
				queue2.Enqueue(removedNode.RightChild);
			}
			while (queue2.Count > 0)
			{
				KdTreeNode<TKey, TValue> kdTreeNode = queue2.Dequeue();
				queue.Enqueue(kdTreeNode);
				for (int i = -1; i <= 1; i += 2)
				{
					if (kdTreeNode[i] != null)
					{
						queue2.Enqueue(kdTreeNode[i]);
						kdTreeNode[i] = null;
					}
				}
			}
			while (queue.Count > 0)
			{
				KdTreeNode<TKey, TValue> kdTreeNode2 = queue.Dequeue();
				Count--;
				Add(kdTreeNode2.Point, kdTreeNode2.Value);
			}
		}

		public void RemoveAt(TKey[] point)
		{
			if (root == null)
			{
				return;
			}
			KdTreeNode<TKey, TValue> removedNode;
			if (typeMath.AreEqual(point, root.Point))
			{
				removedNode = root;
				root = null;
				Count--;
				ReadChildNodes(removedNode);
				return;
			}
			removedNode = root;
			int num = -1;
			do
			{
				num = (num + 1) % dimensions;
				int compare = typeMath.Compare(point[num], removedNode.Point[num]);
				if (removedNode[compare] == null)
				{
					break;
				}
				if (typeMath.AreEqual(point, removedNode[compare].Point))
				{
					KdTreeNode<TKey, TValue> removedNode2 = removedNode[compare];
					removedNode[compare] = null;
					Count--;
					ReadChildNodes(removedNode2);
				}
				else
				{
					removedNode = removedNode[compare];
				}
			}
			while (removedNode != null);
		}

		public KdTreeNode<TKey, TValue>[] GetNearestNeighbours(TKey[] point, int count)
		{
			if (count > Count)
			{
				count = Count;
			}
			if (count < 0)
			{
				throw new ArgumentException("Number of neighbors cannot be negative");
			}
			if (count == 0)
			{
				return new KdTreeNode<TKey, TValue>[0];
			}
			KdTreeNode<TKey, TValue>[] array2 = new KdTreeNode<TKey, TValue>[count];
			NearestNeighbourList<KdTreeNode<TKey, TValue>, TKey> nearestNeighbourList = new NearestNeighbourList<KdTreeNode<TKey, TValue>, TKey>(count, typeMath);
			HyperRect<TKey> rect = HyperRect<TKey>.Infinite(dimensions, typeMath);
			AddNearestNeighbours(root, point, rect, 0, nearestNeighbourList, typeMath.MaxValue);
			count = nearestNeighbourList.Count;
			KdTreeNode<TKey, TValue>[] array = new KdTreeNode<TKey, TValue>[count];
			for (int i = 0; i < count; i++)
			{
				array[count - i - 1] = nearestNeighbourList.RemoveFurtherest();
			}
			return array;
		}

		private void AddNearestNeighbours(KdTreeNode<TKey, TValue> node, TKey[] target, HyperRect<TKey> rect, int depth, NearestNeighbourList<KdTreeNode<TKey, TValue>, TKey> nearestNeighbours, TKey maxSearchRadiusSquared)
		{
			if (node == null)
			{
				return;
			}
			int num = depth % dimensions;
			HyperRect<TKey> hyperRect = rect.Clone();
			hyperRect.MaxPoint[num] = node.Point[num];
			HyperRect<TKey> hyperRect2 = rect.Clone();
			hyperRect2.MinPoint[num] = node.Point[num];
			int num2 = typeMath.Compare(target[num], node.Point[num]);
			HyperRect<TKey> rect2 = ((num2 <= 0) ? hyperRect : hyperRect2);
			HyperRect<TKey> rect3 = ((num2 <= 0) ? hyperRect2 : hyperRect);
			KdTreeNode<TKey, TValue> kdTreeNode = ((num2 <= 0) ? node.LeftChild : node.RightChild);
			KdTreeNode<TKey, TValue> node2 = ((num2 <= 0) ? node.RightChild : node.LeftChild);
			if (kdTreeNode != null)
			{
				AddNearestNeighbours(kdTreeNode, target, rect2, depth + 1, nearestNeighbours, maxSearchRadiusSquared);
			}
			TKey[] closestPoint = rect3.GetClosestPoint(target, typeMath);
			TKey a = typeMath.DistanceSquaredBetweenPoints(closestPoint, target);
			if (typeMath.Compare(a, maxSearchRadiusSquared) <= 0)
			{
				if (nearestNeighbours.IsCapacityReached)
				{
					if (typeMath.Compare(a, nearestNeighbours.GetFurtherestDistance()) < 0)
					{
						AddNearestNeighbours(node2, target, rect3, depth + 1, nearestNeighbours, maxSearchRadiusSquared);
					}
				}
				else
				{
					AddNearestNeighbours(node2, target, rect3, depth + 1, nearestNeighbours, maxSearchRadiusSquared);
				}
			}
			a = typeMath.DistanceSquaredBetweenPoints(node.Point, target);
			if (typeMath.Compare(a, maxSearchRadiusSquared) <= 0)
			{
				nearestNeighbours.Add(node, a);
			}
		}

		public KdTreeNode<TKey, TValue>[] RadialSearch(TKey[] center, TKey radius, int count)
		{
			NearestNeighbourList<KdTreeNode<TKey, TValue>, TKey> nearestNeighbourList = new NearestNeighbourList<KdTreeNode<TKey, TValue>, TKey>(count, typeMath);
			AddNearestNeighbours(root, center, HyperRect<TKey>.Infinite(dimensions, typeMath), 0, nearestNeighbourList, typeMath.Multiply(radius, radius));
			count = nearestNeighbourList.Count;
			KdTreeNode<TKey, TValue>[] array = new KdTreeNode<TKey, TValue>[count];
			for (int i = 0; i < count; i++)
			{
				array[count - i - 1] = nearestNeighbourList.RemoveFurtherest();
			}
			return array;
		}

		public bool TryFindValueAt(TKey[] point, out TValue value)
		{
			KdTreeNode<TKey, TValue> kdTreeNode = root;
			int num = -1;
			while (true)
			{
				if (kdTreeNode == null)
				{
					value = default(TValue);
					return false;
				}
				if (typeMath.AreEqual(point, kdTreeNode.Point))
				{
					break;
				}
				num = (num + 1) % dimensions;
				int compare = typeMath.Compare(point[num], kdTreeNode.Point[num]);
				kdTreeNode = kdTreeNode[compare];
			}
			value = kdTreeNode.Value;
			return true;
		}

		public TValue FindValueAt(TKey[] point)
		{
			TValue value;
			if (TryFindValueAt(point, out value))
			{
				return value;
			}
			return default(TValue);
		}

		public bool TryFindValue(TValue value, out TKey[] point)
		{
			if (root == null)
			{
				point = null;
				return false;
			}
			Queue<KdTreeNode<TKey, TValue>> queue = new Queue<KdTreeNode<TKey, TValue>>();
			queue.Enqueue(root);
			while (queue.Count > 0)
			{
				KdTreeNode<TKey, TValue> kdTreeNode = queue.Dequeue();
				if (kdTreeNode.Value.Equals(value))
				{
					point = kdTreeNode.Point;
					return true;
				}
				for (int i = -1; i <= 1; i += 2)
				{
					KdTreeNode<TKey, TValue> kdTreeNode2 = kdTreeNode[i];
					if (kdTreeNode2 != null)
					{
						queue.Enqueue(kdTreeNode2);
					}
				}
			}
			point = null;
			return false;
		}

		public TKey[] FindValue(TValue value)
		{
			TKey[] point;
			if (TryFindValue(value, out point))
			{
				return point;
			}
			return null;
		}

		private void AddNodeToStringBuilder(KdTreeNode<TKey, TValue> node, StringBuilder sb, int depth)
		{
			sb.AppendLine(node.ToString());
			for (int i = -1; i <= 1; i += 2)
			{
				for (int j = 0; j <= depth; j++)
				{
					sb.Append("\t");
				}
				sb.Append((i == -1) ? "L " : "R ");
				if (node[i] == null)
				{
					sb.AppendLine("");
				}
				else
				{
					AddNodeToStringBuilder(node[i], sb, depth + 1);
				}
			}
		}

		public override string ToString()
		{
			if (root == null)
			{
				return "";
			}
			StringBuilder stringBuilder = new StringBuilder();
			AddNodeToStringBuilder(root, stringBuilder, 0);
			return stringBuilder.ToString();
		}

		private void AddNodesToList(KdTreeNode<TKey, TValue> node, List<KdTreeNode<TKey, TValue>> nodes)
		{
			if (node == null)
			{
				return;
			}
			nodes.Add(node);
			for (int i = -1; i <= 1; i += 2)
			{
				if (node[i] != null)
				{
					AddNodesToList(node[i], nodes);
					node[i] = null;
				}
			}
		}

		private void SortNodesArray(KdTreeNode<TKey, TValue>[] nodes, int byDimension, int fromIndex, int toIndex)
		{
			for (int i = fromIndex + 1; i <= toIndex; i++)
			{
				int num = i;
				while (true)
				{
					KdTreeNode<TKey, TValue> kdTreeNode = nodes[num - 1];
					KdTreeNode<TKey, TValue> kdTreeNode2 = nodes[num];
					if (typeMath.Compare(kdTreeNode2.Point[byDimension], kdTreeNode.Point[byDimension]) >= 0)
					{
						break;
					}
					nodes[num - 1] = kdTreeNode2;
					nodes[num] = kdTreeNode;
				}
			}
		}

		private void AddNodesBalanced(KdTreeNode<TKey, TValue>[] nodes, int byDimension, int fromIndex, int toIndex)
		{
			if (fromIndex == toIndex)
			{
				Add(nodes[fromIndex].Point, nodes[fromIndex].Value);
				nodes[fromIndex] = null;
				return;
			}
			SortNodesArray(nodes, byDimension, fromIndex, toIndex);
			int num = fromIndex + (int)System.Math.Round((float)(toIndex + 1 - fromIndex) / 2f) - 1;
			Add(nodes[num].Point, nodes[num].Value);
			nodes[num] = null;
			int byDimension2 = (byDimension + 1) % dimensions;
			if (fromIndex < num)
			{
				AddNodesBalanced(nodes, byDimension2, fromIndex, num - 1);
			}
			if (toIndex > num)
			{
				AddNodesBalanced(nodes, byDimension2, num + 1, toIndex);
			}
		}

		public void Balance()
		{
			List<KdTreeNode<TKey, TValue>> list = new List<KdTreeNode<TKey, TValue>>();
			AddNodesToList(root, list);
			Clear();
			AddNodesBalanced(list.ToArray(), 0, 0, list.Count - 1);
		}

		private void RemoveChildNodes(KdTreeNode<TKey, TValue> node)
		{
			for (int i = -1; i <= 1; i += 2)
			{
				if (node[i] != null)
				{
					RemoveChildNodes(node[i]);
					node[i] = null;
				}
			}
		}

		public void Clear()
		{
			if (root != null)
			{
				RemoveChildNodes(root);
			}
		}

		public void SaveToFile(string filename)
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			using (FileStream fileStream = File.Create(filename))
			{
				binaryFormatter.Serialize(fileStream, this);
				fileStream.Flush();
			}
		}

		public static KdTree<TKey, TValue> LoadFromFile(string filename)
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			using (FileStream serializationStream = File.Open(filename, FileMode.Open))
			{
				return (KdTree<TKey, TValue>)binaryFormatter.Deserialize(serializationStream);
			}
		}

		public IEnumerator<KdTreeNode<TKey, TValue>> GetEnumerator()
		{
			Stack<KdTreeNode<TKey, TValue>> left = new Stack<KdTreeNode<TKey, TValue>>();
			Stack<KdTreeNode<TKey, TValue>> right = new Stack<KdTreeNode<TKey, TValue>>();
			Action<KdTreeNode<TKey, TValue>> addLeft = (KdTreeNode<TKey, TValue> node) =>
			{
				if (node.LeftChild != null)
				{
					left.Push(node.LeftChild);
				}
			};
			Action<KdTreeNode<TKey, TValue>> addRight = (KdTreeNode<TKey, TValue> node) =>
			{
				if (node.RightChild != null)
				{
					right.Push(node.RightChild);
				}
			};
			if (root == null)
			{
				yield break;
			}
			yield return root;
			addLeft(root);
			addRight(root);
			while (true)
			{
				if (left.Any())
				{
					KdTreeNode<TKey, TValue> kdTreeNode = left.Pop();
					addLeft(kdTreeNode);
					addRight(kdTreeNode);
					yield return kdTreeNode;
					continue;
				}
				if (right.Any())
				{
					KdTreeNode<TKey, TValue> kdTreeNode2 = right.Pop();
					addLeft(kdTreeNode2);
					addRight(kdTreeNode2);
					yield return kdTreeNode2;
					continue;
				}
				break;
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
