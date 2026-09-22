using UnityEngine.Purchasing.Extension;

namespace UnityEngine.Purchasing
{
	public class CloudMoolahModule : IPurchasingModule
	{
		public void Configure(IPurchasingBinder binder)
		{
			Debug.Log("CloudMoolah Configure");
			binder.RegisterStore("MoolahAppStore", InstantiateMoolahAppStore(binder));
		}

		private IStore InstantiateMoolahAppStore(IPurchasingBinder binder)
		{
			if (IsSupportPlatform())
			{
				GameObject gameObject = GameObject.Find("IAPUtil");
				MoolahStoreImpl moolahStoreImpl = gameObject.AddComponent<MoolahStoreImpl>();
				binder.RegisterExtension((IMoolahExtension)moolahStoreImpl);
				binder.RegisterConfiguration((IMoolahConfiguration)moolahStoreImpl);
				return moolahStoreImpl;
			}
			return null;
		}

		private bool IsSupportPlatform()
		{
			if (Application.platform == RuntimePlatform.Android)
			{
				return true;
			}
			return false;
		}
	}
}
