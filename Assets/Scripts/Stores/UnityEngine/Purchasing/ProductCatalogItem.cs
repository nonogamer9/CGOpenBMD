using System;
using System.Collections.Generic;

namespace UnityEngine.Purchasing
{
	[Serializable]
	public class ProductCatalogItem
	{
		public string id;

		public ProductType type;

		[SerializeField]
		private List<StoreID> storeIDs = new List<StoreID>();

		public LocalizedProductDescription defaultDescription = new LocalizedProductDescription();

		public string screenshotPath;

		public int applePriceTier = 0;

		public int xiaomiPriceTier = 0;

		public Price googlePrice = new Price();

		public string pricingTemplateID;

		[SerializeField]
		private List<LocalizedProductDescription> descriptions = new List<LocalizedProductDescription>();

		[SerializeField]
		private List<ProductCatalogPayout> payouts = new List<ProductCatalogPayout>();

		public IList<ProductCatalogPayout> Payouts
		{
			get
			{
				return payouts;
			}
		}

		public ICollection<StoreID> allStoreIDs
		{
			get
			{
				return storeIDs;
			}
		}

		public bool HasAvailableLocale
		{
			get
			{
				return Enum.GetValues(typeof(TranslationLocale)).Length > descriptions.Count + 1;
			}
		}

		public TranslationLocale NextAvailableLocale
		{
			get
			{
				foreach (TranslationLocale value in Enum.GetValues(typeof(TranslationLocale)))
				{
					if (GetDescription(value) == null && defaultDescription.googleLocale != value)
					{
						return value;
					}
				}
				return TranslationLocale.en_US;
			}
		}

		public ICollection<LocalizedProductDescription> translatedDescriptions
		{
			get
			{
				return descriptions;
			}
		}

		public void AddPayout()
		{
			payouts.Add(new ProductCatalogPayout());
		}

		public void RemovePayout(ProductCatalogPayout payout)
		{
			payouts.Remove(payout);
		}

		public ProductCatalogItem Clone()
		{
			ProductCatalogItem productCatalogItem = new ProductCatalogItem();
			productCatalogItem.id = id;
			productCatalogItem.type = type;
			productCatalogItem.SetStoreIDs(allStoreIDs);
			productCatalogItem.defaultDescription = defaultDescription.Clone();
			productCatalogItem.screenshotPath = screenshotPath;
			productCatalogItem.applePriceTier = applePriceTier;
			productCatalogItem.googlePrice.value = googlePrice.value;
			productCatalogItem.pricingTemplateID = pricingTemplateID;
			foreach (LocalizedProductDescription description in descriptions)
			{
				productCatalogItem.descriptions.Add(description.Clone());
			}
			return productCatalogItem;
		}

		public void SetStoreID(string aStore, string aId)
		{
			storeIDs.RemoveAll((StoreID obj) => obj.store == aStore);
			if (!string.IsNullOrEmpty(aId))
			{
				storeIDs.Add(new StoreID(aStore, aId));
			}
		}

		public string GetStoreID(string store)
		{
			StoreID storeID = storeIDs.Find((StoreID obj) => obj.store == store);
			return (storeID == null) ? null : storeID.id;
		}

		public void SetStoreIDs(ICollection<StoreID> storeIds)
		{
			foreach (StoreID storeId in storeIds)
			{
				storeIDs.RemoveAll((StoreID obj) => obj.store == storeId.store);
				if (!string.IsNullOrEmpty(storeId.id))
				{
					storeIDs.Add(new StoreID(storeId.store, storeId.id));
				}
			}
		}

		public LocalizedProductDescription GetDescription(TranslationLocale locale)
		{
			return descriptions.Find((LocalizedProductDescription obj) => obj.googleLocale == locale);
		}

		public LocalizedProductDescription GetOrCreateDescription(TranslationLocale locale)
		{
			return GetDescription(locale) ?? AddDescription(locale);
		}

		public LocalizedProductDescription AddDescription(TranslationLocale locale)
		{
			RemoveDescription(locale);
			LocalizedProductDescription localizedProductDescription = new LocalizedProductDescription();
			localizedProductDescription.googleLocale = locale;
			descriptions.Add(localizedProductDescription);
			return localizedProductDescription;
		}

		public void RemoveDescription(TranslationLocale locale)
		{
			descriptions.RemoveAll((LocalizedProductDescription obj) => obj.googleLocale == locale);
		}
	}
}
