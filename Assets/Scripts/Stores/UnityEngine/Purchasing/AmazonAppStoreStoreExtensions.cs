using System.Collections.Generic;
using UnityEngine.Purchasing.Extension;

namespace UnityEngine.Purchasing
{
	public class AmazonAppStoreStoreExtensions : IAmazonExtensions, IStoreExtension, IAmazonConfiguration, IStoreConfiguration
	{
		private AndroidJavaObject android;

		public string amazonUserId
		{
			get
			{
				return android.Call<string>("getAmazonUserId", new object[0]);
			}
		}

		public AmazonAppStoreStoreExtensions(AndroidJavaObject a)
		{
			android = a;
		}

		public void WriteSandboxJSON(HashSet<ProductDefinition> products)
		{
			android.Call("writeSandboxJSON", JSONSerializer.SerializeProductDefs(products));
		}

		public void NotifyUnableToFulfillUnavailableProduct(string transactionID)
		{
			android.Call("notifyUnableToFulfillUnavailableProduct", transactionID);
		}
	}
}
