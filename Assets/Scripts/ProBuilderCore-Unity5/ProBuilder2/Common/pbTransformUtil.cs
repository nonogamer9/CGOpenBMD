using System.Collections.Generic;
using UnityEngine;

namespace ProBuilder2.Common
{
	public static class pbTransformUtil
	{
		private static Dictionary<Transform, Transform[]> _childrenStack = new Dictionary<Transform, Transform[]>();

		public static void UnparentChildren(Transform t)
		{
			Transform[] array = new Transform[t.childCount];
			for (int i = 0; i < t.childCount; i++)
			{
				(array[i] = t.GetChild(i)).SetParent(null, true);
			}
			_childrenStack.Add(t, array);
		}

		public static void ReparentChildren(Transform t)
		{
			Transform[] value;
			if (_childrenStack.TryGetValue(t, out value))
			{
				Transform[] array = value;
				foreach (Transform transform in array)
				{
					transform.SetParent(t, true);
				}
				_childrenStack.Remove(t);
			}
		}
	}
}
