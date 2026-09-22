using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using Uniject;
using UnityEngine.Purchasing.Default;
using UnityEngine.Purchasing.Extension;

namespace UnityEngine.Purchasing
{
	public class StandardPurchasingModule : AbstractPurchasingModule, IAndroidStoreSelection, IStoreConfiguration
	{
		internal class StoreInstance
		{
			[CompilerGenerated]
			[DebuggerBrowsable(DebuggerBrowsableState.Never)]
			private readonly string storeName__BackingField;

			[DebuggerBrowsable(DebuggerBrowsableState.Never)]
			[CompilerGenerated]
			private readonly IStore instance__BackingField;

			internal string storeName
			{
				[CompilerGenerated]
				get
				{
					return storeName__BackingField;
				}
			}

			internal IStore instance
			{
				[CompilerGenerated]
				get
				{
					return instance__BackingField;
				}
			}

			internal StoreInstance(string name, IStore instance)
			{
				storeName__BackingField = name;
				instance__BackingField = instance;
			}
		}

		private class MicrosoftConfiguration : IMicrosoftConfiguration, IStoreConfiguration
		{
			private bool useMock;

			private StandardPurchasingModule module;

			public bool useMockBillingSystem
			{
				get
				{
					return useMock;
				}
				set
				{
					module.UseMockWindowsStore(value);
					useMock = value;
				}
			}

			public MicrosoftConfiguration(StandardPurchasingModule module)
			{
				this.module = module;
			}
		}

		public const string k_PackageVersion = "1.20.1";

		private AppStore m_AppStorePlatform;

		private INativeStoreProvider m_NativeStoreProvider;

		private RuntimePlatform m_RuntimePlatform;

		private bool m_UseCloudCatalog;

		private static StandardPurchasingModule ModuleInstance;

		private static Dictionary<AppStore, string> AndroidStoreNameMap = new Dictionary<AppStore, string>
		{
			{
				AppStore.AmazonAppStore,
				"AmazonApps"
			},
			{
				AppStore.GooglePlay,
				"GooglePlay"
			},
			{
				AppStore.SamsungApps,
				"SamsungApps"
			},
			{
				AppStore.CloudMoolah,
				"MoolahAppStore"
			},
			{
				AppStore.XiaomiMiPay,
				"XiaomiMiPay"
			},
			{
				AppStore.NotSpecified,
				"GooglePlay"
			}
		};

		private CloudCatalogImpl m_CloudCatalog;

		private bool usingMockMicrosoft;

		private WinRTStore windowsStore;

		internal IUtil util { get; private set; }

		internal ILogger logger { get; private set; }

		internal IAsyncWebUtil webUtil { get; private set; }

		internal StoreInstance storeInstance { get; private set; }

		[Obsolete("Use StandardPurchasingModule.appStore instead")]
		public AndroidStore androidStore
		{
			get
			{
				AndroidStore result = AndroidStore.NotSpecified;
				try
				{
					result = (AndroidStore)Enum.Parse(typeof(AndroidStore), m_AppStorePlatform.ToString());
				}
				catch (Exception)
				{
				}
				return result;
			}
		}

		public AppStore appStore
		{
			get
			{
				return m_AppStorePlatform;
			}
		}

		[Obsolete("Use IMicrosoftConfiguration to toggle use of the Microsoft IAP simulator.")]
		public bool useMockBillingSystem
		{
			get
			{
				return usingMockMicrosoft;
			}
			set
			{
				UseMockWindowsStore(value);
				usingMockMicrosoft = value;
			}
		}

		public FakeStoreUIMode useFakeStoreUIMode { get; set; }

		public bool useFakeStoreAlways { get; set; }

		internal StandardPurchasingModule(IUtil util, IAsyncWebUtil webUtil, ILogger logger, INativeStoreProvider nativeStoreProvider, RuntimePlatform platform, AppStore android, bool useCloudCatalog)
		{
			this.util = util;
			this.webUtil = webUtil;
			this.logger = logger;
			m_NativeStoreProvider = nativeStoreProvider;
			m_RuntimePlatform = platform;
			useFakeStoreUIMode = FakeStoreUIMode.Default;
			useFakeStoreAlways = false;
			m_AppStorePlatform = android;
			m_UseCloudCatalog = useCloudCatalog;
			Promo.InitPromo(platform, logger, "1.20.1", util, webUtil);
		}

		public static StandardPurchasingModule Instance()
		{
			return Instance(AppStore.NotSpecified);
		}

		[Obsolete("Use StandardPurchasingModule.Instance(AppStore) instead")]
		public static StandardPurchasingModule Instance(AndroidStore androidStore)
		{
			AppStore appStore = AppStore.NotSpecified;
			try
			{
				appStore = (AppStore)Enum.Parse(typeof(AppStore), androidStore.ToString());
			}
			catch (Exception)
			{
			}
			return Instance(appStore);
		}

