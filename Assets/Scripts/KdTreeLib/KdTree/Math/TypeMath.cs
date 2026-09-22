using System;

namespace KdTree.Math
{
	[Serializable]
	public abstract class TypeMath<T> : ITypeMath<T>
	{
		public abstract T MinValue { get; }

		public abstract T MaxValue { get; }

		public abstract T Zero { get; }

		public abstract T NegativeInfinity { get; }

		public abstract T PositiveInfinity { get; }

		public abstract int Compare(T a, T b);

		public abstract bool AreEqual(T a, T b);

		public virtual bool AreEqual(T[] a, T[] b)
		{
			if (a.Length != b.Length)
			{
				return false;
			}
			for (int i = 0; i < a.Length; i++)
			{
				if (!AreEqual(a[i], b[i]))
				{
					return false;
				}
			}
			return true;
		}

		public T Min(T a, T b)
		{
			if (Compare(a, b) < 0)
			{
				return a;
			}
			return b;
		}

		public T Max(T a, T b)
		{
			if (Compare(a, b) > 0)
			{
				return a;
			}
			return b;
		}

		public abstract T Add(T a, T b);

		public abstract T Subtract(T a, T b);

		public abstract T Multiply(T a, T b);

		public abstract T DistanceSquaredBetweenPoints(T[] a, T[] b);
	}
}
