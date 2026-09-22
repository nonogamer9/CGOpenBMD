using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using AOT;

namespace UnityEngine.Advertisements.iOS
{
	internal sealed class Platform : IPlatform
	{
		private delegate void unityAdsReady(string placementId);

		private delegate void unityAdsDidError(long rawError, string message);

		private delegate void unityAdsDidStart(string placementId);

		private delegate void unityAdsDidFinish(string placementId, long rawFinishState);

		private static Platform s_Instance;

		private static CallbackExecutor s_CallbackExecutor;

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
				return UnityAdsEngineIsInitialized();
			}
		}

		public bool isSupported
		{
			get
			{
				return UnityAdsEngineIsSupported();
			}
		}

		public string version
		{
			get
			{
				return UnityAdsEngineGetVersion();
			}
		}

		public bool debugMode
		{
			get
			{
				return UnityAdsEngineGetDebugMode();
			}
			set
			{
				UnityAdsEngineSetDebugMode(value);
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
		{
			s_Instance = this;
			GameObject gameObject = new GameObject("UnityAdsCallbackExecutorObject")
			{
				hideFlags = (HideFlags.HideAndDontSave | HideFlags.HideInInspector)
			};
			s_CallbackExecutor = gameObject.AddComponent<CallbackExecutor>();
			Object.DontDestroyOnLoad(gameObject);
			UnityAdsEngineSetReadyCallback(UnityAdsReady);
			UnityAdsEngineSetDidErrorCallback(UnityAdsDidError);
			UnityAdsEngineSetDidStartCallback(UnityAdsDidStart);
			UnityAdsEngineSetDidFinishCallback(UnityAdsDidFinish);
		}

		[DllImport("__Internal")]
		private static extern void UnityAdsEngineInitialize(string gameId, bool testMode);

		[DllImport("__Internal")]
		private static extern void UnityAdsEngineShow(string placementId);

		[DllImport("__Internal")]
		private static extern bool UnityAdsEngineGetDebugMode();

		[DllImport("__Internal")]
		private static extern void UnityAdsEngineSetDebugMode(bool debugMode);

		[DllImport("__Internal")]
		private static extern bool UnityAdsEngineIsSupported();

		[DllImport("__Internal")]
		private static extern bool UnityAdsEngineIsReady(string placementId);

		[DllImport("__Internal")]
		private static extern long UnityAdsEngineGetPlacementState(string placementId);

		[DllImport("__Internal")]
		private static extern string UnityAdsEngineGetVersion();

		[DllImport("__Internal")]
		private static extern bool UnityAdsEngineIsInitialized();

		[DllImport("__Internal")]
		private static extern void UnityAdsEngineSetMetaData(string category, string data);

		[DllImport("__Internal")]
		private static extern void UnityAdsEngineSetReadyCallback(unityAdsReady callback);

		[DllImport("__Internal")]
		private static extern void UnityAdsEngineSetDidErrorCallback(unityAdsDidError callback);

		[DllImport("__Internal")]
		private static extern void UnityAdsEngineSetDidStartCallback(unityAdsDidStart callback);

		[DllImport("__Internal")]
		private static extern void UnityAdsEngineSetDidFinishCallback(unityAdsDidFinish callback);

		[MonoPInvokeCallback(typeof(unityAdsReady))]
		private static void UnityAdsReady(string placementId)
		{
			EventHandler<ReadyEventArgs> handler = s_Instance.OnReady__BackingField;
			if (handler != null)
			{
				s_CallbackExecutor.Post((CallbackExecutor executor) =>
				{
					handler(s_Instance, new ReadyEventArgs(placementId));
				});
			}
		}

		[MonoPInvokeCallback(typeof(unityAdsDidError))]
		private static void UnityAdsDidError(long rawError, string message)
		{
			EventHandler<ErrorEventArgs> handler = s_Instance.OnError__BackingField;
			if (handler != null)
			{
				s_CallbackExecutor.Post((CallbackExecutor executor) =>
				{
					handler(s_Instance, new ErrorEventArgs(rawError, message));
				});
			}
		}

		[MonoPInvokeCallback(typeof(unityAdsDidStart))]
		private static void UnityAdsDidStart(string placementId)
		{
			EventHandler<StartEventArgs> handler = s_Instance.OnStart__BackingField;
			if (handler != null)
			{
				s_CallbackExecutor.Post((CallbackExecutor executor) =>
				{
					handler(s_Instance, new StartEventArgs(placementId));
				});
			}
		}

		[MonoPInvokeCallback(typeof(unityAdsDidFinish))]
		private static void UnityAdsDidFinish(string placementId, long rawShowResult)
		{
			EventHandler<FinishEventArgs> handler = s_Instance.OnFinish__BackingField;
			if (handler != null)
			{
				ShowResult showResult = (ShowResult)rawShowResult;
				s_CallbackExecutor.Post((CallbackExecutor executor) =>
				{
					handler(s_Instance, new FinishEventArgs(placementId, showResult));
				});
			}
		}

		public void Initialize(string gameId, bool testMode)
		{
			UnityAdsEngineInitialize(gameId, testMode);
		}

		public bool IsReady(string placementId)
		{
			return UnityAdsEngineIsReady(placementId);
		}

		public PlacementState GetPlacementState(string placementId)
		{
			return (PlacementState)UnityAdsEngineGetPlacementState(placementId);
		}

		public void Show(string placementId)
		{
			UnityAdsEngineShow(placementId);
		}

		public void SetMetaData(MetaData metaData)
		{
			UnityAdsEngineSetMetaData(metaData.category, metaData.ToJSON());
		}
	}
}
