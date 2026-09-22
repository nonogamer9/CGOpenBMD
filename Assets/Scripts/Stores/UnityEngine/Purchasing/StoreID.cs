using System;

namespace UnityEngine.Purchasing
{
	[Serializable]
	public class StoreID
	{
		public string store;

		public string id;

		public StoreID(string store_, string id_)
		{
			store = store_;
			id = id_;
		}
	}
}
