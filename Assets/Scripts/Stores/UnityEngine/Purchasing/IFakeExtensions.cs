namespace UnityEngine.Purchasing
{
	internal interface IFakeExtensions : IStoreExtension
	{
		string unavailableProductId { get; set; }
	}
}
