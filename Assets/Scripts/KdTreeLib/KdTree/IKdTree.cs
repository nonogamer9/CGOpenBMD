using System.Collections;
using System.Collections.Generic;

namespace KdTree
{
	public interface IKdTree<TKey, TValue> : IEnumerable<KdTreeNode<TKey, TValue>>, IEnumerable
	{
		int Count { get; }

		bool Add(TKey[] point, TValue value);

		bool TryFindValueAt(TKey[] point, out TValue value);

		TValue FindValueAt(TKey[] point);

		bool TryFindValue(TValue value, out TKey[] point);

		TKey[] FindValue(TValue value);

		KdTreeNode<TKey, TValue>[] RadialSearch(TKey[] center, TKey radius, int count);

		void RemoveAt(TKey[] point);

		void Clear();

		KdTreeNode<TKey, TValue>[] GetNearestNeighbours(TKey[] point, int count = int.MaxValue);
	}
}
