using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Text;
using UnityEngine.Purchasing.Extension;

namespace UnityEngine.Purchasing
{
	[AddComponentMenu("")]
	internal class MoolahStoreImpl : MonoBehaviour, IStore, IMoolahExtension, IStoreExtension, IMoolahConfiguration, IStoreConfiguration
	{
		private static readonly string pollingPath = "https://api.cloudmoolah.com/CMPayment/api/polling.ashx";

		private static readonly string requestAuthCodePath = "https://api.cloudmoolah.com/CMPayment/api/authGlobal.ashx";

		private static readonly string requestRestoreTransactionUrl = "https://api.cloudmoolah.com/CMPayment/receipt/recover.ashx";

		private static readonly string requestValidateReceiptUrl = "https://api.cloudmoolah.com/CMPayment/receipt/validate.ashx";

		private static readonly string requestProductValidateUrl = "https://api.cloudmoolah.com/CMPayment/product/validate.ashx";

		private IStoreCallback m_callback;

		private bool isNeedPolling = false;

		private string m_CurrentStoreProductID;

		private bool isRequestAuthCodeing = false;

		private string m_appKey;

		private string m_hashKey;

		private string m_notificationURL;

		private CloudMoolahMode m_mode = CloudMoolahMode.Production;

		private string m_CustomerID = "";

		public string appKey
		{
			get
			{
				return m_appKey;
			}
			set
			{
				m_appKey = value;
			}
		}

		public string hashKey
		{
			get
			{
				return m_hashKey;
			}
			set
			{
				m_hashKey = value;
			}
		}

		public string notificationURL
		{
			get
			{
				return m_notificationURL;
			}
			set
			{
				m_notificationURL = value;
			}
		}

		public void Initialize(IStoreCallback m_callback)
		{
			Debug.Log("CloudMoolah Initialize");
			this.m_callback = m_callback;
			if (string.IsNullOrEmpty(m_appKey))
			{
				throw new Exception("IMoolahConfiguration.appkey is null!");
			}
			if (string.IsNullOrEmpty(m_hashKey))
			{
				throw new Exception("IMoolahConfiguration.hashKey is null!");
			}
		}

		public void RetrieveProducts(ReadOnlyCollection<ProductDefinition> productDefinitions)
		{
			if (GetMode() != CloudMoolahMode.Production)
			{
				List<ProductDescription> list = new List<ProductDescription>();
				foreach (ProductDefinition productDefinition in productDefinitions)
				{
					ProductMetadata metadata = new ProductMetadata("$0.01", "CloudMoolah title for " + productDefinition.storeSpecificId, "CloudMoolah description", "USD", 0.01m);
					list.Add(new ProductDescription(productDefinition.storeSpecificId, metadata));
				}
				RetrieveProductsSucceeded(list);
				return;
			}
			List<object> list2 = new List<object>();
			foreach (ProductDefinition productDefinition2 in productDefinitions)
			{
				Dictionary<string, object> dictionary = new Dictionary<string, object>();
				dictionary.Add("pid", productDefinition2.storeSpecificId);
				dictionary.Add("productType", GetProductTypeIndex(productDefinition2.type));
				list2.Add(dictionary);
			}
			string productInfo = MiniJson.JsonEncode(list2);
			StartCoroutine(VaildateProduct(m_appKey, productInfo, (bool state, string result) =>
			{
				Debug.Log("CloudMoolah VaildateProduct state: " + state);
				VaildateProductProcess(state, result);
			}));
		}

		private int GetProductTypeIndex(ProductType type)
		{
			switch (type)
			{
			case ProductType.Consumable:
				return 1;
			case ProductType.NonConsumable:
				return 2;
			case ProductType.Subscription:
				return 3;
			default:
				return 0;
			}
		}

