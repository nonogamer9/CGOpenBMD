using Uniject;
using UnityEngine.Purchasing.Extension;

namespace UnityEngine.Purchasing
{
	internal interface INativeStoreProvider
	{
		INativeStore GetAndroidStore(IUnityCallback callback, AppStore store, IPurchasingBinder binder, IUtil util);

		INativeAppleStore GetStorekit(IUnityCallback callback);

		INativeTizenStore GetTizenStore(IUnityCallback callback, IPurchasingBinder binder);

		INativeFacebookStore GetFacebookStore();

		INativeFacebookStore GetFacebookStore(IUnityCallback callback, IPurchasingBinder binder);
	}
}
