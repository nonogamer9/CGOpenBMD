namespace UnityEngine.Purchasing.Extension
{
	public class ProductDescription
	{
		public ProductType type;

		public string storeSpecificId { get; private set; }

		public ProductMetadata metadata { get; private set; }

		public string receipt { get; private set; }

		public string transactionId { get; set; }

		public ProductDescription(string id, ProductMetadata metadata, string receipt, string transactionId)
		{
			storeSpecificId = id;
			this.metadata = metadata;
			this.receipt = receipt;
			this.transactionId = transactionId;
		}

		public ProductDescription(string id, ProductMetadata metadata, string receipt, string transactionId, ProductType type)
			: this(id, metadata, receipt, transactionId)
		{
			this.type = type;
		}

		public ProductDescription(string id, ProductMetadata metadata)
			: this(id, metadata, null, null)
		{
		}
	}
}
