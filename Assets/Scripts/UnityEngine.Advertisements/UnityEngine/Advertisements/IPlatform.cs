using System;

namespace UnityEngine.Advertisements
{
	internal interface IPlatform
	{
		bool isInitialized { get; }

		bool isSupported { get; }

		string version { get; }

		bool debugMode { get; set; }

		event EventHandler<ReadyEventArgs> OnReady;

		event EventHandler<StartEventArgs> OnStart;

		event EventHandler<FinishEventArgs> OnFinish;

		event EventHandler<ErrorEventArgs> OnError;

		void Initialize(string gameId, bool testMode);

		bool IsReady(string placementId);

		PlacementState GetPlacementState(string placementId);

		void Show(string placementId);

		void SetMetaData(MetaData metaData);
	}
}
