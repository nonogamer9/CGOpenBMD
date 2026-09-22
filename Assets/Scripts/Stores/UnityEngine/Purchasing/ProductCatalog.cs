using System;
using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.Purchasing
{
	[Serializable]
	public class ProductCatalog
	{
		private static IProductCatalogImpl instance;

		public string appleSKU;

		public string appleTeamID;

		public bool enableCodelessAutoInitialization = false;

		[SerializeField]
		private List<ProductCatalogItem> products = new List<ProductCatalogItem>();

		public const string kCatalogPath = "Assets/Resources/IAPProductCatalog.json";

		public const string prevkCatalogPath = "Assets/Plugins/UnityPurchasing/Resources/IAPProductCatalog.json";

		public ICollection<ProductCatalogItem> allProducts
		{
			get
			{
				return products;
			}
		}

		public ICollection<ProductCatalogItem> allValidProducts
		{
			get
			{
				return products.Where((ProductCatalogItem x) => !string.IsNullOrEmpty(x.id) && x.id.Trim().Length != 0).ToList();
			}
		}

		internal static void Initialize()
		{
			if (instance == null)
			{
				Initialize(new ProductCatalogImpl());
			}
		}

		public static void Initialize(IProductCatalogImpl productCatalogImpl)
		{
			instance = productCatalogImpl;
		}

		public void Add(ProductCatalogItem item)
		{
			products.Add(item);
		}

		public void Remove(ProductCatalogItem item)
		{
			products.Remove(item);
		}

		public bool IsEmpty()
		{
			foreach (ProductCatalogItem product in products)
			{
				if (!string.IsNullOrEmpty(product.id))
				{
					return false;
				}
			}
			return true;
		}

		public static string Serialize(ProductCatalog catalog)
		{
			return JsonUtility.ToJson(catalog);
		}

		public static ProductCatalog Deserialize(string catalogJSON)
		{
			return JsonUtility.FromJson<ProductCatalog>(catalogJSON);
		}

		public static ProductCatalog FromTextAsset(TextAsset asset)
		{
			return Deserialize(asset.text);
		}

		public static ProductCatalog LoadDefaultCatalog()
		{
			Initialize();
			return instance.LoadDefaultCatalog();
		}
	}
}
