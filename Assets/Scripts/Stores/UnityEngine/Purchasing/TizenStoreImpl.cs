using AOT;
using Uniject;
using UnityEngine.Purchasing.Extension;

namespace UnityEngine.Purchasing
{
	internal class TizenStoreImpl : JSONStore, ITizenStoreConfiguration, IStoreConfiguration
	{
		private static TizenStoreImpl instance;

		private INativeTizenStore m_Native;

		public TizenStoreImpl(IUtil util)
		{
			instance = this;
		}

		public void SetNativeStore(INativeTizenStore tizen)
		{
			SetNativeStore((INativeStore)tizen);
			m_Native = tizen;
			m_Native.SetUnityPurchasingCallback(MessageCallback);
		}

		public void SetGroupId(string group)
		{
			m_Native.SetGroupId(group);
		}

		[MonoPInvokeCallback(typeof(UnityNativePurchasingCallback))]
		private static void MessageCallback(string subject, string payload, string receipt, string transactionId)
		{
			instance.ProcessMessage(subject, payload, receipt, transactionId);
		}

		private void ProcessMessage(string subject, string payload, string receipt, string transactionId)
		{
			Debug.Log("[UnityIAP] ProcessMessage subject: " + subject + " payload: " + payload + " receipt: " + receipt + " transactionId: " + transactionId);
			switch (subject)
			{
			case "OnSetupFailed":
				OnSetupFailed(payload);
				break;
			case "OnProductsRetrieved":
				OnProductsRetrieved(payload);
				break;
			case "OnPurchaseSucceeded":
				OnPurchaseSucceeded(payload, receipt, transactionId);
				break;
			case "OnPurchaseFailed":
				OnPurchaseFailed(payload);
				break;
			}
		}
	}
}