		private void VaildateProductProcess(bool state, string result)
		{
			if (state)
			{
				Dictionary<string, object> dictionary = MiniJson.JsonDecode(result) as Dictionary<string, object>;
				string text = dictionary["code"].ToString();
				if (text == "1" || text == "2")
				{
					List<object> list = dictionary["values"] as List<object>;
					List<ProductDescription> list2 = new List<ProductDescription>();
					foreach (object item in list)
					{
						Dictionary<string, object> dictionary2 = item as Dictionary<string, object>;
						string currentString = GetCurrentString(dictionary2["pid"]);
						if (string.IsNullOrEmpty(currentString))
						{
							continue;
						}
						string currentString2 = GetCurrentString(dictionary2["priceString"]);
						if (!string.IsNullOrEmpty(currentString2))
						{
							string text2 = Convert.ToDouble(currentString2).ToString("f2");
							string currentString3 = GetCurrentString(dictionary2["title"]);
							string currentString4 = GetCurrentString(dictionary2["description"]);
							string currentString5 = GetCurrentString(dictionary2["currencyCode"]);
							string value = "0.00";
							if (dictionary2["localizedPrice"] == null)
							{
								value = text2;
							}
							decimal localizedPrice = Convert.ToDecimal(value);
							ProductMetadata metadata = new ProductMetadata(text2, currentString3, currentString4, currentString5, localizedPrice);
							list2.Add(new ProductDescription(currentString, metadata));
						}
					}
					RetrieveProductsSucceeded(list2);
					Debug.Log("CloudMoolah ProductList.length: " + list2.Count);
				}
				else
				{
					RetrieveProductsFailed(InitializationFailureReason.NoProductsAvailable);
				}
			}
			else
			{
				RetrieveProductsFailed(InitializationFailureReason.PurchasingUnavailable);
			}
		}

		private string GetCurrentString(object obj)
		{
			if (obj == null)
			{
				return null;
			}
			return obj.ToString();
		}

		private IEnumerator VaildateProduct(string appkey, string productInfo, Action<bool, string> result)
		{
			string sign = GetStringMD5(appkey + m_hashKey);
			WWWForm wf = new WWWForm();
			wf.AddField("appId", appkey);
			wf.AddField("productInfo", productInfo);
			wf.AddField("sign", sign);
			WWW w = new WWW(requestProductValidateUrl, wf);
			yield return w;
			if (!string.IsNullOrEmpty(w.error))
			{
				Debug.Log("CloudMoolah ValidateProduct w.error: " + w.error);
				result(false, w.error);
			}
			else
			{
				result(true, w.text);
				Debug.Log("CloudMoolah ValidateProduct w.text: " + w.text);
			}
		}

		private void RetrieveProductsSucceeded(List<ProductDescription> products)
		{
			m_callback.OnProductsRetrieved(products);
		}

		private void RetrieveProductsFailed(InitializationFailureReason reason)
		{
			m_callback.OnSetupFailed(reason);
		}

		public void ClosePayWebView(string result)
		{
			Debug.Log("CloudMoolah ClosePayWebView");
			if (isNeedPolling)
			{
				isNeedPolling = false;
				PurchaseFailed(m_CurrentStoreProductID, PurchaseFailureReason.UserCancelled, "UserCancelled");
			}
		}

		private void PurchaseRusult(string resultJson)
		{
			isNeedPolling = false;
			Dictionary<string, object> dictionary = MiniJson.JsonDecode(resultJson) as Dictionary<string, object>;
			Debug.Log("CloudMoolah PurchaseResult resultJson: " + resultJson);
			string text = dictionary["code"].ToString();
			if (text == "1")
			{
				Dictionary<string, object> dictionary2 = dictionary["values"] as Dictionary<string, object>;
				string text2 = dictionary2["state"].ToString();
				string transactionId = dictionary2["tradeSeq"].ToString();
				string storeSpecificId = dictionary2["productId"].ToString();
				string msg = dictionary["msg"].ToString();
				if (text2 == TradeSeqState.PAY_CONFIRM.ToString() || text2 == TradeSeqState.ORDER_SUCCEED.ToString())
				{
					string receipt = dictionary2["receipt"].ToString();
					PurchaseSucceed(storeSpecificId, receipt, transactionId);
				}
				else if (text2 == TradeSeqState.PAY_FAILED.ToString())
				{
					PurchaseFailed(storeSpecificId, (PurchaseFailureReason)Enum.Parse(typeof(PurchaseFailureReason), "Unknown"), msg);
				}
			}
		}

