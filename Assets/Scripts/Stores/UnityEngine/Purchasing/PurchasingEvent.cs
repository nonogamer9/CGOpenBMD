using System.Collections.Generic;
using System.Linq;
using UnityEngine.Purchasing.MiniJSON;

namespace UnityEngine.Purchasing
{
	internal class PurchasingEvent
	{
		private Dictionary<string, object> EventDict;

		public PurchasingEvent(Dictionary<string, object> eventDict)
		{
			EventDict = eventDict;
		}

		public string FlatJSON(Dictionary<string, object> profileDict)
		{
			Dictionary<string, object> obj = profileDict.Concat(EventDict).ToDictionary((KeyValuePair<string, object> s) => s.Key, (KeyValuePair<string, object> s) => s.Value);
			string text = Json.Serialize(obj);
			return (text != null) ? text : string.Empty;
		}
	}
}
