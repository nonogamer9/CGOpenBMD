using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace UnityEngine.Advertisements.Android
{
	internal sealed class Platform : AndroidJavaProxy, IPlatform
	{
		private readonly AndroidJavaObject m_CurrentActivity;

		private readonly AndroidJavaClass m_UnityAds;

		private readonly CallbackExecutor m_CallbackExecutor;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private EventHandler<ReadyEventArgs> OnReady__BackingField;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private EventHandler<StartEventArgs> OnStart__BackingField;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private EventHandler<FinishEventArgs> OnFinish__BackingField;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private EventHandler<ErrorEventArgs> OnError__BackingField;

		public bool isInitialized
		{
			get
			{
				return m_UnityAds.CallStatic<bool>("isInitialized", new object[0]);
			}
		}

		public bool isSupported
		{
			get
			{
				return m_UnityAds.CallStatic<bool>("isSupported", new object[0]);
			}
		}

		public string version
		{
			get
			{
				return m_UnityAds.CallStatic<string>("getVersion", new object[0]);
			}
		}

		public bool debugMode
		{
			get
			{
				return m_UnityAds.CallStatic<bool>("getDebugMode", new object[0]);
			}
			set
			{
				m_UnityAds.CallStatic("setDebugMode", value);
			}
		}

		public event EventHandler<ReadyEventArgs> OnReady
		{
			add
			{
				EventHandler<ReadyEventArgs> eventHandler = OnReady__BackingField;
				EventHandler<ReadyEventArgs> eventHandler2;
				do
				{
					eventHandler2 = eventHandler;
					eventHandler = Interlocked.CompareExchange(ref OnReady__BackingField, (EventHandler<ReadyEventArgs>)Delegate.Combine(eventHandler2, value), eventHandler);
				}
				while ((object)eventHandler != eventHandler2);
			}
			remove
			{
				EventHandler<ReadyEventArgs> eventHandler = OnReady__BackingField;
				EventHandler<ReadyEventArgs> eventHandler2;
				do
				{
					eventHandler2 = eventHandler;
					eventHandler = Interlocked.CompareExchange(ref OnReady__BackingField, (EventHandler<ReadyEventArgs>)Delegate.Remove(eventHandler2, value), eventHandler);
				}
				while ((object)eventHandler != eventHandler2);
			}
		}

		public event EventHandler<StartEventArgs> OnStart
		{
			add
			{
				EventHandler<StartEventArgs> eventHandler = OnStart__BackingField;
				EventHandler<StartEventArgs> eventHandler2;
				do
				{
					eventHandler2 = eventHandler;
					eventHandler = Interlocked.CompareExchange(ref OnStart__BackingField, (EventHandler<StartEventArgs>)Delegate.Combine(eventHandler2, value), eventHandler);
				}
				while ((object)eventHandler != eventHandler2);
			}
			remove
			{
				EventHandler<StartEventArgs> eventHandler = OnStart__BackingField;
				EventHandler<StartEventArgs> eventHandler2;
				do
				{
					eventHandler2 = eventHandler;
					eventHandler = Interlocked.CompareExchange(ref OnStart__BackingField, (EventHandler<StartEventArgs>)Delegate.Remove(eventHandler2, value), eventHandler);
				}
				while ((object)eventHandler != eventHandler2);
			}
		}

		public event EventHandler<FinishEventArgs> OnFinish
		{
			add
			{
				EventHandler<FinishEventArgs> eventHandler = OnFinish__BackingField;
				EventHandler<FinishEventArgs> eventHandler2;
				do
				{
					eventHandler2 = eventHandler;
					eventHandler = Interlocked.CompareExchange(ref OnFinish__BackingField, (EventHandler<FinishEventArgs>)Delegate.Combine(eventHandler2, value), eventHandler);
				}
				while ((object)eventHandler != eventHandler2);
			}
			remove
			{
				EventHandler<FinishEventArgs> eventHandler = OnFinish__BackingField;
				EventHandler<FinishEventArgs> eventHandler2;
				do
				{
					eventHandler2 = eventHandler;
					eventHandler = Interlocked.CompareExchange(ref OnFinish__BackingField, (EventHandler<FinishEventArgs>)Delegate.Remove(eventHandler2, value), eventHandler);
				}
				while ((object)eventHandler != eventHandler2);
			}
		}

		public event EventHandler<ErrorEventArgs> OnError
		{
			add
			{
				EventHandler<ErrorEventArgs> eventHandler = OnError__BackingField;
				EventHandler<ErrorEventArgs> eventHandler2;
				do
				{
					eventHandler2 = eventHandler;
					eventHandler = Interlocked.CompareExchange(ref OnError__BackingField, (EventHandler<ErrorEventArgs>)Delegate.Combine(eventHandler2, value), eventHandler);
				}
				while ((object)eventHandler != eventHandler2);
			}
			remove
			{
				EventHandler<ErrorEventArgs> eventHandler = OnError__BackingField;
				EventHandler<ErrorEventArgs> eventHandler2;
				do
				{
					eventHandler2 = eventHandler;
					eventHandler = Interlocked.CompareExchange(ref OnError__BackingField, (EventHandler<ErrorEventArgs>)Delegate.Remove(eventHandler2, value), eventHandler);
				}
				while ((object)eventHandler != eventHandler2);
			}
		}

		public Platform()
			: base("com.unity3d.ads.IUnityAdsListener")
		{
			AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
			m_CurrentActivity = androidJavaClass.GetStatic<AndroidJavaObject>("currentActivity");
			m_UnityAds = new AndroidJavaClass("com.unity3d.ads.UnityAds");
			GameObject gameObject = new GameObject("UnityAdsCallbackExecutorObject")
			{
				hideFlags = (HideFlags.HideAndDontSave | HideFlags.HideInInspector)
			};
			m_CallbackExecutor = gameObject.AddComponent<CallbackExecutor>();
			Object.DontDestroyOnLoad(gameObject);
		}

		private void onUnityAdsReady(string placementId)
		{
			EventHandler<ReadyEventArgs> handler = OnReady__BackingField;
			if (handler != null)
			{
				m_CallbackExecutor.Post((CallbackExecutor executor) =>
				{
					handler(this, new ReadyEventArgs(placementId));
				});
			}
		}

		private void onUnityAdsStart(string placementId)
		{
			EventHandler<StartEventArgs> handler = OnStart__BackingField;
			if (handler != null)
			{
				m_CallbackExecutor.Post((CallbackExecutor executor) =>
				{
					handler(this, new StartEventArgs(placementId));
				});
			}
		}

		private void onUnityAdsFinish(string placementId, AndroidJavaObject rawShowResult)
		{
			EventHandler<FinishEventArgs> handler = OnFinish__BackingField;
			if (handler != null)
			{
				ShowResult showResult = (ShowResult)rawShowResult.Call<int>("ordinal", new object[0]);
				m_CallbackExecutor.Post((CallbackExecutor executor) =>
				{
					handler(this, new FinishEventArgs(placementId, showResult));
				});
			}
		}

		private void onUnityAdsError(AndroidJavaObject rawError, string message)
		{
			EventHandler<ErrorEventArgs> handler = OnError__BackingField;
			if (handler != null)
			{
				long error = rawError.Call<int>("ordinal", new object[0]);
				m_CallbackExecutor.Post((CallbackExecutor executor) =>
				{
					handler(this, new ErrorEventArgs(error, message));
				});
			}
		}

		public void Initialize(string gameId, bool testMode)
		{
			m_UnityAds.CallStatic("initialize", m_CurrentActivity, gameId, this, testMode);
		}

		public bool IsReady(string placementId)
		{
			if (placementId == null)
			{
				return m_UnityAds.CallStatic<bool>("isReady", new object[0]);
			}
			return m_UnityAds.CallStatic<bool>("isReady", new object[1] { placementId });
		}

		public PlacementState GetPlacementState(string placementId)
		{
			AndroidJavaObject androidJavaObject = ((placementId != null) ? m_UnityAds.CallStatic<AndroidJavaObject>("getPlacementState", new object[1] { placementId }) : m_UnityAds.CallStatic<AndroidJavaObject>("getPlacementState", new object[0]));
			return (PlacementState)androidJavaObject.Call<int>("ordinal", new object[0]);
		}

		public void Show(string placementId)
		{
			if (placementId == null)
			{
				m_UnityAds.CallStatic("show", m_CurrentActivity);
			}
			else
			{
				m_UnityAds.CallStatic("show", m_CurrentActivity, placementId);
			}
		}

		public void SetMetaData(MetaData metaData)
		{
			AndroidJavaObject androidJavaObject = new AndroidJavaObject("com.unity3d.ads.metadata.MetaData", m_CurrentActivity);
			androidJavaObject.Call("setCategory", metaData.category);
			foreach (KeyValuePair<string, object> value in metaData.Values)
			{
				androidJavaObject.Call<bool>("set", new object[2] { value.Key, value.Value });
			}
			androidJavaObject.Call("commit");
		}
	}
}
