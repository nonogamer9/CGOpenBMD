using System;

namespace UnityEngine.Advertisements
{
	internal class ReadyEventArgs : EventArgs
	{
		public string placementId { get; private set; }

		public ReadyEventArgs(string placementId)
		{
			this.placementId = placementId;
		}
	}
}
