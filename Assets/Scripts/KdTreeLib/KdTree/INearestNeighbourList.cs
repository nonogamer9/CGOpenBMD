namespace KdTree
{
	public interface INearestNeighbourList<TItem, TDistance>
	{
		int MaxCapacity { get; }

		int Count { get; }

		bool Add(TItem item, TDistance distance);

		TItem GetFurtherest();

		TItem RemoveFurtherest();
	}
}
