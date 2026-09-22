using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;

namespace UnityEngine.Purchasing
{
	public class CloudCatalogImpl
	{
		private IAsyncWebUtil m_AsyncUtil;

		private string m_CacheFileName;

		private ILogger m_Logger;

		private string m_CatalogURL;

		private string m_StoreName;

		private const int kMaxRetryDelayInSeconds = 300;

		private const string kCatalogURL = "https://catalog.iap.cloud.unity3d.com";

		public static CloudCatalogImpl CreateInstance(string storeName)
		{
			GameObject gameObject = new GameObject();
			Object.DontDestroyOnLoad(gameObject);
			gameObject.name = "Unity IAP";
			gameObject.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
			AsyncWebUtil util = gameObject.AddComponent<AsyncWebUtil>();
			string text = Path.Combine(Path.Combine(Application.persistentDataPath, "Unity"), Path.Combine(Application.cloudProjectId, "IAP"));
			string cacheFile = null;
			try
			{
				Directory.CreateDirectory(text);
				cacheFile = Path.Combine(text, "catalog.json");
			}
			catch (Exception message)
			{
				Debug.unityLogger.Log("Unable to cache IAP catalog", message);
			}
			string catalogURL = string.Format("{0}/{1}", "https://catalog.iap.cloud.unity3d.com", Application.cloudProjectId);
			return new CloudCatalogImpl(util, cacheFile, Debug.unityLogger, catalogURL, storeName);
		}

		internal CloudCatalogImpl(IAsyncWebUtil util, string cacheFile, ILogger logger, string catalogURL, string storeName)
		{
			m_AsyncUtil = util;
			m_CacheFileName = cacheFile;
			m_Logger = logger;
			m_CatalogURL = catalogURL;
			m_StoreName = storeName;
		}

		public void FetchProducts(Action<HashSet<ProductDefinition>> callback)
		{
			FetchProducts(callback, 0);
		}

		internal void FetchProducts(Action<HashSet<ProductDefinition>> callback, int delayInSeconds)
		{
			m_Logger.Log("Fetching IAP cloud catalog...");
			m_AsyncUtil.Get(m_CatalogURL, (string response) =>
			{
				m_Logger.Log("Fetched catalog");
				try
				{
					HashSet<ProductDefinition> obj = ParseProductsFromJSON(response, m_StoreName);
					TryPersistCatalog(response);
					callback(obj);
				}
				catch (SerializationException message)
				{
					m_Logger.LogError("Error parsing IAP catalog", message);
					m_Logger.Log(response);
					callback(TryLoadCachedCatalog());
				}
			}, (string error) =>
			{
				HashSet<ProductDefinition> hashSet = TryLoadCachedCatalog();
				if (hashSet != null && hashSet.Count > 0)
				{
					m_Logger.Log("Failed to fetch IAP catalog, using cache.");
					callback(hashSet);
				}
				else
				{
					delayInSeconds = Math.Max(5, delayInSeconds * 2);
					delayInSeconds = Math.Min(300, delayInSeconds);
					m_AsyncUtil.Schedule(() =>
					{
						FetchProducts(callback, delayInSeconds);
					}, delayInSeconds);
				}
			});
		}

		internal static HashSet<ProductDefinition> ParseProductsFromJSON(string json, string storeName)
		{
			HashSet<ProductDefinition> hashSet = new HashSet<ProductDefinition>();
			try
			{
				Dictionary<string, object> dictionary = (Dictionary<string, object>)MiniJson.JsonDecode(json);
				object value;
				dictionary.TryGetValue("products", out value);
				List<object> list = value as List<object>;
				string key = CamelCaseToSnakeCase(storeName);
				foreach (object item2 in list)
				{
					Dictionary<string, object> dictionary2 = (Dictionary<string, object>)item2;
					object value2;
					dictionary2.TryGetValue("id", out value2);
					object value3;
					dictionary2.TryGetValue("store_ids", out value3);
					object value4;
					dictionary2.TryGetValue("type", out value4);
					Dictionary<string, object> dictionary3 = value3 as Dictionary<string, object>;
					string storeSpecificId = (string)value2;
					if (dictionary3 != null && dictionary3.ContainsKey(key))
					{
						object value5 = null;
						dictionary3.TryGetValue(key, out value5);
						if (value5 != null)
						{
							storeSpecificId = (string)value5;
						}
					}
					ProductType type = (ProductType)Enum.Parse(typeof(ProductType), (string)value4);
					ProductDefinition item = new ProductDefinition((string)value2, storeSpecificId, type);
					hashSet.Add(item);
				}
				return hashSet;
			}
			catch (Exception innerException)
			{
				throw new SerializationException("Error parsing JSON", innerException);
			}
		}

		internal static string CamelCaseToSnakeCase(string s)
		{
			IEnumerable<string> source = s.Select((char a, int b) => (char.IsUpper(a) && b > 0) ? ("_" + char.ToLower(a)) : char.ToLower(a).ToString());
			return source.Aggregate((string a, string b) => a + b);
		}

		private void TryPersistCatalog(string response)
		{
			if (m_CacheFileName == null)
			{
				return;
			}
			try
			{
				File.WriteAllText(m_CacheFileName, response);
			}
			catch (Exception message)
			{
				m_Logger.LogError("Failed persisting IAP catalog", message);
			}
		}

		private HashSet<ProductDefinition> TryLoadCachedCatalog()
		{
			if (m_CacheFileName != null && File.Exists(m_CacheFileName))
			{
				try
				{
					string json = File.ReadAllText(m_CacheFileName);
					return ParseProductsFromJSON(json, m_StoreName);
				}
				catch (Exception message)
				{
					m_Logger.LogError("Error loading cached catalog", message);
				}
			}
			return new HashSet<ProductDefinition>();
		}
	}
}
