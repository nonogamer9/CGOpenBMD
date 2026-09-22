using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine.Purchasing.Extension;

namespace UnityEngine.Purchasing
{
	internal class FakeStore : JSONStore, IFakeExtensions, IStoreExtension, INativeStore
	{
		protected enum DialogType
		{
			Purchase = 0,
			RetrieveProducts = 1
		}

		public const string Name = "fake";

		private IStoreCallback m_Biller;

		private List<string> m_PurchasedProducts = new List<string>();

		public bool purchaseCalled;

		public bool restoreCalled;

		public FakeStoreUIMode UIMode = FakeStoreUIMode.Default;

		public string unavailableProductId { get; set; }

		public override void Initialize(IStoreCallback biller)
		{
			m_Biller = biller;
			base.Initialize(biller);
			SetNativeStore(this);
		}

		public void RetrieveProducts(string json)
		{
			List<object> productsList = (List<object>)MiniJson.JsonDecode(json);
			List<ProductDefinition> source = productsList.DecodeJSON("fake");
			StoreRetrieveProducts(new ReadOnlyCollection<ProductDefinition>(source.ToList()));
		}

		public void StoreRetrieveProducts(ReadOnlyCollection<ProductDefinition> productDefinitions)
		{
			List<ProductDescription> products = new List<ProductDescription>();
			foreach (ProductDefinition productDefinition in productDefinitions)
			{
				if (!(unavailableProductId != productDefinition.id))
				{
					continue;
				}
				ProductMetadata metadata = new ProductMetadata("$0.01", "Fake title for " + productDefinition.id, "Fake description", "USD", 0.01m);
				ProductCatalog productCatalog = ProductCatalog.LoadDefaultCatalog();
				if (productCatalog != null)
				{
					foreach (ProductCatalogItem allProduct in productCatalog.allProducts)
					{
						if (allProduct.id == productDefinition.id)
						{
							metadata = new ProductMetadata(allProduct.googlePrice.value.ToString(), allProduct.defaultDescription.Title, allProduct.defaultDescription.Description, "USD", allProduct.googlePrice.value);
						}
					}
				}
				products.Add(new ProductDescription(productDefinition.storeSpecificId, metadata));
			}
			Action<bool, InitializationFailureReason> action = (bool allow, InitializationFailureReason failureReason) =>
			{
				if (allow)
				{
					m_Biller.OnProductsRetrieved(products);
					Promo.ProvideProductsToAds(this, m_Biller);
				}
				else
				{
					m_Biller.OnSetupFailed(failureReason);
				}
			};
			if (UIMode != FakeStoreUIMode.DeveloperUser || !StartUI(productDefinitions, DialogType.RetrieveProducts, action))
			{
				action(true, InitializationFailureReason.AppNotKnown);
			}
		}

		public void Purchase(string productJSON, string developerPayload)
		{
			Dictionary<string, object> dictionary = (Dictionary<string, object>)MiniJson.JsonDecode(productJSON);
			object value;
			dictionary.TryGetValue("id", out value);
			string id = value.ToString();
			dictionary.TryGetValue("storeSpecificId", out value);
			string storeSpecificId = value.ToString();
			dictionary.TryGetValue("type", out value);
			string value2 = value.ToString();
			ProductType type = (Enum.IsDefined(typeof(ProductType), value2) ? ((ProductType)Enum.Parse(typeof(ProductType), value2)) : ProductType.Consumable);
			ProductDefinition product = new ProductDefinition(id, storeSpecificId, type);
			FakePurchase(product, developerPayload);
		}

		private void FakePurchase(ProductDefinition product, string developerPayload)
		{
			purchaseCalled = true;
			if (product.type != ProductType.Consumable)
			{
				m_PurchasedProducts.Add(product.storeSpecificId);
			}
			Action<bool, PurchaseFailureReason> action = (bool allow, PurchaseFailureReason failureReason) =>
			{
				if (allow)
				{
					_003C_003En__0(product.storeSpecificId, "{ \"this\" : \"is a fake receipt\" }", Guid.NewGuid().ToString());
				}
				else
				{
					if (failureReason == (PurchaseFailureReason)Enum.Parse(typeof(PurchaseFailureReason), "Unknown"))
					{
						failureReason = PurchaseFailureReason.UserCancelled;
					}
					PurchaseFailureDescription failure = new PurchaseFailureDescription(product.storeSpecificId, failureReason, "failed a fake store purchase");
					OnPurchaseFailed(failure);
				}
			};
			if (!StartUI(product, DialogType.Purchase, action))
			{
				action(true, (PurchaseFailureReason)Enum.Parse(typeof(PurchaseFailureReason), "Unknown"));
			}
		}

		public void RestoreTransactions(Action<bool> callback)
		{
			restoreCalled = true;
			foreach (string purchasedProduct in m_PurchasedProducts)
			{
				m_Biller.OnPurchaseSucceeded(purchasedProduct, "{ \"this\" : \"is a fake receipt\" }", "1");
			}
			callback(true);
		}

		public void FinishTransaction(string productJSON, string transactionID)
		{
		}

		public override void FinishTransaction(ProductDefinition product, string transactionId)
		{
		}

		public void RegisterPurchaseForRestore(string productId)
		{
			m_PurchasedProducts.Add(productId);
		}

		protected virtual bool StartUI<T>(object model, DialogType dialogType, Action<bool, T> callback)
		{
			return false;
		}

		[CompilerGenerated]
		[DebuggerHidden]
		private void _003C_003En__0(string id, string receipt, string transactionID)
		{
			base.OnPurchaseSucceeded(id, receipt, transactionID);
		}
	}
}
