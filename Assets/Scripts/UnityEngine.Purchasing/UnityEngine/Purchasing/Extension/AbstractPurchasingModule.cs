namespace UnityEngine.Purchasing.Extension
{
	public abstract class AbstractPurchasingModule : IPurchasingModule
	{
		protected IPurchasingBinder m_Binder;

		public void Configure(IPurchasingBinder binder)
		{
			m_Binder = binder;
			Configure();
		}

		protected void RegisterStore(string name, IStore a)
		{
			m_Binder.RegisterStore(name, a);
		}

		protected void BindExtension<T>(T instance) where T : IStoreExtension
		{
			m_Binder.RegisterExtension(instance);
		}

		protected void BindConfiguration<T>(T instance) where T : IStoreConfiguration
		{
			m_Binder.RegisterConfiguration(instance);
		}

		public abstract void Configure();
	}
}
