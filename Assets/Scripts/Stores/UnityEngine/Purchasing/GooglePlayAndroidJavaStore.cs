using System;
using Uniject;
using UnityEngine.XR;

namespace UnityEngine.Purchasing
{
	internal class GooglePlayAndroidJavaStore : AndroidJavaStore
	{
		private IUtil m_Util;

		public GooglePlayAndroidJavaStore(AndroidJavaObject store, IUtil util)
			: base(store)
		{
			m_Util = util;
			string text = "";
			if (Enum.IsDefined(typeof(PurchaseFailureReason), "DuplicateTransaction"))
			{
				text += "supportsPurchaseFailureReasonDuplicateTransaction";
			}
			GetStore().Call("SetFeatures", text);
		}

		public override void Purchase(string productJSON, string developerPayload)
		{
			if (!developerPayload.Contains("iapPromo"))
			{
				GetStore().Call("SetUnityVrEnabled", XRSettings.enabled);
			}
			base.Purchase(productJSON, developerPayload);
		}
	}
}
