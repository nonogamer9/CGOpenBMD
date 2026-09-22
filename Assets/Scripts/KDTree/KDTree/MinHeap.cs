using System;

namespace KDTree
{
	public class MinHeap<T>
	{
		private static int DEFAULT_SIZE = 64;

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

		public MinHeap()
			: this(DEFAULT_SIZE)
		{
		}

		public MinHeap(int iCapacity)
		{
			tData = new T[iCapacity];
			tKeys = new double[iCapacity];
			Capacity = iCapacity;
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
			tData[Size] = value;
			tKeys[Size] = key;
			SiftUp(Size);
			Size++;
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
			SiftDown(0);
		}

		private void SiftUp(int iChild)
		{
			int num = (iChild - 1) / 2;
			while (iChild != 0 && tKeys[iChild] < tKeys[num])
			{
				T val = tData[num];
				double num2 = tKeys[num];
				tData[num] = tData[iChild];
				tKeys[num] = tKeys[iChild];
				tData[iChild] = val;
				tKeys[iChild] = num2;
				iChild = num;
				num = (iChild - 1) / 2;
			}
		}

		private void SiftDown(int iParent)
		{
			int num = iParent * 2 + 1;
			while (num < Size)
			{
				if (num + 1 < Size && tKeys[num] > tKeys[num + 1])
				{
					num++;
				}
				if (tKeys[iParent] > tKeys[num])
				{
					T val = tData[iParent];
					double num2 = tKeys[iParent];
					tData[iParent] = tData[num];
					tKeys[iParent] = tKeys[num];
					tData[num] = val;
					tKeys[num] = num2;
					iParent = num;
					num = iParent * 2 + 1;
					continue;
				}
				break;
			}
		}
	}
}