		public void Purchase(ProductDefinition product, string developerPayload)
		{
			Debug.Log("CloudMoolah Purchase: " + product.storeSpecificId);
			if (GetMode() == CloudMoolahMode.AlwaysSucceed)
			{
				PurchaseSucceed(product.storeSpecificId, "CloudMoolah TestMode receipt", Guid.NewGuid().ToString());
				return;
			}
			if (GetMode() == CloudMoolahMode.AlwaysFailed)
			{
				PurchaseFailed(product.storeSpecificId, PurchaseFailureReason.UserCancelled, "TestMode UserCancelled");
				return;
			}
			if (isNeedPolling)
			{
				throw new Exception("CloudMoolah Aborting this purchase. Pending purchase detected.");
			}
			Action<string, string, string> purchaseSucceed = (string productid, string receipt, string transactionId) =>
			{
				PurchaseSucceed(productid, receipt, transactionId);
			};
			Action<string, PurchaseFailureReason, string> purchaseFailed = (string storeSpecificId, PurchaseFailureReason failureReason, string msg) =>
			{
				if (failureReason == (PurchaseFailureReason)Enum.Parse(typeof(PurchaseFailureReason), "Unknown"))
				{
					failureReason = PurchaseFailureReason.UserCancelled;
				}
				PurchaseFailed(storeSpecificId, failureReason, msg);
			};
			Action<string, string, string> succeed = (string transactionId, string authGlobal, string paymentUrl) =>
			{
				if (string.IsNullOrEmpty(paymentUrl))
				{
					throw new Exception("authGlobal is null!");
				}
				if (string.IsNullOrEmpty(authGlobal))
				{
					throw new Exception("authGlobal is null! ");
				}
				if (string.IsNullOrEmpty(transactionId))
				{
					throw new Exception("transactionId is null! ");
				}
				isNeedPolling = true;
				string text = m_CustomerID;
				if (string.IsNullOrEmpty(text))
				{
					text = DeviceUniqueIdentifier();
				}
				if (Application.platform == RuntimePlatform.Android)
				{
					PayMethod.showPayWebView(paymentUrl, authGlobal, transactionId, m_hashKey, text);
				}
				else
				{
					Application.OpenURL(paymentUrl);
					StartCoroutine(StartPurchasePolling(authGlobal, transactionId, purchaseSucceed, purchaseFailed));
				}
			};
			Action<string, string> failed = (string productID, string msg) =>
			{
				PurchaseFailed(productID, PurchaseFailureReason.PaymentDeclined, "request MoolahStoreAuthCode failed !");
			};
			m_CurrentStoreProductID = product.storeSpecificId;
			RequestAuthCode(product.storeSpecificId, developerPayload, succeed, failed);
		}

		private string DeviceUniqueIdentifier()
		{
			string deviceID = PayMethod.getDeviceID();
			Debug.Log("CloudMoolah getDeviceID: " + deviceID);
			return deviceID;
		}

		private void RequestAuthCode(string productID, string payload, Action<string, string, string> succeed, Action<string, string> failed)
		{
			if (isRequestAuthCodeing)
			{
				failed(productID, "RequestAuthCode repeat");
				throw new Exception("RequestAuthCode repeat");
			}
			string text = m_CustomerID;
			if (string.IsNullOrEmpty(text))
			{
				text = DeviceUniqueIdentifier();
			}
			if (string.IsNullOrEmpty(text))
			{
				failed(productID, "customerId is null");
				throw new Exception("customerId or m_UniqueID is null!");
			}
			string text2 = "1";
			string text3 = Guid.NewGuid().ToString();
			string md5String = m_appKey + text + productID + text3 + text2 + m_hashKey;
			string stringMD = GetStringMD5(md5String);
			string text4 = "?APPId=" + m_appKey + "&customerId=" + text + "&productId=" + productID + "&tradeSeq=" + text3 + "&tradeType=" + text2 + "&payLoad=" + payload + "&sign=" + stringMD;
			WWWForm wWWForm = new WWWForm();
			wWWForm.AddField("APPId", m_appKey);
			wWWForm.AddField("customerId", text);
			wWWForm.AddField("productId", productID);
			wWWForm.AddField("tradeSeq", text3);
			wWWForm.AddField("tradeType", text2);
			wWWForm.AddField("payload", payload);
			if (m_notificationURL != null && m_notificationURL != "")
			{
				wWWForm.AddField("notificationURL", m_notificationURL);
				Debug.Log("CloudMoolah notificationURL: " + notificationURL);
			}
			wWWForm.AddField("sign", stringMD);
			isRequestAuthCodeing = true;
			Debug.Log("CloudMoolah: " + requestAuthCodePath + text4);
			StartCoroutine(RequestAuthCode(wWWForm, productID, text3, succeed, failed));
		}

		private IEnumerator RequestAuthCode(WWWForm wf, string productID, string transactionId, Action<string, string, string> succeed, Action<string, string> failed)
		{
			WWW w = new WWW(requestAuthCodePath, wf);
			yield return w;
			isRequestAuthCodeing = false;
			if (!string.IsNullOrEmpty(w.error))
			{
				Debug.Log("CloudMoolah RequestAuthCode error: " + w.error);
				failed(productID, w.error);
				yield break;
			}
			Debug.Log("CloudMoolah RequestAuthCode w.text: " + w.text);
			Dictionary<string, object> authCodeResult = MiniJson.JsonDecode(w.text) as Dictionary<string, object>;
			if (authCodeResult["code"].ToString() != "1")
			{
				failed(productID, GetCurrentString(authCodeResult["msg"]));
				yield break;
			}
			Dictionary<string, object> authCodeValues = authCodeResult["values"] as Dictionary<string, object>;
			string authCode = authCodeValues["authCode"].ToString();
			string paymentURL = authCodeValues["requestUrl"].ToString();
			succeed(transactionId, authCode, paymentURL);
		}

