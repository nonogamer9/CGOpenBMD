namespace UnityEngine.Purchasing
{
	public class PurchaseEventArgs
	{
		public Product purchasedProduct { get; private set; }

		internal PurchaseEventArgs(Product purchasedProduct)
		{
			this.purchasedProduct = purchasedProduct;
		}
	}
}
