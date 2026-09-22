using System;
using System.Collections.Generic;
using UnityEngine.Purchasing.Extension;

namespace UnityEngine.Purchasing
{
	internal class PurchasingFactory : IPurchasingBinder, IExtensionProvider
	{
		private Dictionary<Type, IStoreConfiguration> m_ConfigMap = new Dictionary<Type, IStoreConfiguration>();

		private Dictionary<Type, IStoreExtension> m_ExtensionMap = new Dictionary<Type, IStoreExtension>();

		private IStore m_Store;

		private ICatalogProvider m_CatalogProvider;

		public string storeName { get; private set; }

		public IStore service
		{
			get
			{
				if (m_Store != null)
				{
					return m_Store;
				}
				throw new InvalidOperationException("No impl available!");
			}
			set
			{
				m_Store = value;
			}
		}

		public PurchasingFactory(IPurchasingModule first, params IPurchasingModule[] remainingModules)
		{
			first.Configure(this);
			foreach (IPurchasingModule purchasingModule in remainingModules)
			{
				purchasingModule.Configure(this);
			}
		}

		public void RegisterStore(string name, IStore s)
		{
			if (m_Store == null && s != null)
			{
				storeName = name;
				service = s;
			}
		}

		public void RegisterExtension<T>(T instance) where T : IStoreExtension
		{
			m_ExtensionMap[typeof(T)] = instance;
		}

		public void RegisterConfiguration<T>(T instance) where T : IStoreConfiguration
		{
			m_ConfigMap[typeof(T)] = instance;
		}

		public T GetConfig<T>() where T : IStoreConfiguration
		{
			if (service is T)
			{
				return (T)service;
			}
			Type typeFromHandle = typeof(T);
			if (m_ConfigMap.ContainsKey(typeFromHandle))
			{
				return (T)m_ConfigMap[typeFromHandle];
			}
			throw new ArgumentException("No binding for config type " + typeFromHandle);
		}

		public T GetExtension<T>() where T : IStoreExtension
		{
			if (service is T)
			{
				return (T)service;
			}
			Type typeFromHandle = typeof(T);
			if (m_ExtensionMap.ContainsKey(typeFromHandle))
			{
				return (T)m_ExtensionMap[typeFromHandle];
			}
			throw new ArgumentException("No binding for type " + typeFromHandle);
		}

		public void SetCatalogProvider(ICatalogProvider provider)
		{
			m_CatalogProvider = provider;
		}

		public void SetCatalogProviderFunction(Action<Action<HashSet<ProductDefinition>>> func)
		{
			m_CatalogProvider = new SimpleCatalogProvider(func);
		}

		internal ICatalogProvider GetCatalogProvider()
		{
			return m_CatalogProvider;
		}
	}
}
