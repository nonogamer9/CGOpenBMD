using System;
using Uniject;

namespace UnityEngine.Purchasing
{
	internal class EventQueue
	{
		private IAsyncWebUtil m_AsyncUtil;

		private static EventQueue QueueInstance;

		internal ProfileData Profile;

		private string TrackingUrl;

		private string EventUrl;

		internal object ProfileDict;

		private const int kMaxRetryDelayInSeconds = 300;

		private EventQueue(IUtil util, IAsyncWebUtil webUtil)
		{
			m_AsyncUtil = webUtil;
			Profile = ProfileData.Instance(util);
			ProfileDict = Profile.GetProfileDict();
			AdsIPC.InitAdsIPC(util);
		}

		public static EventQueue Instance(IUtil util, IAsyncWebUtil webUtil)
		{
			if (QueueInstance == null)
			{
				QueueInstance = new EventQueue(util, webUtil);
			}
			return QueueInstance;
		}

		internal void SetAdsUrl(string url)
		{
			TrackingUrl = url;
		}

		internal void SetIapUrl(string url)
		{
			EventUrl = url;
		}

		internal bool SendEvent(EventDestType dest, string json, string url = null, int? delayInSeconds = null)
		{
			if (m_AsyncUtil == null)
			{
				return false;
			}
			string target;
			switch (dest)
			{
			case EventDestType.IAP:
				target = ((url != null) ? url : EventUrl);
				if (target == null || json == null)
				{
					break;
				}
				m_AsyncUtil.Post(target, json, (string response) =>
				{
				}, (string error) =>
				{
					if (delayInSeconds.HasValue)
					{
						delayInSeconds = Math.Max(5, delayInSeconds.Value * 2);
						delayInSeconds = Math.Min(300, delayInSeconds.Value);
						m_AsyncUtil.Schedule(() =>
						{
							SendEvent(dest, json, target, delayInSeconds);
						}, delayInSeconds.Value);
					}
				});
				return true;
			case EventDestType.AdsTracking:
				target = ((url != null) ? url : TrackingUrl);
				if (target == null)
				{
					break;
				}
				m_AsyncUtil.Get(target, (string response) =>
				{
				}, (string error) =>
				{
				});
				return true;
			case EventDestType.AdsIPC:
				return AdsIPC.SendEvent(json);
			default:
				return false;
			}
			return false;
		}

		internal bool SendEvent(string json)
		{
			SendEvent(EventDestType.AdsTracking, null);
			SendEvent(EventDestType.IAP, json);
			return false;
		}
	}
}