		public static StandardPurchasingModule Instance(AppStore androidStore)
		{
			if (ModuleInstance == null)
			{
				ILogger unityLogger = Debug.unityLogger;
				unityLogger.Log("UnityIAP Version: 1.20.1");
				GameObject gameObject = new GameObject("IAPUtil");
				Object.DontDestroyOnLoad(gameObject);
				gameObject.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
				UnityUtil unityUtil = gameObject.AddComponent<UnityUtil>();
				AsyncWebUtil asyncWebUtil = gameObject.AddComponent<AsyncWebUtil>();
				TextAsset textAsset = Resources.Load("BillingMode") as TextAsset;
				StoreConfiguration storeConfiguration = null;
				if (null != textAsset)
				{
					storeConfiguration = StoreConfiguration.Deserialize(textAsset.text);
				}
				if (androidStore == AppStore.NotSpecified)
				{
					androidStore = AppStore.GooglePlay;
					if (storeConfiguration != null)
					{
						AppStore appStore = storeConfiguration.androidStore;
						if (appStore != AppStore.NotSpecified)
						{
							androidStore = appStore;
						}
					}
				}
				ModuleInstance = new StandardPurchasingModule(unityUtil, asyncWebUtil, unityLogger, new NativeStoreProvider(), Application.platform, androidStore, false);
			}
			return ModuleInstance;
		}

		public override void Configure()
		{
			BindConfiguration((IGooglePlayConfiguration)new FakeGooglePlayConfiguration());
			BindConfiguration((IAppleConfiguration)new FakeAppleConfiguation());
			BindExtension((IAppleExtensions)new FakeAppleExtensions());
			BindConfiguration((IAmazonConfiguration)new FakeAmazonExtensions());
			BindExtension((IAmazonExtensions)new FakeAmazonExtensions());
			BindConfiguration((ISamsungAppsConfiguration)new FakeSamsungAppsExtensions());
			BindExtension((ISamsungAppsExtensions)new FakeSamsungAppsExtensions());
			BindConfiguration((IGooglePlayConfiguration)new FakeGooglePlayStoreExtensions());
			BindExtension((IGooglePlayStoreExtensions)new FakeGooglePlayStoreExtensions());
			BindConfiguration((IMoolahConfiguration)new FakeMoolahConfiguration());
			BindExtension((IMoolahExtension)new FakeMoolahExtensions());
			BindConfiguration((IUnityChannelConfiguration)new FakeUnityChannelConfiguration());
			BindExtension((IUnityChannelExtensions)new FakeUnityChannelExtensions());
			BindConfiguration((IMicrosoftConfiguration)new MicrosoftConfiguration(this));
			BindExtension((IMicrosoftExtensions)new FakeMicrosoftExtensions());
			BindConfiguration((ITizenStoreConfiguration)new FakeTizenStoreConfiguration());
			BindConfiguration((IAndroidStoreSelection)this);
			BindConfiguration((IManagedStoreConfig)new FakeManagedStoreConfig());
			BindExtension((IManagedStoreExtensions)new FakeManagedStoreExtensions());
			BindExtension((ITransactionHistoryExtensions)new FakeTransactionHistoryExtensions());
			if (storeInstance == null)
			{
				storeInstance = InstantiateStore();
			}
			RegisterStore(storeInstance.storeName, storeInstance.instance);
			if (m_UseCloudCatalog)
			{
				MethodInfo method = m_Binder.GetType().GetMethod("SetCatalogProviderFunction");
				if (method != null)
				{
					m_CloudCatalog = CloudCatalogImpl.CreateInstance(storeInstance.storeName);
					Action<Action<HashSet<ProductDefinition>>> action = (Action<HashSet<ProductDefinition>> callback) =>
					{
						MethodInfo method2 = typeof(CloudCatalogImpl).GetMethod("FetchProducts");
						if (method2 != null)
						{
							method2.Invoke(m_CloudCatalog, new object[1] { callback });
						}
						else
						{
							callback(new HashSet<ProductDefinition>());
						}
					};
					method.Invoke(m_Binder, new object[1] { action });
				}
			}
			IStoreInternal storeInternal = storeInstance.instance as IStoreInternal;
			if (storeInternal != null)
			{
				storeInternal.SetModule(this);
			}
			IManagedStoreExtensions managedStoreExtensions = storeInstance.instance as IManagedStoreExtensions;
			if (managedStoreExtensions != null)
			{
				BindExtension(managedStoreExtensions);
			}
			if (util.IsClassOrSubclass(typeof(JSONStore), storeInstance.instance.GetType()))
			{
				JSONStore instance = (JSONStore)storeInstance.instance;
				BindExtension((ITransactionHistoryExtensions)instance);
			}
		}

