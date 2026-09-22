using System.Collections.Generic;

namespace UnityEngine.Purchasing
{
	internal interface IUnityAnalytics
	{
		void Transaction(string productId, decimal price, string currency, string receipt, string signature);

		void CustomEvent(string name, Dictionary<string, object> data);
	}
}
