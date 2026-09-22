using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine.Purchasing.Extension;
using UnityEngine.Purchasing.MiniJSON;

namespace UnityEngine.Purchasing
{
	internal class JSONStore : AbstractStore, IUnityCallback, IManagedStoreExtensions, IStoreExtension, IStoreInternal, IManagedStoreConfig, IStoreConfiguration, ITransactionHistoryExtensions
	{
		private StoreCatalogImpl m_managedStore;

		protected IStoreCallback unity;

		private INativeStore store;

		private List<ProductDefinition> m_storeCatalog;

		private bool isManagedStoreEnabled = true;

		private ProfileData m_profileData;

		private bool isRefreshing = false;

		private bool isFirstTimeRetrievingProducts = true;

		private Action refreshCallback;

		private StandardPurchasingModule m_Module;

		private HashSet<ProductDefinition> m_BuilderProducts = new HashSet<ProductDefinition>();

		private ILogger m_Logger;

		private EventQueue m_EventQueue;

		private Dictionary<string, object> promoPayload = null;

		private const string kIapEventsBase = "https://events.iap.unity3d.com/events";

		private const string kIecCatalogBase = "https://ecommerce.iap.unity3d.com";

		private bool catalogDisabled = false;

		private bool testStore = false;

		private string iapBaseUrl = null;

		private string eventBaseUrl = "https://events.iap.unity3d.com/events";

		private PurchaseFailureDescription lastPurchaseFailureDescription;

		private StoreSpecificPurchaseErrorCode _lastPurchaseErrorCode = StoreSpecificPurchaseErrorCode.Unknown;

		private string kStoreSpecificErrorCodeKey = "storeSpecificErrorCode";

		public Product[] storeCatalog
		{
			get
			{
				List<Product> list = new List<Product>();
				if (m_storeCatalog != null && unity.products.all != null)
				{
					foreach (ProductDefinition item in m_storeCatalog)
					{
						Product[] all = unity.products.all;
						foreach (Product product in all)
						{
							bool flag = false;
							if (product.definition.type != ProductType.Consumable && (product.hasReceipt || !string.IsNullOrEmpty(product.transactionID)))
							{
								flag = true;
							}
							if (product.availableToPurchase && !flag && product.definition.storeSpecificId == item.storeSpecificId)
							{
								list.Add(product);
							}
						}
					}
				}
				return list.ToArray();
			}
		}

		public bool disableStoreCatalog
		{
			get
			{
				return catalogDisabled;
			}
			set
			{
				if (value)
				{
					catalogDisabled = true;
					isManagedStoreEnabled = false;
					if (m_Logger != null)
					{
						m_Logger.LogWarning("UnityIAP", "Disabling store optimization");
					}
				}
				else
				{
					catalogDisabled = false;
					isManagedStoreEnabled = true;
					if (m_Logger != null)
					{
						m_Logger.Log("UnityIAP", "Enabling store optimization");
					}
				}
			}
		}

		public bool? trackingOptOut
		{
			get
			{
				ProfileData profileData = ProfileData.Instance(m_Module.util);
				return profileData.TrackingOptOut;
			}
			set
			{
				ProfileData profileData = ProfileData.Instance(m_Module.util);
				profileData.SetTrackingOptOut(value);
			}
		}

		public bool storeTestEnabled
		{
			get
			{
				return testStore;
			}
			set
			{
				if (!testStore)
				{
					testStore = value;
					ProfileData profileData = ProfileData.Instance(m_Module.util);
					profileData.SetStoreTestEnabled(value);
				}
			}
		}

		public string baseIapUrl
		{
			get
			{
				return iapBaseUrl;
			}
			set
			{
				if (iapBaseUrl == null && !string.IsNullOrEmpty(value))
				{
					storeTestEnabled = true;
					iapBaseUrl = value;
				}
			}
		}

		public string baseEventUrl
		{
			get
			{
				return eventBaseUrl;
			}
			set
			{
				if (!string.IsNullOrEmpty(value))
				{
					storeTestEnabled = true;
					eventBaseUrl = value;
				}
			}
		}

		public void SetNativeStore(INativeStore native)
		{
			store = native;
		}

		void IStoreInternal.SetModule(StandardPurchasingModule module)
		{
			if (module != null)
			{
				m_Module = module;
				if (module.logger != null)
				{
					m_Logger = module.logger;
				}
				else
				{
					m_Logger = Debug.unityLogger;
				}
			}
		}

		public override void Initialize(IStoreCallback callback)
		{
			unity = callback;
			m_EventQueue = EventQueue.Instance(m_Module.util, m_Module.webUtil);
			m_profileData = ProfileData.Instance(m_Module.util);
			if (m_Module != null)
			{
				string storeName = m_Module.storeInstance.storeName;
				m_profileData.SetStoreName(storeName);
				if (string.IsNullOrEmpty(iapBaseUrl))
				{
					iapBaseUrl = "https://ecommerce.iap.unity3d.com";
				}
				m_managedStore = StoreCatalogImpl.CreateInstance(storeName, iapBaseUrl, m_Module.webUtil, m_Module.logger, m_Module.util);
			}
			else if (m_Logger != null)
			{
				m_Logger.LogWarning("UnityIAP", "JSONStore init has no reference to SPM, can't start managed store");
			}
		}

