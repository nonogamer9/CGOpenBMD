using System;
using System.Collections.Generic;
using Uniject;
using UnityEngine.Purchasing.Extension;
using UnityEngine.Purchasing.MiniJSON;
using UnityEngine.Scripting;

namespace UnityEngine.Purchasing
{
	public class Promo
	{
		private static JSONStore s_PromoPurchaser = null;

		private static IStoreCallback s_Unity = null;

		private static RuntimePlatform s_RuntimePlatform;

		private static ILogger s_Logger;

		private static string s_Version;

		private static IUtil s_Util;

		private static IAsyncWebUtil s_WebUtil;

		private static bool s_IsReady = false;

		private static string s_ProductJSON;

		[Preserve]
		public static bool IsReady()
		{
			return s_IsReady;
		}

		[Preserve]
		public static string Version()
		{
			return s_Version;
		}

		internal static void InitPromo(RuntimePlatform platform, ILogger logger, IUtil util, IAsyncWebUtil webUtil)
		{
			InitPromo(platform, logger, "Unknown", util, webUtil);
		}

		internal static void InitPromo(RuntimePlatform platform, ILogger logger, string version, IUtil util, IAsyncWebUtil webUtil)
		{
			s_RuntimePlatform = platform;
			if (logger != null)
			{
				s_Logger = logger;
				s_Version = version;
				s_Util = util;
				s_WebUtil = webUtil;
				return;
			}
			throw new ArgumentException("UnityIAP: Promo initialized with null logger!");
		}

		private static HashSet<Product> UpdatePromoProductList()
		{
			if (s_Unity == null || s_Unity.products == null)
			{
				s_Logger.LogError("UnityIAP Promo", "Trying to update list without manager or products ready");
				return null;
			}
			HashSet<Product> hashSet = new HashSet<Product>();
			Product[] all = s_Unity.products.all;
			Product[] array = all;
			foreach (Product product in array)
			{
				if (product.availableToPurchase && (product.definition.type == ProductType.Consumable || string.IsNullOrEmpty(product.transactionID)))
				{
					hashSet.Add(product);
				}
			}
			return (hashSet.Count > 0) ? hashSet : null;
		}

		internal static void ProvideProductsToAds(JSONStore purchaser, IStoreCallback manager)
		{
			if (purchaser == null || manager == null)
			{
				s_Logger.LogError("UnityIAP Promo", "Attempt to set promo products without a valid purchaser!");
				return;
			}
			s_PromoPurchaser = purchaser;
			s_Unity = manager;
			ProvideProductsToAds(UpdatePromoProductList());
		}

		private static void ProvideProductsToAds(HashSet<Product> productsForAds)
		{
			List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
			if (productsForAds != null)
			{
				foreach (Product productsForAd in productsForAds)
				{
					Dictionary<string, object> dictionary = new Dictionary<string, object>();
					dictionary.Add("productId", productsForAd.definition.id);
					dictionary.Add("iapProductId", productsForAd.definition.id);
					dictionary.Add("productType", productsForAd.definition.type.ToString());
					dictionary.Add("localizedPriceString", productsForAd.metadata.localizedPriceString);
					dictionary.Add("localizedTitle", productsForAd.metadata.localizedTitle);
					dictionary.Add("imageUrl", null);
					dictionary.Add("isoCurrencyCode", productsForAd.metadata.isoCurrencyCode);
					dictionary.Add("localizedPrice", productsForAd.metadata.localizedPrice);
					list.Add(dictionary);
				}
			}
			else
			{
				s_Logger.Log("UnityIAP Promo", "Clearing promo product metadata");
			}
			s_ProductJSON = Json.Serialize(list);
			if (list.Count > 0)
			{
				s_IsReady = true;
				s_Logger.Log("UnityIAP: Promo interface is available for " + list.Count + " items");
				EventQueue eventQueue = EventQueue.Instance(s_Util, s_WebUtil);
				eventQueue.SendEvent(EventDestType.AdsIPC, "{\"type\":\"CatalogUpdated\"}");
			}
		}

