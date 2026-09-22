namespace KdTree
{
	public interface IPriorityQueue<TItem, TPriority>
	{
		int Count { get; }

		void Enqueue(TItem item, TPriority priority);

		TItem Dequeue();
	}
}
