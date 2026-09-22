namespace UnityEngine.Purchasing
{
	public interface IAmazonExtensions : IStoreExtension
	{
		string amazonUserId { get; }

		void NotifyUnableToFulfillUnavailableProduct(string transactionID);
	}
}
