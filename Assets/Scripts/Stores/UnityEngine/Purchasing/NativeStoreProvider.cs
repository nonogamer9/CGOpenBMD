using System;
using Uniject;
using UnityEngine.Purchasing.Extension;

namespace UnityEngine.Purchasing
{
	internal class NativeStoreProvider : INativeStoreProvider
	{
		public INativeStore GetAndroidStore(IUnityCallback callback, AppStore store, IPurchasingBinder binder, IUtil util)
		{
			INativeStore androidStoreHelper;
			try
			{
				androidStoreHelper = GetAndroidStoreHelper(callback, store, binder, util);
			}
			catch (Exception ex)
			{
				throw new NotSupportedException("Failed to bind to native store: " + ex.ToString());
			}
			if (androidStoreHelper != null)
			{
				return androidStoreHelper;
			}
			throw new NotImplementedException();
		}

		private INativeStore GetAndroidStoreHelper(IUnityCallback callback, AppStore store, IPurchasingBinder binder, IUtil util)
		{
			switch (store)
			{
			case AppStore.AmazonAppStore:
			{
				using (AndroidJavaClass androidJavaClass3 = new AndroidJavaClass("com.unity.purchasing.amazon.AmazonPurchasing"))
				{
					JavaBridge javaBridge3 = new JavaBridge(new ScriptingUnityCallback(callback, util));
					AndroidJavaObject androidJavaObject3 = androidJavaClass3.CallStatic<AndroidJavaObject>("instance", new object[1] { javaBridge3 });
					AmazonAppStoreStoreExtensions instance = new AmazonAppStoreStoreExtensions(androidJavaObject3);
					binder.RegisterExtension((IAmazonExtensions)instance);
					binder.RegisterConfiguration((IAmazonConfiguration)instance);
					return new AndroidJavaStore(androidJavaObject3);
				}
			}
			case AppStore.GooglePlay:
			{
				using (AndroidJavaClass androidJavaClass2 = new AndroidJavaClass("com.unity.purchasing.googleplay.GooglePlayPurchasing"))
				{
					GooglePlayStoreExtensions googlePlayStoreExtensions = new GooglePlayStoreExtensions();
					JavaBridge javaBridge2 = new JavaBridge(new ScriptingUnityCallback(callback, util));
					AndroidJavaObject androidJavaObject2 = androidJavaClass2.CallStatic<AndroidJavaObject>("instance", new object[1] { javaBridge2 });
					googlePlayStoreExtensions.SetAndroidJavaObject(androidJavaObject2);
					binder.RegisterExtension((IGooglePlayStoreExtensions)googlePlayStoreExtensions);
					binder.RegisterConfiguration((IGooglePlayConfiguration)googlePlayStoreExtensions);
					return new GooglePlayAndroidJavaStore(androidJavaObject2, util);
				}
			}
			case AppStore.SamsungApps:
			{
				using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.unity.purchasing.samsung.SamsungPurchasing"))
				{
					SamsungAppsStoreExtensions samsungAppsStoreExtensions = new SamsungAppsStoreExtensions();
					JavaBridge javaBridge = new JavaBridge(new ScriptingUnityCallback(callback, util));
					AndroidJavaObject androidJavaObject = androidJavaClass.CallStatic<AndroidJavaObject>("instance", new object[2] { javaBridge, samsungAppsStoreExtensions });
					samsungAppsStoreExtensions.SetAndroidJavaObject(androidJavaObject);
					binder.RegisterExtension((ISamsungAppsExtensions)samsungAppsStoreExtensions);
					binder.RegisterConfiguration((ISamsungAppsConfiguration)samsungAppsStoreExtensions);
					return new AndroidJavaStore(androidJavaObject);
				}
			}
			case AppStore.XiaomiMiPay:
			{
				UnityChannelImpl unityChannelImpl = new UnityChannelImpl();
				UnityChannelBindings unityChannelBindings = new UnityChannelBindings();
				unityChannelImpl.SetNativeStore(unityChannelBindings);
				binder.RegisterExtension((IUnityChannelExtensions)unityChannelImpl);
				binder.RegisterConfiguration((IUnityChannelConfiguration)unityChannelImpl);
				return unityChannelBindings;
			}
			default:
				throw new NotImplementedException();
			}
		}

		public INativeAppleStore GetStorekit(IUnityCallback callback)
		{
			if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.tvOS)
			{
				return new iOSStoreBindings();
			}
			return new OSXStoreBindings();
		}

		public INativeTizenStore GetTizenStore(IUnityCallback callback, IPurchasingBinder binder)
		{
			return new TizenStoreBindings();
		}

		public INativeFacebookStore GetFacebookStore()
		{
			return new FacebookStoreBindings();
		}

		public INativeFacebookStore GetFacebookStore(IUnityCallback callback, IPurchasingBinder binder)
		{
			return new FacebookStoreBindings();
		}
	}
}
