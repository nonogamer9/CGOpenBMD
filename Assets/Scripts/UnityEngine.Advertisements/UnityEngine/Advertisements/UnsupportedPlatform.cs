using System;
using System.Diagnostics;
using System.Threading;

namespace UnityEngine.Advertisements
{
	internal sealed class UnsupportedPlatform : IPlatform
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private EventHandler<FinishEventArgs> OnFinish__BackingField;

		public bool isInitialized
		{
			get
			{
				return false;
			}
		}

		public bool isSupported
		{
			get
			{
				return false;
			}
		}

		public string version
		{
			get
			{
				return null;
			}
		}

		public bool debugMode
		{
			get
			{
				return false;
			}
			set
			{
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
			}
			remove
			{
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

		public void Initialize(string gameId, bool testMode)
		{
		}

		public bool IsReady(string placementId)
		{
			return false;
		}

		public PlacementState GetPlacementState(string placementId)
		{
			return PlacementState.NotAvailable;
		}

		public void Show(string placementId)
		{
			EventHandler<FinishEventArgs> onFinish__BackingField = OnFinish__BackingField;
			if (onFinish__BackingField != null)
			{
				onFinish__BackingField(this, new FinishEventArgs(placementId, ShowResult.Failed));
			}
		}

		public void SetMetaData(MetaData metaData)
		{
		}
	}
}
