using System;
using UnityEngine;

namespace DG.DemiLib
{
	[Serializable]
	public struct IntRange
	{
		public int min;

		public int max;

		public IntRange(int min, int max)
		{
			this.min = min;
			this.max = max;
		}

		public float RandomWithin()
		{
			return UnityEngine.Random.Range(min, max + 1);
		}
	}
}
