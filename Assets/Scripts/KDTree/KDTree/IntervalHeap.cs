using System;

namespace KDTree
{
	public class IntervalHeap<T>
	{
		private const int DEFAULT_SIZE = 64;

		private T[] tData;

		private double[] tKeys;

		public int Size { get; private set; }

		public int Capacity { get; private set; }

		public T Min
		{
			get
			{
				if (Size == 0)
				{
					throw new Exception();
				}
				return tData[0];
			}
		}

		public T Max
		{
			get
			{
				if (Size == 0)
				{
					throw new Exception();
				}
				if (Size == 1)
				{
					return tData[0];
				}
				return tData[1];
			}
		}

		public double MinKey
		{
			get
			{
				if (Size == 0)
				{
					throw new Exception();
				}
				return tKeys[0];
			}
		}

		public double MaxKey
		{
			get
			{
				if (Size == 0)
				{
					throw new Exception();
				}
				if (Size == 1)
				{
					return tKeys[0];
				}
				return tKeys[1];
			}
		}

		public IntervalHeap()
			: this(64)
		{
		}

		public IntervalHeap(int capacity)
		{
			tData = new T[capacity];
			tKeys = new double[capacity];
			Capacity = capacity;
			Size = 0;
		}

		public void Insert(double key, T value)
		{
			if (Size >= Capacity)
			{
				Capacity *= 2;
				T[] destinationArray = new T[Capacity];
				Array.Copy(tData, destinationArray, tData.Length);
				tData = destinationArray;
				double[] destinationArray2 = new double[Capacity];
				Array.Copy(tKeys, destinationArray2, tKeys.Length);
				tKeys = destinationArray2;
			}
			Size++;
			tData[Size - 1] = value;
			tKeys[Size - 1] = key;
			SiftInsertedValueUp();
		}

		public void RemoveMin()
		{
			if (Size == 0)
			{
				throw new Exception();
			}
			Size--;
			tData[0] = tData[Size];
			tKeys[0] = tKeys[Size];
			tData[Size] = default(T);
			SiftDownMin(0);
		}

		public void ReplaceMin(double key, T value)
		{
			if (Size == 0)
			{
				throw new Exception();
			}
			tData[0] = value;
			tKeys[0] = key;
			if (Size > 1)
			{
				if (tKeys[1] < key)
				{
					Swap(0, 1);
				}
				SiftDownMin(0);
			}
		}

		public void RemoveMax()
		{
			if (Size == 0)
			{
				throw new Exception();
			}
			if (Size == 1)
			{
				RemoveMin();
				return;
			}
			Size--;
			tData[1] = tData[Size];
			tKeys[1] = tKeys[Size];
			tData[Size] = default(T);
			SiftDownMax(1);
		}

		public void ReplaceMax(double key, T value)
		{
			if (Size == 0)
			{
				throw new Exception();
			}
			if (Size == 1)
			{
				ReplaceMin(key, value);
				return;
			}
			tData[1] = value;
			tKeys[1] = key;
			if (key < tKeys[0])
			{
				Swap(0, 1);
			}
			SiftDownMax(1);
		}

		private int Swap(int x, int y)
		{
			T val = tData[y];
			double num = tKeys[y];
			tData[y] = tData[x];
			tKeys[y] = tKeys[x];
			tData[x] = val;
			tKeys[x] = num;
			return y;
		}

		private void SiftInsertedValueUp()
		{
			int num = Size - 1;
			switch (num)
			{
			case 0:
				return;
			case 1:
				if (tKeys[num] < tKeys[num - 1])
				{
					Swap(num, num - 1);
				}
				return;
			}
			if (num % 2 == 1)
			{
				int num2 = (num / 2 - 1) | 1;
				if (tKeys[num] < tKeys[num - 1])
				{
					num = Swap(num, num - 1);
					if (tKeys[num] < tKeys[num2 - 1])
					{
						num = Swap(num, num2 - 1);
						SiftUpMin(num);
					}
				}
				else if (tKeys[num] > tKeys[num2])
				{
					num = Swap(num, num2);
					SiftUpMax(num);
				}
			}
			else
			{
				int num2 = (num / 2 - 1) | 1;
				if (tKeys[num] > tKeys[num2])
				{
					num = Swap(num, num2);
					SiftUpMax(num);
				}
				else if (tKeys[num] < tKeys[num2 - 1])
				{
					num = Swap(num, num2 - 1);
					SiftUpMin(num);
				}
			}
		}

		private void SiftUpMin(int iChild)
		{
			int num = (iChild / 2 - 1) & -2;
			while (num >= 0 && tKeys[iChild] < tKeys[num])
			{
				Swap(iChild, num);
				iChild = num;
				num = (iChild / 2 - 1) & -2;
			}
		}

		private void SiftUpMax(int iChild)
		{
			int num = (iChild / 2 - 1) | 1;
			while (num >= 0 && tKeys[iChild] > tKeys[num])
			{
				Swap(iChild, num);
				iChild = num;
				num = (iChild / 2 - 1) | 1;
			}
		}

		private void SiftDownMin(int iParent)
		{
			int num = iParent * 2 + 2;
			while (num < Size)
			{
				if (num + 2 < Size && tKeys[num + 2] < tKeys[num])
				{
					num += 2;
				}
				if (tKeys[num] < tKeys[iParent])
				{
					Swap(iParent, num);
					if (num + 1 < Size && tKeys[num + 1] < tKeys[num])
					{
						Swap(num, num + 1);
					}
					iParent = num;
					num = iParent * 2 + 2;
					continue;
				}
				break;
			}
		}

		private void SiftDownMax(int iParent)
		{
			int num = iParent * 2 + 1;
			while (num <= Size)
			{
				if (num == Size)
				{
					if (tKeys[num - 1] > tKeys[iParent])
					{
						Swap(iParent, num - 1);
					}
					break;
				}
				if (num + 2 == Size)
				{
					if (tKeys[num + 1] > tKeys[num])
					{
						if (tKeys[num + 1] > tKeys[iParent])
						{
							Swap(iParent, num + 1);
						}
						break;
					}
				}
				else if (num + 2 < Size && tKeys[num + 2] > tKeys[num])
				{
					num += 2;
				}
				if (tKeys[num] > tKeys[iParent])
				{
					Swap(iParent, num);
					if (tKeys[num - 1] > tKeys[num])
					{
						Swap(num, num - 1);
					}
					iParent = num;
					num = iParent * 2 + 1;
					continue;
				}
				break;
			}
		}
	}
}
