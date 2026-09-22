using System;

namespace KdTree
{
	public class PriorityQueue<TItem, TPriority> : IPriorityQueue<TItem, TPriority>
	{
		private ITypeMath<TPriority> priorityMath;

		private ItemPriority<TItem, TPriority>[] queue;

		private int capacity;

		private int count;

		public int Count
		{
			get
			{
				return count;
			}
		}

		public PriorityQueue(int capacity, ITypeMath<TPriority> priorityMath)
		{
			if (capacity <= 0)
			{
				throw new ArgumentException("Capacity must be greater than zero");
			}
			this.capacity = capacity;
			queue = new ItemPriority<TItem, TPriority>[capacity];
			this.priorityMath = priorityMath;
		}

		private void ExpandCapacity()
		{
			capacity *= 2;
			ItemPriority<TItem, TPriority>[] destinationArray = new ItemPriority<TItem, TPriority>[capacity];
			Array.Copy(queue, destinationArray, queue.Length);
			queue = destinationArray;
		}

		public void Enqueue(TItem item, TPriority priority)
		{
			if (++count > capacity)
			{
				ExpandCapacity();
			}
			int num = count - 1;
			queue[num] = new ItemPriority<TItem, TPriority>
			{
				Item = item,
				Priority = priority
			};
			ReorderItem(num, -1);
		}

		public TItem Dequeue()
		{
			TItem item = queue[0].Item;
			queue[0].Item = default(TItem);
			queue[0].Priority = priorityMath.MinValue;
			ReorderItem(0, 1);
			count--;
			return item;
		}

		private void ReorderItem(int index, int direction)
		{
			if (direction != -1 && direction != 1)
			{
				throw new ArgumentException("Invalid Direction");
			}
			ItemPriority<TItem, TPriority> itemPriority = queue[index];
			for (int i = index + direction; i >= 0 && i < count; i += direction)
			{
				ItemPriority<TItem, TPriority> itemPriority2 = queue[i];
				int num = priorityMath.Compare(itemPriority.Priority, itemPriority2.Priority);
				if ((direction == -1 && num > 0) || (direction == 1 && num < 0))
				{
					queue[index] = itemPriority2;
					queue[i] = itemPriority;
					index += direction;
					continue;
				}
				break;
			}
		}

		public TItem GetHighest()
		{
			if (count == 0)
			{
				throw new Exception("Queue is empty");
			}
			return queue[0].Item;
		}

		public TPriority GetHighestPriority()
		{
			if (count == 0)
			{
				throw new Exception("Queue is empty");
			}
			return queue[0].Priority;
		}
	}
}
