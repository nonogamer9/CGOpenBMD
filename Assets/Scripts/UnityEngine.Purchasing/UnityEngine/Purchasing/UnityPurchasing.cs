using System;
using System.Collections.Generic;
using UnityEngine.Purchasing.Extension;

namespace UnityEngine.Purchasing
{
	public abstract class UnityPurchasing
	{
		public static void Initialize(IStoreListener listener, ConfigurationBuilder builder)
		{
			Initialize(listener, builder, Debug.unityLogger, Application.persistentDataPath, new UnityAnalytics(), builder.factory.GetCatalogProvider());
		}

		public static void ClearTransactionLog()
		{
			TransactionLog transactionLog = new TransactionLog(Debug.unityLogger, Application.persistentDataPath);
			transactionLog.Clear();
		}

		internal static void Initialize(IStoreListener listener, ConfigurationBuilder builder, ILogger logger, string persistentDatapath, IUnityAnalytics analytics, ICatalogProvider catalog)
		{
			TransactionLog tDb = new TransactionLog(logger, persistentDatapath);
			PurchasingManager manager = new PurchasingManager(tDb, logger, builder.factory.service, builder.factory.storeName);
			AnalyticsReporter analytics2 = new AnalyticsReporter(analytics);
			StoreListenerProxy proxy = new StoreListenerProxy(listener, analytics2, builder.factory);
			FetchAndMergeProducts(builder.useCatalogProvider, builder.products, catalog, (HashSet<ProductDefinition> response) =>
			{
				manager.Initialize(proxy, response);
			});
		}

		internal static void FetchAndMergeProducts(bool useCatalog, HashSet<ProductDefinition> localProductSet, ICatalogProvider catalog, Action<HashSet<ProductDefinition>> callback)
		{
			if (useCatalog && catalog != null)
			{
				catalog.FetchProducts((HashSet<ProductDefinition> cloudProducts) =>
				{
					HashSet<ProductDefinition> hashSet = new HashSet<ProductDefinition>(localProductSet);
					foreach (ProductDefinition cloudProduct in cloudProducts)
					{
						hashSet.Remove(cloudProduct);
						hashSet.Add(cloudProduct);
					}
					callback(hashSet);
				});
			}
			else
			{
				callback(localProductSet);
			}
		}
	}
}
