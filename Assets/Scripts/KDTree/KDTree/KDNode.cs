using System;

namespace KDTree
{
	public class KDNode<T>
	{
		protected internal int iDimensions;

		protected internal int iBucketCapacity;

		protected internal double[][] tPoints;

		protected internal T[] tData;

		protected internal KDNode<T> pLeft;

		protected internal KDNode<T> pRight;

		protected internal int iSplitDimension;

		protected internal double fSplitValue;

		protected internal double[] tMinBound;

		protected internal double[] tMaxBound;

		protected internal bool bSinglePoint;

		public int Size { get; private set; }

		public bool IsLeaf
		{
			get
			{
				return tPoints != null;
			}
		}

		protected KDNode(int iDimensions, int iBucketCapacity)
		{
			this.iDimensions = iDimensions;
			this.iBucketCapacity = iBucketCapacity;
			Size = 0;
			bSinglePoint = true;
			tPoints = new double[iBucketCapacity + 1][];
			tData = new T[iBucketCapacity + 1];
		}

		public void AddPoint(double[] tPoint, T kValue)
		{
			KDNode<T> kDNode = this;
			while (!kDNode.IsLeaf)
			{
				kDNode.ExtendBounds(tPoint);
				kDNode.Size++;
				kDNode = ((!(tPoint[kDNode.iSplitDimension] > kDNode.fSplitValue)) ? kDNode.pLeft : kDNode.pRight);
			}
			kDNode.AddLeafPoint(tPoint, kValue);
		}

		private void AddLeafPoint(double[] tPoint, T kValue)
		{
			tPoints[Size] = tPoint;
			tData[Size] = kValue;
			ExtendBounds(tPoint);
			Size++;
			if (Size == tPoints.Length - 1)
			{
				if (CalculateSplit())
				{
					SplitLeafNode();
				}
				else
				{
					IncreaseLeafCapacity();
				}
			}
		}

		private bool CheckBounds(double[] tPoint)
		{
			for (int i = 0; i < iDimensions; i++)
			{
				if (tPoint[i] > tMaxBound[i])
				{
					return false;
				}
				if (tPoint[i] < tMinBound[i])
				{
					return false;
				}
			}
			return true;
		}

		private void ExtendBounds(double[] tPoint)
		{
			if (tMinBound == null)
			{
				tMinBound = new double[iDimensions];
				tMaxBound = new double[iDimensions];
				Array.Copy(tPoint, tMinBound, iDimensions);
				Array.Copy(tPoint, tMaxBound, iDimensions);
				return;
			}
			for (int i = 0; i < iDimensions; i++)
			{
				if (double.IsNaN(tPoint[i]))
				{
					if (!double.IsNaN(tMinBound[i]) || !double.IsNaN(tMaxBound[i]))
					{
						bSinglePoint = false;
					}
					tMinBound[i] = double.NaN;
					tMaxBound[i] = double.NaN;
				}
				else if (tMinBound[i] > tPoint[i])
				{
					tMinBound[i] = tPoint[i];
					bSinglePoint = false;
				}
				else if (tMaxBound[i] < tPoint[i])
				{
					tMaxBound[i] = tPoint[i];
					bSinglePoint = false;
				}
			}
		}

		private void IncreaseLeafCapacity()
		{
			Array.Resize(ref tPoints, tPoints.Length * 2);
			Array.Resize(ref tData, tData.Length * 2);
		}

		private bool CalculateSplit()
		{
			if (bSinglePoint)
			{
				return false;
			}
			double num = 0.0;
			for (int i = 0; i < iDimensions; i++)
			{
				double num2 = tMaxBound[i] - tMinBound[i];
				if (double.IsNaN(num2))
				{
					num2 = 0.0;
				}
				if (num2 > num)
				{
					iSplitDimension = i;
					num = num2;
				}
			}
			if (num == 0.0)
			{
				return false;
			}
			fSplitValue = (tMinBound[iSplitDimension] + tMaxBound[iSplitDimension]) * 0.5;
			if (fSplitValue == double.PositiveInfinity)
			{
				fSplitValue = double.MaxValue;
			}
			else if (fSplitValue == double.NegativeInfinity)
			{
				fSplitValue = double.MinValue;
			}
			if (fSplitValue == tMaxBound[iSplitDimension])
			{
				fSplitValue = tMinBound[iSplitDimension];
			}
			return true;
		}

		private void SplitLeafNode()
		{
			pRight = new KDNode<T>(iDimensions, iBucketCapacity);
			pLeft = new KDNode<T>(iDimensions, iBucketCapacity);
			for (int i = 0; i < Size; i++)
			{
				double[] array = tPoints[i];
				T kValue = tData[i];
				if (array[iSplitDimension] > fSplitValue)
				{
					pRight.AddLeafPoint(array, kValue);
				}
				else
				{
					pLeft.AddLeafPoint(array, kValue);
				}
			}
			tPoints = null;
			tData = null;
		}
	}
}
