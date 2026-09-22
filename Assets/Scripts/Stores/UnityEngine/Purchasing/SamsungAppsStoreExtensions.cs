using System;
using UnityEngine.Purchasing.Extension;

namespace UnityEngine.Purchasing
{
	internal class SamsungAppsStoreExtensions : AndroidJavaProxy, ISamsungAppsCallback, ISamsungAppsExtensions, IStoreExtension, ISamsungAppsConfiguration, IStoreConfiguration
	{
		private Action<bool> m_RestoreCallback;

		private AndroidJavaObject m_Java;

		public SamsungAppsStoreExtensions()
			: base("com.unity.purchasing.samsung.ISamsungAppsStoreCallback")
		{
		}

		public void SetAndroidJavaObject(AndroidJavaObject java)
		{
			m_Java = java;
		}

		public void SetMode(SamsungAppsMode mode)
		{
			m_Java.Call("setMode", mode.ToString());
		}

		public void RestoreTransactions(Action<bool> callback)
		{
			m_RestoreCallback = callback;
			m_Java.Call("restoreTransactions");
		}

		public void OnTransactionsRestored(bool result)
		{
			if (m_RestoreCallback != null)
			{
				m_RestoreCallback(result);
			}
		}
	}
}
