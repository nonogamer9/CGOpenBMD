using System;
using System.Collections.Generic;

namespace UnityEngine.Advertisements
{
	[AddComponentMenu("")]
	internal sealed class CallbackExecutor : MonoBehaviour
	{
		private readonly Queue<Action<CallbackExecutor>> s_Queue = new Queue<Action<CallbackExecutor>>();

		public void Post(Action<CallbackExecutor> action)
		{
			lock (s_Queue)
			{
				s_Queue.Enqueue(action);
			}
		}

		private void Update()
		{
			lock (s_Queue)
			{
				while (s_Queue.Count > 0)
				{
					s_Queue.Dequeue()(this);
				}
			}
		}
	}
}
