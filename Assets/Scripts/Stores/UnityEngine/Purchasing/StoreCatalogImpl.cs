using System;
using System.Collections.Generic;
using Uniject;

namespace UnityEngine.Purchasing
{
	internal class StoreCatalogImpl
	{
		private IAsyncWebUtil m_AsyncUtil;

		private ILogger m_Logger;

		private string m_CatalogURL;

		private string m_StoreName;

		private FileReference m_cachedStoreCatalogReference;

		private const string kFileName = "store.json";

		private static ProfileData profile;

		private const string kCatalogURL = "https://ecommerce.iap.unity3d.com";

		public static StoreCatalogImpl CreateInstance(string storeName, string baseUrl, IAsyncWebUtil webUtil, ILogger logger, IUtil util)
		{
			if (string.IsNullOrEmpty(storeName) || string.IsNullOrEmpty(baseUrl))
			{
				return null;
			}
			if (logger == null)
			{
				logger = Debug.unityLogger;
			}
			profile = ProfileData.Instance(util);
			Dictionary<string, object> profileIds = profile.GetProfileIds();
			string catalogURL = baseUrl + "/catalog" + profileIds.ToQueryString();
			FileReference fileReference = FileReference.CreateInstance("store.json", logger, util);
			return new StoreCatalogImpl(webUtil, logger, catalogURL, storeName, fileReference);
		}

		internal StoreCatalogImpl(IAsyncWebUtil util, ILogger logger, string catalogURL, string storeName, FileReference fileReference)
		{
			m_AsyncUtil = util;
			m_Logger = logger;
			m_CatalogURL = catalogURL;
			m_StoreName = storeName;
			m_cachedStoreCatalogReference = fileReference;
		}

		internal void FetchProducts(Action<List<ProductDefinition>> callback)
		{
			m_AsyncUtil.Get(m_CatalogURL, (string response) =>
			{
				List<ProductDefinition> list = ParseProductsFromJSON(response, m_StoreName, m_Logger);
				if (list == null)
				{
					m_Logger.LogError("Failed to fetch IAP catalog due to malformed response for " + m_StoreName, "response: " + response);
					handleCachedCatalog(callback);
				}
				else
				{
					m_Logger.Log("Fetched catalog successfully");
					if (m_cachedStoreCatalogReference != null)
					{
						m_cachedStoreCatalogReference.Save(response);
					}
					callback(list);
				}
			}, (string error) =>
			{
				handleCachedCatalog(callback);
			});
		}

		internal static List<ProductDefinition> ParseProductsFromJSON(string json, string storeName, ILogger logger)
		{
			if (string.IsNullOrEmpty(json))
			{
				return null;
			}
			HashSet<ProductDefinition> hashSet = new HashSet<ProductDefinition>();
			try
			{
				Dictionary<string, object> dictionary = (Dictionary<string, object>)MiniJson.JsonDecode(json);
				object value;
				dictionary.TryGetValue("catalog", out value);
				object value2;
				if (dictionary.TryGetValue("abGroup", out value2) && profile != null)
				{
					profile.SetStoreABGroup(Convert.ToInt32(value2));
				}
				Dictionary<string, object> dictionary2 = value as Dictionary<string, object>;
				object value3;
				if (dictionary2.TryGetValue("id", out value3))
				{
					if (profile != null)
					{
						string text = value3 as string;
						if (text == "")
						{
							text = "empty-catalog-id";
						}
						profile.SetCatalogId(text);
					}
				}
				else if (profile != null)
				{
					profile.SetCatalogId("no-catalog-id-present");
				}
				object value4;
				dictionary2.TryGetValue("products", out value4);
				List<object> productsList = (List<object>)value4;
				return productsList.DecodeJSON(storeName);
			}
			catch (Exception ex)
			{
				if (logger != null)
				{
					logger.LogWarning("UnityIAP", "Error parsing catalog, exception " + ex);
				}
				return null;
			}
		}

		private void handleCachedCatalog(Action<List<ProductDefinition>> callback)
		{
			List<ProductDefinition> list = null;
			if (m_cachedStoreCatalogReference != null)
			{
				list = ParseProductsFromJSON(m_cachedStoreCatalogReference.Load(), m_StoreName, m_Logger);
				if (list == null || list.Count == 0)
				{
					m_Logger.Log("Using configuration builder objects");
				}
				else
				{
					m_Logger.Log("Using cached IAP catalog");
				}
			}
			else
			{
				m_Logger.Log("Using registered configuration builder objects");
			}
			callback(list);
		}
	}
}
