using UnityEngine.Purchasing.Extension;

namespace UnityEngine.Purchasing
{
	internal class FakeMoolahConfiguration : IMoolahConfiguration, IStoreConfiguration
	{
		private string m_appKey;

		private string m_hashKey;

		private string m_notificationURL;

		public string appKey
		{
			get
			{
				return m_appKey;
			}
			set
			{
				m_appKey = value;
			}
		}

		public string hashKey
		{
			get
			{
				return m_hashKey;
			}
			set
			{
				m_hashKey = value;
			}
		}

		public string notificationURL
		{
			get
			{
				return m_notificationURL;
			}
			set
			{
				m_notificationURL = value;
			}
		}

		public void SetMode(CloudMoolahMode mode)
		{
		}
	}
}
