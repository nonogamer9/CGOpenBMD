using System;
using System.Collections;

namespace KDTree
{
	public class NearestNeighbour<T> : IEnumerator
	{
		private double[] tSearchPoint;

		private DistanceFunctions kDistanceFunction;

		private MinHeap<KDNode<T>> pPending;

		private IntervalHeap<T> pEvaluated;

		private KDNode<T> pRoot = null;

		private int iMaxPointsReturned = 0;

		private int iPointsRemaining;

		private double fThreshold;

		private double _CurrentDistance = -1.0;

		private T _Current = default(T);

		object IEnumerator.Current
		{
			get
			{
				return _Current;
			}
		}

		public double CurrentDistance
		{
			get
			{
				return _CurrentDistance;
			}
		}

		public T Current
		{
			get
			{
				return _Current;
			}
		}

		public NearestNeighbour(KDNode<T> pRoot, double[] tSearchPoint, DistanceFunctions kDistance, int iMaxPoints, double fThreshold)
		{
			if (tSearchPoint.Length != pRoot.iDimensions)
			{
				throw new Exception("Dimensionality of search point and kd-tree are not the same.");
			}
			this.tSearchPoint = new double[tSearchPoint.Length];
			Array.Copy(tSearchPoint, this.tSearchPoint, tSearchPoint.Length);
			iPointsRemaining = Math.Min(iMaxPoints, pRoot.Size);
			this.fThreshold = fThreshold;
			kDistanceFunction = kDistance;
			this.pRoot = pRoot;
			iMaxPointsReturned = iMaxPoints;
			_CurrentDistance = -1.0;
			pEvaluated = new IntervalHeap<T>();
			pPending = new MinHeap<KDNode<T>>();
			pPending.Insert(0.0, pRoot);
		}

		public bool MoveNext()
		{
			if (iPointsRemaining == 0)
			{
				_Current = default(T);
				return false;
			}
			while (pPending.Size > 0 && (pEvaluated.Size == 0 || pPending.MinKey < pEvaluated.MinKey))
			{
				KDNode<T> kDNode = pPending.Min;
				pPending.RemoveMin();
				while (!kDNode.IsLeaf)
				{
					KDNode<T> kDNode2;
					if (tSearchPoint[kDNode.iSplitDimension] > kDNode.fSplitValue)
					{
						kDNode2 = kDNode.pLeft;
						kDNode = kDNode.pRight;
					}
					else
					{
						kDNode2 = kDNode.pRight;
						kDNode = kDNode.pLeft;
					}
					double num = kDistanceFunction.DistanceToRectangle(tSearchPoint, kDNode2.tMinBound, kDNode2.tMaxBound);
					if ((!(fThreshold >= 0.0) || !(num > fThreshold)) && (pEvaluated.Size < iPointsRemaining || num <= pEvaluated.MaxKey))
					{
						pPending.Insert(num, kDNode2);
					}
				}
				if (kDNode.bSinglePoint)
				{
					double num = kDistanceFunction.Distance(kDNode.tPoints[0], tSearchPoint);
					if ((fThreshold >= 0.0 && num >= fThreshold) || (pEvaluated.Size >= iPointsRemaining && !(num <= pEvaluated.MaxKey)))
					{
						continue;
					}
					for (int i = 0; i < kDNode.Size; i++)
					{
						if (pEvaluated.Size == iPointsRemaining)
						{
							pEvaluated.ReplaceMax(num, kDNode.tData[i]);
						}
						else
						{
							pEvaluated.Insert(num, kDNode.tData[i]);
						}
					}
					continue;
				}
				for (int i = 0; i < kDNode.Size; i++)
				{
					double num = kDistanceFunction.Distance(kDNode.tPoints[i], tSearchPoint);
					if (!(fThreshold >= 0.0) || !(num >= fThreshold))
					{
						if (pEvaluated.Size < iPointsRemaining)
						{
							pEvaluated.Insert(num, kDNode.tData[i]);
						}
						else if (num < pEvaluated.MaxKey)
						{
							pEvaluated.ReplaceMax(num, kDNode.tData[i]);
						}
					}
				}
			}
			if (pEvaluated.Size == 0)
			{
				return false;
			}
			iPointsRemaining--;
			_CurrentDistance = pEvaluated.MinKey;
			_Current = pEvaluated.Min;
			pEvaluated.RemoveMin();
			return true;
		}

		public void Reset()
		{
			iPointsRemaining = Math.Min(iMaxPointsReturned, pRoot.Size);
			_CurrentDistance = -1.0;
			pEvaluated = new IntervalHeap<T>();
			pPending = new MinHeap<KDNode<T>>();
			pPending.Insert(0.0, pRoot);
		}
	}
}
