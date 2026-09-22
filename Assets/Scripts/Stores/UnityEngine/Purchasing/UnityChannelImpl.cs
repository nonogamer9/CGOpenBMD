using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine.Purchasing.Extension;
using UnityEngine.Purchasing.MiniJSON;

namespace UnityEngine.Purchasing
{
	internal class UnityChannelImpl : JSONStore, IUnityChannelExtensions, IStoreExtension, IUnityChannelConfiguration, IStoreConfiguration
	{
		private const string k_DuplicateTransaction = "DuplicateTransaction";

		private const string k_Unknown = "Unknown";

		protected INativeUnityChannelStore m_Bindings;

		protected string m_LastPurchaseError = "";

		public bool fetchReceiptPayloadOnPurchase { get; set; }

		public void SetNativeStore(INativeUnityChannelStore unityChannelBindings)
		{
			SetNativeStore((INativeStore)unityChannelBindings);
			m_Bindings = unityChannelBindings;
		}

		public override void RetrieveProducts(ReadOnlyCollection<ProductDefinition> products)
		{
			m_Bindings.RetrieveProducts(products, (bool result, string json) =>
			{
				OnProductsRetrieved(json);
			});
		}

		public override void Purchase(ProductDefinition product, string developerPayload)
		{
			m_Bindings.Purchase(product.storeSpecificId, (bool purchaseSuccess, string message) =>
			{
				m_LastPurchaseError = "";
				if (purchaseSuccess)
				{
					Dictionary<string, object> dic = message.HashtableFromJson();
					string transactionId = dic.GetString("gameOrderId");
					string text = dic.GetString("productCode");
					if (!string.IsNullOrEmpty(transactionId))
					{
						dic["transactionId"] = transactionId;
					}
					if (!string.IsNullOrEmpty(text))
					{
						dic["storeSpecificId"] = text;
					}
					if (!product.storeSpecificId.Equals(text))
					{
						Debug.LogWarningFormat("UnityChannelImpl received mismatching product code for purchase. Expected {0}, received {1}.", product.storeSpecificId, text);
					}
					if (fetchReceiptPayloadOnPurchase)
					{
						ValidateReceipt(transactionId, (bool success, string signData, string signature) =>
						{
							if (success)
							{
								dic["json"] = signData;
								dic["signature"] = signature;
								extractDeveloperPayload(dic, signData);
							}
							else
							{
								dic["json"] = signData ?? "ValidateReceipt error";
								dic["signature"] = signature ?? "ValidateReceipt error";
								dic["error"] = "ValidateReceipt";
							}
							string receipt2 = dic.toJson();
							unity.OnPurchaseSucceeded(product.storeSpecificId, receipt2, transactionId);
						});
					}
					else
					{
						string receipt = dic.toJson();
						unity.OnPurchaseSucceeded(product.storeSpecificId, receipt, transactionId);
					}
				}
				else
				{
					PurchaseFailureReason reason = (PurchaseFailureReason)Enum.Parse(typeof(PurchaseFailureReason), "Unknown");
					string value = reason.ToString();
					Dictionary<string, object> dictionary = message.HashtableFromJson();
					string value2 = dictionary.GetString("isRepeat");
					bool result;
					bool.TryParse(value2, out result);
					if (result)
					{
						if (Enum.IsDefined(typeof(PurchaseFailureReason), "DuplicateTransaction"))
						{
							reason = (PurchaseFailureReason)Enum.Parse(typeof(PurchaseFailureReason), "DuplicateTransaction");
						}
						value = "DuplicateTransaction";
					}
					Dictionary<string, object> dictionary2 = new Dictionary<string, object>();
					dictionary2["error"] = value;
					if (dictionary.ContainsKey("purchaseInfo"))
					{
						dictionary2["purchaseInfo"] = dictionary["purchaseInfo"];
					}
					string lastPurchaseError = dictionary2.toJson();
					m_LastPurchaseError = lastPurchaseError;
					PurchaseFailureDescription desc = new PurchaseFailureDescription(product.storeSpecificId, reason, message);
					unity.OnPurchaseFailed(desc);
				}
			}, developerPayload);
		}

		private void extractDeveloperPayload(Dictionary<string, object> dic, string signData)
		{
			Dictionary<string, object> dictionary = (Dictionary<string, object>)MiniJson.JsonDecode(signData);
			if (!dictionary.ContainsKey("extension"))
			{
				return;
			}
			string text = (string)dictionary["extension"];
			if (!string.IsNullOrEmpty(text))
			{
				Dictionary<string, object> dictionary2 = (Dictionary<string, object>)MiniJson.JsonDecode(text);
				if (dictionary2.ContainsKey("cpUserInfo"))
				{
					dic["developerPayload"] = (string)dictionary2["cpUserInfo"];
				}
			}
		}

		public override void FinishTransaction(ProductDefinition product, string transactionId)
		{
		}

		public void ConfirmPurchase(string transactionId, Action<bool, string, string> callback)
		{
			m_Bindings.ConfirmPurchase(transactionId, callback);
		}

		public void ValidateReceipt(string transactionIdentifier, Action<bool, string, string> callback)
		{
			m_Bindings.ValidateReceipt(transactionIdentifier, callback);
		}

		public string GetLastPurchaseError()
		{
			return m_LastPurchaseError;
		}
	}
}