		public override void RetrieveProducts(ReadOnlyCollection<ProductDefinition> products)
		{
			if (isManagedStoreEnabled && m_managedStore != null && (isRefreshing || isFirstTimeRetrievingProducts))
			{
				m_BuilderProducts = new HashSet<ProductDefinition>(products);
				m_managedStore.FetchProducts(ProcessManagedStoreResponse);
			}
			else
			{
				store.RetrieveProducts(JSONSerializer.SerializeProductDefs(products));
			}
			isFirstTimeRetrievingProducts = false;
		}

		internal void ProcessManagedStoreResponse(List<ProductDefinition> storeProducts)
		{
			m_storeCatalog = storeProducts;
			if (isRefreshing)
			{
				isRefreshing = false;
				if (storeCatalog.Length == 0 && refreshCallback != null)
				{
					refreshCallback();
					refreshCallback = null;
					return;
				}
			}
			HashSet<ProductDefinition> hashSet = new HashSet<ProductDefinition>(m_BuilderProducts);
			if (storeProducts != null)
			{
				hashSet.UnionWith(storeProducts);
			}
			store.RetrieveProducts(JSONSerializer.SerializeProductDefs(hashSet));
		}

		public override void Purchase(ProductDefinition product, string developerPayload)
		{
			if (!string.IsNullOrEmpty(developerPayload))
			{
				try
				{
					Dictionary<string, object> dictionary = (Dictionary<string, object>)Json.Deserialize(developerPayload);
					object value;
					if (dictionary != null && dictionary.ContainsKey("iapPromo") && dictionary.TryGetValue("productId", out value))
					{
						m_Logger.Log(string.Concat("UnityIAP: Promo Purchase(", value, ")"));
						promoPayload = dictionary;
						promoPayload.Add("type", "iap.purchase");
						promoPayload.Add("iap_service", true);
						Product product2 = unity.products.WithID(value as string);
						promoPayload.Add("amount", product2.metadata.localizedPrice);
						promoPayload.Add("currency", product2.metadata.isoCurrencyCode);
						developerPayload = "iapPromo";
					}
				}
				catch (Exception ex)
				{
					m_Logger.LogWarning("UnityIAP", "JSONStore exception handling developerPayload: " + ex);
				}
			}
			store.Purchase(JSONSerializer.SerializeProductDef(product), developerPayload);
		}

		public override void FinishTransaction(ProductDefinition product, string transactionId)
		{
			string productJSON = ((product == null) ? null : JSONSerializer.SerializeProductDef(product));
			store.FinishTransaction(productJSON, transactionId);
		}

		public void OnSetupFailed(string reason)
		{
			InitializationFailureReason reason2 = (InitializationFailureReason)Enum.Parse(typeof(InitializationFailureReason), reason, true);
			unity.OnSetupFailed(reason2);
		}

		public virtual void OnProductsRetrieved(string json)
		{
			unity.OnProductsRetrieved(JSONSerializer.DeserializeProductDescriptions(json));
			Promo.ProvideProductsToAds(this, unity);
		}

		public virtual void OnPurchaseSucceeded(string id, string receipt, string transactionID)
		{
			Product product = unity.products.WithStoreSpecificID(id);
			if (promoPayload != null && (id == (string)promoPayload["productId"] || id == (string)promoPayload["storeSpecificId"]))
			{
				promoPayload.Add("purchase", "OK");
				if (product != null)
				{
					promoPayload.Add("productType", product.definition.type.ToString());
				}
				Dictionary<string, string> dictionary = new Dictionary<string, string>();
				dictionary.Add("data", FormatUnifiedReceipt(receipt, transactionID));
				promoPayload.Add("receipt", dictionary);
				PurchasingEvent purchasingEvent = new PurchasingEvent(promoPayload);
				Dictionary<string, object> profileDict = m_profileData.GetProfileDict();
				string json = purchasingEvent.FlatJSON(profileDict);
				m_EventQueue.SendEvent(json);
				promoPayload.Clear();
				promoPayload = null;
			}
			else if (product != null)
			{
				Dictionary<string, object> dictionary2 = new Dictionary<string, object>();
				dictionary2.Add("type", "iap.purchase");
				dictionary2.Add("iap_service", true);
				dictionary2.Add("iapPromo", false);
				dictionary2.Add("purchase", "OK");
				dictionary2.Add("productId", product.definition.id);
				dictionary2.Add("storeSpecificId", product.definition.storeSpecificId);
				dictionary2.Add("amount", product.metadata.localizedPrice);
				dictionary2.Add("currency", product.metadata.isoCurrencyCode);
				dictionary2.Add("productType", product.definition.type.ToString());
				Dictionary<string, string> dictionary3 = new Dictionary<string, string>();
				dictionary3.Add("data", FormatUnifiedReceipt(receipt, transactionID));
				dictionary2.Add("receipt", dictionary3);
				PurchasingEvent purchasingEvent2 = new PurchasingEvent(dictionary2);
				Dictionary<string, object> profileDict2 = m_profileData.GetProfileDict();
				string json2 = purchasingEvent2.FlatJSON(profileDict2);
				m_EventQueue.SendEvent(EventDestType.IAP, json2, eventBaseUrl + "/v1/organic_purchase");
			}
			unity.OnPurchaseSucceeded(id, receipt, transactionID);
			Promo.ProvideProductsToAds(this, unity);
		}

