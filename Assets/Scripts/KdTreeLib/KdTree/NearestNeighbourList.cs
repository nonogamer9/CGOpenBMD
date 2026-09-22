using System;

namespace KdTree
{
	public class NearestNeighbourList<TItem, TDistance> : INearestNeighbourList<TItem, TDistance>
	{
		private PriorityQueue<TItem, TDistance> queue;

		private ITypeMath<TDistance> distanceMath;

		private int maxCapacity;

		public int MaxCapacity
		{
			get
			{
				return maxCapacity;
			}
		}

		public int Count
		{
			get
			{
				return queue.Count;
			}
		}

		public bool IsCapacityReached
		{
			get
			{
				return Count == MaxCapacity;
			}
		}

		public NearestNeighbourList(int maxCapacity, ITypeMath<TDistance> distanceMath)
		{
			this.maxCapacity = maxCapacity;
			this.distanceMath = distanceMath;
			queue = new PriorityQueue<TItem, TDistance>(maxCapacity, distanceMath);
		}

		public bool Add(TItem item, TDistance distance)
		{
			if (queue.Count >= maxCapacity)
			{
				if (distanceMath.Compare(distance, queue.GetHighestPriority()) < 0)
				{
					queue.Dequeue();
					queue.Enqueue(item, distance);
					return true;
				}
				return false;
			}
			queue.Enqueue(item, distance);
			return true;
		}

		public TItem GetFurtherest()
		{
			if (Count == 0)
			{
				throw new Exception("List is empty");
			}
			return queue.GetHighest();
		}

		public TDistance GetFurtherestDistance()
		{
			if (Count == 0)
			{
				throw new Exception("List is empty");
			}
			return queue.GetHighestPriority();
		}

		public TItem RemoveFurtherest()
		{
			return queue.Dequeue();
		}
	}
}
