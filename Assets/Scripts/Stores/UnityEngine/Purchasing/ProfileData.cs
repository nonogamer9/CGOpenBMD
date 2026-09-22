using System;
using System.Collections.Generic;
using Uniject;

namespace UnityEngine.Purchasing
{
	internal class ProfileData
	{
		private IUtil m_Util;

		private static ProfileData ProfileInstance;

		private const string kConnectSessionInfoApplicationId = "appid";

		private const string kConnectSessionInfoUserId = "userid";

		private const string kConnectSessionInfoSessionId = "sessionid";

		private const string kConnectSessionInfoPlatformName = "platform";

		private const string kConnectSessionInfoPlatformId = "platformid";

		private const string kConnectSessionInfoSdkVersion = "sdk_ver";

		private const string kConnectSessionInfoDeviceId = "deviceid";

		private const string kConnectSessionInfoBuildGuid = "build_guid";

		private const string kConnectSessionInfoIapVersion = "iap_ver";

		private const string kConnectSessionInfoAdsGamerToken = "gamerToken";

		private const string kConnectSessionInfoAdsTrackingOptOut = "trackingOptOut";

		private const string kConnectSessionInfoAdsGameId = "gameId";

		private const string kConnectSessionInfoAdsABGroup = "abGroup";

		private const string kConnectSessionInfoStoreABGroup = "store_abgroup";

		private const string kConnectSessionInfoCatalogId = "catalogid";

		private const string kConnectSessionInfoMonetizationId = "umpid";

		private const string kConnectSessionInfoStoreTest = "iap_test";

		private const string kConnectSessionInfoStoreName = "store";

		private const string kConnectSessionInfoGameVersion = "game_ver";

		private const string kOsVersion = "osv";

		private const string kDpi = "ppi";

		private const string kWidth = "w";

		private const string kHeight = "h";

		private const string kOrient = "orient";

		public string AppId { get; internal set; }

		public string UserId { get; internal set; }

		public ulong SessionId { get; internal set; }

		public string Platform { get; internal set; }

		public int PlatformId { get; internal set; }

		public string SdkVer { get; internal set; }

		public string OsVer { get; internal set; }

		public int ScreenWidth { get; internal set; }

		public int ScreenHeight { get; internal set; }

		public float ScreenDpi { get; internal set; }

		public string ScreenOrientation { get; internal set; }

		public string DeviceId { get; internal set; }

		public string BuildGUID { get; internal set; }

		public string IapVer { get; internal set; }

		public string AdsGamerToken { get; internal set; }

		public bool? TrackingOptOut { get; internal set; }

		public int? AdsABGroup { get; internal set; }

		public string AdsGameId { get; internal set; }

		public int? StoreABGroup { get; internal set; }

		public string CatalogId { get; internal set; }

		public string MonetizationId { get; internal set; }

		public string StoreName { get; internal set; }

		public string GameVersion { get; internal set; }

		public bool? StoreTestEnabled { get; internal set; }

		private ProfileData(IUtil util)
		{
			m_Util = util;
			AppId = util.cloudProjectId;
			Platform = util.platform.ToString();
			PlatformId = (int)util.platform;
			SdkVer = util.unityVersion;
			OsVer = util.operatingSystem;
			DeviceId = util.deviceUniqueIdentifier;
			GameVersion = util.gameVersion;
			IapVer = Promo.Version();
			UserId = util.userId;
			SessionId = util.sessionId;
			ScreenWidth = util.screenWidth;
			ScreenHeight = util.screenHeight;
			ScreenDpi = util.screenDpi;
			ScreenOrientation = util.screenOrientation;
		}

