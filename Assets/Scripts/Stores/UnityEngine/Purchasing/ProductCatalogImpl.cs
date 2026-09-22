namespace UnityEngine.Purchasing
{
	internal class ProductCatalogImpl : IProductCatalogImpl
	{
		public ProductCatalog LoadDefaultCatalog()
		{
			TextAsset textAsset = Resources.Load("IAPProductCatalog") as TextAsset;
			if (textAsset != null)
			{
				return ProductCatalog.FromTextAsset(textAsset);
			}
			return new ProductCatalog();
		}
	}
}
