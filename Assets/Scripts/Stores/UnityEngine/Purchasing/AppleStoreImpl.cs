using System;
using System.Collections.Generic;
using AOT;
using Uniject;
using UnityEngine.Purchasing.Extension;
using UnityEngine.Purchasing.Security;

namespace UnityEngine.Purchasing
{
	internal class AppleStoreImpl : JSONStore, IAppleExtensions, IStoreExtension, IAppleConfiguration, IStoreConfiguration
	{
		private Action<Product> m_DeferredCallback;

		private Action m_RefreshReceiptError;

		private Action<string> m_RefreshReceiptSuccess;

		private Action<bool> m_RestoreCallback;

		private Action<Product> m_PromotionalPurchaseCallback;

		private INativeAppleStore m_Native;

		private static IUtil util;

		private static AppleStoreImpl instance;

		private string products_json;

		public string appReceipt
		{
			get
			{
				return m_Native.appReceipt;
			}
		}

		public bool canMakePayments
		{
			get
			{
				return m_Native.canMakePayments;
			}
		}

		public bool simulateAskToBuy
		{
			get
			{
				return m_Native.simulateAskToBuy;
			}
			set
			{
				m_Native.simulateAskToBuy = value;
			}
		}

		public AppleStoreImpl(IUtil util)
		{
			AppleStoreImpl.util = util;
			instance = this;
		}

		public void SetNativeStore(INativeAppleStore apple)
		{
			SetNativeStore((INativeStore)apple);
			m_Native = apple;
			apple.SetUnityPurchasingCallback(MessageCallback);
		}

		public void SetApplePromotionalPurchaseInterceptorCallback(Action<Product> callback)
		{
			m_PromotionalPurchaseCallback = callback;
		}

		public void SetStorePromotionOrder(List<Product> products)
		{
			List<string> list = new List<string>();
			foreach (Product product in products)
			{
				if (product != null && !string.IsNullOrEmpty(product.definition.storeSpecificId))
				{
					list.Add(product.definition.storeSpecificId);
				}
			}
			Dictionary<string, object> json = new Dictionary<string, object> { { "products", list } };
			m_Native.SetStorePromotionOrder(MiniJson.JsonEncode(json));
		}

		public void SetStorePromotionVisibility(Product product, AppleStorePromotionVisibility visibility)
		{
			if (product == null)
			{
				throw new ArgumentNullException("product");
			}
			m_Native.SetStorePromotionVisibility(product.definition.storeSpecificId, visibility.ToString());
		}

		public string GetTransactionReceiptForProduct(Product product)
		{
			return m_Native.GetTransactionReceiptForProductId(product.definition.storeSpecificId);
		}

		public void SetApplicationUsername(string applicationUsername)
		{
			m_Native.SetApplicationUsername(applicationUsername);
		}