		private IEnumerator StartPurchasePolling(string authGlobal, string transactionId, Action<string, string, string> purchaseSucceed, Action<string, PurchaseFailureReason, string> purchaseFailed)
		{
			yield return new WaitForSeconds(6f);
			if (!isNeedPolling)
			{
				yield break;
			}
			string orderSuccess = "0";
			string signstr = authGlobal + orderSuccess + transactionId + m_hashKey;
			string sign = GetStringMD5(signstr);
			string param = "?authCode=" + authGlobal + "&orderSuccess=" + orderSuccess + "&tradeSeq=" + transactionId + "&sign=" + sign;
			string url = pollingPath + ((param == null) ? "" : param);
			WWW pollingstr = new WWW(url);
			yield return pollingstr;
			if (!string.IsNullOrEmpty(pollingstr.error))
			{
				Debug.Log("CloudMoolah StartPurchasePolling PC error: " + pollingstr.error);
			}
			else
			{
				Dictionary<string, object> jsonPollingObjects = MiniJson.JsonDecode(pollingstr.text) as Dictionary<string, object>;
				Debug.Log("CloudMoolah StartPurchasePolling PC resultJson: " + pollingstr.text);
				string code = jsonPollingObjects["code"].ToString();
				if (code == "1")
				{
					Dictionary<string, object> pollingValues = jsonPollingObjects["values"] as Dictionary<string, object>;
					string tradeSeq = pollingValues["tradeSeq"].ToString();
					string tradeState = pollingValues["state"].ToString();
					string productId = pollingValues["productId"].ToString();
					string Msg = jsonPollingObjects["msg"].ToString();
					if (tradeState == TradeSeqState.PAY_CONFIRM.ToString() || tradeState == TradeSeqState.ORDER_SUCCEED.ToString())
					{
						isNeedPolling = false;
						string receipt = pollingValues["receipt"].ToString();
						purchaseSucceed(productId, receipt, tradeSeq);
						yield break;
					}
					if (tradeState == TradeSeqState.PAY_FAILED.ToString())
					{
						isNeedPolling = false;
						purchaseFailed(productId, PurchaseFailureReason.UserCancelled, Msg);
						yield break;
					}
				}
			}
			StartCoroutine(StartPurchasePolling(authGlobal, transactionId, purchaseSucceed, purchaseFailed));
		}

		public void PurchaseSucceed(string storeSpecificId, string receipt, string transactionId)
		{
			Debug.Log("CloudMoolah PurchaseSucceed");
			m_callback.OnPurchaseSucceeded(storeSpecificId, receipt, transactionId);
		}

		public void PurchaseFailed(string storeSpecificId, PurchaseFailureReason reason, string msg)
		{
			PurchaseFailureDescription desc = new PurchaseFailureDescription(storeSpecificId, reason, msg);
			m_callback.OnPurchaseFailed(desc);
		}

		public void FinishTransaction(ProductDefinition product, string transactionId)
		{
			Debug.Log("CloudMoolah FinishTransaction");
		}

		private string GetStringMD5(string md5String)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(md5String);
			MD5CryptoServiceProvider mD5CryptoServiceProvider = new MD5CryptoServiceProvider();
			byte[] array = mD5CryptoServiceProvider.ComputeHash(bytes);
			string text = "";
			for (int i = 0; i < array.Length; i++)
			{
				text += Convert.ToString(array[i], 16).PadLeft(2, '0');
			}
			return text.PadLeft(32, '0');
		}

		public void SetMode(CloudMoolahMode mode)
		{
			m_mode = mode;
		}

		private CloudMoolahMode GetMode()
		{
			return m_mode;
		}

		public void RestoreTransactionID(Action<RestoreTransactionIDState> result)
		{
			if (GetMode() == CloudMoolahMode.AlwaysSucceed)
			{
				result(RestoreTransactionIDState.RestoreSucceed);
			}
			else if (GetMode() == CloudMoolahMode.AlwaysFailed)
			{
				result(RestoreTransactionIDState.RestoreFailed);
			}
			else
			{
				StartCoroutine(RestoreTransactionIDProcess(result));
			}
		}

