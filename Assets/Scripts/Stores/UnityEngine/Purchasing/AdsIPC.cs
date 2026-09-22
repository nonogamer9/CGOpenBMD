using System;
using System.Reflection;
using Uniject;

namespace UnityEngine.Purchasing
{
	internal class AdsIPC
	{
		private static string adsAdvertisementClassName = "UnityEngine.Advertisements.Purchasing,UnityEngine.Advertisements.";

		private static string adsMessageSendName = "SendEvent";

		private static Type adsAdvertisementType = null;

		private static MethodInfo adsMessageSend = null;

		internal static bool InitAdsIPC(IUtil util)
		{
			if (util.platform == RuntimePlatform.IPhonePlayer)
			{
				adsAdvertisementClassName += "iOS";
			}
			else
			{
				if (util.platform != RuntimePlatform.Android)
				{
					return false;
				}
				adsAdvertisementClassName += "Android";
			}
			try
			{
				adsAdvertisementType = Type.GetType(adsAdvertisementClassName);
				if (adsAdvertisementType != null)
				{
					adsMessageSend = adsAdvertisementType.GetMethod(adsMessageSendName);
					if (adsMessageSend != null)
					{
						return true;
					}
				}
			}
			catch
			{
			}
			return false;
		}

		internal static bool SendEvent(string json)
		{
			if (adsMessageSend != null)
			{
				adsMessageSend.Invoke(null, new string[1] { json });
				return true;
			}
			return false;
		}
	}
}