		[Preserve]
		public static string QueryPromoProducts()
		{
			return s_ProductJSON;
		}

		[Preserve]
		public static bool InitiatePromoPurchase(string itemRequest)
		{
			return InitiatePurchasingCommand(itemRequest);
		}

		[Preserve]
		public static bool InitiatePurchasingCommand(string command)
		{
			if (string.IsNullOrEmpty(command))
			{
				if (s_Logger != null)
				{
					s_Logger.LogFormat(LogType.Warning, "Promo received null or empty command");
				}
				return false;
			}
			Dictionary<string, object> dictionary = null;
			try
			{
				dictionary = (Dictionary<string, object>)Json.Deserialize(command);
				if (dictionary == null)
				{
					return false;
				}
				object value;
				if (dictionary.TryGetValue("purchaseTrackingUrls", out value))
				{
					if (value != null)
					{
						List<object> list = value as List<object>;
						EventQueue eventQueue = EventQueue.Instance(s_Util, s_WebUtil);
						if (list.Count > 0)
						{
							eventQueue.SetIapUrl(list[0] as string);
						}
						if (list.Count > 1)
						{
							eventQueue.SetAdsUrl(list[1] as string);
						}
					}
					dictionary.Remove("purchaseTrackingUrls");
				}
				command = Json.Serialize(dictionary);
				object value2;
				if (!dictionary.TryGetValue("request", out value2))
				{
					return ExecPromoPurchase(command);
				}
				string text = ((string)value2).ToLower();
				switch (text)
				{
				case "purchase":
					return ExecPromoPurchase(command);
				case "setids":
				{
					ProfileData profileData = ProfileData.Instance(s_Util);
					object value3;
					if (dictionary.TryGetValue("gamerToken", out value3))
					{
						profileData.SetGamerToken(value3 as string);
					}
					if (dictionary.TryGetValue("trackingOptOut", out value3))
					{
						profileData.SetTrackingOptOut(value3 as bool?);
					}
					if (dictionary.TryGetValue("gameId", out value3))
					{
						profileData.SetGameId(value3 as string);
					}
					if (dictionary.TryGetValue("abGroup", out value3))
					{
						profileData.SetABGroup(value3 as int?);
					}
					return true;
				}
				case "close":
					if (s_Logger != null)
					{
						s_Logger.Log("UnityIAP Promo: AdUnit closed without purchase");
					}
					return true;
				default:
					if (s_Logger != null)
					{
						s_Logger.LogWarning("UnityIAP Promo", "Unknown request received: " + text);
					}
					return false;
				}
			}
			catch (Exception ex)
			{
				if (s_Logger != null)
				{
					s_Logger.LogError("UnityIAP Promo", string.Concat("Exception while processing incoming request: ", ex, "\n", command));
				}
				return false;
			}
		}

		internal static bool ExecPromoPurchase(string itemRequest)
		{
			if (!s_IsReady || s_PromoPurchaser == null)
			{
				if (s_Logger != null)
				{
					s_Logger.LogError("UnityIAP Promo", "Promo purchase attempted without proper configuration");
				}
				return false;
			}
			object value = null;
			Dictionary<string, object> dictionary = null;
			try
			{
				dictionary = (Dictionary<string, object>)Json.Deserialize(itemRequest);
				if (!dictionary.TryGetValue("productId", out value))
				{
					s_Logger.LogError("UnityIAP", "Promo purchase unable to determine Product ID");
					return false;
				}
			}
			catch
			{
				s_Logger.LogError("UnityIAP", "Promo purchase argument exception");
				return false;
			}
			if (string.IsNullOrEmpty(value as string))
			{
				s_Logger.LogError("UnityIAP", "Promo product is null or empty!");
				return false;
			}
			Product product = s_Unity.products.WithID((string)value);
			if (product == null)
			{
				s_Logger.LogError("UnityIAP", "Promo product lookup failed");
				return false;
			}
			dictionary.Add("storeSpecificId", product.definition.storeSpecificId);
			string developerPayload = Json.Serialize(dictionary);
			s_PromoPurchaser.Purchase(product.definition, developerPayload);
			return true;
		}
	}
}