		private IEnumerator RestoreTransactionIDProcess(Action<RestoreTransactionIDState> result)
		{
			string customID = m_CustomerID;
			if (string.IsNullOrEmpty(customID))
			{
				customID = DeviceUniqueIdentifier();
			}
			WWWForm wf = new WWWForm();
			wf.AddField("appId", m_appKey);
			wf.AddField("customerId", customID);
			DateTime now = DateTime.Now;
			string endDate = now.ToString("yyyy/MM/dd");
			string startDate = now.AddDays(-7.0).ToString("yyyy/MM/dd");
			wf.AddField("startDate", startDate);
			wf.AddField("endDate", endDate);
			string sign = GetStringMD5(m_appKey + customID + m_hashKey);
			wf.AddField("Sign", sign);
			WWW w = new WWW(requestRestoreTransactionUrl, wf);
			yield return w;
			if (!string.IsNullOrEmpty(w.error))
			{
				Debug.LogError("CloudMoolah RestoreTransactionID error: " + w.error);
				result(RestoreTransactionIDState.NotKnown);
				yield break;
			}
			Debug.LogError("CloudMoolah RestoreTransactionIDProcess text: " + w.text);
			Dictionary<string, object> restoreObjects = MiniJson.JsonDecode(w.text) as Dictionary<string, object>;
			string code = restoreObjects["code"].ToString();
			if (code == "1")
			{
				List<object> restoreValues = restoreObjects["values"] as List<object>;
				foreach (Dictionary<string, object> restoreObjectElem in restoreValues)
				{
					string productId = restoreObjectElem["productId"].ToString();
					string tradeSeq = restoreObjectElem["tradeSeq"].ToString();
					string receipt = restoreObjectElem["receipt"].ToString();
					PurchaseSucceed(productId, receipt, tradeSeq);
				}
				result(RestoreTransactionIDState.RestoreSucceed);
			}
			else if (code == "CMB0000147")
			{
				result(RestoreTransactionIDState.NoTransactionRestore);
			}
			else
			{
				result(RestoreTransactionIDState.RestoreFailed);
			}
		}

		public void ValidateReceipt(string transactionId, string receipt, Action<string, ValidateReceiptState, string> result)
		{
			if (string.IsNullOrEmpty(transactionId) || string.IsNullOrEmpty(receipt))
			{
				result(transactionId, ValidateReceiptState.ValidateFailed, "transactionId or receipt is null");
			}
			else if (GetMode() == CloudMoolahMode.AlwaysSucceed)
			{
				result(transactionId, ValidateReceiptState.ValidateSucceed, "TestMode ValidateSucceed");
			}
			else if (GetMode() == CloudMoolahMode.AlwaysFailed)
			{
				result(transactionId, ValidateReceiptState.ValidateFailed, "TestMode ValidateFailed");
			}
			else
			{
				StartCoroutine(ValidateReceiptProcess(transactionId, receipt, result));
			}
		}

		private IEnumerator ValidateReceiptProcess(string transactionId, string receipt, Action<string, ValidateReceiptState, string> result)
		{
			Dictionary<string, object> tempJson = MiniJson.JsonDecode(receipt) as Dictionary<string, object>;
			if (tempJson != null && tempJson["Payload"] != null)
			{
				receipt = tempJson["Payload"].ToString();
			}
			WWWForm wf = new WWWForm();
			wf.AddField("appId", m_appKey);
			wf.AddField("receipt", receipt);
			string sign = GetStringMD5(m_appKey + receipt + m_hashKey);
			wf.AddField("sign", sign);
			WWW w = new WWW(requestValidateReceiptUrl, wf);
			yield return w;
			if (!string.IsNullOrEmpty(w.error))
			{
				Debug.LogError("CloudMoolah ValidateReceipt error: " + w.error);
				result(transactionId, ValidateReceiptState.NotKnown, "ValidateReceiptState NotKnown");
				yield break;
			}
			Debug.LogError("CloudMoolah validateReceiptProcess text: " + w.text);
			Dictionary<string, object> jsonObjects = MiniJson.JsonDecode(w.text) as Dictionary<string, object>;
			string code = jsonObjects["code"].ToString();
			string msg = jsonObjects["msg"].ToString();
			if (code == "1")
			{
				result(transactionId, ValidateReceiptState.ValidateSucceed, "ValidateReceiptState ValidateSucceeded");
			}
			else
			{
				result(transactionId, ValidateReceiptState.ValidateFailed, msg);
			}
		}
	}
}