		public override void OnProductsRetrieved(string json)
		{
			List<ProductDescription> list = JSONSerializer.DeserializeProductDescriptions(json);
			List<ProductDescription> list2 = null;
			products_json = json;
			if (m_Native != null)
			{
				string text = m_Native.appReceipt;
				if (!string.IsNullOrEmpty(text))
				{
					AppleReceipt appleReceiptFromBase64String = getAppleReceiptFromBase64String(text);
					if (appleReceiptFromBase64String != null && appleReceiptFromBase64String.inAppPurchaseReceipts != null && appleReceiptFromBase64String.inAppPurchaseReceipts.Length != 0)
					{
						list2 = new List<ProductDescription>();
						foreach (ProductDescription productDescription in list)
						{
							AppleInAppPurchaseReceipt[] array = Array.FindAll(appleReceiptFromBase64String.inAppPurchaseReceipts, (AppleInAppPurchaseReceipt r) => r.productID == productDescription.storeSpecificId);
							if (array == null || array.Length == 0)
							{
								list2.Add(productDescription);
								continue;
							}
							Array.Sort(array, (AppleInAppPurchaseReceipt b, AppleInAppPurchaseReceipt a) => a.purchaseDate.CompareTo(b.purchaseDate));
							AppleInAppPurchaseReceipt appleInAppPurchaseReceipt = array[0];
							switch ((AppleStoreProductType)Enum.Parse(typeof(AppleStoreProductType), appleInAppPurchaseReceipt.productType.ToString()))
							{
							case AppleStoreProductType.AutoRenewingSubscription:
								if (new SubscriptionInfo(appleInAppPurchaseReceipt, null).isExpired() == Result.True)
								{
									list2.Add(productDescription);
								}
								else
								{
									list2.Add(new ProductDescription(productDescription.storeSpecificId, productDescription.metadata, text, appleInAppPurchaseReceipt.transactionID));
								}
								break;
							case AppleStoreProductType.Consumable:
								list2.Add(productDescription);
								break;
							default:
								list2.Add(new ProductDescription(productDescription.storeSpecificId, productDescription.metadata, text, appleInAppPurchaseReceipt.transactionID));
								break;
							}
						}
						AppleInAppPurchaseReceipt[] inAppPurchaseReceipts = appleReceiptFromBase64String.inAppPurchaseReceipts;
						foreach (AppleInAppPurchaseReceipt appleInAppPurchaseReceipt2 in inAppPurchaseReceipts)
						{
							Console.WriteLine("                    productID: {0}", appleInAppPurchaseReceipt2.productID);
							Console.WriteLine("                transactionID: {0}", appleInAppPurchaseReceipt2.transactionID);
							Console.WriteLine("originalTransactionIdentifier: {0}", appleInAppPurchaseReceipt2.originalTransactionIdentifier);
							Console.WriteLine("                 purchaseDate: {0}", appleInAppPurchaseReceipt2.purchaseDate);
							Console.WriteLine("         originalPurchaseDate: {0}", appleInAppPurchaseReceipt2.originalPurchaseDate);
							Console.WriteLine("   subscriptionExpirationDate: {0}", appleInAppPurchaseReceipt2.subscriptionExpirationDate);
						}
					}
				}
			}
			unity.OnProductsRetrieved(list2 ?? list);
			Promo.ProvideProductsToAds(this, unity);
			if (m_PromotionalPurchaseCallback != null)
			{
				m_Native.InterceptPromotionalPurchases();
			}
			m_Native.AddTransactionObserver();
		}

		public void RestoreTransactions(Action<bool> callback)
		{
			m_RestoreCallback = callback;
			m_Native.RestoreTransactions();
		}

		public void RefreshAppReceipt(Action<string> successCallback, Action errorCallback)
		{
			m_RefreshReceiptSuccess = successCallback;
			m_RefreshReceiptError = errorCallback;
			m_Native.RefreshAppReceipt();
		}

		public void RegisterPurchaseDeferredListener(Action<Product> callback)
		{
			m_DeferredCallback = callback;
		}

		public void ContinuePromotionalPurchases()
		{
			m_Native.ContinuePromotionalPurchases();
		}

		public Dictionary<string, string> GetIntroductoryPriceDictionary()
		{
			return JSONSerializer.DeserializeSubscriptionDescriptions(products_json);
		}

		public void OnPurchaseDeferred(string productId)
		{
			if (m_DeferredCallback != null)
			{
				Product product = unity.products.WithStoreSpecificID(productId);
				if (product != null)
				{
					m_DeferredCallback(product);
				}
			}
		}

		public void OnPromotionalPurchaseAttempted(string productId)
		{
			if (m_PromotionalPurchaseCallback != null)
			{
				Product product = unity.products.WithStoreSpecificID(productId);
				if (product != null)
				{
					m_PromotionalPurchaseCallback(product);
				}
			}
		}

		public void OnTransactionsRestoredSuccess()
		{
			if (m_RestoreCallback != null)
			{
				m_RestoreCallback(true);
			}
		}

