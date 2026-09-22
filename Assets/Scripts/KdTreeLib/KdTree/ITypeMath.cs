namespace KdTree
{
	public interface ITypeMath<T>
	{
		T MinValue { get; }

		T MaxValue { get; }

		T Zero { get; }

		T NegativeInfinity { get; }

		T PositiveInfinity { get; }

		int Compare(T a, T b);

		T Min(T a, T b);

		T Max(T a, T b);

		bool AreEqual(T a, T b);

		bool AreEqual(T[] a, T[] b);

		T Add(T a, T b);

		T Subtract(T a, T b);

		T Multiply(T a, T b);

		T DistanceSquaredBetweenPoints(T[] a, T[] b);
	}
}
