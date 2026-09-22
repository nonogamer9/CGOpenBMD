using System;
using System.Collections.Generic;
using System.Text;

namespace KdTree
{
	[Serializable]
	public class KdTreeNode<TKey, TValue>
	{
		public TKey[] Point;

		public TValue Value;

		public List<TValue> Duplicates;

		internal KdTreeNode<TKey, TValue> LeftChild;

		internal KdTreeNode<TKey, TValue> RightChild;

		internal KdTreeNode<TKey, TValue> this[int compare]
		{
			get
			{
				if (compare <= 0)
				{
					return LeftChild;
				}
				return RightChild;
			}
			set
			{
				if (compare <= 0)
				{
					LeftChild = value;
				}
				else
				{
					RightChild = value;
				}
			}
		}

		public bool IsLeaf
		{
			get
			{
				if (LeftChild == null)
				{
					return RightChild == null;
				}
				return false;
			}
		}

		public KdTreeNode()
		{
		}

		public KdTreeNode(TKey[] point, TValue value)
		{
			Point = point;
			Value = value;
		}

		public void AddDuplicate(TValue value)
		{
			if (Duplicates == null)
			{
				Duplicates = new List<TValue> { value };
			}
			else
			{
				Duplicates.Add(value);
			}
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < Point.Length; i++)
			{
				stringBuilder.Append(Point[i].ToString());
			}
			if (Value == null)
			{
				stringBuilder.Append("null");
			}
			else
			{
				stringBuilder.Append(Value.ToString());
			}
			return stringBuilder.ToString();
		}
	}
}
