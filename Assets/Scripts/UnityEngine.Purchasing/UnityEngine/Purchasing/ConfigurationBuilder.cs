using System;
using System.Collections.Generic;
using UnityEngine.Purchasing.Extension;

namespace UnityEngine.Purchasing
{
	public class ConfigurationBuilder
	{
		private PurchasingFactory m_Factory;

		private HashSet<ProductDefinition> m_Products = new HashSet<ProductDefinition>();

		[Obsolete("This property has been renamed 'useCatalogProvider'", false)]
		public bool useCloudCatalog { get; set; }

		public bool useCatalogProvider { get; set; }

		public HashSet<ProductDefinition> products
		{
			get
			{
				return m_Products;
			}
		}

		internal PurchasingFactory factory
		{
			get
			{
				return m_Factory;
			}
		}

		internal ConfigurationBuilder(PurchasingFactory factory)
		{
			m_Factory = factory;
		}

		public T Configure<T>() where T : IStoreConfiguration
		{
			return m_Factory.GetConfig<T>();
		}

		public static ConfigurationBuilder Instance(IPurchasingModule first, params IPurchasingModule[] rest)
		{
			PurchasingFactory purchasingFactory = new PurchasingFactory(first, rest);
			return new ConfigurationBuilder(purchasingFactory);
		}

		public ConfigurationBuilder AddProduct(string id, ProductType type)
		{
			return AddProduct(id, type, null);
		}

		public ConfigurationBuilder AddProduct(string id, ProductType type, IDs storeIDs)
		{
			return AddProduct(id, type, storeIDs, (IEnumerable<PayoutDefinition>)null);
		}

		public ConfigurationBuilder AddProduct(string id, ProductType type, IDs storeIDs, PayoutDefinition payout)
		{
			return AddProduct(id, type, storeIDs, new List<PayoutDefinition> { payout });
		}

		public ConfigurationBuilder AddProduct(string id, ProductType type, IDs storeIDs, IEnumerable<PayoutDefinition> payouts)
		{
			string storeSpecificId = id;
			if (storeIDs != null)
			{
				storeSpecificId = storeIDs.SpecificIDForStore(factory.storeName, id);
			}
			ProductDefinition productDefinition = new ProductDefinition(id, storeSpecificId, type);
			productDefinition.SetPayouts(payouts);
			m_Products.Add(productDefinition);
			return this;
		}

		public ConfigurationBuilder AddProducts(IEnumerable<ProductDefinition> products)
		{
			foreach (ProductDefinition product in products)
			{
				m_Products.Add(product);
			}
			return this;
		}
	}
}