		public void OnTransactionsRestoredFail(string error)
		{
			if (m_RestoreCallback != null)
			{
				m_RestoreCallback(false);
			}
		}

		public void OnAppReceiptRetrieved(string receipt)
		{
			if (receipt != null && m_RefreshReceiptSuccess != null)
			{
				m_RefreshReceiptSuccess(receipt);
			}
		}

		public void OnAppReceiptRefreshedFailed()
		{
			if (m_RefreshReceiptError != null)
			{
				m_RefreshReceiptError();
			}
		}

		[MonoPInvokeCallback(typeof(UnityPurchasingCallback))]
		private static void MessageCallback(string subject, string payload, string receipt, string transactionId)
		{
			util.RunOnMainThread(() =>
			{
				instance.ProcessMessage(subject, payload, receipt, transactionId);
			});
		}

		private void ProcessMessage(string subject, string payload, string receipt, string transactionId)
		{
			switch (subject)
			{
			case "OnSetupFailed":
				OnSetupFailed(payload);
				break;
			case "OnProductsRetrieved":
				OnProductsRetrieved(payload);
				break;
			case "OnPurchaseSucceeded":
				OnPurchaseSucceeded(payload, receipt, transactionId);
				break;
			case "OnPurchaseFailed":
				OnPurchaseFailed(payload);
				break;
			case "onProductPurchaseDeferred":
				OnPurchaseDeferred(payload);
				break;
			case "onPromotionalPurchaseAttempted":
				OnPromotionalPurchaseAttempted(payload);
				break;
			case "onTransactionsRestoredSuccess":
				OnTransactionsRestoredSuccess();
				break;
			case "onTransactionsRestoredFail":
				OnTransactionsRestoredFail(payload);
				break;
			case "onAppReceiptRefreshed":
				OnAppReceiptRetrieved(payload);
				break;
			case "onAppReceiptRefreshFailed":
				OnAppReceiptRefreshedFailed();
				break;
			}
		}

		public override void OnPurchaseSucceeded(string id, string receipt, string transactionId)
		{
			if (isValidPurchaseState(getAppleReceiptFromBase64String(receipt), id))
			{
				base.OnPurchaseSucceeded(id, receipt, transactionId);
			}
			else
			{
				FinishTransaction(null, transactionId);
			}
		}

		internal AppleReceipt getAppleReceiptFromBase64String(string receipt)
		{
			AppleReceipt result = null;
			if (!string.IsNullOrEmpty(receipt))
			{
				AppleReceiptParser appleReceiptParser = new AppleReceiptParser();
				try
				{
					result = appleReceiptParser.Parse(Convert.FromBase64String(receipt));
				}
				catch (Exception)
				{
				}
			}
			return result;
		}

		internal bool isValidPurchaseState(AppleReceipt appleReceipt, string id)
		{
			bool result = true;
			if (appleReceipt != null && appleReceipt.inAppPurchaseReceipts != null && appleReceipt.inAppPurchaseReceipts.Length != 0)
			{
				AppleInAppPurchaseReceipt[] array = Array.FindAll(appleReceipt.inAppPurchaseReceipts, (AppleInAppPurchaseReceipt r) => r.productID == id);
				if (array != null && array.Length != 0)
				{
					Array.Sort(array, (AppleInAppPurchaseReceipt b, AppleInAppPurchaseReceipt a) => a.purchaseDate.CompareTo(b.purchaseDate));
					AppleInAppPurchaseReceipt appleInAppPurchaseReceipt = array[0];
					AppleStoreProductType appleStoreProductType = (AppleStoreProductType)Enum.Parse(typeof(AppleStoreProductType), appleInAppPurchaseReceipt.productType.ToString());
					if (appleStoreProductType == AppleStoreProductType.AutoRenewingSubscription && new SubscriptionInfo(appleInAppPurchaseReceipt, null).isExpired() == Result.True)
					{
						result = false;
					}
				}
			}
			return result;
		}
	}
}
