using System;
using UnityEngine.Advertisements.Android;
using UnityEngine.Advertisements.Editor;
using UnityEngine.Advertisements.iOS;

namespace UnityEngine.Advertisements
{
	public static class Advertisement
	{
		[Flags]
		private enum DebugLevelInternal
		{
			None = 0,
			Error = 1,
			Warning = 2,
			Info = 4,
			Debug = 8
		}

		[Obsolete("Use Advertisement.debugMode instead.")]
		[Flags]
		public enum DebugLevel
		{
			None = 0,
			Error = 1,
			Warning = 2,
			Info = 4,
			Debug = 8
		}

		private static bool s_Initialized;

		private static IPlatform s_Platform;

		private static bool s_EditorSupportedPlatform;

		private static bool s_Showing;

		private static DebugLevelInternal s_DebugLevel = ((!Debug.isDebugBuild) ? (DebugLevelInternal.Error | DebugLevelInternal.Warning | DebugLevelInternal.Info) : (DebugLevelInternal.Error | DebugLevelInternal.Warning | DebugLevelInternal.Info | DebugLevelInternal.Debug));

		private static bool initializeOnStartup
		{
			get
			{
				return UnityAdsSettings.initializeOnStartup;
			}
		}

		[Obsolete("Use Advertisement.debugMode instead.")]
		public static DebugLevel debugLevel
		{
			get
			{
				return (DebugLevel)s_DebugLevel;
			}
			set
			{
				s_DebugLevel = (DebugLevelInternal)value;
			}
		}

		public static bool isInitialized
		{
			get
			{
				return s_Initialized;
			}
			private set
			{
				s_Initialized = value;
			}
		}

		public static bool isSupported
		{
			get
			{
				if ((Application.isEditor && s_EditorSupportedPlatform) || Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.Android)
				{
					return IsEnabled();
				}
				return false;
			}
		}

		public static bool debugMode
		{
			get
			{
				return s_Platform.debugMode;
			}
			set
			{
				s_Platform.debugMode = value;
			}
		}

		public static bool testMode
		{
			get
			{
				return UnityAdsSettings.testMode;
			}
		}

		public static string gameId
		{
			get
			{
				return UnityAdsSettings.GetGameId(Application.platform);
			}
		}

		public static string version
		{
			get
			{
				return s_Platform.version;
			}
		}

		public static bool isShowing
		{
			get
			{
				return s_Showing;
			}
			private set
			{
				s_Showing = value;
			}
		}

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void LoadRuntime()
		{
			if (s_Platform != null || Application.isEditor || !isSupported)
			{
				return;
			}
			try
			{
				switch (Application.platform)
				{
				case RuntimePlatform.Android:
					s_Platform = new UnityEngine.Advertisements.Android.Platform();
					break;
				case RuntimePlatform.IPhonePlayer:
					s_Platform = new UnityEngine.Advertisements.iOS.Platform();
					break;
				default:
					s_Platform = new UnsupportedPlatform();
					break;
				}
			}
			catch (Exception exception)
			{
				Debug.LogError("Initializing Unity Ads.");
				Debug.LogException(exception);
				s_Platform = new UnsupportedPlatform();
			}
			Load();
		}

		private static void LoadEditor(string extensionPath, bool supportedPlatform)
		{
			if (s_Platform == null)
			{
				if (supportedPlatform)
				{
					s_Platform = new UnityEngine.Advertisements.Editor.Platform(extensionPath);
					s_EditorSupportedPlatform = true;
				}
				else
				{
					s_Platform = new UnsupportedPlatform();
				}
				Load();
			}
		}

		private static void Load()
		{
			if (s_Platform != null && isSupported && initializeOnStartup)
			{
				Initialize(gameId, testMode);
			}
		}

		private static bool IsEnabled()
		{
			return UnityAdsSettings.enabled;
		}

		public static void Initialize(string gameId)
		{
			Initialize(gameId, false);
		}

		public static void Initialize(string gameId, bool testMode)
		{
			if (!isInitialized)
			{
				isInitialized = true;
				s_Platform.OnStart += (object sender, StartEventArgs e) =>
				{
					isShowing = true;
				};
				s_Platform.OnFinish += (object sender, FinishEventArgs e) =>
				{
					isShowing = false;
				};
				MetaData metaData = new MetaData("framework");
				metaData.Set("name", "Unity");
				metaData.Set("version", Application.unityVersion);
				SetMetaData(metaData);
				MetaData metaData2 = new MetaData("adapter");
				metaData2.Set("name", "Engine");
				metaData2.Set("version", version);
				SetMetaData(metaData2);
				s_Platform.Initialize(gameId, testMode);
			}
		}

		public static bool IsReady()
		{
			return IsReady(null);
		}

		public static bool IsReady(string placementId)
		{
			return s_Platform.IsReady((!string.IsNullOrEmpty(placementId)) ? placementId : null);
		}

		public static PlacementState GetPlacementState()
		{
			return GetPlacementState(null);
		}

		public static PlacementState GetPlacementState(string placementId)
		{
			return s_Platform.GetPlacementState((!string.IsNullOrEmpty(placementId)) ? placementId : null);
		}

		public static void Show()
		{
			Show(null, null);
		}

		public static void Show(ShowOptions showOptions)
		{
			Show(null, showOptions);
		}

		public static void Show(string placementId)
		{
			Show(placementId, null);
		}

		public static void Show(string placementId, ShowOptions showOptions)
		{
			if (showOptions != null)
			{
				if (showOptions.resultCallback != null)
				{
					EventHandler<FinishEventArgs> finishHandler = null;
					finishHandler = (object sender, FinishEventArgs e) =>
					{
						showOptions.resultCallback(e.showResult);
						s_Platform.OnFinish -= finishHandler;
					};
					s_Platform.OnFinish += finishHandler;
				}
				if (!string.IsNullOrEmpty(showOptions.gamerSid))
				{
					MetaData metaData = new MetaData("player");
					metaData.Set("server_id", showOptions.gamerSid);
					SetMetaData(metaData);
				}
			}
			s_Platform.Show((!string.IsNullOrEmpty(placementId)) ? placementId : null);
		}

		public static void SetMetaData(MetaData metaData)
		{
			s_Platform.SetMetaData(metaData);
		}
	}
}