		private StoreInstance InstantiateStore()
		{
			if (useFakeStoreAlways)
			{
				return new StoreInstance("fake", InstantiateFakeStore());
			}
			switch (m_RuntimePlatform)
			{
			case RuntimePlatform.OSXPlayer:
				return new StoreInstance("MacAppStore", InstantiateApple());
			case RuntimePlatform.IPhonePlayer:
			case RuntimePlatform.tvOS:
				return new StoreInstance("AppleAppStore", InstantiateApple());
			case RuntimePlatform.Android:
				switch (m_AppStorePlatform)
				{
				case AppStore.CloudMoolah:
					return new StoreInstance("MoolahAppStore", InstantiateCloudMoolah());
				case AppStore.XiaomiMiPay:
					return new StoreInstance(AndroidStoreNameMap[m_AppStorePlatform], InstantiateUnityChannel());
				default:
					return new StoreInstance(AndroidStoreNameMap[m_AppStorePlatform], InstantiateAndroid());
				}
			case RuntimePlatform.MetroPlayerX86:
			case RuntimePlatform.MetroPlayerX64:
			case RuntimePlatform.MetroPlayerARM:
				return new StoreInstance("WinRT", instantiateWindowsStore());
			case RuntimePlatform.TizenPlayer:
				return new StoreInstance("TizenStore", InstantiateTizen());
			case RuntimePlatform.WindowsPlayer:
			case RuntimePlatform.WebGLPlayer:
				return new StoreInstance("FacebookStore", InstantiateFacebook());
			default:
				return new StoreInstance("fake", InstantiateFakeStore());
			}
		}

		private IStore InstantiateAndroid()
		{
			JSONStore store = new JSONStore();
			return InstantiateAndroidHelper(store);
		}

		private IStore InstantiateUnityChannel()
		{
			UnityChannelImpl unityChannelImpl = new UnityChannelImpl();
			BindExtension((IUnityChannelExtensions)unityChannelImpl);
			INativeUnityChannelStore nativeStore = (INativeUnityChannelStore)GetAndroidNativeStore(unityChannelImpl);
			unityChannelImpl.SetNativeStore(nativeStore);
			return unityChannelImpl;
		}

		private IStore InstantiateAndroidHelper(JSONStore store)
		{
			store.SetNativeStore(GetAndroidNativeStore(store));
			return store;
		}

		private INativeStore GetAndroidNativeStore(JSONStore store)
		{
			return m_NativeStoreProvider.GetAndroidStore(store, m_AppStorePlatform, m_Binder, util);
		}

		private IStore InstantiateCloudMoolah()
		{
			GameObject gameObject = GameObject.Find("IAPUtil");
			MoolahStoreImpl moolahStoreImpl = gameObject.AddComponent<MoolahStoreImpl>();
			BindConfiguration((IMoolahConfiguration)moolahStoreImpl);
			BindExtension((IMoolahExtension)moolahStoreImpl);
			return moolahStoreImpl;
		}

		private IStore InstantiateApple()
		{
			AppleStoreImpl appleStoreImpl = new AppleStoreImpl(util);
			INativeAppleStore storekit = m_NativeStoreProvider.GetStorekit(appleStoreImpl);
			appleStoreImpl.SetNativeStore(storekit);
			BindExtension((IAppleExtensions)appleStoreImpl);
			return appleStoreImpl;
		}

		private void UseMockWindowsStore(bool value)
		{
			if (windowsStore != null)
			{
				IWindowsIAP windowsIAP = Factory.Create(value);
				windowsStore.SetWindowsIAP(windowsIAP);
			}
		}

		private IStore instantiateWindowsStore()
		{
			IWindowsIAP win = Factory.Create(false);
			windowsStore = new WinRTStore(win, util, logger);
			util.AddPauseListener(windowsStore.restoreTransactions);
			return windowsStore;
		}

		private IStore InstantiateTizen()
		{
			TizenStoreImpl tizenStoreImpl = new TizenStoreImpl(util);
			tizenStoreImpl.SetNativeStore(m_NativeStoreProvider.GetTizenStore(tizenStoreImpl, m_Binder));
			BindConfiguration((ITizenStoreConfiguration)tizenStoreImpl);
			return tizenStoreImpl;
		}

		private IStore InstantiateFacebook()
		{
			INativeFacebookStore facebookStore = m_NativeStoreProvider.GetFacebookStore();
			if (facebookStore.Check())
			{
				FacebookStoreImpl facebookStoreImpl = new FacebookStoreImpl(util);
				facebookStoreImpl.SetNativeStore(facebookStore);
				return facebookStoreImpl;
			}
			return null;
		}

		private IStore InstantiateFakeStore()
		{
			FakeStore fakeStore = null;
			if (useFakeStoreUIMode != FakeStoreUIMode.Default)
			{
				fakeStore = new UIFakeStore();
				fakeStore.UIMode = useFakeStoreUIMode;
			}
			if (fakeStore == null)
			{
				fakeStore = new FakeStore();
			}
			return fakeStore;
		}
	}
}
