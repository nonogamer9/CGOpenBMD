using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Uniject;
using UnityEngine.Purchasing.Default;
using UnityEngine.Purchasing.Extension;

namespace UnityEngine.Purchasing
{
	internal class WinRTStore : AbstractStore, IWindowsIAPCallback, IMicrosoftExtensions, IStoreExtension
	{
		private IWindowsIAP win8;

		private IStoreCallback callback;

		private IUtil util;

		private ILogger logger;

		private bool m_CanReceivePurchases = false;

		private static int count;

		public WinRTStore(IWindowsIAP win8, IUtil util, ILogger logger)
		{
			this.win8 = win8;
			this.util = util;
			this.logger = logger;
		}

		public void SetWindowsIAP(IWindowsIAP iap)
		{
			win8 = iap;
		}

		public override void Initialize(IStoreCallback biller)
		{
			callback = biller;
		}

		public override void RetrieveProducts(ReadOnlyCollection<ProductDefinition> productDefs)
		{
			IEnumerable<WinProductDescription> source = from def in productDefs
				where def.type != ProductType.Subscription
				select new WinProductDescription(def.storeSpecificId, "$0.01", "Fake title - " + def.storeSpecificId, "Fake description - " + def.storeSpecificId, "USD", 0.01m, null, null, def.type == ProductType.Consumable);
			win8.BuildDummyProducts(source.ToList());
			init(0);
		}

		public override void FinishTransaction(ProductDefinition product, string transactionId)
		{
			win8.FinaliseTransaction(transactionId);
		}

		private void init(int delay)
		{
			win8.Initialize(this);
			win8.RetrieveProducts(true);
		}

		public override void Purchase(ProductDefinition product, string developerPayload)
		{
			win8.Purchase(product.storeSpecificId);
		}

		public void restoreTransactions(bool pausing)
		{
			if (!pausing && m_CanReceivePurchases)
			{
				win8.RetrieveProducts(false);
			}
		}

		public void RestoreTransactions()
		{
			logger.Log("Explicit RestoreTransactions()");
			win8.RetrieveProducts(false);
			m_CanReceivePurchases = true;
		}

		public void logError(string error)
		{
			logger.LogError("Unity Purchasing", error);
		}

		public void OnProductListReceived(WinProductDescription[] winProducts)
		{
			util.RunOnMainThread(() =>
			{
				IEnumerable<ProductDescription> source = from product in winProducts
					let metadata = new ProductMetadata(product.price, product.title, product.description, product.ISOCurrencyCode, product.priceDecimal)
					select new ProductDescription(product.platformSpecificID, metadata, product.receipt, product.transactionID);
				callback.OnProductsRetrieved(source.ToList());
			});
		}

		public void log(string message)
		{
			util.RunOnMainThread(() =>
			{
				logger.Log(message);
			});
		}

		public void OnPurchaseFailed(string productId, string error)
		{
			util.RunOnMainThread(() =>
			{
				logger.LogFormat(LogType.Error, "Purchase failed: {0}, {1}", productId, error);
				if ("AlreadyPurchased" == error)
				{
					try
					{
						callback.OnPurchaseFailed(new PurchaseFailureDescription(productId, (PurchaseFailureReason)Enum.Parse(typeof(PurchaseFailureReason), "DuplicateTransaction"), error));
						return;
					}
					catch
					{
						callback.OnPurchaseFailed(new PurchaseFailureDescription(productId, (PurchaseFailureReason)Enum.Parse(typeof(PurchaseFailureReason), "Unknown"), error));
						return;
					}
				}
				if ("NotPurchased" == error)
				{
					callback.OnPurchaseFailed(new PurchaseFailureDescription(productId, (PurchaseFailureReason)Enum.Parse(typeof(PurchaseFailureReason), "UserCancelled"), error));
				}
				else
				{
					callback.OnPurchaseFailed(new PurchaseFailureDescription(productId, (PurchaseFailureReason)Enum.Parse(typeof(PurchaseFailureReason), "Unknown"), error));
				}
			});
		}

		public void OnPurchaseSucceeded(string productId, string receipt, string tranId)
		{
			util.RunOnMainThread(() =>
			{
				logger.Log("PURCHASE SUCCEEDED!:{0}", count++);
				m_CanReceivePurchases = true;
				callback.OnPurchaseSucceeded(productId, receipt, tranId);
			});
		}

		public void OnProductListError(string message)
		{
			util.RunOnMainThread(() =>
			{
				if (message.Contains("801900CC"))
				{
					callback.OnSetupFailed(InitializationFailureReason.AppNotKnown);
				}
				else
				{
					logError("Unable to retrieve product listings. UnityIAP will automatically retry...");
					logError(message);
					init(3000);
				}
			});
		}
	}
}
