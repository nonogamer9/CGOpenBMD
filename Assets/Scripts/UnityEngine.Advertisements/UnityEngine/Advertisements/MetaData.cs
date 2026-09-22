using System.Collections.Generic;
using UnityEngine.Advertisements.SimpleJson;

namespace UnityEngine.Advertisements
{
	public sealed class MetaData
	{
		private readonly IDictionary<string, object> m_MetaData = new Dictionary<string, object>();

		public string category { get; private set; }

		public IDictionary<string, object> Values
		{
			get
			{
				return m_MetaData;
			}
		}

		public MetaData(string category)
		{
			this.category = category;
		}

		public void Set(string key, object value)
		{
			m_MetaData[key] = value;
		}

		public object Get(string key)
		{
			return m_MetaData[key];
		}

		internal string ToJSON()
		{
			return UnityEngine.Advertisements.SimpleJson.SimpleJson.SerializeObject(m_MetaData);
		}
	}
}
