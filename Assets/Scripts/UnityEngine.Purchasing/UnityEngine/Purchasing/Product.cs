namespace UnityEngine.Purchasing
{
	public class Product
	{
		public ProductDefinition definition { get; private set; }

		public ProductMetadata metadata { get; internal set; }

		public bool availableToPurchase { get; internal set; }

		public string transactionID { get; internal set; }

		public bool hasReceipt
		{
			get
			{
				return !string.IsNullOrEmpty(receipt);
			}
		}

		public string receipt { get; internal set; }

		internal Product(ProductDefinition definition, ProductMetadata metadata, string receipt)
		{
			this.definition = definition;
			this.metadata = metadata;
			this.receipt = receipt;
		}

		internal Product(ProductDefinition definition, ProductMetadata metadata)
			: this(definition, metadata, null)
		{
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			Product product = obj as Product;
			if (product == null)
			{
				return false;
			}
			return definition.Equals(product.definition);
		}

		public override int GetHashCode()
		{
			return definition.GetHashCode();
		}
	}
}
