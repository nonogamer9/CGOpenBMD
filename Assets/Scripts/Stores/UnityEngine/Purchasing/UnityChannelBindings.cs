using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine.ChannelPurchase;
using UnityEngine.Purchasing.Extension;
using UnityEngine.Purchasing.MiniJSON;

namespace UnityEngine.Purchasing
{
	internal class UnityChannelBindings : IPurchaseListener, INativeUnityChannelStore, INativeStore
	{
		protected Action<bool, string> m_PurchaseCallback;

		protected string m_PurchaseGuid;

		protected Dictionary<string, List<Action<bool, string, string>>> m_ValidateCallbacks = new Dictionary<string, List<Action<bool, string, string>>>();

		protected Dictionary<string, List<Action<bool, string, string>>> m_PurchaseConfirmCallbacks = new Dictionary<string, List<Action<bool, string, string>>>();

		public void OnPurchase(PurchaseInfo purchaseInfo)
		{
			Dictionary<string, string> obj = PurchaseInfoToDictionary(purchaseInfo);
			string arg = obj.toJson();
			m_PurchaseCallback(true, arg);
			m_PurchaseCallback = null;
		}

		public void OnPurchaseFailed(string message, PurchaseInfo purchaseInfo)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary["error"] = message;
			if (purchaseInfo != null)
			{
				dictionary["purchaseInfo"] = PurchaseInfoToDictionary(purchaseInfo);
			}
			string arg = dictionary.toJson();
			m_PurchaseCallback(false, arg);
			m_PurchaseCallback = null;
		}

		public void OnPurchaseRepeated(string productCode)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary["error"] = "repeat";
			dictionary["isRepeat"] = true;
			string arg = dictionary.toJson();
			m_PurchaseCallback(false, arg);
			m_PurchaseCallback = null;
		}

		public void OnPurchaseConfirm(string transactionId)
		{
			OnPurchaseConfirmCallbackDispatcher(transactionId, true, transactionId, "");
		}

		public void OnPurchaseConfirmFailed(string transactionId, string message)
		{
			OnPurchaseConfirmCallbackDispatcher(transactionId, false, transactionId, message);
		}

		protected void OnPurchaseConfirmCallbackDispatcher(string transactionId, bool result, string param1, string param2)
		{
			OnResponseCallbackDispatcher(transactionId, result, param1, param2, m_PurchaseConfirmCallbacks);
		}

		public void OnReceiptValidate(ReceiptInfo receiptInfo)
		{
			OnReceiptValidateCallbackDispatcher(receiptInfo.gameOrderId, true, receiptInfo.signData, receiptInfo.signature);
		}

		public void OnReceiptValidateFailed(string transactionId, string message)
		{
			OnReceiptValidateCallbackDispatcher(transactionId, false, message, null);
		}

		protected void OnReceiptValidateCallbackDispatcher(string transactionId, bool result, string param1, string param2)
		{
			OnResponseCallbackDispatcher(transactionId, result, param1, param2, m_ValidateCallbacks);
		}

		protected void OnResponseCallbackDispatcher(string transactionId, bool result, string param1, string param2, Dictionary<string, List<Action<bool, string, string>>> callbackDictionary)
		{
			if (!callbackDictionary.ContainsKey(transactionId))
			{
				return;
			}
			List<Action<bool, string, string>> list = callbackDictionary[transactionId];
			callbackDictionary.Remove(transactionId);
			foreach (Action<bool, string, string> item in list)
			{
				item(result, param1, param2);
			}
		}

		public void Purchase(string productId, Action<bool, string> callback, string developerPayload = null)
		{
			if (callback != null)
			{
				if (m_PurchaseCallback != null)
				{
					callback(false, "{ \"error\" : \"already purchasing\" }");
					return;
				}
				m_PurchaseCallback = callback;
				m_PurchaseGuid = Guid.NewGuid().ToString();
				PurchaseService.Purchase(productId, m_PurchaseGuid, this, developerPayload);
			}
		}

		public void RetrieveProducts(ReadOnlyCollection<ProductDefinition> products, Action<bool, string> callback)
		{
			HashSet<ProductDescription> hashSet = new HashSet<ProductDescription>();
			ProductCatalog productCatalog = ProductCatalog.LoadDefaultCatalog();
			foreach (ProductCatalogItem allValidProduct in productCatalog.allValidProducts)
			{
				foreach (ProductDefinition product in products)
				{
					if (string.Equals(allValidProduct.id, product.id))
					{
						int num = XiaomiPriceTiers.XiaomiPriceTierPrices[allValidProduct.xiaomiPriceTier];
						string priceString = string.Format("¥{0:0.00}", num);
						LocalizedProductDescription defaultDescription = allValidProduct.defaultDescription;
						LocalizedProductDescription description = allValidProduct.GetDescription(TranslationLocale.zh_CN);
						defaultDescription = description ?? defaultDescription;
						ProductMetadata metadata = new ProductMetadata(priceString, defaultDescription.Title, defaultDescription.Description, "CNY", num);
						ProductDescription item = new ProductDescription(product.storeSpecificId, metadata);
						hashSet.Add(item);
					}
				}
			}
			string arg = JSONSerializer.SerializeProductDescs(hashSet);
			callback(true, arg);
		}

		public void ValidateReceipt(string transactionId, Action<bool, string, string> callback)
		{
			RequestUniquely(transactionId, callback, m_ValidateCallbacks, () =>
			{
				PurchaseService.ValidateReceipt(transactionId, this);
			});
		}

		public void ConfirmPurchase(string transactionId, Action<bool, string, string> callback)
		{
			RequestUniquely(transactionId, callback, m_PurchaseConfirmCallbacks, () =>
			{
				PurchaseService.ConfirmPurchase(transactionId, this);
			});
		}

		protected void RequestUniquely(string transactionId, Action<bool, string, string> callback, Dictionary<string, List<Action<bool, string, string>>> callbackDictionary, Action requestAction)
		{
			if (callback != null)
			{
				if (string.IsNullOrEmpty(transactionId))
				{
					callback(false, "{ \"error\" : \"transactionId missing\" }", null);
				}
				else if (!callbackDictionary.ContainsKey(transactionId))
				{
					callbackDictionary.Add(transactionId, new List<Action<bool, string, string>> { callback });
					requestAction();
				}
				else
				{
					callbackDictionary[transactionId].Add(callback);
				}
			}
		}

		public void RetrieveProducts(string json)
		{
			throw new NotImplementedException();
		}

		public void Purchase(string productJSON, string developerPayload)
		{
			throw new NotImplementedException();
		}

		public void FinishTransaction(string productJSON, string transactionID)
		{
			throw new NotImplementedException();
		}

		internal static Dictionary<string, string> PurchaseInfoToDictionary(PurchaseInfo purchaseInfo)
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			dictionary["gameOrderId"] = purchaseInfo.gameOrderId;
			dictionary["productCode"] = purchaseInfo.productCode;
			dictionary["orderQueryToken"] = purchaseInfo.orderQueryToken;
			dictionary["developerPayload"] = purchaseInfo.developerPayload;
			return dictionary;
		}
	}
}
