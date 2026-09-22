using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine.Purchasing.Extension;

namespace UnityEngine.Purchasing
{
	internal class PurchasingManager : IStoreCallback, IStoreController
	{
		[Serializable]
		private class UnifiedReceipt
		{
			public string Store;

			public string TransactionID;

			public string Payload;
		}

		private IStore m_Store;

		private IInternalStoreListener m_Listener;

		private ILogger m_Logger;

		private TransactionLog m_TransactionLog;

		private string m_StoreName;

		private Action m_AdditionalProductsCallback;

		private Action<InitializationFailureReason> m_AdditionalProductsFailCallback;

		private bool initialized;

		public bool useTransactionLog { get; set; }

		public ProductCollection products { get; private set; }

		internal PurchasingManager(TransactionLog tDb, ILogger logger, IStore store, string storeName)
		{
			m_TransactionLog = tDb;
			m_Store = store;
			m_Logger = logger;
			m_StoreName = storeName;
			useTransactionLog = true;
		}

		public void InitiatePurchase(Product product)
		{
			InitiatePurchase(product, string.Empty);
		}

		public void InitiatePurchase(string productId)
		{
			InitiatePurchase(productId, string.Empty);
		}

		public void InitiatePurchase(Product product, string developerPayload)
		{
			if (product == null)
			{
				m_Logger.Log("Trying to purchase null Product");
				return;
			}
			if (!product.availableToPurchase)
			{
				m_Listener.OnPurchaseFailed(product, PurchaseFailureReason.ProductUnavailable);
				return;
			}
			m_Store.Purchase(product.definition, developerPayload);
			m_Logger.Log("purchase({0})", product.definition.id);
		}

		public void InitiatePurchase(string purchasableId, string developerPayload)
		{
			Product product = products.WithID(purchasableId);
			if (product == null)
			{
				m_Logger.LogWarning("Unable to purchase unknown product with id: {0}", purchasableId);
			}
			InitiatePurchase(product, developerPayload);
		}

		public void ConfirmPendingPurchase(Product product)
		{
			if (product == null)
			{
				m_Logger.Log("Unable to confirm purchase with null Product");
				return;
			}
			if (string.IsNullOrEmpty(product.transactionID))
			{
				m_Logger.Log("Unable to confirm purchase; Product has missing or empty transactionID");
				return;
			}
			if (useTransactionLog)
			{
				m_TransactionLog.Record(product.transactionID);
			}
			m_Store.FinishTransaction(product.definition, product.transactionID);
		}

		public void OnPurchaseSucceeded(string id, string receipt, string transactionId)
		{
			Product product = products.WithStoreSpecificID(id);
			if (product == null)
			{
				ProductDefinition definition = new ProductDefinition(id, ProductType.NonConsumable);
				product = new Product(definition, new ProductMetadata());
			}
			string receipt2 = FormatUnifiedReceipt(receipt, transactionId);
			product.receipt = receipt2;
			product.transactionID = transactionId;
			ProcessPurchaseIfNew(product);
		}

		public void OnSetupFailed(InitializationFailureReason reason)
		{
			if (initialized)
			{
				if (m_AdditionalProductsFailCallback != null)
				{
					m_AdditionalProductsFailCallback(reason);
				}
			}
			else
			{
				m_Listener.OnInitializeFailed(reason);
			}
		}

		public void OnPurchaseFailed(PurchaseFailureDescription description)
		{
			Product product = products.WithStoreSpecificID(description.productId);
			if (product == null)
			{
				m_Logger.LogError("Failed to purchase unknown product {0}", description.productId);
				return;
			}
			m_Logger.Log("onPurchaseFailedEvent({0})", product.definition.id);
			m_Listener.OnPurchaseFailed(product, description.reason);
		}

		public void OnProductsRetrieved(List<ProductDescription> products)
		{
			HashSet<Product> hashSet = new HashSet<Product>();
			foreach (ProductDescription product2 in products)
			{
				Product product = this.products.WithStoreSpecificID(product2.storeSpecificId);
				if (product == null)
				{
					ProductDefinition definition = new ProductDefinition(product2.storeSpecificId, product2.storeSpecificId, product2.type);
					product = new Product(definition, product2.metadata);
					hashSet.Add(product);
				}
				product.availableToPurchase = true;
				product.metadata = product2.metadata;
				product.transactionID = product2.transactionId;
				if (!string.IsNullOrEmpty(product2.receipt))
				{
					product.receipt = FormatUnifiedReceipt(product2.receipt, product2.transactionId);
				}
			}
			if (hashSet.Count > 0)
			{
				this.products.AddProducts(hashSet);
			}
			CheckForInitialization();
			foreach (Product item in this.products.set)
			{
				if (!string.IsNullOrEmpty(item.receipt) && !string.IsNullOrEmpty(item.transactionID))
				{
					ProcessPurchaseIfNew(item);
				}
			}
		}

		public void FetchAdditionalProducts(HashSet<ProductDefinition> products, Action successCallback, Action<InitializationFailureReason> failCallback)
		{
			m_AdditionalProductsCallback = successCallback;
			m_AdditionalProductsFailCallback = failCallback;
			this.products.AddProducts(products.Select((ProductDefinition x) => new Product(x, new ProductMetadata())));
			m_Store.RetrieveProducts(new ReadOnlyCollection<ProductDefinition>(products.ToList()));
		}

		private void ProcessPurchaseIfNew(Product product)
		{
			if (useTransactionLog && m_TransactionLog.HasRecordOf(product.transactionID))
			{
				m_Logger.Log("Already recorded transaction " + product.transactionID);
				m_Store.FinishTransaction(product.definition, product.transactionID);
				return;
			}
			PurchaseEventArgs e = new PurchaseEventArgs(product);
			if (m_Listener.ProcessPurchase(e) == PurchaseProcessingResult.Complete)
			{
				ConfirmPendingPurchase(product);
			}
		}

		private void CheckForInitialization()
		{
			if (!initialized)
			{
				bool flag = false;
				foreach (Product item in products.set)
				{
					if (!item.availableToPurchase)
					{
						m_Logger.LogFormat(LogType.Warning, "Unavailable product {0} -{1}", item.definition.id, item.definition.storeSpecificId);
					}
					else
					{
						flag = true;
					}
				}
				if (flag)
				{
					m_Listener.OnInitialized(this);
				}
				else
				{
					OnSetupFailed(InitializationFailureReason.NoProductsAvailable);
				}
				initialized = true;
			}
			else if (m_AdditionalProductsCallback != null)
			{
				m_AdditionalProductsCallback();
			}
		}

		public void Initialize(IInternalStoreListener listener, HashSet<ProductDefinition> products)
		{
			m_Listener = listener;
			m_Store.Initialize(this);
			Product[] array = products.Select((ProductDefinition x) => new Product(x, new ProductMetadata())).ToArray();
			this.products = new ProductCollection(array);
			ReadOnlyCollection<ProductDefinition> readOnlyCollection = new ReadOnlyCollection<ProductDefinition>(products.ToList());
			m_Store.RetrieveProducts(readOnlyCollection);
		}

		private string FormatUnifiedReceipt(string platformReceipt, string transactionId)
		{
			UnifiedReceipt unifiedReceipt = new UnifiedReceipt();
			unifiedReceipt.Store = m_StoreName;
			unifiedReceipt.TransactionID = transactionId;
			unifiedReceipt.Payload = platformReceipt;
			UnifiedReceipt obj = unifiedReceipt;
			return JsonUtility.ToJson(obj);
		}
	}
}
