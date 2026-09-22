using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;

namespace UnityEngine.Advertisements.Editor
{
	internal sealed class Platform : IPlatform
	{
		private static string s_BaseUrl = "http://adserver.unityads.unity3d.com/games";

		private bool m_DebugMode;

		private Configuration m_Configuration;

		private Placeholder m_Placeholder;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private EventHandler<StartEventArgs> OnStart__BackingField;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private EventHandler<FinishEventArgs> OnFinish__BackingField;

		public bool isInitialized
		{
			get
			{
				return m_Configuration != null;
			}
		}

		public bool isSupported
		{
			get
			{
				return Application.isEditor;
			}
		}

		public string version
		{
			get
			{
				return "2.1.1";
			}
		}

		public bool debugMode
		{
			get
			{
				return m_DebugMode;
			}
			set
			{
				m_DebugMode = value;
			}
		}

		public event EventHandler<ReadyEventArgs> OnReady
		{
			add
			{
			}
			remove
			{
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
			}
			remove
			{
			}
		}

		public Platform(string extensionPath)
		{
			GameObject gameObject = new GameObject("UnityAdsEditorPlaceHolderObject")
			{
				hideFlags = (HideFlags.HideAndDontSave | HideFlags.HideInInspector)
			};
			m_Placeholder = gameObject.AddComponent<Placeholder>();
			m_Placeholder.OnFinish += (object sender, FinishEventArgs e) =>
			{
				EventHandler<FinishEventArgs> onFinish__BackingField = OnFinish__BackingField;
				if (onFinish__BackingField != null)
				{
					onFinish__BackingField(sender, new FinishEventArgs(e.placementId, e.showResult));
				}
			};
			m_Placeholder.Load(extensionPath);
		}

		public void Initialize(string gameId, bool testMode)
		{
			Debug.Log("UnityAdsEditor: Initialize(" + gameId + ", " + testMode + ");");
			string requestUriString = string.Join("/", new string[3]
			{
				s_BaseUrl,
				gameId,
				string.Join("&", new string[3]
				{
					"configuration?platform=editor",
					"unityVersion=" + Uri.EscapeDataString(Application.unityVersion),
					"sdkVersionName=" + Uri.EscapeDataString(version)
				})
			});
			WebRequest request = WebRequest.Create(requestUriString);
			request.BeginGetResponse((IAsyncResult result) =>
			{
				WebResponse webResponse = request.EndGetResponse(result);
				StreamReader streamReader = new StreamReader(webResponse.GetResponseStream());
				string text = streamReader.ReadToEnd();
				try
				{
					m_Configuration = new Configuration(text);
					if (!m_Configuration.enabled)
					{
						Debug.LogWarning("gameId " + gameId + " is not enabled");
					}
				}
				catch (Exception exception)
				{
					Debug.LogError("Failed to parse configuration for gameId: " + gameId);
					Debug.Log(text);
					Debug.LogException(exception);
				}
				streamReader.Close();
				webResponse.Close();
			}, null);
		}

		public bool IsReady(string placementId)
		{
			if (placementId == null)
			{
				return isInitialized;
			}
			return isInitialized && m_Configuration.placements.ContainsKey(placementId);
		}

		public PlacementState GetPlacementState(string placementId)
		{
			if (IsReady(placementId))
			{
				return PlacementState.Ready;
			}
			return PlacementState.NotAvailable;
		}

		public void Show(string placementId)
		{
			if (isInitialized && placementId == null)
			{
				placementId = m_Configuration.defaultPlacement;
			}
			if (IsReady(placementId))
			{
				EventHandler<StartEventArgs> onStart__BackingField = OnStart__BackingField;
				if (onStart__BackingField != null)
				{
					onStart__BackingField(this, new StartEventArgs(placementId));
				}
				m_Placeholder.Show(placementId, m_Configuration.placements[placementId]);
			}
			else
			{
				EventHandler<FinishEventArgs> onFinish__BackingField = OnFinish__BackingField;
				if (onFinish__BackingField != null)
				{
					onFinish__BackingField(this, new FinishEventArgs(placementId, ShowResult.Failed));
				}
			}
		}

		public void SetMetaData(MetaData metaData)
		{
		}
	}
}
