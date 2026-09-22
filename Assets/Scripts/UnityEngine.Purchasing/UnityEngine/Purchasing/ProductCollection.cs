using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.Purchasing
{
	public class ProductCollection
	{
		private Dictionary<string, Product> m_IdToProduct;

		private Dictionary<string, Product> m_StoreSpecificIdToProduct;

		private Product[] m_Products;

		private HashSet<Product> m_ProductSet = new HashSet<Product>();

		public HashSet<Product> set
		{
			get
			{
				return m_ProductSet;
			}
		}

		public Product[] all
		{
			get
			{
				return m_Products;
			}
		}

		internal ProductCollection(Product[] products)
		{
			AddProducts(products);
		}

		internal void AddProducts(IEnumerable<Product> products)
		{
			m_ProductSet.UnionWith(products);
			m_Products = m_ProductSet.ToArray();
			m_IdToProduct = m_Products.ToDictionary((Product x) => x.definition.id);
			m_StoreSpecificIdToProduct = m_Products.ToDictionary((Product x) => x.definition.storeSpecificId);
		}

		public Product WithID(string id)
		{
			Product value = null;
			m_IdToProduct.TryGetValue(id, out value);
			return value;
		}

		public Product WithStoreSpecificID(string id)
		{
			Product value = null;
			m_StoreSpecificIdToProduct.TryGetValue(id, out value);
			return value;
		}
	}
}