		public void OnPurchaseFailed(string json)
		{
			OnPurchaseFailed(JSONSerializer.DeserializeFailureReason(json), json);
		}

		public void OnPurchaseFailed(PurchaseFailureDescription failure, string json = null)
		{
			if (promoPayload != null)
			{
				promoPayload["type"] = "iap.purchasefailed";
				promoPayload.Add("purchase", "FAILED");
				if (json != null)
				{
					promoPayload.Add("failureJSON", json);
				}
				PurchasingEvent purchasingEvent = new PurchasingEvent(promoPayload);
				Dictionary<string, object> profileDict = m_profileData.GetProfileDict();
				string json2 = purchasingEvent.FlatJSON(profileDict);
				m_EventQueue.SendEvent(EventDestType.IAP, json2);
				promoPayload.Clear();
				promoPayload = null;
			}
			else
			{
				Product product = unity.products.WithStoreSpecificID(failure.productId);
				Dictionary<string, object> dictionary = new Dictionary<string, object>();
				dictionary.Add("type", "iap.purchasefailed");
				dictionary.Add("iap_service", true);
				dictionary.Add("iapPromo", false);
				dictionary.Add("purchase", "FAILED");
				dictionary.Add("productId", product.definition.id);
				dictionary.Add("storeSpecificId", product.definition.storeSpecificId);
				dictionary.Add("amount", product.metadata.localizedPrice);
				dictionary.Add("currency", product.metadata.isoCurrencyCode);
				if (json != null)
				{
					dictionary.Add("failureJSON", json);
				}
				PurchasingEvent purchasingEvent2 = new PurchasingEvent(dictionary);
				Dictionary<string, object> profileDict2 = ProfileData.Instance(m_Module.util).GetProfileDict();
				string json3 = purchasingEvent2.FlatJSON(profileDict2);
				m_EventQueue.SendEvent(EventDestType.IAP, json3, eventBaseUrl + "/v1/organic_purchase");
			}
			lastPurchaseFailureDescription = failure;
			_lastPurchaseErrorCode = ParseStoreSpecificPurchaseErrorCode(json);
			unity.OnPurchaseFailed(failure);
		}

        public void RefreshCatalog(Action callback)
        {
            if (isManagedStoreEnabled && m_managedStore != null)
            {
                isRefreshing = true;
                refreshCallback = callback;
                m_managedStore.FetchProducts(ProcessManagedStoreResponse);
            }
            else
            {
                isRefreshing = false;
                refreshCallback = null;
                m_Logger?.LogWarning("UnityIAP", "Unable to refresh catalog because managed store is disabled.");
                callback?.Invoke();
            }
        }

        public PurchaseFailureDescription GetLastPurchaseFailureDescription()
		{
			return lastPurchaseFailureDescription;
		}

		public StoreSpecificPurchaseErrorCode GetLastStoreSpecificPurchaseErrorCode()
		{
			return _lastPurchaseErrorCode;
		}

		private string FormatUnifiedReceipt(string platformReceipt, string transactionId)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			if (m_Module != null)
			{
				dictionary["Store"] = m_Module.storeInstance.storeName;
			}
			else
			{
				dictionary["Store"] = "unknown";
			}
			dictionary["TransactionID"] = transactionId ?? string.Empty;
			dictionary["Payload"] = platformReceipt ?? string.Empty;
			return Json.Serialize(dictionary);
		}

		private StoreSpecificPurchaseErrorCode ParseStoreSpecificPurchaseErrorCode(string json)
		{
			if (json == null)
			{
				return StoreSpecificPurchaseErrorCode.Unknown;
			}
			Dictionary<string, object> dictionary = MiniJson.JsonDecode(json) as Dictionary<string, object>;
			if (dictionary != null && dictionary.ContainsKey(kStoreSpecificErrorCodeKey) && Enum.IsDefined(typeof(StoreSpecificPurchaseErrorCode), (string)dictionary[kStoreSpecificErrorCodeKey]))
			{
				string value = (string)dictionary[kStoreSpecificErrorCodeKey];
				return (StoreSpecificPurchaseErrorCode)Enum.Parse(typeof(StoreSpecificPurchaseErrorCode), value);
			}
			return StoreSpecificPurchaseErrorCode.Unknown;
		}
	}
}
