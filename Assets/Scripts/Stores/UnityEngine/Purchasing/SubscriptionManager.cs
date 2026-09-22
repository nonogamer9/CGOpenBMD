using System;
using System.Collections.Generic;
using UnityEngine.Purchasing.Security;

namespace UnityEngine.Purchasing
{
	public class SubscriptionManager
	{
		private string receipt;

		private string productId;

		private string intro_json;

		public static void UpdateSubscription(Product newProduct, Product oldProduct, string developerPayload, Action<Product, string> appleStore, Action<string, string> googleStore)
		{
			if (oldProduct.receipt == null)
			{
				Debug.Log("The product has not been purchased, a subscription can only be upgrade/downgrade when has already been purchased");
				return;
			}
			Dictionary<string, object> dictionary = (Dictionary<string, object>)MiniJson.JsonDecode(oldProduct.receipt);
			if (dictionary == null || !dictionary.ContainsKey("Store") || !dictionary.ContainsKey("Payload"))
			{
				Debug.Log("The product receipt does not contain enough information");
				return;
			}
			string text = (string)dictionary["Store"];
			string text2 = (string)dictionary["Payload"];
			if (text2 == null)
			{
				return;
			}
			switch (text)
			{
			case "GooglePlay":
			{
				SubscriptionManager subscriptionManager = new SubscriptionManager(oldProduct, null);
				SubscriptionInfo subscriptionInfo = null;
				try
				{
					subscriptionInfo = subscriptionManager.getSubscriptionInfo();
				}
				catch (Exception message)
				{
					Debug.unityLogger.Log("Error: the product that will be updated does not have a valid receipt", message);
					break;
				}
				string storeSpecificId = newProduct.definition.storeSpecificId;
				googleStore(subscriptionInfo.getSubscriptionInfoJsonString(), storeSpecificId);
				break;
			}
			case "AppleAppStore":
			case "MacAppStore":
				appleStore(newProduct, developerPayload);
				break;
			default:
				Debug.Log("This store does not support update subscriptions");
				break;
			}
		}

		public static void UpdateSubscriptionInGooglePlayStore(Product oldProduct, Product newProduct, Action<string, string> googlePlayUpdateCallback)
		{
			SubscriptionManager subscriptionManager = new SubscriptionManager(oldProduct, null);
			SubscriptionInfo subscriptionInfo = null;
			try
			{
				subscriptionInfo = subscriptionManager.getSubscriptionInfo();
			}
			catch (Exception message)
			{
				Debug.unityLogger.Log("Error: the product that will be updated does not have a valid receipt", message);
				return;
			}
			string storeSpecificId = newProduct.definition.storeSpecificId;
			googlePlayUpdateCallback(subscriptionInfo.getSubscriptionInfoJsonString(), storeSpecificId);
		}

		public static void UpdateSubscriptionInAppleStore(Product newProduct, string developerPayload, Action<Product, string> appleStoreUpdateCallback)
		{
			appleStoreUpdateCallback(newProduct, developerPayload);
		}

		public SubscriptionManager(Product product, string intro_json)
		{
			receipt = product.receipt;
			productId = product.definition.storeSpecificId;
			this.intro_json = intro_json;
		}

		public SubscriptionManager(string receipt, string id, string intro_json)
		{
			this.receipt = receipt;
			productId = id;
			this.intro_json = intro_json;
		}

		public SubscriptionInfo getSubscriptionInfo()
		{
			if (receipt != null)
			{
				Dictionary<string, object> dictionary = (Dictionary<string, object>)MiniJson.JsonDecode(receipt);
				string text = (string)dictionary["Store"];
				string text2 = (string)dictionary["Payload"];
				if (text2 != null)
				{
					switch (text)
					{
					case "GooglePlay":
						return getGooglePlayStoreSubInfo(text2);
					case "AppleAppStore":
					case "MacAppStore":
						if (productId == null)
						{
							throw new NullProductIdException();
						}
						return getAppleAppStoreSubInfo(text2, productId);
					case "AmazonApps":
						return getAmazonAppStoreSubInfo(productId);
					default:
						throw new StoreSubscriptionInfoNotSupportedException("Store not supported: " + text);
					}
				}
			}
			throw new NullReceiptException();
		}

		private SubscriptionInfo getAmazonAppStoreSubInfo(string productId)
		{
			return new SubscriptionInfo(productId);
		}

		private SubscriptionInfo getAppleAppStoreSubInfo(string payload, string productId)
		{
			AppleReceipt appleReceipt = null;
			try
			{
				appleReceipt = new AppleReceiptParser().Parse(Convert.FromBase64String(payload));
			}
			catch (ArgumentException message)
			{
				Debug.unityLogger.Log("Unable to parse Apple receipt", message);
			}
			catch (IAPSecurityException message2)
			{
				Debug.unityLogger.Log("Unable to parse Apple receipt", message2);
			}
			catch (NullReferenceException message3)
			{
				Debug.unityLogger.Log("Unable to parse Apple receipt", message3);
			}
			List<AppleInAppPurchaseReceipt> list = new List<AppleInAppPurchaseReceipt>();
			if (appleReceipt != null && appleReceipt.inAppPurchaseReceipts != null && appleReceipt.inAppPurchaseReceipts.Length != 0)
			{
				AppleInAppPurchaseReceipt[] inAppPurchaseReceipts = appleReceipt.inAppPurchaseReceipts;
				foreach (AppleInAppPurchaseReceipt appleInAppPurchaseReceipt in inAppPurchaseReceipts)
				{
					if (appleInAppPurchaseReceipt.productID.Equals(productId))
					{
						list.Add(appleInAppPurchaseReceipt);
					}
				}
			}
			return (list.Count == 0) ? null : new SubscriptionInfo(findMostRecentReceipt(list), intro_json);
		}

		private AppleInAppPurchaseReceipt findMostRecentReceipt(List<AppleInAppPurchaseReceipt> receipts)
		{
			receipts.Sort((AppleInAppPurchaseReceipt b, AppleInAppPurchaseReceipt a) => a.purchaseDate.CompareTo(b.purchaseDate));
			return receipts[0];
		}

		private SubscriptionInfo getGooglePlayStoreSubInfo(string payload)
		{
			Dictionary<string, object> dictionary = (Dictionary<string, object>)MiniJson.JsonDecode(payload);
			string skuDetails = (string)dictionary["skuDetails"];
			bool purchaseHistorySupported = (bool)dictionary["isPurchaseHistorySupported"];
			Dictionary<string, object> dictionary2 = (Dictionary<string, object>)MiniJson.JsonDecode((string)dictionary["json"]);
			bool isAutoRenewing = (bool)dictionary2["autoRenewing"];
			DateTime purchaseDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds((long)dictionary2["purchaseTime"]);
			string json = (string)dictionary2["developerPayload"];
			Dictionary<string, object> dictionary3 = (Dictionary<string, object>)MiniJson.JsonDecode(json);
			bool isFreeTrial = (bool)dictionary3["is_free_trial"];
			bool hasIntroductoryPriceTrial = (bool)dictionary3["has_introductory_price_trial"];
			string updateMetadata = null;
			if ((bool)dictionary3["is_updated"])
			{
				updateMetadata = (string)dictionary3["update_subscription_metadata"];
			}
			return new SubscriptionInfo(skuDetails, isAutoRenewing, purchaseDate, isFreeTrial, hasIntroductoryPriceTrial, purchaseHistorySupported, updateMetadata);
		}
	}
}