		internal Dictionary<string, object> GetProfileDict()
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary.Add("appid", AppId);
			dictionary.Add("platform", Platform);
			dictionary.Add("platformid", PlatformId);
			if (!string.IsNullOrEmpty(AdsGameId))
			{
				dictionary.Add("gameId", AdsGameId);
			}
			dictionary.Add("sdk_ver", SdkVer);
			if (DeviceId != "n/a")
			{
				dictionary.Add("deviceid", DeviceId);
			}
			if (!string.IsNullOrEmpty(UserId))
			{
				dictionary.Add("userid", UserId);
			}
			if (SessionId != 0)
			{
				dictionary.Add("sessionid", SessionId);
			}
			if (!string.IsNullOrEmpty(BuildGUID))
			{
				dictionary.Add("build_guid", BuildGUID);
			}
			if (!string.IsNullOrEmpty(IapVer))
			{
				dictionary.Add("iap_ver", IapVer);
			}
			if (!string.IsNullOrEmpty(AdsGamerToken))
			{
				dictionary.Add("gamerToken", AdsGamerToken);
			}
			if (TrackingOptOut.HasValue)
			{
				dictionary.Add("trackingOptOut", TrackingOptOut);
			}
			if (AdsABGroup.HasValue)
			{
				dictionary.Add("abGroup", AdsABGroup);
			}
			if (StoreABGroup.HasValue)
			{
				dictionary.Add("store_abgroup", StoreABGroup);
			}
			if (!string.IsNullOrEmpty(CatalogId))
			{
				dictionary.Add("catalogid", CatalogId);
			}
			if (StoreTestEnabled.HasValue)
			{
				dictionary.Add("iap_test", StoreTestEnabled);
			}
			if (!string.IsNullOrEmpty(StoreName))
			{
				dictionary.Add("store", StoreName);
			}
			if (!string.IsNullOrEmpty(GameVersion))
			{
				dictionary.Add("game_ver", GameVersion);
			}
			if (!string.IsNullOrEmpty(OsVer))
			{
				dictionary.Add("osv", OsVer);
			}
			dictionary.Add("w", ScreenWidth);
			dictionary.Add("h", ScreenHeight);
			dictionary.Add("ppi", ScreenDpi);
			ScreenOrientation = m_Util.screenOrientation;
			if (!string.IsNullOrEmpty(ScreenOrientation))
			{
				dictionary.Add("orient", ScreenOrientation);
			}
			return dictionary;
		}

		internal Dictionary<string, object> GetProfileIds()
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary.Add("appid", AppId);
			if (DeviceId != "n/a")
			{
				dictionary.Add("deviceid", DeviceId);
			}
			if (!string.IsNullOrEmpty(UserId))
			{
				dictionary.Add("userid", UserId);
			}
			if (!string.IsNullOrEmpty(AdsGamerToken))
			{
				dictionary.Add("gamerToken", AdsGamerToken);
			}
			if (TrackingOptOut.HasValue)
			{
				dictionary.Add("trackingOptOut", TrackingOptOut);
			}
			if (!string.IsNullOrEmpty(MonetizationId))
			{
				dictionary.Add("umpid", MonetizationId);
			}
			if (SessionId != 0)
			{
				string value = Convert.ToString(SessionId);
				if (!string.IsNullOrEmpty(value))
				{
					dictionary.Add("sessionid", value);
				}
			}
			return dictionary;
		}

		internal static ProfileData Instance(IUtil util)
		{
			if (ProfileInstance == null)
			{
				ProfileInstance = new ProfileData(util);
			}
			return ProfileInstance;
		}

		internal void SetGamerToken(string gamerToken)
		{
			if (!string.IsNullOrEmpty(gamerToken))
			{
				AdsGamerToken = gamerToken;
			}
		}

		internal void SetTrackingOptOut(bool? trackingOptOut)
		{
			if (trackingOptOut.HasValue)
			{
				TrackingOptOut = trackingOptOut;
			}
		}

		internal void SetGameId(string gameid)
		{
			if (!string.IsNullOrEmpty(gameid))
			{
				AdsGameId = gameid;
			}
		}

		internal void SetABGroup(int? abgroup)
		{
			if (abgroup.HasValue && abgroup > 0)
			{
				AdsABGroup = abgroup;
			}
		}

		internal void SetStoreABGroup(int? abgroup)
		{
			if (abgroup.HasValue)
			{
				StoreABGroup = abgroup;
			}
		}

		internal void SetCatalogId(string storeid)
		{
			if (storeid != null)
			{
				CatalogId = storeid;
			}
		}

		internal void SetMonetizationId(string umpid)
		{
			if (!string.IsNullOrEmpty(umpid))
			{
				MonetizationId = umpid;
			}
		}

		internal void SetStoreTestEnabled(bool enable)
		{
			StoreTestEnabled = enable;
		}

		internal void SetStoreName(string storename)
		{
			if (!string.IsNullOrEmpty(storename))
			{
				StoreName = storename;
			}
		}
	}
}
